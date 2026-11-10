using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Checkers.View
{
    /// <summary>
    /// Whose turn it is and the piece count on top; Undo / New game / Back at the bottom.
    /// </summary>
    public class CheckersHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private string _statusKey = "";
        private int _whitePieces;
        private int _blackPieces;

        public event Action UndoClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_undoButton.GetComponentInChildren<TMP_Text>(), "checkers.undo");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "checkers.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "checkers.back");

            _undoButton.onClick.AddListener(() => { if (UndoClicked != null) UndoClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTexts;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTexts;
        }

        public void SetStatus(string key)
        {
            _statusKey = key;
            ShowTexts();
        }

        public void SetCounts(int whitePieces, int blackPieces)
        {
            _whitePieces = whitePieces;
            _blackPieces = blackPieces;
            ShowTexts();
        }

        public void SetUndoEnabled(bool enabled)
        {
            _undoButton.interactable = enabled;
        }

        private void ShowTexts()
        {
            _statusText.text = _statusKey.Length > 0 ? Texts.Get(_statusKey) : "";
            _countText.text = Texts.Get("checkers.you") + " " + _whitePieces + "  x  " + _blackPieces + " " + Texts.Get("checkers.computer");
        }
    }
}
