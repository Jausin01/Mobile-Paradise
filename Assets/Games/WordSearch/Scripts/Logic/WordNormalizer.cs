using System.Text;

namespace MobileParadise.Games.WordSearch.Logic
{
    /// <summary>
    /// Turns a word into what goes on the grid: capital letters A-Z only (no accents, spaces or hyphens).
    /// </summary>
    public static class WordNormalizer
    {
        private const string Accented = "ÁÀÂÃÄÉÈÊËÍÌÎÏÓÒÔÕÖÚÙÛÜÇÑ";
        private const string Plain = "AAAAAEEEEIIIIOOOOOUUUUCN";

        public static string ToGridWord(string word)
        {
            StringBuilder builder = new StringBuilder();
            string upper = word.ToUpperInvariant();

            for (int i = 0; i < upper.Length; i++)
            {
                char letter = upper[i];
                int accent = Accented.IndexOf(letter);
                if (accent >= 0) letter = Plain[accent];

                if (letter >= 'A' && letter <= 'Z') builder.Append(letter);
            }
            return builder.ToString();
        }
    }
}
