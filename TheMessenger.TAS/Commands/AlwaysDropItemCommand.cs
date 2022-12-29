using HarmonyLib;
using TAS;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Utils;

namespace TheMessenger.TAS.Commands;

[HarmonyPatch]
public class AlwaysDropItemCommand {
    enum ItemType {
        Default,
        TimeShard,
        Mana,
        HP,
        None,
    }

    private const string commandName = "AlwaysDropItem";
    private static ItemType itemType;
    
    [DisableRun]
    private static void DisableRun() {
        itemType = ItemType.Default;
    }

    // 使用 Postfix 确保 RNG 一致
    [HarmonyPatch(typeof(ItemDropperData), nameof(ItemDropperData.GetItemToDrop))]
    [HarmonyPostfix]
    private static void GetItemToDrop(ItemDropperData __instance, ref ItemDropperData.ItemDropData __result) {
        if (itemType == ItemType.Default) {
            return;
        }

        foreach (ItemDropperData.ItemDropData data in __instance.itemDropData) {
            if (!data.itemToDrop) {
                if (itemType == ItemType.None) {
                    __result = data;
                    return;
                } else {
                    continue;
                }
            }

            if (data.itemToDrop.name.Contains(itemType.ToString())) {
                __result = data;
                return;
            }
        }
    }

    [TasCommand(commandName, LegalInMainGame = true)]
    private static void AlwaysDropItem(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired specify item type");
            return;
        }

        if (EnumHelpers<ItemType>.TryParse(args[0], out ItemType direction)) {
            itemType = direction;
        } else {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nInvalid item type");
        }
    }
}