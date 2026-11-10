using System;
using MobileParadise.Games.Sudoku.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Sudoku.View
{
    /// <summary>
    /// Draws the 9x9 grid: 81 tappable cells with the numbers, and the highlights
    /// (selected cell, its row / column / box, other cells with the same number, wrong numbers).
    /// </summary>
    public class SudokuBoardView : MonoBehaviour
    {
        private const float CellSize = 104f;
        private const float Gap = 4f;
        private const float BoxGap = 8f;   // extra space between the 3x3 boxes

        private static readonly Color CellColor = new Color(0.96f, 0.91f, 0.80f);
        private static readonly Color PeerColor = new Color(0.90f, 0.82f, 0.64f);
        private static readonly Color SameNumberColor = new Color(0.78f, 0.86f, 0.62f);
        private static readonly Color SelectedColor = new Color(0.95f, 0.78f, 0.38f);

        private static readonly Color GivenText = new Color(0.17f, 0.11f, 0.07f);
        private static readonly Color PlayerText = new Color(0.15f, 0.30f, 0.65f);
        private static readonly Color WrongText = new Color(0.82f, 0.15f, 0.15f);

        [SerializeField] private RectTransform _holder;

        private readonly Image[] _cells = new Image[81];
        private readonly TMP_Text[] _texts = new TMP_Text[81];

        public event Action<int> CellClicked;

        public void Build()
        {
            for (int index = 0; index < 81; index++)
            {
                int column = index % 9;
                int row = index / 9;

                GameObject cellObject = new GameObject("Cell " + index, typeof(RectTransform), typeof(Image), typeof(Button));
                cellObject.transform.SetParent(_holder, false);

                RectTransform rect = cellObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(CellSize, CellSize);
                rect.anchoredPosition = new Vector2(
                    column * (CellSize + Gap) + (column / 3) * BoxGap,
                    -(row * (CellSize + Gap) + (row / 3) * BoxGap));

                Image image = cellObject.GetComponent<Image>();
                Button button = cellObject.GetComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;

                int clicked = index;
                button.onClick.AddListener(() => OnCellClicked(clicked));

                GameObject textObject = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(cellObject.transform, false);
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
                text.fontSize = 68;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;

                _cells[index] = image;
                _texts[index] = text;
            }
        }

        public void Render(SudokuGame game, int selected)
        {
            int selectedValue = selected >= 0 ? game.ValueAt(selected) : 0;

            for (int index = 0; index < 81; index++)
            {
                int value = game.ValueAt(index);
                _texts[index].text = value == 0 ? "" : value.ToString();
                _texts[index].color = game.IsGiven(index) ? GivenText : (game.IsWrong(index) ? WrongText : PlayerText);

                Color color = CellColor;
                if (selected >= 0 && IsPeer(selected, index)) color = PeerColor;
                if (selectedValue != 0 && value == selectedValue) color = SameNumberColor;
                if (index == selected) color = SelectedColor;
                _cells[index].color = color;
            }
        }

        private static bool IsPeer(int a, int b)
        {
            return a / 9 == b / 9 || a % 9 == b % 9 || SudokuSolver.BoxOf(a) == SudokuSolver.BoxOf(b);
        }

        private void OnCellClicked(int index)
        {
            if (CellClicked != null) CellClicked(index);
        }
    }
}
