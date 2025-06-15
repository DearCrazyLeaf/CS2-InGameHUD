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

        public Config Config { get; set; } = new();

        public void OnConfigParsed(Config config)
        {
            // 更新配置
            Config = config;
            Console.WriteLine($"[InGameHUD] Configuration loaded successfully");
        }

        public override void Load(bool hotReload)
        {
            try
            {
                // 初始化 GameHUD API
                var capability = new PluginCapability<IGameHUDAPI>("gamehud:api");
                _api = capability.Get();

                if (_api == null)
                {
                    Console.WriteLine("[InGameHUD] GameHUDAPI not found!");
                    throw new Exception("GameHUDAPI not found!");
                }

                // 初始化数据库
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

                // 注册命令
                AddCommand("hud", "Toggle HUD visibility", CommandToggleHUD);
                AddCommand("hudpos", "Change HUD position (1-5)", CommandHUDPosition);

                // 注册事件
                RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
                RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
                RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
                RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);

                // 注册Tick更新
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
                            // 直接调用更新方法，它内部会处理状态检查
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

            // 添加状态检查：如果玩家死亡或观察者，则移除HUD并不再更新
            if (!player.PawnIsAlive || player.TeamNum == (int)CsTeam.Spectator)
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                return;
            }

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData)) return;

            try
            {
                // 更新自定义数据
                if (_dbConnected && _db != null)
                {
                    var customDataTask = _db.GetCustomData(steamId);
                    customDataTask.Wait();
                    playerData.CustomData = customDataTask.Result;
                }

                var (position, justify) = GetHUDPosition(playerData.HUDPosition);

                // 设置HUD参数
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

                // 构建HUD内容
                var hudBuilder = new StringBuilder();

                // 玩家名称
                hudBuilder.AppendLine($"你好！【{player.PlayerName}】");
                hudBuilder.AppendLine($"===================");

                // --- 新增：显示当前系统时间
                if (Config.ShowTime)
                {
                    hudBuilder.AppendLine($"当前时间: {DateTime.Now:HH:mm}");
                }

                // --- 新增：显示玩家 Ping
                //    根据 CSSharp API 文档，CCSPlayerController.Ping 返回当前 Ping 值
                if (Config.ShowPing)
                {
                    hudBuilder.AppendLine($"延迟: {player.Ping} ms");
                }

                // KDA统计
                if (Config.ShowKDA && player.ActionTrackingServices?.MatchStats != null)
                {
                    var stats = player.ActionTrackingServices.MatchStats;
                    hudBuilder.AppendLine($"KDR: {stats.Kills}/{stats.Deaths}/{stats.Assists}");
                }

                // 生命值
                if (Config.ShowHealth && player.PlayerPawn != null && player.PlayerPawn.Value != null)
                {
                    hudBuilder.AppendLine($"HP: {player.PlayerPawn.Value.Health}");
                }

                // 阵营显示
                if (Config.ShowTeams)
                {
                    string teamName = "SPEC";
                    if (player.TeamNum == 2)
                    {
                        teamName = "T";
                    }
                    else if (player.TeamNum == 3)
                    {
                        teamName = "CT";
                    }
                    hudBuilder.AppendLine($"阵营: {teamName}");
                }

                // --- 新增：显示玩家得分
                //    根据 CSSharp API 文档，MatchStats.Score 提供当前得分
                if (Config.ShowScore)
                {
                    hudBuilder.AppendLine($"得分: {player.Score}");
                }

                // -----------------------------------------------------------------添加自定义数据
                if (Config.CustomData.Credits.Enabled && _storeApi != null)
                {
                    try
                    {
                        Console.WriteLine($"[InGameHUD] Attempting to get credits for {player.PlayerName}");

                        // 确保玩家对象有效
                        if (player != null && player.IsValid)
                        {
                            int playerCredits = _storeApi.GetPlayerCredits(player);
                            Console.WriteLine($"[InGameHUD] Credits for {player.PlayerName}: {playerCredits}");

                            // 将积分数据存储到 playerData 中
                            playerData.CustomData["credits"] = playerCredits.ToString();

                            // 在 HUD 上显示积分
                            hudBuilder.AppendLine($"积分: {playerCredits}");
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

                // —— 新增：上次签到
                if (playerData.CustomData.ContainsKey("last_signin"))
                {
                    var lastSignInRaw = playerData.CustomData["last_signin"];
                    if (DateTime.TryParse(lastSignInRaw, out var lastSignInDt))
                    {
                        int daysAgo = (DateTime.Now.Date - lastSignInDt.Date).Days;
                        string display = daysAgo == 0 ? "今天" : $"{daysAgo}天前";
                        hudBuilder.AppendLine($"上次签到: {display}");
                    }
                    else
                    {
                        hudBuilder.AppendLine($"上次签到: 从未签到或数据异常");
                    }
                }

                if (playerData.CustomData.ContainsKey("playtime"))
                {
                    var playtime = int.Parse(playerData.CustomData["playtime"]);
                    var hours = playtime / 3600;
                    var minutes = (playtime % 3600) / 60;
                    hudBuilder.AppendLine($"游玩时长: {hours}小时{minutes}分钟");
                }
                // -----------------------------------------------------------------结束
                hudBuilder.AppendLine($"===================");
                    hudBuilder.AppendLine($"!hud开关面板");
                    hudBuilder.AppendLine($"!help查看帮助");
                    hudBuilder.AppendLine($"!store打开商店");
                    hudBuilder.AppendLine($"官方网站: hlymcn.cn");
                    hudBuilder.AppendLine($"===================");

                if (Config.ShowAnnouncementTitle)
                {
                    hudBuilder.AppendLine($"公告标题");
                }

                if (Config.ShowAnnouncement)
                {
                    hudBuilder.AppendLine($"公告内容");
                }

                // 显示HUD
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
                // 如果加载失败，使用默认设置
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

            // 检查玩家状态
            bool isInvalidState = !player.PawnIsAlive || player.TeamNum == (int)CsTeam.Spectator;
            if (isInvalidState)
            {
                player.PrintToChat($" {ChatColors.Red}当前状态无法启用HUD（死亡或观察者）");
                return;
            }

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData))
            {
                playerData = new PlayerData(steamId);
                _playerCache[steamId] = playerData;
            }

            playerData.HUDEnabled = !playerData.HUDEnabled;

            if (playerData.HUDEnabled)
            {
                UpdatePlayerHUDSync(player);
                player.PrintToChat($" {ChatColors.Green}HUD已启用");
            }
            else
            {
                _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                player.PrintToChat($" {ChatColors.Red}HUD已禁用");
            }

            SavePlayerSettingsSync(player);
        }

        private void CommandHUDPosition(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid) return;

            if (command.ArgCount != 1)
            {
                player.PrintToChat($" {ChatColors.Green}用法: !hudpos <1-5>");
                player.PrintToChat($" {ChatColors.Green}1:左上 2:右上 3:左下 4:右下 5:居中");
                return;
            }

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData))
            {
                playerData = new PlayerData(steamId);
                _playerCache[steamId] = playerData;
            }

            if (int.TryParse(command.ArgByIndex(1), out int pos) && pos >= 1 && pos <= 5)
            {
                playerData.HUDPosition = (HUDPosition)(pos - 1);

                if (playerData.HUDEnabled)
                {
                    _api?.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                    UpdatePlayerHUDSync(player);
                }

                player.PrintToChat($" {ChatColors.Green}HUD位置已更改");
                SavePlayerSettingsSync(player);
            }
            else
            {
                player.PrintToChat($" {ChatColors.Red}无效的位置! 请使用 1-5");
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
                // 如果切换到观察者队伍，立即移除HUD
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
                _storeApi = null; // 释放StoreAPI引用
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