using System.Collections.Generic;

namespace MobileParadise.Games.Checkers.Logic
{
    /// <summary>
    /// The pieces on the 8x8 board and the Brazilian draughts rules for moving them. No Unity code here.
    /// Rules: men move forward and capture in all four directions; kings ("damas") slide any distance and
    /// capture from a distance; capturing is mandatory and the move that takes the most pieces must be played;
    /// a captured piece stays on the board until the move ends and cannot be jumped twice;
    /// a man that only passes the last row while capturing is not crowned.
    /// </summary>
    public class CheckersBoard
    {
        public const int Size = 8;
        public const int SquareCount = Size * Size;

        public const int Empty = 0;
        public const int WhiteMan = 1;
        public const int WhiteKing = 2;
        public const int BlackMan = 3;
        public const int BlackKing = 4;

        private static readonly int[] RowSteps = { -1, -1, 1, 1 };
        private static readonly int[] ColumnSteps = { -1, 1, -1, 1 };

        private readonly int[] _cells = new int[SquareCount];

        /// <summary>
        /// The starting position: 12 pieces each on the dark squares.
        /// </summary>
        public static CheckersBoard Start()
        {
            CheckersBoard board = new CheckersBoard();
            for (int row = 0; row < Size; row++)
            {
                for (int column = 0; column < Size; column++)
                {
                    if (!IsDarkSquare(row, column)) continue;

                    if (row < 3) board._cells[row * Size + column] = BlackMan;
                    else if (row > 4) board._cells[row * Size + column] = WhiteMan;
                }
            }
            return board;
        }

        /// <summary>
        /// A board from 64 numbers (a saved game). Check IsValid(cells) first.
        /// </summary>
        public static CheckersBoard FromCells(int[] cells)
        {
            CheckersBoard board = new CheckersBoard();
            System.Array.Copy(cells, board._cells, SquareCount);
            return board;
        }

        public static bool IsValid(int[] cells)
        {
            if (cells == null || cells.Length != SquareCount) return false;

            int white = 0;
            int black = 0;
            for (int i = 0; i < SquareCount; i++)
            {
                if (cells[i] < Empty || cells[i] > BlackKing) return false;
                if (cells[i] != Empty && !IsDarkSquare(i / Size, i % Size)) return false;
                if (IsWhite(cells[i])) white++;
                if (IsBlack(cells[i])) black++;
            }
            return white > 0 && black > 0;
        }

        public static bool IsDarkSquare(int row, int column)
        {
            return (row + column) % 2 == 1;
        }

        public static bool IsWhite(int piece)
        {
            return piece == WhiteMan || piece == WhiteKing;
        }

        public static bool IsBlack(int piece)
        {
            return piece == BlackMan || piece == BlackKing;
        }

        public static bool IsKing(int piece)
        {
            return piece == WhiteKing || piece == BlackKing;
        }

        public static bool BelongsTo(int piece, CheckersSide side)
        {
            return side == CheckersSide.White ? IsWhite(piece) : IsBlack(piece);
        }

        public static CheckersSide Opponent(CheckersSide side)
        {
            return side == CheckersSide.White ? CheckersSide.Black : CheckersSide.White;
        }

        public int Get(int square)
        {
            return _cells[square];
        }

        public int[] CopyCells()
        {
            return (int[])_cells.Clone();
        }

        public int Count(CheckersSide side)
        {
            int count = 0;
            for (int i = 0; i < SquareCount; i++)
            {
                if (BelongsTo(_cells[i], side)) count++;
            }
            return count;
        }

        // ---- Moves ------------------------------------------------------------

        /// <summary>
        /// All moves the side may play now: only captures (the longest ones) if any exist, otherwise the simple moves.
        /// </summary>
        public List<CheckersMove> LegalMoves(CheckersSide side)
        {
            List<CheckersMove> captures = new List<CheckersMove>();
            for (int square = 0; square < SquareCount; square++)
            {
                int piece = _cells[square];
                if (!BelongsTo(piece, side)) continue;

                List<int> path = new List<int>();
                path.Add(square);
                FindCaptures(square, piece, side, square, path, new List<int>(), captures);
            }

            if (captures.Count > 0) return KeepLongest(captures);

            List<CheckersMove> moves = new List<CheckersMove>();
            for (int square = 0; square < SquareCount; square++)
            {
                if (BelongsTo(_cells[square], side)) AddSimpleMoves(square, side, moves);
            }
            return moves;
        }

        /// <summary>
        /// The board after a move (this board is not changed).
        /// </summary>
        public CheckersBoard Apply(CheckersMove move)
        {
            CheckersBoard board = new CheckersBoard();
            System.Array.Copy(_cells, board._cells, SquareCount);

            int piece = board._cells[move.From];
            board._cells[move.From] = Empty;
            for (int i = 0; i < move.Captured.Length; i++)
            {
                board._cells[move.Captured[i]] = Empty;
            }

            if (move.Promotes) piece = IsWhite(piece) ? WhiteKing : BlackKing;
            board._cells[move.To] = piece;
            return board;
        }

        // ---- Helpers ----------------------------------------------------------

        private static List<CheckersMove> KeepLongest(List<CheckersMove> captures)
        {
            int longest = 0;
            for (int i = 0; i < captures.Count; i++)
            {
                if (captures[i].Captured.Length > longest) longest = captures[i].Captured.Length;
            }

            List<CheckersMove> result = new List<CheckersMove>();
            for (int i = 0; i < captures.Count; i++)
            {
                if (captures[i].Captured.Length == longest) result.Add(captures[i]);
            }
            return result;
        }

        // The square one step away, or -1 when it is off the board.
        private static int Step(int square, int direction, int steps)
        {
            int row = square / Size + RowSteps[direction] * steps;
            int column = square % Size + ColumnSteps[direction] * steps;
            if (row < 0 || row >= Size || column < 0 || column >= Size) return -1;
            return row * Size + column;
        }

        // The moving piece has left its square, so that square counts as empty while it is capturing.
        private bool IsFree(int square, int origin)
        {
            return _cells[square] == Empty || square == origin;
        }

        private void FindCaptures(int current, int piece, CheckersSide side, int origin, List<int> path, List<int> captured, List<CheckersMove> results)
        {
            bool jumped = false;
            CheckersSide enemy = Opponent(side);

            for (int direction = 0; direction < 4; direction++)
            {
                if (IsKing(piece))
                {
                    // Slide over free squares to the first piece; if it is an enemy, any free square behind it is a landing.
                    int target = Step(current, direction, 1);
                    int distance = 1;
                    while (target >= 0 && IsFree(target, origin))
                    {
                        distance++;
                        target = Step(current, direction, distance);
                    }

                    if (target < 0 || !BelongsTo(_cells[target], enemy) || captured.Contains(target)) continue;

                    int landing = Step(target, direction, 1);
                    int away = 1;
                    while (landing >= 0 && IsFree(landing, origin))
                    {
                        jumped = true;
                        Continue(landing, piece, side, origin, path, captured, target, results);
                        away++;
                        landing = Step(target, direction, away);
                    }
                }
                else
                {
                    int target = Step(current, direction, 1);
                    int landing = Step(current, direction, 2);
                    if (target < 0 || landing < 0) continue;
                    if (!BelongsTo(_cells[target], enemy) || captured.Contains(target) || !IsFree(landing, origin)) continue;

                    jumped = true;
                    Continue(landing, piece, side, origin, path, captured, target, results);
                }
            }

            if (!jumped && captured.Count > 0)
            {
                bool promotes = !IsKing(piece) && ReachesLastRow(current, side);
                results.Add(new CheckersMove(path.ToArray(), captured.ToArray(), promotes));
            }
        }

        private void Continue(int landing, int piece, CheckersSide side, int origin, List<int> path, List<int> captured, int taken, List<CheckersMove> results)
        {
            path.Add(landing);
            captured.Add(taken);
            FindCaptures(landing, piece, side, origin, path, captured, results);
            captured.RemoveAt(captured.Count - 1);
            path.RemoveAt(path.Count - 1);
        }

        private void AddSimpleMoves(int square, CheckersSide side, List<CheckersMove> moves)
        {
            int piece = _cells[square];
            int forward = side == CheckersSide.White ? -1 : 1;

            for (int direction = 0; direction < 4; direction++)
            {
                if (IsKing(piece))
                {
                    int target = Step(square, direction, 1);
                    int distance = 1;
                    while (target >= 0 && _cells[target] == Empty)
                    {
                        moves.Add(new CheckersMove(new int[] { square, target }, new int[0], false));
                        distance++;
                        target = Step(square, direction, distance);
                    }
                }
                else if (RowSteps[direction] == forward)
                {
                    int target = Step(square, direction, 1);
                    if (target >= 0 && _cells[target] == Empty)
                    {
                        moves.Add(new CheckersMove(new int[] { square, target }, new int[0], ReachesLastRow(target, side)));
                    }
                }
            }
        }

        private static bool ReachesLastRow(int square, CheckersSide side)
        {
            int row = square / Size;
            return side == CheckersSide.White ? row == 0 : row == Size - 1;
        }
    }
}
