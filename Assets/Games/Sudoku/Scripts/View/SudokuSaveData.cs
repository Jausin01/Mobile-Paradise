using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Sudoku.Logic;

namespace MobileParadise.Games.Sudoku.View
{
    /// <summary>
    /// The game in progress, saved after every change so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class SudokuSaveData
    {
        private const string Key = "sudoku_save";

        public bool HasGame;
        public int Level;        // 0 Easy, 1 Medium, 2 Hard
        public int[] Givens;     // 81 numbers, 0 = empty
        public int[] Solution;
        public int[] Values;     // starting numbers plus what the player filled in
        public int Hints;

        public static SudokuSaveData Load()
        {
            SudokuSaveData data = SaveSystem.Load<SudokuSaveData>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(SudokuGame game, SudokuLevel level)
        {
            SudokuSaveData data = new SudokuSaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.Givens = game.CopyGivens();
            data.Solution = game.CopySolution();
            data.Values = game.CopyValues();
            data.Hints = game.HintsUsed;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new SudokuSaveData());
        }

        public SudokuGame ToGame()
        {
            return new SudokuGame((int[])Givens.Clone(), (int[])Solution.Clone(), (int[])Values.Clone(), Hints);
        }

        private bool IsUsable()
        {
            return HasGame && Level >= 0 && Level <= 2
                && Givens != null && Givens.Length == 81
                && Solution != null && Solution.Length == 81
                && Values != null && Values.Length == 81;
        }
    }
}
