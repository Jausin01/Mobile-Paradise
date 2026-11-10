namespace MobileParadise.Games.Snake.Logic
{
    public enum SnakeLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// Board size, speed and walls of each level. Change the numbers here to tune the difficulty.
    /// Easy: the snake comes out on the other side of the board. Medium / Hard: walls hurt.
    /// </summary>
    public static class SnakeLevels
    {
        public static int Columns(SnakeLevel level)
        {
            return level == SnakeLevel.Easy ? 10 : 12;
        }

        public static int Rows(SnakeLevel level)
        {
            return level == SnakeLevel.Easy ? 14 : 16;
        }

        public static bool Wrap(SnakeLevel level)
        {
            return level == SnakeLevel.Easy;
        }

        /// <summary>Seconds between two steps.</summary>
        public static float StepSeconds(SnakeLevel level)
        {
            switch (level)
            {
                case SnakeLevel.Easy: return 0.32f;
                case SnakeLevel.Medium: return 0.22f;
                default: return 0.15f;
            }
        }
    }
}
