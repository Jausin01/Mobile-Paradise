namespace MobileParadise.Games.Minesweeper.Logic
{
    public enum MinesLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// Size and mine count of each level (portrait: columns x rows). Change the numbers here to tune the difficulty.
    /// </summary>
    public static class MinesLevels
    {
        public static int Columns(MinesLevel level)
        {
            switch (level)
            {
                case MinesLevel.Easy: return 7;
                case MinesLevel.Medium: return 8;
                default: return 9;
            }
        }

        public static int Rows(MinesLevel level)
        {
            switch (level)
            {
                case MinesLevel.Easy: return 9;
                case MinesLevel.Medium: return 10;
                default: return 12;
            }
        }

        public static int Mines(MinesLevel level)
        {
            switch (level)
            {
                case MinesLevel.Easy: return 8;
                case MinesLevel.Medium: return 14;
                default: return 22;
            }
        }
    }
}
