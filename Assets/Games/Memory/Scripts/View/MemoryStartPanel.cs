using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Memory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// Start screen: choose the card pictures (theme), then tap a level to begin (or go back to the main menu).
    /// </summary>
    public class MemoryStartPanel : MonoBehaviour
    {
        private static readonly MemoryLevel[] Levels = { MemoryLevel.Easy, MemoryLevel.Medium, MemoryLevel.Hard };
        private static readonly string[] LevelKeys = { "memory.easy", "memory.medium", "memory.hard" };
        private static readonly string[] ThemeKeys = { "memory.theme.fruits", "memory.theme.animals" };

        private static readonly Color ButtonColor = new Color(0.93f, 0.80f, 0.50f);
        private const float DimFactor = 0.6f;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private TMP_Text _themeLabel;
        [SerializeField] private MemoryCardSet[] _cardSets;   // same order as ThemeKeys: Fruits, Animals
        [SerializeField] private Button[] _themeButtons;
        [SerializeField] private Image[] _themeIcons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        private Action<MemoryLevel> _onChosen;
        private Action _onContinue;
        private bool _initialized;
        private int _themeIndex;

        public MemoryCardSet SelectedSet
        {
            get { return _cardSets[_themeIndex]; }
        }

        public int ThemeCount
        {
            get { return _cardSets.Length; }
        }

        public int SelectedThemeIndex
        {
            get { return _themeIndex; }
        }

        /// <summary>
        /// How many pictures each card set has (to check a saved game against).
        /// </summary>
        public int[] PictureCounts()
        {
            int[] counts = new int[_cardSets.Length];
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = _cardSets[i].Count;
            }
            return counts;
        }

        public MemoryCardSet SetAt(int index)
        {
            return _cardSets[index];
        }

        public void Show(Action<MemoryLevel> onChosen, Action onContinue, bool hasSavedGame)
        {
            _onChosen = onChosen;
            _onContinue = onContinue;

            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            _continueButton.gameObject.SetActive(hasSavedGame);
            LocalizedText.Bind(_continueButton.GetComponentInChildren<TMP_Text>(), "common.continue");

            LocalizedText.Bind(_title, "memory.choose_level");
            LocalizedText.Bind(_themeLabel, "memory.theme");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "memory.back");
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                LocalizedText.Bind(_levelButtons[i].GetComponentInChildren<TMP_Text>(), LevelKeys[i]);
            }
            for (int i = 0; i < _themeButtons.Length; i++)
            {
                LocalizedText.Bind(_themeButtons[i].GetComponentInChildren<TMP_Text>(), ThemeKeys[i]);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Initialize()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int index = i;
                _levelButtons[i].onClick.AddListener(() => OnLevelClicked(index));
            }

            for (int i = 0; i < _themeButtons.Length; i++)
            {
                int index = i;
                _themeButtons[i].onClick.AddListener(() => OnThemeClicked(index));
                _themeIcons[i].sprite = _cardSets[i].GetFace(0);
            }

            _themeIndex = MemoryPrefs.GetTheme(_cardSets.Length);
            RefreshSelection();
        }

        private void OnContinueClicked()
        {
            Action callback = _onContinue;
            Hide();

            if (callback != null)
            {
                callback();
            }
        }

        private void OnLevelClicked(int index)
        {
            Action<MemoryLevel> callback = _onChosen;
            Hide();

            if (callback != null)
            {
                callback(Levels[index]);
            }
        }

        private void OnThemeClicked(int index)
        {
            _themeIndex = index;
            MemoryPrefs.SaveTheme(index);
            RefreshSelection();
        }

        /// <summary>
        /// The chosen theme is bright and full size; the other is dimmed and smaller.
        /// </summary>
        private void RefreshSelection()
        {
            for (int i = 0; i < _themeButtons.Length; i++)
            {
                bool selected = i == _themeIndex;
                _themeButtons[i].transform.localScale = selected ? Vector3.one : new Vector3(0.88f, 0.88f, 1f);

                Color color = ButtonColor;
                if (!selected) color = new Color(color.r * DimFactor, color.g * DimFactor, color.b * DimFactor, 1f);
                _themeButtons[i].GetComponent<Image>().color = color;
            }
        }
    }
}
