using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Core.UI;
using MobileParadise.Games.Tetris.Logic;
using UnityEngine;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// Connects the Tetris rules (TetrisGame) to the screens.
    /// The piece falls by itself (faster as lines are cleared); the player moves it with the buttons or the finger.
    /// Flow: start screen -> Playing <-> Paused -> result panel. Pause (and leaving the app) saves the game;
    /// "Continue" from the start screen opens it paused.
    /// </summary>
    public class TetrisGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            Paused,
            Over
        }

        private const float ResultPauseSeconds = 1.0f;

        [SerializeField] private TetrisStartPanel _startPanel;
        [SerializeField] private TetrisBoardView _boardView;
        [SerializeField] private TetrisNextView _nextView;
        [SerializeField] private TetrisTouchArea _touchArea;
        [SerializeField] private TetrisHud _hud;
        [SerializeField] private TetrisResultPanel _resultPanel;

        private TetrisGame _game;
        private TetrisLevel _level;
        private Phase _phase;
        private float _timer;
        private int _best;

        private void Start()
        {
            _boardView.Build();
            _nextView.Build();
            _touchArea.Setup(_boardView.Holder, _boardView.CellSize);
            _touchArea.MoveRequested += OnMove;
            _touchArea.RotateRequested += OnRotate;
            _touchArea.DropRequested += OnSoftDrop;

            _hud.Init();
            _hud.MoveClicked += OnMove;
            _hud.RotateClicked += OnRotate;
            _hud.DropClicked += OnSoftDrop;
            _hud.PauseClicked += TogglePause;
            _hud.NewGameClicked += StartGame;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        private void Update()
        {
            if (_phase != Phase.Playing || UiPause.IsOpen) return;   // stops while settings / music / help are open

            _timer += Time.unscaledDeltaTime;
            if (_timer < TetrisLevels.StepSeconds(_level, _game.Lines)) return;

            _timer = 0f;
            Apply(_game.Drop(false));
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Pause();
        }

        private void OnApplicationQuit()
        {
            SaveIfActive();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            SaveIfActive();
            StopAllCoroutines();
            _phase = Phase.Choosing;
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(OnLevelChosen, OnContinue, TetrisSaveData.Load() != null);
        }

        private void OnLevelChosen(TetrisLevel level)
        {
            _level = level;
            StartGame();
        }

        private void StartGame()
        {
            TetrisSaveData.Clear();   // a new game replaces the saved one
            BeginGame(new TetrisGame(new System.Random()), false);
        }

        private void OnContinue()
        {
            TetrisSaveData save = TetrisSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _level = (TetrisLevel)save.Level;
            BeginGame(save.ToGame(), true);
        }

        private void BeginGame(TetrisGame game, bool startPaused)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _game = game;
            _best = TetrisBestData.Load(_level);
            _timer = 0f;
            _phase = startPaused ? Phase.Paused : Phase.Playing;
            _hud.SetPaused(startPaused);
            Refresh();
        }

        private void TogglePause()
        {
            if (_phase == Phase.Playing) Pause();
            else if (_phase == Phase.Paused) Resume();
        }

        private void Pause()
        {
            if (_phase != Phase.Playing) return;

            _phase = Phase.Paused;
            _hud.SetPaused(true);
            SaveIfActive();
        }

        private void Resume()
        {
            _timer = 0f;
            _phase = Phase.Playing;
            _hud.SetPaused(false);
        }

        // ---- Input ------------------------------------------------------------

        private void OnMove(int direction)
        {
            if (_phase != Phase.Playing) return;

            if (_game.MoveHorizontal(direction)) Refresh();
        }

        private void OnRotate()
        {
            if (_phase != Phase.Playing) return;

            if (_game.Rotate())
            {
                AudioManager.PlaySfx("sudoku_select");
                Refresh();
            }
        }

        // Pressing Down: the piece falls one row now, and the automatic timer starts over.
        private void OnSoftDrop()
        {
            if (_phase != Phase.Playing) return;

            _timer = 0f;
            Apply(_game.Drop(true));
        }

        // ---- Steps ------------------------------------------------------------

        private void Apply(TetrisStep step)
        {
            if (step != TetrisStep.Moved)
            {
                AudioManager.PlaySfx(_game.LastCleared > 0 ? "solitaire_found" : "solitaire_place");
                UpdateBest();
            }
            Refresh();

            if (step == TetrisStep.Locked)
            {
                TetrisSaveData.Save(_game, _level);
            }
            else if (step == TetrisStep.Over)
            {
                _phase = Phase.Over;
                TetrisSaveData.Clear();   // finished: nothing to continue
                StartCoroutine(ResultRoutine());
            }
        }

        private void UpdateBest()
        {
            if (_game.Score <= _best) return;

            _best = _game.Score;
            TetrisBestData.Save(_level, _best);
        }

        private IEnumerator ResultRoutine()
        {
            AudioManager.PlaySfx("mines_boom");
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);

            _resultPanel.Show("tetris.over", StartGame, ShowStartPanel);
        }

        // ---- Helpers ----------------------------------------------------------

        private void SaveIfActive()
        {
            if (_game != null && (_phase == Phase.Playing || _phase == Phase.Paused))
            {
                TetrisSaveData.Save(_game, _level);
            }
        }

        private void Refresh()
        {
            _boardView.Render(_game, TetrisLevels.ShowGhost(_level));
            _nextView.Render(_game.NextType);
            _hud.SetStats(_game.Score, _best, _game.Lines, _game.Level);
        }
    }
}
