using System;
using MobileParadise.Games.Solitaire.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// Draws the table: stock and waste and the four foundations on top, the seven columns below.
    /// All 52 cards are created once; Render() puts each one where the game says it is.
    /// Taps are reported as "which pile, which card" (PileClicked); empty places can be tapped too.
    /// </summary>
    public class SolitaireBoardView : MonoBehaviour
    {
        private const float FrameScale = 100f / 32f / 4f;
        private const float Margin = 24f;          // space between the felt edge and the cards
        private const float GapX = 10f;
        private const float TopGap = 36f;          // between the top row and the columns
        private const float FaceDownStep = 22f;    // how much of a face-down card shows
        private const float FaceUpStep = 58f;      // how much of a face-up card shows (rank and suit)
        private const float WasteFan = 38f;        // sideways step of the waste cards when three are drawn
        private const float CardRatio = 1.4f;

        private static readonly Color SlotColor = new Color(0.12f, 0.30f, 0.20f);

        [SerializeField] private RectTransform _holder;
        [SerializeField] private Sprite _frameSprite;
        [SerializeField] private Sprite[] _suitSprites;     // clubs, diamonds, hearts, spades
        [SerializeField] private Sprite _recycleSprite;

        private readonly CardView[] _cards = new CardView[CardInfo.CardCount];
        private Image _stockIcon;
        private float _cardWidth;
        private float _cardHeight;

        public event Action<PileRef, int> PileClicked;

        public void Build()
        {
            Vector2 holder = _holder.sizeDelta;
            _cardWidth = Mathf.Floor((holder.x - 2f * Margin - 6f * GapX) / 7f);
            _cardHeight = Mathf.Round(_cardWidth * CardRatio);

            // Empty places (drawn first, so the cards cover them).
            Image stockSlot = CreateSlot("StockSlot", ColumnX(0), 0f, new PileRef(PileKind.Stock, 0), 0);
            _stockIcon = CreateSlotIcon(stockSlot, _recycleSprite, 0.6f);
            for (int suit = 0; suit < CardInfo.SuitCount; suit++)
            {
                Image slot = CreateSlot("FoundationSlot " + suit, ColumnX(3 + suit), 0f, new PileRef(PileKind.Foundation, suit), 0);
                CreateSlotIcon(slot, _suitSprites[suit], 0.3f).enabled = true;
            }
            for (int column = 0; column < 7; column++)
            {
                CreateSlot("TableauSlot " + column, ColumnX(column), TableauTop, new PileRef(PileKind.Tableau, column), -1);
            }

            for (int card = 0; card < _cards.Length; card++)
            {
                GameObject go = new GameObject("Card " + card, typeof(RectTransform), typeof(Image), typeof(Button), typeof(CardView));
                go.transform.SetParent(_holder, false);

                CardView view = go.GetComponent<CardView>();
                view.Init(_cardWidth, _cardHeight, _frameSprite, _suitSprites);
                view.Clicked += OnCardClicked;
                go.SetActive(false);
                _cards[card] = view;
            }
        }

        public void Render(KlondikeGame game, bool hasSelection, PileRef selectedPile, int selectedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].gameObject.SetActive(false);
            }

            // Stock: only the top card shows.
            if (game.StockCount > 0)
            {
                Place(game.StockTop, new PileRef(PileKind.Stock, 0), 0, ColumnX(0), 0f, false, false);
            }
            _stockIcon.enabled = game.StockCount == 0 && game.WasteCount > 0;

            // Waste: the top card (the top three when three are drawn at a time).
            int shown = Mathf.Min(game.DrawCount, game.WasteCount);
            for (int i = 0; i < shown; i++)
            {
                int index = game.WasteCount - shown + i;
                bool top = i == shown - 1;
                bool selected = top && hasSelection && selectedPile.Kind == PileKind.Waste;
                Place(game.WasteCard(index), new PileRef(PileKind.Waste, 0), index, ColumnX(1) + i * WasteFan, 0f, true, selected);
            }

            // Foundations: only the top card of each shows.
            for (int suit = 0; suit < CardInfo.SuitCount; suit++)
            {
                int top = game.FoundationTop(suit);
                if (top < 0) continue;

                bool selected = hasSelection && selectedPile.Kind == PileKind.Foundation && selectedPile.Index == suit;
                Place(top, new PileRef(PileKind.Foundation, suit), 0, ColumnX(3 + suit), 0f, true, selected);
            }

            for (int column = 0; column < 7; column++)
            {
                RenderColumn(game, column, hasSelection, selectedPile, selectedIndex);
            }
        }

        // ---- Columns ---------------------------------------------------------------

        private void RenderColumn(KlondikeGame game, int column, bool hasSelection, PileRef selectedPile, int selectedIndex)
        {
            int count = game.TableauCount(column);
            if (count == 0) return;

            // Squeeze the steps if a very long column would run off the bottom of the table.
            float available = _holder.sizeDelta.y - Margin - (Margin + TableauTop);
            float needed = _cardHeight;
            for (int i = 0; i < count - 1; i++)
            {
                needed += game.IsFaceUp(column, i) ? FaceUpStep : FaceDownStep;
            }
            float factor = needed > available ? (available - _cardHeight) / (needed - _cardHeight) : 1f;

            float y = TableauTop;
            for (int i = 0; i < count; i++)
            {
                bool faceUp = game.IsFaceUp(column, i);
                bool selected = hasSelection && selectedPile.Kind == PileKind.Tableau && selectedPile.Index == column && i >= selectedIndex;
                Place(game.TableauCard(column, i), new PileRef(PileKind.Tableau, column), i, ColumnX(column), y, faceUp, selected);
                y += (faceUp ? FaceUpStep : FaceDownStep) * factor;
            }
        }

        // ---- Helpers ---------------------------------------------------------------

        // Top of the columns, measured from the top of the holder (cards and slots add the margin themselves).
        private float TableauTop
        {
            get { return _cardHeight + TopGap; }
        }

        private float ColumnX(int column)
        {
            return column * (_cardWidth + GapX);
        }

        private void Place(int card, PileRef pile, int index, float x, float y, bool faceUp, bool selected)
        {
            CardView view = _cards[card];
            view.Pile = pile;
            view.Index = index;
            view.Show(card, faceUp, selected);

            view.GetComponent<RectTransform>().anchoredPosition = new Vector2(Margin + x, -(Margin + y));
            view.gameObject.SetActive(true);
            view.transform.SetAsLastSibling();   // later cards are drawn on top of earlier ones
        }

        private Image CreateSlot(string name, float x, float y, PileRef pile, int index)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_holder, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(_cardWidth, _cardHeight);
            rect.anchoredPosition = new Vector2(Margin + x, -(Margin + y));

            Image image = go.GetComponent<Image>();
            image.sprite = _frameSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = FrameScale;
            image.color = SlotColor;

            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => OnPileClicked(pile, index));
            return image;
        }

        private Image CreateSlotIcon(Image slot, Sprite sprite, float alpha)
        {
            GameObject go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(slot.transform, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(_cardWidth * 0.5f, _cardWidth * 0.5f);

            Image icon = go.GetComponent<Image>();
            icon.sprite = sprite;
            icon.color = new Color(1f, 1f, 1f, alpha);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.enabled = false;
            return icon;
        }

        private void OnCardClicked(CardView card)
        {
            OnPileClicked(card.Pile, card.Index);
        }

        private void OnPileClicked(PileRef pile, int index)
        {
            if (PileClicked != null) PileClicked(pile, index);
        }
    }
}
