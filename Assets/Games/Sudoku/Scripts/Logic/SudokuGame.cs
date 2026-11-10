namespace MobileParadise.Games.Sudoku.Logic
{
    public enum PlaceResult
    {
        Ignored = 0,   // a starting number cannot be changed
        Correct = 1,
        Wrong = 2,
        Cleared = 3
    }

    /// <summary>
    /// One Sudoku game: starting numbers, the player's numbers and the solution.
    /// A number is "wrong" when it differs from the solution (the screen shows it in red right away).
    /// </summary>
    public class SudokuGame
    {
        private readonly int[] _givens;
        private readonly int[] _solution;
        private readonly int[] _values;

        public int HintsUsed { get; private set; }

        public SudokuGame(SudokuPuzzle puzzle)
        {
            _givens = puzzle.Givens;
            _solution = puzzle.Solution;
            _values = (int[])puzzle.Givens.Clone();
        }

        /// <summary>
        /// Rebuilds a game that was saved in the middle.
        /// </summary>
        public SudokuGame(int[] givens, int[] solution, int[] values, int hintsUsed)
        {
            _givens = givens;
            _solution = solution;
            _values = values;
            HintsUsed = hintsUsed;
        }

        public int[] CopyGivens()
        {
            return (int[])_givens.Clone();
        }

        public int[] CopySolution()
        {
            return (int[])_solution.Clone();
        }

        public int[] CopyValues()
        {
            return (int[])_values.Clone();
        }

        public int ValueAt(int index)
        {
            return _values[index];
        }

        public bool IsGiven(int index)
        {
            return _givens[index] != 0;
        }

        public bool IsWrong(int index)
        {
            return _values[index] != 0 && _values[index] != _solution[index];
        }

        /// <summary>
        /// A cell the player can still work on: not a starting number, and empty or wrong.
        /// </summary>
        public bool NeedsWork(int index)
        {
            return !IsGiven(index) && _values[index] != _solution[index];
        }

        public PlaceResult Place(int index, int digit)
        {
            if (IsGiven(index)) return PlaceResult.Ignored;

            _values[index] = digit;
            return digit == _solution[index] ? PlaceResult.Correct : PlaceResult.Wrong;
        }

        public PlaceResult Clear(int index)
        {
            if (IsGiven(index)) return PlaceResult.Ignored;

            _values[index] = 0;
            return PlaceResult.Cleared;
        }

        /// <summary>
        /// Fills the cell with the right number.
        /// </summary>
        public bool Hint(int index)
        {
            if (!NeedsWork(index)) return false;

            _values[index] = _solution[index];
            HintsUsed++;
            return true;
        }

        public bool IsComplete
        {
            get
            {
                for (int i = 0; i < 81; i++)
                {
                    if (_values[i] != _solution[i]) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// How many cells currently hold this digit (right or wrong).
        /// </summary>
        public int CountOf(int digit)
        {
            int count = 0;
            for (int i = 0; i < 81; i++)
            {
                if (_values[i] == digit) count++;
            }
            return count;
        }

        /// <summary>
        /// First cell that still needs work, or -1.
        /// </summary>
        public int FindCellNeedingWork()
        {
            for (int i = 0; i < 81; i++)
            {
                if (NeedsWork(i)) return i;
            }
            return -1;
        }
    }
}
