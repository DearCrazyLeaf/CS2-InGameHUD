using MySqlConnector;
using InGameHUD.Models;
using Dapper;

namespace InGameHUD.Managers
{
    public class MySQLManager : IPlayerData
    {
        private readonly Config _config;
        private readonly string _connectionString;
        private bool _isInitialized = false;

        public MySQLManager(Config config)
        {
            _config = config;
            _connectionString = $"Server={config.MySqlConnection.Host};" +
                              $"Port={config.MySqlConnection.Port};" +
                              $"Database={config.MySqlConnection.Database};" +
                              $"User={config.MySqlConnection.Username};" +
                              $"Password={config.MySqlConnection.Password};";
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                await CreatePlayerSettingsTable(connection);
                _isInitialized = true;
                Console.WriteLine("[InGameHUD] MySQL initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] MySQL initialization error: {ex.Message}");
                return false;
            }
        }

        private async Task CreatePlayerSettingsTable(MySqlConnection connection)
        {
            try
            {
                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS player_settings (
                        steam_id VARCHAR(32) NOT NULL PRIMARY KEY,
                        hud_enabled BOOLEAN NOT NULL DEFAULT TRUE,
                        hud_position INT NOT NULL DEFAULT 1,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;");

                //Console.WriteLine("[InGameHUD] MySQL table initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error creating MySQL table: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SavePlayerSettingsAsync(PlayerData playerData)
        {
            if (!_isInitialized) return false;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    SteamId = playerData.SteamID,
                    Enabled = playerData.HUDEnabled,
                    Position = (int)playerData.HUDPosition + 1
                };

                int rowsAffected = await connection.ExecuteAsync(@"
                    INSERT INTO player_settings (steam_id, hud_enabled, hud_position) 
                    VALUES (@SteamId, @Enabled, @Position)
                    ON DUPLICATE KEY UPDATE 
                    hud_enabled = @Enabled,
                    hud_position = @Position",
                    parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error saving player settings to MySQL: {ex.Message}");
                return false;
            }
        }

        public async Task<PlayerData> LoadPlayerSettingsAsync(string steamId)
        {
            if (!_isInitialized) return new PlayerData(steamId);

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM player_settings WHERE steam_id = @SteamId",
                    new { SteamId = steamId });

                if (result != null)
                {
                    return new PlayerData(steamId)
                    {
                        HUDEnabled = result.hud_enabled,
                        HUDPosition = (HUDPosition)(result.hud_position - 1)
                    };
                }
                return new PlayerData(steamId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading player settings from MySQL: {ex.Message}");
                return new PlayerData(steamId);
            }
        }

        public async Task<Dictionary<string, string>> GetCustomDataAsync(string steamId)
        {
            var result = new Dictionary<string, string>();
            if (!_isInitialized) return result;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                if (_config.CustomData.Playtime.Enabled)
                {
                    try
                    {
                        var playtimeTable = _config.CustomData.Playtime.TableName;
                        var playtimeColumn = _config.CustomData.Playtime.ColumnName;

                        string playtimeQuery = $@"
                            SELECT `{playtimeColumn}` 
                            FROM `{_config.MySqlConnection.Database}`.`{playtimeTable}` 
                            WHERE steam_id = @SteamId;";

                        var playtimeResult = await connection.ExecuteScalarAsync<object>(
                            playtimeQuery, new { SteamId = steamId });

                        if (playtimeResult != null && playtimeResult != DBNull.Value)
                        {
                            result["playtime"] = playtimeResult.ToString() ?? "0";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error getting playtime from MySQL: {ex.Message}");
                    }
                }

                if (_config.CustomData.Signin.Enabled)
                {
                    try
                    {
                        var signinTable = _config.CustomData.Signin.TableName;
                        var signinColumn = _config.CustomData.Signin.ColumnName;

                        string signinQuery = $@"
                            SELECT `{signinColumn}` 
                            FROM `{_config.MySqlConnection.Database}`.`{signinTable}` 
                            WHERE steamid64 = @SteamId;";

                        var signinResult = await connection.ExecuteScalarAsync<object>(
                            signinQuery, new { SteamId = steamId });

                        if (signinResult != null && signinResult != DBNull.Value)
                        {
                            result["last_signin"] = signinResult.ToString()!;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error getting last signin from MySQL: {ex.Message}");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error in GetCustomData from MySQL: {ex.Message}");
                return result;
            }
        }
    }
}