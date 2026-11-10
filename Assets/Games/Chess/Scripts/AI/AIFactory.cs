using System;

namespace MobileParadise.Games.Chess.AI
{
    /// <summary>
    /// The three difficulty levels in one place: tune them here.
    /// </summary>
    public static class AIFactory
    {
        public static IChessAI Create(Difficulty difficulty)
        {
            int seed = Environment.TickCount;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    return new SearchAI(1, false, 200, seed);
                case Difficulty.Medium:
                    return new SearchAI(2, true, 40, seed);
                default:
                    return new SearchAI(3, true, 0, seed);
            }
        }
    }
}
