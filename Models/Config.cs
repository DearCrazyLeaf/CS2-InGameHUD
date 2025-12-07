using System.Drawing;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace InGameHUD.Models
{
    public class Config : IBasePluginConfig
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 6;

        [JsonPropertyName("font_size")]
        public int FontSize { get; set; } = 20;

        [JsonPropertyName("font_name")]
        public string FontName { get; set; } = "Arial Bold";

        [JsonPropertyName("scale")]
        public float Scale { get; set; } = 0.08f;

        [JsonPropertyName("background_opacity")]
        public float BackgroundOpacity { get; set; } = 0.2f;

        [JsonPropertyName("background_scale")]
        public float BackgroundScale { get; set; } = 0.1f;

        [JsonPropertyName("show_kda")]
        public bool ShowKDA { get; set; } = true;

        [JsonPropertyName("show_health")]
        public bool ShowHealth { get; set; } = true;

        [JsonPropertyName("show_team")]
        public bool ShowTeams { get; set; } = true;

        [JsonPropertyName("show_time")]
        public bool ShowTime { get; set; } = true;

        [JsonPropertyName("show_map_time")]
        public bool ShowMapTime { get; set; } = true;

        [JsonPropertyName("map_time_mode")]
        public MapTimeMode MapTimeMode { get; set; } = MapTimeMode.TimeLimit;

        [JsonPropertyName("show_ping")]
        public bool ShowPing { get; set; } = true;

        [JsonPropertyName("show_score")]
        public bool ShowScore { get; set; } = true;

        [JsonPropertyName("show_announcement_title")]
        public bool ShowAnnouncementTitle { get; set; } = true;

        [JsonPropertyName("show_announcement")]
        public bool ShowAnnouncement { get; set; } = true;

        private string _textColorName = "Orange";

        [JsonPropertyName("text_color")]
        public string TextColor
        {
            get => _textColorName;
            set
            {
                _textColorName = value;
                try
                {
                    _ = Color.FromName(value);
                }
                catch
                {
                    Console.WriteLine($"[InGameHUD] Invalid color name: {value}, using default White");
                    _textColorName = "White";
                }
            }
        }

        [JsonPropertyName("hud_toggle_mode")]
        public int HudToggleMode { get; set; } = 1; // 1=命令永久显示，2=Tab短暂显示

        [JsonPropertyName("hud_tab_duration_sec")]
        public float HudTabDurationSec { get; set; } = 3.0f; // Tab 模式显示时长

        [JsonPropertyName("hudcommand")]
        public List<string> HUDCommands { get; set; } = ["hud","togglehud"];

        [JsonPropertyName("poscommand")]
        public List<string> POSCommands { get; set; } = ["hudpos", "changepos"];

        [JsonPropertyName("mysql_connection")]
        public MySqlSettings MySqlConnection { get; set; } = new();

        [JsonPropertyName("custom_data")]
        public CustomDataSettings CustomData { get; set; } = new();

        [JsonPropertyName("positions")]
        public PositionsSettings Positions { get; set; } = new();
    }

    public class PositionsSettings
    {
        [JsonPropertyName("TopLeft")]
        public PositionConfig TopLeft { get; set; } = new PositionConfig(-20, 3, 42);

        [JsonPropertyName("TopRight")]
        public PositionConfig TopRight { get; set; } = new PositionConfig(20, 3, 42);

        [JsonPropertyName("BottomLeft")]
        public PositionConfig BottomLeft { get; set; } = new PositionConfig(-20, -3, 42);

        [JsonPropertyName("BottomRight")]
        public PositionConfig BottomRight { get; set; } = new PositionConfig(20, -3, 42);

        [JsonPropertyName("Center")]
        public PositionConfig Center { get; set; } = new PositionConfig(0, 0, 42);
    }

    public class MySqlSettings
    {
        [JsonPropertyName("use_mysql")]
        public bool Enabled { get; set; } = false;

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
        public CreditsSetting Credits { get; set; } = new()
        {
            Enabled = false,
        };

        [JsonPropertyName("playtime")]
        public CustomTableSettings Playtime { get; set; } = new()
        {
            Enabled = true,
            SchemaName = "SchemaName",
            TableName = "players_stats",
            ColumnName = "playtime",
            ColumnSteamID = "steam_id"
        };

        [JsonPropertyName("signin")]
        public CustomTableSettings Signin { get; set; } = new()
        {
            Enabled = true,
            SchemaName = "SchemaName",
            TableName = "player_signin",
            ColumnName = "last_signin",
            ColumnSteamID = "steamid64"
        };

        [JsonPropertyName("customdisplay1")]
        public CustomTableSettings Display1 { get; set; } = new()
        {
            Enabled = false,
            SchemaName = "SchemaName",
            TableName = "TableName",
            ColumnName = "ColumnName",
            ColumnSteamID = "ColumnSteamID"
        };

        [JsonPropertyName("customdisplay2")]
        public CustomTableSettings Display2 { get; set; } = new()
        {
            Enabled = false,
            SchemaName = "SchemaName",
            TableName = "TableName",
            ColumnName = "ColumnName",
            ColumnSteamID = "ColumnSteamID"
        };

        [JsonPropertyName("customdisplay3")]
        public CustomTableSettings Display3 { get; set; } = new()
        {
            Enabled = false,
            SchemaName = "SchemaName",
            TableName = "TableName",
            ColumnName = "ColumnName",
            ColumnSteamID = "ColumnSteamID"
        };
    }

    public class CreditsSetting
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class CustomTableSettings
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("schema_name")]
        public string SchemaName { get; set; } = "";

        [JsonPropertyName("table_name")]
        public string TableName { get; set; } = "";

        [JsonPropertyName("column_name")]
        public string ColumnName { get; set; } = "";

        [JsonPropertyName("column_steamid")]
        public string ColumnSteamID { get; set; } = "";
    }
}