using HarmonyLib;
using Rewired;
using TAS;
using TAS.Core;
using TAS.Core.Hotkey;
using UnityEngine;

namespace TheMessenger.TAS;

[HarmonyPatch]
public class ManagerUpdater : PluginComponent {
    private void Update() {
        Hotkeys.AllowKeyboard = Application.isFocused || !CommunicationServer.Connected;
    }

    [HarmonyPatch(typeof(InputManager_Base), "Update")]
    [HarmonyPrefix]
    private static bool InputManagerBaseUpdate() {
        Manager.Update();
        return !Manager.Running;
    }
}