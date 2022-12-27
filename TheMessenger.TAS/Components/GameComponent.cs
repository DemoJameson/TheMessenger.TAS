using HarmonyLib;
using TAS;
using TAS.Core;
using TAS.Core.Input;
using UnityEngine;

namespace TheMessenger.TAS.Components;

[HarmonyPatch]
public class GameComponent : PluginComponent, IGame {
    public static GameComponent Instance { get; private set; }
    public static int FixedFrameRate = DefaultFixedFrameRate;
    public static int DefaultFixedFrameRate => 60;
    public string CurrentTime => GameInfoHelper.FormattedRoomTime;
    public float FastForwardSpeed => FastForward.DefaultSpeed;
    public float SlowForwardSpeed => 0.1f;
    public string LevelName => CurrentSceneName;
    public ulong FrameCount => (ulong) Time.frameCount;
    public bool IsLoading => SceneLoader.loadingScene;
    public string GameInfo => GameInfoHelper.Info;

    private void Awake() {
        Instance = this;
        Manager.Init(this);
    }

    public void SetFrameRate(float multiple) {
        int newFrameRate = (int) (FixedFrameRate * multiple);
        Time.timeScale = Time.timeScale == 0 ? 0 : (float) newFrameRate / FixedFrameRate;
        Time.captureFramerate = newFrameRate;
        Application.targetFrameRate = newFrameRate;
        Time.fixedDeltaTime = 1f / FixedFrameRate;
        Time.maximumDeltaTime = Time.fixedDeltaTime;
        QualitySettings.vSyncCount = 0;
    }

    public void SetInputs(InputFrame currentInput) {
        HookInput.SetInputs();
    }
}