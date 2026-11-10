using System;
using MobileParadise.Games.WordSearch.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// Draws the letter grid: a square per cell with its letter, colored bands over the words already found,
    /// the line being dragged, the first letter picked by tapping, and the hint.
    /// </summary>
    public class WordSearchBoardView : MonoBehaviour
    {
        private const float FramePadding = 20f;

        private static readonly Color PaperColor = new Color(0.98f, 0.95f, 0.86f);
        private static readonly Color LetterColor = new Color(0.17f, 0.11f, 0.07f);
        private static readonly Color PreviewColor = new Color(0.98f, 0.85f, 0.30f, 0.85f);
        private static readonly Color TapColor = new Color(0.98f, 0.85f, 0.30f, 0.85f);
        private static readonly Color HintColor = new Color(0.95f, 0.45f, 0.35f, 0.85f);

        // One soft color per found word, repeating when there are more words than colors.
        private static readonly Color[] FoundColors =
        {
            new Color(0.55f, 0.82f, 0.50f, 0.80f),
            new Color(0.50f, 0.74f, 0.92f, 0.80f),
            new Color(0.95f, 0.68f, 0.45f, 0.80f),
            new Color(0.80f, 0.60f, 0.90f, 0.80f),
            new Color(0.95f, 0.60f, 0.70f, 0.80f),
            new Color(0.55f, 0.85f, 0.80f, 0.80f),
            new Color(0.90f, 0.82f, 0.45f, 0.80f),
            new Color(0.70f, 0.78f, 0.45f, 0.80f)
        };

        [SerializeField] private RectTransform _holder;
        [SerializeField] private RectTransform _frame;
        [SerializeField] private WordGridInput _input;

        private Image[] _overlays;
        private TextMeshProUGUI[] _letters;
        private GameObject[] _cells;
        private int _size;

        public event Action<int, int> LineReleased;
        public event Action<int> CellTapped;
        public event Action<int, int> LinePreviewed;

        public void Init()
        {
            _input.Previewed += (a, b) => { if (LinePreviewed != null) LinePreviewed(a, b); };
            _input.Released += (a, b) => { if (LineReleased != null) LineReleased(a, b); };
            _input.Tapped += (cell) => { if (CellTapped != null) CellTapped(cell); };
        }

        /// <summary>
        /// Creates the squares for a puzzle of this size (the old ones are removed first).
        /// </summary>
        public void Build(int size)
        {
            if (_cells != null)
            {
                for (int i = 0; i < _cells.Length; i++)
                {
                    Destroy(_cells[i]);
                }
            }

            _size = size;
            int count = size * size;
            _cells = new GameObject[count];
            _overlays = new Image[count];
            _letters = new TextMeshProUGUI[count];

            Vector2 holder = _holder.sizeDelta;
            float cell = Mathf.Floor((Mathf.Min(holder.x, holder.y) - 2f * FramePadding) / size);
            float grid = cell * size;
            _frame.sizeDelta = new Vector2(grid + 2f * FramePadding, grid + 2f * FramePadding);

            for (int i = 0; i < count; i++)
            {
                CreateCell(i, cell, grid);
            }

            _input.Setup(_holder, size, cell);
            _input.transform.SetAsLastSibling();   // above the squares, so it receives the finger
        }

        /// <summary>
        /// Shows the letters and colors. 'preview' is the line being dragged (or null), 'tapCell' the first tapped
        /// letter and 'hintCell' the hint (each -1 for none).
        /// </summary>
        public void Render(WordSearchPuzzle puzzle, int[] preview, int tapCell, int hintCell)
        {
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                _letters[i].text = puzzle.Letter(i).ToString();
                _overlays[i].enabled = false;
            }

            for (int w = 0; w < puzzle.WordCount; w++)
            {
                if (!puzzle.Word(w).Found) continue;
                Paint(puzzle.CellsOf(w), FoundColors[w % FoundColors.Length]);
            }

            if (hintCell >= 0) Paint(new int[] { hintCell }, HintColor);
            if (tapCell >= 0) Paint(new int[] { tapCell }, TapColor);
            if (preview != null) Paint(preview, PreviewColor);
        }

        // ---- Helpers ----------------------------------------------------------

        private void Paint(int[] cells, Color color)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                _overlays[cells[i]].color = color;
                _overlays[cells[i]].enabled = true;
            }
        }

        private void CreateCell(int index, float cell, float grid)
        {
            int column = index % _size;
            int row = index / _size;

            GameObject cellObject = new GameObject("Cell " + index, typeof(RectTransform), typeof(Image));
            cellObject.transform.SetParent(_holder, false);
            _cells[index] = cellObject;

            RectTransform rect = cellObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(cell - 2f, cell - 2f);
            rect.anchoredPosition = new Vector2(-grid / 2f + column * cell + 1f, grid / 2f - row * cell - 1f);

            Image paper = cellObject.GetComponent<Image>();
            paper.color = PaperColor;
            paper.raycastTarget = false;

            GameObject overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(cellObject.transform, false);
            StretchToParent(overlay.GetComponent<RectTransform>());
            Image overlayImage = overlay.GetComponent<Image>();
            overlayImage.raycastTarget = false;
            overlayImage.enabled = false;
            _overlays[index] = overlayImage;

            GameObject letter = new GameObject("Letter", typeof(RectTransform), typeof(TextMeshProUGUI));
            letter.transform.SetParent(cellObject.transform, false);
            StretchToParent(letter.GetComponent<RectTransform>());
            TextMeshProUGUI text = letter.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.fontSize = cell * 0.62f;
            text.color = LetterColor;
            text.raycastTarget = false;
            _letters[index] = text;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
