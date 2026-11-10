using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// Level and move count on top; Undo / New game / Back buttons at the bottom.
    /// New game deals again with the same level; Back returns to the level screen.
    /// </summary>
    public class SolitaireHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _movesText;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _moves;

        public event Action UndoClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_undoButton.GetComponentInChildren<TMP_Text>(), "solitaire.undo");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "solitaire.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "solitaire.back");

            _undoButton.onClick.AddListener(() => { if (UndoClicked != null) UndoClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowMoves;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowMoves;
        }

        public void SetLevel(string textKey)
        {
            LocalizedText.Bind(_levelText, textKey);
        }

        public void SetMoves(int moves)
        {
            _moves = moves;
            ShowMoves();
        }

        public void SetUndoEnabled(bool enabled)
        {
            _undoButton.interactable = enabled;
        }

        private void ShowMoves()
        {
            _movesText.text = Texts.Get("solitaire.moves") + ": " + _moves;
        }
    }
}
