using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// Move counter on top, New game / Back buttons at the bottom.
    /// New game restarts with the same level and theme; Back returns to the level screen.
    /// </summary>
    public class MemoryHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _movesLabel;
        [SerializeField] private TMP_Text _movesValue;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_movesLabel, "memory.moves");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "memory.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "memory.back");

            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        public void SetMoves(int moves)
        {
            _movesValue.text = moves.ToString();
        }

        private void OnNewGameClicked()
        {
            if (NewGameClicked != null) NewGameClicked();
        }

        private void OnBackClicked()
        {
            if (BackClicked != null) BackClicked();
        }
    }
}
