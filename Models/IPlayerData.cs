namespace InGameHUD.Models
{
    public interface IPlayerData
    {
        Task<bool> InitializeAsync();
        Task<bool> SavePlayerSettingsAsync(PlayerData playerData);
        Task<PlayerData> LoadPlayerSettingsAsync(string steamId);
        Task<Dictionary<string, string>> GetCustomDataAsync(string steamId);
    }
}