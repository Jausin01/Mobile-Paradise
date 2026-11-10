using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Blackjack.View
{
    public enum BlackjackHudMode
    {
        Betting,    // bet buttons and Deal
        Playing,    // Hit / Stand / Double
        Waiting     // the dealer is playing: no buttons
    }

    /// <summary>
    /// Chips and bet on top; at the bottom the bet buttons + Deal, or Hit / Stand / Double, and Back.
    /// </summary>
    public class BlackjackHud : MonoBehaviour
    {
        private const int SmallStep = 10;
        private const int BigStep = 50;

        [SerializeField] private TMP_Text _chipsText;
        [SerializeField] private TMP_Text _betText;
        [SerializeField] private GameObject _betRow;
        [SerializeField] private Button _minusBigButton;
        [SerializeField] private Button _minusSmallButton;
        [SerializeField] private Button _plusSmallButton;
        [SerializeField] private Button _plusBigButton;
        [SerializeField] private GameObject _dealRow;
        [SerializeField] private Button _dealButton;
        [SerializeField] private GameObject _actionRow;
        [SerializeField] private Button _hitButton;
        [SerializeField] private Button _standButton;
        [SerializeField] private Button _doubleButton;
        [SerializeField] private Button _backButton;

        private int _chips;
        private int _bet;

        public event Action<int> BetChanged;   // the change in chips (+10, -50 ...)
        public event Action DealClicked;
        public event Action HitClicked;
        public event Action StandClicked;
        public event Action DoubleClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_dealButton.GetComponentInChildren<TMP_Text>(), "bj.deal");
            LocalizedText.Bind(_hitButton.GetComponentInChildren<TMP_Text>(), "bj.hit");
            LocalizedText.Bind(_standButton.GetComponentInChildren<TMP_Text>(), "bj.stand");
            LocalizedText.Bind(_doubleButton.GetComponentInChildren<TMP_Text>(), "bj.double");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "bj.back");

            _minusBigButton.onClick.AddListener(() => ChangeBet(-BigStep));
            _minusSmallButton.onClick.AddListener(() => ChangeBet(-SmallStep));
            _plusSmallButton.onClick.AddListener(() => ChangeBet(SmallStep));
            _plusBigButton.onClick.AddListener(() => ChangeBet(BigStep));

            _dealButton.onClick.AddListener(() => { if (DealClicked != null) DealClicked(); });
            _hitButton.onClick.AddListener(() => { if (HitClicked != null) HitClicked(); });
            _standButton.onClick.AddListener(() => { if (StandClicked != null) StandClicked(); });
            _doubleButton.onClick.AddListener(() => { if (DoubleClicked != null) DoubleClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowNumbers;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowNumbers;
        }

        public void SetMode(BlackjackHudMode mode)
        {
            _betRow.SetActive(mode == BlackjackHudMode.Betting);
            _dealRow.SetActive(mode == BlackjackHudMode.Betting);
            _actionRow.SetActive(mode == BlackjackHudMode.Playing);
        }

        public void SetChips(int chips, int bet)
        {
            _chips = chips;
            _bet = bet;
            ShowNumbers();
        }

        public void SetDoubleEnabled(bool enabled)
        {
            _doubleButton.interactable = enabled;
        }

        private void ShowNumbers()
        {
            _chipsText.text = Texts.Get("bj.chips") + ": " + _chips;
            _betText.text = Texts.Get("bj.bet") + ": " + _bet;
        }

        private void ChangeBet(int delta)
        {
            if (BetChanged != null) BetChanged(delta);
        }
    }
}
