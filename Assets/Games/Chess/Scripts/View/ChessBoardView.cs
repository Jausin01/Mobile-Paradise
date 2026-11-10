using System;
using System.Collections.Generic;
using MobileParadise.Games.Chess.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// Draws the board: 64 tappable squares, the pieces, and the highlights
    /// (last move, selected piece, legal targets, king in check).
    /// White is at the bottom. Squares are 128 px: 32 px art scaled x4.
    /// </summary>
    public class ChessBoardView : MonoBehaviour
    {
        private const float SquareSize = 128f;

        private static readonly Color LastMoveTint = new Color(0.96f, 0.90f, 0.35f);
        private static readonly Color SelectedTint = new Color(0.45f, 0.80f, 0.45f);
        private static readonly Color CheckTint = new Color(0.90f, 0.25f, 0.25f);
        private static readonly Color CaptureTint = new Color(0.90f, 0.40f, 0.30f);
        private static readonly Color MarkerColor = new Color(0.15f, 0.40f, 0.15f, 0.65f);

        [SerializeField] private RectTransform _boardRoot;
        [SerializeField] private PieceSpriteSet _pieceSet;

        private BoardTheme _theme = BoardTheme.All[0];
        private bool _flipped;   // true = Black at the bottom
        private readonly RectTransform[] _squareRects = new RectTransform[64];

        private readonly Image[] _squares = new Image[64];
        private readonly Image[] _pieces = new Image[64];
        private readonly Image[] _markers = new Image[64];

        public event Action<int> SquareClicked;

        public PieceSpriteSet PieceSet
        {
            get { return _pieceSet; }
        }

        /// <summary>
        /// Changes the piece art and square colors. Call Render afterwards.
        /// </summary>
        public void SetAppearance(PieceSpriteSet pieceSet, BoardTheme theme, bool blackAtBottom)
        {
            if (pieceSet != null) _pieceSet = pieceSet;
            if (theme != null) _theme = theme;

            _flipped = blackAtBottom;
            PositionSquares();
        }

        public void Build()
        {
            if (_pieceSet == null)
            {
                Debug.LogError("ChessBoardView: Piece Set is not assigned. Rebuild the Chess scene or assign it on the Board object.");
            }

            Sprite dot = CreateDotSprite();

            for (int square = 0; square < 64; square++)
            {
                GameObject squareObject = new GameObject("Square " + Square.Name(square), typeof(RectTransform), typeof(Image), typeof(Button));
                squareObject.transform.SetParent(_boardRoot, false);

                RectTransform rect = squareObject.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.sizeDelta = new Vector2(SquareSize, SquareSize);
                _squareRects[square] = rect;

                Image image = squareObject.GetComponent<Image>();
                Button button = squareObject.GetComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = image;

                int clickedSquare = square;
                button.onClick.AddListener(() => OnSquareClicked(clickedSquare));

                _squares[square] = image;
                _markers[square] = CreateChildImage(squareObject.transform, "Marker", dot, false);
                _pieces[square] = CreateChildImage(squareObject.transform, "Piece", null, true);
            }

            PositionSquares();
        }

        /// <summary>
        /// Places each square. When flipped (player has Black) the board is turned around.
        /// </summary>
        private void PositionSquares()
        {
            for (int square = 0; square < 64; square++)
            {
                if (_squareRects[square] == null) continue;

                int file = _flipped ? 7 - Square.File(square) : Square.File(square);
                int rank = _flipped ? 7 - Square.Rank(square) : Square.Rank(square);
                _squareRects[square].anchoredPosition = new Vector2(file * SquareSize, rank * SquareSize);
            }
        }

        public void Render(ChessGame game, int selected, List<Move> selectedMoves)
        {
            if (_pieceSet == null) return;

            Board board = game.Board;

            int checkedKing = -1;
            if (game.Status == GameStatus.Check || game.Status == GameStatus.Checkmate)
            {
                checkedKing = board.KingSquare(game.SideToMove);
            }

            int lastFrom = -1;
            int lastTo = -1;
            if (game.HasLastMove)
            {
                lastFrom = game.LastMove.From;
                lastTo = game.LastMove.To;
            }

            for (int square = 0; square < 64; square++)
            {
                bool dark = (Square.File(square) + Square.Rank(square)) % 2 == 0;
                Color color = dark ? _theme.Dark : _theme.Light;

                if (square == lastFrom || square == lastTo) color = Color.Lerp(color, LastMoveTint, 0.5f);
                if (square == checkedKing) color = Color.Lerp(color, CheckTint, 0.7f);
                if (square == selected) color = Color.Lerp(color, SelectedTint, 0.6f);
                _squares[square].color = color;

                _markers[square].enabled = false;

                Piece piece = board.Get(square);
                _pieces[square].enabled = !piece.IsNone;
                if (!piece.IsNone)
                {
                    _pieces[square].sprite = _pieceSet.Get(piece.Type, piece.Color);
                }
            }

            if (selectedMoves == null) return;

            for (int i = 0; i < selectedMoves.Count; i++)
            {
                int target = selectedMoves[i].To;
                bool isCapture = !board.Get(target).IsNone || selectedMoves[i].Kind == MoveKind.EnPassant;

                if (isCapture) _squares[target].color = Color.Lerp(_squares[target].color, CaptureTint, 0.6f);
                else _markers[target].enabled = true;
            }
        }

        private void OnSquareClicked(int square)
        {
            if (SquareClicked != null)
            {
                SquareClicked(square);
            }
        }

        private static Image CreateChildImage(Transform parent, string name, Sprite sprite, bool stretch)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(parent, false);

            RectTransform rect = child.GetComponent<RectTransform>();
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(48, 48);
            }

            Image image = child.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = stretch ? Color.white : MarkerColor;
            image.enabled = false;
            return image;
        }

        /// <summary>
        /// Small round sprite for "you can move here" markers, made in code (no art file needed).
        /// </summary>
        private static Sprite CreateDotSprite()
        {
            const int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            float center = (size - 1) / 2f;
            float radius = 7.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    texture.SetPixel(x, y, dx * dx + dy * dy <= radius * radius ? Color.white : Color.clear);
                }
            }
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
