using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Domino.Logic
{
    /// <summary>
    /// The computer's choice of tile. It likes to get rid of heavy tiles and doubles first, and to leave
    /// an open end that matches many of its own remaining tiles. A little randomness keeps it from being predictable.
    /// </summary>
    public static class DominoAI
    {
        private const int PipWeight = 10;
        private const int DoubleBonus = 15;
        private const int MatchWeight = 4;
        private const int Jitter = 6;

        /// <summary>
        /// The best move for this side, or null when it has nothing to play.
        /// </summary>
        public static DominoMove Choose(DominoRound round, DominoSide side, Random random)
        {
            IList<int> hand = round.Hand(side);
            DominoMove best = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < hand.Count; i++)
            {
                int id = hand[i];
                for (int e = 0; e < 2; e++)
                {
                    DominoEnd end = (DominoEnd)e;
                    if (!round.CanPlace(id, end)) continue;

                    int score = Score(round, hand, id, end) + random.Next(Jitter);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = new DominoMove(id, end);
                    }
                }
            }
            return best;
        }

        private static int Score(DominoRound round, IList<int> hand, int id, DominoEnd end)
        {
            int open = end == DominoEnd.Left ? round.LeftValue : round.RightValue;
            int newEnd = DominoTiles.Low(id) == open ? DominoTiles.High(id) : DominoTiles.Low(id);

            int score = DominoTiles.Pips(id) * PipWeight;
            if (DominoTiles.IsDouble(id)) score += DoubleBonus;

            int matches = 0;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] == id) continue;
                if (DominoTiles.Low(hand[i]) == newEnd || DominoTiles.High(hand[i]) == newEnd) matches++;
            }
            return score + matches * MatchWeight;
        }
    }
}
