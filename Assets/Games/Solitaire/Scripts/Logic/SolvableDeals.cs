using System;

namespace MobileParadise.Games.Solitaire.Logic
{
    // Picks a deal that is known to be winnable. The deals come from SolvableDealsData
    // (found offline by a solver and checked by replaying the win on the real rules).
    public static class SolvableDeals
    {
        // Returns a deck order (52 card ids) for a winnable deal, or a plain shuffle if no data exists.
        public static int[] Pick(int drawCount, Random random)
        {
            string[] list = (drawCount == 1) ? SolvableDealsData.Draw1 : SolvableDealsData.Draw3;
            if (list == null || list.Length == 0)
            {
                return KlondikeGame.ShuffledDeck(random);
            }

            string text = list[random.Next(list.Length)];
            int[] deck = Decode(text);
            if (deck == null)
            {
                return KlondikeGame.ShuffledDeck(random);
            }

            // Relabeling suits (swap the two blacks, the two reds, or flip every color) keeps the deal winnable,
            // because the rules only care about alternating colors and one foundation per suit.
            bool swapBlack = random.Next(2) == 1;
            bool swapRed = random.Next(2) == 1;
            bool swapColors = random.Next(2) == 1;
            for (int i = 0; i < deck.Length; i++)
            {
                deck[i] = MapCard(deck[i], swapBlack, swapRed, swapColors);
            }
            return deck;
        }

        // Card id = suit * 13 + (rank - 1). Suits: 0 clubs, 1 diamonds, 2 hearts, 3 spades.
        private static int MapCard(int card, bool swapBlack, bool swapRed, bool swapColors)
        {
            int suit = card / 13;
            int rank = card % 13;
            if (suit == 0 || suit == 3)
            {
                if (swapBlack) suit = 3 - suit;
            }
            else
            {
                if (swapRed) suit = 3 - suit;
            }
            if (swapColors)
            {
                // clubs <-> diamonds, spades <-> hearts
                if (suit == 0) suit = 1;
                else if (suit == 1) suit = 0;
                else if (suit == 2) suit = 3;
                else suit = 2;
            }
            return suit * 13 + rank;
        }

        private static int[] Decode(string text)
        {
            if (text == null || text.Length != 52) return null;
            int[] deck = new int[52];
            bool[] used = new bool[52];
            for (int i = 0; i < 52; i++)
            {
                char c = text[i];
                int id = (c >= 'A' && c <= 'Z') ? (c - 'A') : (26 + (c - 'a'));
                if (id < 0 || id > 51 || used[id]) return null;
                used[id] = true;
                deck[i] = id;
            }
            return deck;
        }
    }
}
