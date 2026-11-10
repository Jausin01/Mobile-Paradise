namespace MobileParadise.Games.WordSearch.Logic
{
    /// <summary>
    /// A theme of words for the puzzles, with a word list in each language.
    /// Words may have accents or spaces; they are cleaned up (WordNormalizer) when a puzzle is made.
    /// </summary>
    public class WordCategory
    {
        public readonly string Key;
        public readonly string NamePortuguese;
        public readonly string NameEnglish;
        public readonly string[] Portuguese;
        public readonly string[] English;

        public WordCategory(string key, string namePortuguese, string nameEnglish, string[] portuguese, string[] english)
        {
            Key = key;
            NamePortuguese = namePortuguese;
            NameEnglish = nameEnglish;
            Portuguese = portuguese;
            English = english;
        }

        public string[] Words(bool portuguese)
        {
            return portuguese ? Portuguese : English;
        }
    }
}
