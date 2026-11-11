using MobileParadise.Core.Scenes;
using MobileParadise.Core.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobileParadise.Core.Performance
{
    /// <summary>
    /// Runs once when the app starts (no scene object needed) and sets the runtime
    /// performance rules: low frame rate for still games (less heat and battery use),
    /// 60 fps only for games with continuous motion, screen kept on while playing,
    /// and the Android back button handler.
    /// </summary>
    public static class PerformanceBootstrap
    {
        private const int SmoothFrameRate = 60;
        private const int StillFrameRate = 30;

        // Scenes with continuous movement. Every other scene only changes when the player taps.
        private static readonly HashSet<string> SmoothScenes = new HashSet<string>
        {
            "Snake_Game", "Breakout_Game", "Tetris_Game", "Game2048_Game"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // On Android vsync overrides targetFrameRate, so vsync must be off for the 60 to count.
            QualitySettings.vSyncCount = 0;
            ApplyFrameRate(SceneManager.GetActiveScene().name);

            GameObject handler = new GameObject("BackButtonHandler");
            handler.AddComponent<BackButtonHandler>();
            Object.DontDestroyOnLoad(handler);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void ApplyFrameRate(string sceneName)
        {
            Application.targetFrameRate = SmoothScenes.Contains(sceneName) ? SmoothFrameRate : StillFrameRate;
        }

        // The menu follows the phone's own screen-off time; during a game the screen stays on.
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyFrameRate(scene.name);
            Screen.sleepTimeout = scene.name == SceneLoader.MenuScene
                ? SleepTimeout.SystemSetting
                : SleepTimeout.NeverSleep;
        }
    }
}
