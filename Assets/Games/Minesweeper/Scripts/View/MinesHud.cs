using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Minesweeper.View
{
    /// <summary>
    /// Level name and mines left on top; the Open / Flag choice and the New game / Back buttons at the bottom.
    /// Open / Flag decide what a tap on the grid does; the chosen one is shown in green.
    /// </summary>
    public class MinesHud : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.62f, 0.82f, 0.50f);
        private static readonly Color UnselectedColor = new Color(0.93f, 0.80f, 0.50f);

        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _minesText;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _flagButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;

        private int _minesLeft;

        public event Action OpenClicked;
        public event Action FlagClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;

        public void Init()
        {
            LocalizedText.Bind(_openButton.GetComponentInChildren<TMP_Text>(), "mines.open");
            LocalizedText.Bind(_flagButton.GetComponentInChildren<TMP_Text>(), "mines.flag");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "mines.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "mines.back");

            _openButton.onClick.AddListener(() => { if (OpenClicked != null) OpenClicked(); });
            _flagButton.onClick.AddListener(() => { if (FlagClicked != null) FlagClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowMinesLeft;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowMinesLeft;
        }

        public void SetLevel(string textKey)
        {
            LocalizedText.Bind(_levelText, textKey);
        }

        public void SetMinesLeft(int minesLeft)
        {
            _minesLeft = minesLeft;
            ShowMinesLeft();
        }

        public void SetFlagMode(bool flagMode)
        {
            _openButton.targetGraphic.color = flagMode ? UnselectedColor : SelectedColor;
            _flagButton.targetGraphic.color = flagMode ? SelectedColor : UnselectedColor;
        }

        private void ShowMinesLeft()
        {
            _minesText.text = Texts.Get("mines.left") + ": " + _minesLeft;
        }
    }
}
