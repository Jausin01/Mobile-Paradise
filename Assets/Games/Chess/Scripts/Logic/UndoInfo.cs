namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Everything MoveExecutor.Unmake needs to restore the board exactly.
    /// </summary>
    public struct UndoInfo
    {
        public Piece Captured;
        public CastlingRights Castling;
        public int EnPassantSquare;
        public int HalfmoveClock;
        public int FullmoveNumber;
    }
}
