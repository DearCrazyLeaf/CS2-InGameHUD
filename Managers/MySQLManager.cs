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
                        var settings = _config.CustomData.Playtime;
                        
                        if (string.IsNullOrEmpty(settings.TableName) || 
                            string.IsNullOrEmpty(settings.ColumnName) || 
                            string.IsNullOrEmpty(settings.ColumnSteamID))
                        {
                            Console.WriteLine("[InGameHUD] Error: Missing configuration for playtime");
                        }
                        else
                        {
                            var schemaName = string.IsNullOrEmpty(settings.SchemaName) 
                                ? _config.MySqlConnection.Database 
                                : settings.SchemaName;

                            string query = $@"
                                SELECT `{settings.ColumnName}` 
                                FROM `{schemaName}`.`{settings.TableName}` 
                                WHERE `{settings.ColumnSteamID}` = @SteamId;";

                            var playtimeResult = await connection.ExecuteScalarAsync<object>(
                                query, new { SteamId = steamId });

                            if (playtimeResult != null && playtimeResult != DBNull.Value)
                            {
                                result["playtime"] = playtimeResult.ToString() ?? "0";
                                //Console.WriteLine($"[InGameHUD] Successfully retrieved playtime data from database '{schemaName}': {result["playtime"]}");
                            }
                            else
                            {
                                Console.WriteLine($"[InGameHUD] No playtime data found in database '{schemaName}'");
                            }
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
                        var settings = _config.CustomData.Signin;
                        
                        if (string.IsNullOrEmpty(settings.TableName) || 
                            string.IsNullOrEmpty(settings.ColumnName) || 
                            string.IsNullOrEmpty(settings.ColumnSteamID))
                        {
                            Console.WriteLine("[InGameHUD] Error: Missing configuration for signin");
                        }
                        else
                        {
                            var schemaName = string.IsNullOrEmpty(settings.SchemaName) 
                                ? _config.MySqlConnection.Database 
                                : settings.SchemaName;

                            string query = $@"
                                SELECT `{settings.ColumnName}` 
                                FROM `{schemaName}`.`{settings.TableName}` 
                                WHERE `{settings.ColumnSteamID}` = @SteamId;";

                            var signinResult = await connection.ExecuteScalarAsync<object>(
                                query, new { SteamId = steamId });

                            if (signinResult != null && signinResult != DBNull.Value)
                            {
                                result["last_signin"] = signinResult.ToString()!;
                                //Console.WriteLine($"[InGameHUD] Successfully retrieved signin data from database '{schemaName}': {result["last_signin"]}");
                            }
                            else
                            {
                                Console.WriteLine($"[InGameHUD] No signin data found in database '{schemaName}'");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error getting signin data from MySQL: {ex.Message}");
                    }
                }

                var customDisplays = new Dictionary<string, CustomTableSettings>
                {
                    {"customdisplay1", _config.CustomData.Display1},
                    {"customdisplay2", _config.CustomData.Display2},
                    {"customdisplay3", _config.CustomData.Display3}
                };

                foreach (var display in customDisplays)
                {
                    var key = display.Key;
                    var settings = display.Value;
                    
                    if (settings.Enabled)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(settings.TableName) || 
                                string.IsNullOrEmpty(settings.ColumnName) || 
                                string.IsNullOrEmpty(settings.ColumnSteamID))
                            {
                                Console.WriteLine($"[InGameHUD] Error: Missing configuration for {key}");
                                continue;
                            }

                            var schemaName = string.IsNullOrEmpty(settings.SchemaName) 
                                ? _config.MySqlConnection.Database 
                                : settings.SchemaName;

                            string query = $@"
                                SELECT `{settings.ColumnName}` 
                                FROM `{schemaName}`.`{settings.TableName}` 
                                WHERE `{settings.ColumnSteamID}` = @SteamId;";

                            var customResult = await connection.ExecuteScalarAsync<object>(
                                query, new { SteamId = steamId });

                            if (customResult != null && customResult != DBNull.Value)
                            {
                                result[key] = customResult.ToString()!;
                                //Console.WriteLine($"[InGameHUD] Successfully retrieved {key} data from database '{schemaName}': {result[key]}");
                            }
                            else
                            {
                                Console.WriteLine($"[InGameHUD] No data found for {key} in database '{schemaName}'");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[InGameHUD] Error getting {key} from MySQL: {ex.Message}");
                        }
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