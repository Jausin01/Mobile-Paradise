using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Checkers.Logic
{
    /// <summary>
    /// The computer player: looks ahead with minimax (alpha-beta) and picks the move with the best material
    /// and position. It deepens its search one move at a time until the node budget of the level is used.
    /// </summary>
    public class CheckersAI
    {
        private const int ManValue = 100;
        private const int KingValue = 300;
        private const int WinValue = 100000;

        private readonly CheckersDifficulty _difficulty;
        private readonly Random _random;
        private int _nodes;
        private int _budget;
        private bool _aborted;

        public CheckersAI(CheckersDifficulty difficulty, Random random)
        {
            _difficulty = difficulty;
            _random = random;
        }

        /// <summary>
        /// Chooses a move for 'side' from the legal moves of the board.
        /// </summary>
        public CheckersMove ChooseMove(CheckersBoard board, CheckersSide side)
        {
            List<CheckersMove> moves = board.LegalMoves(side);
            if (moves.Count == 0) return null;
            if (moves.Count == 1) return moves[0];

            if (_random.NextDouble() < CheckersSettings.MistakeChance(_difficulty))
            {
                return moves[_random.Next(moves.Count)];
            }

            Shuffle(moves);   // equal moves are chosen at random, so games differ
            _budget = CheckersSettings.NodeBudget(_difficulty);
            _nodes = 0;
            _aborted = false;

            CheckersMove best = moves[0];
            int maxDepth = CheckersSettings.SearchDepth(_difficulty);

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                CheckersMove depthBest = null;
                int depthScore = -int.MaxValue;

                for (int i = 0; i < moves.Count; i++)
                {
                    int score = -Search(board.Apply(moves[i]), CheckersBoard.Opponent(side), depth - 1, -int.MaxValue, -depthScore, 1);
                    if (_aborted) break;

                    if (score > depthScore)
                    {
                        depthScore = score;
                        depthBest = moves[i];
                    }
                }

                if (_aborted) break;   // keep the move of the last complete search
                best = depthBest;
                moves.Remove(best);    // the best move is looked at first in the next, deeper search
                moves.Insert(0, best);
            }
            return best;
        }

        // Negamax with alpha-beta: the score is from the point of view of 'side', the one to move.
        private int Search(CheckersBoard board, CheckersSide side, int depth, int alpha, int beta, int ply)
        {
            if (++_nodes > _budget)
            {
                _aborted = true;
                return 0;
            }

            List<CheckersMove> moves = board.LegalMoves(side);
            if (moves.Count == 0) return -WinValue + ply;   // no move: this side loses (sooner is worse)
            if (depth <= 0) return Evaluate(board, side);

            int best = -int.MaxValue;
            for (int i = 0; i < moves.Count; i++)
            {
                int score = -Search(board.Apply(moves[i]), CheckersBoard.Opponent(side), depth - 1, -beta, -alpha, ply + 1);
                if (_aborted) return 0;

                if (score > best) best = score;
                if (best > alpha) alpha = best;
                if (alpha >= beta) break;
            }
            return best;
        }

        // Material first; then men are worth more the closer they are to being crowned, and the center and the back row help.
        private static int Evaluate(CheckersBoard board, CheckersSide side)
        {
            int score = 0;
            for (int square = 0; square < CheckersBoard.SquareCount; square++)
            {
                int piece = board.Get(square);
                if (piece == CheckersBoard.Empty) continue;

                int row = square / CheckersBoard.Size;
                int column = square % CheckersBoard.Size;
                int value;

                if (CheckersBoard.IsKing(piece))
                {
                    value = KingValue;
                }
                else
                {
                    bool white = CheckersBoard.IsWhite(piece);
                    int progress = white ? (CheckersBoard.Size - 1 - row) : row;   // 0 at home .. 7 at the last row
                    value = ManValue + progress * 4;
                    if (progress == 0) value += 6;   // a man at home guards the back row
                }

                if (row >= 2 && row <= 5 && column >= 2 && column <= 5) value += 3;

                score += CheckersBoard.BelongsTo(piece, side) ? value : -value;
            }
            return score;
        }

        private void Shuffle(List<CheckersMove> moves)
        {
            for (int i = moves.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                CheckersMove temp = moves[i];
                moves[i] = moves[j];
                moves[j] = temp;
            }
        }
    }
}
