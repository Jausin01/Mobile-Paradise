using System;
using System.Collections.Generic;

namespace MobileParadise.Games.WordSearch.Logic
{
    /// <summary>
    /// Makes a new puzzle every time: picks random words, hides them in random places and directions
    /// (words may cross where letters match), and fills the empty cells with random letters.
    /// </summary>
    public static class WordSearchGenerator
    {
        private const int MinWordLength = 3;
        private const int PlacementTries = 200;
        private const int PuzzleTries = 30;

        /// <summary>
        /// Builds a puzzle from a list of words. Returns null only if the list is too small to fill the level.
        /// </summary>
        public static WordSearchPuzzle Generate(List<string> candidateWords, WordSearchLevel level, Random random)
        {
            int size = WordSearchLevels.Size(level);
            int wanted = WordSearchLevels.WordCount(level);
            int[][] directions = WordSearchLevels.Directions(level);

            List<string> pool = CleanWords(candidateWords, size);
            if (pool.Count < wanted) return null;

            for (int attempt = 0; attempt < PuzzleTries; attempt++)
            {
                WordSearchPuzzle puzzle = TryGenerate(pool, size, wanted, directions, random);
                if (puzzle != null) return puzzle;
            }
            return null;
        }

        // ---- Steps ------------------------------------------------------------

        private static List<string> CleanWords(List<string> words, int size)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < words.Count; i++)
            {
                string word = WordNormalizer.ToGridWord(words[i]);
                if (word.Length >= MinWordLength && word.Length <= size && !result.Contains(word))
                {
                    result.Add(word);
                }
            }
            return result;
        }

        private static WordSearchPuzzle TryGenerate(List<string> pool, int size, int wanted, int[][] directions, Random random)
        {
            List<string> shuffled = new List<string>(pool);
            Shuffle(shuffled, random);

            char[] letters = new char[size * size];
            List<PlacedWord> placed = new List<PlacedWord>();

            // Take words in random order, but place the long ones first: they are the hardest to fit.
            List<string> chosen = shuffled.GetRange(0, wanted);
            chosen.Sort((a, b) => b.Length.CompareTo(a.Length));

            for (int i = 0; i < chosen.Count; i++)
            {
                PlacedWord word = PlaceWord(chosen[i], letters, size, directions, random);
                if (word == null) return null;
                placed.Add(word);
            }

            FillEmptyCells(letters, random);
            Shuffle(placed, random);   // the word list is not in length order
            return new WordSearchPuzzle(size, letters, placed);
        }

        private static PlacedWord PlaceWord(string text, char[] letters, int size, int[][] directions, Random random)
        {
            for (int attempt = 0; attempt < PlacementTries; attempt++)
            {
                int[] direction = directions[random.Next(directions.Length)];
                int dx = direction[0];
                int dy = direction[1];

                int column = random.Next(size);
                int row = random.Next(size);
                if (!Fits(text, letters, size, column, row, dx, dy)) continue;

                for (int i = 0; i < text.Length; i++)
                {
                    letters[(row + dy * i) * size + (column + dx * i)] = text[i];
                }

                PlacedWord word = new PlacedWord();
                word.Text = text;
                word.StartCell = row * size + column;
                word.Dx = dx;
                word.Dy = dy;
                return word;
            }
            return null;
        }

        // The word stays inside the grid and only meets letters that are the same as its own.
        private static bool Fits(string text, char[] letters, int size, int column, int row, int dx, int dy)
        {
            int lastColumn = column + dx * (text.Length - 1);
            int lastRow = row + dy * (text.Length - 1);
            if (lastColumn < 0 || lastColumn >= size || lastRow < 0 || lastRow >= size) return false;

            for (int i = 0; i < text.Length; i++)
            {
                char existing = letters[(row + dy * i) * size + (column + dx * i)];
                if (existing != '\0' && existing != text[i]) return false;
            }
            return true;
        }

        private static void FillEmptyCells(char[] letters, Random random)
        {
            for (int i = 0; i < letters.Length; i++)
            {
                if (letters[i] == '\0') letters[i] = (char)('A' + random.Next(26));
            }
        }

        private static void Shuffle<T>(List<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
