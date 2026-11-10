using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// Score, best, lines and level on top; the Left / Rotate / Right / Down buttons; and Pause / New game / Back.
    /// While paused, a "Paused" panel covers the board and the Pause button says "Continue".
    /// </summary>
    public class TetrisHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _bestText;
        [SerializeField] private TMP_Text _linesText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _pausedText;
        [SerializeField] private GameObject _pausedPanel;
        [SerializeField] private TMP_Text _nextLabel;
        [SerializeField] private TetrisHoldButton _leftButton;
        [SerializeField] private TetrisHoldButton _rightButton;
        [SerializeField] private TetrisHoldButton _downButton;
        [SerializeField] private Button _rotateButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _score;
        private int _best;
        private int _lines;
        private int _level = 1;

        public event Action<int> MoveClicked;
        public event Action RotateClicked;
        public event Action DropClicked;
        public event Action PauseClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_leftButton.GetComponentInChildren<TMP_Text>(), "tetris.left");
            LocalizedText.Bind(_rightButton.GetComponentInChildren<TMP_Text>(), "tetris.right");
            LocalizedText.Bind(_downButton.GetComponentInChildren<TMP_Text>(), "tetris.down");
            LocalizedText.Bind(_rotateButton.GetComponentInChildren<TMP_Text>(), "tetris.rotate");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "tetris.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "tetris.back");
            LocalizedText.Bind(_pausedText, "tetris.paused");
            LocalizedText.Bind(_nextLabel, "tetris.next");

            _leftButton.Triggered += () => { if (MoveClicked != null) MoveClicked(-1); };
            _rightButton.Triggered += () => { if (MoveClicked != null) MoveClicked(1); };
            _downButton.Triggered += () => { if (DropClicked != null) DropClicked(); };
            _rotateButton.onClick.AddListener(() => { if (RotateClicked != null) RotateClicked(); });
            _pauseButton.onClick.AddListener(() => { if (PauseClicked != null) PauseClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });

            SetPaused(false);
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTexts;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTexts;
        }

        public void SetStats(int score, int best, int lines, int level)
        {
            _score = score;
            _best = best;
            _lines = lines;
            _level = level;
            ShowTexts();
        }

        public void SetPaused(bool paused)
        {
            _pausedPanel.SetActive(paused);
            LocalizedText.Bind(_pauseButton.GetComponentInChildren<TMP_Text>(), paused ? "common.continue" : "tetris.pause");
        }

        private void ShowTexts()
        {
            _scoreText.text = Texts.Get("tetris.score") + ": " + _score;
            _bestText.text = Texts.Get("tetris.best") + ": " + _best;
            _linesText.text = Texts.Get("tetris.lines") + ": " + _lines;
            _levelText.text = Texts.Get("tetris.level") + ": " + _level;
        }
    }
}
