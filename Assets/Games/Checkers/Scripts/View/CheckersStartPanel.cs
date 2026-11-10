using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Checkers.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Checkers.View
{
    /// <summary>
    /// Start screen: Continue (when a game was saved), or tap Easy / Medium / Hard to begin, or go back to the main menu.
    /// </summary>
    public class CheckersStartPanel : MonoBehaviour
    {
        private static readonly string[] LevelKeys = { "checkers.easy", "checkers.medium", "checkers.hard" };
        private static readonly CheckersDifficulty[] Levels = { CheckersDifficulty.Easy, CheckersDifficulty.Medium, CheckersDifficulty.Hard };

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        private Action<CheckersDifficulty> _onChosen;
        private Action _onContinue;
        private bool _initialized;

        public void Show(Action<CheckersDifficulty> onChosen, Action onContinue, bool hasSavedGame)
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

            LocalizedText.Bind(_title, "checkers.choose_level");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "checkers.back");
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
            if (callback != null) callback();
        }

        private void OnLevelClicked(int index)
        {
            Action<CheckersDifficulty> callback = _onChosen;
            Hide();
            if (callback != null) callback(Levels[index]);
        }
    }
}
