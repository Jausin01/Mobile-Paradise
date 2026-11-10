namespace MobileParadise.Games.Sudoku.Logic
{
    /// <summary>
    /// Backtracking solver. Used to fill a full board and to check that a puzzle has exactly one solution.
    /// Cells are 81 ints (row-major, 0 = empty). Always picks the empty cell with the fewest candidates.
    /// </summary>
    public static class SudokuSolver
    {
        private const int AllDigits = 0x3FE;   // bits 1..9

        /// <summary>
        /// Counts solutions, stopping at 'limit' (use 2 to ask "is it unique?").
        /// The board is restored before returning.
        /// </summary>
        public static int CountSolutions(int[] cells, int limit)
        {
            int[] rows = new int[9];
            int[] columns = new int[9];
            int[] boxes = new int[9];

            for (int i = 0; i < 81; i++)
            {
                if (cells[i] == 0) continue;
                int bit = 1 << cells[i];
                rows[i / 9] |= bit;
                columns[i % 9] |= bit;
                boxes[BoxOf(i)] |= bit;
            }

            return Search(cells, rows, columns, boxes, limit, null);
        }

        /// <summary>
        /// Fills the board completely, trying digits in random order. Needs a board without contradictions.
        /// </summary>
        public static bool Fill(int[] cells, System.Random random)
        {
            int[] rows = new int[9];
            int[] columns = new int[9];
            int[] boxes = new int[9];

            for (int i = 0; i < 81; i++)
            {
                if (cells[i] == 0) continue;
                int bit = 1 << cells[i];
                rows[i / 9] |= bit;
                columns[i % 9] |= bit;
                boxes[BoxOf(i)] |= bit;
            }

            return Search(cells, rows, columns, boxes, 1, random) == 1;
        }

        public static int BoxOf(int index)
        {
            return (index / 27) * 3 + (index % 9) / 3;
        }

        // With 'random' set it stops at the first solution and keeps it in 'cells'; without it, it only counts.
        private static int Search(int[] cells, int[] rows, int[] columns, int[] boxes, int limit, System.Random random)
        {
            int best = -1;
            int bestMask = 0;
            int bestCount = 10;

            for (int i = 0; i < 81; i++)
            {
                if (cells[i] != 0) continue;

                int used = rows[i / 9] | columns[i % 9] | boxes[BoxOf(i)];
                int mask = AllDigits & ~used;
                int count = CountBits(mask);

                if (count == 0) return 0;   // dead end
                if (count < bestCount)
                {
                    best = i;
                    bestMask = mask;
                    bestCount = count;
                    if (count == 1) break;
                }
            }

            if (best < 0) return 1;   // no empty cell left: one solution found

            int[] digits = new int[bestCount];
            int n = 0;
            for (int d = 1; d <= 9; d++)
            {
                if ((bestMask & (1 << d)) != 0) digits[n++] = d;
            }
            if (random != null) Shuffle(digits, random);

            int found = 0;
            for (int k = 0; k < digits.Length; k++)
            {
                int digit = digits[k];
                int bit = 1 << digit;

                cells[best] = digit;
                rows[best / 9] |= bit;
                columns[best % 9] |= bit;
                boxes[BoxOf(best)] |= bit;

                found += Search(cells, rows, columns, boxes, limit - found, random);

                if (random != null && found > 0) return found;   // keep the filled board

                cells[best] = 0;
                rows[best / 9] &= ~bit;
                columns[best % 9] &= ~bit;
                boxes[BoxOf(best)] &= ~bit;

                if (found >= limit) return found;
            }
            return found;
        }

        private static int CountBits(int value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }
            return count;
        }

        private static void Shuffle(int[] items, System.Random random)
        {
            for (int i = items.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }
}
