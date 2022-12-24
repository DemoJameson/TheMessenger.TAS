using HarmonyLib;
using TAS;
using UnityEngine;

namespace TheMessenger.TAS.Components.Determinism;

[HarmonyPatch]
public class FixedUnscaledDeltaTime {
    [HarmonyPatch(typeof(Time), nameof(Time.unscaledDeltaTime), MethodType.Getter)]
    [HarmonyPatch(typeof(Time), nameof(Time.smoothDeltaTime), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool TimeUnscaledDeltaTime(ref float __result) {
        if (Manager.Running) {
            __result = Time.deltaTime;
            return false;
        } else {
            return true;
        }
    }
    
    [HarmonyPatch(typeof(Time), nameof(Time.unscaledTime), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool TimeUnscaledTime(ref float __result) {
        if (Manager.Running) {
            __result = Time.time;
            return false;
        } else {
            return true;
        }
    }
}