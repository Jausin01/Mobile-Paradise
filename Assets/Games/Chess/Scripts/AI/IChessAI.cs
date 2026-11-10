using MobileParadise.Games.Chess.Logic;

namespace MobileParadise.Games.Chess.AI
{
    public interface IChessAI
    {
        /// <summary>
        /// Picks a move for board.SideToMove. The board must have at least one legal move.
        /// It may be modified while searching, but is restored before returning,
        /// so pass a Clone() when calling from another thread.
        /// </summary>
        Move ChooseMove(Board board);
    }
}
