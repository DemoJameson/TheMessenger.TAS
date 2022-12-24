using BepInEx.Configuration;
using UnityEngine;

namespace TheMessenger.TAS.Components.Trainers;

public class TeleportToMouse : PluginComponent {
    private static ConfigEntry<KeyboardShortcut> teleportHotkey;

    private void Awake() {
        teleportHotkey = Plugin.Instance.Config.Bind("Trainers", "Teleport To Mouse", new KeyboardShortcut(KeyCode.T));
        Camera.onPreRender += PreRender;
    }

    private void OnDestroy() {
        Camera.onPreRender -= PreRender;
    }

    private void Update() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void PreRender(Camera cam) {
        if (teleportHotkey.IsDownEx()) {
            if (Manager<PlayerManager>.instance?.player is not { } player || Camera.main is not { } camera) {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = camera.nearClipPlane;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            player.transform.position = worldPos;
            player.velocity = Vector2.zero;
        }
    }
}