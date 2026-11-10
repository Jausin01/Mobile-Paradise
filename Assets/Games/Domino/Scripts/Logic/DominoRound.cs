using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Domino.Logic
{
    /// <summary>
    /// One round of dominoes between the player and the computer. No Unity code here.
    /// 7 tiles each, 14 in the stock. The player holding the highest double (or else the highest tile) puts it down first.
    /// Then the players take turns adding a tile that matches an end of the chain. With no playable tile you must
    /// draw from the stock until you have one; with the stock empty you pass.
    /// The round ends when a hand is empty (that player wins the other's pips) or when nobody can move
    /// (the lower total wins the difference).
    /// </summary>
    public class DominoRound
    {
        public const int HandSize = 7;

        private readonly List<PlacedTile> _chain = new List<PlacedTile>();
        private readonly List<int>[] _hands = { new List<int>(), new List<int>() };
        private readonly List<int> _stock = new List<int>();
        private DominoSide _turn;
        private int _passes;
        private DominoResult _result = DominoResult.Playing;
        private int _points;
        private bool _blocked;
        private int _lastPlacedId = -1;

        /// <summary>
        /// A fresh round: deals the tiles and puts the first one down.
        /// </summary>
        public DominoRound(Random random)
        {
            List<int> deck = new List<int>();
            for (int id = 0; id < DominoTiles.Count; id++)
            {
                deck.Add(id);
            }
            Shuffle(deck, random);

            for (int i = 0; i < HandSize; i++)
            {
                _hands[(int)DominoSide.Player].Add(deck[i]);
                _hands[(int)DominoSide.Computer].Add(deck[HandSize + i]);
            }
            for (int i = 2 * HandSize; i < deck.Count; i++)
            {
                _stock.Add(deck[i]);
            }

            int starterTile;
            DominoSide starter = ChooseStarter(out starterTile);
            _hands[(int)starter].Remove(starterTile);
            _chain.Add(new PlacedTile(starterTile, DominoTiles.Low(starterTile), DominoTiles.High(starterTile)));
            _lastPlacedId = starterTile;
            _turn = Other(starter);
        }

        /// <summary>
        /// A round in progress, restored from a save. The data must already have been validated.
        /// </summary>
        public DominoRound(IList<PlacedTile> chain, IList<int> player, IList<int> computer, IList<int> stock, DominoSide turn, int passes)
        {
            _chain.AddRange(chain);
            _hands[(int)DominoSide.Player].AddRange(player);
            _hands[(int)DominoSide.Computer].AddRange(computer);
            _stock.AddRange(stock);
            _turn = turn;
            _passes = passes;

            if (_hands[(int)DominoSide.Player].Count == 0) Finish(DominoSide.Player);
            else if (_hands[(int)DominoSide.Computer].Count == 0) Finish(DominoSide.Computer);
            else if (_passes >= 2) FinishBlocked();
        }

        public DominoSide Turn { get { return _turn; } }
        public DominoResult Result { get { return _result; } }
        public int Points { get { return _points; } }
        public bool Blocked { get { return _blocked; } }
        public int StockCount { get { return _stock.Count; } }
        public int Passes { get { return _passes; } }
        public int LastPlacedId { get { return _lastPlacedId; } }
        public bool IsOver { get { return _result != DominoResult.Playing; } }

        public IList<PlacedTile> Chain { get { return _chain.AsReadOnly(); } }
        public IList<int> Hand(DominoSide side) { return _hands[(int)side].AsReadOnly(); }
        public IList<int> Stock { get { return _stock.AsReadOnly(); } }
        public int HandCount(DominoSide side) { return _hands[(int)side].Count; }

        public int LeftValue { get { return _chain[0].Left; } }
        public int RightValue { get { return _chain[_chain.Count - 1].Right; } }

        public int Pips(DominoSide side)
        {
            int total = 0;
            List<int> hand = _hands[(int)side];
            for (int i = 0; i < hand.Count; i++)
            {
                total += DominoTiles.Pips(hand[i]);
            }
            return total;
        }

        // ---- Rules ------------------------------------------------------------

        public bool CanPlace(int id, DominoEnd end)
        {
            int open = end == DominoEnd.Left ? LeftValue : RightValue;
            return DominoTiles.Low(id) == open || DominoTiles.High(id) == open;
        }

        public bool CanPlay(int id)
        {
            return CanPlace(id, DominoEnd.Left) || CanPlace(id, DominoEnd.Right);
        }

        public bool HasPlayable(DominoSide side)
        {
            List<int> hand = _hands[(int)side];
            for (int i = 0; i < hand.Count; i++)
            {
                if (CanPlay(hand[i])) return true;
            }
            return false;
        }

        public bool CanDraw(DominoSide side)
        {
            return !IsOver && _turn == side && _stock.Count > 0 && !HasPlayable(side);
        }

        public bool MustPass(DominoSide side)
        {
            return !IsOver && _turn == side && _stock.Count == 0 && !HasPlayable(side);
        }

        // ---- Actions ----------------------------------------------------------

        /// <summary>
        /// Puts a tile from the side's hand on one end of the chain. False if it is not that side's turn or the tile does not fit.
        /// </summary>
        public bool Play(DominoSide side, int id, DominoEnd end)
        {
            if (IsOver || _turn != side) return false;
            if (!_hands[(int)side].Contains(id) || !CanPlace(id, end)) return false;

            int low = DominoTiles.Low(id);
            int high = DominoTiles.High(id);
            if (end == DominoEnd.Left)
            {
                int open = LeftValue;
                int other = low == open ? high : low;
                _chain.Insert(0, new PlacedTile(id, other, open));
            }
            else
            {
                int open = RightValue;
                int other = low == open ? high : low;
                _chain.Add(new PlacedTile(id, open, other));
            }

            _hands[(int)side].Remove(id);
            _lastPlacedId = id;
            _passes = 0;

            if (_hands[(int)side].Count == 0)
            {
                Finish(side);
                return true;
            }
            _turn = Other(side);
            return true;
        }

        /// <summary>
        /// Takes the next tile from the stock into the side's hand (only when it has nothing to play). Returns the tile, or -1.
        /// </summary>
        public int Draw(DominoSide side)
        {
            if (!CanDraw(side)) return -1;

            int id = _stock[_stock.Count - 1];
            _stock.RemoveAt(_stock.Count - 1);
            _hands[(int)side].Add(id);
            return id;
        }

        /// <summary>
        /// Passes the turn (only with nothing to play and an empty stock).
        /// </summary>
        public bool Pass(DominoSide side)
        {
            if (!MustPass(side)) return false;

            _passes++;
            if (_passes >= 2)
            {
                FinishBlocked();
                return true;
            }
            _turn = Other(side);
            return true;
        }

        public static DominoSide Other(DominoSide side)
        {
            return side == DominoSide.Player ? DominoSide.Computer : DominoSide.Player;
        }

        // ---- Helpers ----------------------------------------------------------

        private void Finish(DominoSide winner)
        {
            _result = winner == DominoSide.Player ? DominoResult.PlayerWins : DominoResult.ComputerWins;
            _points = Pips(Other(winner));
        }

        private void FinishBlocked()
        {
            _blocked = true;
            int player = Pips(DominoSide.Player);
            int computer = Pips(DominoSide.Computer);
            if (player < computer)
            {
                _result = DominoResult.PlayerWins;
                _points = computer - player;
            }
            else if (computer < player)
            {
                _result = DominoResult.ComputerWins;
                _points = player - computer;
            }
            else
            {
                _result = DominoResult.Tie;
                _points = 0;
            }
        }

        // The highest double starts; with no double at all, the tile with the most pips.
        private DominoSide ChooseStarter(out int tile)
        {
            for (int value = DominoTiles.MaxValue; value >= 0; value--)
            {
                int id = DominoTiles.DoubleId(value);
                for (int side = 0; side < 2; side++)
                {
                    if (_hands[side].Contains(id))
                    {
                        tile = id;
                        return (DominoSide)side;
                    }
                }
            }

            int best = -1;
            DominoSide bestSide = DominoSide.Player;
            for (int side = 0; side < 2; side++)
            {
                for (int i = 0; i < _hands[side].Count; i++)
                {
                    int id = _hands[side][i];
                    if (best < 0 || IsBetterStarter(id, best))
                    {
                        best = id;
                        bestSide = (DominoSide)side;
                    }
                }
            }
            tile = best;
            return bestSide;
        }

        private static bool IsBetterStarter(int id, int best)
        {
            int pips = DominoTiles.Pips(id);
            int bestPips = DominoTiles.Pips(best);
            if (pips != bestPips) return pips > bestPips;
            return DominoTiles.High(id) > DominoTiles.High(best);
        }

        private static void Shuffle(List<int> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
