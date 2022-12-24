using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace TheMessenger.TAS.Commands;

public class TimeShardCommand : PluginComponent {
    [TasCommand("TimeShard", LegalInMainGame = false)]
    private static void TimeShard(string[] args) {
        if (args.IsEmpty()) {
            return;
        }

        if (int.TryParse(args[0], out int timeShard)) {
            if (timeShard < 0) {
                timeShard = 0;
            }

            if (Manager<InventoryManager>.Instance is { } inventoryManager) {
                inventoryManager.SetItemQuantity(EItems.TIME_SHARD, timeShard);
            }

            if (Manager<UIManager>.Instance is { } uiManager && uiManager.GetView<InGameHud>() is { } view) {
                view.RefreshTimeshards();
            }
        }
    }
}