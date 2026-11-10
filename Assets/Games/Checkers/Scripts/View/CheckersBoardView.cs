using System;
using System.Collections.Generic;
using MobileParadise.Games.Checkers.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Checkers.View
{
    /// <summary>
    /// Draws the 8x8 board: squares, pieces (with a crown on kings) and the highlights
    /// (the selected piece, where it can go, and the last move). White is at the bottom.
    /// </summary>
    public class CheckersBoardView : MonoBehaviour
    {
        private const float FramePadding = 24f;

        private static readonly Color LightSquare = new Color(0.94f, 0.85f, 0.71f);
        private static readonly Color DarkSquare = new Color(0.71f, 0.53f, 0.39f);
        private static readonly Color SelectedColor = new Color(0.98f, 0.85f, 0.30f, 0.75f);
        private static readonly Color TargetColor = new Color(0.45f, 0.85f, 0.40f, 0.65f);
        private static readonly Color LastMoveColor = new Color(0.98f, 0.92f, 0.45f, 0.40f);

        [SerializeField] private RectTransform _holder;
        [SerializeField] private RectTransform _frame;
        [SerializeField] private Sprite _whiteSprite;
        [SerializeField] private Sprite _blackSprite;
        [SerializeField] private Sprite _crownSprite;

        private Image[] _squares;
        private Image[] _overlays;
        private Image[] _pieces;
        private Image[] _crowns;
        private bool _built;

        public event Action<int> SquareClicked;

        /// <summary>
        /// Creates the 64 squares. Call once.
        /// </summary>
        public void Build()
        {
            if (_built) return;
            _built = true;

            int count = CheckersBoard.SquareCount;
            _squares = new Image[count];
            _overlays = new Image[count];
            _pieces = new Image[count];
            _crowns = new Image[count];

            Vector2 holder = _holder.sizeDelta;
            float size = Mathf.Floor((Mathf.Min(holder.x, holder.y) - 2f * FramePadding) / CheckersBoard.Size);
            float grid = size * CheckersBoard.Size;
            _frame.sizeDelta = new Vector2(grid + 2f * FramePadding, grid + 2f * FramePadding);

            for (int square = 0; square < count; square++)
            {
                CreateSquare(square, square % CheckersBoard.Size, square / CheckersBoard.Size, size, grid);
            }
        }

        /// <summary>
        /// Shows the board. 'selected' is a square or -1; 'targets' are where the selected piece can go; 'lastFrom/lastTo' or -1.
        /// </summary>
        public void Render(CheckersBoard board, int selected, List<int> targets, int lastFrom, int lastTo)
        {
            for (int square = 0; square < CheckersBoard.SquareCount; square++)
            {
                Color overlay = Color.clear;
                if (square == lastFrom || square == lastTo) overlay = LastMoveColor;
                if (targets != null && targets.Contains(square)) overlay = TargetColor;
                if (square == selected) overlay = SelectedColor;

                _overlays[square].color = overlay;
                _overlays[square].enabled = overlay.a > 0f;

                int piece = board.Get(square);
                _pieces[square].enabled = piece != CheckersBoard.Empty;
                _crowns[square].enabled = CheckersBoard.IsKing(piece);
                if (piece != CheckersBoard.Empty)
                {
                    _pieces[square].sprite = CheckersBoard.IsWhite(piece) ? _whiteSprite : _blackSprite;
                }
            }
        }

        // ---- Helpers ----------------------------------------------------------

        private void CreateSquare(int square, int column, int row, float size, float grid)
        {
            GameObject squareObject = new GameObject("Square " + square, typeof(RectTransform), typeof(Image), typeof(Button));
            squareObject.transform.SetParent(_holder, false);

            RectTransform rect = squareObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(-grid / 2f + column * size, grid / 2f - row * size);

            Image image = squareObject.GetComponent<Image>();
            image.color = CheckersBoard.IsDarkSquare(row, column) ? DarkSquare : LightSquare;
            Button button = squareObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => OnSquareClicked(square));

            _overlays[square] = CreateChild(squareObject.transform, "Overlay", 0f, false);
            _pieces[square] = CreateChild(squareObject.transform, "Piece", size * 0.08f, true);
            _crowns[square] = CreateChild(squareObject.transform, "Crown", size * 0.28f, true);
            _crowns[square].sprite = _crownSprite;
            _squares[square] = image;
        }

        private static Image CreateChild(Transform parent, string name, float margin, bool keepAspect)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, margin);
            rect.offsetMax = new Vector2(-margin, -margin);

            Image image = go.GetComponent<Image>();
            image.preserveAspect = keepAspect;
            image.raycastTarget = false;
            image.enabled = false;
            return image;
        }

        private void OnSquareClicked(int square)
        {
            if (SquareClicked != null) SquareClicked(square);
        }
    }
}
