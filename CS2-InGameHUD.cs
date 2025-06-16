using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using InGameHUD.Models;
using InGameHUD.Managers;
using System.Text;
using System.Drawing;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CS2_GameHUDAPI;
using StoreApi;
using Microsoft.Extensions.Hosting.Internal;

namespace InGameHUD
{
    [MinimumApiVersion(318)]
    public class InGameHUD : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "InGame HUD";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "DearCrazyLeaf";
        public override string ModuleDescription => "Displays customizable HUD for players";
        private int _tickCounter = 0;
        private static IStoreApi? _storeApi;
        private static IGameHUDAPI? _api;
        private const byte MAIN_HUD_CHANNEL = 0;
        private Dictionary<string, PlayerData> _playerCache = new();
        private DatabaseManager? _db;
        private bool _dbConnected = false;
        private LanguageManager? _langManager;

        public Config Config { get; set; } = new();

        public void OnConfigParsed(Config config)
        {
            Config = config;
            Console.WriteLine($"[InGameHUD] Configuration loaded successfully");
        }

        public override void Load(bool hotReload)
        {
            try
            {
                _langManager = new LanguageManager(ModuleDirectory, Config.DefaultLanguage);

                var capability = new PluginCapability<IGameHUDAPI>("gamehud:api");
                _api = capability.Get();

                if (_api == null)
                {
                    Console.WriteLine("[InGameHUD] GameHUDAPI not found!");
                    throw new Exception("GameHUDAPI not found!");
                }

                _db = new DatabaseManager(Config);
                try
                {
                    var connectionTask = _db.TestConnection();
                    connectionTask.Wait();
                    var (success, error) = connectionTask.Result;
                    if (!success)
                    {
                        Console.WriteLine($"[InGameHUD] Database connection failed: {error}");
                        _dbConnected = false;
                    }
                    else
                    {
                        Console.WriteLine("[InGameHUD] Database connected successfully!");
                        _dbConnected = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Database initialization error: {ex.Message}");
                    _dbConnected = false;
                }

                AddCommand("hud", "Toggle HUD visibility", CommandToggleHUD);
                AddCommand("hudpos", "Change HUD position (1-5)", CommandHUDPosition);

                RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
                RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
                RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
                RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);

                RegisterListener<Listeners.OnTick>(() =>
                {
                    if (++_tickCounter % 320 != 0) return;

                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (player == null || !player.IsValid || player.IsBot)
                            continue;

                        var steamId = player.SteamID.ToString();

                        if (_playerCache.TryGetValue(steamId, out var playerData) && playerData.HUDEnabled)
                        {
                            UpdatePlayerHUDSync(player);
                        }
                    }
                });

                Console.WriteLine("[InGameHUD] Plugin loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading plugin: {ex.Message}");
                throw;
            }
        }

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            base.OnAllPluginsLoaded(hotReload);

            try
            {
                Console.WriteLine("[InGameHUD] Attempting to load StoreAPI...");
                _storeApi = IStoreApi.Capability.Get();

                if (_storeApi != null)
                {
                    Console.WriteLine("[InGameHUD] StoreAPI loaded successfully!");
                }
                else
                {
                    Console.WriteLine("[InGameHUD] StoreAPI not found. Credits will not be shown.");
                    Config.CustomData.Credits.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading StoreAPI: {ex.Message}");
                Config.CustomData.Credits.Enabled = false;
            }
        }

        private void UpdatePlayerHUDSync(CCSPlayerController player)
        {
            if (player == null || !player.IsValid) return;

            if (!player.PawnIsAlive || player.TeamNum == (int)CsTeam.Spectator)
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                return;
            }

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData)) return;

            try
            {
                if (_dbConnected && _db != null)
                {
                    var customDataTask = _db.GetCustomData(steamId);
                    customDataTask.Wait();
                    playerData.CustomData = customDataTask.Result;
                }

                var (position, justify) = GetHUDPosition(playerData.HUDPosition);
                string lang = playerData.Language;

                _api?.Native_GameHUD_SetParams(
                    player,
                    MAIN_HUD_CHANNEL,
                    position,
                    Color.FromName(Config.TextColor),
                    Config.FontSize,
                    Config.FontName,
                    Config.Scale,
                    justify,
                    PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER,
                    PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE,
                    Config.BackgroundScale,
                    Config.BackgroundOpacity
                );

                var hudBuilder = new StringBuilder();

                hudBuilder.AppendLine(_langManager.GetPhrase("hud.greeting", lang, player.PlayerName));
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.separator", lang));

                if (Config.ShowTime)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.current_time", lang, DateTime.Now.ToString("HH:mm")));
                }

                if (Config.ShowPing)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.ping", lang, player.Ping));
                }

                if (Config.ShowKDA && player.ActionTrackingServices?.MatchStats != null)
                {
                    var stats = player.ActionTrackingServices.MatchStats;
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.kda", lang, stats.Kills, stats.Deaths, stats.Assists));
                }

                if (Config.ShowHealth && player.PlayerPawn != null && player.PlayerPawn.Value != null)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.health", lang, player.PlayerPawn.Value.Health));
                }

                if (Config.ShowTeams)
                {
                    string teamKey = "hud.team_spec";
                    if (player.TeamNum == 2)
                    {
                        teamKey = "hud.team_t";
                    }
                    else if (player.TeamNum == 3)
                    {
                        teamKey = "hud.team_ct";
                    }
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.team", lang, _langManager.GetPhrase(teamKey, lang)));
                }

                if (Config.ShowScore)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.score", lang, player.Score));
                }

                // -----------------------------------------------------------------添加自定义数据
                if (Config.CustomData.Credits.Enabled && _storeApi != null)
                {
                    try
                    {
                        Console.WriteLine($"[InGameHUD] Attempting to get credits for {player.PlayerName}");

                        if (player != null && player.IsValid)
                        {
                            int playerCredits = _storeApi.GetPlayerCredits(player);
                            Console.WriteLine($"[InGameHUD] Credits for {player.PlayerName}: {playerCredits}");

                            playerData.CustomData["credits"] = playerCredits.ToString();

                            hudBuilder.AppendLine(_langManager.GetPhrase("hud.credits", lang, playerCredits));
                        }
                        else
                        {
                            Console.WriteLine("[InGameHUD] Player invalid when trying to get credits");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error getting player credits: {ex.Message}");
                        Console.WriteLine($"[InGameHUD] Stack trace: {ex.StackTrace}");
                    }
                }

                if (playerData.CustomData.ContainsKey("last_signin"))
                {
                    var lastSignInRaw = playerData.CustomData["last_signin"];
                    if (DateTime.TryParse(lastSignInRaw, out var lastSignInDt))
                    {
                        int daysAgo = (DateTime.Now.Date - lastSignInDt.Date).Days;
                        string display = daysAgo == 0
                            ? _langManager.GetPhrase("hud.today", lang)
                            : _langManager.GetPhrase("hud.days_ago", lang, daysAgo);
                        hudBuilder.AppendLine(_langManager.GetPhrase("hud.last_signin", lang, display));
                    }
                    else
                    {
                        hudBuilder.AppendLine(_langManager.GetPhrase("hud.last_signin", lang,
                            _langManager.GetPhrase("hud.never_signed", lang)));
                    }
                }

                if (playerData.CustomData.ContainsKey("playtime"))
                {
                    var playtime = int.Parse(playerData.CustomData["playtime"]);
                    var hours = playtime / 3600;
                    var minutes = (playtime % 3600) / 60;
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.playtime", lang, hours, minutes));
                }
                // -----------------------------------------------------------------结束
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.separator_bottom", lang));
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.hint_toggle", lang));
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.hint_help", lang));
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.hint_store", lang));
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.hint_website", lang));
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.separator_final", lang));

                if (Config.ShowAnnouncementTitle)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.announcement_title", lang));
                }

                if (Config.ShowAnnouncement)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.announcement_content", lang));
                }

                _api?.Native_GameHUD_ShowPermanent(player, MAIN_HUD_CHANNEL, hudBuilder.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error updating HUD for {player.PlayerName}: {ex.Message}");
            }
        }

        private void LoadPlayerSettingsSync(CCSPlayerController player)
        {
            if (!_dbConnected || _db == null || player == null || !player.IsValid) return;

            try
            {
                var steamId = player.SteamID.ToString();
                var settingsTask = _db.LoadPlayerSettings(steamId);
                settingsTask.Wait();
                _playerCache[steamId] = settingsTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading settings for {player.PlayerName}: {ex.Message}");
                if (player != null && player.IsValid)
                {
                    _playerCache[player.SteamID.ToString()] = new PlayerData(player.SteamID.ToString());
                }
            }
        }

        private void SavePlayerSettingsSync(CCSPlayerController player)
        {
            if (!_dbConnected || _db == null || player == null || !player.IsValid) return;

            try
            {
                var steamId = player.SteamID.ToString();
                if (_playerCache.TryGetValue(steamId, out var playerData))
                {
                    _db.SavePlayerSettings(playerData).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error saving settings for {player.PlayerName}: {ex.Message}");
            }
        }

        private void CommandToggleHUD(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid) return;

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData))
            {
                playerData = new PlayerData(steamId);
                _playerCache[steamId] = playerData;
            }

            string lang = playerData.Language;

            bool isInvalidState = !player.PawnIsAlive || player.TeamNum == (int)CsTeam.Spectator;
            if (isInvalidState)
            {
                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.invalid_state", lang)}");
                return;
            }

            playerData.HUDEnabled = !playerData.HUDEnabled;

            if (playerData.HUDEnabled)
            {
                UpdatePlayerHUDSync(player);
                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.enabled", lang)}");
            }
            else
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.disabled", lang)}");
            }

            SavePlayerSettingsSync(player);
        }

        private void CommandHUDPosition(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid) return;

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData))
            {
                playerData = new PlayerData(steamId);
                _playerCache[steamId] = playerData;
            }

            string lang = playerData.Language;

            if (command.ArgCount != 1)
            {
                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.position_usage", lang)}");
                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.position_help", lang)}");
                return;
            }

            if (int.TryParse(command.ArgByIndex(1), out int pos) && pos >= 1 && pos <= 5)
            {
                playerData.HUDPosition = (HUDPosition)(pos - 1);

                if (playerData.HUDEnabled)
                {
                    _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                    UpdatePlayerHUDSync(player);
                }

                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.position_changed", lang)}");
                SavePlayerSettingsSync(player);
            }
            else
            {
                player.PrintToChat($" {_langManager.GetColoredPhrase("hud.position_invalid", lang)}");
            }
        }

        [GameEventHandler]
        public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            LoadPlayerSettingsSync(player);
            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            var steamId = player.SteamID.ToString();
            if (_playerCache.ContainsKey(steamId))
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                SavePlayerSettingsSync(player);
                _playerCache.Remove(steamId);
            }

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player != null && player.IsValid && !player.IsBot)
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
            }
            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player != null && player.IsValid && !player.IsBot)
            {
                if (@event.Team == (byte)CsTeam.Spectator)
                {
                    _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                }
            }
            return HookResult.Continue;
        }

        private (Vector, PointWorldTextJustifyHorizontal_t) GetHUDPosition(HUDPosition position)
        {
            return position switch
            {
                HUDPosition.TopLeft => (new Vector(25, 90, 70), PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT),
                HUDPosition.TopRight => (new Vector(65, 10, 70), PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT),
                HUDPosition.BottomLeft => (new Vector(65, 90, 70), PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT),
                HUDPosition.BottomRight => (new Vector(25, 10, 70), PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT),
                HUDPosition.Center => (new Vector(50, 50, 70), PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER),
                _ => (new Vector(65, 10, 70), PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT)
            };
        }

        public override void Unload(bool hotReload)
        {
            try
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player != null && player.IsValid)
                    {
                        _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                    }
                }

                _playerCache.Clear();
                _api = null;
                _storeApi = null;
                _db = null;
                Console.WriteLine("[InGameHUD] Plugin unloaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error during unload: {ex.Message}");
            }
        }
    }
}