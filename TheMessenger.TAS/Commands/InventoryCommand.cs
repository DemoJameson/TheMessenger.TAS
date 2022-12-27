using System;
using System.Reflection;
using HarmonyLib;
using TAS.Core.Input.Commands;
using TheMessenger.TAS.Utils;

namespace TheMessenger.TAS.Commands;

[HarmonyPatch]
public class InventoryCommand : PluginComponent {
    private static bool invokingCommand;

    [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.SetItemQuantity))]
    [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.AddItem))]
    [HarmonyPostfix]
    private static void SetItemQuantity(EItems itemId, int quantity, InventoryManager __instance, MethodBase __originalMethod) {
        if (invokingCommand || itemId is EItems.TIME_SHARD or EItems.MANA or EItems.POTION) {
            return;
        }

        Plugin.Log.LogInfo($"{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}: itemId={itemId} quantity={quantity} currentQuantity={__instance.GetItemQuantity(itemId)}");
    }
    
    [TasCommand("Inventory", LegalInMainGame = false)]
    private static void Inventory(string[] args) {
        if (args.Length < 2) {
            ShowToastAndDisableTAS("Inventory Command Failed\nRequired item and quantity parameters");
            return;
        }

        if (EnumHelpers<EItems>.TryParse(args[0], out EItems item)) {
            if (int.TryParse(args[1], out int quantity)) {
                quantity = Math.Max(0, quantity);
                if (Manager<InventoryManager>.instance is { } inventoryManager) {
                    invokingCommand = true;

                    if (quantity == 0) {
                        inventoryManager.RemoveItem(item, int.MaxValue);
                        if (item == EItems.CLIMBING_CLAWS) {
                            HasClimbedCommand.HasClimbed(new[] {"false"});
                        }
                    } else {
                        inventoryManager.SetItemQuantity(item, quantity);
                    }

                    ProcessShopUpgradeID(item, quantity);

                    invokingCommand = false;
                }
            } else {
                ShowToastAndDisableTAS("Inventory Command Failed\nInvalid quantity");
            }
        } else {
            ShowToastAndDisableTAS("Inventory Command Failed\nInvalid item");
        }
    }

    private static void ProcessShopUpgradeID(EItems item, int quantity) {
        if (Manager<InventoryManager>.instance is not { } inventoryManager) {
            return;
        }

        if (item == EItems.HEART_CONTAINER) {
            if (quantity == 0) {
                inventoryManager.shopUpgradeUnlocked.Remove(EShopUpgradeID.HP_UPGRADE_1);
                inventoryManager.shopUpgradeUnlocked.Remove(EShopUpgradeID.HP_UPGRADE_2);
            } else {
                inventoryManager.SetShopUpgradeAsUnlocked(EShopUpgradeID.HP_UPGRADE_1);
                if (quantity == 1) {
                    inventoryManager.shopUpgradeUnlocked.Remove(EShopUpgradeID.HP_UPGRADE_2);
                } else {
                    inventoryManager.SetShopUpgradeAsUnlocked(EShopUpgradeID.HP_UPGRADE_2);
                } 
            }
        } else if (item == EItems.SHURIKEN_UPGRADE) {
            if (quantity == 0) {
                inventoryManager.shopUpgradeUnlocked.Remove(EShopUpgradeID.SHURIKEN_UPGRADE_1);
                inventoryManager.shopUpgradeUnlocked.Remove(EShopUpgradeID.SHURIKEN_UPGRADE_2);
            } else {
                inventoryManager.SetShopUpgradeAsUnlocked(EShopUpgradeID.SHURIKEN_UPGRADE_1);
                if (quantity == 1) {
                    inventoryManager.shopUpgradeUnlocked.Remove(EShopUpgradeID.SHURIKEN_UPGRADE_2);
                } else {
                    inventoryManager.SetShopUpgradeAsUnlocked(EShopUpgradeID.SHURIKEN_UPGRADE_2);
                } 
            }
        } else if (GetUpgradeID(item) is { } upgradeID) {
            if (quantity == 0) {
                inventoryManager.shopUpgradeUnlocked.Remove(upgradeID);
            } else {
                inventoryManager.SetShopUpgradeAsUnlocked(upgradeID);
            }
        }
    }

    private static EShopUpgradeID? GetUpgradeID(EItems item) {
        return item switch {
            EItems.AIR_RECOVER => EShopUpgradeID.AIR_RECOVER,
            EItems.ATTACK_PROJECTILES => EShopUpgradeID.ATTACK_PROJECTILE,
            EItems.CHARGED_ATTACK => EShopUpgradeID.CHARGED_ATTACK,
            EItems.CHECKPOINT_UPGRADE => EShopUpgradeID.CHECKPOINT_FULL,
            EItems.DAMAGE_REDUCTION => EShopUpgradeID.DAMAGE_REDUCTION,
            EItems.ENEMY_DROP_HP => EShopUpgradeID.ENEMY_DROP_HP,
            EItems.ENEMY_DROP_MANA => EShopUpgradeID.ENEMY_DROP_MANA,
            EItems.GLIDE_ATTACK => EShopUpgradeID.GLIDE_ATTACK,
            EItems.POTION_FULL_HEAL_AND_HP_UPGRADE => EShopUpgradeID.POTION_FULL_HEAL_AND_HP,
            EItems.MAP_POWER_SEAL_TOTAL => EShopUpgradeID.POWER_SEAL_WORLD_MAP,
            EItems.MAP_POWER_SEAL_PINS => EShopUpgradeID.POWER_SEAL,
            EItems.QUARBLE_DISCOUNT_50 => EShopUpgradeID.QUARBLE_DISCOUNT_50,
            EItems.SHURIKEN => EShopUpgradeID.SHURIKEN,
            EItems.SWIM_DASH => EShopUpgradeID.SWIM_DASH,
            EItems.MAP_TIME_WARP => EShopUpgradeID.TIME_WARP,
            _ => null
        };
    }
}

// public enum EItems {
//     NONE = -1,
//     TIME_SHARD = 0,
//     HEALING_HEART = 1,
//     HEART_CONTAINER = 2,
//     SCROLL_UPGRADE = 3,
//     GRAPLOU = 4,
//     WINGSUIT = 6,
//     CLIMBING_CLAWS = 7,
//     KEY_OF_COURAGE = 11,
//     KEY_OF_HOPE = 12,
//     KEY_OF_LOVE = 13,
//     KEY_OF_STRENGTH = 14,
//     KEY_OF_CHAOS = 15,
//     KEY_OF_SYMBIOSIS = 16,
//     SEASHELL = 19,
//     NECROPHOBIC_WORKER = 20,
//     ACROPHOBIC_WORKER = 21,
//     CLAUSTROPHOBIC_WORKER = 22,
//     PYROPHOBIC_WORKER = 23,
//     POWER_THISTLE = 25,
//     SHURIKEN = 26,
//     CANDLE = 29,
//     MANA_CONTAINER = 31,
//     MANA = 32,
//     DAMAGE_REDUCTION = 34,
//     AIR_RECOVER = 37,
//     SHURIKEN_UPGRADE = 38,
//     SWIM_DASH = 39,
//     MAGIC_BOOTS = 40,
//     GLIDE_ATTACK = 41,
//     POTION_FULL_HEAL_AND_HP_UPGRADE = 42,
//     CHECKPOINT_UPGRADE = 43,
//     DAMAGE_BOOST = 45,
//     QUARBLE_DISCOUNT_50 = 49,
//     BASIC_SCROLL = 50,
//     DEMON_KING_CROWN = 51,
//     MAP = 52,
//     PRE_COLLECTED_MUSIC_NOTE_1 = 53,
//     PRE_COLLECTED_MUSIC_NOTE_2 = 54,
//     RUXXTIN_AMULET = 55,
//     TEA_SEED = 56,
//     TEA_LEAVES = 57,
//     SUN_CREST = 58,
//     MOON_CREST = 59,
//     FAIRY_BOTTLE = 60,
//     ENEMY_DROP_MANA = 61,
//     ENEMY_DROP_HP = 62,
//     CHARGED_ATTACK = 63,
//     ATTACK_PROJECTILES = 64,
//     CLOUD_STEP = 65,
//     POWER_SEAL = 66,
//     MAP_TIME_WARP = 68,
//     MAP_POWER_SEAL_TOTAL = 69,
//     MAP_POWER_SEAL_PINS = 70,
//     POTION = 71,
//     WINDMILL_SHURIKEN = 72,
//     JUKEBOX = 73,
//     SUNKEN_CRESTS = 74,
//     PHOBEKINS = 75,
//     MONEY_SINK_REWARD = 76,
//     MONEY_WRENCH = 77,
//     MASK_PIECE = 78,
//     FEATHER = 79
// }