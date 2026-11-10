using UnityEngine.SceneManagement;

namespace MobileParadise.Core.Scenes
{
    /// <summary>
    /// Single place to change scenes. Every "Back to menu" button calls LoadMenu().
    /// </summary>
    public static class SceneLoader
    {
        public const string MenuScene = "Main_Menu";

        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadMenu()
        {
            SceneManager.LoadScene(MenuScene);
        }
    }
}
