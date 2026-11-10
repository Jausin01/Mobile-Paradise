using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Chess.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// Pop-up with four pieces to choose from when a pawn reaches the last rank.
    /// </summary>
    public class PromotionPanel : MonoBehaviour
    {
        private static readonly PieceType[] Choices =
        {
            PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight
        };

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Image[] _icons;

        private Action<PieceType> _onChosen;
        private bool _initialized;

        public void Show(PieceSpriteSet pieceSet, PieceColor color, Action<PieceType> onChosen)
        {
            _onChosen = onChosen;

            if (!_initialized)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    int index = i;
                    _buttons[i].onClick.AddListener(() => OnButtonClicked(index));
                }
                _initialized = true;
            }

            LocalizedText.Bind(_title, "chess.promote");
            for (int i = 0; i < Choices.Length; i++)
            {
                _icons[i].sprite = pieceSet.Get(Choices[i], color);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnButtonClicked(int index)
        {
            Action<PieceType> callback = _onChosen;
            Hide();

            if (callback != null)
            {
                callback(Choices[index]);
            }
        }
    }
}
