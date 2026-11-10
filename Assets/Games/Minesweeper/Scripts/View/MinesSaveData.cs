using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Minesweeper.Logic;

namespace MobileParadise.Games.Minesweeper.View
{
    /// <summary>
    /// The game in progress, saved after every tap so "Continue" works even if the app is closed.
    /// Only games that already started (the first cell was opened) are saved.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class MinesSaveData
    {
        private const string Key = "minesweeper_save";

        public bool HasGame;
        public int Level;          // 0 Easy, 1 Medium, 2 Hard
        public bool[] Mines;
        public bool[] Revealed;
        public bool[] Flagged;

        public static MinesSaveData Load()
        {
            MinesSaveData data = SaveSystem.Load<MinesSaveData>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(MinesBoard board, MinesLevel level)
        {
            MinesSaveData data = new MinesSaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.Mines = board.CopyMines();
            data.Revealed = board.CopyRevealed();
            data.Flagged = board.CopyFlagged();
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new MinesSaveData());
        }

        public MinesBoard ToBoard()
        {
            MinesLevel level = (MinesLevel)Level;
            return new MinesBoard(MinesLevels.Columns(level), MinesLevels.Rows(level), Mines, Revealed, Flagged);
        }

        private bool IsUsable()
        {
            if (!HasGame || Level < 0 || Level > 2) return false;

            MinesLevel level = (MinesLevel)Level;
            int cells = MinesLevels.Columns(level) * MinesLevels.Rows(level);
            return Mines != null && Mines.Length == cells
                && Revealed != null && Revealed.Length == cells
                && Flagged != null && Flagged.Length == cells;
        }
    }
}
