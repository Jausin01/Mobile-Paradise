using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Core.Localization;
using MobileParadise.Games.WordSearch.Logic;
using UnityEngine;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// Connects the puzzle (WordSearchPuzzle) to the screens. Every new game makes a fresh puzzle.
    /// The player finds a word by dragging over its letters, or by tapping its first and then its last letter.
    /// Flow: start screen -> playing -> result panel. The puzzle is saved after every word found.
    /// </summary>
    public class WordSearchGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            Over
        }

        private const float ResultPauseSeconds = 1.0f;

        [SerializeField] private WordSearchStartPanel _startPanel;
        [SerializeField] private WordSearchBoardView _boardView;
        [SerializeField] private WordListView _wordList;
        [SerializeField] private WordSearchHud _hud;
        [SerializeField] private WordSearchResultPanel _resultPanel;

        private WordSearchPuzzle _puzzle;
        private WordSearchLevel _level;
        private int _category = -1;
        private Phase _phase;
        private int[] _preview;
        private int _tapCell = -1;
        private int _hintCell = -1;
        private readonly System.Random _random = new System.Random();

        private void Start()
        {
            _boardView.Init();
            _boardView.LinePreviewed += OnLinePreviewed;
            _boardView.LineReleased += OnLineReleased;
            _boardView.CellTapped += OnCellTapped;

            _hud.Init();
            _hud.HintClicked += OnHint;
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
            _startPanel.Show(OnLevelChosen, OnContinue, WordSearchSaveData.Load() != null, _category);
        }

        private void OnLevelChosen(WordSearchLevel level, int category)
        {
            _level = level;
            _category = category;
            StartGame();
        }

        // A fresh puzzle with the same level and theme.
        private void StartGame()
        {
            WordSearchPuzzle puzzle = WordSearchGenerator.Generate(WordBank.WordsFor(_category, Texts.IsPortuguese), _level, _random);
            if (puzzle == null)
            {
                Debug.LogError("Word Search: could not make a puzzle (not enough words in the category).");
                ShowStartPanel();
                return;
            }

            BeginGame(puzzle);
            WordSearchSaveData.Save(_puzzle, _level, _category);
        }

        private void OnContinue()
        {
            WordSearchSaveData save = WordSearchSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _level = (WordSearchLevel)save.Level;
            _category = save.Category;
            BeginGame(save.ToPuzzle());
        }

        private void BeginGame(WordSearchPuzzle puzzle)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _puzzle = puzzle;
            _phase = Phase.Playing;
            _preview = null;
            _tapCell = -1;
            _hintCell = -1;

            _boardView.Build(_puzzle.Size);
            _hud.SetCategory(_category);
            Refresh();

            if (_puzzle.AllFound) Finish();   // a saved puzzle that was already complete
        }

        private void Finish()
        {
            _phase = Phase.Over;
            WordSearchSaveData.Clear();   // finished: nothing to continue
            StartCoroutine(ResultRoutine());
        }

        private IEnumerator ResultRoutine()
        {
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);

            AudioManager.PlaySfx("mines_win");
            _resultPanel.Show("ws.win", StartGame, ShowStartPanel);
        }

        // ---- Input ------------------------------------------------------------

        private void OnLinePreviewed(int from, int to)
        {
            if (_phase != Phase.Playing) return;

            _preview = from < 0 ? null : _puzzle.LineBetween(from, to);
            Refresh();
        }

        private void OnLineReleased(int from, int to)
        {
            if (_phase != Phase.Playing) return;

            _tapCell = -1;
            Claim(from, to);
        }

        private void OnCellTapped(int cell)
        {
            if (_phase != Phase.Playing) return;

            if (_tapCell < 0)
            {
                _tapCell = cell;
                _hintCell = -1;
                AudioManager.PlaySfx("sudoku_select");
                Refresh();
                return;
            }

            int first = _tapCell;
            _tapCell = -1;
            if (first == cell)
            {
                Refresh();   // tapping the same letter again cancels
                return;
            }
            Claim(first, cell);
        }

        private void Claim(int from, int to)
        {
            _hintCell = -1;

            if (_puzzle.TryClaim(from, to) < 0)
            {
                AudioManager.PlaySfx("solitaire_wrong");
                Refresh();
                return;
            }

            AudioManager.PlaySfx("solitaire_found");
            WordSearchSaveData.Save(_puzzle, _level, _category);
            Refresh();
            if (_puzzle.AllFound) Finish();
        }

        private void OnHint()
        {
            if (_phase != Phase.Playing) return;

            _tapCell = -1;
            _hintCell = _puzzle.HintCell(_random);
            AudioManager.PlaySfx("solitaire_select");
            Refresh();
        }

        // ---- Helpers ----------------------------------------------------------

        private void Refresh()
        {
            _boardView.Render(_puzzle, _preview, _tapCell, _hintCell);
            _wordList.Render(_puzzle);
            _hud.SetProgress(_puzzle.FoundCount, _puzzle.WordCount);
            _hud.SetHintEnabled(_phase == Phase.Playing && !_puzzle.AllFound);
        }
    }
}
