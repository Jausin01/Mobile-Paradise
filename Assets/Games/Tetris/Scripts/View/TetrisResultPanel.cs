using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// Shown when the game ends (the pieces reach the top): a message, Play again / Back.
    /// </summary>
    public class TetrisResultPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _backButton;

        private Action _onAgain;
        private Action _onBack;
        private bool _initialized;

        public void Show(string titleKey, Action onAgain, Action onBack)
        {
            _onAgain = onAgain;
            _onBack = onBack;

            if (!_initialized)
            {
                _againButton.onClick.AddListener(() => Invoke(_onAgain));
                _backButton.onClick.AddListener(() => Invoke(_onBack));
                _initialized = true;
            }

            LocalizedText.Bind(_title, titleKey);
            LocalizedText.Bind(_againButton.GetComponentInChildren<TMP_Text>(), "tetris.play_again");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "tetris.back");

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
