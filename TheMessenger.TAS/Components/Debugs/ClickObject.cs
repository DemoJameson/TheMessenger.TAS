using BepInEx.Configuration;
using UnityEngine;

namespace TheMessenger.TAS.Components.Debugs;

public class ClickObject : PluginComponent {
    private ConfigEntry<bool> option;

    private void Awake() {
        option = Plugin.Instance.Config.Bind("Debug", "Click Object", false, isAdvanced: true);
    }

    private void Update() {
        if (!option.Value) {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Camera.main is {} camera) {
            RaycastHit2D hit = Physics2D.CircleCast(camera.ScreenToWorldPoint(Input.mousePosition), 1f, Vector2.up);
            if (hit.collider != null) {
                Debug.LogWarning($"{GetNames(hit.transform)}\n{hit.transform.position}");
            }
        }
    }

    private static string GetNames(Transform transform) {
        string result = transform.name;
        while (transform = transform.parent) {
            result = $"{transform.name}/{result}";
        }

        return result;
    }
}