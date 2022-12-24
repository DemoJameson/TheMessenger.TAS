using UnityEngine;

namespace TheMessenger.TAS.Components.Assists;

public class RunInBackground : PluginComponent {
    private void Update() {
        Application.runInBackground = true;
    }
}