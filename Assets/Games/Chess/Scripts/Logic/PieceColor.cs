namespace MobileParadise.Games.Chess.Logic
{
    public enum PieceColor
    {
        White = 0,
        Black = 1
    }

    public static class PieceColorExtensions
    {
        public static PieceColor Opposite(this PieceColor color)
        {
            return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }
    }
}
