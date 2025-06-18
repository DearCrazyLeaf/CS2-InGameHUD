namespace InGameHUD.Models
{
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
}