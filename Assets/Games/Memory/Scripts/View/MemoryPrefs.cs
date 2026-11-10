using UnityEngine;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// The card theme the player picked, remembered between sessions.
    /// </summary>
    public static class MemoryPrefs
    {
        private const string ThemeKey = "memory.theme";

        public static int GetTheme(int themeCount)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(ThemeKey, 0), 0, themeCount - 1);
        }

        public static void SaveTheme(int theme)
        {
            PlayerPrefs.SetInt(ThemeKey, theme);
            PlayerPrefs.Save();
        }
    }
}
