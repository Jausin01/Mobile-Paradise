using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// Shown when the lives run out: "Game over" with the final score, Play again / Back.
    /// </summary>
    public class BreakoutResultPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _backButton;

        private Action _onAgain;
        private Action _onBack;
        private bool _initialized;

        public void Show(int score, Action onAgain, Action onBack)
        {
            _onAgain = onAgain;
            _onBack = onBack;

            if (!_initialized)
            {
                _againButton.onClick.AddListener(() => Invoke(_onAgain));
                _backButton.onClick.AddListener(() => Invoke(_onBack));
                _initialized = true;
            }

            _title.text = Texts.Get("breakout.game_over") + "\n" + Texts.Get("breakout.score") + ": " + score;
            LocalizedText.Bind(_againButton.GetComponentInChildren<TMP_Text>(), "breakout.play_again");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "breakout.back");

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Invoke(Action action)
        {
            Hide();
            if (action != null) action();
        }
    }
}
