namespace MobileParadise.Games.Solitaire.Logic
{
    public enum SolitaireLevel
    {
        Easy = 0,   // turn over one card from the stock at a time
        Hard = 1    // turn over three
    }

    public static class SolitaireLevels
    {
        public static int DrawCount(SolitaireLevel level)
        {
            return level == SolitaireLevel.Easy ? 1 : 3;
        }
    }
}
