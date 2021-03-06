﻿using System;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using System.Linq;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects;
using Trinity.Framework.Objects.Enums;
using Trinity.Framework.Reference;
using Trinity.Settings;
using Trinity.Settings.ItemList;
using Zeta.Bot;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;


namespace Trinity.Components.Combat
{
    public interface ILootProvider
    {
        bool ShouldDrop(TrinityItem item, ItemEvaluationType scheduledAction);

        bool ShouldStash(TrinityItem item);

        bool ShouldSalvage(TrinityItem item);

        bool ShouldPickup(TrinityItem item);

        bool ShouldSell(TrinityItem item);

        bool IsBackpackFull { get; }

        Vector2 FindBackpackSlot(bool twoSlot);
    }

    public class DefaultLootProvider : ILootProvider
    {
        public static int FreeBagSlots { get; set; } = 4;
        public static int FreeBagSlotsInTown { get; set; } = 30;

        public bool ShouldPickup(TrinityItem item)
        {
            if (item == null || !item.IsValid)
            {
                Core.Logger.Debug($"Not a valid item {item?.InternalName} Sno={item?.ActorSnoId} GbId={item?.GameBalanceId}");
                return false;
            }

            if (item.RawItemType == RawItemType.CosmeticPet)
            {
                Core.Logger.Log($"Pet found! - Picking it up {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }
            if (item.RawItemType == RawItemType.CosmeticWings)
            {
                Core.Logger.Log($"Wings found! - Picking it up {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }
            if (item.RawItemType == RawItemType.CosmeticPennant)
            {
                Core.Logger.Log($"Wings found! - Picking it up {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }
            if (item.RawItemType == RawItemType.CosmeticPortraitFrame)
            {
                Core.Logger.Log($"Portrait found! - Picking it up {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }

            //if (item.InternalNameLowerCase.Contains("cosmetic"))
            //    return true;

            if (Core.Settings.Items.DisableLootingInCombat && TrinityCombat.IsInCombat && item.Distance > 8f)
                return false;

            if (Core.Settings.Items.DontPickupInTown && Core.Player.IsInTown && !item.IsItemAssigned)
                return false;

            // 451002, //sir williams - 451002 (TentacleBear_C_Unique_Cosmetic_02)
            // portrait - 410998 (Cosmetic_Portrait_Frame_1)

            //if (Core.Settings.Items.InCombatLooting != SettingMode.Enabled && Combat.IsInCombat)
            //{
            //    if (Core.Settings.Items.InCombatLooting == SettingMode.Disabled)
            //        return false;

            //    if (Core.Settings.Items.InCombatLooting == SettingMode.Selective)
            //    {
            //        var pickupQuality = TypeConversions.GetPickupItemQuality(item.TrinityItemQuality);
            //        if (!Core.Settings.Items.InCombatLootQualities.HasFlag(pickupQuality))
            //            return false;
            //    }
            //}

            if (Core.Settings.Items.DontWalkToLowQuality && item.Distance > 8f && item.IsLowQuality && !item.IsCraftingReagent)
                return false;

            if (item.IsAncient && Core.Settings.ItemList.AlwaysStashAncients)
                return true;

            if (item.IsPrimalAncient && Core.Settings.ItemList.AlwaysStashPrimalAncients)
                return true;

            if (item.TrinityItemType == TrinityItemType.ConsumableAddSockets)
                return true;

            if (item.RawItemType == RawItemType.Book && Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.Lore))
                return true;

            if (item.RawItemType == RawItemType.Junk && Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.CultistPage))
                return true;

            if (item.GameBalanceId == GameData.ItemGameBalanceIds.DeathsBreath)
                return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.DeathsBreath);

            if (item.ActorSnoId == (int)SNOActor.A1_BlackMushroom)
                return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.RottenMushroom);

            if (item.GameBalanceId == GameData.ItemGameBalanceIds.ArcaneDust)
                return true; //Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.ArcaneDust);

            if (item.GameBalanceId == GameData.ItemGameBalanceIds.VeiledCrystal)
                return true; // Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.VeiledCrystals);

            if (item.GameBalanceId == GameData.ItemGameBalanceIds.ReusableParts)
                return true; //Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.ReusableParts);

            if (item.GameBalanceId == GameData.ItemGameBalanceIds.ForgottenSoul)
                return true;

            if (GameData.HerdingMatsSnoIds.Contains(item.ActorSnoId))
                return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.StaffOfHeardingParts);

            if (GameData.TransmogTable.Contains(item.GameBalanceId) || item.InternalName.StartsWith("Transmog") || item.ActorSnoId == 110952) //Rakanishu's Blade
            {
                Core.Logger.Log($"Transmog found! - Picking it up for its visual goodness {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }

            if (item.TrinityItemType == TrinityItemType.TieredLootrunKey || item.TrinityItemType == TrinityItemType.LootRunKey)
                return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.TieredLootrunKey);

            if (item.TrinityItemType == TrinityItemType.HealthPotion && item.ItemQualityLevel >= ItemQuality.Legendary)
                return true;

            if (item.TrinityItemType == TrinityItemType.InfernalKey || item.TrinityItemType == TrinityItemType.PortalDevice)
                return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.KeywardenIngredients);

            if (item.TrinityItemType == TrinityItemType.UberReagent)
                return true;

            if (item.TrinityItemType == TrinityItemType.HoradricRelic && Core.Player.BloodShards < Core.Player.MaxBloodShards)
                return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.BloodShards);

            if (item.TrinityItemType == TrinityItemType.ProgressionGlobe)
                return true;

            if (item.TrinityItemType == TrinityItemType.CraftingMaterial)
                return true;

            switch (item.RawItemType)
            {
                case RawItemType.CraftingPlan:
                case RawItemType.CraftingPlan_Jeweler:
                case RawItemType.CraftingPlan_Smith:
                case RawItemType.CraftingPlanLegendary_Smith:
                case RawItemType.CraftingPlan_Mystic:
                case RawItemType.CraftingPlan_MysticTransmog:
                    return Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.CraftingPlans);
            }

            if (item.TrinityItemType == TrinityItemType.CraftingPlan && item.ItemQualityLevel >= ItemQuality.Legendary && Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.CraftingPlans))
            {
                return true;
            }

            if (item.Type == TrinityObjectType.BloodShard)
                return true;

            if (item.ItemType == ItemType.LegendaryGem)
                return true;

            if (item.ItemQualityLevel >= ItemQuality.Legendary)
                return true;

            switch (item.TrinityItemBaseType)
            {
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponRange:
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                case TrinityItemBaseType.Jewelry:
                case TrinityItemBaseType.FollowerItem:

                    if (item.TrinityItemQuality == TrinityItemQuality.Inferior)
                        return Core.Settings.Items.PickupQualities.HasFlag(PickupItemQualities.Grey);

                    if (item.TrinityItemQuality == TrinityItemQuality.Common)
                        return Core.Settings.Items.PickupQualities.HasFlag(PickupItemQualities.White);

                    if (item.TrinityItemQuality == TrinityItemQuality.Magic)
                        return Core.Settings.Items.PickupQualities.HasFlag(PickupItemQualities.Blue);

                    if (item.TrinityItemQuality == TrinityItemQuality.Rare)
                        return Core.Settings.Items.PickupQualities.HasFlag(PickupItemQualities.Yellow);

                    return false;

                case TrinityItemBaseType.Gem:

                    if ((int)item.GemQuality < Core.Settings.Items.GemLevel ||
                        (item.TrinityItemType == TrinityItemType.Ruby && !Core.Settings.Items.GemTypes.HasFlag(TrinityGemType.Ruby)) ||
                        (item.TrinityItemType == TrinityItemType.Emerald && !Core.Settings.Items.GemTypes.HasFlag(TrinityGemType.Emerald)) ||
                        (item.TrinityItemType == TrinityItemType.Amethyst && !Core.Settings.Items.GemTypes.HasFlag(TrinityGemType.Amethyst)) ||
                        (item.TrinityItemType == TrinityItemType.Topaz && !Core.Settings.Items.GemTypes.HasFlag(TrinityGemType.Topaz)) ||
                        (item.TrinityItemType == TrinityItemType.Diamond && !Core.Settings.Items.GemTypes.HasFlag(TrinityGemType.Diamond)))
                    {
                        return false;
                    }
                    break;

                case TrinityItemBaseType.Misc:
                case TrinityItemBaseType.HealthGlobe:
                    return true;

                case TrinityItemBaseType.ProgressionGlobe:
                    return true;

                default:
                    return false;
            }

            return true;
        }

        public bool ShouldDrop(TrinityItem item, ItemEvaluationType scheduledAction)
        {
            if (item.IsProtected() || item.IsAccountBound)
                return false;

            if (item.IsGem || item.IsCraftingReagent || item.TrinityItemType == TrinityItemType.CraftingPlan)
                return false;

            if (!item.IsUnidentified && (item.IsPotion || item.RawItemType == RawItemType.GeneralUtility || item.IsMiscItem))
                return false;

            if (item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                if (Core.Settings.Items.DropInTownMode == DropInTownOption.All)
                {
                    Core.Logger.Verbose($"Should Drop {item.Name} - Setting='{Core.Settings.Items.DropInTownMode}'");
                    return true;
                }

                switch (scheduledAction)
                {
                    case ItemEvaluationType.Keep:

                        if (Core.Settings.Items.DropInTownMode == DropInTownOption.Keep)
                        {
                            Core.Logger.Verbose($"Should Drop {item.Name} - Setting='{Core.Settings.Items.DropInTownMode}' and item is scheduled for {scheduledAction}");
                            return true;
                        }

                        break;

                    case ItemEvaluationType.Salvage:
                    case ItemEvaluationType.Sell:

                        if (Core.Settings.Items.DropInTownMode == DropInTownOption.Vendor)
                        {
                            Core.Logger.Verbose($"Should Drop {item.Name} - Setting='{Core.Settings.Items.DropInTownMode}' and item is scheduled for {scheduledAction}");
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        public bool ShouldStash(TrinityItem item)
        {
            if (Core.ProfileSettings.Options.ShouldKeepInBackpack(item.ActorSnoId))
            {
                Core.Logger.Debug($"Profile Setting Keep in Backpack - Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return false;
            }

            if (item.IsAncient && Core.Settings.ItemList.AlwaysStashAncients)
            {
                Core.Logger.Debug($"Stashing due to ItemList setting - Always stash ancients. (col={item.InventoryColumn}, row={item.InventoryRow}). Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return true;
            }

            if (item.IsPrimalAncient && Core.Settings.ItemList.AlwaysStashPrimalAncients)
            {
                Core.Logger.Debug($"Stashing due to ItemList setting - Always stash primal ancients. (col={item.InventoryColumn}, row={item.InventoryRow}). Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return true;
            }

            // 451002, //sir williams - 451002 (TentacleBear_C_Unique_Cosmetic_02)
            // portrait - 410998 (Cosmetic_Portrait_Frame_1)
            if (item.InternalNameLowerCase.Contains("cosmetic"))
                return true;

            if (item.IsProtected())
            {
                Core.Logger.Debug($"Not stashing due to item being in a protected slot (col={item.InventoryColumn}, row={item.InventoryRow}). Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return false;
            }

            if (Core.Player.IsInventoryLockedForGreaterRift || !Core.Settings.Items.KeepLegendaryUnid && Core.Player.ParticipatingInTieredLootRun)
            {
                Core.Logger.Debug($"Not stashing due to inventory locked, keep unidentified setting or participating in loot run. Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return false;
            }

            if (item.RawItemType == RawItemType.TreasureBag)
                return Core.Settings.Items.StashTreasureBags;

            if (GameData.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return true;

            if (GameData.TransmogTable.Contains(item.GameBalanceId) || item.InternalName.StartsWith("Transmog") || item.ActorSnoId == 110952) //Rakanishu's Blade
            {
                var setting = Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.TransmogWhites);
                Core.Logger.Log($"Transmog found! - Stash Setting={setting} {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return setting;
            }

            if (item.RawItemType == RawItemType.CosmeticPet)
            {
                Core.Logger.Log($"Pet found! - Stashing it {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }
            if (item.RawItemType == RawItemType.CosmeticWings)
            {
                Core.Logger.Log($"Wings found! - Stashing it  {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }
            if (item.RawItemType == RawItemType.CosmeticPennant)
            {
                Core.Logger.Log($"Wings found! - Stashing it  {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }
            if (item.RawItemType == RawItemType.CosmeticPortraitFrame)
            {
                Core.Logger.Log($"Portrait found! - Stashing it {item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId}");
                return true;
            }

            if (Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.RottenMushroom) && item.ActorSnoId == (int)SNOActor.A1_BlackMushroom)
            {
                Core.Logger.Log($"Rotten Mushroom found! - Stash Setting. Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return true;
            }

            if (GameData.HerdingMatsSnoIds.Contains(item.ActorSnoId))
            {
                Core.Logger.Log($"Staff of Herding Mat found! - Stash Setting. Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                return true;
            }

            // Now look for Misc items we might want to keep
            TrinityItemType tItemType = item.TrinityItemType;
            TrinityItemBaseType tBaseType = item.TrinityItemBaseType;

            // Keep any high gems placed in backpack while levelling, so we can socket items with them.
            if (item.IsGem && item.GemQuality >= GemQuality.Marquise && ZetaDia.Me.Level < 70)
            {
                return false;
            }

            var isHandledLegendaryType = item.ItemBaseType == ItemBaseType.Armor || item.ItemBaseType == ItemBaseType.Jewelry || item.ItemBaseType == ItemBaseType.Weapon || item.IsPotion;
            if (item.ItemQualityLevel >= ItemQuality.Legendary && isHandledLegendaryType)
            {
                /* Stash items that hasn't had their legendary power extracted if the CubeExtractOption is set, if we have no currency for extracting it we can assume it would have been salvaged instead.
                if (Core.Settings.KanaisCube.ExtractLegendaryPowers != CubeExtractOption.None && !Core.Inventory.Currency.HasCurrency(TransmuteRecipe.ExtractLegendaryPower))
                {
                    Item legendaryItem = Legendary.GetItem(item);

                    if (legendaryItem != null && legendaryItem.LegendaryAffix != "" &&
                        Core.Inventory.Stash.ByActorSno(legendaryItem.Id) != null &&
                        !ZetaDia.Storage.PlayerDataManager.ActivePlayerData.KanaisPowersExtractedActorSnoIds.Contains(
                            legendaryItem.Id))
                    {
                        Core.Logger.Debug($"Stashing due to legendary power hasn't been extracted yet.");
                        return true;
                    }
                }*/

                if (Core.Settings.Items.LegendaryMode == LegendaryMode.Ignore)
                {
                    Core.Logger.Log($"TRASHING: Ignore Legendary. Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                    return false;
                }

                if (Core.Settings.Items.LegendaryMode == LegendaryMode.AlwaysStash)
                {
                    Core.Logger.Log($"STASHING: Always Stash Legendary Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                    return true;
                }

                if (Core.Settings.Items.LegendaryMode == LegendaryMode.StashAncients)
                {
                    if (item.IsAncient)
                    {
                        Core.Logger.Log($"STASHING: Only Stash Ancients Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                        return true;
                    }
                    Core.Logger.Log($"TRASHING: Only Stash Ancients Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                    return false;
                }

                if (Core.Settings.ItemList.AlwaysTrashNonAncients && !item.IsAncient)
                {
                    Core.Logger.Log($"TRASHING: ItemList Option - Always Sell/Salvage Non-Ancients Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                    return false;
                }
            }

            bool isEquipment = (tBaseType == TrinityItemBaseType.Armor ||
                tBaseType == TrinityItemBaseType.Jewelry ||
                tBaseType == TrinityItemBaseType.Offhand ||
                tBaseType == TrinityItemBaseType.WeaponOneHand ||
                tBaseType == TrinityItemBaseType.WeaponRange ||
                tBaseType == TrinityItemBaseType.WeaponTwoHand);

            if (item.ItemType == ItemType.KeystoneFragment)
            {
                return true;
            }

            if (item.TrinityItemType == TrinityItemType.HoradricCache)
                return false;

            if (item.IsUnidentified)
            {
                Core.Logger.Log("{0} [{1}] = (autokeep unidentified items)", item.Name, item.InternalName);
                return true;
            }

            if (tItemType == TrinityItemType.StaffOfHerding)
            {
                Core.Logger.Log(LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.CraftingMaterial || item.IsCraftingReagent)
            {
                Core.Logger.Log(LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.Emerald || tItemType == TrinityItemType.Amethyst || tItemType == TrinityItemType.Topaz || tItemType == TrinityItemType.Ruby || tItemType == TrinityItemType.Diamond)
            {
                Core.Logger.Log(LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", item.Name, item.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.CraftTome)
            {
                Core.Logger.Log(LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", item.Name, item.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.InfernalKey)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep infernal key)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.HealthPotion)
            {
                var equippedPotion = Core.Player.EquippedHealthPotion;
                if (equippedPotion == null)
                {
                    Core.Logger.Debug("Potion being stashed because an equipped potion was not found.");
                    return true;
                }
                if (equippedPotion.AnnId == item.AnnId)
                {
                    Core.Logger.Debug($"{item.Name} [{item.InternalName}] [{tItemType}] = (dont stash equipped potion)");
                    return false;
                }
            }

            if (tItemType == TrinityItemType.CraftingPlan && item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep legendary plans)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.ConsumableAddSockets)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep Ramaladni's Gift)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.PortalDevice)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep Machines)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.UberReagent)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep Uber Reagents)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.TieredLootrunKey)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (ignoring Tiered Rift Keys)", item.Name, item.InternalName, tItemType);
                return false;
            }

            if (tItemType == TrinityItemType.CraftingPlan)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep plans)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (item.ItemQualityLevel <= ItemQuality.Superior && (isEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (trash whites)", item.Name, item.InternalName, tItemType);
                return false;
            }
            if (item.ItemQualityLevel >= ItemQuality.Magic1 && item.ItemQualityLevel <= ItemQuality.Magic3 && (isEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (trashing blues)", item.Name, item.InternalName, tItemType);
                return false;
            }

            if (item.ItemQualityLevel >= ItemQuality.Rare4 && item.ItemQualityLevel <= ItemQuality.Rare6 && (isEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (force salvage rare)", item.Name, item.InternalName, tItemType);
                return false;
            }

            if (item.ItemQualityLevel >= ItemQuality.Legendary && Core.Settings.Items.LegendaryMode == LegendaryMode.ItemList && (item.IsEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem || item.IsPotion))
            {
                var result = ItemListEvaluator.ShouldStashItem(item);
                Core.Logger.Log("{0} [{1}] [{2}] = {3}", item.Name, item.InternalName, tItemType, "ItemListCheck=" + (result ? "KEEP" : "TRASH"));

                return result;
            }

            if (item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                Core.Logger.Log("{0} [{1}] [{2}] = (autokeep legendaries)", item.Name, item.InternalName, tItemType);
                return true;
            }

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        public bool ShouldSalvage(TrinityItem item)
        {
            var reason = string.Empty;
            try
            {
                if (item.IsProtected())
                {
                    reason = "Protected";
                    return false;
                }

                if (item.IsCosmeticItem)
                {
                    reason = "Cosmetic";
                    return false;
                }

                if (item.IsUnidentified)
                {
                    reason = "Not Identified";
                    return false;
                }

                if (item.IsAncient && Core.Settings.ItemList.AlwaysStashAncients)
                {
                    reason = "ItemList Stash Ancients";
                    return false;
                }

                if (item.IsPrimalAncient && Core.Settings.ItemList.AlwaysStashPrimalAncients)
                {
                    reason = "ItemList Stash Primal Ancients";
                    return false;
                }

                if (Core.Player.IsInventoryLockedForGreaterRift || !Core.Settings.Items.KeepLegendaryUnid && Core.Player.ParticipatingInTieredLootRun)
                {
                    reason = "Rift Locked Inventory";
                    return false;
                }

                if (Core.ProfileSettings.Options.ShouldKeepInBackpack(item.ActorSnoId))
                {
                    reason = "Profile Setting Keep in Backpack";
                    return false;
                }

                if (!item.IsSalvageable)
                {
                    reason = "Not Salvagable";
                    return false;
                }

                if (GameData.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                {
                    reason = "Vanity Item";
                    return false;
                }

                if (item.ItemType == ItemType.KeystoneFragment)
                {
                    reason = "Rift Key";
                    return false;
                }

                if (item.TrinityItemType == TrinityItemType.HealthPotion)
                {
                    var equippedPotion = Core.Player.EquippedHealthPotion;
                    if (equippedPotion == null)
                    {
                        Core.Logger.Debug("Potion being kept because an equipped potion was not found.");
                        return false;
                    }
                    if (equippedPotion.AnnId == item.AnnId)
                    {
                        reason = "Equipped Potion";
                        return false;
                    }
                }

                if (!Core.Settings.Items.SpecialItems.HasFlag(SpecialItemTypes.TransmogWhites) && GameData.TransmogTable.Contains(item.GameBalanceId) || item.InternalName.StartsWith("Transmog") || item.ActorSnoId == 110952) //Rakanishu's Blade
                {
                    reason = "Transmog Setting";
                    return true;
                }

 

                reason = "Default";
                switch (item.TrinityItemType)
                {
                    case TrinityItemType.HealthPotion:
                        return true;
                }
                switch (item.TrinityItemBaseType)
                {
                    case TrinityItemBaseType.WeaponRange:
                    case TrinityItemBaseType.WeaponOneHand:
                    case TrinityItemBaseType.WeaponTwoHand:
                    case TrinityItemBaseType.Armor:
                    case TrinityItemBaseType.Offhand:
                    case TrinityItemBaseType.Jewelry:
                    case TrinityItemBaseType.FollowerItem:
                        return true;

                    case TrinityItemBaseType.Gem:
                    case TrinityItemBaseType.Misc:
                    case TrinityItemBaseType.Unknown:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Exception in TrinitySalvage Evaluation for {item.Name} ({item.ActorSnoId}) InternalName={item.InternalName} Quality={item.ItemQualityLevel} Ancient={item.IsAncient} Identified={!item.IsUnidentified} RawItemType={item.RawItemType} {ex}");
            }
            finally
            {
                Core.Logger.Debug($"Salvage Evaluation for: {item.Name} ({item.ActorSnoId}) Reason={reason} InternalName={item.InternalName} Quality={item.ItemQualityLevel} Ancient={item.IsAncient} Identified={!item.IsUnidentified} RawItemType={item.RawItemType}");
            }
            return false;
        }

        public bool ShouldSell(TrinityItem item)
        {
            var reason = string.Empty;
            try
            {
                if (item.IsProtected())
                {
                    reason = $"Protected Slot [col:{item.InventoryColumn}, row:{item.InventoryRow}]";
                    return false;
                }

                if (Core.ProfileSettings.Options.ShouldKeepInBackpack(item.ActorSnoId))
                {
                    reason = "Profile Setting Keep in Backpack";
                    return false;
                }

                if (item.IsAncient && Core.Settings.ItemList.AlwaysStashAncients)
                {
                    Core.Logger.Debug($"Not Selling due to ItemList setting - Always stash ancients. (col={item.InventoryColumn}, row={item.InventoryRow}). Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                    return false;
                }

                if (item.IsPrimalAncient && Core.Settings.ItemList.AlwaysStashPrimalAncients)
                {
                    Core.Logger.Debug($"Not Selling due to ItemList setting - Always stash primal ancients. (col={item.InventoryColumn}, row={item.InventoryRow}). Item={item.Name} InternalName={item.InternalName} Sno={item.ActorSnoId} GbId={item.GameBalanceId} RawItemType={item.RawItemType}");
                    return false;
                }

                if (item.IsCosmeticItem)
                {
                    return false;
                }

                //ActorId: 367009, Type: Item, Name: Griswold's Scribblings
                if (item.ActorSnoId == 367009)
                {
                    reason = "Special Case - Griswold's Scribblings";
                    return true;
                }

                if (item.ActorSnoId == 210432)
                {
                    reason = "Special Case - Never sell staff of cow";
                    return false;
                }

                if (item.IsUnidentified)
                {
                    reason = "Not Identified";
                    return false;
                }

                if (item.TrinityItemType == TrinityItemType.HealthPotion)
                {
                    var equippedPotion = Core.Player.EquippedHealthPotion;
                    if (equippedPotion == null)
                    {
                        Core.Logger.Debug($"Legendary Potion {item.Name} ({item.ActorSnoId}) being kept because an equipped potion was not found.");
                        return false;
                    }
                    if (equippedPotion.AnnId == item.AnnId)
                    {
                        reason = "Equipped Potion";
                        return false;
                    }
                }

                if (item.IsEquipment && item.RequiredLevel <= 1)
                {
                    reason = "Unable to salvage level 1 items";
                    return true;
                }

                if (Core.Player.IsInventoryLockedForGreaterRift || !Core.Settings.Items.KeepLegendaryUnid && Core.Player.ParticipatingInTieredLootRun)
                {
                    reason = "Rift Locked Inventory";
                    return false;
                }

                if (item.IsVendorBought)
                {
                    reason = "Unable to salvage vendor bought items";
                    return false;
                }

                if (item.IsGem && item.GemQuality >= GemQuality.Marquise && ZetaDia.Me.Level < 70)
                {
                    reason = "auto-keep high level gems";
                    return false;
                }

                if (GameData.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                {
                    reason = "Vantity item";
                    return false;
                }

                if (item.ItemType == ItemType.KeystoneFragment)
                {
                    reason = "Rift Key";
                    return false;
                }

                if (item.ItemType == ItemType.HoradricCache)
                {
                    reason = "HoradricCache";
                    return false;
                }

                reason = "Default";
                switch (item.TrinityItemBaseType)
                {
                    case TrinityItemBaseType.WeaponRange:
                    case TrinityItemBaseType.WeaponOneHand:
                    case TrinityItemBaseType.WeaponTwoHand:
                    case TrinityItemBaseType.Armor:
                    case TrinityItemBaseType.Offhand:
                    case TrinityItemBaseType.Jewelry:
                    case TrinityItemBaseType.FollowerItem:
                        return true;

                    case TrinityItemBaseType.Gem:
                    case TrinityItemBaseType.Misc:
                        if (item.TrinityItemType == TrinityItemType.CraftingPlan)
                            return true;
                        if (item.TrinityItemType == TrinityItemType.CraftingMaterial)
                            return true;
                        return false;

                    case TrinityItemBaseType.Unknown:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Exception in TrinitySell Evaluation for {item.Name} ({item.ActorSnoId}) InternalName={item.InternalName} Quality={item.ItemQualityLevel} Ancient={item.IsAncient} Identified={!item.IsUnidentified} RawItemType={item.RawItemType} {ex}");
            }
            finally
            {
                Core.Logger.Debug($"Sell Evaluation for: {item.Name} ({item.ActorSnoId}) Reason={reason} InternalName={item.InternalName} Quality={item.ItemQualityLevel} Ancient={item.IsAncient} Identified={!item.IsUnidentified} RawItemType={item.RawItemType}");
            }
            return false;
        }

        private static int _lastBackPackCount;
        private static int _lastProtectedSlotsCount;
        private static Vector2 _lastBackPackLocation = new Vector2(-2, -2);

        public static readonly Vector2 NoFreeSlot = new Vector2(-1, -1);

        internal static void ResetBackPackCheck()
        {
            _lastBackPackCount = -1;
            _lastProtectedSlotsCount = -1;
            _lastBackPackLocation = new Vector2(-2, -2);
            LastCheckBackpackDurability = DateTime.MinValue;
        }

        public static DateTime LastCheckBackpackDurability { get; set; }

        public static bool IsAnyTwoSlotBackpackLocation
        {
            get
            {
                var validLocation = FindBackpackLocation(true, true);
                return validLocation.X >= 0 && validLocation.Y >= 0;
            }
        }

        private static readonly CacheField<bool> _isValidTwoSlotBackpackLocation = new CacheField<bool>(UpdateSpeed.Fast);

        public static bool CachedIsValidTwoSlotBackpackLocation => _isValidTwoSlotBackpackLocation.GetValue(IsValidTwoSlotBackpackLocation);

        public static bool IsValidTwoSlotBackpackLocation()
        {
            var validLocation = FindBackpackLocation(true, false);
            return validLocation.X >= 0 && validLocation.Y >= 0;
        }

        public bool IsBackpackFull => !IsValidTwoSlotBackpackLocation();

        public Vector2 FindBackpackSlot(bool twoSlot)
        {
            return FindBackpackLocation(twoSlot, true);
        }

        internal static Vector2 FindBackpackLocation(bool isOriginalTwoSlot, bool forceRefresh = false)
        {
            using (new PerformanceLogger("FindValidBackpackLocation"))
            {
                try
                {
                    if (!forceRefresh && _lastBackPackLocation != new Vector2(-2, -2) && _lastBackPackLocation != new Vector2(-1, -1) &&
                        _lastBackPackCount == Core.Inventory.BackpackItemCount &&
                        _lastProtectedSlotsCount == CharacterSettings.Instance.ProtectedBagSlots.Count)
                    {
                        return _lastBackPackLocation;
                    }

                    bool[,] backpackSlotBlocked = new bool[10, 6];

                    int freeBagSlots = 60;

                    if (!forceRefresh)
                    {
                        _lastProtectedSlotsCount = CharacterSettings.Instance.ProtectedBagSlots.Count;
                        _lastBackPackCount = Core.Inventory.BackpackItemCount;
                    }

                    // Block off the entire of any "protected bag slots"
                    foreach (InventorySquare square in CharacterSettings.Instance.ProtectedBagSlots)
                    {
                        backpackSlotBlocked[square.Column, square.Row] = true;
                        freeBagSlots--;
                    }

                    // Map out all the items already in the backpack
                    foreach (ACDItem item in InventoryManager.Backpack)
                    {
                        if (!item.IsValid)
                        {
                            Core.Logger.Error("Invalid backpack item detetected! marking down two slots!");
                            freeBagSlots -= 2;
                            continue;
                        }
                        int row = item.InventoryRow;
                        int col = item.InventoryColumn;

                        if (row < 0 || row > 5)
                        {
                            Core.Logger.Error("Item {0} ({1}) is reporting invalid backpack row of {2}!",
                                item.Name, item.InternalName, item.InventoryRow);
                            continue;
                        }

                        if (col < 0 || col > 9)
                        {
                            Core.Logger.Error("Item {0} ({1}) is reporting invalid backpack column of {2}!",
                                item.Name, item.InternalName, item.InventoryColumn);
                            continue;
                        }

                        // Slot is already protected, don't double count
                        if (!backpackSlotBlocked[col, row])
                        {
                            backpackSlotBlocked[col, row] = true;
                            freeBagSlots--;
                        }

                        if (!item.IsTwoSquareItem)
                            continue;

                        try
                        {
                            // Slot is already protected, don't double count
                            if (backpackSlotBlocked[col, row + 1])
                                continue;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (item.IsValid && !item.IsDisposed)
                            {
                                Core.Logger.Debug("Error checking for next slot on item {0}, row={1} col={2} IsTwoSquare={3} ItemType={4}",
                                    item.Name, item.InventoryRow, item.InventoryColumn, item.IsTwoSquareItem, item.ItemType);
                            }
                            else
                            {
                                Core.Logger.Debug("Error checking for next slot on item is no longer valid");
                            }
                            continue;
                        }

                        freeBagSlots--;
                        backpackSlotBlocked[col, row + 1] = true;
                    }

                    bool noFreeSlots = freeBagSlots < 1;
                    int unprotectedSlots = 60 - _lastProtectedSlotsCount;

                    // Use count of Unprotected slots if FreeBagSlots is higher than unprotected slots
                    int minFreeSlots = Core.Player.IsInTown ?
                        Math.Min(FreeBagSlotsInTown, unprotectedSlots) :
                        Math.Min(FreeBagSlots, unprotectedSlots);

                    // free bag slots is less than required
                    if (noFreeSlots || freeBagSlots < minFreeSlots && !forceRefresh)
                    {
                        Core.Logger.Debug("Free Bag Slots is less than required. FreeSlots={0}, FreeBagSlots={1} FreeBagSlotsInTown={2} IsInTown={3} Protected={4} BackpackCount={5}",
                            freeBagSlots, FreeBagSlots, FreeBagSlotsInTown, Core.Player.IsInTown,
                            _lastProtectedSlotsCount, _lastBackPackCount);
                        _lastBackPackLocation = NoFreeSlot;
                        return _lastBackPackLocation;
                    }

                    // 10 columns
                    Vector2 pos;
                    for (int col = 0; col <= 9; col++)
                    {
                        // 6 rows
                        for (int row = 0; row <= 5; row++)
                        {
                            // Slot is blocked, skip
                            if (backpackSlotBlocked[col, row])
                                continue;

                            // Not a two slotitem, slot not blocked, use it!
                            if (!isOriginalTwoSlot)
                            {
                                pos = new Vector2(col, row);
                                if (!forceRefresh)
                                {
                                    _lastBackPackLocation = pos;
                                }
                                return pos;
                            }

                            // Is a Two Slot, Can't check for 2 slot items on last row
                            if (row == 5)
                                continue;

                            // Is a Two Slot, check row below
                            if (backpackSlotBlocked[col, row + 1])
                                continue;

                            pos = new Vector2(col, row);
                            if (!forceRefresh)
                            {
                                _lastBackPackLocation = pos;
                            }
                            return pos;
                        }
                    }

                    // no free slot
                    Core.Logger.Debug("No Free slots!");

                    pos = NoFreeSlot;
                    if (!forceRefresh)
                    {
                        _lastBackPackLocation = pos;
                    }

                    return pos;
                }
                catch (Exception ex)
                {
                    Core.Logger.Log("Error in finding backpack slot");
                    Core.Logger.Log("{0}", ex.ToString());
                    return NoFreeSlot;
                }
            }
        }
    }
}