using System;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TheMessenger.TAS.Components.Trainers.Cameras;

[HarmonyPatch]
public class CameraFollow : PluginComponent {
    public static bool Following;
    private static ConfigEntry<KeyboardShortcut> cameraFollowHotkey;
    
    private void Awake() {
        cameraFollowHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Follow", new KeyboardShortcut(KeyCode.F, KeyCode.LeftControl));
    }

    private void Update() {
        if (cameraFollowHotkey.IsDownEx()) {
            Following = !Following;
        }
    }
}