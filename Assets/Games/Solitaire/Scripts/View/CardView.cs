using System;
using MobileParadise.Games.Solitaire.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// One playing card on screen: a cream card with its rank and suit, or a blue card back.
    /// The rank and the small suit sit in the top strip, so a card is readable even when others cover it.
    /// Cards are created once and reused; the board view says where each one is.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        private const float FrameScale = 100f / 32f / 4f;   // art pixel = 4 canvas pixels, like the other frames
        private static readonly Color FaceColor = new Color(0.98f, 0.95f, 0.88f);
        private static readonly Color SelectedColor = new Color(0.95f, 0.78f, 0.38f);
        private static readonly Color BackColor = new Color(0.22f, 0.32f, 0.62f);
        private static readonly Color BackInnerColor = new Color(0.32f, 0.44f, 0.78f);
        private static readonly Color RedColor = new Color(0.80f, 0.15f, 0.15f);
        private static readonly Color BlackColor = new Color(0.12f, 0.09f, 0.07f);
        private static readonly string[] RankNames = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        private Image _background;
        private Image _backInner;
        private Image _cornerSuit;
        private Image _centerSuit;
        private TMP_Text _rank;
        private Sprite[] _suits;

        public event Action<CardView> Clicked;

        /// <summary>Which pile this card is in right now (set by the board view).</summary>
        public PileRef Pile { get; set; }

        /// <summary>Position inside the pile (for a column: the card's index).</summary>
        public int Index { get; set; }

        public void Init(float width, float height, Sprite frame, Sprite[] suits)
        {
            _suits = suits;

            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(width, height);

            _background = GetComponent<Image>();
            _background.sprite = frame;
            _background.type = Image.Type.Sliced;
            _background.pixelsPerUnitMultiplier = FrameScale;

            Button button = GetComponent<Button>();
            button.targetGraphic = _background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(OnClicked);

            _backInner = CreateImage("BackInner", Vector2.zero, Vector2.one, new Vector2(18, 18), new Vector2(-18, -18));
            _backInner.sprite = frame;
            _backInner.type = Image.Type.Sliced;
            _backInner.pixelsPerUnitMultiplier = FrameScale;
            _backInner.color = BackInnerColor;

            GameObject rankObject = new GameObject("Rank", typeof(RectTransform), typeof(TextMeshProUGUI));
            rankObject.transform.SetParent(transform, false);
            RectTransform rankRect = rankObject.GetComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0, 1);
            rankRect.anchorMax = new Vector2(0, 1);
            rankRect.pivot = new Vector2(0, 1);
            rankRect.anchoredPosition = new Vector2(10, -2);
            rankRect.sizeDelta = new Vector2(width * 0.55f, 56);
            _rank = rankObject.GetComponent<TextMeshProUGUI>();
            _rank.fontSize = 50;
            _rank.fontStyle = FontStyles.Bold;
            _rank.alignment = TextAlignmentOptions.MidlineLeft;
            _rank.raycastTarget = false;

            float corner = 44f;
            _cornerSuit = CreateImage("CornerSuit", new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            RectTransform cornerRect = _cornerSuit.rectTransform;
            cornerRect.pivot = new Vector2(1, 1);
            cornerRect.sizeDelta = new Vector2(corner, corner);
            cornerRect.anchoredPosition = new Vector2(-10, -8);

            _centerSuit = CreateImage("CenterSuit", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            RectTransform centerRect = _centerSuit.rectTransform;
            centerRect.sizeDelta = new Vector2(width * 0.5f, width * 0.5f);
            centerRect.anchoredPosition = new Vector2(0, -height * 0.12f);
        }

        public void Show(int card, bool faceUp, bool selected)
        {
            _backInner.enabled = !faceUp;
            _rank.enabled = faceUp;
            _cornerSuit.enabled = faceUp;
            _centerSuit.enabled = faceUp;

            if (!faceUp)
            {
                _background.color = BackColor;
                return;
            }

            Color ink = CardInfo.IsRed(card) ? RedColor : BlackColor;
            Sprite suit = _suits[CardInfo.Suit(card)];

            _background.color = selected ? SelectedColor : FaceColor;
            _rank.text = RankNames[CardInfo.Rank(card) - 1];
            _rank.color = ink;
            _cornerSuit.sprite = suit;
            _cornerSuit.color = ink;
            _centerSuit.sprite = suit;
            _centerSuit.color = ink;
        }

        private Image CreateImage(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = go.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private void OnClicked()
        {
            if (Clicked != null) Clicked(this);
        }
    }
}
