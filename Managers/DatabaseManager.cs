using System.Data.SQLite;
using MySqlConnector;
using InGameHUD.Models;

namespace InGameHUD.Managers
{
    public class DatabaseManager
    {
        private readonly string _dbPath;
        private readonly string _mysqlConnectionString;
        private readonly Config _config;

        public DatabaseManager(string moduleDirectory, Config config)
        {
            Console.WriteLine($"[InGameHUD] DatabaseManager initialization:");
            Console.WriteLine($"[InGameHUD] Input moduleDirectory: {moduleDirectory}");

            if (string.IsNullOrEmpty(moduleDirectory))
                throw new ArgumentNullException(nameof(moduleDirectory));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config;

            try
            {
                // 确保路径是绝对路径
                _dbPath = Path.GetFullPath(Path.Combine(moduleDirectory, "player_settings.db"));

                // 确保目录存在
                var directory = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 构建MySQL连接字符串
                _mysqlConnectionString = $"Server={config.MySqlConnection.Host};" +
                                       $"Port={config.MySqlConnection.Port};" +
                                       $"Database={config.MySqlConnection.Database};" +
                                       $"User={config.MySqlConnection.Username};" +
                                       $"Password={config.MySqlConnection.Password}";

                InitializeDatabase();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize DatabaseManager: {ex.Message}", ex);
            }
        }

        private void InitializeDatabase()
        {
            Console.WriteLine($"[InGameHUD] Database initialization:");
            Console.WriteLine($"[InGameHUD] Database path: {_dbPath}");
            Console.WriteLine($"[InGameHUD] Database directory exists: {Directory.Exists(Path.GetDirectoryName(_dbPath))}");

            if (string.IsNullOrEmpty(_dbPath))
                throw new InvalidOperationException("Database path is not initialized");

            try
            {
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                }

                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();

                using var cmd = new SQLiteCommand(
                    @"CREATE TABLE IF NOT EXISTS player_settings (
                    steam_id TEXT PRIMARY KEY,
                    hud_enabled INTEGER DEFAULT 1,
                    hud_position INTEGER DEFAULT 0,
                    language TEXT DEFAULT 'zh'
                )", conn);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize database: {ex.Message}", ex);
            }
        }

        public async Task<PlayerData> LoadPlayerData(string steamId)
        {
            var playerData = new PlayerData { SteamID = steamId };

            // 加载本地设置
            using (var sqliteConn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                await sqliteConn.OpenAsync();
                using var cmd = new SQLiteCommand(
                    "SELECT hud_enabled, hud_position, language FROM player_settings WHERE steam_id = @steamId",
                    sqliteConn);
                cmd.Parameters.AddWithValue("@steamId", steamId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    playerData.HUDEnabled = reader.GetInt32(0) == 1;
                    playerData.HUDPosition = (HUDPosition)reader.GetInt32(1);
                    playerData.Language = reader.GetString(2);
                }
                else
                {
                    using var insertCmd = new SQLiteCommand(
                        @"INSERT INTO player_settings 
                        (steam_id, hud_enabled, hud_position, language) 
                        VALUES (@steamId, 1, 0, @lang)",
                        sqliteConn);
                    insertCmd.Parameters.AddWithValue("@steamId", steamId);
                    insertCmd.Parameters.AddWithValue("@lang", _config.DefaultLanguage);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            // 加载MySQL数据
            try
            {
                using var mysqlConn = new MySqlConnection(_mysqlConnectionString);
                await mysqlConn.OpenAsync();

                // 加载积分
                if (_config.CustomData.Credits.Enabled)
                {
                    using var creditsCmd = new MySqlCommand(
                        $"SELECT {_config.CustomData.Credits.ColumnName} " +
                        $"FROM {_config.CustomData.Credits.TableName} " +
                        $"WHERE steam_id = @steamId", mysqlConn);
                    creditsCmd.Parameters.AddWithValue("@steamId", steamId);

                    var creditsResult = await creditsCmd.ExecuteScalarAsync();
                    if (creditsResult != null && creditsResult != DBNull.Value)
                    {
                        playerData.Credits = Convert.ToInt32(creditsResult);
                    }
                }

                // 加载游玩时间
                if (_config.CustomData.Playtime.Enabled)
                {
                    using var playtimeCmd = new MySqlCommand(
                        $"SELECT {_config.CustomData.Playtime.ColumnName} " +
                        $"FROM {_config.CustomData.Playtime.TableName} " +
                        $"WHERE steam_id = @steamId", mysqlConn);
                    playtimeCmd.Parameters.AddWithValue("@steamId", steamId);

                    var playtimeResult = await playtimeCmd.ExecuteScalarAsync();
                    if (playtimeResult != null && playtimeResult != DBNull.Value)
                    {
                        playerData.Playtime = TimeSpan.FromSeconds(Convert.ToInt32(playtimeResult));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading MySQL data: {ex.Message}");
            }

            return playerData;
        }

        public async Task SavePlayerData(string steamId, PlayerData playerData)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(
                @"INSERT OR REPLACE INTO player_settings 
                (steam_id, hud_enabled, hud_position, language) 
                VALUES (@steamId, @enabled, @position, @lang)", conn);

            cmd.Parameters.AddWithValue("@steamId", steamId);
            cmd.Parameters.AddWithValue("@enabled", playerData.HUDEnabled ? 1 : 0);
            cmd.Parameters.AddWithValue("@position", (int)playerData.HUDPosition);
            cmd.Parameters.AddWithValue("@lang", playerData.Language);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}