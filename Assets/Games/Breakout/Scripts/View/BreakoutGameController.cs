using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Core.UI;
using MobileParadise.Games.Breakout.Logic;
using UnityEngine;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// Connects the Breakout rules (BreakoutWorld) to the screens.
    /// Flow: start screen -> Ready (ball on the paddle, tap to launch) -> Playing -> next level, or Game over.
    /// Pause (and leaving the app) stops the ball; a tap continues.
    /// The game is saved when a level starts, so Continue returns to the start of that level.
    /// </summary>
    public class BreakoutGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Ready,
            Playing,
            Paused,
            Between      // level cleared or game over: waiting a moment
        }

        private const float FixedStep = 1f / 120f;
        private const float MaxFrameTime = 0.05f;
        private const float BetweenPauseSeconds = 1.2f;

        [SerializeField] private BreakoutStartPanel _startPanel;
        [SerializeField] private BreakoutFieldView _fieldView;
        [SerializeField] private BreakoutInputArea _inputArea;
        [SerializeField] private BreakoutHud _hud;
        [SerializeField] private BreakoutResultPanel _resultPanel;

        private BreakoutWorld _world;
        private BreakoutDifficulty _difficulty;
        private Phase _phase;
        private float _accumulated;

        private void Start()
        {
            _fieldView.Build();
            _inputArea.PaddleMoved += OnPaddleMoved;
            _inputArea.Tapped += OnTapped;

            _hud.Init();
            _hud.PauseClicked += Pause;
            _hud.NewGameClicked += StartGame;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        private void Update()
        {
            if (_phase != Phase.Playing || UiPause.IsOpen) return;   // stops while settings / music / help are open

            _accumulated += Mathf.Min(Time.unscaledDeltaTime, MaxFrameTime);
            while (_accumulated >= FixedStep && _phase == Phase.Playing)
            {
                _accumulated -= FixedStep;
                HandleEvents(_world.Step(FixedStep));
            }

            Refresh();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Pause();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            StopAllCoroutines();
            _phase = Phase.Choosing;
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(OnDifficultyChosen, OnContinue, BreakoutSaveData.Load() != null);
        }

        private void OnDifficultyChosen(BreakoutDifficulty difficulty)
        {
            _difficulty = difficulty;
            StartGame();
        }

        private void StartGame()
        {
            BeginLevel(1, 0, BreakoutSettings.StartLives);
        }

        private void OnContinue()
        {
            BreakoutSaveData save = BreakoutSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _difficulty = (BreakoutDifficulty)save.Difficulty;
            BeginLevel(save.Level, save.Score, save.Lives);
        }

        private void BeginLevel(int level, int score, int lives)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            BreakoutSaveData.Save(_difficulty, level, score, lives);
            _world = new BreakoutWorld(_difficulty, level, score, lives, new System.Random());
            _accumulated = 0f;
            _phase = Phase.Ready;
            _hud.SetHint("breakout.hint_launch");
            Refresh();
        }

        private void Pause()
        {
            if (_phase != Phase.Playing) return;

            _phase = Phase.Paused;
            _hud.SetHint("breakout.hint_paused");
        }

        // ---- Input ------------------------------------------------------------

        private void OnPaddleMoved(float x)
        {
            if (_phase != Phase.Ready && _phase != Phase.Playing) return;

            _world.MovePaddleTo(x);
            if (_phase == Phase.Ready) Refresh();
        }

        private void OnTapped()
        {
            if (_phase == Phase.Ready)
            {
                _world.Launch();
                _accumulated = 0f;
                _phase = Phase.Playing;
                _hud.SetHint("");
            }
            else if (_phase == Phase.Paused)
            {
                _accumulated = 0f;
                _phase = Phase.Playing;
                _hud.SetHint("");
            }
        }

        // ---- Events and helpers -----------------------------------------------

        private void HandleEvents(BreakoutEvents events)
        {
            if (events == BreakoutEvents.None) return;

            if ((events & BreakoutEvents.BrickBroken) != 0) AudioManager.PlaySfx("sudoku_place");
            else if ((events & BreakoutEvents.Brick) != 0) AudioManager.PlaySfx("sudoku_select");
            else if ((events & BreakoutEvents.Paddle) != 0) AudioManager.PlaySfx("chess_move");

            if ((events & BreakoutEvents.GameOver) != 0)
            {
                _phase = Phase.Between;
                BreakoutSaveData.Clear();
                StartCoroutine(GameOverRoutine());
            }
            else if ((events & BreakoutEvents.LifeLost) != 0)
            {
                AudioManager.PlaySfx("solitaire_wrong");
                _phase = Phase.Ready;
                _hud.SetHint("breakout.hint_launch");
            }
            else if ((events & BreakoutEvents.LevelCleared) != 0)
            {
                _phase = Phase.Between;
                StartCoroutine(NextLevelRoutine());
            }
        }

        private IEnumerator NextLevelRoutine()
        {
            Refresh();
            AudioManager.PlaySfx("solitaire_found");
            yield return new WaitForSecondsRealtime(BetweenPauseSeconds);

            BeginLevel(_world.Level + 1, _world.Score, _world.Lives);
        }

        private IEnumerator GameOverRoutine()
        {
            Refresh();
            AudioManager.PlaySfx("chess_lose");
            yield return new WaitForSecondsRealtime(BetweenPauseSeconds);

            _resultPanel.Show(_world.Score, StartGame, ShowStartPanel);
        }

        private void Refresh()
        {
            _fieldView.Render(_world);
            _hud.SetStats(_world.Level, _world.Score, _world.Lives);
        }
    }
}
