using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

namespace HelmoBots
{
    [MinimumApiVersion(80)]
    public class SRCON : BasePlugin, IPluginConfig<HelmoBotsConfig>
    {
        public override string ModuleName => "HelmoBots";
        public override string ModuleAuthor => "sn0wLow";
        public override string ModuleVersion => "0.0.1";
        public override string ModuleDescription => "Some features for CS2 Bots";

        public HelmoBotsConfig Config { get; set; } = null!;

        public void OnConfigParsed(HelmoBotsConfig config)
        {
            Config = config;
        }

        public override void Load(bool hotReload)
        {
            if (Config.IsEnabled)
            {
                RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
                RegisterEventHandler<EventItemPurchase>(OnItemPurchase, HookMode.Post);
            }
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            var player = @event.Userid;

            if (player is null || !player.IsValidPlayer(true) || !player.IsBot)
            {
                return HookResult.Continue;
            }

            if (player.PlayerPawn.Value!.WeaponServices is null)
            {
                return HookResult.Continue;
            }

            var rules = GetGameRules()!;
            var random = new Random();

            SetupBotEquipment(player, rules);

            if (random.Next(4) < 3)
            {
                // If the Bot died last round or it's the first round of either half,
                // remove the default pistol and replace it with a USP-S.
                // Otherwise, do not change it since the default pistol might have been picked up.
                if (!player.PawnIsAlive || IsPistolRound())
                {

                    AddTimer(0.1f, () =>
                    {
                        var botWeapons = player.PlayerPawn.Value!.WeaponServices.MyWeapons;

                        foreach (var weapon in botWeapons)
                        {

                            if (weapon?.Value?.DesignerName == "weapon_hkp2000")
                            {
                                player.RemoveItemByDesignerName("weapon_hkp2000");
                                player.GiveNamedItem("weapon_usp_silencer");
                                return;

                            }
                        }
                    });
                }

            }

            return HookResult.Continue;
        }

        private void SetupBotEquipment(CCSPlayerController player, CCSGameRules rules)
        {
            var random = new Random();
            var moneyService = player.InGameMoneyServices;


            if (moneyService is null)
            {
                return;
            }


            var itemService = player.PlayerPawn!.Value!.ItemServices;

            if (itemService is null)
            {
                return;
            }

            var account = moneyService.Account;

            if (IsPistolRound())
            {
                if (player.IsCT())
                {
                    if (HasAnyCTADefuseKit())
                    {
                        if (account >= 650)
                        {
                            BuyKevlarNoHelmet(player, moneyService);
                        }
                    }
                    else
                    {
                        if (account >= 400)
                        {
                            BuyDefuser(player, itemService, moneyService);
                        }
                    }
                }
                else
                {
                    if (account >= 650)
                    {
                        BuyKevlarNoHelmet(player, moneyService);
                    }
                }
            }
            else
            {
                if (player.IsCT())
                {
                    if (account >= 3000 || (account >= 400 && !HasAnyCTADefuseKit()))
                    {
                        BuyDefuser(player, itemService, moneyService);
                    }
                }
            }
        }

        public HookResult OnItemPurchase(EventItemPurchase @event, GameEventInfo info)
        {
            var player = @event.Userid;

            if (player is null || !player.IsValidPlayer(true) || !player.IsBot)
            {
                return HookResult.Continue;
            }

            var random = new Random();

            if (random.Next(4) == 0)
            {
                return HookResult.Continue;
            }

            if (@event.Weapon == "weapon_m4a1")
            {
                player.RemoveItemByDesignerName("weapon_m4a1");
                player.GiveNamedItem("weapon_m4a1_silencer");
            }
            else if (@event.Weapon == "weapon_aug")
            {
                player.RemoveItemByDesignerName("weapon_aug");

                if (random.Next(2) == 0)
                {
                    player.GiveNamedItem("weapon_m4a1");
                }
                else
                {
                    player.GiveNamedItem("weapon_m4a1_silencer");
                }
            }
            else if (@event.Weapon == "weapon_mac10" || @event.Weapon == "weapon_mp9")
            {
                player.RemoveItemByDesignerName(@event.Weapon);

                var randomIndex = random.Next(3);

                if (randomIndex == 0)
                {
                    player.GiveNamedItem("weapon_mp5sd");
                }
                else if (randomIndex == 1)
                {
                    player.GiveNamedItem("weapon_mp7");
                }
                else
                {
                    player.GiveNamedItem("weapon_ump45");
                }
            }
            else if (@event.Weapon == "weapon_nova")
            {
                player.RemoveItemByDesignerName("weapon_nova");

                if (player.IsCT())
                {
                    player.GiveNamedItem("weapon_mag7");
                }
                else
                {
                    player.GiveNamedItem("weapon_sawedoff");

                }

            }
            else if (@event.Weapon == "weapon_fiveseven" || @event.Weapon == "weapon_tec9")
            {
                player.RemoveItemByDesignerName(@event.Weapon);
                player.GiveNamedItem("weapon_cz75a");
            }
            else if (@event.Weapon == "weapon_deagle")
            {
                // 12.5% chance in total
                if (random.Next(6) == 0)
                {
                    player.RemoveItemByDesignerName("weapon_deagle");
                    player.GiveNamedItem("weapon_revolver");
                }

            }
            else if (@event.Weapon == "weapon_p250")
            {
                player.RemoveItemByDesignerName("weapon_p250");
                player.GiveNamedItem("weapon_elite");
            }

            return HookResult.Continue;
        }

        public static void BuyKevlarNoHelmet(CCSPlayerController player, CCSPlayerController_InGameMoneyServices moneyService)
        {
            player.GiveNamedItem("item_kevlar");
            moneyService.Account -= 650;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        public static void BuyDefuser(CCSPlayerController player, CPlayer_ItemServices itemServices,
            CCSPlayerController_InGameMoneyServices moneyService)
        {
            var itemService = new CCSPlayer_ItemServices(itemServices.Handle);
            itemService.HasDefuser = true;


            moneyService.Account -= 400;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        public static bool IsPistolRound()
        {
            // Apparently, sometimes after using mp_restartgame 1, 'RoundsPlayedThisPhase'
            // doesn't update, so 'TotalRoundsPlayed' has to be checked as a backup.
            var rules = GetGameRules();
            return rules?.RoundsPlayedThisPhase == 0 || rules?.TotalRoundsPlayed == 0;
        }

        public static bool HasAnyCTADefuseKit()
        {
            return Utilities.GetPlayers().Where(x => x.IsCT())
                .Where(x => x.PlayerPawn.Value?.ItemServices is not null)
                .Any(x => new CCSPlayer_ItemServices(x.PlayerPawn!.Value!.ItemServices!.Handle).HasDefuser);
        }

        public static CCSGameRules? GetGameRules()
        {
            return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                 .First()
                 .GameRules;
        }
    }
}
