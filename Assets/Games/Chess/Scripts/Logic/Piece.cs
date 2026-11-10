namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// A piece on the board. default(Piece) is an empty square.
    /// </summary>
    public struct Piece
    {
        public static readonly Piece None = new Piece(PieceType.None, PieceColor.White);

        public readonly PieceType Type;
        public readonly PieceColor Color;

        public Piece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
        }

        public bool IsNone
        {
            get { return Type == PieceType.None; }
        }
    }
}
