using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using TheMessenger.TAS.Commands;
using TheMessenger.TAS.Components.Helpers;
using TheMessenger.TAS.Utils;
using UnityEngine;

namespace TheMessenger.TAS.Components;

[HarmonyPatch]
public class GameInfoHelper : PluginComponent {
    private static ConfigEntry<int> decimalsConfig;

    private static Vector3? lastPlayerPosition;
    private static string lastLevelName;
    private static string lastTime;
    private static string lastInfo;
    private static readonly List<string> infos = new();
    private static readonly List<string> statuses = new();
    private static string seed => SeedCommand.Seed == "" ? "" : $" Seed: {SeedCommand.Seed}";

    public static float RoomTime { get; private set; }
    public static float LevelTime { get; private set; }
    public static string FormattedRoomTime => FormatTime(RoomTime);
    private static string lastFormattedRoomTime;

    [HarmonyPatch(typeof(LevelRoom), nameof(LevelRoom.EnterRoom))]
    [HarmonyPostfix]
    private static void LevelRoomEnterRoom(bool teleportedInRoom) {
        if (!teleportedInRoom && RoomTime > 1.64f) {
            lastFormattedRoomTime = $" {FormattedRoomTime}";
        }

        RoomTime = 0f;
    }

    private void Awake() {
        decimalsConfig = Plugin.Instance.Config.Bind("Info", "Info Decimals", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 15)));

        HookHelper.ActiveSceneChanged(() => {
            lastPlayerPosition = null;
            lastLevelName = null;
            lastTime = null;
            lastInfo = null;
            LevelTime = 0;
        });
    }

    private void Update() {
        RoomTime += Time.deltaTime;
        LevelTime += Time.deltaTime;
    }

    public static string Info {
        get {
            GameComponent gameComponent = GameComponent.Instance;

            if (lastTime == gameComponent.CurrentTime) {
                return lastInfo;
            }

            if (Manager<PlayerManager>.instance is {player: { } player} playerManager) {
                infos.Clear();
                statuses.Clear();

                Vector3 position = player.transform.position;
                infos.Add($"Pos:   {position.ToSimpleString(decimalsConfig.Value)}");
                infos.Add($"Speed: {(player.velocity).ToSimpleString(decimalsConfig.Value)}");

                if (player.GetTotalExternalVelocity() != Vector2.zero) {
                    infos.Add($"External: {(player.GetTotalExternalVelocity()).ToSimpleString(decimalsConfig.Value)} {player.externalVelocities.Keys.Join()}");
                }

                lastPlayerPosition ??= position;
                infos.Add($"Vel:   {((position - lastPlayerPosition.Value) * GameComponent.FixedFrameRate).ToSimpleString(decimalsConfig.Value)}");
                lastPlayerPosition = position;

                bool windmill = Manager<ProgressionManager>.instance?.useWindmillShuriken == true;
                infos.Add($"HP: {player.currentHp}  Mana: {(windmill ? player.windmillCharge.ToString("F2") : playerManager.playerMana)}  TimeShard: {Manager<InventoryManager>.instance.GetItemQuantity(EItems.TIME_SHARD)}");

                // if (player.stateMachine.currentState is { } state) {
                    // infos.Add($"State: {FormatState(state)}");
                // }

                if (BossInfoHelper.GetInfo() is { } bossInfo) {
                    infos.Add(bossInfo);
                }

                if (player.hasDied) {
                    statuses.Add("Dead");
                } else if (player.IsInvincible()) {
                    statuses.Add("Invincible");
                }

                if (player.stateMachine.currentState is PlayerCinematicState) {
                    statuses.Add("Cutscene");
                } else if (player.paused || player.inputBlocked ||
                           Manager<InputManager>.instance is {blockAllInputs: true}) {
                    statuses.Add("NoControl");
                } else {
                    if (player.CanJump()) {
                        statuses.Add("Jump");
                    } else if (TryAllowWallJump(player, out string direction)) {
                        statuses.Add($"WallJump{direction}");
                    }

                    if (player.CanGlide()) {
                        statuses.Add("Glide");
                    }

                    if (player.CanAttack()) {
                        statuses.Add("Attack");
                    }

                    if (player.graplou.CanThrow()) {
                        statuses.Add("Rope");
                    }

                    if (player.stateMachine.currentState is PlayerDefaultState {allowWallJumpTolerance: false, wallJumpToleranceDelayCoroutine: null} && Manager<InventoryManager>.instance is {} inventoryManager && inventoryManager.GetItemQuantity(EItems.CLIMBING_CLAWS) > 0) {
                        statuses.Add("Unclimbed");
                    }
                }

                infos.Add(statuses.Join(delimiter: " "));
                infos.Add($"{gameComponent.CurrentTime}{lastFormattedRoomTime}");
                infos.Add($"{FormatTime(LevelTime)}{seed}");
                infos.Add($"{CurrentSceneIndex} {gameComponent.LevelName}");

                lastTime = gameComponent.CurrentTime;
                return lastInfo = infos.Join(delimiter: "\n");
            } else {
                return lastInfo = $"{CurrentSceneIndex} {gameComponent.LevelName}";
            }
        }
    }

    private static bool TryAllowWallJump(PlayerController player, out string direction) {
        direction = null;
        if (Manager<InventoryManager>.Instance.GetItemQuantity(EItems.CLIMBING_CLAWS) <= 0) {
            return false;
        }

        if (player.stateMachine.currentState is not PlayerDefaultState state) {
            return false;
        }

        if (!state.allowWallJumpTolerance) {
            return false;
        }

        float horizontalTolerance = 0.7f;
        player.TestWallProbes(player.LookDirection, 0f, 0f, horizontalTolerance, 0.5f, 0.5f, testWallKickProbe: true);
        if (player.BottomWallProbeTestResult.success && (player.TopWallProbeTestResult.success || player.SingleTileWallKickProbeTestResult.success)) {
            direction = player.LookDirection == 1 ? "R" : "L";
            return true;
        }

        player.TestWallProbes(-player.LookDirection, 0f, 0f, horizontalTolerance, 0.5f, 0.5f, testWallKickProbe: true);
        if (player.BottomWallProbeTestResult.success && (player.TopWallProbeTestResult.success || player.SingleTileWallKickProbeTestResult.success)) {
            direction = -player.LookDirection == 1 ? "R" : "L";
            return true;
        }

        return false;
    }

    public static string FormatTime(float time) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string formatted =
            $"{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}.{timeSpan.Milliseconds.ToString().PadLeft(3, '0')}";
        return $"{formatted}({time.ToCeilingFrames()})";
    }

    private static string FormatState(StateMachineState state) {
        return state switch {
            PlayerCinematicState => "Cinematic",
            PlayerDefaultState => "Default",
            PlayerDisembarkSkySerpentState => "DisembarkSkySerpent",
            PlayerEnterCheckpointState => "EnterCheckpoint",
            PlayerEnterShopState => "EnterShop",
            PlayerGlideState => "Glide",
            PlayerGrabbedByPlantState => "GrabbedByPlant",
            PlayerHookPullState => "HookPull",
            PlayerInWaterState => "InWater",
            PlayerKnockbackState => "Knockback",
            PlayerLeaveCheckpointState => "LeaveCheckpoint",
            PlayerLeaveShopState => "LeaveShop",
            PlayerQuillshroomState => "Quillshroom",
            PlayerRespawnState => "Respawn",
            PlayerRidingBossRocketState => "RidingBossRocket",
            PlayerRidingRocketState => "RidingRocket",
            PlayerRidingSkySerpentState => "RidingSkySerpent",
            PlayerShotByPlantState => "ShotByPlant",
            _ => state.GetType().Name
        };
    }
}