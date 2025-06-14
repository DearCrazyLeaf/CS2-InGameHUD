using System.Text.Json;

namespace InGameHUD.Managers
{
    public class LanguageManager
    {
        private readonly Dictionary<string, Dictionary<string, object>> _translations;
        private readonly string _defaultLanguage;

        public LanguageManager(string moduleDirectory, string defaultLanguage = "zh")
        {
            _translations = new Dictionary<string, Dictionary<string, object>>();
            _defaultLanguage = defaultLanguage;
            LoadTranslations(moduleDirectory);
        }

        private void LoadTranslations(string moduleDirectory)
        {
            var langDirectory = Path.Combine(moduleDirectory, "lang");
            if (!Directory.Exists(langDirectory))
            {
                Directory.CreateDirectory(langDirectory);
                CreateDefaultLanguageFiles(langDirectory);
            }

            foreach (var file in Directory.GetFiles(langDirectory, "*.json"))
            {
                try
                {
                    var language = Path.GetFileNameWithoutExtension(file);
                    var json = File.ReadAllText(file);
                    var translation = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (translation != null)
                    {
                        _translations[language] = translation;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading language file {file}: {ex.Message}");
                }
            }

            if (!_translations.ContainsKey(_defaultLanguage))
            {
                throw new Exception($"Default language '{_defaultLanguage}' not found!");
            }
        }

        private void CreateDefaultLanguageFiles(string directory)
        {
            // 创建默认的中文语言文件
            var zhContent = new Dictionary<string, object>
            {
                ["hud"] = new Dictionary<string, string>
                {
                    ["player_name"] = "玩家: {0}",
                    ["kda"] = "KDA: {0}/{1}/{2}",
                    ["weapon"] = "武器: {0} ({1}/{2})",
                    ["credits"] = "积分: {0}",
                    ["playtime"] = "游玩时间: {0}",
                    ["enabled"] = "HUD已启用",
                    ["disabled"] = "HUD已禁用",
                    ["position_usage"] = "用法: !hudpos <1-5>",
                    ["position_help"] = "1 - 左上 | 2 - 左下 | 3 - 右上 | 4 - 右下 | 5 - 中间"
                }
            };

            File.WriteAllText(
                Path.Combine(directory, "zh.json"),
                JsonSerializer.Serialize(zhContent, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        public string GetPhrase(string key, string language = "zh", params object[] args)
        {
            try
            {
                if (!_translations.ContainsKey(language))
                {
                    language = _defaultLanguage;
                }

                var keys = key.Split('.');
                var current = _translations[language];

                foreach (var k in keys)
                {
                    if (current.TryGetValue(k, out var value))
                    {
                        if (value is JsonElement element)
                        {
                            if (element.ValueKind == JsonValueKind.Object)
                            {
                                current = element.Deserialize<Dictionary<string, object>>()!;
                                continue;
                            }
                            if (element.ValueKind == JsonValueKind.String)
                            {
                                return string.Format(element.GetString()!, args);
                            }
                        }
                    }
                }

                return key;
            }
            catch (Exception)
            {
                return key;
            }
        }
    }
}