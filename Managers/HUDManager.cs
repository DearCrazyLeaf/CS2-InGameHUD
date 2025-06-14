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
        private readonly InGameHUD _plugin;
        private const byte MAIN_HUD_CHANNEL = 1;

        public HUDManager(IGameHUDAPI api, Config config, LanguageManager langManager)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _langManager = langManager ?? throw new ArgumentNullException(nameof(langManager));
        }

        public void UpdateHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (!playerData.HUDEnabled || player == null || !player.IsValid || player.PlayerPawn?.Value == null)
            {
                DisableHUD(player);
                return;
            }

            try
            {
                var hudBuilder = new StringBuilder();
                var lang = playerData.Language;
                var pawn = player.PlayerPawn.Value;

                hudBuilder.AppendLine(_langManager.GetPhrase("hud.player_name", lang, player.PlayerName));

                if (_config.ShowKDA)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.kda", lang,
                        player.ActionTrackingServices?.MatchStats.Kills ?? 0,
                        player.ActionTrackingServices?.MatchStats.Deaths ?? 0,
                        player.ActionTrackingServices?.MatchStats.Assists ?? 0));
                }

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

                if (_config.CustomData.Credits.Enabled)
                {
                    hudBuilder.AppendLine(_langManager.GetPhrase("hud.credits", lang,
                        playerData.Credits));
                }

                var position = GetHUDPosition(playerData.HUDPosition);

                _api.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);

                _api.Native_GameHUD_SetParams(
                    player,
                    MAIN_HUD_CHANNEL,
                    position,
                    _config.TextColor,
                    35,  // 字体大小
                    "Arial Bold",
                    0.07f,  // 保持原始缩放
                    PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT,
                    PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP,
                    PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE,
                    0.0f,
                    0.0f
                );

                _api.Native_GameHUD_Show(player, MAIN_HUD_CHANNEL, hudBuilder.ToString(), 5.0f);

                _plugin.AddTimer(4.9f, () => RefreshHUD(player, playerData));

                Console.WriteLine($"[InGameHUD] Updated HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error updating HUD: {ex.Message}");
            }
        }

        private void RefreshHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (player.IsValid && playerData.HUDEnabled)
            {
                UpdateHUD(player, playerData);
            }
        }

        public void EnableHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (!player.IsValid) return;

            try
            {
                DisableHUD(player);
                playerData.HUDEnabled = true;
                UpdateHUD(player, playerData);
                Console.WriteLine($"[InGameHUD] Enabled HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error enabling HUD: {ex.Message}");
            }
        }

        public void DisableHUD(CCSPlayerController player)
        {
            if (!player.IsValid) return;

            try
            {
                _api.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
                Console.WriteLine($"[InGameHUD] Disabled HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error disabling HUD: {ex.Message}");
            }
        }

        private Vector GetHUDPosition(HUDPosition position)
        {
            return position switch
            {
                HUDPosition.TopLeft => new Vector(10, 10, 80),      // 左上角
                HUDPosition.TopRight => new Vector(90, 10, 80),     // 右上角
                HUDPosition.BottomLeft => new Vector(10, 90, 80),   // 左下角
                HUDPosition.BottomRight => new Vector(90, 90, 80),  // 右下角
                HUDPosition.Center => new Vector(50, 50, 80),       // 中间
                _ => new Vector(90, 10, 80)                         // 默认右上角
            };
        }
    }
}