using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// Reads the finger on the letter grid. Put it on a transparent raycast Image that covers the grid.
    /// Drag from one letter to another: the end snaps to the nearest of the 8 directions.
    /// A simple tap (no drag) is reported as Tapped, so the player can also tap the first and then the last letter.
    /// </summary>
    public class WordGridInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        // Right, down-right, down, down-left, left, up-left, up, up-right (dy grows downward).
        private static readonly int[] StepX = { 1, 1, 0, -1, -1, -1, 0, 1 };
        private static readonly int[] StepY = { 0, 1, 1, 1, 0, -1, -1, -1 };

        private const float DeadZone = 0.6f;   // in cells

        private RectTransform _holder;
        private int _size;
        private float _cell;
        private int _start = -1;
        private int _end = -1;

        /// <summary>The line being dragged. (-1, -1) means no line any more.</summary>
        public event Action<int, int> Previewed;
        public event Action<int, int> Released;
        public event Action<int> Tapped;

        /// <summary>
        /// The holder is the grid's parent (pivot in the middle); 'cell' is the size of one letter square.
        /// </summary>
        public void Setup(RectTransform holder, int size, float cell)
        {
            _holder = holder;
            _size = size;
            _cell = cell;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 cells;
            if (!ToCellSpace(eventData, out cells)) return;

            _start = CellAt(cells);
            _end = _start;
            if (_start >= 0 && Previewed != null) Previewed(_start, _start);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_start < 0) return;

            Vector2 cells;
            if (!ToCellSpace(eventData, out cells)) return;

            int end = SnappedEnd(cells);
            if (end == _end) return;

            _end = end;
            if (Previewed != null) Previewed(_start, _end);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_start < 0) return;

            int start = _start;
            int end = _end;
            _start = -1;
            _end = -1;

            if (Previewed != null) Previewed(-1, -1);
            if (end == start)
            {
                if (Tapped != null) Tapped(start);
            }
            else if (Released != null)
            {
                Released(start, end);
            }
        }

        // ---- Helpers ----------------------------------------------------------

        // The pointer position in "cell units" from the top-left corner of the grid (x to the right, y downward).
        private bool ToCellSpace(PointerEventData eventData, out Vector2 cells)
        {
            cells = Vector2.zero;
            if (_holder == null) return false;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_holder, eventData.position, eventData.pressEventCamera, out local))
            {
                return false;
            }

            float half = _cell * _size / 2f;
            cells = new Vector2((local.x + half) / _cell, (half - local.y) / _cell);
            return true;
        }

        private int CellAt(Vector2 cells)
        {
            int column = Mathf.FloorToInt(cells.x);
            int row = Mathf.FloorToInt(cells.y);
            if (column < 0 || column >= _size || row < 0 || row >= _size) return -1;
            return row * _size + column;
        }

        private int SnappedEnd(Vector2 cells)
        {
            int startColumn = _start % _size;
            int startRow = _start / _size;
            float vx = cells.x - (startColumn + 0.5f);
            float vy = cells.y - (startRow + 0.5f);
            if (Mathf.Sqrt(vx * vx + vy * vy) < DeadZone) return _start;

            float angle = Mathf.Atan2(vy, vx);   // 0 = right, grows clockwise on screen
            int octant = Mathf.RoundToInt(angle / (Mathf.PI / 4f));
            octant = ((octant % 8) + 8) % 8;

            int dx = StepX[octant];
            int dy = StepY[octant];
            float along = (vx * dx + vy * dy) / (dx * dx + dy * dy);
            int steps = Mathf.RoundToInt(along);

            // Keep the end inside the grid.
            while (steps > 0)
            {
                int column = startColumn + dx * steps;
                int row = startRow + dy * steps;
                if (column >= 0 && column < _size && row >= 0 && row < _size) break;
                steps--;
            }
            return (startRow + dy * steps) * _size + (startColumn + dx * steps);
        }
    }
}
