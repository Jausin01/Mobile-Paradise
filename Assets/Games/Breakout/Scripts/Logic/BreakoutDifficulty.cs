namespace MobileParadise.Games.Breakout.Logic
{
    public enum BreakoutDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// Paddle size and ball speed of each difficulty (field units, see BreakoutWorld). Change the numbers here to tune it.
    /// </summary>
    public static class BreakoutSettings
    {
        public const int StartLives = 3;

        public static float PaddleWidth(BreakoutDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BreakoutDifficulty.Easy: return 280f;
                case BreakoutDifficulty.Medium: return 220f;
                default: return 170f;
            }
        }

        /// <summary>Ball speed in field units per second, at level 1.</summary>
        public static float BallSpeed(BreakoutDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BreakoutDifficulty.Easy: return 620f;
                case BreakoutDifficulty.Medium: return 760f;
                default: return 900f;
            }
        }
    }
}
