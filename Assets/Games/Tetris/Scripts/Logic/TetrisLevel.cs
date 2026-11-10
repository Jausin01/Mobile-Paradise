using System;

namespace MobileParadise.Games.Tetris.Logic
{
    public enum TetrisLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// How fast the pieces fall at each difficulty. The wait between steps starts at StartSeconds and
    /// shrinks by the Factor every 10 lines, but never below MinSeconds. Tune the numbers here.
    /// </summary>
    public static class TetrisLevels
    {
        public const int LinesPerLevel = 10;

        public static float StartSeconds(TetrisLevel level)
        {
            switch (level)
            {
                case TetrisLevel.Easy: return 1.0f;
                case TetrisLevel.Medium: return 0.8f;
                default: return 0.6f;
            }
        }

        private static float MinSeconds(TetrisLevel level)
        {
            switch (level)
            {
                case TetrisLevel.Easy: return 0.35f;
                case TetrisLevel.Medium: return 0.15f;
                default: return 0.08f;
            }
        }

        private static float Factor(TetrisLevel level)
        {
            switch (level)
            {
                case TetrisLevel.Easy: return 0.92f;
                case TetrisLevel.Medium: return 0.88f;
                default: return 0.85f;
            }
        }

        /// <summary>
        /// Seconds between automatic steps down, after 'lines' lines were cleared.
        /// </summary>
        public static float StepSeconds(TetrisLevel level, int lines)
        {
            int steps = lines / LinesPerLevel;
            float seconds = StartSeconds(level) * (float)Math.Pow(Factor(level), steps);
            return Math.Max(seconds, MinSeconds(level));
        }

        /// <summary>
        /// Only the Easy level shows where the piece will land.
        /// </summary>
        public static bool ShowGhost(TetrisLevel level)
        {
            return level == TetrisLevel.Easy;
        }
    }
}
