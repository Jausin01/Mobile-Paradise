using MobileParadise.Games.Breakout.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// Draws the field: the bricks, the paddle and the ball. The field is 1000 x 1400 units and the holder
    /// has that size, so a world position is used directly as a position inside the holder.
    /// </summary>
    public class BreakoutFieldView : MonoBehaviour
    {
        private const float FrameScale = 100f / 32f / 4f;
        private const float BrickGap = 6f;

        private static readonly Color PaddleColor = new Color(0.93f, 0.80f, 0.50f);
        private static readonly Color BallColor = new Color(0.98f, 0.97f, 0.92f);

        // One color per brick row; tougher bricks are drawn darker.
        private static readonly Color[] RowColors =
        {
            new Color(0.90f, 0.35f, 0.32f),
            new Color(0.95f, 0.55f, 0.30f),
            new Color(0.95f, 0.80f, 0.35f),
            new Color(0.55f, 0.80f, 0.40f),
            new Color(0.35f, 0.70f, 0.85f),
            new Color(0.45f, 0.50f, 0.85f),
            new Color(0.65f, 0.45f, 0.80f),
            new Color(0.85f, 0.50f, 0.70f),
            new Color(0.75f, 0.75f, 0.75f)
        };

        [SerializeField] private RectTransform _holder;
        [SerializeField] private Sprite _frameSprite;

        private Image[] _bricks;
        private RectTransform _paddle;
        private RectTransform _ball;
        private bool _built;

        /// <summary>
        /// Creates the bricks, paddle and ball. Call once.
        /// </summary>
        public void Build()
        {
            if (_built) return;
            _built = true;

            int count = BreakoutLevels.Columns * BreakoutLevels.Rows;
            _bricks = new Image[count];
            for (int index = 0; index < count; index++)
            {
                int column = index % BreakoutLevels.Columns;
                int row = index / BreakoutLevels.Columns;
                Image brick = CreatePiece("Brick " + index, BrickWidth() - BrickGap, BreakoutWorld.BrickHeight - BrickGap, true).GetComponent<Image>();
                RectTransform rect = brick.rectTransform;
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(column * BreakoutWorld.BrickWidth + BrickGap / 2f, -(BreakoutWorld.BrickTop + row * BreakoutWorld.BrickHeight + BrickGap / 2f));
                _bricks[index] = brick;
            }

            _paddle = CreatePiece("Paddle", 100f, BreakoutWorld.PaddleHeight, true);
            _paddle.GetComponent<Image>().color = PaddleColor;

            _ball = CreatePiece("Ball", BreakoutWorld.BallRadius * 2f, BreakoutWorld.BallRadius * 2f, false);
            Image ballImage = _ball.GetComponent<Image>();
            ballImage.sprite = MakeCircleSprite();
            ballImage.color = BallColor;
        }

        public void Render(BreakoutWorld world)
        {
            for (int index = 0; index < _bricks.Length; index++)
            {
                int strength = world.BrickStrength(index);
                _bricks[index].enabled = strength > 0;
                if (strength <= 0) continue;

                float shade = strength == 1 ? 1f : (strength == 2 ? 0.72f : 0.5f);
                Color color = RowColors[(index / BreakoutLevels.Columns) % RowColors.Length] * shade;
                color.a = 1f;
                _bricks[index].color = color;
            }

            _paddle.sizeDelta = new Vector2(world.PaddleWidth, BreakoutWorld.PaddleHeight);
            _paddle.anchoredPosition = new Vector2(world.PaddleX - world.PaddleWidth / 2f, -BreakoutWorld.PaddleTop);
            _ball.anchoredPosition = new Vector2(world.BallX, -world.BallY);
        }

        // ---- Helpers ----------------------------------------------------------

        private static float BrickWidth()
        {
            return BreakoutWorld.BrickWidth;
        }

        // A piece anchored at the top-left of the holder. 'framed' pieces use the sliced frame art.
        private RectTransform CreatePiece(string name, float width, float height, bool framed)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_holder, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = framed ? new Vector2(0, 1) : new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);

            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            if (framed)
            {
                image.sprite = _frameSprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = FrameScale;
            }
            return rect;
        }

        // A small white circle made in code, so the ball needs no art file.
        private static Sprite MakeCircleSprite()
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            float center = (size - 1) / 2f;
            float radius = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    bool inside = dx * dx + dy * dy <= radius * radius;
                    texture.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
