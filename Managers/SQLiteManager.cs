using Microsoft.Data.Sqlite;
using InGameHUD.Models;
using System.IO;
using Dapper;

namespace InGameHUD.Managers
{
    public class SQLiteManager : IPlayerData
    {
        private readonly string _connectionString;
        private readonly string _dbPath;
        private bool _isInitialized = false;

        public SQLiteManager(string pluginPath)
        {
            var dataPath = Path.Combine(pluginPath, "data");

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            _dbPath = Path.Combine(dataPath, "ingamehud.db");
            _connectionString = $"Data Source={_dbPath}";

            try
            {
                using var testConnection = new SqliteConnection(_connectionString);
                testConnection.Open();
                //Console.WriteLine($"[InGameHUD] SQLite connection test successful at {_dbPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] SQLite connection test failed: {ex.Message}");
            }
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                await CreatePlayerSettingsTable(connection);

                _isInitialized = true;
                //Console.WriteLine($"[InGameHUD] SQLite initialized successfully at {_dbPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] SQLite initialization error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[InGameHUD] SQLite inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        private async Task CreatePlayerSettingsTable(SqliteConnection connection)
        {
            try
            {
                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS player_settings (
                        steam_id TEXT NOT NULL PRIMARY KEY,
                        hud_enabled INTEGER NOT NULL DEFAULT 1,
                        hud_position INTEGER NOT NULL DEFAULT 1,
                        created_at TEXT DEFAULT (datetime('now')),
                        updated_at TEXT DEFAULT (datetime('now'))
                    );");

                //Console.WriteLine("[InGameHUD] SQLite tables initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error creating SQLite tables: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SavePlayerSettingsAsync(PlayerData playerData)
        {
            if (!_isInitialized) return false;

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    SteamId = playerData.SteamID,
                    Enabled = Convert.ToInt32(playerData.HUDEnabled),
                    Position = Convert.ToInt32((int)playerData.HUDPosition + 1)
                };

                int rowsAffected = await connection.ExecuteAsync(@"
                    INSERT INTO player_settings (steam_id, hud_enabled, hud_position, updated_at) 
                    VALUES (@SteamId, @Enabled, @Position, datetime('now'))
                    ON CONFLICT(steam_id) DO UPDATE SET 
                    hud_enabled = @Enabled,
                    hud_position = @Position,
                    updated_at = datetime('now')",
                parameters);

                var verification = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT hud_enabled, hud_position FROM player_settings WHERE steam_id = @SteamId",
                    new { SteamId = playerData.SteamID });

                if (verification != null)
                {
                    bool savedEnabled = Convert.ToBoolean(verification.hud_enabled);
                    int savedPosition = Convert.ToInt32(verification.hud_position);

                    bool match = savedEnabled == playerData.HUDEnabled &&
                                savedPosition == (int)playerData.HUDPosition + 1;

                    return match && rowsAffected > 0;
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[InGameHUD] Error: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<PlayerData> LoadPlayerSettingsAsync(string steamId)
        {
            if (!_isInitialized) return new PlayerData(steamId);

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM player_settings WHERE steam_id = @SteamId",
                    new { SteamId = steamId });

                if (result != null)
                {
                    bool hudEnabled = Convert.ToBoolean(result.hud_enabled);
                    int hudPosition = Convert.ToInt32(result.hud_position);

                    return new PlayerData(steamId)
                    {
                        HUDEnabled = hudEnabled,
                        HUDPosition = (HUDPosition)(hudPosition - 1)
                    };
                }

                return new PlayerData(steamId);
            }
            catch (Exception ex)
            { 
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[InGameHUD] Error: {ex.InnerException.Message}");
                }
                return new PlayerData(steamId);
            }
        }

        public Task<Dictionary<string, string>> GetCustomDataAsync(string steamId)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }

        public async Task<bool> BulkSavePlayerSettingsAsync(IEnumerable<PlayerData> playersData)
        {
            if (!_isInitialized) return false;

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var playerData in playersData)
                    {
                        await connection.ExecuteAsync(@"
                            INSERT INTO player_settings (steam_id, hud_enabled, hud_position, updated_at) 
                            VALUES (@SteamId, @Enabled, @Position, datetime('now'))
                            ON CONFLICT(steam_id) DO UPDATE SET 
                            hud_enabled = @Enabled,
                            hud_position = @Position,
                            updated_at = datetime('now')",
                            new
                            {
                                SteamId = playerData.SteamID,
                                Enabled = playerData.HUDEnabled ? 1 : 0,
                                Position = (int)playerData.HUDPosition + 1
                            },
                            transaction);
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"[InGameHUD] Transaction error: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error in bulk save: {ex.Message}");
                return false;
            }
        }
    }
}