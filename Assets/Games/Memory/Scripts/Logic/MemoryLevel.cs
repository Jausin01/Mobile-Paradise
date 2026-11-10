namespace MobileParadise.Games.Memory.Logic
{
    public enum MemoryLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// Grid size of each level (portrait: columns x rows). Change the numbers here to tune the difficulty.
    /// </summary>
    public static class MemoryLevels
    {
        public static int Columns(MemoryLevel level)
        {
            return level == MemoryLevel.Easy ? 3 : 4;
        }

        public static int Rows(MemoryLevel level)
        {
            switch (level)
            {
                case MemoryLevel.Easy: return 4;
                case MemoryLevel.Medium: return 4;
                default: return 5;
            }
        }

        public static int PairCount(MemoryLevel level)
        {
            return Columns(level) * Rows(level) / 2;
        }
    }
}
