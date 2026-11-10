namespace MobileParadise.Games.Solitaire.Logic
{
    /// <summary>
    /// A card is a number 0..51: suit * 13 + (rank - 1).
    /// Suits: 0 Clubs, 1 Diamonds, 2 Hearts, 3 Spades. Rank: 1 (Ace) .. 13 (King).
    /// </summary>
    public static class CardInfo
    {
        public const int CardCount = 52;
        public const int SuitCount = 4;
        public const int King = 13;

        public static int Suit(int card)
        {
            return card / 13;
        }

        public static int Rank(int card)
        {
            return card % 13 + 1;
        }

        public static bool IsRed(int card)
        {
            int suit = Suit(card);
            return suit == 1 || suit == 2;
        }
    }
}
