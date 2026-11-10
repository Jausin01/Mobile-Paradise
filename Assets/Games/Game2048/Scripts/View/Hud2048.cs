using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// Score and best score on top; the swipe hint and the Undo / New game / Back buttons at the bottom.
    /// </summary>
    public class Hud2048 : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _bestText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _score;
        private int _best;

        public event Action UndoClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_hintText, "g2048.hint");
            LocalizedText.Bind(_undoButton.GetComponentInChildren<TMP_Text>(), "g2048.undo");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "g2048.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "g2048.back");

            _undoButton.onClick.AddListener(() => { if (UndoClicked != null) UndoClicked(); });
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

        public void SetUndoEnabled(bool enabled)
        {
            _undoButton.interactable = enabled;
        }

        private void ShowScores()
        {
            _scoreText.text = Texts.Get("g2048.score") + ": " + _score;
            _bestText.text = Texts.Get("g2048.best") + ": " + _best;
        }
    }
}
