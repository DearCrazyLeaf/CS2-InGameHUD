using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes.Registration;  // 添加事件注册引用
using CounterStrikeSharp.API.Modules.Events;  // 添加事件模块引用
using CS2_GameHUDAPI;
using InGameHUD.Managers;
using InGameHUD.Models;
using System.Text.Json;

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
        private DatabaseManager _dbManager = null!;
        private HUDManager _hudManager = null!;
        private LanguageManager _langManager = null!;
        public Config Config { get; set; } = new Config();
        private Dictionary<string, PlayerData> _playerCache = null!;

        public void OnConfigParsed(Config config)
        {
            Config = config;
        }

        public override void Load(bool hotReload)
        {
            Console.WriteLine($"[InGameHUD] ModuleDirectory: {ModuleDirectory}");
            Console.WriteLine($"[InGameHUD] ModuleDirectory exists: {Directory.Exists(ModuleDirectory)}");
            Console.WriteLine($"[InGameHUD] ModuleDirectory full path: {Path.GetFullPath(ModuleDirectory)}");

            if (string.IsNullOrEmpty(ModuleDirectory))
                throw new InvalidOperationException("ModuleDirectory is not initialized");

            try
            {
                // 使用 Capability 系统获取 API
                var capability = new PluginCapability<IGameHUDAPI>("gamehud:api");
                _api = IGameHUDAPI.Capability.Get();

                if (_api == null)
                {
                    throw new Exception("GameHUDAPI not found!");
                }

                _playerCache = new Dictionary<string, PlayerData>();

                // 初始化管理器
                _langManager = new LanguageManager(ModuleDirectory, Config.DefaultLanguage);
                _dbManager = new DatabaseManager(ModuleDirectory, Config);
                _hudManager = new HUDManager(_api, Config, _langManager);

                // 注册命令
                AddCommand("hud", "Toggle HUD visibility", CommandToggleHUD);
                AddCommand("hudpos", "Change HUD position (1-5)", CommandHUDPosition);
                AddCommand("hudlang", "Change HUD language (zh/en)", CommandChangeLanguage);

                // 注册事件
                RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
                RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

                // 创建定时器
                AddTimer(Config.HUDUpdateInterval, UpdateHUD, TimerFlags.REPEAT);

                // 热重载时重新加载所有玩家的HUD
                if (hotReload)
                {
                    ReloadAllPlayersHUD();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing plugin: {ex.Message}");
                throw;
            }
        }


        private async void ReloadAllPlayersHUD()
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p != null && p.IsValid && !p.IsBot))
            {
                var steamId = player.SteamID.ToString();
                var playerData = await _dbManager.LoadPlayerData(steamId);
                _playerCache[steamId] = playerData;
                if (playerData.HUDEnabled)
                {
                    _hudManager.EnableHUD(player, playerData);
                }
            }
        }

        [GameEventHandler]
        public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            var steamId = player.SteamID.ToString();
            Task.Run(async () =>
            {
                var playerData = await _dbManager.LoadPlayerData(steamId);
                _playerCache[steamId] = playerData;
                if (playerData.HUDEnabled)
                {
                    _hudManager.EnableHUD(player, playerData);
                }
            });

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            var steamId = player.SteamID.ToString();
            if (_playerCache.TryGetValue(steamId, out var playerData))
            {
                _hudManager.DisableHUD(player);
                Task.Run(async () =>
                {
                    await SavePlayerDataWithRetry(steamId, playerData);
                    _playerCache.Remove(steamId);
                });
            }

            return HookResult.Continue;
        }

        // 修改UpdateHUD方法为同步方法
        private void UpdateHUD()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player == null || !player.IsValid ||
                    player.PlayerPawn.Value == null ||
                    player.PlayerPawn.Value.IsValid == false ||
                    player.Connected != PlayerConnectedState.PlayerConnected ||
                    player.IsBot)
                    continue;

                var steamId = player.SteamID.ToString();
                if (_playerCache.TryGetValue(steamId, out var playerData) && playerData.HUDEnabled)
                {
                    Server.NextFrame(() => _hudManager.UpdateHUD(player, playerData));
                }
            }
        }

        private void CommandToggleHUD(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid) return;

            var steamId = player.SteamID.ToString();
            if (_playerCache.TryGetValue(steamId, out var playerData))
            {
                playerData.HUDEnabled = !playerData.HUDEnabled;

                if (playerData.HUDEnabled)
                {
                    _hudManager.EnableHUD(player, playerData);
                    // 立即更新一次 HUD
                    Server.NextFrame(() => _hudManager.UpdateHUD(player, playerData));
                    player.PrintToChat($" {ChatColors.Green}{_langManager.GetPhrase("hud.enabled", playerData.Language)}");
                }
                else
                {
                    _hudManager.DisableHUD(player);
                    player.PrintToChat($" {ChatColors.Red}{_langManager.GetPhrase("hud.disabled", playerData.Language)}");
                }

                // 立即保存数据
                _dbManager.SavePlayerData(steamId, playerData).Wait();
            }
        }

        private void CommandHUDPosition(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid) return;

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData)) return;

            if (command.ArgCount != 1)
            {
                player.PrintToChat($" {ChatColors.Green}{_langManager.GetPhrase("hud.position_usage", playerData.Language)}");
                player.PrintToChat($" {ChatColors.Green}{_langManager.GetPhrase("hud.position_help", playerData.Language)}");
                return;
            }

            if (int.TryParse(command.ArgByIndex(1), out int pos) && pos >= 1 && pos <= 5)
            {
                playerData.HUDPosition = (HUDPosition)(pos - 1);
                if (playerData.HUDEnabled)
                {
                    _hudManager.DisableHUD(player);  // 先禁用当前HUD
                    _hudManager.EnableHUD(player, playerData);  // 在新位置重新启用
                    Server.NextFrame(() => _hudManager.UpdateHUD(player, playerData));
                }

                // 立即保存数据
                _dbManager.SavePlayerData(steamId, playerData).Wait();
            }
        }

        private void CommandChangeLanguage(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid) return;

            var steamId = player.SteamID.ToString();
            if (!_playerCache.TryGetValue(steamId, out var playerData)) return;

            if (command.ArgCount != 1)
            {
                player.PrintToChat($" {ChatColors.Green}Usage: !hudlang <zh/en>");
                return;
            }

            var language = command.ArgByIndex(1).ToLower();
            if (language != "zh" && language != "en")
            {
                player.PrintToChat($" {ChatColors.Red}Supported languages: zh, en");
                return;
            }

            playerData.Language = language;
            if (playerData.HUDEnabled)
            {
                _hudManager.UpdateHUD(player, playerData);
            }

            Task.Run(async () =>
            {
                await _dbManager.SavePlayerData(steamId, playerData);
            });
        }

        private async Task SavePlayerDataWithRetry(string steamId, PlayerData playerData, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    playerData.UpdateLastUpdated();
                    await _dbManager.SavePlayerData(steamId, playerData);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Attempt {i + 1} failed to save player data for {steamId}: {ex.Message}");
                    if (i == maxRetries - 1)
                    {
                        Console.WriteLine($"[InGameHUD] Failed to save player data after {maxRetries} attempts");
                    }
                    await Task.Delay(100 * (i + 1)); // 递增延迟重试
                }
            }
        }
    }
}