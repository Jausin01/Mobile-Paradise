using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Solitaire.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// Start screen: Continue (when a game was saved), or tap Easy (1 card) / Hard (3 cards) to begin, or go back to the main menu.
    /// </summary>
    public class SolitaireStartPanel : MonoBehaviour
    {
        public static readonly string[] LevelKeys = { "solitaire.level_easy", "solitaire.level_hard" };
        private static readonly string[] ButtonKeys = { "solitaire.easy", "solitaire.hard" };
        private static readonly SolitaireLevel[] Levels = { SolitaireLevel.Easy, SolitaireLevel.Hard };

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        private Action<SolitaireLevel> _onChosen;
        private Action _onContinue;
        private bool _initialized;

        public void Show(Action<SolitaireLevel> onChosen, Action onContinue, bool hasSavedGame)
        {
            _onChosen = onChosen;
            _onContinue = onContinue;

            if (!_initialized)
            {
                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    int index = i;
                    _levelButtons[i].onClick.AddListener(() => OnLevelClicked(index));
                }
                _continueButton.onClick.AddListener(OnContinueClicked);
                _initialized = true;
            }

            _continueButton.gameObject.SetActive(hasSavedGame);
            LocalizedText.Bind(_continueButton.GetComponentInChildren<TMP_Text>(), "common.continue");

            LocalizedText.Bind(_title, "solitaire.choose_level");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "solitaire.back");
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                LocalizedText.Bind(_levelButtons[i].GetComponentInChildren<TMP_Text>(), ButtonKeys[i]);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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
            Action<SolitaireLevel> callback = _onChosen;
            Hide();

            if (callback != null)
            {
                callback(Levels[index]);
            }
        }
    }
}
