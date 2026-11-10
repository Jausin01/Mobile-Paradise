using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// Level, score and lives on top; a hint and the Pause / New game / Back buttons at the bottom.
    /// </summary>
    public class BreakoutHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _livesText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _level;
        private int _score;
        private int _lives;
        private string _hintKey = "";

        public event Action PauseClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_pauseButton.GetComponentInChildren<TMP_Text>(), "breakout.pause");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "breakout.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "breakout.back");

            _pauseButton.onClick.AddListener(() => { if (PauseClicked != null) PauseClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTexts;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTexts;
        }

        public void SetStats(int level, int score, int lives)
        {
            _level = level;
            _score = score;
            _lives = lives;
            ShowTexts();
        }

        /// <summary>
        /// The hint under the board; an empty key hides it.
        /// </summary>
        public void SetHint(string key)
        {
            _hintKey = key;
            ShowTexts();
        }

        private void ShowTexts()
        {
            _levelText.text = Texts.Get("breakout.level") + ": " + _level;
            _scoreText.text = Texts.Get("breakout.score") + ": " + _score;
            _livesText.text = Texts.Get("breakout.lives") + ": " + _lives;

            _hintText.gameObject.SetActive(_hintKey.Length > 0);
            if (_hintKey.Length > 0) _hintText.text = Texts.Get(_hintKey);
        }
    }
}
