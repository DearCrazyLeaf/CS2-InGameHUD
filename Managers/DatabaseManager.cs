using InGameHUD.Models;

namespace InGameHUD.Managers
{
    public class DatabaseManager
    {
        private readonly Config _config;
        private readonly string _pluginPath;
        private IPlayerData? _activeProvider;
        private bool _isInitialized = false;

        public DatabaseManager(Config config, string pluginPath)
        {
            _config = config;
            _pluginPath = pluginPath;
        }

        public async Task<(bool success, string error)> InitializeAsync()
        {
            try
            {
                if (_config.MySqlConnection.Enabled)
                {
                    Console.WriteLine("[InGameHUD] MySQL is enabled, trying to connect...");
                    var mysqlProvider = new MySQLManager(_config);
                    bool mysqlSuccess = await mysqlProvider.InitializeAsync();

                    if (mysqlSuccess)
                    {
                        Console.WriteLine("[InGameHUD] Using MySQL as data provider");
                        _activeProvider = mysqlProvider;
                        _isInitialized = true;
                        return (true, string.Empty);
                    }

                    Console.WriteLine("[InGameHUD] MySQL connection failed, falling back to SQLite");
                }
                else
                {
                    Console.WriteLine("[InGameHUD] MySQL is disabled in config, using SQLite instead");
                }
                try
                {
                    var sqliteProvider = new SQLiteManager(_pluginPath);
                    bool sqliteSuccess = await sqliteProvider.InitializeAsync();

                    if (sqliteSuccess)
                    {
                        Console.WriteLine("[InGameHUD] Using SQLite as data provider");
                        _activeProvider = sqliteProvider;
                        _isInitialized = true;
                        return (true, string.Empty);
                    }
                    else
                    {
                        return (false, "SQLite initialization failed");
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"SQLite initialization error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Database initialization error: {ex.Message}");
            }
        }

        public async Task<bool> SavePlayerSettings(PlayerData playerData)
        {
            if (!_isInitialized || _activeProvider == null)
                return false;

            try
            {
                return await _activeProvider.SavePlayerSettingsAsync(playerData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error saving player settings: {ex.Message}");
                return false;
            }
        }

        public async Task<PlayerData> LoadPlayerSettings(string steamId)
        {
            if (!_isInitialized || _activeProvider == null)
                return new PlayerData(steamId);

            try
            {
                return await _activeProvider.LoadPlayerSettingsAsync(steamId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading player settings: {ex.Message}");
                return new PlayerData(steamId);
            }
        }

        public async Task<Dictionary<string, string>> GetCustomData(string steamId)
        {
            if (!_isInitialized || _activeProvider == null)
                return new Dictionary<string, string>();

            try
            {
                return await _activeProvider.GetCustomDataAsync(steamId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error getting custom data: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public async Task<bool> BulkSavePlayerSettings(IEnumerable<PlayerData> playersData)
        {
            if (!_isInitialized || _activeProvider == null)
                return false;

            try
            {
                if (_activeProvider is SQLiteManager sqliteManager)
                {
                    return await sqliteManager.BulkSavePlayerSettingsAsync(playersData);
                }

                bool success = true;
                foreach (var playerData in playersData)
                {
                    if (!await _activeProvider.SavePlayerSettingsAsync(playerData))
                        success = false;
                }
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error bulk saving player settings: {ex.Message}");
                return false;
            }
        }

        public bool IsInitialized => _isInitialized;
    }
}