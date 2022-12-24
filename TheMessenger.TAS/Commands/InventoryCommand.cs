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