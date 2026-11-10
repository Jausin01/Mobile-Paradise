using System;

namespace MobileParadise.Games.Breakout.Logic
{
    /// <summary>
    /// The brick layouts. Six patterns repeat; every time they come around the bricks get stronger.
    /// A brick is its hit points: 0 none, 1..3 (tougher bricks need more hits).
    /// </summary>
    public static class BreakoutLevels
    {
        public const int Columns = 8;
        public const int Rows = 9;
        private const int PatternCount = 6;

        /// <summary>
        /// The bricks of a level (1, 2, 3 ...), row by row: index = row * Columns + column.
        /// </summary>
        public static int[] Bricks(int level)
        {
            int pattern = (level - 1) % PatternCount;
            int bonus = (level - 1) / PatternCount;
            int[] bricks = new int[Columns * Rows];

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    if (!IsBrick(pattern, row, column)) continue;

                    int strength = 1 + bonus + ((row == 0 && level >= 4) ? 1 : 0);
                    bricks[row * Columns + column] = Math.Min(3, strength);
                }
            }
            return bricks;
        }

        private static bool IsBrick(int pattern, int row, int column)
        {
            int last = Columns - 1;
            switch (pattern)
            {
                case 0: return row < 4;
                case 1: return row < 5 && (row + column) % 2 == 0;
                case 2: return row < 4 && column >= row && column <= last - row;
                case 3: return row < 5 && row % 2 == 0;
                case 4: return row < 5 && (row == 0 || row == 4 || column == 0 || column == last);
                default: return row < 6 && (column == row || column == row + 1 || column == last - row || column == last - row - 1);
            }
        }
    }
}
