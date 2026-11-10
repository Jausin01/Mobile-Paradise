using System.Collections.Generic;
using MobileParadise.Core.Localization;
using MobileParadise.Games.Blackjack.Logic;
using MobileParadise.Games.Solitaire.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Blackjack.View
{
    /// <summary>
    /// Draws the table: the dealer's cards on top, a message in the middle, the player's cards below,
    /// and each side's total. The cards are the same CardView used by Solitaire.
    /// Cards are created when first needed and reused.
    /// </summary>
    public class BlackjackTableView : MonoBehaviour
    {
        private const float CardWidth = 220f;
        private const float CardHeight = 308f;
        private const float SideMargin = 30f;
        private const float DealerTop = -130f;
        private const float PlayerTop = -810f;

        [SerializeField] private RectTransform _holder;
        [SerializeField] private Sprite _frameSprite;
        [SerializeField] private Sprite[] _suitSprites;
        [SerializeField] private TMP_Text _dealerLabel;
        [SerializeField] private TMP_Text _playerLabel;
        [SerializeField] private TMP_Text _messageText;

        private readonly List<CardView> _dealerCards = new List<CardView>();
        private readonly List<CardView> _playerCards = new List<CardView>();

        private int _dealerTotal = -1;   // -1: no cards yet
        private int _playerTotal = -1;
        private string _messageKey = "";
        private int _messageAmount;

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTexts;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTexts;
        }

        /// <summary>
        /// Shows both hands. The dealer's second card is face down while the player is still playing.
        /// </summary>
        public void Render(BlackjackGame game)
        {
            bool hidden = game.HoleHidden;
            LayoutHand(_dealerCards, game.Dealer, hidden ? 1 : game.Dealer.Count, hidden, DealerTop);
            LayoutHand(_playerCards, game.Player, game.Player.Count, false, PlayerTop);

            _dealerTotal = game.Dealer.Count == 0 ? -1 : game.Dealer.Total(hidden ? 1 : game.Dealer.Count);
            _playerTotal = game.Player.Count == 0 ? -1 : game.Player.Value;
            ShowTexts();
        }

        /// <summary>
        /// The message in the middle: a text key and an optional chip amount (shown with + or -).
        /// </summary>
        public void SetMessage(string key, int amount)
        {
            _messageKey = key;
            _messageAmount = amount;
            ShowTexts();
        }

        // ---- Helpers ----------------------------------------------------------

        private void ShowTexts()
        {
            _dealerLabel.text = Texts.Get("bj.dealer") + (_dealerTotal >= 0 ? ": " + _dealerTotal : "");
            _playerLabel.text = Texts.Get("bj.you") + (_playerTotal >= 0 ? ": " + _playerTotal : "");

            string message = _messageKey.Length > 0 ? Texts.Get(_messageKey) : "";
            if (_messageAmount > 0) message += "  +" + _messageAmount;
            else if (_messageAmount < 0) message += "  -" + (-_messageAmount);
            _messageText.text = message;
        }

        // 'visibleCards' cards of the hand are shown (the rest are not created yet); 'secondHidden' turns the second face down.
        private void LayoutHand(List<CardView> views, BlackjackHand hand, int shownCount, bool secondHidden, float top)
        {
            int count = hand.Count;
            while (views.Count < count)
            {
                views.Add(CreateCard());
            }

            float holderWidth = _holder.sizeDelta.x;
            float step = CardWidth * 0.62f;
            if (count > 1)
            {
                step = Mathf.Min(step, (holderWidth - 2f * SideMargin - CardWidth) / (count - 1));
            }
            float totalWidth = CardWidth + (count > 1 ? (count - 1) * step : 0f);
            float startX = (holderWidth - totalWidth) / 2f;

            for (int i = 0; i < views.Count; i++)
            {
                bool used = i < count;
                views[i].gameObject.SetActive(used);
                if (!used) continue;

                bool faceUp = !(secondHidden && i == 1);
                views[i].Show(hand.Card(i), faceUp, false);
                views[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * step, top);
            }
        }

        private CardView CreateCard()
        {
            GameObject go = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CardView));
            go.transform.SetParent(_holder, false);

            CardView view = go.GetComponent<CardView>();
            view.Init(CardWidth, CardHeight, _frameSprite, _suitSprites);
            go.GetComponent<Image>().raycastTarget = false;   // the cards are only for looking
            return view;
        }
    }
}
