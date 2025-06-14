using System.Text.Json;
using InGameHUD.Models;

namespace InGameHUD.Managers
{
    public class ConfigManager
    {
        public Config Config { get; private set; }

        public ConfigManager(string moduleDirectory)
        {
            string configPath = Path.Join(moduleDirectory, "config.json");
            LoadConfig(configPath);
        }

        private void LoadConfig(string path)
        {
            if (!File.Exists(path))
            {
                Config = new Config();
                string jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, jsonString);
            }
            else
            {
                string jsonString = File.ReadAllText(path);
                Config = JsonSerializer.Deserialize<Config>(jsonString) ?? new Config();
            }
        }
    }
}