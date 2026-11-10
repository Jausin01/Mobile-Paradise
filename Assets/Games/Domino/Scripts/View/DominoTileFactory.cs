using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// Builds the picture of one domino tile out of UI images: a border, an ivory face, a divider line and the dots.
    /// The dots use a round sprite that is drawn by code the first time it is needed (no art file).
    /// </summary>
    public static class DominoTileFactory
    {
        public static readonly Color FaceColor = new Color(0.97f, 0.95f, 0.88f);
        public static readonly Color DimmedFaceColor = new Color(0.62f, 0.60f, 0.55f);
        public static readonly Color BorderColor = new Color(0.25f, 0.18f, 0.12f);
        public static readonly Color HighlightColor = new Color(0.98f, 0.80f, 0.15f);

        private static readonly Color PipColor = new Color(0.15f, 0.10f, 0.07f);

        // Which of the 9 spots (row by row) hold a dot, for each value 0-6.
        private static readonly int[][] PipSpots =
        {
            new int[] { },
            new int[] { 4 },
            new int[] { 0, 8 },
            new int[] { 0, 4, 8 },
            new int[] { 0, 2, 6, 8 },
            new int[] { 0, 2, 4, 6, 8 },
            new int[] { 0, 2, 3, 5, 6, 8 }
        };

        private static Sprite _pipSprite;

        /// <summary>
        /// Creates a tile of the given size. 'vertical' puts the first value on top, otherwise on the left.
        /// The root has an Image that can receive taps (turn off raycastTarget for tiles that are only shown).
        /// </summary>
        public static RectTransform Create(Transform parent, string name, int first, int second, bool vertical,
            float width, float height, Color face, Color border)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            root.GetComponent<Image>().color = border;

            float margin = Mathf.Max(2f, Mathf.Min(width, height) * 0.06f);
            RectTransform faceRect = CreateImage(rect, "Face", face, null);
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = new Vector2(margin, margin);
            faceRect.offsetMax = new Vector2(-margin, -margin);

            CreateDivider(rect, vertical, width, height);
            CreateHalf(rect, first, vertical, 0, width, height);
            CreateHalf(rect, second, vertical, 1, width, height);
            return rect;
        }

        // ---- Parts ------------------------------------------------------------

        private static void CreateDivider(RectTransform parent, bool vertical, float width, float height)
        {
            float thickness = Mathf.Max(2f, Mathf.Min(width, height) * 0.05f);
            Vector2 size = vertical ? new Vector2(width * 0.8f, thickness) : new Vector2(thickness, height * 0.8f);

            RectTransform line = CreateImage(parent, "Divider", PipColor, null);
            Center(line, size, Vector2.zero);
        }

        // One half of the tile: the dots for 'value', arranged on a 3x3 grid in the middle of the half.
        private static void CreateHalf(RectTransform parent, int value, bool vertical, int half, float width, float height)
        {
            float halfWidth = vertical ? width : width / 2f;
            float halfHeight = vertical ? height / 2f : height;
            float sign = half == 0 ? 1f : -1f;
            Vector2 center = vertical ? new Vector2(0f, sign * height / 4f) : new Vector2(-sign * width / 4f, 0f);

            float cell = Mathf.Min(halfWidth, halfHeight) * 0.78f / 3f;
            float diameter = cell * 0.78f;
            int[] spots = PipSpots[value];
            for (int i = 0; i < spots.Length; i++)
            {
                int column = spots[i] % 3;
                int row = spots[i] / 3;
                RectTransform pip = CreateImage(parent, "Pip", PipColor, GetPipSprite());
                Center(pip, new Vector2(diameter, diameter), center + new Vector2((column - 1) * cell, (1 - row) * cell));
            }
        }

        private static RectTransform CreateImage(RectTransform parent, string name, Color color, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            return go.GetComponent<RectTransform>();
        }

        private static void Center(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static Sprite GetPipSprite()
        {
            if (_pipSprite != null) return _pipSprite;

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            float radius = size / 2f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size / 2f;
                    float dy = y + 0.5f - size / 2f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius + 0.5f - distance)));
                }
            }
            texture.Apply();

            _pipSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _pipSprite;
        }
    }
}
