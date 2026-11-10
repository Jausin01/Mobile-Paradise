namespace MobileParadise.Games.Sudoku.Logic
{
    public enum SudokuLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// How many numbers are already filled in at the start (out of 81). More = easier.
    /// </summary>
    public static class SudokuLevels
    {
        public static int Clues(SudokuLevel level)
        {
            switch (level)
            {
                case SudokuLevel.Easy: return 42;
                case SudokuLevel.Medium: return 34;
                default: return 28;
            }
        }
    }
}
