using HarmonyLib;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Components.Helpers;

namespace TheMessenger.TAS.Commands;

[HarmonyPatch]
public class SeedCommand : PluginComponent {
    public static string Seed => seed ?? "";
    private static string seed;

    [TasCommand("Seed", LegalInMainGame = true)]
    private static void SetSeed(string[] args) {
        seed = args.IsEmpty() ? "" : args[0];
    }

    private void Awake() {
        HookHelper.ActiveSceneChanged(() => seed = "");
    }
    
    [HarmonyPatch(typeof(LevelRoom), nameof(LevelRoom.EnterRoom))]
    [HarmonyPostfix]
    private static void LevelRoomEnterRoom(bool teleportedInRoom) {
        seed = "";
    }
}