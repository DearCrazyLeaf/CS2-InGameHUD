namespace InGameHUD.Models
{
    public class PlayerData
    {
        public PlayerData(string steamId)
        {
            SteamID = steamId;
        }

        public string SteamID { get; }
        public string Language { get; set; } = "zh-Hans";
        public HUDPosition HUDPosition { get; set; } = HUDPosition.TopRight;
        public bool HUDEnabled { get; set; } = true;
        public Dictionary<string, string> CustomData { get; set; } = new();
    }
}