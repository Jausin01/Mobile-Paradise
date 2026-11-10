using System.Collections.Generic;
using MobileParadise.Games.Snake.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// Draws the field: a checkered grass, the snake (head, body, tail sprites turned along the snake) and the apple.
    /// The cell size is worked out from the holder size and the level, so every level fills the same area.
    /// The sprites all point up in the art; each piece is rotated so its top faces the head.
    /// </summary>
    public class SnakeBoardView : MonoBehaviour
    {
        private const float FramePadding = 24f;
        private const float SnakeScale = 1.4f;   // the art has empty margins, so pieces are drawn bigger than the cell
        private const float AppleScale = 1.15f;
        private const float CurveScale = 1.2f;   // the curve art is a smaller 20x20 picture with a thinner line, so it needs its own scale

        private static readonly Color GrassA = new Color(0.67f, 0.84f, 0.46f);
        private static readonly Color GrassB = new Color(0.62f, 0.79f, 0.41f);
        private static readonly Color DeadHeadColor = new Color(1f, 0.55f, 0.50f);

        [SerializeField] private RectTransform _holder;
        [SerializeField] private RectTransform _frame;
        [SerializeField] private Sprite _headSprite;
        [SerializeField] private Sprite _bodySprite;
        [SerializeField] private Sprite _tailSprite;
        [SerializeField] private Sprite _curveSprite;
        [SerializeField] private Sprite _appleSprite;

        private readonly List<GameObject> _cellObjects = new List<GameObject>();
        private Image[] _cells;
        private Image[] _pieces;
        private RectTransform[] _pieceRects;
        private int _columns;
        private float _cellSize;

        /// <summary>
        /// Creates the cells for a new game (and removes the old ones).
        /// </summary>
        public void Build(int columns, int rows)
        {
            for (int i = 0; i < _cellObjects.Count; i++)
            {
                Destroy(_cellObjects[i]);
            }
            _cellObjects.Clear();

            _columns = columns;
            int count = columns * rows;
            _cells = new Image[count];
            _pieces = new Image[count];
            _pieceRects = new RectTransform[count];

            Vector2 holder = _holder.sizeDelta;
            _cellSize = Mathf.Floor(Mathf.Min(
                (holder.x - 2f * FramePadding) / columns,
                (holder.y - 2f * FramePadding) / rows));

            float gridWidth = columns * _cellSize;
            float gridHeight = rows * _cellSize;
            _frame.sizeDelta = new Vector2(gridWidth + 2f * FramePadding, gridHeight + 2f * FramePadding);

            // All grass first, then all pieces, so a piece is never covered by the grass of the next cell.
            for (int index = 0; index < count; index++)
            {
                CreateCell(index, index % columns, index / columns, gridWidth, gridHeight);
            }
            for (int index = 0; index < count; index++)
            {
                CreatePiece(index, index % columns, index / columns, gridWidth, gridHeight);
            }
        }

        public void Render(SnakeGame game)
        {
            for (int index = 0; index < _cells.Length; index++)
            {
                int column = index % _columns;
                int row = index / _columns;
                _cells[index].color = ((column + row) % 2 == 0) ? GrassA : GrassB;
                _pieces[index].enabled = false;
            }

            ShowPiece(game.Food, _appleSprite, SnakeDirection.Up, AppleScale, Color.white);

            int[] body = game.CopyBody();
            for (int i = 0; i < body.Length; i++)
            {
                Sprite sprite;
                SnakeDirection facing;
                Color tint = Color.white;

                if (i == 0)
                {
                    sprite = _headSprite;
                    facing = game.Direction;
                    if (game.Status == SnakeStatus.Dead) tint = DeadHeadColor;
                }
                else if (i == body.Length - 1)
                {
                    sprite = _tailSprite;
                    facing = DirectionTo(body[i], body[i - 1]);   // the top of the piece faces the head
                }
                else
                {
                    SnakeDirection toHead = DirectionTo(body[i], body[i - 1]);
                    SnakeDirection toTail = DirectionTo(body[i], body[i + 1]);

                    if (toHead == SnakeGame.Opposite(toTail))
                    {
                        sprite = _bodySprite;
                        facing = toHead;
                    }
                    else
                    {
                        // A turn: the curve art connects Up and Right; it is rotated to connect the two neighbours.
                        ShowPiece(body[i], _curveSprite, SnakeDirection.Up, CurveScale, tint);
                        _pieceRects[body[i]].localRotation = Quaternion.Euler(0f, 0f, CurveAngle(toHead, toTail));
                        continue;
                    }
                }

                ShowPiece(body[i], sprite, facing, SnakeScale, tint);
            }
        }

        // ---- Helpers ----------------------------------------------------------

        private void ShowPiece(int cell, Sprite sprite, SnakeDirection facing, float scale, Color tint)
        {
            Image piece = _pieces[cell];
            piece.sprite = sprite;
            piece.color = tint;
            piece.enabled = true;

            _pieceRects[cell].sizeDelta = new Vector2(_cellSize * scale, _cellSize * scale);
            _pieceRects[cell].localRotation = Quaternion.Euler(0f, 0f, AngleOf(facing));
        }

        // The sprite points up; turning it counter-clockwise by this angle makes it face the direction.
        private static float AngleOf(SnakeDirection direction)
        {
            switch (direction)
            {
                case SnakeDirection.Left: return 90f;
                case SnakeDirection.Down: return 180f;
                case SnakeDirection.Right: return 270f;
                default: return 0f;
            }
        }

        // Angle (counter-clockwise) that turns the Up+Right curve into one that joins the two given sides.
        private static float CurveAngle(SnakeDirection a, SnakeDirection b)
        {
            bool up = a == SnakeDirection.Up || b == SnakeDirection.Up;
            bool right = a == SnakeDirection.Right || b == SnakeDirection.Right;
            bool left = a == SnakeDirection.Left || b == SnakeDirection.Left;

            if (up && right) return 0f;
            if (up && left) return 90f;
            if (left) return 180f;   // Left + Down
            return 270f;             // Right + Down
        }

        // The direction from one cell to a neighbouring cell (the board may wrap around its edges).
        private SnakeDirection DirectionTo(int from, int to)
        {
            int columns = _columns;
            int rows = _cells.Length / columns;
            int dc = to % columns - from % columns;
            int dr = to / columns - from / columns;

            if (Mathf.Abs(dc) > 1) dc = dc > 0 ? -1 : 1;
            if (Mathf.Abs(dr) > 1) dr = dr > 0 ? -1 : 1;

            if (dc > 0) return SnakeDirection.Right;
            if (dc < 0) return SnakeDirection.Left;
            if (dr > 0) return SnakeDirection.Down;
            return SnakeDirection.Up;
        }

        private void CreateCell(int index, int column, int row, float gridWidth, float gridHeight)
        {
            GameObject cellObject = new GameObject("Cell " + index, typeof(RectTransform), typeof(Image));
            cellObject.transform.SetParent(_holder, false);
            _cellObjects.Add(cellObject);

            RectTransform rect = cellObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(_cellSize, _cellSize);
            rect.anchoredPosition = new Vector2(
                -gridWidth / 2f + column * _cellSize,
                gridHeight / 2f - row * _cellSize);

            Image image = cellObject.GetComponent<Image>();
            image.raycastTarget = false;
            _cells[index] = image;
        }

        private void CreatePiece(int index, int column, int row, float gridWidth, float gridHeight)
        {
            GameObject pieceObject = new GameObject("Piece " + index, typeof(RectTransform), typeof(Image));
            pieceObject.transform.SetParent(_holder, false);
            _cellObjects.Add(pieceObject);

            RectTransform rect = pieceObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);   // rotates around its own center
            rect.anchoredPosition = new Vector2(
                -gridWidth / 2f + (column + 0.5f) * _cellSize,
                gridHeight / 2f - (row + 0.5f) * _cellSize);

            Image image = pieceObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = false;

            _pieces[index] = image;
            _pieceRects[index] = rect;
        }
    }
}
