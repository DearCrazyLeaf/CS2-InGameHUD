using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text;
using CS2_GameHUDAPI;
using InGameHUD.Models;
using System.Drawing;

namespace InGameHUD.Managers
{
    public class HUDManager
    {
        private readonly IGameHUDAPI _api;
        private readonly Config _config;
        private readonly LanguageManager _langManager;
        private const byte MAIN_HUD_CHANNEL = 1;
        // 添加字典来跟踪每个玩家的HUD状态
        private readonly Dictionary<string, bool> _activeHuds = new();

        public HUDManager(IGameHUDAPI api, Config config, LanguageManager langManager)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _langManager = langManager ?? throw new ArgumentNullException(nameof(langManager));
        }

        public void UpdateHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (!playerData.HUDEnabled || !player.IsValid || player.PlayerPawn?.Value == null)
            {
                DisableHUD(player);
                return;
            }

            var steamId = player.SteamID.ToString();

            try
            {
                var hudBuilder = new StringBuilder();
                var lang = playerData.Language;
                var pawn = player.PlayerPawn.Value;

                // 构建HUD内容
                hudBuilder.AppendLine(_langManager.GetPhrase("hud.player_name", lang, player.PlayerName));

                // KDA信息
                if (_config.ShowKDA)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.kda", lang,
                        player.ActionTrackingServices?.MatchStats.Kills ?? 0,
                        player.ActionTrackingServices?.MatchStats.Deaths ?? 0,
                        player.ActionTrackingServices?.MatchStats.Assists ?? 0));
                }

                // 武器信息
                if (_config.ShowWeapon && player.PawnIsAlive)
                {
                    var weaponServices = pawn.WeaponServices;
                    if (weaponServices?.ActiveWeapon?.Value != null)
                    {
                        var weapon = weaponServices.ActiveWeapon.Value;
                        var clip = weapon.Clip1;
                        var reserve = weapon.ReserveAmmo.Length > 0 ? weapon.ReserveAmmo[0] : 0;

                        hudBuilder.AppendLine(_langManager.GetPhrase("hud.weapon", lang,
                            weapon.DesignerName,
                            clip,
                            reserve));
                    }
                }

                // 自定义数据
                if (_config.CustomData.Credits.Enabled)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.credits", lang,
                        playerData.Credits));
                }

                if (_config.CustomData.Playtime.Enabled && playerData.Playtime != TimeSpan.Zero)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.playtime", lang,
                        (int)playerData.Playtime.TotalHours,
                        playerData.Playtime.Minutes));
                }

                var position = GetHUDPosition(playerData.HUDPosition);

                // 如果HUD不存在或位置改变，重新创建
                if (!_activeHuds.ContainsKey(steamId))
                {
                    // 设置HUD参数
                    _api.Native_GameHUD_SetParams(
                        player,
                        MAIN_HUD_CHANNEL,
                        position,
                        _config.TextColor,
                        50,
                        "Arial Bold",
                        0.07f,
                        PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT,
                        PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP,
                        PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE,
                        0.0f,
                        0.0f
                    );
                    _activeHuds[steamId] = true;
                }

                // 更新HUD内容
                _api.Native_GameHUD_ShowPermanent(player, MAIN_HUD_CHANNEL, hudBuilder.ToString());
                Console.WriteLine($"[InGameHUD] Updated HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error updating HUD for player {player.PlayerName}: {ex.Message}");
                DisableHUD(player);
            }
        }

        public void EnableHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (!player.IsValid) return;

            var steamId = player.SteamID.ToString();
            try
            {
                DisableHUD(player); // 先清除可能存在的HUD
                playerData.HUDEnabled = true;
                UpdateHUD(player, playerData);
                Console.WriteLine($"[InGameHUD] Enabled HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error enabling HUD for player {player.PlayerName}: {ex.Message}");
            }
        }

        public void DisableHUD(CCSPlayerController player)
        {
            if (!player.IsValid) return;

            var steamId = player.SteamID.ToString();
            try
            {
                _api.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                _activeHuds.Remove(steamId);
                Console.WriteLine($"[InGameHUD] Disabled HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error disabling HUD for player {player.PlayerName}: {ex.Message}");
            }
        }

        private Vector GetHUDPosition(HUDPosition position)
        {
            return position switch
            {
                HUDPosition.TopLeft => new Vector(10, 10, 0),
                HUDPosition.BottomLeft => new Vector(10, 90, 0),
                HUDPosition.TopRight => new Vector(90, 10, 0),
                HUDPosition.BottomRight => new Vector(90, 90, 0),
                HUDPosition.Center => new Vector(50, 50, 0),
                _ => new Vector(10, 10, 0)
            };
        }
    }
}