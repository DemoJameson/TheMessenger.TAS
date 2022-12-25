using BepInEx.Configuration;
using UnityEngine;

namespace TheMessenger.TAS.Components.Trainers.Cameras;

public class CameraMove : PluginComponent {
    private static ConfigEntry<KeyboardShortcut> cameraMoveLeftHotkey;
    private static ConfigEntry<KeyboardShortcut> cameraMoveRightHotkey;
    private static ConfigEntry<KeyboardShortcut> cameraMoveUpHotkey;
    private static ConfigEntry<KeyboardShortcut> cameraMoveDownHotkey;
    private const float step = 1f;
    public static Vector2 Offset = Vector2.zero;

    private void Awake() {
        int order = 0;
        cameraMoveLeftHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Move Left",
            new KeyboardShortcut(KeyCode.LeftArrow, KeyCode.LeftControl), --order);
        cameraMoveRightHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Move Right",
            new KeyboardShortcut(KeyCode.RightArrow, KeyCode.LeftControl), --order);
        cameraMoveUpHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Move Up",
            new KeyboardShortcut(KeyCode.UpArrow, KeyCode.LeftControl), --order);
        cameraMoveDownHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Move Down",
            new KeyboardShortcut(KeyCode.DownArrow, KeyCode.LeftControl), --order);
    }

    private void Update() {
        if (cameraMoveLeftHotkey.IsPressedEx()) {
            Offset.x -= step;
        }

        if (cameraMoveRightHotkey.IsPressedEx()) {
            Offset.x += step;
        }

        if (cameraMoveDownHotkey.IsPressedEx()) {
            Offset.y -= step;
        }

        if (cameraMoveUpHotkey.IsPressedEx()) {
            Offset.y += step;
        }
    }
}