using System.Text.Json;

namespace InGameHUD.Managers
{
    public class LanguageManager
    {
        private readonly Dictionary<string, Dictionary<string, object>> _translations;
        private readonly string _defaultLanguage;
        private readonly string _langDirectory;

        public LanguageManager(string moduleDirectory, string defaultLanguage = "zh")
        {
            _translations = new Dictionary<string, Dictionary<string, object>>();
            _defaultLanguage = defaultLanguage;
            _langDirectory = Path.Combine(moduleDirectory, "lang");
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            try
            {
                if (!Directory.Exists(_langDirectory))
                {
                    Directory.CreateDirectory(_langDirectory);
                    CreateDefaultLanguageFiles();
                }

                foreach (var file in Directory.GetFiles(_langDirectory, "*.json"))
                {
                    try
                    {
                        var language = Path.GetFileNameWithoutExtension(file);
                        var json = File.ReadAllText(file);
                        var translation = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                        if (translation != null)
                        {
                            _translations[language] = translation;
                            Console.WriteLine($"[InGameHUD] Loaded language file: {language}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error loading language file {file}: {ex.Message}");
                    }
                }

                if (!_translations.ContainsKey(_defaultLanguage))
                {
                    Console.WriteLine($"[InGameHUD] Default language '{_defaultLanguage}' not found, creating it...");
                    CreateDefaultLanguageFiles();
                    LoadTranslations();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error in LoadTranslations: {ex.Message}");
                throw;
            }
        }

        private void CreateDefaultLanguageFiles()
        {
            var zhContent = new Dictionary<string, object>
            {
                ["hud"] = new Dictionary<string, string>
                {
                    ["player_name"] = "玩家: {0}",
                    ["kda"] = "KDA: {0}/{1}/{2}",
                    ["weapon"] = "武器: {0} ({1}/{2})",
                    ["credits"] = "积分: {0}",
                    ["playtime"] = "游玩时间: {0}小时{1}分钟",
                    ["enabled"] = "HUD已启用",
                    ["disabled"] = "HUD已禁用",
                    ["position_usage"] = "用法: !hudpos <1-5>",
                    ["position_help"] = "1=左上 | 2=左下 | 3=右上 | 4=右下 | 5=中间",
                    ["position_changed"] = "HUD位置已更改",
                    ["position_invalid"] = "无效的位置值，请使用1-5",
                    ["language_changed"] = "语言已更改为{0}"
                }
            };

            var enContent = new Dictionary<string, object>
            {
                ["hud"] = new Dictionary<string, string>
                {
                    ["player_name"] = "Player: {0}",
                    ["kda"] = "KDA: {0}/{1}/{2}",
                    ["weapon"] = "Weapon: {0} ({1}/{2})",
                    ["credits"] = "Credits: {0}",
                    ["playtime"] = "Playtime: {0}h {1}m",
                    ["enabled"] = "HUD Enabled",
                    ["disabled"] = "HUD Disabled",
                    ["position_usage"] = "Usage: !hudpos <1-5>",
                    ["position_help"] = "1=TopLeft | 2=BottomLeft | 3=TopRight | 4=BottomRight | 5=Center",
                    ["position_changed"] = "HUD position changed",
                    ["position_invalid"] = "Invalid position value, use 1-5",
                    ["language_changed"] = "Language changed to {0}"
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                File.WriteAllText(
                    Path.Combine(_langDirectory, "zh.json"),
                    JsonSerializer.Serialize(zhContent, options)
                );

                File.WriteAllText(
                    Path.Combine(_langDirectory, "en.json"),
                    JsonSerializer.Serialize(enContent, options)
                );

                Console.WriteLine("[InGameHUD] Created default language files");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error creating language files: {ex.Message}");
                throw;
            }
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
                            switch (element.ValueKind)
                            {
                                case JsonValueKind.Object:
                                    current = element.Deserialize<Dictionary<string, object>>()!;
                                    continue;
                                case JsonValueKind.String:
                                    var format = element.GetString();
                                    if (format != null)
                                    {
                                        try
                                        {
                                            return string.Format(format, args);
                                        }
                                        catch (FormatException)
                                        {
                                            return format;
                                        }
                                    }
                                    break;
                            }
                        }
                        else if (value is Dictionary<string, object> dict)
                        {
                            current = dict;
                            continue;
                        }
                        else if (value is string str)
                        {
                            try
                            {
                                return string.Format(str, args);
                            }
                            catch (FormatException)
                            {
                                return str;
                            }
                        }
                    }
                }

                Console.WriteLine($"[InGameHUD] Translation not found for key: {key} in language: {language}");
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error getting phrase for key {key}: {ex.Message}");
                return key;
            }
        }
    }
}