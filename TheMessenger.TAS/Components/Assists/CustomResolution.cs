using System.Collections;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TheMessenger.TAS.Components.Assists;

[HarmonyPatch]
public class CustomResolution : PluginComponent {
    enum Resolutions {
        Original,
        W800H450,
        W960H540,
        W1120H630,
    }

    private static CustomResolution instance;
    private static ConfigEntry<Resolutions> resolution;

    private void Awake() {
        instance = this;
        resolution = Plugin.Instance.Config.Bind("Assists", "Custom Resolution", Resolutions.Original);
        resolution.SettingChanged += (_, _) => { SetResolution(); };

        if (!IsHotReloaded) {
            StartCoroutine(WaitGameLaunched());
        }
    }

    private static void SetResolution() {
        instance.StopAllCoroutines();

        GetResolution(out int width, out int height);

        if (width > 0 && height > 0) {
            Screen.SetResolution(width, height, false);
        }

        if (Manager<SaveManager>.instance is {saveGame: { } saveGame}) {
            saveGame.options.pixelPerfect = false;
        }
    }

    public static void GetResolution(out int width, out int height) {
        width = 0;
        height = 0;
        if (resolution.Value == Resolutions.Original) {
            if (Manager<SaveManager>.instance is {saveGame.options: {} options}) {
                width = options.resolutionWidth;
                height = options.resolutionHeight;
            }
        } else if (resolution.Value == Resolutions.W800H450) {
            width = 800;
            height = 450;
        } else if (resolution.Value == Resolutions.W960H540) {
            width = 960;
            height = 540;
        } else {
            width = 1120;
            height = 630;
        }
    }

    private static IEnumerator WaitGameLaunched() {
        while (Manager<SaveManager>.instance is not {saveGame: { }}) {
            yield return null;
        }

        SetResolution();
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ResolutionIsValid))]
    [HarmonyPrefix]
    private static bool SaveManagerResolutionIsValid(ref bool __result) {
        if (resolution.Value == Resolutions.Original) {
            return true;
        } else {
            __result = true;
            return false;
        }
    }
}