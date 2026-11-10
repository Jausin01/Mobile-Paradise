using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Blackjack.Logic
{
    public enum BlackjackState
    {
        Betting,      // waiting for a bet (also after a round ended)
        PlayerTurn,
        DealerTurn,
        Finished
    }

    public enum BlackjackOutcome
    {
        None,
        Blackjack,    // player's first two cards make 21: pays 3 to 2
        Win,
        DealerBust,
        Push,
        Lose,
        Bust          // the player went over 21
    }

    /// <summary>
    /// The rules of Blackjack with play-money chips. No Unity code here.
    /// Dealer stands on every 17, a blackjack pays 3 to 2, the player can Hit, Stand or Double (no split).
    /// The chips only change when a round ends, so leaving in the middle of a hand costs nothing.
    /// </summary>
    public class BlackjackGame
    {
        public const int MinBet = 10;
        public const int BetStep = 10;
        public const int StartChips = 1000;

        private const int Decks = 4;
        private const int ReshuffleBelow = 40;   // cards left in the shoe

        private readonly Random _random;
        private readonly List<int> _shoe = new List<int>();
        private readonly BlackjackHand _player = new BlackjackHand();
        private readonly BlackjackHand _dealer = new BlackjackHand();

        private int _chips;
        private int _bet = MinBet;
        private int _baseBet = MinBet;   // the bet before a Double
        private int _lastChange;
        private BlackjackState _state = BlackjackState.Betting;
        private BlackjackOutcome _outcome = BlackjackOutcome.None;

        public BlackjackGame(int chips, Random random)
        {
            _chips = chips;
            _random = random;
            ClampBet();
        }

        public int Chips { get { return _chips; } }
        public int Bet { get { return _bet; } }
        public int LastChange { get { return _lastChange; } }
        public BlackjackState State { get { return _state; } }
        public BlackjackOutcome Outcome { get { return _outcome; } }
        public BlackjackHand Player { get { return _player; } }
        public BlackjackHand Dealer { get { return _dealer; } }

        /// <summary>The dealer's second card stays face down until the player is done.</summary>
        public bool HoleHidden { get { return _state == BlackjackState.PlayerTurn; } }

        public bool CanBet { get { return _state == BlackjackState.Betting || _state == BlackjackState.Finished; } }
        public bool IsBroke { get { return _chips < MinBet; } }

        public bool CanDouble
        {
            get { return _state == BlackjackState.PlayerTurn && _player.Count == 2 && _chips >= _bet * 2; }
        }

        /// <summary>
        /// Changes the bet (between rounds only), keeping it between the minimum and the chips owned.
        /// </summary>
        public bool ChangeBet(int delta)
        {
            if (!CanBet) return false;

            int before = _bet;
            _bet += delta;
            ClampBet();
            return _bet != before;
        }

        /// <summary>
        /// Starts a round: two cards each. A blackjack on either side ends the round at once.
        /// </summary>
        public bool Deal()
        {
            if (!CanBet || IsBroke) return false;

            ClampBet();
            if (_shoe.Count < ReshuffleBelow) BuildShoe();

            _baseBet = _bet;
            _player.Clear();
            _dealer.Clear();
            _outcome = BlackjackOutcome.None;
            _lastChange = 0;

            _player.Add(DrawCard());
            _dealer.Add(DrawCard());
            _player.Add(DrawCard());
            _dealer.Add(DrawCard());   // the hole card

            _state = BlackjackState.PlayerTurn;
            if (_player.IsBlackjack || _dealer.IsBlackjack) Settle();
            return true;
        }

        public bool Hit()
        {
            if (_state != BlackjackState.PlayerTurn) return false;

            _player.Add(DrawCard());
            if (_player.IsBust) Settle();
            else if (_player.Value == 21) _state = BlackjackState.DealerTurn;   // nothing better to do
            return true;
        }

        public bool Stand()
        {
            if (_state != BlackjackState.PlayerTurn) return false;

            _state = BlackjackState.DealerTurn;
            return true;
        }

        /// <summary>
        /// Doubles the bet, takes exactly one more card and then stands.
        /// </summary>
        public bool DoubleDown()
        {
            if (!CanDouble) return false;

            _bet *= 2;
            _player.Add(DrawCard());
            if (_player.IsBust) Settle();
            else _state = BlackjackState.DealerTurn;
            return true;
        }

        /// <summary>
        /// One step of the dealer's turn: draws a card and returns true while the dealer still had to draw.
        /// When it returns false the round has been settled.
        /// </summary>
        public bool DealerPlay()
        {
            if (_state != BlackjackState.DealerTurn) return false;

            if (_dealer.Value < 17)
            {
                _dealer.Add(DrawCard());
                return true;
            }

            Settle();
            return false;
        }

        // ---- Helpers ----------------------------------------------------------

        private void Settle()
        {
            if (_player.IsBust)
            {
                _outcome = BlackjackOutcome.Bust;
            }
            else if (_player.IsBlackjack || _dealer.IsBlackjack)
            {
                if (_player.IsBlackjack && _dealer.IsBlackjack) _outcome = BlackjackOutcome.Push;
                else if (_player.IsBlackjack) _outcome = BlackjackOutcome.Blackjack;
                else _outcome = BlackjackOutcome.Lose;
            }
            else if (_dealer.IsBust)
            {
                _outcome = BlackjackOutcome.DealerBust;
            }
            else if (_player.Value > _dealer.Value)
            {
                _outcome = BlackjackOutcome.Win;
            }
            else if (_player.Value < _dealer.Value)
            {
                _outcome = BlackjackOutcome.Lose;
            }
            else
            {
                _outcome = BlackjackOutcome.Push;
            }

            switch (_outcome)
            {
                case BlackjackOutcome.Blackjack: _lastChange = _bet * 3 / 2; break;
                case BlackjackOutcome.Win:
                case BlackjackOutcome.DealerBust: _lastChange = _bet; break;
                case BlackjackOutcome.Lose:
                case BlackjackOutcome.Bust: _lastChange = -_bet; break;
                default: _lastChange = 0; break;
            }

            _chips += _lastChange;
            _state = BlackjackState.Finished;

            // A doubled bet goes back to the normal size for the next round.
            _bet = _baseBet;
            ClampBet();
        }

        private int DrawCard()
        {
            int card = _shoe[_shoe.Count - 1];
            _shoe.RemoveAt(_shoe.Count - 1);
            return card;
        }

        private void BuildShoe()
        {
            _shoe.Clear();
            for (int deck = 0; deck < Decks; deck++)
            {
                for (int card = 0; card < 52; card++)
                {
                    _shoe.Add(card);
                }
            }

            for (int i = _shoe.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                int temp = _shoe[i];
                _shoe[i] = _shoe[j];
                _shoe[j] = temp;
            }
        }

        // The largest bet: all the chips, in whole steps of BetStep.
        private int MaxBet()
        {
            return Math.Max(MinBet, _chips / BetStep * BetStep);
        }

        private void ClampBet()
        {
            if (_bet > MaxBet()) _bet = MaxBet();
            if (_bet < MinBet) _bet = MinBet;
            _bet = _bet / BetStep * BetStep;
            if (_bet < MinBet) _bet = MinBet;
        }
    }
}
