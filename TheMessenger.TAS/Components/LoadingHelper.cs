using HarmonyLib;
using TheMessenger.TAS.Components.Helpers;

namespace TheMessenger.TAS.Components; 

[HarmonyPatch]
public class LoadingHelper : PluginComponent {
    public static bool Loading { get; private set; }

    [HarmonyPatch(typeof(LevelManager), nameof(LevelManager.LoadLevel))]
    [HarmonyPrefix]
    private static void LevelManagerLoadLevel() {
        Loading = true;
    }

    private void Awake() {
        // HookHelper.SceneLoaded(SceneLoaded);
    }
}