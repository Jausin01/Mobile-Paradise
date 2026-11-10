using System.Collections.Generic;

namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Generates moves. Pseudo-legal = follows piece movement but may leave the
    /// own king in check; legal = pseudo-legal minus those.
    /// </summary>
    public static class MoveGenerator
    {
        private static readonly PieceType[] PromotionTypes =
        {
            PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight
        };

        public static List<Move> GenerateLegal(Board board)
        {
            List<Move> pseudo = new List<Move>(64);
            GeneratePseudoLegal(board, pseudo);

            PieceColor mover = board.SideToMove;
            List<Move> legal = new List<Move>(pseudo.Count);
            for (int i = 0; i < pseudo.Count; i++)
            {
                UndoInfo undo = MoveExecutor.Make(board, pseudo[i]);
                bool kingSafe = !board.IsInCheck(mover);
                MoveExecutor.Unmake(board, pseudo[i], undo);

                if (kingSafe)
                {
                    legal.Add(pseudo[i]);
                }
            }
            return legal;
        }

        public static void GeneratePseudoLegal(Board board, List<Move> moves)
        {
            PieceColor us = board.SideToMove;
            for (int square = 0; square < 64; square++)
            {
                Piece piece = board.Get(square);
                if (piece.IsNone || piece.Color != us) continue;

                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        GeneratePawn(board, square, us, moves);
                        break;
                    case PieceType.Knight:
                        GenerateSteps(board, square, us, Offsets.KnightFile, Offsets.KnightRank, moves);
                        break;
                    case PieceType.King:
                        GenerateSteps(board, square, us, Offsets.KingFile, Offsets.KingRank, moves);
                        GenerateCastling(board, square, us, moves);
                        break;
                    case PieceType.Bishop:
                        GenerateSlides(board, square, us, Offsets.BishopFile, Offsets.BishopRank, moves);
                        break;
                    case PieceType.Rook:
                        GenerateSlides(board, square, us, Offsets.RookFile, Offsets.RookRank, moves);
                        break;
                    case PieceType.Queen:
                        GenerateSlides(board, square, us, Offsets.BishopFile, Offsets.BishopRank, moves);
                        GenerateSlides(board, square, us, Offsets.RookFile, Offsets.RookRank, moves);
                        break;
                }
            }
        }

        private static void GeneratePawn(Board board, int square, PieceColor us, List<Move> moves)
        {
            int file = Square.File(square);
            int rank = Square.Rank(square);
            int dir = us == PieceColor.White ? 1 : -1;
            int startRank = us == PieceColor.White ? 1 : 6;
            int promotionRank = us == PieceColor.White ? 7 : 0;
            int nextRank = rank + dir;
            bool promotes = nextRank == promotionRank;

            int one = Square.Make(file, nextRank);
            if (board.Get(one).IsNone)
            {
                AddPawnMove(square, one, promotes, moves);

                if (rank == startRank)
                {
                    int two = Square.Make(file, rank + 2 * dir);
                    if (board.Get(two).IsNone)
                    {
                        moves.Add(new Move(square, two, PieceType.None, MoveKind.DoublePawn));
                    }
                }
            }

            for (int df = -1; df <= 1; df += 2)
            {
                int captureFile = file + df;
                if (captureFile < 0 || captureFile > 7) continue;

                int target = Square.Make(captureFile, nextRank);
                Piece targetPiece = board.Get(target);

                if (!targetPiece.IsNone && targetPiece.Color != us)
                {
                    AddPawnMove(square, target, promotes, moves);
                }
                else if (targetPiece.IsNone && target == board.EnPassantSquare)
                {
                    moves.Add(new Move(square, target, PieceType.None, MoveKind.EnPassant));
                }
            }
        }

        private static void AddPawnMove(int from, int to, bool promotes, List<Move> moves)
        {
            if (!promotes)
            {
                moves.Add(new Move(from, to, PieceType.None, MoveKind.Normal));
                return;
            }

            for (int i = 0; i < PromotionTypes.Length; i++)
            {
                moves.Add(new Move(from, to, PromotionTypes[i], MoveKind.Promotion));
            }
        }

        private static void GenerateSteps(Board board, int square, PieceColor us, int[] dFile, int[] dRank, List<Move> moves)
        {
            int file = Square.File(square);
            int rank = Square.Rank(square);

            for (int i = 0; i < dFile.Length; i++)
            {
                int f = file + dFile[i];
                int r = rank + dRank[i];
                if (!Square.InBounds(f, r)) continue;

                int target = Square.Make(f, r);
                Piece targetPiece = board.Get(target);
                if (targetPiece.IsNone || targetPiece.Color != us)
                {
                    moves.Add(new Move(square, target, PieceType.None, MoveKind.Normal));
                }
            }
        }

        private static void GenerateSlides(Board board, int square, PieceColor us, int[] dFile, int[] dRank, List<Move> moves)
        {
            int file = Square.File(square);
            int rank = Square.Rank(square);

            for (int d = 0; d < dFile.Length; d++)
            {
                int f = file + dFile[d];
                int r = rank + dRank[d];
                while (Square.InBounds(f, r))
                {
                    int target = Square.Make(f, r);
                    Piece targetPiece = board.Get(target);

                    if (targetPiece.IsNone)
                    {
                        moves.Add(new Move(square, target, PieceType.None, MoveKind.Normal));
                    }
                    else
                    {
                        if (targetPiece.Color != us)
                        {
                            moves.Add(new Move(square, target, PieceType.None, MoveKind.Normal));
                        }
                        break;
                    }

                    f += dFile[d];
                    r += dRank[d];
                }
            }
        }

        private static void GenerateCastling(Board board, int kingSquare, PieceColor us, List<Move> moves)
        {
            int homeRank = us == PieceColor.White ? 0 : 7;
            if (kingSquare != Square.Make(4, homeRank)) return;

            PieceColor them = us.Opposite();
            if (board.IsSquareAttacked(kingSquare, them)) return;

            CastlingRights kingSideRight = us == PieceColor.White ? CastlingRights.WhiteKingSide : CastlingRights.BlackKingSide;
            CastlingRights queenSideRight = us == PieceColor.White ? CastlingRights.WhiteQueenSide : CastlingRights.BlackQueenSide;

            if ((board.Castling & kingSideRight) != 0
                && HasRook(board, Square.Make(7, homeRank), us)
                && board.Get(Square.Make(5, homeRank)).IsNone
                && board.Get(Square.Make(6, homeRank)).IsNone
                && !board.IsSquareAttacked(Square.Make(5, homeRank), them)
                && !board.IsSquareAttacked(Square.Make(6, homeRank), them))
            {
                moves.Add(new Move(kingSquare, Square.Make(6, homeRank), PieceType.None, MoveKind.CastleKingSide));
            }

            if ((board.Castling & queenSideRight) != 0
                && HasRook(board, Square.Make(0, homeRank), us)
                && board.Get(Square.Make(1, homeRank)).IsNone
                && board.Get(Square.Make(2, homeRank)).IsNone
                && board.Get(Square.Make(3, homeRank)).IsNone
                && !board.IsSquareAttacked(Square.Make(3, homeRank), them)
                && !board.IsSquareAttacked(Square.Make(2, homeRank), them))
            {
                moves.Add(new Move(kingSquare, Square.Make(2, homeRank), PieceType.None, MoveKind.CastleQueenSide));
            }
        }

        private static bool HasRook(Board board, int square, PieceColor color)
        {
            Piece piece = board.Get(square);
            return piece.Type == PieceType.Rook && piece.Color == color;
        }
    }
}
