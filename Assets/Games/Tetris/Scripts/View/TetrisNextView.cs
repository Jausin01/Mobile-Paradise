using MobileParadise.Games.Tetris.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// Shows the next piece in a small 4 x 4 grid.
    /// </summary>
    public class TetrisNextView : MonoBehaviour
    {
        private const int Grid = 4;

        [SerializeField] private RectTransform _holder;

        private Image[] _cells;

        /// <summary>
        /// Creates the 16 cells. Call once.
        /// </summary>
        public void Build()
        {
            Vector2 holder = _holder.sizeDelta;
            float size = Mathf.Floor(Mathf.Min(holder.x, holder.y) / Grid);
            float grid = size * Grid;

            _cells = new Image[Grid * Grid];
            for (int i = 0; i < _cells.Length; i++)
            {
                GameObject go = new GameObject("Cell " + i, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_holder, false);

                RectTransform rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(size - 2f, size - 2f);
                rect.anchoredPosition = new Vector2(-grid / 2f + (i % Grid) * size + 1f, grid / 2f - (i / Grid) * size - 1f);

                _cells[i] = go.GetComponent<Image>();
                _cells[i].raycastTarget = false;
                _cells[i].enabled = false;
            }
        }

        public void Render(int type)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].enabled = false;
            }

            // Centered in the grid: the small pieces sit in a 2x2 or 3x3 box, so shift them to the middle.
            int offset = (Grid - TetrisShapes.Size(type)) / 2;
            int[] shape = TetrisShapes.Cells(type, 0);
            for (int i = 0; i < shape.Length; i += 2)
            {
                Image cell = _cells[(shape[i + 1] + offset) * Grid + shape[i] + offset];
                cell.color = TetrisColors.Piece(type);
                cell.enabled = true;
            }
        }
    }
}
