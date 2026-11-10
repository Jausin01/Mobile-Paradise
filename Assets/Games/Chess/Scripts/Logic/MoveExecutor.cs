namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Applies a move to a board (Make) and reverts it (Unmake).
    /// Does not check legality: MoveGenerator decides what is legal.
    /// </summary>
    public static class MoveExecutor
    {
        public static UndoInfo Make(Board board, Move move)
        {
            Piece moving = board.Get(move.From);
            PieceColor us = moving.Color;

            UndoInfo undo = new UndoInfo();
            undo.Castling = board.Castling;
            undo.EnPassantSquare = board.EnPassantSquare;
            undo.HalfmoveClock = board.HalfmoveClock;
            undo.FullmoveNumber = board.FullmoveNumber;

            int captureSquare = move.To;
            if (move.Kind == MoveKind.EnPassant)
            {
                captureSquare = Square.Make(Square.File(move.To), Square.Rank(move.From));
            }
            undo.Captured = board.Get(captureSquare);

            board.Set(captureSquare, Piece.None);
            board.Set(move.From, Piece.None);
            board.Set(move.To, move.Kind == MoveKind.Promotion ? new Piece(move.Promotion, us) : moving);

            if (move.Kind == MoveKind.CastleKingSide)
            {
                MoveRook(board, Square.Rank(move.From), 7, 5);
            }
            else if (move.Kind == MoveKind.CastleQueenSide)
            {
                MoveRook(board, Square.Rank(move.From), 0, 3);
            }

            board.EnPassantSquare = -1;
            if (move.Kind == MoveKind.DoublePawn)
            {
                board.EnPassantSquare = Square.Make(Square.File(move.From), (Square.Rank(move.From) + Square.Rank(move.To)) / 2);
            }

            UpdateCastlingRights(board, moving, move);

            if (moving.Type == PieceType.Pawn || !undo.Captured.IsNone) board.HalfmoveClock = 0;
            else board.HalfmoveClock++;

            if (us == PieceColor.Black) board.FullmoveNumber++;
            board.SideToMove = us.Opposite();

            return undo;
        }

        public static void Unmake(Board board, Move move, UndoInfo undo)
        {
            PieceColor us = board.SideToMove.Opposite();
            board.SideToMove = us;

            Piece placed = board.Get(move.To);
            Piece moving = move.Kind == MoveKind.Promotion ? new Piece(PieceType.Pawn, us) : placed;

            board.Set(move.To, Piece.None);
            board.Set(move.From, moving);

            if (move.Kind == MoveKind.EnPassant)
            {
                board.Set(Square.Make(Square.File(move.To), Square.Rank(move.From)), undo.Captured);
            }
            else
            {
                board.Set(move.To, undo.Captured);
            }

            if (move.Kind == MoveKind.CastleKingSide)
            {
                MoveRook(board, Square.Rank(move.From), 5, 7);
            }
            else if (move.Kind == MoveKind.CastleQueenSide)
            {
                MoveRook(board, Square.Rank(move.From), 3, 0);
            }

            board.Castling = undo.Castling;
            board.EnPassantSquare = undo.EnPassantSquare;
            board.HalfmoveClock = undo.HalfmoveClock;
            board.FullmoveNumber = undo.FullmoveNumber;
        }

        private static void MoveRook(Board board, int rank, int fromFile, int toFile)
        {
            int from = Square.Make(fromFile, rank);
            int to = Square.Make(toFile, rank);
            board.Set(to, board.Get(from));
            board.Set(from, Piece.None);
        }

        private static void UpdateCastlingRights(Board board, Piece moving, Move move)
        {
            if (moving.Type == PieceType.King)
            {
                board.Castling &= moving.Color == PieceColor.White
                    ? ~(CastlingRights.WhiteKingSide | CastlingRights.WhiteQueenSide)
                    : ~(CastlingRights.BlackKingSide | CastlingRights.BlackQueenSide);
            }

            // Moving a rook from its corner, or capturing a rook on its corner, loses that right.
            ClearForCorner(board, move.From);
            ClearForCorner(board, move.To);
        }

        private static void ClearForCorner(Board board, int square)
        {
            if (square == 0) board.Castling &= ~CastlingRights.WhiteQueenSide;
            else if (square == 7) board.Castling &= ~CastlingRights.WhiteKingSide;
            else if (square == 56) board.Castling &= ~CastlingRights.BlackQueenSide;
            else if (square == 63) board.Castling &= ~CastlingRights.BlackKingSide;
        }
    }
}
