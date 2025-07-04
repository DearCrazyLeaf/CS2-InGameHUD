using System.Text.Json.Serialization;

namespace InGameHUD.Models
{
    public enum HUDPosition
    {
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight,
        Center
    }

    public enum MapTimeMode
    {
        TimeLimit = 0,
        RoundLimit = 1
    }

    public class PlayerData
    {
        public PlayerData(string steamId)
        {
            SteamID = steamId;
        }

        public string SteamID { get; }
        public HUDPosition HUDPosition { get; set; } = HUDPosition.TopRight;
        public bool HUDEnabled { get; set; } = true;
        public Dictionary<string, string> CustomData { get; set; } = new();
    }

    public interface IPlayerData
    {
        Task<bool> InitializeAsync();
        Task<bool> SavePlayerSettingsAsync(PlayerData playerData);
        Task<PlayerData> LoadPlayerSettingsAsync(string steamId);
        Task<Dictionary<string, string>> GetCustomDataAsync(string steamId);
    }

    public class PlayerSettingsDto
    {
        public string SteamId { get; set; } = string.Empty;
        public int HudEnabled { get; set; } = 1;
        public int HudPosition { get; set; } = 1;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;

        public PlayerData ToPlayerData()
        {
            return new PlayerData(SteamId)
            {
                HUDEnabled = HudEnabled != 0,
                HUDPosition = (HUDPosition)(HudPosition - 1)
            };
        }

        public static PlayerSettingsDto FromPlayerData(PlayerData playerData)
        {
            return new PlayerSettingsDto
            {
                SteamId = playerData.SteamID,
                HudEnabled = playerData.HUDEnabled ? 1 : 0,
                HudPosition = (int)playerData.HUDPosition + 1
            };
        }
    }

    public class PositionConfig
    {
        [JsonPropertyName("x_offset")]
        public float XOffset { get; set; }

        [JsonPropertyName("y_offset")]
        public float YOffset { get; set; }

        [JsonPropertyName("z_distance")]
        public float ZDistance { get; set; }

        public PositionConfig()
        {
            //
        }

        public PositionConfig(float x, float y, float z)
        {
            XOffset = x;
            YOffset = y;
            ZDistance = z;
        }
    }
}