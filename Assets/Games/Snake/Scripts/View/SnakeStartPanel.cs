using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Snake.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// Start screen: Continue (when a game was saved), or tap Easy / Medium / Hard to begin, or go back to the main menu.
    /// </summary>
    public class SnakeStartPanel : MonoBehaviour
    {
        private static readonly string[] LevelKeys = { "snake.easy", "snake.medium", "snake.hard" };
        private static readonly SnakeLevel[] Levels = { SnakeLevel.Easy, SnakeLevel.Medium, SnakeLevel.Hard };

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        private Action<SnakeLevel> _onChosen;
        private Action _onContinue;
        private bool _initialized;

        public void Show(Action<SnakeLevel> onChosen, Action onContinue, bool hasSavedGame)
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

            LocalizedText.Bind(_title, "snake.choose_level");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "snake.back");
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
            Action<SnakeLevel> callback = _onChosen;
            Hide();
            if (callback != null) callback(Levels[index]);
        }
    }
}
