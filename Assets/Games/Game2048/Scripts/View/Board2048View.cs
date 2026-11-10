using MobileParadise.Games.Game2048.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// Draws the 4x4 board: a slot and a tile per cell. New tiles grow in and merged tiles pop a little.
    /// The tile size is worked out from the holder size.
    /// </summary>
    public class Board2048View : MonoBehaviour
    {
        private const float Gap = 14f;
        private const float FramePadding = 24f;
        private const float PopSeconds = 0.16f;

        private static readonly Color EmptyColor = new Color(0.60f, 0.45f, 0.30f);
        private static readonly Color DarkText = new Color(0.30f, 0.20f, 0.12f);
        private static readonly Color LightText = new Color(0.99f, 0.97f, 0.92f);

        // Index = exponent (1 = "2", 2 = "4" ...). The last one is used for anything above 2048.
        private static readonly Color[] TileColors =
        {
            EmptyColor,
            new Color(0.96f, 0.91f, 0.80f),
            new Color(0.95f, 0.85f, 0.62f),
            new Color(0.96f, 0.68f, 0.38f),
            new Color(0.96f, 0.55f, 0.32f),
            new Color(0.95f, 0.42f, 0.30f),
            new Color(0.88f, 0.28f, 0.22f),
            new Color(0.93f, 0.80f, 0.40f),
            new Color(0.92f, 0.77f, 0.28f),
            new Color(0.90f, 0.73f, 0.20f),
            new Color(0.88f, 0.68f, 0.12f),
            new Color(0.55f, 0.78f, 0.32f),
            new Color(0.25f, 0.22f, 0.32f)
        };

        [SerializeField] private RectTransform _holder;

        private Image[] _tiles;
        private TMP_Text[] _texts;
        private RectTransform[] _rects;
        private float[] _popTimer;
        private bool[] _popIsNew;
        private float _tileSize;

        /// <summary>
        /// Creates the 16 tiles. Call once.
        /// </summary>
        public void Build()
        {
            _tiles = new Image[Board2048.CellCount];
            _texts = new TMP_Text[Board2048.CellCount];
            _rects = new RectTransform[Board2048.CellCount];
            _popTimer = new float[Board2048.CellCount];
            _popIsNew = new bool[Board2048.CellCount];

            Vector2 holder = _holder.sizeDelta;
            float available = Mathf.Min(holder.x, holder.y) - 2f * FramePadding;
            _tileSize = Mathf.Floor((available - Gap * (Board2048.Size - 1)) / Board2048.Size);
            float gridSize = Board2048.Size * _tileSize + (Board2048.Size - 1) * Gap;

            for (int index = 0; index < Board2048.CellCount; index++)
            {
                CreateTile(index, index % Board2048.Size, index / Board2048.Size, gridSize);
            }
        }

        /// <summary>
        /// Shows the board. With animate the new tile and the merged tiles get a short pop.
        /// </summary>
        public void Render(Board2048 board, bool animate)
        {
            for (int index = 0; index < Board2048.CellCount; index++)
            {
                int exponent = board.Exponent(index);
                _tiles[index].color = TileColors[Mathf.Min(exponent, TileColors.Length - 1)];

                if (exponent == 0)
                {
                    _texts[index].text = "";
                }
                else
                {
                    int value = 1 << exponent;
                    _texts[index].text = value.ToString();
                    _texts[index].fontSize = _tileSize * FontFactor(value);
                    _texts[index].color = exponent <= 2 ? DarkText : LightText;
                }

                _popTimer[index] = 0f;
                _rects[index].localScale = Vector3.one;

                if (animate && exponent > 0)
                {
                    if (index == board.LastSpawn)
                    {
                        _popTimer[index] = PopSeconds;
                        _popIsNew[index] = true;
                    }
                    else if (board.WasMerged(index))
                    {
                        _popTimer[index] = PopSeconds;
                        _popIsNew[index] = false;
                    }
                }
            }
        }

        private void Update()
        {
            if (_popTimer == null) return;

            for (int index = 0; index < _popTimer.Length; index++)
            {
                if (_popTimer[index] <= 0f) continue;

                _popTimer[index] -= Time.unscaledDeltaTime;
                float progress = 1f - Mathf.Max(_popTimer[index], 0f) / PopSeconds;
                float scale = _popIsNew[index]
                    ? Mathf.Lerp(0.3f, 1f, progress)
                    : 1f + 0.18f * Mathf.Sin(progress * Mathf.PI);
                _rects[index].localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static float FontFactor(int value)
        {
            if (value < 100) return 0.55f;
            if (value < 1000) return 0.45f;
            if (value < 10000) return 0.36f;
            return 0.29f;
        }

        private void CreateTile(int index, int column, int row, float gridSize)
        {
            GameObject tileObject = new GameObject("Tile " + index, typeof(RectTransform), typeof(Image));
            tileObject.transform.SetParent(_holder, false);

            RectTransform rect = tileObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(_tileSize, _tileSize);
            rect.anchoredPosition = new Vector2(
                -gridSize / 2f + _tileSize / 2f + column * (_tileSize + Gap),
                gridSize / 2f - _tileSize / 2f - row * (_tileSize + Gap));

            Image image = tileObject.GetComponent<Image>();
            image.raycastTarget = false;

            GameObject textObject = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(tileObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            _tiles[index] = image;
            _texts[index] = text;
            _rects[index] = rect;
        }
    }
}
