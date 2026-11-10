using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Tetris.Logic
{
    /// <summary>
    /// Hands out pieces from a shuffled "bag" of all 7: every piece appears once before any repeats,
    /// so the player never waits long for a piece they need.
    /// </summary>
    public class PieceBag
    {
        private readonly Random _random;
        private readonly List<int> _remaining = new List<int>();

        public PieceBag(Random random)
        {
            _random = random;
        }

        /// <summary>
        /// A bag in the middle of being used (from a save).
        /// </summary>
        public PieceBag(Random random, int[] remaining)
        {
            _random = random;
            _remaining.AddRange(remaining);
        }

        public int Next()
        {
            if (_remaining.Count == 0) Refill();

            int type = _remaining[_remaining.Count - 1];
            _remaining.RemoveAt(_remaining.Count - 1);
            return type;
        }

        public int[] Remaining()
        {
            return _remaining.ToArray();
        }

        private void Refill()
        {
            for (int type = 0; type < TetrisShapes.PieceCount; type++)
            {
                _remaining.Add(type);
            }

            for (int i = _remaining.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                int temp = _remaining[i];
                _remaining[i] = _remaining[j];
                _remaining[j] = temp;
            }
        }
    }
}
