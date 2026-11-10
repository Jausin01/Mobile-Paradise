using System;
using MobileParadise.Games.Chess.Logic;

namespace MobileParadise.Games.Chess.AI
{
    /// <summary>
    /// Static position score in centipawns (100 = one pawn).
    /// Material + piece-square tables + a "mop-up" bonus that helps the AI
    /// finish won endgames (push the enemy king to the edge, bring the own king close).
    /// </summary>
    public static class Evaluator
    {
        private const int EndgameMaterial = 2400;   // total non-pawn material of both sides
        private const int MopUpMinAdvantage = 300;

        public static int PieceValue(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return 100;
                case PieceType.Knight: return 320;
                case PieceType.Bishop: return 330;
                case PieceType.Rook: return 500;
                case PieceType.Queen: return 900;
            }
            return 0;
        }

        /// <summary>
        /// Score from the point of view of board.SideToMove (positive = good for them).
        /// </summary>
        public static int Evaluate(Board board)
        {
            int nonPawnMaterial = 0;
            for (int square = 0; square < 64; square++)
            {
                Piece piece = board.Get(square);
                if (piece.Type != PieceType.Pawn)
                {
                    nonPawnMaterial += PieceValue(piece.Type);
                }
            }
            bool endgame = nonPawnMaterial <= EndgameMaterial;

            int score = 0;
            for (int square = 0; square < 64; square++)
            {
                Piece piece = board.Get(square);
                if (piece.IsNone) continue;

                int value = PieceValue(piece.Type) + PieceSquareTables.Get(piece.Type, piece.Color, square, endgame);
                score += piece.Color == PieceColor.White ? value : -value;
            }

            score += MopUp(board, score, endgame);
            return board.SideToMove == PieceColor.White ? score : -score;
        }

        private static int MopUp(Board board, int score, bool endgame)
        {
            if (!endgame || Math.Abs(score) < MopUpMinAdvantage) return 0;

            PieceColor winner = score > 0 ? PieceColor.White : PieceColor.Black;
            int winnerKing = board.KingSquare(winner);
            int loserKing = board.KingSquare(winner.Opposite());
            if (winnerKing < 0 || loserKing < 0) return 0;

            int loserFromCenter = Math.Abs(2 * Square.File(loserKing) - 7) + Math.Abs(2 * Square.Rank(loserKing) - 7);
            int kingDistance = Math.Abs(Square.File(winnerKing) - Square.File(loserKing))
                             + Math.Abs(Square.Rank(winnerKing) - Square.Rank(loserKing));

            int bonus = loserFromCenter * 10 + (14 - kingDistance) * 4;
            return winner == PieceColor.White ? bonus : -bonus;
        }
    }
}
