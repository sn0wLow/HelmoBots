using CounterStrikeSharp.API.Core;

namespace HelmoBots
{
    public static class PlayerControllerExtensions
    {
        public static bool IsCT(this CCSPlayerController playerController)
        {
            return playerController.Team ==
            CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist;
        }

        public static bool IsT(this CCSPlayerController playerController)
        {
            return playerController.Team ==
            CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist;
        }


        public static bool IsValidPlayer(this CCSPlayerController playerController, bool allowBots = false)
        {
            return (playerController is not null && playerController.IsValid &&
            playerController.PlayerPawn is not null && playerController.PlayerPawn.IsValid &&
            playerController.PlayerPawn.Value is not null && 
            (!playerController.IsBot || allowBots) && !playerController.IsHLTV);
        }
    }
}
