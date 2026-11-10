using System;

namespace MobileParadise.Games.Sudoku.Logic
{
    public class SudokuPuzzle
    {
        public readonly int[] Givens;     // starting numbers, 0 = empty
        public readonly int[] Solution;   // the full board

        public SudokuPuzzle(int[] givens, int[] solution)
        {
            Givens = givens;
            Solution = solution;
        }
    }

    /// <summary>
    /// Makes a random full board, then empties cells one by one, keeping only removals
    /// that leave exactly one solution, until the level's number of clues is reached.
    /// </summary>
    public static class SudokuGenerator
    {
        public static SudokuPuzzle Generate(SudokuLevel level, Random random)
        {
            int[] solution = new int[81];
            SudokuSolver.Fill(solution, random);

            int[] givens = (int[])solution.Clone();
            int[] order = new int[81];
            for (int i = 0; i < 81; i++)
            {
                order[i] = i;
            }
            for (int i = 80; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }

            int target = SudokuLevels.Clues(level);
            int clues = 81;
            for (int k = 0; k < 81 && clues > target; k++)
            {
                int cell = order[k];
                int saved = givens[cell];
                givens[cell] = 0;

                if (SudokuSolver.CountSolutions(givens, 2) == 1)
                {
                    clues--;
                }
                else
                {
                    givens[cell] = saved;   // removing it would allow more than one solution
                }
            }

            return new SudokuPuzzle(givens, solution);
        }
    }
}
