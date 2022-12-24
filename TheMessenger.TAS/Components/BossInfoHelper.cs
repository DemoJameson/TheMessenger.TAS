using HarmonyLib;
using TAS.Core.Utils;
using TheMessenger.TAS.Components.Helpers;

namespace TheMessenger.TAS.Components;

[HarmonyPatch]
public class BossInfoHelper : PluginComponent {
    private static bool fightStarted;

    private void Awake() {
        HookHelper.ActiveSceneChanged(() => {
            fightStarted = false;
        });
    }

    public static string GetInfo() {
        if (!fightStarted || Manager<ProgressionManager>.instance is not { } progressionManager) {
            return null;
        }

        string state = "";
        string health = "";
        if (CurrentSceneName == "Level_02_AutumnHills_Build" && !progressionManager.HasDefeatedBoss(BossIds.LEAF_GOLEM)) {
            if (Manager<LeafGolemFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.CurrentHP}/{bossInstance.maxHP}";
            }
        } else if (CurrentSceneName == "Level_03_ForlornTemple_Build" && !progressionManager.HasDefeatedBoss(BossIds.DEMON_ARTIFICIER)) {
            if (Manager<DemonKingFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = bossInstance.CurrentHP + "/" + bossInstance.maxHP;
            }
        } else if (CurrentSceneName == "Level_04_Catacombs_Build" && !progressionManager.HasDefeatedBoss(BossIds.NECROMANCER)) {
            if (Manager<NecromancerFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.CurrentHP}/{bossInstance.maxHP}";
            }
        } else if (CurrentSceneName == "Level_05_A_HowlingGrotto_Build" && !progressionManager.HasDefeatedBoss(BossIds.EMERALD_GOLEM)) {
            if (Manager<EmeraldGolemFightManager>.Instance is {bossComponent: { } bossComponent} manager) {
                if (manager.EssenceComponent == null) {
                    state = GetStateName(bossComponent.stateMachine);
                    health = $"B: {bossComponent.CurrentHP}/{bossComponent.maxHP} G: {bossComponent.gemHP}/{bossComponent.gemMaxHP}";
                } else {
                    state = "MovementCoroutine";
                    health = $"E: {manager.EssenceComponent.CurrentHP}/{manager.EssenceComponent.maxHP}";
                }
            }
        } else if (CurrentSceneName == "Level_07_QuillshroomMarsh_Build" && !progressionManager.HasDefeatedBoss(BossIds.LEAF_GOLEM)) {
            if (Manager<QueenOfQuillsFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.CurrentHP}/{bossInstance.maxHP}";
            }
        } else if (CurrentSceneName == "Level_08_SearingCrags_Build" && !progressionManager.HasDefeatedBoss(BossIds.COLOS_SUSSES)) {
            if (Manager<SearingCragsBossFightManager>.Instance is {colossusesInstance: { } bossInstance} manager) {
                if (bossInstance.stateMachine != null) {
                    state = string.Concat(new object[] {
                        state,
                        GetStateName(bossInstance.stateMachine),
                        " C: ",
                        GetStateName(manager.colosInstance.stateMachine),
                        " S: ",
                        GetStateName(manager.susesInstance.stateMachine)
                    });
                    if (manager.colosInstance != null && manager.susesInstance != null) {
                        health =
                            $"{health}C: {manager.colosInstance.CurrentHP}/{manager.colosInstance.maxHP} S: {manager.susesInstance.CurrentHP}/{manager.susesInstance.maxHP}";
                    }
                }
            }
        } else if (CurrentSceneName == "Level_10_A_TowerOfTime_Build" && !progressionManager.HasDefeatedBoss(BossIds.ARCANE_GOLEM)) {
            if (Manager<ArcaneGolemBossFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.head.CurrentHP}/{bossInstance.head.maxHP}";
                health = $"{health}  P2: {bossInstance.secondPhaseStartHP}";
            }
        } else if (CurrentSceneName == "Level_11_A_CloudRuins_Build" && !progressionManager.HasDefeatedBoss(BossIds.MANFRED)) {
            if (Manager<ManfredBossfightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = bossInstance.head.hittable.CurrentHP + "/" +
                         bossInstance.head.hittable.maxHP;
            }
        } else if (CurrentSceneName == "Level_12_UnderWorld_Build" && !progressionManager.HasDefeatedBoss(BossIds.DEMON_GENERAL)) {
            if (Manager<DemonGeneralFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.CurrentHP}/{bossInstance.maxHP}";
            }
        } else if (CurrentSceneName == "Level_04_C_RiviereTurquoise_Build" && !progressionManager.HasDefeatedBoss(BossIds.BUTTERFLY_MATRIARCH)) {
            if (Manager<ButterflyMatriarchFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.CurrentHP}/{bossInstance.maxHP}";
                health = $"{health}  P2: {bossInstance.phase1MaxHP}";
                health = $"{health}, P3: {bossInstance.phase2MaxHP}";
            }
        } else if (CurrentSceneName == "Level_09_B_ElementalSkylands_Build" && !progressionManager.HasDefeatedBoss(BossIds.CLOCKWORK_CONCIERGE)) {
            if (Manager<ConciergeFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = string.Concat(new object[] {
                    state,
                    GetStateName(bossInstance.stateMachine),
                    " B: ",
                    GetStateName(bossInstance.bodyStateMachine),
                    " H: ",
                    GetStateName(bossInstance.headStateMachine)
                });
                health = bossInstance.opened
                    ? $"H: {bossInstance.heart.CurrentHP}/{bossInstance.heart.maxHP}"
                    : $"{health}H: {bossInstance.head.CurrentHP} C: {bossInstance.bodyCanon_1.CurrentHP}|{bossInstance.bodyCanon_2.CurrentHP}|{bossInstance.bodyCanon_3.CurrentHP} T: {bossInstance.sideTrap.CurrentHP}";
            }
        } else if (CurrentSceneName == "Level_11_B_MusicBox_Build") {
            if (Manager<PhantomFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.hittable.CurrentHP}/{bossInstance.hittable.maxHP}";
                health = $"{health}  P2: {bossInstance.moveSequence_2_Threshold * bossInstance.hittable.maxHP}";
                health = $"{health}, P3: {bossInstance.moveSequence_3_Threshold * bossInstance.hittable.maxHP}";
            }
        } else if (CurrentSceneName == "Level_15_Surf") {
            if (Manager<SurfBossManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.hittable.CurrentHP}/{bossInstance.hittable.maxHP}";
                health = $"{health}  P2: {bossInstance.moveSequence_2_Threshold * bossInstance.hittable.maxHP}";
                health = $"{health}, P3: {bossInstance.moveSequence_3_Threshold * bossInstance.hittable.maxHP}";
            }
        } else if (CurrentSceneName == "Level_16_Beach_Build") {
            if (Manager<TotemBossFightManager>.Instance is {bossInstance: { } bossInstance}) {
                state = GetStateName(bossInstance.stateMachine);
                health = $"{bossInstance.CurrentHp}/{bossInstance.maxHP}";
            }
        }

        if (state.IsEmpty() && health.IsEmpty()) {
            return null;
        } else {
            return $"Boss:  {health}  {state}";
        }
    }

    private static string GetStateName(StateMachine stateMachine) {
        return stateMachine.currentState.name.Split(' ', '(')[0];
    }

    [HarmonyPatch(typeof(LeafGolemFightManager), nameof(LeafGolemFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(DemonKingFightManager), nameof(DemonKingFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(NecromancerFightManager), nameof(NecromancerFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(HowlingGrottoBossIntroCutscene), nameof(HowlingGrottoBossIntroCutscene.Play))]
    [HarmonyPatch(typeof(QueenOfQuillsFightManager), nameof(QueenOfQuillsFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(SearingCragsBossFightManager), nameof(SearingCragsBossFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(ArcaneGolemBossFightManager), nameof(ArcaneGolemBossFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(ManfredBossfightManager), nameof(ManfredBossfightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(DemonGeneralFightManager), nameof(DemonGeneralFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(ButterflyMatriarchFightManager), nameof(ButterflyMatriarchFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(ConciergeFightManager), nameof(ConciergeFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(PhantomFightManager), nameof(PhantomFightManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(SurfBossManager), nameof(SurfBossManager.OnIntroCutsceneStart))]
    [HarmonyPatch(typeof(TotemBossFightManager), nameof(TotemBossFightManager.OnIntroCutsceneStart))]
    [HarmonyPrefix]
    private static void OnIntroCutsceneStart() {
        fightStarted = true;
    }
    
    [HarmonyPatch(typeof(LevelManager), nameof(LevelManager.CallOnLevelReload))]
    [HarmonyPrefix]
    private static void LevelManagerCallOnLevelReload() {
        fightStarted = false;
    }
}