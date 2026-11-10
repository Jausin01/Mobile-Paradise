using System;
using System.Collections.Generic;
using MobileParadise.Games.Chess.Logic;

namespace MobileParadise.Games.Chess.AI
{
    /// <summary>
    /// Alpha-beta search (negamax). Difficulty comes from three knobs:
    /// search depth, whether captures are followed to a calm position (quiescence),
    /// and random noise added to each root move's score (makes weaker levels blunder).
    /// </summary>
    public class SearchAI : IChessAI
    {
        private const int Mate = 100000;
        private const int Infinity = 1000000;
        private const int MaxQuiescenceDepth = 6;

        private readonly int _depth;
        private readonly bool _useQuiescence;
        private readonly int _noise;
        private readonly Random _random;

        public SearchAI(int depth, bool useQuiescence, int noise, int seed)
        {
            _depth = depth;
            _useQuiescence = useQuiescence;
            _noise = noise;
            _random = new Random(seed);
        }

        public Move ChooseMove(Board board)
        {
            List<Move> moves = MoveGenerator.GenerateLegal(board);
            OrderMoves(board, moves);

            Move best = moves[0];
            int bestScore = -Infinity;

            for (int i = 0; i < moves.Count; i++)
            {
                // With noise every root move needs its exact score; without it we can prune.
                int alpha = _noise > 0 ? -Infinity : bestScore;

                UndoInfo undo = MoveExecutor.Make(board, moves[i]);
                int score = -Search(board, _depth - 1, -Infinity, -alpha, 1);
                MoveExecutor.Unmake(board, moves[i], undo);

                if (_noise > 0)
                {
                    score += _random.Next(-_noise, _noise + 1);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = moves[i];
                }
            }
            return best;
        }

        private int Search(Board board, int depth, int alpha, int beta, int ply)
        {
            if (depth <= 0)
            {
                return _useQuiescence ? Quiesce(board, alpha, beta, ply, 0) : Evaluator.Evaluate(board);
            }

            List<Move> moves = MoveGenerator.GenerateLegal(board);
            if (moves.Count == 0)
            {
                return board.IsInCheck(board.SideToMove) ? -(Mate - ply) : 0;
            }
            if (board.HalfmoveClock >= 100) return 0;

            OrderMoves(board, moves);

            int best = -Infinity;
            for (int i = 0; i < moves.Count; i++)
            {
                UndoInfo undo = MoveExecutor.Make(board, moves[i]);
                int score = -Search(board, depth - 1, -beta, -alpha, ply + 1);
                MoveExecutor.Unmake(board, moves[i], undo);

                if (score > best) best = score;
                if (best > alpha) alpha = best;
                if (alpha >= beta) break;
            }
            return best;
        }

        /// <summary>
        /// Keeps searching captures and promotions until the position is calm,
        /// so the AI does not misjudge the middle of an exchange.
        /// </summary>
        private int Quiesce(Board board, int alpha, int beta, int ply, int qDepth)
        {
            List<Move> moves = MoveGenerator.GenerateLegal(board);
            bool inCheck = board.IsInCheck(board.SideToMove);
            if (moves.Count == 0)
            {
                return inCheck ? -(Mate - ply) : 0;
            }

            int standPat = Evaluator.Evaluate(board);
            if (qDepth >= MaxQuiescenceDepth) return standPat;

            int best = -Infinity;
            if (!inCheck)
            {
                if (standPat >= beta) return standPat;
                if (standPat > alpha) alpha = standPat;
                best = standPat;
            }

            OrderMoves(board, moves);
            for (int i = 0; i < moves.Count; i++)
            {
                if (!inCheck && !IsNoisy(board, moves[i])) continue;

                UndoInfo undo = MoveExecutor.Make(board, moves[i]);
                int score = -Quiesce(board, -beta, -alpha, ply + 1, qDepth + 1);
                MoveExecutor.Unmake(board, moves[i], undo);

                if (score > best) best = score;
                if (best > alpha) alpha = best;
                if (alpha >= beta) break;
            }
            return best;
        }

        private static bool IsNoisy(Board board, Move move)
        {
            return move.Kind == MoveKind.Promotion
                || move.Kind == MoveKind.EnPassant
                || !board.Get(move.To).IsNone;
        }

        /// <summary>
        /// Most valuable victim / least valuable attacker first (better pruning),
        /// with a random tie-break so equal moves are not always played in the same order.
        /// </summary>
        private void OrderMoves(Board board, List<Move> moves)
        {
            int count = moves.Count;
            int[] keys = new int[count];
            Move[] items = new Move[count];

            for (int i = 0; i < count; i++)
            {
                Move move = moves[i];
                int key = _random.Next(0, 100);

                Piece victim = board.Get(move.To);
                if (!victim.IsNone)
                {
                    key += 1000 + Evaluator.PieceValue(victim.Type) * 10 - Evaluator.PieceValue(board.Get(move.From).Type);
                }
                else if (move.Kind == MoveKind.EnPassant)
                {
                    key += 1000 + 1000 - 100;
                }
                if (move.Kind == MoveKind.Promotion)
                {
                    key += 8000 + Evaluator.PieceValue(move.Promotion);
                }

                keys[i] = -key;   // Array.Sort is ascending, we want highest key first
                items[i] = move;
            }

            Array.Sort(keys, items);
            for (int i = 0; i < count; i++)
            {
                moves[i] = items[i];
            }
        }
    }
}
