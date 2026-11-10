using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Snake.Logic
{
    public enum SnakeDirection
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public enum SnakeStatus
    {
        Playing,
        Dead,
        Won
    }

    public enum StepResult
    {
        Moved,
        Ate,
        Died,
        Won
    }

    /// <summary>
    /// The rules of Snake. No Unity code here.
    /// A cell is a number: row * columns + column. The body list starts at the head.
    /// Turns are queued (at most two) so a quick double swipe is not lost.
    /// </summary>
    public class SnakeGame
    {
        private const int StartLength = 3;
        private const int MaxQueuedTurns = 2;

        private readonly int _columns;
        private readonly int _rows;
        private readonly bool _wrap;
        private readonly Random _random;
        private readonly List<int> _body = new List<int>();
        private readonly Queue<SnakeDirection> _turns = new Queue<SnakeDirection>();

        private SnakeDirection _direction;
        private int _food;
        private int _score;
        private SnakeStatus _status = SnakeStatus.Playing;

        /// <summary>
        /// A new game: a snake of 3 cells in the middle, heading right, and the first food.
        /// </summary>
        public SnakeGame(int columns, int rows, bool wrap, Random random)
        {
            _columns = columns;
            _rows = rows;
            _wrap = wrap;
            _random = random;
            _direction = SnakeDirection.Right;

            int middle = (rows / 2) * columns + columns / 2;
            for (int i = 0; i < StartLength; i++)
            {
                _body.Add(middle - i);
            }
            PlaceFood();
        }

        /// <summary>
        /// A saved game. Check IsValid(...) first.
        /// </summary>
        public SnakeGame(int columns, int rows, bool wrap, int[] body, SnakeDirection direction, int food, int score, Random random)
        {
            _columns = columns;
            _rows = rows;
            _wrap = wrap;
            _random = random;
            _direction = direction;
            _food = food;
            _score = score;
            _body.AddRange(body);
        }

        public int Columns { get { return _columns; } }
        public int Rows { get { return _rows; } }
        public int CellCount { get { return _columns * _rows; } }
        public int Score { get { return _score; } }
        public int Food { get { return _food; } }
        public int Head { get { return _body[0]; } }
        public int Length { get { return _body.Count; } }
        public SnakeDirection Direction { get { return _direction; } }
        public SnakeStatus Status { get { return _status; } }

        public static bool IsValid(int columns, int rows, int[] body, int direction, int food, int score)
        {
            int cells = columns * rows;
            if (body == null || body.Length < 1 || body.Length >= cells) return false;
            if (direction < 0 || direction > 3 || score < 0) return false;
            if (food < 0 || food >= cells) return false;

            bool[] used = new bool[cells];
            for (int i = 0; i < body.Length; i++)
            {
                if (body[i] < 0 || body[i] >= cells || used[body[i]]) return false;
                used[body[i]] = true;
            }
            return !used[food];
        }

        public static SnakeDirection Opposite(SnakeDirection direction)
        {
            return (SnakeDirection)(((int)direction + 2) % 4);
        }

        public bool IsBody(int cell)
        {
            return _body.Contains(cell);
        }

        public int[] CopyBody()
        {
            return _body.ToArray();
        }

        /// <summary>
        /// Queues a turn. A turn to the direction it already goes, or straight back, is ignored.
        /// </summary>
        public bool Turn(SnakeDirection direction)
        {
            if (_turns.Count >= MaxQueuedTurns) return false;

            SnakeDirection reference = _direction;
            if (_turns.Count > 0)
            {
                SnakeDirection[] queued = _turns.ToArray();
                reference = queued[queued.Length - 1];
            }

            if (direction == reference || direction == Opposite(reference)) return false;

            _turns.Enqueue(direction);
            return true;
        }

        /// <summary>
        /// Moves the snake one cell.
        /// </summary>
        public StepResult Step()
        {
            if (_status != SnakeStatus.Playing) return StepResult.Died;

            if (_turns.Count > 0) _direction = _turns.Dequeue();

            int next = NextCell(_body[0], _direction);
            if (next < 0)
            {
                _status = SnakeStatus.Dead;
                return StepResult.Died;
            }

            bool eating = next == _food;

            // The tail leaves its cell in the same step, so only the other cells block the way.
            int blocking = eating ? _body.Count : _body.Count - 1;
            for (int i = 0; i < blocking; i++)
            {
                if (_body[i] == next)
                {
                    _status = SnakeStatus.Dead;
                    return StepResult.Died;
                }
            }

            _body.Insert(0, next);
            if (!eating)
            {
                _body.RemoveAt(_body.Count - 1);
                return StepResult.Moved;
            }

            _score++;
            if (_body.Count >= CellCount)
            {
                _status = SnakeStatus.Won;
                return StepResult.Won;
            }

            PlaceFood();
            return StepResult.Ate;
        }

        // ---- Helpers ----------------------------------------------------------

        // The cell after 'from' in a direction; -1 if it is a wall.
        private int NextCell(int from, SnakeDirection direction)
        {
            int column = from % _columns;
            int row = from / _columns;

            switch (direction)
            {
                case SnakeDirection.Up: row--; break;
                case SnakeDirection.Down: row++; break;
                case SnakeDirection.Left: column--; break;
                default: column++; break;
            }

            if (_wrap)
            {
                column = (column + _columns) % _columns;
                row = (row + _rows) % _rows;
            }
            else if (column < 0 || column >= _columns || row < 0 || row >= _rows)
            {
                return -1;
            }

            return row * _columns + column;
        }

        private void PlaceFood()
        {
            List<int> empty = new List<int>();
            for (int cell = 0; cell < CellCount; cell++)
            {
                if (!_body.Contains(cell)) empty.Add(cell);
            }
            _food = empty[_random.Next(empty.Count)];
        }
    }
}
