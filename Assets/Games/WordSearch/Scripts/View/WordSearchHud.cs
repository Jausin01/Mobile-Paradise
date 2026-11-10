using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// The theme and the count of words found on top; Hint / New game / Back at the bottom.
    /// </summary>
    public class WordSearchHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _themeText;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _category = -1;
        private int _found;
        private int _total;

        public event Action HintClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_hintButton.GetComponentInChildren<TMP_Text>(), "ws.hint");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "ws.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "ws.back");

            _hintButton.onClick.AddListener(() => { if (HintClicked != null) HintClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTexts;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTexts;
        }

        public void SetCategory(int category)
        {
            _category = category;
            ShowTexts();
        }

        public void SetProgress(int found, int total)
        {
            _found = found;
            _total = total;
            ShowTexts();
        }

        public void SetHintEnabled(bool enabled)
        {
            _hintButton.interactable = enabled;
        }

        private void ShowTexts()
        {
            _themeText.text = WordSearchTexts.CategoryName(_category);
            _countText.text = Texts.Get("ws.found") + " " + _found + "/" + _total;
        }
    }
}
