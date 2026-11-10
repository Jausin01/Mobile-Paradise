using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Sudoku.View
{
    /// <summary>
    /// Level name on top; New game / Hint / Back buttons at the bottom.
    /// New game starts a new puzzle of the same level; Back returns to the level screen.
    /// </summary>
    public class SudokuHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _backButton;

        public event Action NewGameClicked;
        public event Action HintClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "sudoku.new_game");
            LocalizedText.Bind(_hintButton.GetComponentInChildren<TMP_Text>(), "sudoku.hint");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "sudoku.back");

            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _hintButton.onClick.AddListener(OnHintClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        public void SetLevel(string textKey)
        {
            LocalizedText.Bind(_levelText, textKey);
        }

        private void OnNewGameClicked()
        {
            if (NewGameClicked != null) NewGameClicked();
        }

        private void OnHintClicked()
        {
            if (HintClicked != null) HintClicked();
        }

        private void OnBackClicked()
        {
            if (BackClicked != null) BackClicked();
        }
    }
}
