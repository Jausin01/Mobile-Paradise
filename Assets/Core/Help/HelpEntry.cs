namespace MobileParadise.Core.Help
{
    /// <summary>
    /// The "how to play" text of one game: which scene it belongs to, the key of the game's name and the key of the explanation.
    /// </summary>
    public class HelpEntry
    {
        public readonly string SceneName;
        public readonly string NameKey;
        public readonly string TextKey;

        public HelpEntry(string sceneName, string nameKey, string textKey)
        {
            SceneName = sceneName;
            NameKey = nameKey;
            TextKey = textKey;
        }
    }
}
