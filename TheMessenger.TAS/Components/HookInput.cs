using HarmonyLib;
using Rewired;
using TAS;
using TAS.Core.Input;
using TAS.Core.Utils;
using TAS.Shared;
using TheMessenger.TAS.Utils;

namespace TheMessenger.TAS.Components;

[HarmonyPatch(typeof(Player))]
public class HookInput : PluginComponent {
    private static int currentFrame;
    private static InputFrame previousInput;
    private static InputFrame currentInput;

    private static bool TryGetActions(string actionName, out Actions actions) {
        actions = actionName switch {
            "Dpad Left" => Actions.Left,
            "Dpad Right" => Actions.Right,
            "Dpad Up" => Actions.Up,
            "Dpad Down" => Actions.Down,
            "Jump" => Actions.JumpAndConfirm,
            "Confirm" => Actions.JumpAndConfirm,
            "Graplou" => Actions.RopeDartAndBack,
            "Back" => Actions.RopeDartAndBack,
            "Attack" => Actions.Attack,
            "Shuriken" => Actions.Shuriken,
            "MagicBoots" => Actions.Tabi,
            "Map" => Actions.Map,
            "Inventory" => Actions.Inventory,
            "Start" => Actions.Pause,
            _ => Actions.None
        };

        return actions != Actions.None;
    }

    public static void SetInputs() {
        InputController controller = Manager.Controller;
        currentFrame = controller.CurrentFrameInTas;
        previousInput = controller.Previous;
        currentInput = controller.Current;
    }

    [EnableRun]
    [DisableRun]
    private static void ResetButtonStates() {
        currentFrame = -1;
        previousInput = null;
        currentInput = null;
    }

    [HarmonyPatch(nameof(Player.GetAxis), typeof(string))]
    [HarmonyPrefix]
    private static bool GetAxis(string actionName, ref float __result) {
        if (!Manager.Running || currentInput == null) {
            return true;
        }

        if (TryGetActions(actionName, out var actions) && currentInput.HasActions(actions)) {
            __result = actions switch {
                Actions.Tabi => 1f,
                Actions.Right => 1f,
                Actions.Left => -1f,
                Actions.Up => 1f,
                Actions.Down => -1f,
                _ => 0f
            };
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButton), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButton(string actionName, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        if (TryGetActions(actionName, out var actions)) {
            __result = currentInput?.HasActions(actions) == true;
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButtonDown), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonDown(string actionName, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        if (TryGetActions(actionName, out var actions)) {
            __result = previousInput?.HasActions(actions) != true && currentInput?.HasActions(actions) == true;
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButtonUp), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonUp(string actionName, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        if (TryGetActions(actionName, out var actions)) {
            __result = previousInput?.HasActions(actions) == true && currentInput?.HasActions(actions) == false;
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButtonTimePressed), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonTimePressed(string actionName, ref float __result) {
        if (!Manager.Running) {
            return true;
        }

        if (TryGetActions(actionName, out var actions) && currentInput?.HasActions(actions) == true) {
            InputController controller = Manager.Controller;
            for (int i = currentFrame - 1; i >= 0; i--) {
                InputFrame previous = controller.Inputs.GetValueOrDefault(i);
                if (previous == null || !previous.HasActions(actions)) {
                    break;
                }

                __result += 1f / GameComponent.FixedFrameRate;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(InputManager_Base), "Update")]
    [HarmonyPrefix]
    private static bool InputManagerUpdate() {
        return !Manager.Running;
    }

    [HarmonyPatch(typeof(InputManager_Base), "FixedUpdate")]
    [HarmonyPrefix]
    private static bool InputManagerFixedUpdate() {
        return !Manager.Running;
    }
}