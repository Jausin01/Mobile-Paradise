using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Game2048.Logic;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// The game in progress, saved after every move so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class Save2048
    {
        private const string Key = "game2048_save";

        public bool HasGame;
        public int[] Cells;
        public int Score;
        public bool WinShown;   // the "you made 2048" message was already shown for this game

        public static Save2048 Load()
        {
            Save2048 data = SaveSystem.Load<Save2048>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(Board2048 board, bool winShown)
        {
            Save2048 data = new Save2048();
            data.HasGame = true;
            data.Cells = board.CopyCells();
            data.Score = board.Score;
            data.WinShown = winShown;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new Save2048());
        }

        public Board2048 ToBoard()
        {
            return new Board2048(Cells, Score, new Random());
        }

        private bool IsUsable()
        {
            return HasGame && Score >= 0 && Board2048.IsValid(Cells);
        }
    }

    /// <summary>
    /// The best score ever, kept apart so a new game does not erase it.
    /// </summary>
    [Serializable]
    public class Best2048
    {
        private const string Key = "game2048_best";

        public int Best;

        public static int Load()
        {
            Best2048 data = SaveSystem.Load<Best2048>(Key);
            return data.Best < 0 ? 0 : data.Best;
        }

        public static void Save(int best)
        {
            Best2048 data = new Best2048();
            data.Best = best;
            SaveSystem.Save(Key, data);
        }
    }
}
