using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// Shown when a round ends: who won, the points and the score; Next round / Back.
    /// </summary>
    public class DominoResultPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _detail;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _backButton;

        private Action _onNext;
        private Action _onBack;
        private bool _initialized;

        public void Show(string titleKey, string detail, Action onNext, Action onBack)
        {
            _onNext = onNext;
            _onBack = onBack;

            if (!_initialized)
            {
                _nextButton.onClick.AddListener(() => Invoke(_onNext));
                _backButton.onClick.AddListener(() => Invoke(_onBack));
                _initialized = true;
            }

            LocalizedText.Bind(_title, titleKey);
            _detail.text = detail;
            LocalizedText.Bind(_nextButton.GetComponentInChildren<TMP_Text>(), "domino.next_round");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "domino.back");

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
