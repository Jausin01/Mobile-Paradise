namespace MobileParadise.Games.Domino.Logic
{
    /// <summary>
    /// The 28 tiles of a double-six set. A tile is a number from 0 to 27 (its id); each id stands for a pair of
    /// values (low, high) with low &lt;= high: 0-0, 0-1, ..., 0-6, 1-1, 1-2, ..., 6-6.
    /// </summary>
    public static class DominoTiles
    {
        public const int Count = 28;
        public const int MaxValue = 6;

        private static readonly int[] LowValues = new int[Count];
        private static readonly int[] HighValues = new int[Count];

        static DominoTiles()
        {
            int id = 0;
            for (int low = 0; low <= MaxValue; low++)
            {
                for (int high = low; high <= MaxValue; high++)
                {
                    LowValues[id] = low;
                    HighValues[id] = high;
                    id++;
                }
            }
        }

        public static int Low(int id)
        {
            return LowValues[id];
        }

        public static int High(int id)
        {
            return HighValues[id];
        }

        public static int Pips(int id)
        {
            return LowValues[id] + HighValues[id];
        }

        public static bool IsDouble(int id)
        {
            return LowValues[id] == HighValues[id];
        }

        public static bool IsValidId(int id)
        {
            return id >= 0 && id < Count;
        }

        /// <summary>
        /// The id of the double with this value (0-0 ... 6-6).
        /// </summary>
        public static int DoubleId(int value)
        {
            for (int id = 0; id < Count; id++)
            {
                if (LowValues[id] == value && HighValues[id] == value) return id;
            }
            return -1;
        }
    }
}
