using System.Collections.Generic;

namespace MobileParadise.Games.Blackjack.Logic
{
    /// <summary>
    /// The cards of one hand and their blackjack value.
    /// A card is a number 0..51 (suit * 13 + rank - 1); only the rank matters here.
    /// An ace counts 11 unless that would go over 21, then it counts 1.
    /// </summary>
    public class BlackjackHand
    {
        private readonly List<int> _cards = new List<int>();

        public int Count { get { return _cards.Count; } }
        public bool IsBust { get { return Value > 21; } }
        public bool IsBlackjack { get { return _cards.Count == 2 && Value == 21; } }

        public int Card(int index)
        {
            return _cards[index];
        }

        public void Add(int card)
        {
            _cards.Add(card);
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public int Value
        {
            get { return Total(_cards.Count); }
        }

        /// <summary>
        /// The value of only the first cards (the dealer's visible card while the other one is hidden).
        /// </summary>
        public int Total(int cardCount)
        {
            int total = 0;
            int aces = 0;

            for (int i = 0; i < cardCount; i++)
            {
                int rank = _cards[i] % 13 + 1;
                if (rank == 1)
                {
                    aces++;
                    total += 11;
                }
                else
                {
                    total += rank >= 10 ? 10 : rank;
                }
            }

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }
            return total;
        }
    }
}
