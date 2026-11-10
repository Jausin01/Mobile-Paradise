using System.Collections.Generic;

namespace MobileParadise.Games.WordSearch.Logic
{
    /// <summary>
    /// A word in the puzzle: its letters, where it starts, which way it runs, and whether the player found it.
    /// </summary>
    public class PlacedWord
    {
        public string Text;
        public int StartCell;
        public int Dx;
        public int Dy;
        public bool Found;
    }

    /// <summary>
    /// The letter grid and its hidden words. No Unity code here.
    /// A cell is a number: row * size + column. The player marks a straight line of cells;
    /// TryClaim checks if that line spells one of the words (forwards or backwards).
    /// </summary>
    public class WordSearchPuzzle
    {
        private readonly int _size;
        private readonly char[] _letters;
        private readonly List<PlacedWord> _words = new List<PlacedWord>();

        public WordSearchPuzzle(int size, char[] letters, List<PlacedWord> words)
        {
            _size = size;
            _letters = letters;
            _words.AddRange(words);
        }

        public int Size { get { return _size; } }
        public int CellCount { get { return _size * _size; } }
        public int WordCount { get { return _words.Count; } }

        public int FoundCount
        {
            get
            {
                int found = 0;
                for (int i = 0; i < _words.Count; i++)
                {
                    if (_words[i].Found) found++;
                }
                return found;
            }
        }

        public bool AllFound { get { return FoundCount == _words.Count; } }

        public char Letter(int cell)
        {
            return _letters[cell];
        }

        public PlacedWord Word(int index)
        {
            return _words[index];
        }

        /// <summary>
        /// The cells of a word, from its first letter to its last.
        /// </summary>
        public int[] CellsOf(int wordIndex)
        {
            PlacedWord word = _words[wordIndex];
            return Line(word.StartCell, word.Dx, word.Dy, word.Text.Length);
        }

        /// <summary>
        /// The cells of a straight line, or null if the two cells are not in a straight line (horizontal, vertical or diagonal).
        /// </summary>
        public int[] LineBetween(int from, int to)
        {
            int dx = to % _size - from % _size;
            int dy = to / _size - from / _size;
            if (dx != 0 && dy != 0 && System.Math.Abs(dx) != System.Math.Abs(dy)) return null;

            int length = System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy)) + 1;
            return Line(from, System.Math.Sign(dx), System.Math.Sign(dy), length);
        }

        /// <summary>
        /// Checks the line from one cell to another against the words that are not found yet.
        /// Returns the index of the word it spells (and marks it found), or -1.
        /// </summary>
        public int TryClaim(int from, int to)
        {
            int[] line = LineBetween(from, to);
            if (line == null || line.Length < 2) return -1;

            char[] letters = new char[line.Length];
            for (int i = 0; i < line.Length; i++)
            {
                letters[i] = _letters[line[i]];
            }
            string forward = new string(letters);
            System.Array.Reverse(letters);
            string backward = new string(letters);

            int dx = System.Math.Sign(to % _size - from % _size);
            int dy = System.Math.Sign(to / _size - from / _size);

            for (int i = 0; i < _words.Count; i++)
            {
                PlacedWord word = _words[i];
                if (word.Found || word.Text.Length != line.Length) continue;

                if (word.Text == forward)
                {
                    Claim(word, from, dx, dy);
                    return i;
                }
                if (word.Text == backward)
                {
                    Claim(word, to, -dx, -dy);
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// The first cell of a word the player has not found yet (for the hint), or -1 when all are found.
        /// </summary>
        public int HintCell(System.Random random)
        {
            List<int> open = new List<int>();
            for (int i = 0; i < _words.Count; i++)
            {
                if (!_words[i].Found) open.Add(i);
            }
            if (open.Count == 0) return -1;

            return _words[open[random.Next(open.Count)]].StartCell;
        }

        // ---- Helpers ----------------------------------------------------------

        private static void Claim(PlacedWord word, int start, int dx, int dy)
        {
            word.Found = true;
            word.StartCell = start;
            word.Dx = dx;
            word.Dy = dy;
        }

        private int[] Line(int start, int dx, int dy, int length)
        {
            int[] cells = new int[length];
            int column = start % _size;
            int row = start / _size;
            for (int i = 0; i < length; i++)
            {
                cells[i] = (row + dy * i) * _size + (column + dx * i);
            }
            return cells;
        }
    }
}
