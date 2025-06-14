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

        public HUDManager(IGameHUDAPI api, Config config, LanguageManager langManager)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _langManager = langManager ?? throw new ArgumentNullException(nameof(langManager));
        }

        public void UpdateHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }

            if (!playerData.HUDEnabled)
            {
                return;
            }

            if (player.PlayerPawn?.Value == null)
            {
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
                    var actionStats = player.ActionTrackingServices?.MatchStats;
                    if (actionStats != null)
                    {
                        hudBuilder.AppendLine(_langManager.GetPhrase("hud.kda", lang,
                            actionStats.Kills,
                            actionStats.Deaths,
                            actionStats.Assists));
                    }
                }

                if (_config.ShowWeapon && player.PawnIsAlive)
                {
                    var weaponServices = pawn.WeaponServices;
                    var activeWeapon = weaponServices?.ActiveWeapon?.Value;
                    if (activeWeapon != null)
                    {
                        var clip = activeWeapon.Clip1;
                        var reserve = activeWeapon.ReserveAmmo.Length > 0 ? activeWeapon.ReserveAmmo[0] : 0;

                        hudBuilder.AppendLine(_langManager.GetPhrase("hud.weapon", lang,
                            activeWeapon.DesignerName,
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

                _api.Native_GameHUD_SetParams(
                    player,
                    MAIN_HUD_CHANNEL,
                    position,
                    _config.TextColor,
                    _config.FontSize,              // 从配置中获取字体大小
                    _config.FontName,              // 从配置中获取字体名称
                    _config.Scale,                 // 从配置中获取缩放
                    PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT,
                    PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_BOTTOM,
                    PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE,
                    _config.BackgroundOpacity,     // 从配置中获取背景透明度
                    _config.BackgroundScale        // 从配置中获取背景缩放
                );

                _api.Native_GameHUD_Show(player, MAIN_HUD_CHANNEL, hudBuilder.ToString());

                Console.WriteLine($"[InGameHUD] Updated HUD for player {player.PlayerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error updating HUD: {ex.Message}");
            }
        }

        public void EnableHUD(CCSPlayerController player, PlayerData playerData)
        {
            if (player == null || !player.IsValid || playerData == null)
                return;

            try
            {
                playerData.HUDEnabled = true;
                UpdateHUD(player, playerData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error enabling HUD for {player.PlayerName}: {ex.Message}");
            }
        }

        public void DisableHUD(CCSPlayerController player)
        {
            if (player == null || !player.IsValid)
                return;

            try
            {
                _api.Native_GameHUD_Remove(player, MAIN_HUD_CHANNEL);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InGameHUD] Error disabling HUD for {player.PlayerName}: {ex.Message}");
            }
        }

        private Vector GetHUDPosition(HUDPosition position)
        {
            return position switch
            {
                HUDPosition.TopLeft => new Vector(5, 5, 95),       // 左上角
                HUDPosition.TopRight => new Vector(25, 5, 95),     // 右上角
                HUDPosition.BottomLeft => new Vector(5, 25, 95),   // 左下角
                HUDPosition.BottomRight => new Vector(25, 25, 95), // 右下角
                HUDPosition.Center => new Vector(15, 15, 95),      // 中间
                _ => new Vector(25, 5, 95)                         // 默认右上角
            };
        }
    }
}