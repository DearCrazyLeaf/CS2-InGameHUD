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
            var defaultPhrases = CreateEnglishPhrases();
            var zhPhrases = CreateChinesePhrases();

            File.WriteAllText(
                Path.Combine(langDir, "en.json"),
                JsonSerializer.Serialize(defaultPhrases, new JsonSerializerOptions { WriteIndented = true })
            );

            File.WriteAllText(
                Path.Combine(langDir, "zh-Hans.json"),
                JsonSerializer.Serialize(zhPhrases, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        private Dictionary<string, string> CreateEnglishPhrases()
        {
            return new Dictionary<string, string>
            {
                { "hud.greeting", "Hello! [{0}]" },
                { "hud.separator", "===================" },
                { "hud.current_time", "Current Time: {0}" },
                { "hud.ping", "Ping: {0} ms" },
                { "hud.kda", "KDA: {0}/{1}/{2}" },
                { "hud.health", "HP: {0}" },
                { "hud.team", "Team: {0}" },
                { "hud.score", "Score: {0}" },
                { "hud.credits", "Credits: {0}" },
                { "hud.last_signin", "Last Sign-in: {0}" },
                { "hud.never_signed", "Never signed in or data error" },
                { "hud.today", "Today" },
                { "hud.days_ago", "{0} days ago" },
                { "hud.playtime", "Playtime: {0} hours {1} minutes" },
                { "hud.separator_bottom", "===================" },
                { "hud.hint_toggle", "!hud to toggle panel" },
                { "hud.hint_help", "!help for help" },
                { "hud.hint_store", "!store to open shop" },
                { "hud.hint_website", "Official website: hlymcn.cn" },
                { "hud.separator_final", "===================" },
                { "hud.announcement_title", "Announcement" },
                { "hud.announcement_content", "Announcement content" },

                { "hud.team_t", "T" },
                { "hud.team_ct", "CT" },
                { "hud.team_spec", "SPEC" },

                { "hud.enabled", "HUD has been enabled" },
                { "hud.disabled", "HUD has been disabled" },
                { "hud.invalid_state", "Cannot enable HUD in current state (dead or spectator)" },
                { "hud.position_usage", "Usage: !hudpos <1-5>" },
                { "hud.position_help", "1:TopLeft 2:TopRight 3:BottomLeft 4:BottomRight 5:Center" },
                { "hud.position_changed", "HUD position has been changed" },
                { "hud.position_invalid", "Invalid position! Use 1-5" },
                { "hud.language_changed", "Language has been changed" }
            };
        }

        private Dictionary<string, string> CreateChinesePhrases()
        {
            return new Dictionary<string, string>
            {
                { "hud.greeting", "你好！【{0}】" },
                { "hud.separator", "===================" },
                { "hud.current_time", "当前时间: {0}" },
                { "hud.ping", "延迟: {0} ms" },
                { "hud.kda", "战绩: {0}/{1}/{2}" },
                { "hud.health", "生命值: {0}" },
                { "hud.team", "阵营: {0}" },
                { "hud.score", "得分: {0}" },
                { "hud.credits", "积分: {0}" },
                { "hud.last_signin", "上次签到: {0}" },
                { "hud.never_signed", "从未签到或数据异常" },
                { "hud.today", "今天" },
                { "hud.days_ago", "{0}天前" },
                { "hud.playtime", "游玩时长: {0}小时{1}分钟" },
                { "hud.separator_bottom", "===================" },
                { "hud.hint_toggle", "!hud开关面板" },
                { "hud.hint_help", "!help查看帮助" },
                { "hud.hint_store", "!store打开商店" },
                { "hud.hint_website", "官方网站: hlymcn.cn" },
                { "hud.separator_final", "===================" },
                { "hud.announcement_title", "公告标题" },
                { "hud.announcement_content", "公告内容" },

                { "hud.team_t", "T" },
                { "hud.team_ct", "CT" },
                { "hud.team_spec", "观察者" },

                { "hud.enabled", "HUD 已启用" },
                { "hud.disabled", "HUD 已禁用" },
                { "hud.invalid_state", "当前状态无法启用HUD（死亡或观察者）" },
                { "hud.position_usage", "用法: !hudpos <1-5>" },
                { "hud.position_help", "1:左上 2:右上 3:左下 4:右下 5:居中" },
                { "hud.position_changed", "HUD 位置已更改" },
                { "hud.position_invalid", "无效的位置! 请使用 1-5" },
                { "hud.language_changed", "语言已更改" }
            };
        }

        public string GetPhrase(string key, string lang = "", params object[] args)
        {
            if (string.IsNullOrEmpty(lang))
                lang = _defaultLanguage;

            if (!_phrases.ContainsKey(lang))
                lang = _defaultLanguage;

            if (!_phrases.ContainsKey(lang))
                lang = "en";

            if (!_phrases.ContainsKey(lang))
                throw new Exception($"Language file for {lang} not found");

            if (_phrases[lang].TryGetValue(key, out string? phrase))
                return args.Length > 0 ? string.Format(phrase, args) : phrase;

            if (lang != "en" && _phrases.ContainsKey("en") && _phrases["en"].TryGetValue(key, out phrase))
                return args.Length > 0 ? string.Format(phrase, args) : phrase;

            return $"[Missing: {key}]";
        }
    }
}