namespace MobileParadise.Games.Tetris.Logic
{
    /// <summary>
    /// The 7 pieces (0 I, 1 O, 2 T, 3 S, 4 Z, 5 J, 6 L) and their 4 rotations.
    /// Each piece lives in a small square box (4x4 for I, 2x2 for O, 3x3 for the others);
    /// a cell is (x, y) inside that box, y growing downward. Turning clockwise maps (x, y) to (size - 1 - y, x).
    /// </summary>
    public static class TetrisShapes
    {
        public const int PieceCount = 7;

        private static readonly int[] Sizes = { 4, 2, 3, 3, 3, 3, 3 };

        // Cells of each piece in its starting rotation, as x, y pairs.
        private static readonly int[][] BaseCells =
        {
            new int[] { 0, 1, 1, 1, 2, 1, 3, 1 },   // I
            new int[] { 0, 0, 1, 0, 0, 1, 1, 1 },   // O
            new int[] { 1, 0, 0, 1, 1, 1, 2, 1 },   // T
            new int[] { 1, 0, 2, 0, 0, 1, 1, 1 },   // S
            new int[] { 0, 0, 1, 0, 1, 1, 2, 1 },   // Z
            new int[] { 0, 0, 0, 1, 1, 1, 2, 1 },   // J
            new int[] { 2, 0, 0, 1, 1, 1, 2, 1 }    // L
        };

        private static readonly int[][][] Rotations = new int[PieceCount][][];

        static TetrisShapes()
        {
            for (int type = 0; type < PieceCount; type++)
            {
                Rotations[type] = new int[4][];
                Rotations[type][0] = BaseCells[type];
                for (int turn = 1; turn < 4; turn++)
                {
                    Rotations[type][turn] = TurnClockwise(Rotations[type][turn - 1], Sizes[type]);
                }
            }
        }

        public static int Size(int type)
        {
            return Sizes[type];
        }

        /// <summary>
        /// The 4 cells of a piece as x, y pairs (8 numbers) in the given rotation (0-3).
        /// </summary>
        public static int[] Cells(int type, int rotation)
        {
            return Rotations[type][rotation];
        }

        private static int[] TurnClockwise(int[] cells, int size)
        {
            int[] turned = new int[cells.Length];
            for (int i = 0; i < cells.Length; i += 2)
            {
                turned[i] = size - 1 - cells[i + 1];
                turned[i + 1] = cells[i];
            }
            return turned;
        }
    }
}
