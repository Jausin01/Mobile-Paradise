namespace MobileParadise.Games.Domino.Logic
{
    public enum DominoSide
    {
        Player = 0,
        Computer = 1
    }

    /// <summary>
    /// Which end of the chain a tile is put on.
    /// </summary>
    public enum DominoEnd
    {
        Left = 0,
        Right = 1
    }

    public enum DominoResult
    {
        Playing,
        PlayerWins,
        ComputerWins,
        Tie
    }

    /// <summary>
    /// A tile in the chain. 'Left' is the value that faces the left end of the chain and 'Right' the one facing right,
    /// so for neighbors, the Right of one tile equals the Left of the next.
    /// </summary>
    public class PlacedTile
    {
        public readonly int Id;
        public readonly int Left;
        public readonly int Right;

        public PlacedTile(int id, int left, int right)
        {
            Id = id;
            Left = left;
            Right = right;
        }
    }

    public class DominoMove
    {
        public readonly int Id;
        public readonly DominoEnd End;

        public DominoMove(int id, DominoEnd end)
        {
            Id = id;
            End = end;
        }
    }
}
