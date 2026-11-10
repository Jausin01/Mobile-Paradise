using System.Collections;
using System.Collections.Generic;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Checkers.Logic;
using UnityEngine;

namespace MobileParadise.Games.Checkers.View
{
    /// <summary>
    /// Connects the draughts rules (CheckersGame) and the computer player (CheckersAI) to the screens.
    /// The player is White (bottom) and moves first. Tap a piece, then tap where it should go.
    /// Flow: start screen -> player's turn <-> computer's turn -> result panel. The game is saved after every move.
    /// </summary>
    public class CheckersGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            PlayerTurn,
            ComputerTurn,
            Over
        }

        private const float ThinkPauseSeconds = 0.6f;
        private const float ResultPauseSeconds = 1.0f;

        [SerializeField] private CheckersStartPanel _startPanel;
        [SerializeField] private CheckersBoardView _boardView;
        [SerializeField] private CheckersHud _hud;
        [SerializeField] private CheckersResultPanel _resultPanel;

        private CheckersGame _game;
        private CheckersAI _ai;
        private CheckersDifficulty _difficulty;
        private Phase _phase;
        private int _selected = -1;
        private List<int> _targets = new List<int>();
        private int _lastFrom = -1;
        private int _lastTo = -1;

        private void Start()
        {
            _boardView.Build();
            _boardView.SquareClicked += OnSquareClicked;

            _hud.Init();
            _hud.UndoClicked += OnUndo;
            _hud.NewGameClicked += StartGame;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            StopAllCoroutines();
            _phase = Phase.Choosing;
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(OnDifficultyChosen, OnContinue, CheckersSaveData.Load() != null);
        }

        private void OnDifficultyChosen(CheckersDifficulty difficulty)
        {
            _difficulty = difficulty;
            StartGame();
        }

        private void StartGame()
        {
            CheckersSaveData.Clear();   // a new game replaces the saved one
            BeginGame(new CheckersGame());
        }

        private void OnContinue()
        {
            CheckersSaveData save = CheckersSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _difficulty = (CheckersDifficulty)save.Difficulty;
            BeginGame(save.ToGame());
        }

        private void BeginGame(CheckersGame game)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _game = game;
            _ai = new CheckersAI(_difficulty, new System.Random());
            ClearSelection();
            _lastFrom = -1;
            _lastTo = -1;
            NextTurn();
        }

        // Decides what happens now: the game ended, the computer plays, or the player plays.
        private void NextTurn()
        {
            Refresh();

            if (_game.Status != CheckersStatus.Playing)
            {
                _phase = Phase.Over;
                CheckersSaveData.Clear();   // finished: nothing to continue
                StartCoroutine(ResultRoutine());
                return;
            }

            if (_game.Turn == CheckersSide.Black)
            {
                _phase = Phase.ComputerTurn;
                Refresh();
                StartCoroutine(ComputerRoutine());
                return;
            }

            _phase = Phase.PlayerTurn;
            Refresh();
        }

        // ---- Input ------------------------------------------------------------

        private void OnSquareClicked(int square)
        {
            if (_phase != Phase.PlayerTurn) return;

            // A destination of the selected piece: play the move.
            if (_selected >= 0 && _targets.Contains(square))
            {
                PlayerMove(FindMove(_selected, square));
                return;
            }

            // One of the player's pieces that can move: select it.
            List<int> targets = TargetsOf(square);
            if (CheckersBoard.IsWhite(_game.Board.Get(square)) && targets.Count > 0)
            {
                _selected = square;
                _targets = targets;
                AudioManager.PlaySfx("chess_select");
                Refresh();
                return;
            }

            if (_selected >= 0 || CheckersBoard.IsWhite(_game.Board.Get(square)))
            {
                AudioManager.PlaySfx("solitaire_wrong");   // that piece cannot move (or a capture is required elsewhere)
            }
            ClearSelection();
            Refresh();
        }

        private void OnUndo()
        {
            if (_phase != Phase.PlayerTurn || !_game.Undo()) return;

            AudioManager.PlaySfx("solitaire_select");
            ClearSelection();
            _lastFrom = -1;
            _lastTo = -1;
            CheckersSaveData.Save(_game, _difficulty);
            Refresh();
        }

        // ---- Moves ------------------------------------------------------------

        private void PlayerMove(CheckersMove move)
        {
            ApplyMove(move);
            NextTurn();
        }

        private IEnumerator ComputerRoutine()
        {
            yield return new WaitForSecondsRealtime(ThinkPauseSeconds);

            CheckersMove move = _ai.ChooseMove(_game.Board, CheckersSide.Black);
            ApplyMove(move);
            NextTurn();
        }

        private void ApplyMove(CheckersMove move)
        {
            AudioManager.PlaySfx(move.IsCapture ? "chess_capture" : "chess_move");
            _game.Play(move);
            _lastFrom = move.From;
            _lastTo = move.To;
            ClearSelection();
            CheckersSaveData.Save(_game, _difficulty);
        }

        private IEnumerator ResultRoutine()
        {
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);

            string key;
            switch (_game.Status)
            {
                case CheckersStatus.WhiteWins: key = "checkers.win"; AudioManager.PlaySfx("chess_win"); break;
                case CheckersStatus.BlackWins: key = "checkers.lose"; AudioManager.PlaySfx("chess_lose"); break;
                default: key = "checkers.draw"; AudioManager.PlaySfx("chess_draw"); break;
            }

            _resultPanel.Show(key, StartGame, ShowStartPanel);
        }

        // ---- Helpers ----------------------------------------------------------

        private CheckersMove FindMove(int from, int to)
        {
            for (int i = 0; i < _game.LegalMoves.Count; i++)
            {
                CheckersMove move = _game.LegalMoves[i];
                if (move.From == from && move.To == to) return move;
            }
            return null;
        }

        // The squares the piece on 'square' can move to (final squares of its legal moves).
        private List<int> TargetsOf(int square)
        {
            List<int> targets = new List<int>();
            for (int i = 0; i < _game.LegalMoves.Count; i++)
            {
                CheckersMove move = _game.LegalMoves[i];
                if (move.From == square && !targets.Contains(move.To)) targets.Add(move.To);
            }
            return targets;
        }

        private void ClearSelection()
        {
            _selected = -1;
            _targets = new List<int>();
        }

        private void Refresh()
        {
            _boardView.Render(_game.Board, _selected, _targets, _lastFrom, _lastTo);
            _hud.SetCounts(_game.Board.Count(CheckersSide.White), _game.Board.Count(CheckersSide.Black));
            _hud.SetUndoEnabled(_phase == Phase.PlayerTurn && _game.CanUndo);

            if (_phase == Phase.ComputerTurn) _hud.SetStatus("checkers.thinking");
            else if (_phase == Phase.PlayerTurn) _hud.SetStatus(_game.MustCapture ? "checkers.must_capture" : "checkers.your_turn");
            else _hud.SetStatus("");
        }
    }
}
