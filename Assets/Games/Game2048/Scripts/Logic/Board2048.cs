using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Game2048.Logic
{
    public enum SlideDirection
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3
    }

    /// <summary>
    /// The rules of 2048 on a 4x4 board. No Unity code here.
    /// A tile is stored as an exponent: 0 empty, 1 = "2", 2 = "4", 3 = "8" ... 11 = "2048".
    /// Move() slides everything, merges equal neighbours once per move and adds a new tile.
    /// </summary>
    public class Board2048
    {
        public const int Size = 4;
        public const int CellCount = Size * Size;
        public const int WinExponent = 11;       // the 2048 tile
        public const int MaxExponent = 17;       // the largest tile that can exist on 16 cells

        private const int MaxUndo = 30;

        private readonly int[] _cells = new int[CellCount];
        private readonly bool[] _merged = new bool[CellCount];
        private readonly List<Snapshot> _undo = new List<Snapshot>();
        private readonly Random _random;

        private int _score;
        private int _lastSpawn = -1;

        private class Snapshot
        {
            public int[] Cells;
            public int Score;
        }

        /// <summary>
        /// A new game: an empty board with two starting tiles.
        /// </summary>
        public Board2048(Random random)
        {
            _random = random;
            Spawn();
            Spawn();
            _lastSpawn = -1;
        }

        /// <summary>
        /// A saved game. Check IsValid(cells) first.
        /// </summary>
        public Board2048(int[] cells, int score, Random random)
        {
            _random = random;
            Array.Copy(cells, _cells, CellCount);
            _score = score;
        }

        public int Score { get { return _score; } }
        public bool CanUndo { get { return _undo.Count > 0; } }

        /// <summary>Cell that got the new tile in the last move, -1 if none.</summary>
        public int LastSpawn { get { return _lastSpawn; } }

        public static bool IsValid(int[] cells)
        {
            if (cells == null || cells.Length != CellCount) return false;

            for (int i = 0; i < CellCount; i++)
            {
                if (cells[i] < 0 || cells[i] > MaxExponent) return false;
            }
            return true;
        }

        public int Exponent(int index)
        {
            return _cells[index];
        }

        /// <summary>True if this cell was made by a merge in the last move.</summary>
        public bool WasMerged(int index)
        {
            return _merged[index];
        }

        public bool AnyMerged()
        {
            for (int i = 0; i < CellCount; i++)
            {
                if (_merged[i]) return true;
            }
            return false;
        }

        public int[] CopyCells()
        {
            return (int[])_cells.Clone();
        }

        public int HighestExponent()
        {
            int best = 0;
            for (int i = 0; i < CellCount; i++)
            {
                if (_cells[i] > best) best = _cells[i];
            }
            return best;
        }

        /// <summary>
        /// Moves the board. Returns false (and changes nothing) if no tile can move that way.
        /// </summary>
        public bool Move(SlideDirection direction)
        {
            int[] before = (int[])_cells.Clone();
            int scoreBefore = _score;

            Array.Clear(_merged, 0, CellCount);
            _lastSpawn = -1;

            bool changed = false;
            for (int line = 0; line < Size; line++)
            {
                if (SlideLine(direction, line)) changed = true;
            }

            if (!changed)
            {
                _score = scoreBefore;
                return false;
            }

            Snapshot snapshot = new Snapshot();
            snapshot.Cells = before;
            snapshot.Score = scoreBefore;
            _undo.Add(snapshot);
            if (_undo.Count > MaxUndo) _undo.RemoveAt(0);

            Spawn();
            return true;
        }

        /// <summary>
        /// Takes back the last move (tiles and score).
        /// </summary>
        public bool Undo()
        {
            if (_undo.Count == 0) return false;

            Snapshot snapshot = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            Array.Copy(snapshot.Cells, _cells, CellCount);
            _score = snapshot.Score;
            Array.Clear(_merged, 0, CellCount);
            _lastSpawn = -1;
            return true;
        }

        /// <summary>
        /// True while any move is possible: an empty cell or two equal neighbours.
        /// </summary>
        public bool CanMove()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int column = 0; column < Size; column++)
                {
                    int value = _cells[row * Size + column];
                    if (value == 0) return true;
                    if (column + 1 < Size && _cells[row * Size + column + 1] == value) return true;
                    if (row + 1 < Size && _cells[(row + 1) * Size + column] == value) return true;
                }
            }
            return false;
        }

        // ---- Helpers ----------------------------------------------------------

        // Position p of a line, counted from the side the tiles slide towards (p = 0 is the destination edge).
        private static int Index(SlideDirection direction, int line, int p)
        {
            switch (direction)
            {
                case SlideDirection.Left: return line * Size + p;
                case SlideDirection.Right: return line * Size + (Size - 1 - p);
                case SlideDirection.Up: return p * Size + line;
                default: return (Size - 1 - p) * Size + line;
            }
        }

        private bool SlideLine(SlideDirection direction, int line)
        {
            int[] result = new int[Size];
            bool[] mergedHere = new bool[Size];
            int write = 0;

            for (int p = 0; p < Size; p++)
            {
                int value = _cells[Index(direction, line, p)];
                if (value == 0) continue;

                if (write > 0 && result[write - 1] == value && !mergedHere[write - 1])
                {
                    result[write - 1] = value + 1;
                    mergedHere[write - 1] = true;
                    _score += 1 << (value + 1);
                }
                else
                {
                    result[write] = value;
                    write++;
                }
            }

            bool changed = false;
            for (int p = 0; p < Size; p++)
            {
                int index = Index(direction, line, p);
                if (_cells[index] != result[p]) changed = true;
                _cells[index] = result[p];
                _merged[index] = mergedHere[p];
            }
            return changed;
        }

        private void Spawn()
        {
            List<int> empty = new List<int>();
            for (int i = 0; i < CellCount; i++)
            {
                if (_cells[i] == 0) empty.Add(i);
            }
            if (empty.Count == 0) return;

            int index = empty[_random.Next(empty.Count)];
            _cells[index] = (_random.Next(10) == 0) ? 2 : 1;   // 10% a "4", otherwise a "2"
            _lastSpawn = index;
        }
    }
}
