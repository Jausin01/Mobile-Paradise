using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Blackjack.View
{
    /// <summary>
    /// Start screen: Continue (when chips were saved), New game, or back to the main menu.
    /// </summary>
    public class BlackjackStartPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _menuButton;

        private Action _onNewGame;
        private Action _onContinue;
        private bool _initialized;

        public void Show(Action onNewGame, Action onContinue, bool hasSavedGame)
        {
            _onNewGame = onNewGame;
            _onContinue = onContinue;

            if (!_initialized)
            {
                _newGameButton.onClick.AddListener(() => Choose(_onNewGame));
                _continueButton.onClick.AddListener(() => Choose(_onContinue));
                _initialized = true;
            }

            _continueButton.gameObject.SetActive(hasSavedGame);
            LocalizedText.Bind(_continueButton.GetComponentInChildren<TMP_Text>(), "common.continue");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "bj.new_game");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "bj.back");
            LocalizedText.Bind(_title, "bj.ready");

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Choose(Action callback)
        {
            Hide();
            if (callback != null) callback();
        }
    }
}
