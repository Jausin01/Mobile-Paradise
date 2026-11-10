namespace MobileParadise.Games.Solitaire.Logic
{
    public enum PileKind
    {
        Stock = 0,
        Waste = 1,
        Foundation = 2,   // Index = suit
        Tableau = 3       // Index = column 0..6
    }

    /// <summary>
    /// Points to one pile of cards on the table.
    /// </summary>
    public struct PileRef
    {
        public readonly PileKind Kind;
        public readonly int Index;

        public PileRef(PileKind kind, int index)
        {
            Kind = kind;
            Index = index;
        }
    }
}
