using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Minesweeper.Logic;
using UnityEngine;

namespace MobileParadise.Games.Minesweeper.View
{
    /// <summary>
    /// Connects the Minesweeper rules (MinesBoard) to the screens.
    /// Flow: start screen -> playing -> result panel -> (play again | back to the start screen).
    /// A tap either opens the cell or puts a flag, depending on the Open / Flag choice.
    /// </summary>
    public class MinesGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            Over
        }

        private const float WinPauseSeconds = 0.7f;
        private const float LosePauseSeconds = 1.6f;   // long enough to see where the mines were

        [SerializeField] private MinesStartPanel _startPanel;
        [SerializeField] private MinesBoardView _boardView;
        [SerializeField] private MinesHud _hud;
        [SerializeField] private MinesResultPanel _resultPanel;

        private MinesBoard _board;
        private MinesLevel _level;
        private Phase _phase;
        private bool _flagMode;

        private void Start()
        {
            _boardView.CellClicked += OnCellClicked;

            _hud.Init();
            _hud.OpenClicked += () => SetFlagMode(false);
            _hud.FlagClicked += () => SetFlagMode(true);
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
            _startPanel.Show(OnLevelChosen, OnContinue, MinesSaveData.Load() != null);
        }

        private void OnLevelChosen(MinesLevel level)
        {
            _level = level;
            StartGame();
        }

        private void StartGame()
        {
            MinesSaveData.Clear();   // a new game replaces the saved one
            BeginGame(new MinesBoard(MinesLevels.Columns(_level), MinesLevels.Rows(_level), MinesLevels.Mines(_level), new System.Random()));
        }

        /// <summary>
        /// Picks up the saved game: same mines, opened cells and flags.
        /// </summary>
        private void OnContinue()
        {
            MinesSaveData save = MinesSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _level = (MinesLevel)save.Level;
            BeginGame(save.ToBoard());
        }

        private void BeginGame(MinesBoard board)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _board = board;
            _boardView.Build(_board.Columns, _board.Rows);
            _hud.SetLevel(MinesStartPanel.LevelKeys[(int)_level]);
            SetFlagMode(false);
            _phase = Phase.Playing;
            Refresh();
        }

        // ---- Input ------------------------------------------------------------

        private void SetFlagMode(bool flagMode)
        {
            _flagMode = flagMode;
            _hud.SetFlagMode(flagMode);
        }

        private void OnCellClicked(int index)
        {
            if (_phase != Phase.Playing) return;

            if (_flagMode)
            {
                if (!_board.ToggleFlag(index)) return;

                AudioManager.PlaySfx("mines_flag");
                AfterChange();
                return;
            }

            RevealResult result = _board.Reveal(index);
            if (result == RevealResult.Ignored) return;

            if (result == RevealResult.Boom) AudioManager.PlaySfx("mines_boom");
            else AudioManager.PlaySfx("mines_dig");
            AfterChange();
        }

        // ---- Helpers ----------------------------------------------------------

        private void AfterChange()
        {
            Refresh();

            if (_board.Status == MinesStatus.Playing)
            {
                if (_board.IsStarted) MinesSaveData.Save(_board, _level);
                return;
            }

            _phase = Phase.Over;
            MinesSaveData.Clear();   // finished: nothing to continue
            StartCoroutine(ResultRoutine(_board.Status == MinesStatus.Won));
        }

        private IEnumerator ResultRoutine(bool won)
        {
            yield return new WaitForSecondsRealtime(won ? WinPauseSeconds : LosePauseSeconds);

            if (won) AudioManager.PlaySfx("mines_win");
            _resultPanel.Show(won ? "mines.win" : "mines.lose", StartGame, ShowStartPanel);
        }

        private void Refresh()
        {
            _boardView.Render(_board);
            _hud.SetMinesLeft(_board.MinesLeft);
        }
    }
}
