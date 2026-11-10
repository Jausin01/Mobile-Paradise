using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// Shown when all pairs are found: congratulations, the number of moves, Play again / Back.
    /// </summary>
    public class MemoryWinPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _movesLabel;
        [SerializeField] private TMP_Text _movesValue;
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _backButton;

        private Action _onAgain;
        private Action _onBack;
        private bool _initialized;

        public void Show(int moves, Action onAgain, Action onBack)
        {
            _onAgain = onAgain;
            _onBack = onBack;

            if (!_initialized)
            {
                _againButton.onClick.AddListener(() => Invoke(_onAgain));
                _backButton.onClick.AddListener(() => Invoke(_onBack));
                _initialized = true;
            }

            LocalizedText.Bind(_title, "memory.win");
            LocalizedText.Bind(_movesLabel, "memory.moves");
            LocalizedText.Bind(_againButton.GetComponentInChildren<TMP_Text>(), "memory.play_again");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "memory.back");
            _movesValue.text = moves.ToString();

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
