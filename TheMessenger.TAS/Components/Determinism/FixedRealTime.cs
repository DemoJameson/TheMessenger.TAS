using System;
using System.Collections;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TAS;
using UnityEngine;

namespace TheMessenger.TAS.Components.Determinism;

[HarmonyPatch]
public class FixedRealTime {
    [HarmonyPatch(typeof(CoroutineUtil), nameof(CoroutineUtil.WaitForRealSeconds))]
    [HarmonyPrefix]
    private static bool CoroutineUtilWaitForRealSeconds(float seconds, ref IEnumerator __result) {
        if (Manager.Running) {
            __result = CoroutineUtil.WaitForSeconds(seconds);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ZoneFXSpawner), nameof(ZoneFXSpawner.Awake))]
    [HarmonyPrefix]
    private static void ZoneFXSpawnerAwake(ZoneFXSpawner __instance) {
        if (Manager.Running) {
            __instance.useRealTime = false;
        }
    }

    [HarmonyPatch(typeof(QuillshroomMarshBossOutroCutscene), nameof(QuillshroomMarshBossOutroCutscene.QuillbleAppearNoQuarbleCoroutine), MethodType.Enumerator)]
    [HarmonyPatch(typeof(QuillshroomMarshBossOutroCutscene), nameof(QuillshroomMarshBossOutroCutscene.QuillbleAppearCoroutine), MethodType.Enumerator)]
    [HarmonyPatch(typeof(QuillshroomMarshBossOutroCutscene), nameof(QuillshroomMarshBossOutroCutscene.WaitAndEndCutscene), MethodType.Enumerator)]
    [HarmonyPatch(typeof(QuillshroomMarshBossOutroCutscene), nameof(QuillshroomMarshBossOutroCutscene.WaitAndHideQuarbleCoroutine), MethodType.Enumerator)]
    [HarmonyPatch(typeof(QuillshroomMarshBossOutroCutscene), nameof(QuillshroomMarshBossOutroCutscene.MessengerCutSceneCoroutine), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void ReplaceWaitForSecondsRealtime(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        while (ilCursor.TryGotoNext(i => i.MatchNewobj<WaitForSecondsRealtime>())) {
            ilCursor.Emit(OpCodes.Dup);
            ilCursor.Index++;
            ilCursor.EmitDelegate<Func<float, object, object>>((time, waitObj) => {
                if (Manager.Running) {
                    return new WaitForSeconds(time);
                } else {
                    return waitObj;
                }
            });
            ilCursor.Index++;
        }
    }
}