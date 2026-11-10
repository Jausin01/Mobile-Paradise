using System;
using System.Collections.Generic;
using MobileParadise.Games.Minesweeper.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Minesweeper.View
{
    /// <summary>
    /// Draws the grid: closed cells, opened cells with their number, flags and (when the game ends) the mines.
    /// The cell size is worked out from the holder size and the level, so every level fills the same area.
    /// </summary>
    public class MinesBoardView : MonoBehaviour
    {
        private const float Gap = 6f;
        private const float FramePadding = 24f;

        private static readonly Color HiddenColor = new Color(0.78f, 0.66f, 0.45f);
        private static readonly Color OpenColor = new Color(0.96f, 0.91f, 0.80f);
        private static readonly Color BoomColor = new Color(0.88f, 0.35f, 0.30f);
        private static readonly Color WrongFlagColor = new Color(0.90f, 0.62f, 0.55f);

        // 1..8, the classic colors made a bit darker so they read on cream.
        private static readonly Color[] NumberColors =
        {
            Color.clear,
            new Color(0.15f, 0.30f, 0.80f),
            new Color(0.10f, 0.50f, 0.20f),
            new Color(0.80f, 0.15f, 0.15f),
            new Color(0.10f, 0.12f, 0.50f),
            new Color(0.50f, 0.20f, 0.10f),
            new Color(0.05f, 0.45f, 0.50f),
            new Color(0.15f, 0.10f, 0.07f),
            new Color(0.40f, 0.40f, 0.40f)
        };

        [SerializeField] private RectTransform _holder;
        [SerializeField] private RectTransform _frame;
        [SerializeField] private Sprite _flagSprite;
        [SerializeField] private Sprite _mineSprite;

        private readonly List<GameObject> _cellObjects = new List<GameObject>();
        private Image[] _cells;
        private TMP_Text[] _texts;
        private Image[] _icons;

        public event Action<int> CellClicked;

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

            int count = columns * rows;
            _cells = new Image[count];
            _texts = new TMP_Text[count];
            _icons = new Image[count];

            Vector2 holder = _holder.sizeDelta;
            float availableWidth = holder.x - 2f * FramePadding;
            float availableHeight = holder.y - 2f * FramePadding;
            float cellSize = Mathf.Floor(Mathf.Min(
                (availableWidth - Gap * (columns - 1)) / columns,
                (availableHeight - Gap * (rows - 1)) / rows));

            float gridWidth = columns * cellSize + (columns - 1) * Gap;
            float gridHeight = rows * cellSize + (rows - 1) * Gap;
            _frame.sizeDelta = new Vector2(gridWidth + 2f * FramePadding, gridHeight + 2f * FramePadding);

            for (int index = 0; index < count; index++)
            {
                CreateCell(index, index % columns, index / columns, cellSize, gridWidth, gridHeight);
            }
        }

        public void Render(MinesBoard board)
        {
            for (int index = 0; index < board.CellCount; index++)
            {
                Sprite icon = null;
                Color color = HiddenColor;
                string text = "";
                Color textColor = Color.clear;

                bool flagged = board.IsFlagged(index);
                bool mine = board.IsMine(index);

                if (board.IsRevealed(index) && !mine)
                {
                    color = OpenColor;
                    int number = board.AdjacentMines(index);
                    if (number > 0)
                    {
                        text = number.ToString();
                        textColor = NumberColors[number];
                    }
                }
                else if (board.Status == MinesStatus.Lost)
                {
                    if (mine && flagged)
                    {
                        icon = _flagSprite;
                    }
                    else if (mine)
                    {
                        icon = _mineSprite;
                        color = index == board.ExplodedIndex ? BoomColor : OpenColor;
                    }
                    else if (flagged)
                    {
                        icon = _flagSprite;   // a flag on a safe cell: it was a mistake
                        color = WrongFlagColor;
                    }
                }
                else if (flagged || (board.Status == MinesStatus.Won && mine))
                {
                    icon = _flagSprite;
                }

                _cells[index].color = color;
                _texts[index].text = text;
                _texts[index].color = textColor;
                _icons[index].sprite = icon;
                _icons[index].enabled = icon != null;
            }
        }

        private void CreateCell(int index, int column, int row, float size, float gridWidth, float gridHeight)
        {
            GameObject cellObject = new GameObject("Cell " + index, typeof(RectTransform), typeof(Image), typeof(Button));
            cellObject.transform.SetParent(_holder, false);
            _cellObjects.Add(cellObject);

            RectTransform rect = cellObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(
                -gridWidth / 2f + column * (size + Gap),
                gridHeight / 2f - row * (size + Gap));

            Image image = cellObject.GetComponent<Image>();
            Button button = cellObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => OnCellClicked(index));

            GameObject textObject = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(cellObject.transform, false);
            Stretch(textObject.GetComponent<RectTransform>(), 0f);

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = size * 0.65f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(cellObject.transform, false);
            Stretch(iconObject.GetComponent<RectTransform>(), size * 0.12f);

            Image icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.enabled = false;

            _cells[index] = image;
            _texts[index] = text;
            _icons[index] = icon;
        }

        private static void Stretch(RectTransform rect, float margin)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, margin);
            rect.offsetMax = new Vector2(-margin, -margin);
        }

        private void OnCellClicked(int index)
        {
            if (CellClicked != null) CellClicked(index);
        }
    }
}
