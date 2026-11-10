using System.Collections.Generic;
using MobileParadise.Core.Help;
using MobileParadise.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// The "?" button and the how-to-play window. Lives on the SettingsOverlay prefab, so it shows everywhere:
    /// inside a game it opens that game's explanation; in the main menu it shows a button for each game.
    /// </summary>
    public class HelpPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _listButton;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private TMP_Text _closeText;
        [SerializeField] private TMP_Text _listButtonText;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Button _gameButtonTemplate;

        private readonly List<GameObject> _gameButtons = new List<GameObject>();
        private HelpEntry _current;      // null = showing the list of games
        private bool _fromList;

        private void Start()
        {
            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _listButton.onClick.AddListener(ShowList);
            _gameButtonTemplate.gameObject.SetActive(false);
            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += Render;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= Render;
        }

        // ---- Opening and closing ----------------------------------------------

        private void Open()
        {
            UiPause.IsOpen = true;
            _panel.SetActive(true);

            HelpEntry entry = HelpRegistry.Find(SceneManager.GetActiveScene().name);
            if (entry != null) ShowEntry(entry, false);
            else ShowList();
        }

        private void Close()
        {
            UiPause.IsOpen = false;
            _panel.SetActive(false);
        }

        // ---- Screens ----------------------------------------------------------

        private void ShowList()
        {
            _current = null;
            _fromList = false;
            Render();
        }

        private void ShowEntry(HelpEntry entry, bool fromList)
        {
            _current = entry;
            _fromList = fromList;
            Render();
        }

        private void Render()
        {
            if (!_panel.activeSelf) return;

            LocalizedText.Bind(_closeText, "settings.close");
            LocalizedText.Bind(_listButtonText, "help.list");

            bool showingList = _current == null;
            _titleText.text = Texts.Get(showingList ? "help.title" : _current.NameKey);
            _bodyText.gameObject.SetActive(!showingList);
            _listButton.gameObject.SetActive(!showingList && _fromList);

            ClearGameButtons();
            if (showingList) CreateGameButtons();
            else _bodyText.text = Texts.Get(_current.TextKey);

            Canvas.ForceUpdateCanvases();
            _scroll.verticalNormalizedPosition = 1f;
        }

        private void CreateGameButtons()
        {
            List<HelpEntry> entries = HelpRegistry.Sorted();
            for (int i = 0; i < entries.Count; i++)
            {
                HelpEntry entry = entries[i];

                GameObject go = Instantiate(_gameButtonTemplate.gameObject, _content);
                go.SetActive(true);
                go.GetComponentInChildren<TMP_Text>().text = Texts.Get(entry.NameKey);
                go.GetComponent<Button>().onClick.AddListener(() => ShowEntry(entry, true));
                _gameButtons.Add(go);
            }
        }

        private void ClearGameButtons()
        {
            for (int i = 0; i < _gameButtons.Count; i++)
            {
                _gameButtons[i].SetActive(false);   // leaves the layout right away; Destroy happens at the end of the frame
                Destroy(_gameButtons[i]);
            }
            _gameButtons.Clear();
        }
    }
}
