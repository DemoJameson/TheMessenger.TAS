using BepInEx.Configuration;
using UnityEngine;

namespace TheMessenger.TAS.Components.Trainers.Cameras;

// TODO camera zoom does not work
public class CameraZoom : PluginComponent {
    private static ConfigEntry<KeyboardShortcut> cameraZoomInHotkey;
    private static ConfigEntry<KeyboardShortcut> cameraZoomOutHotkey;
    private const float step = 0.99f;
    public static float Zoom = 1f;

    // private void Awake() {
    //     cameraZoomInHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Zoom In", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftControl));
    //     cameraZoomOutHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Zoom Out", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftControl));
    // }
    //
    // private void Update() {
    //     if (cameraZoomInHotkey.IsPressedEx()) {
    //         Zoom *= step;
    //     }
    //
    //     if (cameraZoomOutHotkey.IsPressedEx()) {
    //         Zoom /= step;
    //     }
    // }
}