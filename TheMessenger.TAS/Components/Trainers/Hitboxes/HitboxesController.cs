using System.Collections;
using BepInEx.Configuration;
using HarmonyLib;
using TAS;
using TheMessenger.TAS.Components.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMessenger.TAS.Components.Trainers.Hitboxes;

public enum HitboxState {
    Hide,
    ShowMost,
    ShowAll
}

[HarmonyPatch]
public class HitboxesController : PluginComponent {
    private static HitboxesRenderer hitboxesRenderer;

    private static HitboxState state = HitboxState.ShowMost;

    public static HitboxState State {
        get => hideHitboxesDuringFastForward.Value && Manager.FastForwarding ? HitboxState.Hide : state;
        private set => state = value;
    }

    private static ConfigEntry<KeyboardShortcut> hitboxesHotkey;
    private static ConfigEntry<bool> hideHitboxesDuringFastForward;
    private static HitboxesController instance;

    [HarmonyPatch(typeof(PoolManager), nameof(PoolManager.GetObjectInstance))]
    [HarmonyPostfix]
    private static void SpawnerUpdate(GameObject __result) {
        AddHitboxes(__result);
    }

    [HarmonyPatch(typeof(PlayerEnterShopState), nameof(PlayerEnterShopState.StateExit))]
    [HarmonyPostfix]
    private static void PlayerEnterShopStateStateExit() {
        if (SceneManager.GetSceneByName("Shop") is {isLoaded: true} shopScene && shopScene.IsValid()) {
            foreach (GameObject rootGameObject in shopScene.GetRootGameObjects()) {
                AddHitboxes(rootGameObject);
            }
        }
    }

    [HarmonyPatch(typeof(TowerOfTimeHQManager), nameof(TowerOfTimeHQManager.TeleportInToTHQ))]
    [HarmonyPostfix]
    private static void TowerOfTimeHQManagerTeleportInToTHQ() {
        if (SceneManager.GetSceneByName("Level_13_TowerOfTimeHQ_Build") is {isLoaded: true} towerScene && towerScene.IsValid()) {
            foreach (GameObject rootGameObject in towerScene.GetRootGameObjects()) {
                AddHitboxes(rootGameObject);
            }
        }
    }

    [HarmonyPatch(typeof(LevelManager), nameof(LevelManager.CallOnLevelReload))]
    [HarmonyPostfix]
    private static void LevelManagerCallOnLevelReload() {
        instance.StartCoroutine(DelayedCreateHitboxRender());
    }

    public void Awake() {
        instance = this;
        hitboxesHotkey = Plugin.Instance.Config.Bind("Trainers", "Toggle Hitboxes", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl));
        hideHitboxesDuringFastForward = Plugin.Instance.Config.Bind("Trainers", "Hide Hitboxes During FastForward", true);
        HookHelper.ActiveSceneChanged((_, scene) => StartCoroutine(DelayedCreateHitboxRender()));
        CreateHitboxRender();
    }

    private void Update() {
        if (hitboxesHotkey.IsDownEx()) {
            State = State switch {
                HitboxState.Hide => HitboxState.ShowMost,
                HitboxState.ShowMost => HitboxState.ShowAll,
                HitboxState.ShowAll => HitboxState.Hide,
                _ => HitboxState.ShowMost
            };

            if (State == HitboxState.ShowMost) {
                Toast.Show("Show Most Hitboxes");
            } else if (State == HitboxState.ShowAll) {
                Toast.Show("Show All Hitboxes");
            }
        }
    }

    public void OnDestroy() {
        DestroyHitboxRender();
    }

    private static void CreateHitboxRender() {
        DestroyHitboxRender();
        hitboxesRenderer = new GameObject().AddComponent<HitboxesRenderer>();
    }

    private static void DestroyHitboxRender() {
        if (hitboxesRenderer) {
            Destroy(hitboxesRenderer);
            hitboxesRenderer = null;
        }
    }

    private static void AddHitboxes(GameObject go) {
        if (hitboxesRenderer) {
            hitboxesRenderer.AddHitboxes(go);
        }
    }

    private static IEnumerator DelayedCreateHitboxRender() {
        yield return null;
        CreateHitboxRender();
    }
}