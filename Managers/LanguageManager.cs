using System.Text.Json;

namespace InGameHUD.Managers
{
    public class LanguageManager
    {
        private readonly Dictionary<string, Dictionary<string, string>> _phrases = new();
        private readonly string _defaultLanguage;

        public LanguageManager(string moduleDirectory, string defaultLanguage)
        {
            _defaultLanguage = defaultLanguage;
            LoadLanguages(moduleDirectory);
        }

        private void LoadLanguages(string moduleDirectory)
        {
            var langDir = Path.Combine(moduleDirectory, "lang");
            if (!Directory.Exists(langDir))
            {
                Directory.CreateDirectory(langDir);
                CreateDefaultLanguageFiles(langDir);
            }

            foreach (var file in Directory.GetFiles(langDir, "*.json"))
            {
                var lang = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);
                try
                {
                    var phrases = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (phrases != null)
                    {
                        _phrases[lang] = phrases;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Error loading language file {file}: {ex.Message}");
                }
            }
        }

        private void CreateDefaultLanguageFiles(string langDir)
        {
            var defaultPhrases = new Dictionary<string, string>
            {
                { "hud.enabled", "HUD has been enabled" },
                { "hud.disabled", "HUD has been disabled" },
                { "hud.position_usage", "Usage: !hudpos <1-5>" },
                { "hud.position_help", "1:TopLeft 2:TopRight 3:BottomLeft 4:BottomRight 5:Center" },
                { "hud.position_changed", "HUD position has been changed" },
                { "hud.position_invalid", "Invalid position! Use 1-5" },
                { "hud.language_changed", "Language has been changed" },
                { "hud.info_name", "Player: {0}" },
                { "hud.info_kda", "KDA: {0}/{1}/{2}" },
                { "hud.info_health", "HP: {0}" },
                { "hud.info_armor", "Armor: {0}" },
                { "hud.info_team", "Team: {0}" },
                { "hud.info_money", "Money: ${0}" }
            };

            var zhPhrases = new Dictionary<string, string>
            {
                { "hud.enabled", "HUD 已启用" },
                { "hud.disabled", "HUD 已禁用" },
                { "hud.position_usage", "用法: !hudpos <1-5>" },
                { "hud.position_help", "1:左上 2:右上 3:左下 4:右下 5:居中" },
                { "hud.position_changed", "HUD 位置已更改" },
                { "hud.position_invalid", "无效的位置! 请使用 1-5" },
                { "hud.language_changed", "语言已更改" },
                { "hud.info_name", "玩家: {0}" },
                { "hud.info_kda", "战绩: {0}/{1}/{2}" },
                { "hud.info_health", "生命值: {0}" },
                { "hud.info_armor", "护甲: {0}" },
                { "hud.info_team", "阵营: {0}" },
                { "hud.info_money", "金钱: ${0}" }
            };

            File.WriteAllText(
                Path.Combine(langDir, "en.json"),
                JsonSerializer.Serialize(defaultPhrases, new JsonSerializerOptions { WriteIndented = true })
            );

            File.WriteAllText(
                Path.Combine(langDir, "zh.json"),
                JsonSerializer.Serialize(zhPhrases, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        public string GetPhrase(string key, string language, params object[] args)
        {
            if (_phrases.TryGetValue(language, out var langPhrases) &&
                langPhrases.TryGetValue(key, out var phrase))
            {
                return string.Format(phrase, args);
            }

            if (_phrases.TryGetValue(_defaultLanguage, out var defaultPhrases) &&
                defaultPhrases.TryGetValue(key, out var defaultPhrase))
            {
                return string.Format(defaultPhrase, args);
            }

            return key;
        }
    }
}