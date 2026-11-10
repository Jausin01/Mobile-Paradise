using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Minesweeper.Logic
{
    public enum MinesStatus
    {
        Playing = 0,
        Won = 1,
        Lost = 2
    }

    public enum RevealResult
    {
        Ignored = 0,   // flagged, already open, or the game is over
        Opened = 1,    // one or more safe cells opened
        Boom = 2,      // the cell was a mine
        Won = 3        // the last safe cell opened
    }

    /// <summary>
    /// The rules of Minesweeper, with no Unity code.
    /// The mines are placed on the first reveal, so the first tap is always safe
    /// (and, when there is room, opens an area).
    /// </summary>
    public class MinesBoard
    {
        private readonly int _columns;
        private readonly int _rows;
        private readonly Random _random;
        private readonly bool[] _mines;
        private readonly bool[] _revealed;
        private readonly bool[] _flagged;
        private readonly int[] _adjacent;
        private bool _started;
        private int _revealedCount;

        public int Columns { get { return _columns; } }
        public int Rows { get { return _rows; } }
        public int CellCount { get { return _columns * _rows; } }
        public int MineCount { get; private set; }
        public int FlagCount { get; private set; }
        public MinesStatus Status { get; private set; }
        public int ExplodedIndex { get; private set; }
        public bool IsStarted { get { return _started; } }

        /// <summary>Mines not flagged yet (shown to the player; can go below zero).</summary>
        public int MinesLeft
        {
            get { return Status == MinesStatus.Won ? 0 : MineCount - FlagCount; }
        }

        public MinesBoard(int columns, int rows, int mineCount, Random random)
        {
            _columns = columns;
            _rows = rows;
            _random = random;
            MineCount = mineCount;
            ExplodedIndex = -1;

            int cells = columns * rows;
            _mines = new bool[cells];
            _revealed = new bool[cells];
            _flagged = new bool[cells];
            _adjacent = new int[cells];
        }

        /// <summary>
        /// Rebuilds a saved game in progress.
        /// </summary>
        public MinesBoard(int columns, int rows, bool[] mines, bool[] revealed, bool[] flagged)
            : this(columns, rows, 0, null)
        {
            for (int i = 0; i < _mines.Length; i++)
            {
                _mines[i] = mines[i];
                _revealed[i] = revealed[i];
                _flagged[i] = flagged[i];

                if (_mines[i]) MineCount++;
                if (_revealed[i]) _revealedCount++;
                if (_flagged[i]) FlagCount++;
            }

            _started = MineCount > 0;
            CountAdjacent();
        }

        public bool IsRevealed(int index)
        {
            return _revealed[index];
        }

        public bool IsFlagged(int index)
        {
            return _flagged[index];
        }

        public bool IsMine(int index)
        {
            return _mines[index];
        }

        public int AdjacentMines(int index)
        {
            return _adjacent[index];
        }

        public bool[] CopyMines()
        {
            return (bool[])_mines.Clone();
        }

        public bool[] CopyRevealed()
        {
            return (bool[])_revealed.Clone();
        }

        public bool[] CopyFlagged()
        {
            return (bool[])_flagged.Clone();
        }

        /// <summary>
        /// Opens a cell. A zero cell opens its neighbours too (flood fill).
        /// </summary>
        public RevealResult Reveal(int index)
        {
            if (Status != MinesStatus.Playing) return RevealResult.Ignored;
            if (index < 0 || index >= _mines.Length) return RevealResult.Ignored;
            if (_revealed[index] || _flagged[index]) return RevealResult.Ignored;

            if (!_started)
            {
                PlaceMines(index);
                _started = true;
            }

            if (_mines[index])
            {
                _revealed[index] = true;
                ExplodedIndex = index;
                Status = MinesStatus.Lost;
                return RevealResult.Boom;
            }

            FloodOpen(index);

            if (_revealedCount == CellCount - MineCount)
            {
                Status = MinesStatus.Won;
                return RevealResult.Won;
            }
            return RevealResult.Opened;
        }

        /// <summary>
        /// Puts a flag on a closed cell, or takes it off. Returns false if nothing changed.
        /// </summary>
        public bool ToggleFlag(int index)
        {
            if (Status != MinesStatus.Playing) return false;
            if (index < 0 || index >= _mines.Length) return false;
            if (_revealed[index]) return false;

            _flagged[index] = !_flagged[index];
            FlagCount += _flagged[index] ? 1 : -1;
            return true;
        }

        // ---- Helpers -------------------------------------------------------------

        private void FloodOpen(int start)
        {
            Stack<int> pending = new Stack<int>();
            pending.Push(start);

            while (pending.Count > 0)
            {
                int index = pending.Pop();
                if (_revealed[index] || _flagged[index]) continue;

                _revealed[index] = true;
                _revealedCount++;
                if (_adjacent[index] != 0) continue;

                int column = index % _columns;
                int row = index / _columns;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int c = column + dx;
                        int r = row + dy;
                        if (c < 0 || c >= _columns || r < 0 || r >= _rows) continue;
                        pending.Push(r * _columns + c);
                    }
                }
            }
        }

        /// <summary>
        /// Random mines, none on the tapped cell or its neighbours (if the board is too crowded for that,
        /// only the tapped cell itself is kept free).
        /// </summary>
        private void PlaceMines(int safeIndex)
        {
            List<int> candidates = new List<int>();
            for (int i = 0; i < _mines.Length; i++)
            {
                if (!IsNear(i, safeIndex)) candidates.Add(i);
            }

            if (candidates.Count < MineCount)
            {
                candidates.Clear();
                for (int i = 0; i < _mines.Length; i++)
                {
                    if (i != safeIndex) candidates.Add(i);
                }
            }

            for (int placed = 0; placed < MineCount; placed++)
            {
                int pick = placed + _random.Next(candidates.Count - placed);
                int temp = candidates[placed];
                candidates[placed] = candidates[pick];
                candidates[pick] = temp;
                _mines[candidates[placed]] = true;
            }

            CountAdjacent();
        }

        private bool IsNear(int a, int b)
        {
            int dx = Math.Abs(a % _columns - b % _columns);
            int dy = Math.Abs(a / _columns - b / _columns);
            return dx <= 1 && dy <= 1;
        }

        private void CountAdjacent()
        {
            for (int index = 0; index < _mines.Length; index++)
            {
                int count = 0;
                int column = index % _columns;
                int row = index / _columns;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int c = column + dx;
                        int r = row + dy;
                        if (c < 0 || c >= _columns || r < 0 || r >= _rows) continue;
                        if (_mines[r * _columns + c]) count++;
                    }
                }
                _adjacent[index] = _mines[index] ? 0 : count;
            }
        }
    }
}
