using System;
using System.Collections.Generic;
using CounterStrikeSharp.API.Modules.Entities;

namespace InGameHUD.Models
{
    public class PlayerData
    {
        public PlayerData(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
                throw new ArgumentNullException(nameof(steamId));

            SteamID = steamId;
            LastUpdated = DateTime.UtcNow;
        }

        public string SteamID { get; private set; }
        public int Credits { get; set; }
        public TimeSpan Playtime { get; set; }
        public HUDPosition HUDPosition { get; set; } = HUDPosition.TopRight;
        public bool HUDEnabled { get; set; } = true;
        public string Language { get; set; } = "zh";
        public Dictionary<string, object> CustomData { get; } = new();
        public DateTime LastUpdated { get; set; }

        public void UpdateLastUpdated()
        {
            LastUpdated = DateTime.UtcNow;
        }

        public PlayerData Clone()
        {
            var clone = new PlayerData(SteamID)
            {
                Credits = Credits,
                Playtime = Playtime,
                HUDPosition = HUDPosition,
                HUDEnabled = HUDEnabled,
                Language = Language,
                LastUpdated = LastUpdated
            };

            foreach (var kvp in CustomData)
            {
                clone.CustomData[kvp.Key] = kvp.Value;
            }

            return clone;
        }
    }
}