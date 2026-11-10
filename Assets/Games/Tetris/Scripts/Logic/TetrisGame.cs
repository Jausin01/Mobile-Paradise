using System;

namespace MobileParadise.Games.Tetris.Logic
{
    public enum TetrisStep
    {
        Moved,    // the piece went down one row
        Locked,   // the piece landed (lines may have been cleared) and a new one appeared
        Over      // the piece landed and the new one has no room: game over
    }

    /// <summary>
    /// The Tetris rules: a 10 x 20 board, the falling piece, the next piece, score and lines. No Unity code here.
    /// Cells are numbered row * Width + column, row 0 at the top. A board cell holds 0 (empty) or piece type + 1.
    /// </summary>
    public class TetrisGame
    {
        public const int Width = 10;
        public const int Height = 20;

        private static readonly int[] LineScores = { 0, 100, 300, 500, 800 };
        private static readonly int[] KickOffsets = { 0, -1, 1, -2, 2 };

        private readonly int[] _cells = new int[Width * Height];
        private readonly PieceBag _bag;
        private int _type;
        private int _rotation;
        private int _x;
        private int _y;
        private int _next;
        private int _score;
        private int _lines;
        private int _lastCleared;
        private bool _over;

        public TetrisGame(Random random)
        {
            _bag = new PieceBag(random);
            _next = _bag.Next();
            Spawn();
        }

        /// <summary>
        /// A game in progress, restored from a save (already checked with IsValid).
        /// </summary>
        public TetrisGame(Random random, int[] cells, int type, int rotation, int x, int y, int next, int[] bag, int score, int lines)
        {
            Array.Copy(cells, _cells, _cells.Length);
            _bag = new PieceBag(random, bag);
            _type = type;
            _rotation = rotation;
            _x = x;
            _y = y;
            _next = next;
            _score = score;
            _lines = lines;
        }

        public int Score { get { return _score; } }
        public int Lines { get { return _lines; } }
        public int Level { get { return _lines / TetrisLevels.LinesPerLevel + 1; } }
        public bool IsOver { get { return _over; } }
        public int CurrentType { get { return _type; } }
        public int NextType { get { return _next; } }
        public int LastCleared { get { return _lastCleared; } }

        // ---- What the screen shows --------------------------------------------

        /// <summary>
        /// 0 for an empty board cell, otherwise piece type + 1.
        /// </summary>
        public int Cell(int index)
        {
            return _cells[index];
        }

        /// <summary>
        /// The 4 board cells of the falling piece.
        /// </summary>
        public int[] PieceCells()
        {
            return CellsAt(_y);
        }

        /// <summary>
        /// The 4 board cells where the falling piece would land if it dropped straight down.
        /// </summary>
        public int[] GhostCells()
        {
            return CellsAt(LandingY());
        }

        // ---- Moves ------------------------------------------------------------

        public bool MoveHorizontal(int direction)
        {
            if (_over || !Fits(_type, _rotation, _x + direction, _y)) return false;

            _x += direction;
            return true;
        }

        /// <summary>
        /// Turns the piece clockwise. If it does not fit, it is nudged one or two columns sideways.
        /// </summary>
        public bool Rotate()
        {
            if (_over || _type == 1) return false;   // the O piece looks the same turned

            int rotation = (_rotation + 1) % 4;
            for (int i = 0; i < KickOffsets.Length; i++)
            {
                int x = _x + KickOffsets[i];
                if (Fits(_type, rotation, x, _y))
                {
                    _rotation = rotation;
                    _x = x;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// One step down. 'byPlayer' gives a point for the row (pressing Down). When the piece cannot go down it lands.
        /// </summary>
        public TetrisStep Drop(bool byPlayer)
        {
            if (_over) return TetrisStep.Over;

            if (Fits(_type, _rotation, _x, _y + 1))
            {
                _y++;
                if (byPlayer) _score++;
                return TetrisStep.Moved;
            }
            return Land();
        }

        // ---- Saving -----------------------------------------------------------

        public int[] CopyCells()
        {
            return (int[])_cells.Clone();
        }

        public int Rotation { get { return _rotation; } }
        public int PieceX { get { return _x; } }
        public int PieceY { get { return _y; } }
        public int[] BagRemaining() { return _bag.Remaining(); }

        /// <summary>
        /// True if the saved numbers make a possible game.
        /// </summary>
        public static bool IsValid(int[] cells, int type, int rotation, int x, int y, int next, int[] bag, int score, int lines)
        {
            if (cells == null || cells.Length != Width * Height || bag == null || bag.Length > TetrisShapes.PieceCount) return false;
            if (score < 0 || lines < 0) return false;
            if (!IsType(type) || !IsType(next) || rotation < 0 || rotation > 3) return false;

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] < 0 || cells[i] > TetrisShapes.PieceCount) return false;
            }
            for (int i = 0; i < bag.Length; i++)
            {
                if (!IsType(bag[i])) return false;
            }

            // The falling piece must be on free cells.
            int[] shape = TetrisShapes.Cells(type, rotation);
            for (int i = 0; i < shape.Length; i += 2)
            {
                int column = x + shape[i];
                int row = y + shape[i + 1];
                if (column < 0 || column >= Width || row < 0 || row >= Height) return false;
                if (cells[row * Width + column] != 0) return false;
            }
            return true;
        }

        // ---- Helpers ----------------------------------------------------------

        private static bool IsType(int type)
        {
            return type >= 0 && type < TetrisShapes.PieceCount;
        }

        private bool Fits(int type, int rotation, int x, int y)
        {
            int[] shape = TetrisShapes.Cells(type, rotation);
            for (int i = 0; i < shape.Length; i += 2)
            {
                int column = x + shape[i];
                int row = y + shape[i + 1];
                if (column < 0 || column >= Width || row < 0 || row >= Height) return false;
                if (_cells[row * Width + column] != 0) return false;
            }
            return true;
        }

        private int LandingY()
        {
            int y = _y;
            while (Fits(_type, _rotation, _x, y + 1))
            {
                y++;
            }
            return y;
        }

        private int[] CellsAt(int y)
        {
            int[] shape = TetrisShapes.Cells(_type, _rotation);
            int[] result = new int[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = (y + shape[i * 2 + 1]) * Width + (_x + shape[i * 2]);
            }
            return result;
        }

        // Writes the piece into the board, clears full rows, scores, and brings the next piece.
        private TetrisStep Land()
        {
            int[] cells = PieceCells();
            for (int i = 0; i < cells.Length; i++)
            {
                _cells[cells[i]] = _type + 1;
            }

            _lastCleared = ClearFullRows();
            _score += LineScores[_lastCleared] * Level;
            _lines += _lastCleared;

            Spawn();
            return _over ? TetrisStep.Over : TetrisStep.Locked;
        }

        private int ClearFullRows()
        {
            int cleared = 0;
            int writeRow = Height - 1;
            for (int row = Height - 1; row >= 0; row--)
            {
                if (IsFull(row))
                {
                    cleared++;
                    continue;
                }

                if (writeRow != row) Array.Copy(_cells, row * Width, _cells, writeRow * Width, Width);
                writeRow--;
            }

            for (int row = writeRow; row >= 0; row--)
            {
                Array.Clear(_cells, row * Width, Width);
            }
            return cleared;
        }

        private bool IsFull(int row)
        {
            for (int column = 0; column < Width; column++)
            {
                if (_cells[row * Width + column] == 0) return false;
            }
            return true;
        }

        private void Spawn()
        {
            _type = _next;
            _next = _bag.Next();
            _rotation = 0;
            _x = (Width - TetrisShapes.Size(_type)) / 2;
            _y = 0;
            if (!Fits(_type, _rotation, _x, _y)) _over = true;
        }
    }
}
