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

namespace InGameHUD
{
    [MinimumApiVersion(318)]
    public class InGameHUD : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "InGame HUD";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "DearCrazyLeaf";
        public override string ModuleDescription => "Displays customizable HUD for players";

        private static IGameHUDAPI? _api;
        private const byte MAIN_HUD_CHANNEL = 0;
        private Dictionary<string, PlayerData> _playerCache = new();
        private DatabaseManager? _db;
        private bool _dbConnected = false;

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

                // 注册Tick更新
                RegisterListener<Listeners.OnTick>(() =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (player == null || !player.IsValid || player.IsBot) continue;

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

        private void UpdatePlayerHUDSync(CCSPlayerController player)
        {
            if (player == null || !player.IsValid) return;

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
                hudBuilder.AppendLine($"玩家: {player.PlayerName}");

                // KDA统计
                if (Config.ShowKDA && player.ActionTrackingServices?.MatchStats != null)
                {
                    var stats = player.ActionTrackingServices.MatchStats;
                    hudBuilder.AppendLine($"战绩: {stats.Kills}/{stats.Deaths}/{stats.Assists}");
                }

                // 生命值
                if (Config.ShowHealth && player.PlayerPawn != null && player.PlayerPawn.Value != null)
                {
                    hudBuilder.AppendLine($"生命值: {player.PlayerPawn.Value.Health}");
                }

                // 阵营显示
                string teamName = "观察者";
                if (player.TeamNum == 2)
                {
                    teamName = "恐怖分子";
                }
                else if (player.TeamNum == 3)
                {
                    teamName = "反恐精英";
                }
                hudBuilder.AppendLine($"阵营: {teamName}");

                // 添加自定义数据
                if (playerData.CustomData.ContainsKey("credits"))
                {
                    hudBuilder.AppendLine($"积分: {playerData.CustomData["credits"]}");
                }

                if (playerData.CustomData.ContainsKey("playtime"))
                {
                    var playtime = int.Parse(playerData.CustomData["playtime"]);
                    var hours = playtime / 3600;
                    var minutes = (playtime % 3600) / 60;
                    hudBuilder.AppendLine($"游玩时长: {hours}小时{minutes}分钟");
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