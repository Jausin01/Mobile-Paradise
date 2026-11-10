using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Core.UI;
using MobileParadise.Games.Snake.Logic;
using UnityEngine;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// Connects the Snake rules (SnakeGame) to the screens.
    /// Flow: start screen -> Ready (waiting for a swipe) -> Playing -> result panel.
    /// Pause (and leaving the app) goes back to Ready: the next swipe continues the game.
    /// The game is saved when it is paused, when the snake eats and when the app is closed.
    /// </summary>
    public class SnakeGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Ready,
            Playing,
            Over
        }

        private const float ResultPauseSeconds = 0.9f;

        [SerializeField] private SnakeStartPanel _startPanel;
        [SerializeField] private SnakeBoardView _boardView;
        [SerializeField] private SnakeSwipeArea _swipeArea;
        [SerializeField] private SnakeHud _hud;
        [SerializeField] private SnakeResultPanel _resultPanel;

        private SnakeGame _game;
        private SnakeLevel _level;
        private Phase _phase;
        private float _timer;
        private int _best;

        private void Start()
        {
            _swipeArea.Swiped += OnSwiped;

            _hud.Init();
            _hud.PauseClicked += Pause;
            _hud.NewGameClicked += StartGame;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        private void Update()
        {
            if (_phase != Phase.Playing || UiPause.IsOpen) return;   // stops while settings / music / help are open

            _timer += Time.unscaledDeltaTime;
            float interval = SnakeLevels.StepSeconds(_level);
            if (_timer < interval) return;

            _timer = 0f;
            Step();
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
            _startPanel.Show(OnLevelChosen, OnContinue, SnakeSaveData.Load() != null);
        }

        private void OnLevelChosen(SnakeLevel level)
        {
            _level = level;
            StartGame();
        }

        private void StartGame()
        {
            SnakeSaveData.Clear();   // a new game replaces the saved one
            BeginGame(new SnakeGame(SnakeLevels.Columns(_level), SnakeLevels.Rows(_level), SnakeLevels.Wrap(_level), new System.Random()));
        }

        private void OnContinue()
        {
            SnakeSaveData save = SnakeSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _level = (SnakeLevel)save.Level;
            BeginGame(save.ToGame());
        }

        private void BeginGame(SnakeGame game)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _game = game;
            _best = SnakeBestData.Load(_level);
            _boardView.Build(_game.Columns, _game.Rows);
            _timer = 0f;
            SetPhase(Phase.Ready);
            Refresh();
        }

        private void Pause()
        {
            if (_phase != Phase.Playing) return;

            SetPhase(Phase.Ready);
            SaveIfActive();
        }

        // ---- Input and steps --------------------------------------------------

        private void OnSwiped(SnakeDirection direction)
        {
            if (_phase == Phase.Ready)
            {
                // The first swipe starts (or resumes) the game; straight back is not allowed.
                if (direction == SnakeGame.Opposite(_game.Direction)) return;

                _game.Turn(direction);
                _timer = 0f;
                SetPhase(Phase.Playing);
                return;
            }

            if (_phase == Phase.Playing)
            {
                _game.Turn(direction);
            }
        }

        private void Step()
        {
            StepResult result = _game.Step();

            if (result == StepResult.Ate || result == StepResult.Won)
            {
                AudioManager.PlaySfx("sudoku_place");
                if (_game.Score > _best)
                {
                    _best = _game.Score;
                    SnakeBestData.Save(_level, _best);
                }
            }

            Refresh();

            if (result == StepResult.Ate)
            {
                SnakeSaveData.Save(_game, _level);
            }
            else if (result == StepResult.Died || result == StepResult.Won)
            {
                SetPhase(Phase.Over);
                SnakeSaveData.Clear();   // finished: nothing to continue
                StartCoroutine(ResultRoutine(result == StepResult.Won));
            }
        }

        // ---- Helpers ----------------------------------------------------------

        private IEnumerator ResultRoutine(bool won)
        {
            AudioManager.PlaySfx(won ? "mines_win" : "mines_boom");
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);

            _resultPanel.Show(won ? "snake.win" : "snake.lose", StartGame, ShowStartPanel);
        }

        private void SetPhase(Phase phase)
        {
            _phase = phase;
            _hud.SetHintVisible(phase == Phase.Ready);
        }

        private void SaveIfActive()
        {
            if (_game != null && (_phase == Phase.Ready || _phase == Phase.Playing))
            {
                SnakeSaveData.Save(_game, _level);
            }
        }

        private void Refresh()
        {
            _boardView.Render(_game);
            _hud.SetScores(_game.Score, _best);
        }
    }
}
