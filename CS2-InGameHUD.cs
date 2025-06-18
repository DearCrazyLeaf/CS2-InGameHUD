using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using InGameHUD.Models;
using InGameHUD.Managers;
using System.Text;
using System.Drawing;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CS2_GameHUDAPI;
using StoreApi;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Modules.Cvars;

namespace InGameHUD
{
    [MinimumApiVersion(318)]
    public class InGameHUD : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "InGame HUD";
        public override string ModuleVersion => "1.3.0";
        public override string ModuleAuthor => "DearCrazyLeaf";
        public override string ModuleDescription => "Displays customizable HUD for players";
        private int _tickCounter = 0;
        private static IStoreApi? _storeApi;
        private static IGameHUDAPI? _api;
        private const byte MAIN_HUD_CHANNEL = 0;
        private Dictionary<string, PlayerData> _playerCache = new();
        private DatabaseManager? _db;
        private bool _dbConnected = false;
        private readonly IStringLocalizer<InGameHUD> _localizer;
        public Config Config { get; set; } = new();
        public InGameHUD(IStringLocalizer<InGameHUD> localizer)
        {
            _localizer = localizer;
        }

        public void OnConfigParsed(Config config)
        {
            Config = config;
            Console.WriteLine($"[InGameHUD] Configuration loaded successfully");
        }

        public override void Load(bool hotReload)
        {
            try
            {
                _db = new DatabaseManager(Config, ModuleDirectory);
                try
                {
                    var initializeTask = _db.InitializeAsync();
                    initializeTask.Wait();
                    var (success, error) = initializeTask.Result;
                    if (!success)
                    {
                        Console.WriteLine($"[InGameHUD] Database initialization failed: {error}");
                        _dbConnected = false;
                    }
                    else
                    {
                        Console.WriteLine("[InGameHUD] Database initialized successfully!");
                        _dbConnected = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Database initialization error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[InGameHUD] Inner exception: {ex.InnerException.Message}");
                    }
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
                Console.WriteLine("[InGameHUD] Attempting to load GameHUDAPI...");
                PluginCapability<IGameHUDAPI> CapabilityCP = new("gamehud:api");
                _api = IGameHUDAPI.Capability.Get();

                if (_api != null)
                {
                    Console.WriteLine("[InGameHUD] GameHUDAPI loaded successfully!");
                }
                else
                {
                    Console.WriteLine("[InGameHUD] GameHUDAPI not found. HUD features will not be available.");
                }
            }
            catch (Exception ex)
            {
                _api = null;
                Console.WriteLine($"[InGameHUD] Error loading GameHUDAPI: {ex.Message}");
            }

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

        private bool IsInWarmup()
        {
            try
            {
                CCSGameRulesProxy? gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
                if (gameRulesProxy == null || gameRulesProxy.GameRules == null)
                    return false;

                return gameRulesProxy.GameRules.WarmupPeriod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error checking warmup status: {ex.Message}");
                return false;
            }
        }

        private (int minutes, int seconds) GetMapTimeRemaining()
        {
            try
            {
                CCSGameRulesProxy? gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
                if (gameRulesProxy == null || gameRulesProxy.GameRules == null)
                    return (0, 0);

                CCSGameRules gameRules = gameRulesProxy.GameRules;

                int timelimit = (int)ConVar.Find("mp_timelimit").GetPrimitiveValue<float>() * 60;

                if (timelimit == 0)
                    return (0, 0);

                int gameStart = (int)gameRules.GameStartTime;
                int currentTime = (int)Server.CurrentTime;
                int timeleft = timelimit - (currentTime - gameStart);

                if (timeleft < 0)
                    timeleft = 0;

                int minutes = timeleft / 60;
                int seconds = timeleft % 60;

                return (minutes, seconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error getting map time: {ex.Message}");
                Console.WriteLine($"[InGameHUD] {ex.StackTrace}");
                return (0, 0);
            }
        }

        private (int currentRound, int maxRounds) GetMapRoundInfo()
        {
            try
            {
                CCSGameRulesProxy? gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
                if (gameRulesProxy == null || gameRulesProxy.GameRules == null)
                    return (0, 0);

                CCSGameRules gameRules = gameRulesProxy.GameRules;

                int maxRounds = (int)ConVar.Find("mp_maxrounds").GetPrimitiveValue<int>();

                int currentRound = gameRules.TotalRoundsPlayed + 1;

                int roundsLeft = maxRounds - currentRound + 1;

                if (roundsLeft < 0)
                    roundsLeft = 0;

                return (currentRound, maxRounds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error getting round info: {ex.Message}");
                Console.WriteLine($"[InGameHUD] {ex.StackTrace}");
                return (0, 0);
            }
        }

        private string GetMapTimeDisplay()
        {
            if (IsInWarmup())
            {
                return _localizer["hud.warmuptime"];
            }

            if (Config.MapTimeMode == MapTimeMode.TimeLimit)
            {
                var (minutes, seconds) = GetMapTimeRemaining();
                return _localizer["hud.map_time_remaining", minutes, seconds];
            }
            else
            {
                var (currentRound, maxRounds) = GetMapRoundInfo();
                return _localizer["hud.map_round_info", currentRound, maxRounds];
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

                // 获取原始HUD位置和水平对齐方式
                var (basePosition, justifyHorizontal) = GetHUDPosition(playerData.HUDPosition);

                // ====== 修改部分开始: 计算自适应位置调整 ======
                // 计算自适应调整因子
                // 基准值
                const float BASE_FONT_SIZE = 50.0f;  // 默认字体大小
                const float BASE_SCALE = 0.1f;       // 默认缩放值

                // 计算字体大小和缩放的比例
                float fontFactor = Config.FontSize / BASE_FONT_SIZE;
                float scaleFactor = BASE_SCALE / Config.Scale;

                // 调整因子，当字体变大或缩放变小时，调整因子>1，位置值需要除以更大的数
                float adjustmentFactor = fontFactor * scaleFactor;

                // 调整后的位置向量 (Z轴保持不变)
                Vector adjustedPosition = new Vector(
                    basePosition.X / adjustmentFactor,  // X轴调整
                    basePosition.Y / adjustmentFactor,  // Y轴调整
                    basePosition.Z                      // Z轴保持不变
                );
                // ====== 修改部分结束 ======

                // 使用调整后的位置而不是原始位置
                _api?.Native_GameHUD_SetParams(
                    player,
                    MAIN_HUD_CHANNEL,
                    adjustedPosition, // 使用调整后的位置
                    Color.FromName(Config.TextColor),
                    Config.FontSize,
                    Config.FontName,
                    Config.Scale,
                    justifyHorizontal,
                    PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER,
                    PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE,
                    Config.BackgroundScale,
                    Config.BackgroundOpacity
                );

                var hudBuilder = new StringBuilder();

                hudBuilder.AppendLine(_localizer["hud.greeting", player.PlayerName]);
                hudBuilder.AppendLine(_localizer["hud.separator"]);

                if (Config.ShowMapTime)
                {
                    hudBuilder.AppendLine(GetMapTimeDisplay());
                }

                if (Config.ShowTime)
                {
                    hudBuilder.AppendLine(_localizer["hud.current_time", DateTime.Now.ToString("HH:mm")]);
                }

                if (Config.ShowPing)
                {
                    hudBuilder.AppendLine(_localizer["hud.ping", player.Ping]);
                }

                if (Config.ShowKDA && player.ActionTrackingServices?.MatchStats != null)
                {
                    var stats = player.ActionTrackingServices.MatchStats;
                    hudBuilder.AppendLine(_localizer["hud.kda", stats.Kills, stats.Deaths, stats.Assists]);
                }

                if (Config.ShowHealth && player.PlayerPawn != null && player.PlayerPawn.Value != null)
                {
                    hudBuilder.AppendLine(_localizer["hud.health", player.PlayerPawn.Value.Health]);
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
                    hudBuilder.AppendLine(_localizer["hud.team", _localizer[teamKey]]);
                }

                if (Config.ShowScore)
                {
                    hudBuilder.AppendLine(_localizer["hud.score", player.Score]);
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

                            hudBuilder.AppendLine(_localizer["hud.credits", playerCredits]);
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
                        int daysAgo = Math.Max(0, (DateTime.Now.Date - lastSignInDt.Date).Days);
                        string display = daysAgo == 0
                            ? _localizer["hud.today"]
                            : _localizer["hud.days_ago", daysAgo];
                        hudBuilder.AppendLine(_localizer["hud.last_signin", display]);
                    }
                    else
                    {
                        hudBuilder.AppendLine(_localizer["hud.last_signin", _localizer["hud.never_signed"]]);
                    }
                }

                if (playerData.CustomData.ContainsKey("playtime"))
                {
                    var playtime = int.Parse(playerData.CustomData["playtime"]);
                    var hours = playtime / 3600;
                    var minutes = (playtime % 3600) / 60;
                    hudBuilder.AppendLine(_localizer["hud.playtime", hours, minutes]);
                }
                // -----------------------------------------------------------------结束
                hudBuilder.AppendLine(_localizer["hud.separator_bottom"]);
                hudBuilder.AppendLine(_localizer["hud.hint_toggle"]);
                hudBuilder.AppendLine(_localizer["hud.hint_help"]);
                hudBuilder.AppendLine(_localizer["hud.hint_store"]);
                hudBuilder.AppendLine(_localizer["hud.hint_website"]);
                hudBuilder.AppendLine(_localizer["hud.separator_final"]);

                if (Config.ShowAnnouncementTitle)
                {
                    hudBuilder.AppendLine(_localizer["hud.announcement_title"]);
                }

                if (Config.ShowAnnouncement)
                {
                    hudBuilder.AppendLine(_localizer["hud.announcement_content"]);
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
            if (player == null || !player.IsValid) return;

            var steamId = player.SteamID.ToString();

            try
            {
                if (_db != null && _dbConnected)
                {
                    var settingsTask = _db.LoadPlayerSettings(steamId);
                    settingsTask.Wait();
                    _playerCache[steamId] = settingsTask.Result;
                    Console.WriteLine($"[InGameHUD] Settings loaded for {player.PlayerName}");
                }
                else
                {
                    _playerCache[steamId] = new PlayerData(steamId);
                    Console.WriteLine($"[InGameHUD] Default settings created for {player.PlayerName} (no database)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error loading settings for {player.PlayerName}: {ex.Message}");
                _playerCache[steamId] = new PlayerData(steamId);
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

            bool isInvalidState = !player.PawnIsAlive || player.TeamNum == (int)CsTeam.Spectator;
            if (isInvalidState)
            {
                player.PrintToChat(_localizer["hud.invalid_state"]);
                return;
            }

            playerData.HUDEnabled = !playerData.HUDEnabled;

            if (playerData.HUDEnabled)
            {
                UpdatePlayerHUDSync(player);
                player.PrintToChat(_localizer["hud.enabled"]);
            }
            else
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                player.PrintToChat(_localizer["hud.disabled"]);
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

            string posArg = command.ArgByIndex(1);
            if (int.TryParse(posArg, out int posInput) && posInput >= 1 && posInput <= 5)
            {
                HUDPosition newPosition = posInput switch
                {
                    1 => HUDPosition.TopLeft,
                    2 => HUDPosition.TopRight,
                    3 => HUDPosition.BottomLeft,
                    4 => HUDPosition.BottomRight,
                    5 => HUDPosition.Center,
                    _ => playerData.HUDPosition
                };

                playerData.HUDPosition = newPosition;

                if (playerData.HUDEnabled)
                {
                    _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                    UpdatePlayerHUDSync(player);
                }

                player.PrintToChat(_localizer["hud.position_changed"]);
                SavePlayerSettingsSync(player);
            }
            else
            {
                player.PrintToChat(_localizer["hud.position_invalid"]);
                player.PrintToChat(_localizer["hud.position_usage"]);
                player.PrintToChat(_localizer["hud.position_help"]);
            }
        }

        [GameEventHandler]
        public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            var steamId = player.SteamID.ToString();

            try
            {
                LoadPlayerSettingsSync(player);

                if (!_playerCache.ContainsKey(steamId))
                {
                    _playerCache[steamId] = new PlayerData(steamId);
                    Console.WriteLine($"[InGameHUD] Created default settings for {player.PlayerName}");
                }

                Server.NextFrame(() =>
                {
                    try
                    {
                        if (player != null && player.IsValid && !player.IsBot &&
                            player.PawnIsAlive && player.TeamNum != (int)CsTeam.Spectator)
                        {
                            if (_playerCache.TryGetValue(steamId, out var playerData) &&
                                playerData.HUDEnabled && _api != null)
                            {
                                Console.WriteLine($"[InGameHUD] Auto-showing HUD for {player.PlayerName}");
                                UpdatePlayerHUDSync(player);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error in delayed HUD display: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error in OnPlayerConnect: {ex.Message}");
                _playerCache[steamId] = new PlayerData(steamId);
            }

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
                _playerCache.Remove(steamId);
                Console.WriteLine($"[InGameHUD] Removed player data for {player.PlayerName} on disconnect");
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
            PositionConfig baseConfig;
            PointWorldTextJustifyHorizontal_t justification;

            switch (position)
            {
                case HUDPosition.TopLeft:
                    baseConfig = Config.Positions.TopLeft;
                    justification = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
                    break;
                case HUDPosition.TopRight:
                    baseConfig = Config.Positions.TopRight;
                    justification = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT;
                    break;
                case HUDPosition.BottomLeft:
                    baseConfig = Config.Positions.BottomLeft;
                    justification = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
                    break;
                case HUDPosition.BottomRight:
                    baseConfig = Config.Positions.BottomRight;
                    justification = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT;
                    break;
                case HUDPosition.Center:
                    baseConfig = Config.Positions.Center;
                    justification = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER;
                    break;
                default:
                    baseConfig = Config.Positions.TopRight;
                    justification = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT;
                    break;
            }

            return (new Vector(baseConfig.XOffset, baseConfig.YOffset, baseConfig.ZDistance), justification);
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