using UnityEngine;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// The color of each piece (0 I, 1 O, 2 T, 3 S, 4 Z, 5 J, 6 L) and of an empty cell.
    /// </summary>
    public static class TetrisColors
    {
        public static readonly Color Empty = new Color(0.12f, 0.09f, 0.07f);

        private static readonly Color[] PieceColors =
        {
            new Color(0.35f, 0.85f, 0.90f),
            new Color(0.95f, 0.85f, 0.30f),
            new Color(0.72f, 0.45f, 0.85f),
            new Color(0.50f, 0.85f, 0.45f),
            new Color(0.90f, 0.40f, 0.40f),
            new Color(0.40f, 0.55f, 0.90f),
            new Color(0.95f, 0.65f, 0.30f)
        };

        public static Color Piece(int type)
        {
            return PieceColors[type];
        }
    }
}
