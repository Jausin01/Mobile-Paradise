using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Minesweeper.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Minesweeper.View
{
    /// <summary>
    /// Start screen: Continue (when a game was saved), or tap Easy / Medium / Hard to begin, or go back to the main menu.
    /// </summary>
    public class MinesStartPanel : MonoBehaviour
    {
        public static readonly string[] LevelKeys = { "mines.easy", "mines.medium", "mines.hard" };
        private static readonly MinesLevel[] Levels = { MinesLevel.Easy, MinesLevel.Medium, MinesLevel.Hard };

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        private Action<MinesLevel> _onChosen;
        private Action _onContinue;
        private bool _initialized;

        public void Show(Action<MinesLevel> onChosen, Action onContinue, bool hasSavedGame)
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

            LocalizedText.Bind(_title, "mines.choose_level");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "mines.back");
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                LocalizedText.Bind(_levelButtons[i].GetComponentInChildren<TMP_Text>(), LevelKeys[i]);
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
            Action<MinesLevel> callback = _onChosen;
            Hide();

            if (callback != null)
            {
                callback(Levels[index]);
            }
        }
    }
}
