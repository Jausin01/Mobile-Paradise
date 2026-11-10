namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Pure data: the 64 squares plus the extra state chess needs
    /// (side to move, castling rights, en passant, move clocks).
    /// </summary>
    public class Board
    {
        private readonly Piece[] _squares = new Piece[64];
        private readonly int[] _kingSquares = { -1, -1 };

        public PieceColor SideToMove = PieceColor.White;
        public CastlingRights Castling = CastlingRights.None;
        public int EnPassantSquare = -1;   // square a pawn can capture onto, or -1
        public int HalfmoveClock;          // plies since last capture or pawn move
        public int FullmoveNumber = 1;

        public Piece Get(int square)
        {
            return _squares[square];
        }

        public void Set(int square, Piece piece)
        {
            _squares[square] = piece;
            if (piece.Type == PieceType.King)
            {
                _kingSquares[(int)piece.Color] = square;
            }
        }

        public int KingSquare(PieceColor color)
        {
            return _kingSquares[(int)color];
        }

        public void Clear()
        {
            for (int i = 0; i < 64; i++)
            {
                _squares[i] = Piece.None;
            }
            _kingSquares[0] = -1;
            _kingSquares[1] = -1;
            SideToMove = PieceColor.White;
            Castling = CastlingRights.None;
            EnPassantSquare = -1;
            HalfmoveClock = 0;
            FullmoveNumber = 1;
        }

        public void SetupInitial()
        {
            FenParser.Load(this, FenParser.StartPosition);
        }

        public bool IsInCheck(PieceColor color)
        {
            int king = KingSquare(color);
            return king >= 0 && IsSquareAttacked(king, color.Opposite());
        }

        /// <summary>
        /// True if any piece of color 'by' attacks the given square.
        /// </summary>
        public bool IsSquareAttacked(int square, PieceColor by)
        {
            int file = Square.File(square);
            int rank = Square.Rank(square);

            // Pawns: a pawn of 'by' attacks diagonally forward, so look one rank "behind" it.
            int pawnRank = rank + (by == PieceColor.White ? -1 : 1);
            for (int df = -1; df <= 1; df += 2)
            {
                if (Square.InBounds(file + df, pawnRank) && IsPiece(Square.Make(file + df, pawnRank), PieceType.Pawn, by))
                {
                    return true;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                int kf = file + Offsets.KnightFile[i];
                int kr = rank + Offsets.KnightRank[i];
                if (Square.InBounds(kf, kr) && IsPiece(Square.Make(kf, kr), PieceType.Knight, by))
                {
                    return true;
                }

                int nf = file + Offsets.KingFile[i];
                int nr = rank + Offsets.KingRank[i];
                if (Square.InBounds(nf, nr) && IsPiece(Square.Make(nf, nr), PieceType.King, by))
                {
                    return true;
                }
            }

            return SliderAttacks(file, rank, by, Offsets.RookFile, Offsets.RookRank, PieceType.Rook)
                || SliderAttacks(file, rank, by, Offsets.BishopFile, Offsets.BishopRank, PieceType.Bishop);
        }

        public Board Clone()
        {
            Board copy = new Board();
            for (int i = 0; i < 64; i++)
            {
                copy.Set(i, _squares[i]);
            }
            copy.SideToMove = SideToMove;
            copy.Castling = Castling;
            copy.EnPassantSquare = EnPassantSquare;
            copy.HalfmoveClock = HalfmoveClock;
            copy.FullmoveNumber = FullmoveNumber;
            return copy;
        }

        private bool IsPiece(int square, PieceType type, PieceColor color)
        {
            Piece piece = _squares[square];
            return piece.Type == type && piece.Color == color;
        }

        private bool SliderAttacks(int file, int rank, PieceColor by, int[] dFile, int[] dRank, PieceType slider)
        {
            for (int d = 0; d < dFile.Length; d++)
            {
                int f = file + dFile[d];
                int r = rank + dRank[d];
                while (Square.InBounds(f, r))
                {
                    Piece piece = _squares[Square.Make(f, r)];
                    if (!piece.IsNone)
                    {
                        if (piece.Color == by && (piece.Type == slider || piece.Type == PieceType.Queen))
                        {
                            return true;
                        }
                        break;
                    }
                    f += dFile[d];
                    r += dRank[d];
                }
            }
            return false;
        }
    }
}
