using System;

namespace MobileParadise.Games.Memory.Logic
{
    public enum CardState
    {
        Hidden = 0,
        Revealed = 1,
        Matched = 2
    }

    public enum FlipResult
    {
        Ignored = 0,    // card already up, or a wrong pair still has to be hidden
        FirstCard = 1,  // first card of a try
        Match = 2,      // second card, same picture
        Mismatch = 3    // second card, different picture: call HideMismatch() after the player has seen it
    }

    /// <summary>
    /// The rules of Memory, with no Unity code. Each picture (face id 0..pairCount-1) appears on two cards.
    /// A "move" is one try: two cards turned over.
    /// </summary>
    public class MemoryBoard
    {
        private readonly int[] _faces;
        private readonly CardState[] _states;
        private int _first = -1;
        private int _second = -1;

        public int CardCount { get { return _faces.Length; } }
        public int PairCount { get { return _faces.Length / 2; } }
        public int Moves { get; private set; }
        public int MatchedPairs { get; private set; }
        public bool IsComplete { get { return MatchedPairs == PairCount; } }

        public MemoryBoard(int pairCount, Random random)
        {
            _faces = new int[pairCount * 2];
            _states = new CardState[pairCount * 2];

            for (int i = 0; i < pairCount; i++)
            {
                _faces[i * 2] = i;
                _faces[i * 2 + 1] = i;
            }

            // Fisher-Yates shuffle
            for (int i = _faces.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = _faces[i];
                _faces[i] = _faces[j];
                _faces[j] = temp;
            }
        }

        /// <summary>
        /// Rebuilds a saved game: which picture is on each card, which cards were already matched, and the moves.
        /// Cards that were only turned over (not matched) come back face down.
        /// </summary>
        public MemoryBoard(int[] faces, bool[] matched, int moves)
        {
            _faces = (int[])faces.Clone();
            _states = new CardState[faces.Length];
            Moves = moves;

            for (int i = 0; i < matched.Length; i++)
            {
                if (!matched[i]) continue;

                _states[i] = CardState.Matched;
                MatchedPairs++;
            }
            MatchedPairs /= 2;   // two cards per pair
        }

        public int[] CopyFaces()
        {
            return (int[])_faces.Clone();
        }

        public int FaceAt(int index)
        {
            return _faces[index];
        }

        public CardState StateAt(int index)
        {
            return _states[index];
        }

        /// <summary>
        /// Turns a card over. For Match and Mismatch, 'otherIndex' is the first card of the try (otherwise -1).
        /// </summary>
        public FlipResult Flip(int index, out int otherIndex)
        {
            otherIndex = -1;

            if (index < 0 || index >= _faces.Length) return FlipResult.Ignored;
            if (_states[index] != CardState.Hidden) return FlipResult.Ignored;
            if (_second >= 0) return FlipResult.Ignored;   // wrong pair still showing

            _states[index] = CardState.Revealed;

            if (_first < 0)
            {
                _first = index;
                return FlipResult.FirstCard;
            }

            otherIndex = _first;
            Moves++;

            if (_faces[_first] == _faces[index])
            {
                _states[_first] = CardState.Matched;
                _states[index] = CardState.Matched;
                MatchedPairs++;
                _first = -1;
                return FlipResult.Match;
            }

            _second = index;
            return FlipResult.Mismatch;
        }

        /// <summary>
        /// Turns the two cards of a wrong pair face down again so the next try can start.
        /// </summary>
        public void HideMismatch()
        {
            if (_second < 0) return;

            _states[_first] = CardState.Hidden;
            _states[_second] = CardState.Hidden;
            _first = -1;
            _second = -1;
        }
    }
}
