using UnityEngine;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Adds the settings gear on top of every scene, once, when the game starts.
    /// The prefab is built by the Editor command "Mobile Paradise > 4. Build Settings Overlay".
    /// </summary>
    public static class SettingsOverlayBootstrap
    {
        private const string PrefabName = "SettingsOverlay";   // Assets/Core/Resources/SettingsOverlay.prefab

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            GameObject prefab = Resources.Load<GameObject>(PrefabName);
            if (prefab == null)
            {
                Debug.LogWarning("SettingsOverlay prefab not found. Run 'Mobile Paradise > 4. Build Settings Overlay'.");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = PrefabName;
            Object.DontDestroyOnLoad(instance);
        }
    }
}
