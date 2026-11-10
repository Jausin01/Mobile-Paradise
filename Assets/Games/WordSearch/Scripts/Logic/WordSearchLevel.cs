namespace MobileParadise.Games.WordSearch.Logic
{
    public enum WordSearchLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// Size, number of words and the directions words may run in, per level. Change the numbers here to tune it.
    /// A direction is a pair (dx, dy): dx to the right, dy downward.
    /// </summary>
    public static class WordSearchLevels
    {
        private static readonly int[][] EasyDirections =
        {
            new int[] { 1, 0 }, new int[] { 0, 1 }
        };

        private static readonly int[][] MediumDirections =
        {
            new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { 1, 1 }, new int[] { 1, -1 }
        };

        private static readonly int[][] HardDirections =
        {
            new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 0, -1 },
            new int[] { 1, 1 }, new int[] { -1, -1 }, new int[] { 1, -1 }, new int[] { -1, 1 }
        };

        public static int Size(WordSearchLevel level)
        {
            switch (level)
            {
                case WordSearchLevel.Easy: return 8;
                case WordSearchLevel.Medium: return 10;
                default: return 12;
            }
        }

        public static int WordCount(WordSearchLevel level)
        {
            switch (level)
            {
                case WordSearchLevel.Easy: return 6;
                case WordSearchLevel.Medium: return 8;
                default: return 10;
            }
        }

        public static int[][] Directions(WordSearchLevel level)
        {
            switch (level)
            {
                case WordSearchLevel.Easy: return EasyDirections;
                case WordSearchLevel.Medium: return MediumDirections;
                default: return HardDirections;
            }
        }
    }
}
