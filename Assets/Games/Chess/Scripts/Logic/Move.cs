namespace MobileParadise.Games.Chess.Logic
{
    public struct Move
    {
        public readonly int From;
        public readonly int To;
        public readonly PieceType Promotion;   // only used when Kind == Promotion
        public readonly MoveKind Kind;

        public Move(int from, int to, PieceType promotion, MoveKind kind)
        {
            From = from;
            To = to;
            Promotion = promotion;
            Kind = kind;
        }

        /// <summary>
        /// Coordinate notation, e.g. "e2e4" or "e7e8q".
        /// </summary>
        public override string ToString()
        {
            string text = Square.Name(From) + Square.Name(To);
            if (Kind == MoveKind.Promotion)
            {
                switch (Promotion)
                {
                    case PieceType.Knight: text += "n"; break;
                    case PieceType.Bishop: text += "b"; break;
                    case PieceType.Rook: text += "r"; break;
                    case PieceType.Queen: text += "q"; break;
                }
            }
            return text;
        }
    }
}
