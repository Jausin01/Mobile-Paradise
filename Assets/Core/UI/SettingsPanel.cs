using MobileParadise.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// The gear button and the settings window (language, text size).
    /// Lives on the SettingsOverlay prefab, which SettingsOverlayBootstrap adds to the game
    /// once, so the gear shows in the menu and in every game.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.62f, 0.82f, 0.50f);
        private static readonly Color UnselectedColor = new Color(0.93f, 0.80f, 0.50f);

        // Normal, large, extra large (buttons are sized so these still fit).
        private static readonly float[] FontScales = { 1.0f, 1.25f, 1.5f };

        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _portugueseButton;
        [SerializeField] private Button _englishButton;
        [SerializeField] private Button[] _fontButtons;
        [SerializeField] private TMP_Text _fontText;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _languageText;
        [SerializeField] private TMP_Text _closeText;

        private void Start()
        {
            LocalizedText.Bind(_titleText, "settings.title");
            LocalizedText.Bind(_languageText, "settings.language");
            LocalizedText.Bind(_fontText, "settings.font");
            LocalizedText.Bind(_closeText, "settings.close");

            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _portugueseButton.onClick.AddListener(() => SetLanguage(Texts.Portuguese));
            _englishButton.onClick.AddListener(() => SetLanguage(Texts.English));
            for (int i = 0; i < _fontButtons.Length; i++)
            {
                int index = i;
                _fontButtons[i].onClick.AddListener(() => SetFontScale(index));
            }

            _panel.SetActive(false);
        }

        private void Open()
        {
            RefreshLanguageButtons();
            RefreshFontButtons();
            UiPause.IsOpen = true;
            _panel.SetActive(true);
        }

        private void Close()
        {
            UiPause.IsOpen = false;
            _panel.SetActive(false);
        }

        private void SetLanguage(string code)
        {
            GameContext.Settings.SetLanguage(code);
            RefreshLanguageButtons();
        }

        private void SetFontScale(int index)
        {
            GameContext.Settings.SetFontScale(FontScales[index]);
            RefreshFontButtons();
        }

        private void RefreshLanguageButtons()
        {
            bool portuguese = GameContext.Settings.Language == Texts.Portuguese;
            _portugueseButton.targetGraphic.color = portuguese ? SelectedColor : UnselectedColor;
            _englishButton.targetGraphic.color = portuguese ? UnselectedColor : SelectedColor;
        }

        private void RefreshFontButtons()
        {
            float current = GameContext.Settings.FontScale;
            for (int i = 0; i < _fontButtons.Length; i++)
            {
                bool selected = Mathf.Abs(FontScales[i] - current) < 0.01f;
                _fontButtons[i].targetGraphic.color = selected ? SelectedColor : UnselectedColor;
            }
        }
    }
}
