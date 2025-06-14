using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Events;
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
                var capability = new PluginCapability<IGameHUDAPI>("gamehud:api");
                _api = IGameHUDAPI.Capability.Get();

                if (_api == null)
                {
                    throw new Exception("GameHUDAPI not found!");
                }

                _playerCache = new Dictionary<string, PlayerData>();

                _langManager = new LanguageManager(ModuleDirectory, Config.DefaultLanguage);
                _dbManager = new DatabaseManager(ModuleDirectory, Config);
                _hudManager = new HUDManager(_api, Config, _langManager);

                AddCommand("hud", "Toggle HUD visibility", CommandToggleHUD);
                AddCommand("hudpos", "Change HUD position (1-5)", CommandHUDPosition);
                AddCommand("hudlang", "Change HUD language (zh/en)", CommandChangeLanguage);

                RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
                RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

                AddTimer(Config.HUDUpdateInterval, UpdateHUD, TimerFlags.REPEAT);

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
                try
                {
                    var playerData = await _dbManager.LoadPlayerData(steamId);
                    _playerCache[steamId] = playerData;

                    if (playerData.HUDEnabled)
                    {
                        Server.NextFrame(() => _hudManager.UpdateHUD(player, playerData));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InGameHUD] Error loading player data for {steamId}: {ex.Message}");
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
                    try
                    {
                        await _dbManager.SavePlayerData(steamId, playerData);
                        _playerCache.Remove(steamId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InGameHUD] Error saving player data for {steamId}: {ex.Message}");
                    }
                });
            }

            return HookResult.Continue;
        }

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
                    Server.NextFrame(() => _hudManager.UpdateHUD(player, playerData));
                    player.PrintToChat($" {ChatColors.Green}{_langManager.GetPhrase("hud.enabled", playerData.Language)}");
                }
                else
                {
                    _hudManager.DisableHUD(player);
                    player.PrintToChat($" {ChatColors.Red}{_langManager.GetPhrase("hud.disabled", playerData.Language)}");
                }

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
                _hudManager.DisableHUD(player);

                playerData.HUDPosition = (HUDPosition)(pos - 1);

                if (playerData.HUDEnabled)
                {
                    _hudManager.EnableHUD(player, playerData);
                }

                Task.Run(async () =>
                {
                    await _dbManager.SavePlayerData(steamId, playerData);
                });

                player.PrintToChat($" {ChatColors.Green}{_langManager.GetPhrase("hud.position_changed", playerData.Language)}");
            }
            else
            {
                player.PrintToChat($" {ChatColors.Red}{_langManager.GetPhrase("hud.position_invalid", playerData.Language)}");
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

        public override void Unload(bool hotReload)
        {
            try
            {
                foreach (var kvp in _playerCache)
                {
                    _dbManager?.SavePlayerData(kvp.Key, kvp.Value).Wait();
                }

                _playerCache.Clear();
                Console.WriteLine("[InGameHUD] Plugin unloaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error during unload: {ex.Message}");
            }
        }
    }
}