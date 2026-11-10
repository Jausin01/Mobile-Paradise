using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// Shown when the game reaches 2048 or has no moves left: a message and two buttons.
    /// The texts are passed in as keys, so the same panel serves both cases.
    /// </summary>
    public class Result2048Panel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _firstButton;
        [SerializeField] private Button _secondButton;

        private Action _onFirst;
        private Action _onSecond;
        private bool _initialized;

        public void Show(string titleKey, string firstKey, Action onFirst, string secondKey, Action onSecond)
        {
            _onFirst = onFirst;
            _onSecond = onSecond;

            if (!_initialized)
            {
                _firstButton.onClick.AddListener(() => Invoke(_onFirst));
                _secondButton.onClick.AddListener(() => Invoke(_onSecond));
                _initialized = true;
            }

            LocalizedText.Bind(_title, titleKey);
            LocalizedText.Bind(_firstButton.GetComponentInChildren<TMP_Text>(), firstKey);
            LocalizedText.Bind(_secondButton.GetComponentInChildren<TMP_Text>(), secondKey);

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
