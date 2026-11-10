using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Breakout.Logic;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// Where the player is, saved when a level starts so "Continue" picks up from that level
    /// (with the score and lives the player had then). Load() returns null when there is nothing usable.
    /// </summary>
    [Serializable]
    public class BreakoutSaveData
    {
        private const string Key = "breakout_save";

        public bool HasGame;
        public int Difficulty;   // 0 Easy, 1 Medium, 2 Hard
        public int Level;
        public int Score;
        public int Lives;

        public static BreakoutSaveData Load()
        {
            BreakoutSaveData data = SaveSystem.Load<BreakoutSaveData>(Key);
            bool usable = data.HasGame && data.Difficulty >= 0 && data.Difficulty <= 2
                && data.Level >= 1 && data.Score >= 0 && data.Lives >= 1;
            return usable ? data : null;
        }

        public static void Save(BreakoutDifficulty difficulty, int level, int score, int lives)
        {
            BreakoutSaveData data = new BreakoutSaveData();
            data.HasGame = true;
            data.Difficulty = (int)difficulty;
            data.Level = level;
            data.Score = score;
            data.Lives = lives;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new BreakoutSaveData());
        }
    }
}
