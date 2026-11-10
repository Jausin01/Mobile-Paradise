using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Game2048.Logic;
using UnityEngine;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// Connects the 2048 rules (Board2048) to the screens.
    /// Flow: start screen -> playing -> (2048 message -> keep playing) -> no moves -> result panel.
    /// </summary>
    public class Game2048Controller : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            Paused
        }

        private const float ResultPauseSeconds = 0.6f;

        [SerializeField] private Start2048Panel _startPanel;
        [SerializeField] private Board2048View _boardView;
        [SerializeField] private SwipeArea _swipeArea;
        [SerializeField] private Hud2048 _hud;
        [SerializeField] private Result2048Panel _resultPanel;

        private Board2048 _board;
        private Phase _phase;
        private int _best;
        private bool _winShown;

        private void Start()
        {
            _best = Best2048.Load();

            _boardView.Build();
            _swipeArea.Swiped += OnSwiped;

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
            _startPanel.Show(StartGame, OnContinue, Save2048.Load() != null);
        }

        private void StartGame()
        {
            Save2048.Clear();   // a new game replaces the saved one
            _winShown = false;
            BeginGame(new Board2048(new System.Random()));
        }

        private void OnContinue()
        {
            Save2048 save = Save2048.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _winShown = save.WinShown;
            BeginGame(save.ToBoard());
        }

        private void BeginGame(Board2048 board)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _board = board;
            _phase = Phase.Playing;
            Refresh(false);
        }

        // ---- Input ------------------------------------------------------------

        private void OnSwiped(SlideDirection direction)
        {
            if (_phase != Phase.Playing) return;
            if (!_board.Move(direction)) return;

            AudioManager.PlaySfx(_board.AnyMerged() ? "solitaire_found" : "solitaire_place");
            if (_board.Score > _best)
            {
                _best = _board.Score;
                Best2048.Save(_best);
            }

            Refresh(true);
            Save2048.Save(_board, _winShown);

            if (!_winShown && _board.HighestExponent() >= Board2048.WinExponent)
            {
                _winShown = true;
                Save2048.Save(_board, _winShown);
                _phase = Phase.Paused;
                StartCoroutine(ShowResultRoutine(true));
            }
            else if (!_board.CanMove())
            {
                Save2048.Clear();   // finished: nothing to continue
                _phase = Phase.Paused;
                StartCoroutine(ShowResultRoutine(false));
            }
        }

        private void OnUndo()
        {
            if (_phase != Phase.Playing) return;
            if (!_board.Undo()) return;

            AudioManager.PlaySfx("solitaire_select");
            Refresh(false);
            Save2048.Save(_board, _winShown);
        }

        // ---- Helpers ----------------------------------------------------------

        private IEnumerator ShowResultRoutine(bool won)
        {
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);

            if (won)
            {
                AudioManager.PlaySfx("mines_win");
                _resultPanel.Show("g2048.win", "g2048.keep_playing", KeepPlaying, "g2048.new_game", StartGame);
            }
            else
            {
                AudioManager.PlaySfx("chess_lose");
                _resultPanel.Show("g2048.lose", "g2048.play_again", StartGame, "g2048.back", ShowStartPanel);
            }
        }

        private void KeepPlaying()
        {
            _phase = Phase.Playing;
        }

        private void Refresh(bool animate)
        {
            _boardView.Render(_board, animate);
            _hud.SetScores(_board.Score, _best);
            _hud.SetUndoEnabled(_board.CanUndo);
        }
    }
}
