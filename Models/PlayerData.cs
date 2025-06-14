namespace InGameHUD.Models
{
    public class PlayerData
    {
        public PlayerData(string steamId)
        {
            SteamID = steamId;
        }

        // �޸�Ϊֻ�����ԣ���Ϊ��������ֻͨ�����캯������
        public string SteamID { get; }
        public HUDPosition HUDPosition { get; set; } = HUDPosition.TopRight;
        public bool HUDEnabled { get; set; } = true;
        public Dictionary<string, string> CustomData { get; set; } = new();
    }
}