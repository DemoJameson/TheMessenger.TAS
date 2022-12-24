using System;
using System.IO;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TAS;
using TAS.Core.Input;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Components.Assists;
using UnityEngine;

namespace TheMessenger.TAS.Commands;

[HarmonyPatch]
public class LoadSaveCommand {
    private const string commandName = "LoadSave";
    private static string path;

    [DisableRun]
    private static void DisableRun() {
        path = null;
    }

    [TasCommand(commandName)]
    private static void LoadSave(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired specify save file path and save slot");
            return;
        }

        if (args.Length == 1) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired specify save slot");
            return;
        }

        string savePath = Path.Combine(Path.GetDirectoryName(InputController.TasFilePath), args[0]);
        if (!File.Exists(savePath)) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nSave file does not exist\n{savePath}");
            return;
        }

        if (!int.TryParse(args[1], out int slot)) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nInvalid save slot");
            return;
        }

        slot--;
        slot = Mathf.Clamp(slot, 0, 2);

        if (Manager<SaveManager>.instance is { } saveManager) {
            path = savePath;
            saveManager.LoadSaveGame();
            saveManager.SelectSaveGameSlot(slot);
            saveManager.LoadSaveSlot(saveManager.GetCurrentSaveGameSlot());
        }
    }

    [HarmonyPatch(typeof(SaveLoadStandalone), nameof(SaveLoadStandalone.Load))]
    [HarmonyILManipulator]
    private static void SaveLoadStandaloneLoad(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        if (ilCursor.TryGotoNext(i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("::ReadAllText"))) {
            ilCursor.EmitDelegate<Func<string, string>>(savePath => path ?? savePath);
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.OnSaveGameLoaded))]
    [HarmonyPrefix]
    private static void SaveManagerOnSaveGameLoaded(SaveGame loadedSaveGame, SaveManager __instance) {
        // keep the current options when loading the save from disk
        if (path != null && __instance.saveGame?.options is {} options) {
            options.language = loadedSaveGame.options.language;
            CustomResolution.GetResolution(out int width, out int height);
            options.resolutionWidth = width;
            options.resolutionHeight = height;
            loadedSaveGame.options = options;
            path = null;
        }
    }
}