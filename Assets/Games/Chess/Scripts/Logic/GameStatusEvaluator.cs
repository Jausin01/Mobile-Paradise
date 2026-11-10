using System.Collections.Generic;

namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// Decides the status of the position for the side to move.
    /// Repetitions is how many times the current position has occurred (1 = first time).
    /// </summary>
    public static class GameStatusEvaluator
    {
        public static GameStatus Evaluate(Board board, List<Move> legalMoves)
        {
            return Evaluate(board, legalMoves, 1);
        }

        public static GameStatus Evaluate(Board board, List<Move> legalMoves, int repetitions)
        {
            bool inCheck = board.IsInCheck(board.SideToMove);

            if (legalMoves.Count == 0)
            {
                return inCheck ? GameStatus.Checkmate : GameStatus.Stalemate;
            }
            if (HasInsufficientMaterial(board))
            {
                return GameStatus.DrawInsufficientMaterial;
            }
            if (repetitions >= 3)
            {
                return GameStatus.DrawRepetition;
            }
            if (board.HalfmoveClock >= 100)
            {
                return GameStatus.DrawFiftyMoves;
            }
            return inCheck ? GameStatus.Check : GameStatus.Ongoing;
        }

        /// <summary>
        /// King vs king, or king plus a single knight/bishop vs king.
        /// </summary>
        public static bool HasInsufficientMaterial(Board board)
        {
            int minorPieces = 0;
            for (int square = 0; square < 64; square++)
            {
                switch (board.Get(square).Type)
                {
                    case PieceType.Pawn:
                    case PieceType.Rook:
                    case PieceType.Queen:
                        return false;
                    case PieceType.Knight:
                    case PieceType.Bishop:
                        minorPieces++;
                        break;
                }
            }
            return minorPieces <= 1;
        }
    }
}
