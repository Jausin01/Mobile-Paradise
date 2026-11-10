using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Sudoku.Logic;
using UnityEngine;

namespace MobileParadise.Games.Sudoku.View
{
    /// <summary>
    /// Connects the Sudoku rules (SudokuGame) to the screens.
    /// Flow: start screen -> playing -> win panel -> (play again | back to the start screen).
    /// </summary>
    public class SudokuGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            Won
        }

        private const float WinPauseSeconds = 0.6f;

        [SerializeField] private SudokuStartPanel _startPanel;
        [SerializeField] private SudokuBoardView _boardView;
        [SerializeField] private SudokuNumberPad _numberPad;
        [SerializeField] private SudokuHud _hud;
        [SerializeField] private SudokuWinPanel _winPanel;

        private SudokuGame _game;
        private SudokuLevel _level;
        private Phase _phase;
        private int _selected = -1;

        private void Start()
        {
            _boardView.Build();
            _boardView.CellClicked += OnCellClicked;

            _numberPad.Init();
            _numberPad.DigitClicked += OnDigitClicked;
            _numberPad.EraseClicked += OnEraseClicked;

            _hud.Init();
            _hud.NewGameClicked += StartGame;
            _hud.HintClicked += OnHintClicked;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            StopAllCoroutines();
            _phase = Phase.Choosing;
            _winPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(OnLevelChosen, OnContinue, SudokuSaveData.Load() != null);
        }

        private void OnLevelChosen(SudokuLevel level)
        {
            _level = level;
            StartGame();
        }

        private void StartGame()
        {
            StopAllCoroutines();
            _winPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _game = new SudokuGame(SudokuGenerator.Generate(_level, new System.Random()));
            _selected = -1;
            _phase = Phase.Playing;

            _hud.SetLevel(SudokuStartPanel.LevelKeys[(int)_level]);
            Refresh();
            SudokuSaveData.Save(_game, _level);
        }

        /// <summary>
        /// Picks up the saved game with the numbers the player had already filled in.
        /// </summary>
        private void OnContinue()
        {
            SudokuSaveData save = SudokuSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            StopAllCoroutines();
            _winPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _level = (SudokuLevel)save.Level;
            _game = save.ToGame();
            _selected = -1;
            _phase = Phase.Playing;

            _hud.SetLevel(SudokuStartPanel.LevelKeys[(int)_level]);
            Refresh();
        }

        // ---- Input ------------------------------------------------------------

        private void OnCellClicked(int index)
        {
            if (_phase != Phase.Playing) return;

            _selected = index;
            AudioManager.PlaySfx("sudoku_select");
            Refresh();
        }

        private void OnDigitClicked(int digit)
        {
            if (_phase != Phase.Playing || _selected < 0) return;

            PlaceResult result = _game.Place(_selected, digit);
            if (result == PlaceResult.Ignored) return;

            AudioManager.PlaySfx(result == PlaceResult.Correct ? "sudoku_place" : "sudoku_wrong");
            AfterChange();
        }

        private void OnEraseClicked()
        {
            if (_phase != Phase.Playing || _selected < 0) return;
            if (_game.Clear(_selected) == PlaceResult.Ignored) return;

            AudioManager.PlaySfx("sudoku_select");
            AfterChange();
        }

        /// <summary>
        /// Fills the selected cell with the right number. If no usable cell is selected,
        /// it picks the first cell that still needs work.
        /// </summary>
        private void OnHintClicked()
        {
            if (_phase != Phase.Playing) return;

            if (_selected < 0 || !_game.NeedsWork(_selected))
            {
                _selected = _game.FindCellNeedingWork();
            }
            if (_selected < 0 || !_game.Hint(_selected)) return;

            AudioManager.PlaySfx("sudoku_place");
            AfterChange();
        }

        // ---- Helpers ----------------------------------------------------------

        private void AfterChange()
        {
            Refresh();

            if (_game.IsComplete)
            {
                _phase = Phase.Won;
                SudokuSaveData.Clear();   // finished: nothing to continue
                StartCoroutine(WinRoutine());
            }
            else
            {
                SudokuSaveData.Save(_game, _level);
            }
        }

        private IEnumerator WinRoutine()
        {
            yield return new WaitForSecondsRealtime(WinPauseSeconds);
            AudioManager.PlaySfx("sudoku_win");
            _winPanel.Show(StartGame, ShowStartPanel);
        }

        private void Refresh()
        {
            _boardView.Render(_game, _selected);

            for (int digit = 1; digit <= 9; digit++)
            {
                _numberPad.SetDigitAvailable(digit, _game.CountOf(digit) < 9);
            }
        }
    }
}
