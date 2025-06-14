using System.Drawing;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace InGameHUD.Models
{
    public class Config : IBasePluginConfig
    {
        public int Version { get; set; } = 1;
        [JsonPropertyName("show_kda")]
        public bool ShowKDA { get; set; } = true;

        [JsonPropertyName("show_weapon")]
        public bool ShowWeapon { get; set; } = true;

        [JsonPropertyName("text_color")]
        public Color TextColor { get; set; } = Color.White;

        [JsonPropertyName("announcement_color")]
        public Color AnnouncementColor { get; set; } = Color.Yellow;

        [JsonPropertyName("mysql_connection")]
        public MySqlSettings MySqlConnection { get; set; } = new();

        [JsonPropertyName("custom_data")]
        public CustomDataSettings CustomData { get; set; } = new();

        [JsonPropertyName("hud_update_interval")]
        public float HUDUpdateInterval { get; set; } = 0.1f;

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
            ColumnName = "credits",
            DisplayName = "credits"
        };

        [JsonPropertyName("playtime")]
        public CustomTableSettings Playtime { get; set; } = new()
        {
            Enabled = true,
            TableName = "players_stats",
            ColumnName = "playtime",
            DisplayName = "playtime"
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

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = "";
    }
}