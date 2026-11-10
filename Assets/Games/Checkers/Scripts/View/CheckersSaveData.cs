using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Checkers.Logic;

namespace MobileParadise.Games.Checkers.View
{
    /// <summary>
    /// The game in progress, saved after every move so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class CheckersSaveData
    {
        private const string Key = "checkers_save";

        public bool HasGame;
        public int Difficulty;   // 0 Easy, 1 Medium, 2 Hard
        public int[] Cells;
        public int Turn;         // 0 White (the player), 1 Black (the computer)
        public int QuietPlies;

        public static CheckersSaveData Load()
        {
            CheckersSaveData data = SaveSystem.Load<CheckersSaveData>(Key);
            bool usable = data.HasGame && data.Difficulty >= 0 && data.Difficulty <= 2
                && (data.Turn == 0 || data.Turn == 1) && data.QuietPlies >= 0
                && CheckersBoard.IsValid(data.Cells);
            return usable ? data : null;
        }

        public static void Save(CheckersGame game, CheckersDifficulty difficulty)
        {
            CheckersSaveData data = new CheckersSaveData();
            data.HasGame = true;
            data.Difficulty = (int)difficulty;
            data.Cells = game.Board.CopyCells();
            data.Turn = (int)game.Turn;
            data.QuietPlies = game.QuietPlies;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new CheckersSaveData());
        }

        public CheckersGame ToGame()
        {
            return new CheckersGame(Cells, (CheckersSide)Turn, QuietPlies);
        }
    }
}
