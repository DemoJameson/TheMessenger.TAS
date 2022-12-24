using TAS;
using TheMessenger.TAS.Components.Helpers;

namespace TheMessenger.TAS.Utils; 

public static class GlobalUtils {
    public static void ShowToastAndDisableTAS(string message) {
        Toast.Show(message);
        Manager.DisableRunLater();
    }
}