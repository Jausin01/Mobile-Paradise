using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using MobileParadise.Games.WordSearch.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// Start screen: Continue (when a puzzle was saved), a Theme button that cycles through the themes
    /// (and "All"), then Easy / Medium / Hard to begin, or back to the main menu.
    /// </summary>
    public class WordSearchStartPanel : MonoBehaviour
    {
        private static readonly string[] LevelKeys = { "ws.easy", "ws.medium", "ws.hard" };
        private static readonly WordSearchLevel[] Levels = { WordSearchLevel.Easy, WordSearchLevel.Medium, WordSearchLevel.Hard };

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _themeButton;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        private Action<WordSearchLevel, int> _onChosen;
        private Action _onContinue;
        private int _category;
        private bool _initialized;

        public void Show(Action<WordSearchLevel, int> onChosen, Action onContinue, bool hasSavedGame, int category)
        {
            _onChosen = onChosen;
            _onContinue = onContinue;
            _category = category;

            if (!_initialized)
            {
                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    int index = i;
                    _levelButtons[i].onClick.AddListener(() => OnLevelClicked(index));
                }
                _continueButton.onClick.AddListener(OnContinueClicked);
                _themeButton.onClick.AddListener(OnThemeClicked);
                _initialized = true;
            }

            _continueButton.gameObject.SetActive(hasSavedGame);
            LocalizedText.Bind(_continueButton.GetComponentInChildren<TMP_Text>(), "common.continue");

            LocalizedText.Bind(_title, "ws.choose_level");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "ws.back");
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                LocalizedText.Bind(_levelButtons[i].GetComponentInChildren<TMP_Text>(), LevelKeys[i]);
            }

            ShowTheme();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTheme;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTheme;
        }

        private void ShowTheme()
        {
            if (_themeButton == null) return;
            _themeButton.GetComponentInChildren<TMP_Text>().text = Texts.Get("ws.theme") + ": " + WordSearchTexts.CategoryName(_category);
        }

        // -1 ("All") -> 0 -> 1 -> ... -> last -> -1
        private void OnThemeClicked()
        {
            _category++;
            if (_category >= WordBank.All.Length) _category = -1;
            ShowTheme();
        }

        private void OnContinueClicked()
        {
            Action callback = _onContinue;
            Hide();
            if (callback != null) callback();
        }

        private void OnLevelClicked(int index)
        {
            Action<WordSearchLevel, int> callback = _onChosen;
            int category = _category;
            Hide();
            if (callback != null) callback(Levels[index], category);
        }
    }
}
