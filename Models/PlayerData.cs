namespace InGameHUD.Models
{
    public class PlayerData
    {
        public PlayerData(string steamId)
        {
            SteamID = steamId;
        }

        // 修改为只读属性，因为在主类中只通过构造函数设置
        public string SteamID { get; }
        public HUDPosition HUDPosition { get; set; } = HUDPosition.TopRight;
        public bool HUDEnabled { get; set; } = true;
        public Dictionary<string, string> CustomData { get; set; } = new();
    }
}