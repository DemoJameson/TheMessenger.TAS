using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TheMessenger.TAS.Components.Trainers;

[HarmonyPatch]
public class Cheats : PluginComponent {
    private static ConfigEntry<KeyboardShortcut> showCheats;
    private static int selectedLevel;

    [HarmonyPatch(typeof(CheatScreen), nameof(CheatScreen.BackToPauseMenu))]
    [HarmonyPrefix]
    private static bool CheatScreenBackToPauseMenu(CheatScreen __instance) {
        __instance.Close(true);
        return false;
    }
    
    [HarmonyPatch(typeof(CheatScreen), nameof(CheatScreen.LateUpdate))]
    [HarmonyPrefix]
    private static void CheatScreenLateUpdate(CheatScreen __instance) {
        if (Manager<InputManager>.Instance.GetBackDown()) {
            __instance.Close(true);
        }
    }
    
    [HarmonyPatch(typeof(PlayerDefaultState), nameof(PlayerDefaultState.ProcessInput))]
    [HarmonyPrefix]
    private static bool PlayerDefaultStateProcessInput() {
        return !ShowingCheatScreen();
    }

    [HarmonyPatch(typeof(LevelSelectCheat), nameof(LevelSelectCheat.Start))]
    [HarmonyPostfix]
    private static void LevelSelectCheatStart(LevelSelectCheat __instance) {
        __instance.selectedLevel = selectedLevel;
        __instance.SetLevel((ELevel) selectedLevel);
    }
    
    [HarmonyPatch(typeof(LevelSelectCheat), "OnMove")]
    [HarmonyPostfix]
    private static void LevelSelectCheatOnMove(LevelSelectCheat __instance) {
        selectedLevel = __instance.selectedLevel;
    }

    private void Awake() {
        showCheats = Plugin.Instance.Config.Bind("Trainers", "Show Cheats Menu", new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl));
    }

    private void Update() {
        if (Manager<PlayerManager>.instance is not {player: { } player} || player.paused) {
            return;
        }

        if (showCheats.IsDownEx() && Manager<UIManager>.instance is { } manager) {
            if (ShowingCheatScreen()) {
                manager.CloseAllScreensOfType<CheatScreen>(true);
            } else {
                manager.ShowView<CheatScreen>(EScreenLayers.PROMPT);
            }
        }
    }

    private static bool ShowingCheatScreen() {
        if (Manager<UIManager>.instance is { } manager) {
            return manager.GetView<CheatScreen>();
        } else {
            return false;
        }
    }
}