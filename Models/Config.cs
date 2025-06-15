using System.Drawing;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace InGameHUD.Models
{
    public class Config : IBasePluginConfig
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("font_size")]
        public int FontSize { get; set; } = 50;

        [JsonPropertyName("font_name")]
        public string FontName { get; set; } = "Arial Bold";

        [JsonPropertyName("scale")]
        public float Scale { get; set; } = 0.1f;

        [JsonPropertyName("background_opacity")]
        public float BackgroundOpacity { get; set; } = 0.6f;

        [JsonPropertyName("background_scale")]
        public float BackgroundScale { get; set; } = 0.3f;

        [JsonPropertyName("show_kda")]
        public bool ShowKDA { get; set; } = true;

        [JsonPropertyName("show_health")]
        public bool ShowHealth { get; set; } = true;

        [JsonPropertyName("show_team")]
        public bool ShowTeams { get; set; } = true;

        // 新增：是否显示当前系统时间
        [JsonPropertyName("show_time")]
        public bool ShowTime { get; set; } = true;

        // 新增：是否显示玩家 Ping
        [JsonPropertyName("show_ping")]
        public bool ShowPing { get; set; } = true;

        // 新增：是否显示玩家得分（积分）
        [JsonPropertyName("show_score")]
        public bool ShowScore { get; set; } = true;

        // 新增：是否显示自定义公告信息
        [JsonPropertyName("show_announcement_title")]
        public bool ShowAnnouncementTitle { get; set; } = true;

        [JsonPropertyName("show_announcement")]
        public bool ShowAnnouncement { get; set; } = true;

        private string _textColorName = "White";

        [JsonPropertyName("text_color")]
        public string TextColor
        {
            get => _textColorName;
            set
            {
                _textColorName = value;
                try
                {
                    // 尝试将字符串转换为Color对象，验证颜色名称是否有效
                    _ = Color.FromName(value);
                }
                catch
                {
                    Console.WriteLine($"[InGameHUD] Invalid color name: {value}, using default White");
                    _textColorName = "White";
                }
            }
        }

        [JsonPropertyName("mysql_connection")]
        public MySqlSettings MySqlConnection { get; set; } = new();

        [JsonPropertyName("custom_data")]
        public CustomDataSettings CustomData { get; set; } = new();

        [JsonPropertyName("default_language")]
        public string DefaultLanguage { get; set; } = "zh";
    }

    public class MySqlSettings
    {
        [JsonPropertyName("host")]
        public string Host { get; set; } = "localhost";

        [JsonPropertyName("port")]
        public int Port { get; set; } = 3306;

        [JsonPropertyName("database")]
        public string Database { get; set; } = "cs2server";

        [JsonPropertyName("username")]
        public string Username { get; set; } = "root";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
    }

    public class CustomDataSettings
    {
        [JsonPropertyName("credits")]
        public CustomTableSettings Credits { get; set; } = new()
        {
            Enabled = true,
            TableName = "store_players",
            ColumnName = "credits"
        };

        [JsonPropertyName("playtime")]
        public CustomTableSettings Playtime { get; set; } = new()
        {
            Enabled = true,
            TableName = "players_stats",
            ColumnName = "playtime"
        };

        [JsonPropertyName("signin")]
        public CustomTableSettings Signin { get; set; } = new()
        {
            Enabled = true,
            TableName = "player_signin",
            ColumnName = "last_signin"
        };
    }

    public class CustomTableSettings
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("table_name")]
        public string TableName { get; set; } = "";

        [JsonPropertyName("column_name")]
        public string ColumnName { get; set; } = "";
    }
}