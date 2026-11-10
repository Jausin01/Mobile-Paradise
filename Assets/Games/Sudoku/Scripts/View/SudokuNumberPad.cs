using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Sudoku.View
{
    /// <summary>
    /// Buttons 1-9 and Erase. A digit button is disabled once all nine of that digit are on the board.
    /// </summary>
    public class SudokuNumberPad : MonoBehaviour
    {
        [SerializeField] private Button[] _digitButtons;   // index 0 = digit 1
        [SerializeField] private Button _eraseButton;

        public event Action<int> DigitClicked;
        public event Action EraseClicked;

        public void Init()
        {
            for (int i = 0; i < _digitButtons.Length; i++)
            {
                int digit = i + 1;
                _digitButtons[i].onClick.AddListener(() => OnDigitClicked(digit));
            }

            _eraseButton.onClick.AddListener(OnEraseClicked);
            LocalizedText.Bind(_eraseButton.GetComponentInChildren<TMP_Text>(), "sudoku.erase");
        }

        public void SetDigitAvailable(int digit, bool available)
        {
            _digitButtons[digit - 1].interactable = available;
        }

        private void OnDigitClicked(int digit)
        {
            if (DigitClicked != null) DigitClicked(digit);
        }

        private void OnEraseClicked()
        {
            if (EraseClicked != null) EraseClicked();
        }
    }
}
