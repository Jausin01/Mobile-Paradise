namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// File/rank step tables shared by attack detection and move generation.
    /// </summary>
    internal static class Offsets
    {
        public static readonly int[] KnightFile = { 1, 2, 2, 1, -1, -2, -2, -1 };
        public static readonly int[] KnightRank = { 2, 1, -1, -2, -2, -1, 1, 2 };

        public static readonly int[] KingFile = { 1, 1, 1, 0, 0, -1, -1, -1 };
        public static readonly int[] KingRank = { 1, 0, -1, 1, -1, 1, 0, -1 };

        public static readonly int[] RookFile = { 1, -1, 0, 0 };
        public static readonly int[] RookRank = { 0, 0, 1, -1 };

        public static readonly int[] BishopFile = { 1, 1, -1, -1 };
        public static readonly int[] BishopRank = { 1, -1, 1, -1 };
    }
}
