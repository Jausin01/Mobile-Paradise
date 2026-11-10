namespace MobileParadise.Games.Checkers.Logic
{
    public enum CheckersDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// How the computer plays at each level. Change the numbers here to tune it.
    /// </summary>
    public static class CheckersSettings
    {
        /// <summary>How many moves ahead the computer looks.</summary>
        public static int SearchDepth(CheckersDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CheckersDifficulty.Easy: return 2;
                case CheckersDifficulty.Medium: return 4;
                default: return 8;
            }
        }

        /// <summary>Chance (0..1) that the computer plays a random move instead of its best one.</summary>
        public static float MistakeChance(CheckersDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CheckersDifficulty.Easy: return 0.35f;
                case CheckersDifficulty.Medium: return 0.05f;
                default: return 0f;
            }
        }

        /// <summary>The computer stops thinking after looking at this many positions (keeps the phone fast).</summary>
        public static int NodeBudget(CheckersDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CheckersDifficulty.Easy: return 5000;
                case CheckersDifficulty.Medium: return 40000;
                default: return 120000;
            }
        }
    }
}
