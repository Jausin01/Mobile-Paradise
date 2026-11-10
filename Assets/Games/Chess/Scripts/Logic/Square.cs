namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Squares are ints 0-63: index = rank * 8 + file.
    /// File 0 = a, rank 0 = White's first rank (a1 = 0, h8 = 63).
    /// </summary>
    public static class Square
    {
        public static int Make(int file, int rank)
        {
            return rank * 8 + file;
        }

        public static int File(int square)
        {
            return square & 7;
        }

        public static int Rank(int square)
        {
            return square >> 3;
        }

        public static bool InBounds(int file, int rank)
        {
            return file >= 0 && file < 8 && rank >= 0 && rank < 8;
        }

        public static string Name(int square)
        {
            return "" + (char)('a' + File(square)) + (char)('1' + Rank(square));
        }

        public static int Parse(string name)
        {
            return Make(name[0] - 'a', name[1] - '1');
        }
    }
}
