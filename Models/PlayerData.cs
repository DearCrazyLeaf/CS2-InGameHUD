using CounterStrikeSharp.API.Modules.Entities;

namespace InGameHUD.Models
{
    public class PlayerData
    {
        public string SteamID { get; set; } = string.Empty;
        public int Credits { get; set; }
        public TimeSpan Playtime { get; set; }
        public HUDPosition HUDPosition { get; set; } = HUDPosition.TopLeft;
        public bool HUDEnabled { get; set; } = true;
        public string Language { get; set; } = "zh";
        public Dictionary<string, object> CustomData { get; set; } = new();
    }
}