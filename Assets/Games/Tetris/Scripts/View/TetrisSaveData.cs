using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Tetris.Logic;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// The game in progress, so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class TetrisSaveData
    {
        private const string Key = "tetris_save";

        public bool HasGame;
        public int Level;        // 0 Easy, 1 Medium, 2 Hard
        public int[] Cells;
        public int Type;
        public int Rotation;
        public int X;
        public int Y;
        public int Next;
        public int[] Bag;
        public int Score;
        public int Lines;

        public static TetrisSaveData Load()
        {
            TetrisSaveData data = SaveSystem.Load<TetrisSaveData>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(TetrisGame game, TetrisLevel level)
        {
            TetrisSaveData data = new TetrisSaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.Cells = game.CopyCells();
            data.Type = game.CurrentType;
            data.Rotation = game.Rotation;
            data.X = game.PieceX;
            data.Y = game.PieceY;
            data.Next = game.NextType;
            data.Bag = game.BagRemaining();
            data.Score = game.Score;
            data.Lines = game.Lines;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new TetrisSaveData());
        }

        public TetrisGame ToGame()
        {
            return new TetrisGame(new Random(), Cells, Type, Rotation, X, Y, Next, Bag, Score, Lines);
        }

        private bool IsUsable()
        {
            if (!HasGame || Level < 0 || Level > 2) return false;
            return TetrisGame.IsValid(Cells, Type, Rotation, X, Y, Next, Bag, Score, Lines);
        }
    }

    /// <summary>
    /// The best score of each level, kept apart so a new game does not erase it.
    /// </summary>
    [Serializable]
    public class TetrisBestData
    {
        private const string Key = "tetris_best";

        public int Easy;
        public int Medium;
        public int Hard;

        public static int Load(TetrisLevel level)
        {
            TetrisBestData data = SaveSystem.Load<TetrisBestData>(Key);
            int value = data.Get(level);
            return value < 0 ? 0 : value;
        }

        public static void Save(TetrisLevel level, int best)
        {
            TetrisBestData data = SaveSystem.Load<TetrisBestData>(Key);
            data.Set(level, best);
            SaveSystem.Save(Key, data);
        }

        private int Get(TetrisLevel level)
        {
            switch (level)
            {
                case TetrisLevel.Easy: return Easy;
                case TetrisLevel.Medium: return Medium;
                default: return Hard;
            }
        }

        private void Set(TetrisLevel level, int value)
        {
            switch (level)
            {
                case TetrisLevel.Easy: Easy = value; break;
                case TetrisLevel.Medium: Medium = value; break;
                default: Hard = value; break;
            }
        }
    }
}
