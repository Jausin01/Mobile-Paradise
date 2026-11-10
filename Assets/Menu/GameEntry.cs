using System;
using UnityEngine;

namespace MobileParadise.Menu
{
    /// <summary>
    /// One game listed in the main menu.
    /// TitleKey is the text key of the translated name (e.g. "game.chess");
    /// if empty or not registered, DisplayName is shown as it is.
    /// </summary>
    [Serializable]
    public class GameEntry
    {
        public string DisplayName;
        public string TitleKey;
        public string SceneName;
        public Sprite Icon;
    }
}
