using HarmonyLib;
using TAS;
using UnityEngine;

namespace TheMessenger.TAS.Components.Assists;

[HarmonyPatch]
public class MuteInBackground : PluginComponent {
    private float? originalVolume;

    private void Update() {
        if (Manager.FastForwarding && !originalVolume.HasValue) {
            originalVolume = AudioListener.volume;
            AudioListener.volume = 0;
        } else if (Manager.FastForwarding && AudioListener.volume != 0) {
            AudioListener.volume = 0;
        } else if (!Manager.FastForwarding && originalVolume.HasValue) {
            AudioListener.volume = originalVolume.Value;
            originalVolume = null;
        }
    }
}