using System;
using System.ComponentModel;
using BepInEx.Configuration;
using UnityEngine;

namespace TheMessenger.TAS.Components.Trainers.Cameras;

public class CameraController : PluginComponent {
    enum ControlCameraTiming {
        [Description("OnPreCull (Might Desync)")]
        OnPreCull,
        [Description("OnPreRender (Limited Render Range)")]
        OnPreRender
    }

    private static ConfigEntry<ControlCameraTiming> controlCameraTiming;
    private static ConfigEntry<KeyboardShortcut> cameraResetHotkey;
    private static Vector3? origPosition;
    public static event Action<Vector3> OnCameraPositionChange;

    private void Awake() {
        controlCameraTiming = Plugin.Instance.Config.Bind("Camera", "Control Camera Timing", ControlCameraTiming.OnPreCull, 10);
        cameraResetHotkey = Plugin.Instance.Config.Bind("Camera", "Camera Reset", new KeyboardShortcut(KeyCode.End, KeyCode.LeftControl), 9);

        Camera.onPreCull += PreCull;
        Camera.onPreRender += PreRender;
        Camera.onPostRender += PostRender;
    }

    private void OnDestroy() {
        Camera.onPreCull -= PreCull;
        Camera.onPreRender -= PreRender;
        Camera.onPostRender -= PostRender;
    }

    private void Update() {
        if (cameraResetHotkey.IsDownEx()) {
            CameraMove.Offset = Vector2.zero;
            CameraZoom.Zoom = 1f;
        }
    }

    private void PreCull(Camera _) {
        if (controlCameraTiming.Value == ControlCameraTiming.OnPreCull) {
            ControlCamera();
        }
    }

    private void PreRender(Camera _) {
        if (controlCameraTiming.Value == ControlCameraTiming.OnPreRender) {
            ControlCamera();
        }
    }

    private void ControlCamera() {
        if (origPosition.HasValue) {
            return;
        }

        if ((CameraFollow.Following || Math.Abs(CameraZoom.Zoom) - 1f > 0.01f || CameraMove.Offset != Vector2.zero) && Camera.main is { } camera) {
            Vector3 position = camera.transform.position;
            origPosition = position;

            if (CameraFollow.Following && GetPlayer() is { } player) {
                position = player.transform.position;
                position.z = origPosition.Value.z;
            }

            camera.transform.position = new Vector3(position.x + CameraMove.Offset.x, position.y + CameraMove.Offset.y, position.z * CameraZoom.Zoom);

            OnCameraPositionChange?.Invoke(camera.transform.position);
        }
    }

    private void PostRender(Camera _) {
        if (origPosition.HasValue && Camera.main is { } camera) {
            camera.transform.position = origPosition.Value;
            origPosition = null;
        }
    }
}