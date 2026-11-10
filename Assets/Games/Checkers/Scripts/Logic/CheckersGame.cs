using System.Collections.Generic;

namespace MobileParadise.Games.Checkers.Logic
{
    public enum CheckersStatus
    {
        Playing,
        WhiteWins,
        BlackWins,
        Draw
    }

    /// <summary>
    /// A game of draughts: the board, whose turn it is, the result, and Undo for the player.
    /// White (the player) moves first. The game is a draw after 20 moves each with only kings moving and no capture.
    /// </summary>
    public class CheckersGame
    {
        private const int DrawAfterPlies = 40;

        private readonly List<Snapshot> _history = new List<Snapshot>();

        private CheckersBoard _board;
        private CheckersSide _turn;
        private int _quietPlies;   // moves in a row with only kings moving and no capture
        private CheckersStatus _status = CheckersStatus.Playing;
        private List<CheckersMove> _legalMoves;

        private class Snapshot
        {
            public CheckersBoard Board;
            public CheckersSide Turn;
            public int QuietPlies;
        }

        public CheckersGame()
        {
            _board = CheckersBoard.Start();
            _turn = CheckersSide.White;
            Refresh();
        }

        /// <summary>
        /// A saved game. Check CheckersBoard.IsValid(cells) first.
        /// </summary>
        public CheckersGame(int[] cells, CheckersSide turn, int quietPlies)
        {
            _board = CheckersBoard.FromCells(cells);
            _turn = turn;
            _quietPlies = quietPlies;
            Refresh();
        }

        public CheckersBoard Board { get { return _board; } }
        public CheckersSide Turn { get { return _turn; } }
        public int QuietPlies { get { return _quietPlies; } }
        public CheckersStatus Status { get { return _status; } }
        public List<CheckersMove> LegalMoves { get { return _legalMoves; } }

        /// <summary>True if the side to move must capture.</summary>
        public bool MustCapture
        {
            get { return _legalMoves.Count > 0 && _legalMoves[0].IsCapture; }
        }

        public bool CanUndo
        {
            get { return _turn == CheckersSide.White && _history.Count >= 2 && _status == CheckersStatus.Playing; }
        }

        /// <summary>
        /// Plays a move that is one of LegalMoves.
        /// </summary>
        public void Play(CheckersMove move)
        {
            Snapshot snapshot = new Snapshot();
            snapshot.Board = _board;
            snapshot.Turn = _turn;
            snapshot.QuietPlies = _quietPlies;
            _history.Add(snapshot);

            bool king = CheckersBoard.IsKing(_board.Get(move.From));
            _quietPlies = (king && !move.IsCapture) ? _quietPlies + 1 : 0;

            _board = _board.Apply(move);
            _turn = CheckersBoard.Opponent(_turn);
            Refresh();
        }

        /// <summary>
        /// Takes back the computer's last move and the player's move before it.
        /// </summary>
        public bool Undo()
        {
            if (!CanUndo) return false;

            Snapshot snapshot = _history[_history.Count - 2];
            _history.RemoveRange(_history.Count - 2, 2);
            _board = snapshot.Board;
            _turn = snapshot.Turn;
            _quietPlies = snapshot.QuietPlies;
            Refresh();
            return true;
        }

        private void Refresh()
        {
            _legalMoves = _board.LegalMoves(_turn);

            if (_legalMoves.Count == 0)
            {
                // No move left (or no piece left): the side to move loses.
                _status = _turn == CheckersSide.White ? CheckersStatus.BlackWins : CheckersStatus.WhiteWins;
            }
            else if (_quietPlies >= DrawAfterPlies)
            {
                _status = CheckersStatus.Draw;
            }
            else
            {
                _status = CheckersStatus.Playing;
            }
        }
    }
}
