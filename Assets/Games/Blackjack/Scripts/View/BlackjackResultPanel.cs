using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Blackjack.View
{
    /// <summary>
    /// Shown when the chips run out: a message, Start over / Back.
    /// </summary>
    public class BlackjackResultPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _backButton;

        private Action _onAgain;
        private Action _onBack;
        private bool _initialized;

        public void Show(Action onAgain, Action onBack)
        {
            _onAgain = onAgain;
            _onBack = onBack;

            if (!_initialized)
            {
                _againButton.onClick.AddListener(() => Invoke(_onAgain));
                _backButton.onClick.AddListener(() => Invoke(_onBack));
                _initialized = true;
            }

            LocalizedText.Bind(_title, "bj.broke");
            LocalizedText.Bind(_againButton.GetComponentInChildren<TMP_Text>(), "bj.restart");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "bj.back");

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
