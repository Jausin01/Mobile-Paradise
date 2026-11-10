using System;
using System.Collections.Generic;
using System.Text;

namespace MobileParadise.Games.Chess.Logic
{
    /// <summary>
    /// One chess game: the board, the move history (with undo) and the status.
    /// The only class the rest of the game (view, AI) needs to talk to.
    /// </summary>
    public class ChessGame
    {
        private readonly Board _board = new Board();
        private readonly List<Move> _history = new List<Move>();
        private readonly List<UndoInfo> _undoStack = new List<UndoInfo>();
        private readonly List<string> _positionKeys = new List<string>();   // one per position, start included
        private List<Move> _legalMoves = new List<Move>();

        /// <summary>Raised after any change: move played, undo, reset, load.</summary>
        public event Action StateChanged;

        public Board Board { get { return _board; } }
        public PieceColor SideToMove { get { return _board.SideToMove; } }
        public GameStatus Status { get; private set; }
        public List<Move> LegalMoves { get { return _legalMoves; } }
        public bool HasLastMove { get { return _history.Count > 0; } }
        public Move LastMove { get { return _history[_history.Count - 1]; } }
        public int MoveCount { get { return _history.Count; } }

        public bool IsOver
        {
            get { return Status != GameStatus.Ongoing && Status != GameStatus.Check; }
        }

        public ChessGame()
        {
            Reset();
        }

        public void Reset()
        {
            LoadFen(FenParser.StartPosition);
        }

        public void LoadFen(string fen)
        {
            FenParser.Load(_board, fen);
            _history.Clear();
            _undoStack.Clear();
            _positionKeys.Clear();
            Refresh(true);
        }

        /// <summary>
        /// The moves played so far, one number each (from + to * 64 + promotion piece * 4096), for saving.
        /// </summary>
        public int[] ExportMoves()
        {
            int[] codes = new int[_history.Count];
            for (int i = 0; i < codes.Length; i++)
            {
                Move move = _history[i];
                int promotion = move.Kind == MoveKind.Promotion ? (int)move.Promotion : 0;
                codes[i] = move.From | (move.To << 6) | (promotion << 12);
            }
            return codes;
        }

        /// <summary>
        /// Starts a new game and replays saved moves. Returns false (and a fresh game) if a move is not legal.
        /// </summary>
        public bool LoadMoves(int[] codes)
        {
            Reset();
            for (int i = 0; i < codes.Length; i++)
            {
                int from = codes[i] & 63;
                int to = (codes[i] >> 6) & 63;
                PieceType promotion = (PieceType)(codes[i] >> 12);

                if (!TryMove(from, to, promotion))
                {
                    Reset();
                    return false;
                }
            }
            return true;
        }

        public List<Move> GetLegalMovesFrom(int square)
        {
            List<Move> result = new List<Move>();
            for (int i = 0; i < _legalMoves.Count; i++)
            {
                if (_legalMoves[i].From == square)
                {
                    result.Add(_legalMoves[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// True if moving from -> to is a pawn promotion (the player must pick a piece).
        /// </summary>
        public bool IsPromotionMove(int from, int to)
        {
            for (int i = 0; i < _legalMoves.Count; i++)
            {
                if (_legalMoves[i].From == from && _legalMoves[i].To == to && _legalMoves[i].Kind == MoveKind.Promotion)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Plays from -> to if it is legal. 'promotion' is only used for promotion moves.
        /// </summary>
        public bool TryMove(int from, int to, PieceType promotion)
        {
            for (int i = 0; i < _legalMoves.Count; i++)
            {
                Move move = _legalMoves[i];
                if (move.From != from || move.To != to) continue;
                if (move.Kind == MoveKind.Promotion && move.Promotion != promotion) continue;

                Play(move);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Plays a move taken from LegalMoves (used by the AI).
        /// </summary>
        public void Play(Move move)
        {
            _undoStack.Add(MoveExecutor.Make(_board, move));
            _history.Add(move);
            Refresh(true);
        }

        public bool Undo()
        {
            if (_history.Count == 0) return false;

            int last = _history.Count - 1;
            MoveExecutor.Unmake(_board, _history[last], _undoStack[last]);
            _history.RemoveAt(last);
            _undoStack.RemoveAt(last);
            _positionKeys.RemoveAt(_positionKeys.Count - 1);
            Refresh(false);
            return true;
        }

        private void Refresh(bool recordPosition)
        {
            _legalMoves = MoveGenerator.GenerateLegal(_board);
            if (recordPosition)
            {
                _positionKeys.Add(BuildPositionKey());
            }
            Status = GameStatusEvaluator.Evaluate(_board, _legalMoves, CountRepetitions());

            if (StateChanged != null)
            {
                StateChanged();
            }
        }

        /// <summary>
        /// How many times the current position has happened in this game (1 = first time).
        /// </summary>
        private int CountRepetitions()
        {
            string current = _positionKeys[_positionKeys.Count - 1];
            int count = 0;
            for (int i = 0; i < _positionKeys.Count; i++)
            {
                if (_positionKeys[i] == current) count++;
            }
            return count;
        }

        /// <summary>
        /// Same pieces, same side to move, same castling rights and same en passant chance = same position.
        /// The en passant square only counts when a legal en passant capture exists.
        /// </summary>
        private string BuildPositionKey()
        {
            StringBuilder key = new StringBuilder(72);
            for (int square = 0; square < 64; square++)
            {
                Piece piece = _board.Get(square);
                if (piece.IsNone)
                {
                    key.Append('.');
                }
                else
                {
                    key.Append((char)('a' + (int)piece.Type + (piece.Color == PieceColor.White ? 0 : 8)));
                }
            }

            key.Append(_board.SideToMove == PieceColor.White ? 'w' : 'b');
            key.Append((int)_board.Castling);

            for (int i = 0; i < _legalMoves.Count; i++)
            {
                if (_legalMoves[i].Kind == MoveKind.EnPassant)
                {
                    key.Append(_board.EnPassantSquare);
                    break;
                }
            }
            return key.ToString();
        }
    }
}
