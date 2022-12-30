using System.Collections.Generic;
using TAS.Core;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMessenger.TAS.Commands;

// TODO 目前 Position.y 必需得是平台上，并且不够精确很可能被处理为整数
// TODO FastLoad 没有完整的重置 Player 的状态，目前只能通过原地等待若干帧让 Player 自行重置状态
public class LoadCommand : PluginComponent {
    private void Awake() {
        // TODO 以 FastLoad 为主，然后输出设置当前状态的所有命令
        CommunicationServer.ReturnDataEvent += dataType => {
            if (dataType == "Load") {
                EBits dimension = Manager<DimensionManager>.instance?.currentDimension ?? EBits.BITS_8;
                string result = dimension == EBits.BITS_16 ? "FastLoad16" : "FastLoad8";
                result += $" {CurrentSceneIndex}";
                if (Manager<PlayerManager>.instance is {player.transform.position: var position} playerManager) {
                    result = $"{result} {position.x} {position.y}";
                    if (ExternalSpeedCommand.GenerateCommand() is { } externalCommand) {
                        result = $"{result}\n{externalCommand}";
                    }

                    return result;
                } else {
                    return result;
                }
            } else {
                return null;
            }
        };
    }

    [TasCommand("Load8", LegalInMainGame = false)]
    private static void Load8(string[] args) {
        LoadInternal(args, false, EBits.BITS_8);
    }

    [TasCommand("Load16", LegalInMainGame = false)]
    private static void Load16(string[] args) {
        LoadInternal(args, false, EBits.BITS_16);
    }

    [TasCommand("FastLoad8", LegalInMainGame = false)]
    private static void FastLoad8(string[] args) {
        LoadInternal(args, true, EBits.BITS_8);
    }

    [TasCommand("FastLoad16", LegalInMainGame = false)]
    private static void FastLoad16(string[] args) {
        LoadInternal(args, true, EBits.BITS_16);
    }

    private static void LoadInternal(string[] args, bool fastLoad, EBits dimension) {
        if (args.Length == 0) {
            ShowToastAndDisableTAS("Load Command Failed\nRequire scene name");
            return;
        }

        string sceneName = args[0];

        if (int.TryParse(sceneName, out int buildIndex)) {
            sceneName = GetSceneName(buildIndex);

            if (sceneName.IsNullOrEmpty()) {
                ShowToastAndDisableTAS("Load Command Failed\nInvalid scene build index");
                return;
            }
        }

        if (CurrentSceneName is "InitialScene" || GameComponent.Instance.IsLoading || Manager<LevelManager>.instance?.levelLoadingInfo != null) {
            ShowToastAndDisableTAS("Load Command Failed\nCannot run during loading");
            return;
        }

        if (CurrentSceneName is "Intro" && sceneName != "Intro" && Manager<SaveManager>.instance?.saveGame == null) {
            ShowToastAndDisableTAS("Load Command Failed\nPlease load save file first");
            return;
        }

        Vector2? position = null;
        if (args.Length >= 3) {
            if (float.TryParse(args[1], out float x) && float.TryParse(args[2], out float y)) {
                position = new Vector2(x, y);
            } else {
                ShowToastAndDisableTAS("Load Command Failed\nCannot parse position");
                return;
            }
        }

        if (sceneName == "Intro") {
            if (CurrentSceneName is "Intro") {
                if (Manager<UIManager>.Instance is { } uiManager) {
                    if (!uiManager.GetView<TitleScreen>() && FindObjectOfType<IntroManager>() is { } introManager &&
                        introManager.introSequence != null) {
                        introManager.OnIntroSkipped();
                    }

                    uiManager.CloseAllScreensOfType<TitleScreen>(false);
                    uiManager.CloseAllScreensOfType<SaveGameSelectionScreen>(false);
                    uiManager.CloseAllScreensOfType<OptionScreen>(false);
                }

                SceneLoader.LoadScene("Intro", LoadSceneMode.Single);
            } else {
                BackToTitleScreen.GoBackToTitleScreen();
            }

            return;
        }

        if (CurrentSceneName is "Intro") {
            if (Manager<UIManager>.Instance is { } uiManager && !uiManager.GetView<TitleScreen>() &&
                FindObjectOfType<IntroManager>() is { } introManager && introManager.introSequence != null) {
                introManager.OnIntroSkipped();
            }
        }

        Manager<PauseManager>.instance?.Resume();

        if (Manager<Level>.instance?.deactivators is { } deactivators) {
            if (deactivators.Contains("InShop")) {
                Manager<UIManager>.instance?.CloseAllScreensOfType<ShopUpgradeScreen>(false);

                if (Manager<UIManager>.instance?.GetView<ShopDialogSelectionScreen>() is { } shopView) {
                    shopView.Close(false);
                }

                if (Manager<Shop>.instance?.shopKeeperInteractionTrigger.GetComponent<ShopkeeperInteractionTrigger>() is { } trigger) {
                    if (FindObjectOfType<ShopKeeperInteractionSequence>() is { } sequence) {
                        trigger.OnSpecialInteractionDone(sequence);
                    }

                    if (FindObjectOfType<ShopChest>() is { } shopChest) {
                        shopChest.OnInteractionDone();
                    }

                    trigger.OnInteractionDone();
                }

                foreach (LeaveShopTrigger leaveShopTrigger in Resources.FindObjectsOfTypeAll<LeaveShopTrigger>()) {
                    leaveShopTrigger.LeaveShop();
                    break;
                }
            } else if (deactivators.Contains("InToTHQ")) {
                Manager<TotHQ>.instance?.LeaveToLevel(false, false);
            }
        }

        if (fastLoad && CurrentSceneName == sceneName) {
            Manager<UIManager>.instance?.CloseAllScreensOfType<PauseScreen>(false);
            Manager<UIManager>.instance?.CloseAllScreensOfType<DialogBox>(false);
            Manager<UIManager>.instance?.CloseAllScreensOfType<AwardItemPopup>(false);
            Manager<UIManager>.instance?.CloseAllScreensOfType<DeathScreen>(false);

            foreach (Checkpoint checkpoint in FindObjectsOfType<Checkpoint>()) {
                checkpoint.isLoadingShop = false;
            }

            if (FindObjectOfType<LeafGolemIntroCutScene>() is { } leafGolemIntroCutScene) {
                leafGolemIntroCutScene.alreadyTriggered = false;
            }

            // modify from Quarble.RespawnPlayer()
            LevelManager levelManager = Manager<LevelManager>.instance;
            PlayerController player = Manager<PlayerManager>.instance?.player;
            levelManager.onCheckpointReloaded += Manager<GameManager>.Instance.OnCheckpointReloaded;
            player.velocity = Vector2.zero;
            player.externalVelocities.Clear();
            player.animator.SetUpdateMode(AnimatorUpdateMode.Normal);
            player.quarble.StopAllCoroutines();
            player.NeedPotion = false;
            player.Unduck();
            player.quarble.totalTimeLeft = 0;
            player.SetHP(player.GetMaxHP());
            player.windmillCharge = 1f;
            Manager<PlayerManager>.instance.PlayerShurikens = Manager<PlayerManager>.Instance.GetMaxShuriken();
            if (Manager<InventoryManager>.instance is { } inventoryManager && inventoryManager.GetItemQuantity(EItems.CHARGED_ATTACK) > 0) {
                player.chargeAttackHandler.chargeProgress = player.chargeAttackHandler.chargeDuration;
                player.chargeAttackHandler.chargeDone = true;
            }

            CheckpointSaveInfo checkpointSaveInfo = Manager<ProgressionManager>.Instance.GetReloadInfo();
            if (position.HasValue) {
                checkpointSaveInfo.loadedLevelPlayerPosition = position.Value;
            }

            levelManager.ReloadCurrentLevel(true, ELevelEntranceID.NONE, null, false, dimension);
        } else {
            Manager<AudioManager>.instance?.StopMusic();
            SafeCloseAllScreens();

            LevelLoadingInfo levelLoadingInfo = new(sceneName, false, true, LoadSceneMode.Single, ELevelEntranceID.NONE, dimension);
            levelLoadingInfo.positionPlayer = position.HasValue;
            if (position.HasValue) {
                if (Manager<ProgressionManager>.instance is { } progressionManager) {
                    if (progressionManager.checkpointSaveInfo == null) {
                        progressionManager.checkpointSaveInfo = new CheckpointSaveInfo {
                            mana = Manager<PlayerManager>.Instance.PlayerShurikens,
                            playerLocationSceneName = sceneName,
                            playerFacingDirection = 1,
                            playerLocationDimension = dimension,
                            loadedLevelPlayerPosition = position.Value,
                            loadedLevelName = sceneName,
                            loadedLevelDimension = dimension,
                        };
                    } else {
                        progressionManager.checkpointSaveInfo.loadedLevelPlayerPosition = position.Value;
                    }
                }
            }

            Manager<LevelManager>.instance?.LoadLevel(levelLoadingInfo);
        }
    }

    private static void SafeCloseAllScreens() {
        if (Manager<UIManager>.instance is not { } uiManager) {
            return;
        }

        List<View> exceptions = new();
        if (uiManager.GetView<SavingScreen>() is { } savingScreen) {
            exceptions.Add(savingScreen);
        }

        uiManager.CloseAllScreens(exceptions);
    }

    private static string GetSceneName(int buildIndex) {
        return buildIndex switch {
            0 => "InitialScene",
            1 => "Loader",
            2 => "Empty",
            3 => "Intro",
            4 => "Shop",
            5 => "Level_13_TowerOfTimeHQ_Build",
            6 => "Level_01_NinjaVillage_Build",
            7 => "Level_02_AutumnHills_Build",
            8 => "Level_03_ForlornTemple_Build",
            9 => "Level_04_Catacombs_Build",
            10 => "Level_04_B_DarkCave_Build",
            11 => "Level_04_C_RiviereTurquoise_Build",
            12 => "Level_05_B_SunkenShrine_Build",
            13 => "Level_05_A_HowlingGrotto_Build",
            14 => "Level_06_A_BambooCreek_Build",
            15 => "Level_07_QuillshroomMarsh_Build",
            16 => "Level_09_A_GlacialPeak_Build",
            17 => "Level_08_SearingCrags_Build",
            18 => "Level_09_B_ElementalSkylands_Build",
            19 => "Level_10_A_TowerOfTime_Build",
            20 => "Level_11_B_MusicBox_Build",
            21 => "Level_11_A_CloudRuins_Build",
            22 => "Level_12_UnderWorld_Build",
            23 => "Level_14_CorruptedFuture_Build",
            24 => "Level_16_Beach_Build",
            25 => "Level_17_Volcano_Build",
            26 => "Level_18_Volcano_Chase_Build",
            27 => "Level_Ending_Build",
            28 => "Level_Ending_PP_Build",
            29 => "Level_15_Surf",
            30 => "Level_Surf_Credits",
            _ => ""
        };
    }
}