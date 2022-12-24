using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMessenger.TAS.Utils; 

public class UnityUtils {
    public static string[] GetAllScenes() {
        return Enumerable.Range(0, SceneManager.sceneCountInBuildSettings)
            .Select(i => Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i))).ToArray();
    }
    
    public static string[] GetAllLayers() {
        return Enumerable.Range(0, 32).Select(LayerMask.LayerToName).ToArray();
    }
}