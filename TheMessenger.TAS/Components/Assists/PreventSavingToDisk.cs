using System;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace TheMessenger.TAS.Components.Assists; 

[HarmonyPatch]
public class PreventSavingToDisk : PluginComponent {
    private static ConfigEntry<bool> preventSavingToDisk; 
    private static bool prevent => preventSavingToDisk.Value;
    
    private void Awake() {
        preventSavingToDisk = Plugin.Instance.Config.Bind("Assists", "Prevent Saving To Disk", false);
    }

    [HarmonyPatch(typeof(SaveLoadStandalone), nameof(SaveLoadStandalone.Save))]
    [HarmonyILManipulator]
    private static void SaveLoadStandaloneSave(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        if (ilCursor.TryGotoNext(i => i.MatchLdstr("/SaveGame.txt"))) {
            ilCursor.Index++;
            ilCursor.EmitDelegate<Func<string, string>>(fileName => prevent ? "/SaveGame.prevent.txt" : fileName);
            ilCursor.Emit(OpCodes.Ldarg_1).EmitDelegate<Action<SaveGame>>(game => File.WriteAllText(Application.persistentDataPath + (prevent ? "/SaveGame.prevent.json" : "/SaveGame.json"), JsonUtility.ToJson((game, true))));
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.OnSaveGameLoaded))]
    [HarmonyPrefix]
    private static void SaveManagerOnSaveGameLoaded(SaveGame loadedSaveGame, SaveManager __instance) {
        // keep the current options when loading the save from disk
        if (prevent && __instance.saveGame?.options is {} options) {
            loadedSaveGame.options = options;
        }
    }
}