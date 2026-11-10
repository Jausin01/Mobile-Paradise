using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// Builds the grid of cards. Cards are 224 px (32 px art x7) with a gap; the holder is sized to the grid
    /// and shrunk to fit the screen by FitInParent.
    /// </summary>
    public class MemoryBoardView : MonoBehaviour
    {
        private const float CardSize = 224f;
        private const float Gap = 24f;

        [SerializeField] private RectTransform _holder;
        [SerializeField] private Sprite _backSprite;
        [SerializeField] private Sprite _frameSprite;

        public MemoryCardView[] Cards { get; private set; }

        /// <summary>
        /// Replaces the cards with a new grid (columns x rows), all face down.
        /// </summary>
        public void Build(int columns, int rows)
        {
            if (_backSprite == null || _frameSprite == null)
            {
                Debug.LogError("MemoryBoardView: card sprites are not assigned. Rebuild the Memory scene.");
            }

            Clear();

            _holder.sizeDelta = new Vector2(columns * CardSize + (columns - 1) * Gap, rows * CardSize + (rows - 1) * Gap);
            Cards = new MemoryCardView[columns * rows];

            for (int index = 0; index < Cards.Length; index++)
            {
                int column = index % columns;
                int row = index / columns;
                Cards[index] = CreateCard(index, column, row);
            }
        }

        private void Clear()
        {
            if (Cards == null) return;

            for (int i = 0; i < Cards.Length; i++)
            {
                Destroy(Cards[i].gameObject);
            }
            Cards = null;
        }

        private MemoryCardView CreateCard(int index, int column, int row)
        {
            GameObject cardObject = new GameObject("Card " + index, typeof(RectTransform), typeof(Image), typeof(Button), typeof(MemoryCardView));
            cardObject.transform.SetParent(_holder, false);

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);   // turns around its center
            rect.sizeDelta = new Vector2(CardSize, CardSize);
            rect.anchoredPosition = new Vector2(column * (CardSize + Gap) + CardSize / 2f, -(row * (CardSize + Gap) + CardSize / 2f));

            Image background = cardObject.GetComponent<Image>();
            Button button = cardObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;

            GameObject faceObject = new GameObject("Face", typeof(RectTransform), typeof(Image));
            faceObject.transform.SetParent(cardObject.transform, false);
            RectTransform faceRect = faceObject.GetComponent<RectTransform>();
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = new Vector2(24, 24);
            faceRect.offsetMax = new Vector2(-24, -24);
            Image face = faceObject.GetComponent<Image>();
            face.preserveAspect = true;
            face.raycastTarget = false;

            MemoryCardView view = cardObject.GetComponent<MemoryCardView>();
            view.Init(index, background, face, button, _backSprite, _frameSprite);
            return view;
        }
    }
}
