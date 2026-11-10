namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Loads a position from FEN text. Used for the start position, tests and puzzles.
    /// </summary>
    public static class FenParser
    {
        public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static void Load(Board board, string fen)
        {
            board.Clear();
            string[] parts = fen.Trim().Split(' ');
            string[] ranks = parts[0].Split('/');

            for (int i = 0; i < 8; i++)
            {
                int rank = 7 - i;
                int file = 0;
                foreach (char ch in ranks[i])
                {
                    if (char.IsDigit(ch))
                    {
                        file += ch - '0';
                    }
                    else
                    {
                        board.Set(Square.Make(file, rank), PieceFromChar(ch));
                        file++;
                    }
                }
            }

            board.SideToMove = parts[1] == "w" ? PieceColor.White : PieceColor.Black;

            foreach (char ch in parts[2])
            {
                switch (ch)
                {
                    case 'K': board.Castling |= CastlingRights.WhiteKingSide; break;
                    case 'Q': board.Castling |= CastlingRights.WhiteQueenSide; break;
                    case 'k': board.Castling |= CastlingRights.BlackKingSide; break;
                    case 'q': board.Castling |= CastlingRights.BlackQueenSide; break;
                }
            }

            board.EnPassantSquare = parts[3] == "-" ? -1 : Square.Parse(parts[3]);

            if (parts.Length > 4) board.HalfmoveClock = int.Parse(parts[4]);
            if (parts.Length > 5) board.FullmoveNumber = int.Parse(parts[5]);
        }

        private static Piece PieceFromChar(char ch)
        {
            PieceColor color = char.IsUpper(ch) ? PieceColor.White : PieceColor.Black;
            switch (char.ToLower(ch))
            {
                case 'p': return new Piece(PieceType.Pawn, color);
                case 'n': return new Piece(PieceType.Knight, color);
                case 'b': return new Piece(PieceType.Bishop, color);
                case 'r': return new Piece(PieceType.Rook, color);
                case 'q': return new Piece(PieceType.Queen, color);
                case 'k': return new Piece(PieceType.King, color);
            }
            return Piece.None;
        }
    }
}
