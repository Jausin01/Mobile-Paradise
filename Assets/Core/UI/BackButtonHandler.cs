using MobileParadise.Core.Scenes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Android back button (the Input System reports it as the Escape key).
    /// In a game it goes back to the main menu; in the main menu it closes the app.
    /// Does nothing while the settings, music or help window is open.
    /// </summary>
    public class BackButtonHandler : MonoBehaviour
    {
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;
            if (UiPause.IsOpen) return;

            if (SceneManager.GetActiveScene().name == SceneLoader.MenuScene)
            {
                Application.Quit();
            }
            else
            {
                SceneLoader.LoadMenu();
            }
        }
    }
}
