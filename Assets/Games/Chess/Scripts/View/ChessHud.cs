using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// Status line on top and the Undo / New game / Back buttons at the bottom.
    /// New game restarts with the same settings; Back returns to the difficulty screen.
    /// </summary>
    public class ChessHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        public event Action UndoClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            SetLabel(_undoButton, "chess.undo");
            SetLabel(_newGameButton, "chess.new_game");
            SetLabel(_backButton, "chess.back");

            _undoButton.onClick.AddListener(OnUndoClicked);
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        /// <summary>
        /// Shows a text by key; it follows the language if it changes while on screen.
        /// </summary>
        public void SetStatus(string textKey)
        {
            LocalizedText.Bind(_statusText, textKey);
        }

        public void SetUndoEnabled(bool enabled)
        {
            _undoButton.interactable = enabled;
        }

        private void OnUndoClicked()
        {
            if (UndoClicked != null) UndoClicked();
        }

        private void OnNewGameClicked()
        {
            if (NewGameClicked != null) NewGameClicked();
        }

        private void OnBackClicked()
        {
            if (BackClicked != null) BackClicked();
        }

        private static void SetLabel(Button button, string textKey)
        {
            LocalizedText.Bind(button.GetComponentInChildren<TMP_Text>(), textKey);
        }
    }
}
