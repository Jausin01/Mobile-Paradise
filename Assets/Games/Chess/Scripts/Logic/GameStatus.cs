namespace MobileParadise.Games.Chess.Logic
{
    public enum GameStatus
    {
        Ongoing = 0,
        Check = 1,
        Checkmate = 2,
        Stalemate = 3,
        DrawFiftyMoves = 4,
        DrawInsufficientMaterial = 5,
        DrawRepetition = 6
    }
}
