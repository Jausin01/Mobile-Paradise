using System;
using System.Collections.Generic;
using MobileParadise.Core.Localization;

namespace MobileParadise.Core.Help
{
    /// <summary>
    /// All "how to play" entries. Each game adds its own (see the files in the Games folder),
    /// so a new game's help is one new file and the help window needs no change.
    /// </summary>
    public static class HelpRegistry
    {
        private static readonly List<HelpEntry> Entries = new List<HelpEntry>();

        /// <summary>
        /// Adds (or replaces) the help of a game. 'nameKey' is the text key of the game's name.
        /// </summary>
        public static void Register(string sceneName, string nameKey, string portuguese, string english)
        {
            string textKey = "help." + sceneName;
            Texts.Set(textKey, portuguese, english);

            Entries.RemoveAll(e => e.SceneName == sceneName);
            Entries.Add(new HelpEntry(sceneName, nameKey, textKey));
        }

        /// <summary>
        /// The help of the game in this scene, or null (the main menu has none).
        /// </summary>
        public static HelpEntry Find(string sceneName)
        {
            return Entries.Find(e => e.SceneName == sceneName);
        }

        /// <summary>
        /// Every entry, sorted by the game's name in the current language.
        /// </summary>
        public static List<HelpEntry> Sorted()
        {
            List<HelpEntry> list = new List<HelpEntry>(Entries);
            list.Sort((a, b) => string.Compare(Texts.Get(a.NameKey), Texts.Get(b.NameKey), StringComparison.CurrentCultureIgnoreCase));
            return list;
        }
    }
}
