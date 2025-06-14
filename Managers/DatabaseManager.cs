using MySqlConnector;
using InGameHUD.Models;

namespace InGameHUD.Managers
{
    public class DatabaseManager
    {
        private readonly Config _config;
        private readonly Dictionary<string, PlayerData> _memoryStorage;
        private string? _connectionString;
        private bool _mysqlAvailable;

        public DatabaseManager(string moduleDirectory, Config config)
        {
            _config = config;
            _memoryStorage = new Dictionary<string, PlayerData>();

            Console.WriteLine("[InGameHUD] DatabaseManager initialization:");

            try
            {
                InitializeMySQLConnection();
                if (_mysqlAvailable)
                {
                    CreateSettingsTable();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] MySQL initialization failed: {ex.Message}");
                _mysqlAvailable = false;
            }
        }

        private void InitializeMySQLConnection()
        {
            try
            {
                _connectionString = $"Server={_config.MySqlConnection.Host};" +
                                  $"Port={_config.MySqlConnection.Port};" +
                                  $"Database={_config.MySqlConnection.Database};" +
                                  $"User={_config.MySqlConnection.Username};" +
                                  $"Password={_config.MySqlConnection.Password};" +
                                  "CharSet=utf8;";

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    _mysqlAvailable = true;
                    Console.WriteLine("[InGameHUD] MySQL connection successful");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] MySQL connection failed: {ex.Message}");
                _mysqlAvailable = false;
                _connectionString = null;
            }
        }

        private void CreateSettingsTable()
        {
            if (!_mysqlAvailable || string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ingamehud_settings (
                    steam_id VARCHAR(64) PRIMARY KEY,
                    hud_enabled BOOLEAN DEFAULT TRUE,
                    hud_position INT DEFAULT 0,
                    language VARCHAR(10) DEFAULT 'zh',
                    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                )";
                command.ExecuteNonQuery();
                Console.WriteLine("[InGameHUD] Settings table created or verified");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Failed to create settings table: {ex.Message}");
                _mysqlAvailable = false;
            }
        }

        public async Task<PlayerData> LoadPlayerData(string steamId)
        {
            var playerData = new PlayerData(steamId);

            if (_mysqlAvailable && !string.IsNullOrEmpty(_connectionString))
            {
                try
                {
                    using var connection = new MySqlConnection(_connectionString);
                    await connection.OpenAsync();

                    // 使用事务来确保数据一致性
                    using var transaction = await connection.BeginTransactionAsync();
                    try
                    {
                        // 加载设置
                        using var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = "SELECT hud_enabled, hud_position, language FROM ingamehud_settings WHERE steam_id = @steam_id";
                        command.Parameters.AddWithValue("@steam_id", steamId);

                        using var reader = await command.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            playerData.HUDEnabled = reader.GetBoolean(0);
                            playerData.HUDPosition = (HUDPosition)reader.GetInt32(1);
                            playerData.Language = reader.GetString(2);
                        }
                        else
                        {
                            // 关闭当前reader以执行插入操作
                            reader.Close();

                            // 插入默认设置
                            command.CommandText = @"
                            INSERT INTO ingamehud_settings (steam_id, hud_enabled, hud_position, language)
                            VALUES (@steam_id, @hud_enabled, @hud_position, @language)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@steam_id", steamId);
                            command.Parameters.AddWithValue("@hud_enabled", true);
                            command.Parameters.AddWithValue("@hud_position", (int)HUDPosition.TopRight);
                            command.Parameters.AddWithValue("@language", _config.DefaultLanguage);
                            await command.ExecuteNonQueryAsync();
                        }

                        // 加载自定义数据
                        if (_config.CustomData.Credits.Enabled)
                        {
                            command.CommandText = $"SELECT {_config.CustomData.Credits.ColumnName} FROM {_config.CustomData.Credits.TableName} WHERE steam_id = @steamid";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@steamid", steamId);
                            var credits = await command.ExecuteScalarAsync();
                            if (credits != null && credits != DBNull.Value)
                            {
                                playerData.Credits = Convert.ToInt32(credits);
                            }
                        }

                        if (_config.CustomData.Playtime.Enabled)
                        {
                            command.CommandText = $"SELECT {_config.CustomData.Playtime.ColumnName} FROM {_config.CustomData.Playtime.TableName} WHERE steam_id = @steamid";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@steamid", steamId);
                            var playtime = await command.ExecuteScalarAsync();
                            if (playtime != null && playtime != DBNull.Value)
                            {
                                playerData.Playtime = TimeSpan.FromSeconds(Convert.ToInt32(playtime));
                            }
                        }

                        await transaction.CommitAsync();
                        Console.WriteLine($"[InGameHUD] Successfully loaded data for player {steamId}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Failed to load data from MySQL: {ex.Message}");
                    // 如果MySQL加载失败，使用内存存储
                    if (_memoryStorage.TryGetValue(steamId, out var storedData))
                    {
                        playerData = storedData;
                    }
                }
            }
            else
            {
                // 从内存加载设置
                if (_memoryStorage.TryGetValue(steamId, out var storedData))
                {
                    playerData = storedData;
                }
                else
                {
                    playerData.HUDEnabled = true;
                    playerData.HUDPosition = HUDPosition.TopRight;
                    playerData.Language = _config.DefaultLanguage;
                    _memoryStorage[steamId] = playerData;
                }
            }

            return playerData;
        }

        public async Task SavePlayerData(string steamId, PlayerData playerData)
        {
            if (_mysqlAvailable && !string.IsNullOrEmpty(_connectionString))
            {
                try
                {
                    using var connection = new MySqlConnection(_connectionString);
                    await connection.OpenAsync();

                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                    INSERT INTO ingamehud_settings 
                        (steam_id, hud_enabled, hud_position, language)
                    VALUES 
                        (@steam_id, @hud_enabled, @hud_position, @language)
                    ON DUPLICATE KEY UPDATE
                        hud_enabled = VALUES(hud_enabled),
                        hud_position = VALUES(hud_position),
                        language = VALUES(language)";

                    command.Parameters.AddWithValue("@steam_id", steamId);
                    command.Parameters.AddWithValue("@hud_enabled", playerData.HUDEnabled);
                    command.Parameters.AddWithValue("@hud_position", (int)playerData.HUDPosition);
                    command.Parameters.AddWithValue("@language", playerData.Language);

                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"[InGameHUD] Successfully saved settings for player {steamId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Failed to save settings to MySQL: {ex.Message}");
                    // 如果MySQL保存失败，保存到内存
                    _memoryStorage[steamId] = playerData;
                }
            }
            else
            {
                _memoryStorage[steamId] = playerData;
            }
        }

        public void Dispose()
        {
            _memoryStorage.Clear();
        }
    }
}