using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// Score and best score on top; the swipe hint and the Pause / New game / Back buttons at the bottom.
    /// </summary>
    public class SnakeHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _bestText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _score;
        private int _best;

        public event Action PauseClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_hintText, "snake.hint");
            LocalizedText.Bind(_pauseButton.GetComponentInChildren<TMP_Text>(), "snake.pause");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "snake.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "snake.back");

            _pauseButton.onClick.AddListener(() => { if (PauseClicked != null) PauseClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowScores;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowScores;
        }

        public void SetScores(int score, int best)
        {
            _score = score;
            _best = best;
            ShowScores();
        }

        /// <summary>
        /// The hint is visible while the snake waits for a swipe.
        /// </summary>
        public void SetHintVisible(bool visible)
        {
            _hintText.gameObject.SetActive(visible);
        }

        private void ShowScores()
        {
            _scoreText.text = Texts.Get("snake.score") + ": " + _score;
            _bestText.text = Texts.Get("snake.best") + ": " + _best;
        }
    }
}
