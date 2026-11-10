using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Snake.Logic;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// The game in progress, so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class SnakeSaveData
    {
        private const string Key = "snake_save";

        public bool HasGame;
        public int Level;        // 0 Easy, 1 Medium, 2 Hard
        public int[] Body;       // head first
        public int Direction;
        public int Food;
        public int Score;

        public static SnakeSaveData Load()
        {
            SnakeSaveData data = SaveSystem.Load<SnakeSaveData>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(SnakeGame game, SnakeLevel level)
        {
            SnakeSaveData data = new SnakeSaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.Body = game.CopyBody();
            data.Direction = (int)game.Direction;
            data.Food = game.Food;
            data.Score = game.Score;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new SnakeSaveData());
        }

        public SnakeGame ToGame()
        {
            SnakeLevel level = (SnakeLevel)Level;
            return new SnakeGame(SnakeLevels.Columns(level), SnakeLevels.Rows(level), SnakeLevels.Wrap(level),
                Body, (SnakeDirection)Direction, Food, Score, new Random());
        }

        private bool IsUsable()
        {
            if (!HasGame || Level < 0 || Level > 2) return false;

            SnakeLevel level = (SnakeLevel)Level;
            return SnakeGame.IsValid(SnakeLevels.Columns(level), SnakeLevels.Rows(level), Body, Direction, Food, Score);
        }
    }

    /// <summary>
    /// The best score of each level, kept apart so a new game does not erase it.
    /// </summary>
    [Serializable]
    public class SnakeBestData
    {
        private const string Key = "snake_best";

        public int Easy;
        public int Medium;
        public int Hard;

        public static int Load(SnakeLevel level)
        {
            SnakeBestData data = SaveSystem.Load<SnakeBestData>(Key);
            return NotNegative(data.Get(level));
        }

        public static void Save(SnakeLevel level, int best)
        {
            SnakeBestData data = SaveSystem.Load<SnakeBestData>(Key);
            data.Set(level, best);
            SaveSystem.Save(Key, data);
        }

        private int Get(SnakeLevel level)
        {
            switch (level)
            {
                case SnakeLevel.Easy: return Easy;
                case SnakeLevel.Medium: return Medium;
                default: return Hard;
            }
        }

        private void Set(SnakeLevel level, int value)
        {
            switch (level)
            {
                case SnakeLevel.Easy: Easy = value; break;
                case SnakeLevel.Medium: Medium = value; break;
                default: Hard = value; break;
            }
        }

        private static int NotNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
