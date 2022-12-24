using HarmonyLib;
using UnityEngine;

namespace TheMessenger.TAS.Components.Determinism; 

[HarmonyPatch]
public class FixedAnimator {
    [HarmonyPatch(typeof(Animator), nameof(Animator.updateMode), MethodType.Setter)]
    [HarmonyPrefix]
    private static void AnimatorUpdateMode(ref AnimatorUpdateMode value) {
        if (value == UnityEngine.AnimatorUpdateMode.UnscaledTime) {
            value = UnityEngine.AnimatorUpdateMode.Normal;
        }
    }
}