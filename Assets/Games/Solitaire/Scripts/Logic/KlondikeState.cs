using System;

namespace MobileParadise.Games.Solitaire.Logic
{
    [Serializable]
    public class CardPile
    {
        public int[] Cards = new int[0];
    }

    /// <summary>
    /// A full copy of a game: used for undo and for the save file.
    /// </summary>
    [Serializable]
    public class KlondikeState
    {
        public int DrawCount;
        public int Moves;
        public CardPile[] Tableau;     // 7 columns, first card = deepest
        public int[] Hidden;           // per column: how many cards (from the deepest) are face down
        public CardPile Stock;         // last card = the one that is turned over next
        public CardPile Waste;         // last card = top
        public CardPile[] Foundation;  // 4 piles by suit, Ace first

        /// <summary>
        /// True when the numbers can safely be loaded: right shape, and the 52 cards each appear exactly once.
        /// </summary>
        public bool IsValid()
        {
            if (DrawCount != 1 && DrawCount != 3) return false;
            if (Tableau == null || Tableau.Length != 7 || Hidden == null || Hidden.Length != 7) return false;
            if (Stock == null || Stock.Cards == null || Waste == null || Waste.Cards == null) return false;
            if (Foundation == null || Foundation.Length != CardInfo.SuitCount) return false;

            bool[] seen = new bool[CardInfo.CardCount];
            int total = 0;

            for (int i = 0; i < 7; i++)
            {
                if (Tableau[i] == null || Tableau[i].Cards == null) return false;
                if (Hidden[i] < 0 || Hidden[i] > Tableau[i].Cards.Length) return false;
                if (Hidden[i] == Tableau[i].Cards.Length && Hidden[i] > 0) return false;   // a column's top card is always face up
                if (!Mark(Tableau[i].Cards, seen, ref total)) return false;
            }
            for (int i = 0; i < CardInfo.SuitCount; i++)
            {
                if (Foundation[i] == null || Foundation[i].Cards == null) return false;
                if (!Mark(Foundation[i].Cards, seen, ref total)) return false;
            }
            return Mark(Stock.Cards, seen, ref total) && Mark(Waste.Cards, seen, ref total) && total == CardInfo.CardCount;
        }

        private static bool Mark(int[] cards, bool[] seen, ref int total)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                int card = cards[i];
                if (card < 0 || card >= CardInfo.CardCount || seen[card]) return false;
                seen[card] = true;
                total++;
            }
            return true;
        }
    }
}
