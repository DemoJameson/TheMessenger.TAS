using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TAS;
using TAS.Core.Utils;
using TheMessenger.TAS.Utils;
using UnityEngine;

namespace TheMessenger.TAS.Components.Debugs;

public class DisableRunWhenReload : PluginComponent {
    private Type scriptEngineType;
    private string dllPath;
    private DateTime lastWriteTime;
    private bool reloaded;

    private void Awake() {
        scriptEngineType = Type.GetType("ScriptEngine.ScriptEngine, ScriptEngine");
        dllPath = PathEx.Combine(Directory.GetParent(Application.dataPath).ToString(), "BepInEx", "scripts", $"{MyPluginInfo.PLUGIN_GUID}.dll");
        if (scriptEngineType == null || !File.Exists(dllPath)) {
            Destroy(this);
        }

        Task.Run(() => {
            lastWriteTime = File.GetLastWriteTime(dllPath);
            while (!reloaded) {
                Thread.Sleep(500);
                if (FindObjectOfType(scriptEngineType) is { } scriptEngine && lastWriteTime != File.GetLastWriteTime(dllPath)) {
                    reloaded = true;
                    Manager.DisableRun();
                    scriptEngine.InvokeMethod("ReloadPlugins");
                }
            }
        });
    }
}