using MobileParadise.Games.Tetris.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// Draws the 10 x 20 board: the landed blocks, the falling piece and, on Easy, a faint "ghost"
    /// where the piece will land. The wooden frame is sized to the grid.
    /// </summary>
    public class TetrisBoardView : MonoBehaviour
    {
        private const float FramePadding = 16f;
        private const float GhostAlpha = 0.35f;

        [SerializeField] private RectTransform _holder;
        [SerializeField] private RectTransform _frame;

        private Image[] _cells;
        private float _cellSize;
        private bool _built;

        public float CellSize { get { return _cellSize; } }
        public RectTransform Holder { get { return _holder; } }

        /// <summary>
        /// Creates the 200 cells. Call once.
        /// </summary>
        public void Build()
        {
            if (_built) return;
            _built = true;

            Vector2 holder = _holder.sizeDelta;
            _cellSize = Mathf.Floor(Mathf.Min((holder.x - 2f * FramePadding) / TetrisGame.Width,
                (holder.y - 2f * FramePadding) / TetrisGame.Height));
            float gridWidth = _cellSize * TetrisGame.Width;
            float gridHeight = _cellSize * TetrisGame.Height;
            _frame.sizeDelta = new Vector2(gridWidth + 2f * FramePadding, gridHeight + 2f * FramePadding);

            _cells = new Image[TetrisGame.Width * TetrisGame.Height];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = CreateCell(i, gridWidth, gridHeight);
            }
        }

        public void Render(TetrisGame game, bool showGhost)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                int block = game.Cell(i);
                _cells[i].color = block == 0 ? TetrisColors.Empty : TetrisColors.Piece(block - 1);
            }

            if (game.IsOver) return;

            Color pieceColor = TetrisColors.Piece(game.CurrentType);
            if (showGhost)
            {
                int[] ghostCells = game.GhostCells();
                for (int i = 0; i < ghostCells.Length; i++)
                {
                    _cells[ghostCells[i]].color = Color.Lerp(TetrisColors.Empty, pieceColor, GhostAlpha);
                }
            }

            int[] pieceCells = game.PieceCells();
            for (int i = 0; i < pieceCells.Length; i++)
            {
                _cells[pieceCells[i]].color = pieceColor;
            }
        }

        private Image CreateCell(int index, float gridWidth, float gridHeight)
        {
            int column = index % TetrisGame.Width;
            int row = index / TetrisGame.Width;

            GameObject go = new GameObject("Cell " + index, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_holder, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(_cellSize - 2f, _cellSize - 2f);
            rect.anchoredPosition = new Vector2(-gridWidth / 2f + column * _cellSize + 1f, gridHeight / 2f - row * _cellSize - 1f);

            Image image = go.GetComponent<Image>();
            image.color = TetrisColors.Empty;
            image.raycastTarget = false;
            return image;
        }
    }
}
