using MySqlConnector;
using InGameHUD.Models;

namespace InGameHUD.Managers
{
    public class DatabaseManager
    {
        private readonly Config _config;
        private readonly string _connectionString;

        public DatabaseManager(Config config)
        {
            _config = config;
            _connectionString = $"Server={config.MySqlConnection.Host};" +
                              $"Port={config.MySqlConnection.Port};" +
                              $"Database={config.MySqlConnection.Database};" +
                              $"User={config.MySqlConnection.Username};" +
                              $"Password={config.MySqlConnection.Password};";
        }

        public async Task<(bool success, string error)> TestConnection()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // 建表
                await CreatePlayerSettingsTable(connection);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task CreatePlayerSettingsTable(MySqlConnection connection)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS player_settings (
                        steam_id VARCHAR(32) NOT NULL PRIMARY KEY,
                        hud_enabled BOOLEAN NOT NULL DEFAULT TRUE,
                        hud_position INT NOT NULL DEFAULT 1,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                await command.ExecuteNonQueryAsync();
                Console.WriteLine("[InGameHUD] Database table initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error creating database table: {ex.Message}");
                throw;
            }
        }

        public async Task SavePlayerSettings(PlayerData playerData)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO player_settings (steam_id, hud_enabled, hud_position) 
                    VALUES (@steamId, @enabled, @position)
                    ON DUPLICATE KEY UPDATE 
                    hud_enabled = @enabled,
                    hud_position = @position";

                command.Parameters.AddWithValue("@steamId", playerData.SteamID);
                command.Parameters.AddWithValue("@enabled", playerData.HUDEnabled);
                command.Parameters.AddWithValue("@position", (int)playerData.HUDPosition + 1);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error saving player settings: {ex.Message}");
                throw;
            }
        }

        public async Task<PlayerData> LoadPlayerSettings(string steamId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM player_settings WHERE steam_id = @steamId";
                command.Parameters.AddWithValue("@steamId", steamId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new PlayerData(steamId)
                    {
                        HUDEnabled = reader.GetBoolean("hud_enabled"),
                        HUDPosition = (HUDPosition)(reader.GetInt32("hud_position") - 1)
                    };
                }
                return new PlayerData(steamId); // 返回默认设置
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading player settings: {ex.Message}");
                return new PlayerData(steamId); // 发生错误时返回默认设置
            }
        }

        public async Task<Dictionary<string, string>> GetCustomData(string steamId)
        {
            var result = new Dictionary<string, string>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // 检查数据库连接状态
                Console.WriteLine($"[InGameHUD] Database connection state: {connection.State}");
                Console.WriteLine($"[InGameHUD] Using database: {_config.MySqlConnection.Database}");

                if (_config.CustomData.Credits.Enabled)
                {
                    try
                    {
                        var creditsCommand = connection.CreateCommand();
                        var creditsTable = _config.CustomData.Credits.TableName;
                        var creditsColumn = _config.CustomData.Credits.ColumnName;

                        // 使用完全限定的表名
                        string creditsQuery = $@"
                            SELECT `{creditsColumn}` 
                            FROM `{_config.MySqlConnection.Database}`.`{creditsTable}` 
                            WHERE SteamID = @steamId;";

                        creditsCommand.CommandText = creditsQuery;
                        creditsCommand.Parameters.AddWithValue("@steamId", steamId);

                        Console.WriteLine($"[InGameHUD] Executing credits query for player {steamId}:");
                        Console.WriteLine($"[InGameHUD] Table: {creditsTable}, Column: {creditsColumn}");
                        Console.WriteLine($"[InGameHUD] Query: {creditsQuery}");

                        var creditsResult = await creditsCommand.ExecuteScalarAsync();

                        if (creditsResult != null && creditsResult != DBNull.Value)
                        {
                            result["credits"] = creditsResult.ToString() ?? "0";
                            Console.WriteLine($"[InGameHUD] Found credits value: {result["credits"]}");
                        }
                        else
                        {
                            Console.WriteLine($"[InGameHUD] No credits found for player {steamId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error getting credits: {ex.Message}");
                        Console.WriteLine($"[InGameHUD] Credits error details: {ex}");
                    }
                }

                if (_config.CustomData.Playtime.Enabled)
                {
                    try
                    {
                        var playtimeCommand = connection.CreateCommand();
                        var playtimeTable = _config.CustomData.Playtime.TableName;
                        var playtimeColumn = _config.CustomData.Playtime.ColumnName;

                        // 使用完全限定的表名
                        string playtimeQuery = $@"
                            SELECT `{playtimeColumn}` 
                            FROM `{_config.MySqlConnection.Database}`.`{playtimeTable}` 
                            WHERE steam_id = @steamId;";

                        playtimeCommand.CommandText = playtimeQuery;
                        playtimeCommand.Parameters.AddWithValue("@steamId", steamId);

                        Console.WriteLine($"[InGameHUD] Executing playtime query for player {steamId}:");
                        Console.WriteLine($"[InGameHUD] Table: {playtimeTable}, Column: {playtimeColumn}");
                        Console.WriteLine($"[InGameHUD] Query: {playtimeQuery}");

                        var playtimeResult = await playtimeCommand.ExecuteScalarAsync();

                        if (playtimeResult != null && playtimeResult != DBNull.Value)
                        {
                            result["playtime"] = playtimeResult.ToString() ?? "0";
                            Console.WriteLine($"[InGameHUD] Found playtime value: {result["playtime"]}");
                        }
                        else
                        {
                            Console.WriteLine($"[InGameHUD] No playtime found for player {steamId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error getting playtime: {ex.Message}");
                        Console.WriteLine($"[InGameHUD] Playtime error details: {ex}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error in GetCustomData: {ex.Message}");
                Console.WriteLine($"[InGameHUD] Database: {_config.MySqlConnection.Database}");
                Console.WriteLine($"[InGameHUD] Connection string: {_connectionString}");
                Console.WriteLine($"[InGameHUD] Stack trace: {ex.StackTrace}");
                return result;
            }
        }
    }
}