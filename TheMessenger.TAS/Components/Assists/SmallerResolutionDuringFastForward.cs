using System;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using TAS;
using UnityEngine;

namespace TheMessenger.TAS.Components.Assists;

[HarmonyPatch]
public class SmallerResolutionDuringFastForward : PluginComponent {
    private static ConfigEntry<bool> smallerResolutionDuringFastForward;
    private static Resolution? lastResolution;
    
    // 提前两帧恢复窗口大小
    private static bool UltraFastForwarding =>
        Manager.UltraFastForwarding && Manager.Controller.FastForwards.LastOrDefault().Value is {} ff && ff.Frame > Manager.Controller.CurrentFrameInTas + 2;

    [DisableRun]
    private static void ResetResolution() {
        if (lastResolution is { } r) {
            Screen.SetResolution(r.width, r.height, Screen.fullScreen, 0);
            lastResolution = null;
        }
    }

    private void Awake() {
        smallerResolutionDuringFastForward = Plugin.Instance.Config.Bind("Assists", "Smaller Resolution During FastForward", false);
    }

    private void Update() {
        if (!smallerResolutionDuringFastForward.Value) {
            return;
        }

        if (UltraFastForwarding && lastResolution == null) {
            lastResolution = new Resolution {
                width = Screen.width,
                height = Screen.height
            };
            
            Screen.SetResolution(320, 180, Screen.fullScreen, 0);
        } else if (!UltraFastForwarding && lastResolution.HasValue) {
            ResetResolution();
        }
    }
}