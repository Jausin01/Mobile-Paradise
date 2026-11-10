namespace MobileParadise.Games.Checkers.Logic
{
    public enum CheckersSide
    {
        White = 0,   // starts at the bottom and moves up (row numbers go down)
        Black = 1
    }

    /// <summary>
    /// One move: the squares the piece visits (a capture can jump several times) and the pieces it takes.
    /// A square is a number: row * 8 + column, row 0 at the top.
    /// </summary>
    public class CheckersMove
    {
        public readonly int[] Path;
        public readonly int[] Captured;
        public readonly bool Promotes;

        public CheckersMove(int[] path, int[] captured, bool promotes)
        {
            Path = path;
            Captured = captured;
            Promotes = promotes;
        }

        public int From { get { return Path[0]; } }
        public int To { get { return Path[Path.Length - 1]; } }
        public bool IsCapture { get { return Captured.Length > 0; } }
    }
}
