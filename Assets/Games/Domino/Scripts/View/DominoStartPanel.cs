using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// Start screen: Continue (when a match was saved), New game, or back to the main menu.
    /// </summary>
    public class DominoStartPanel : MonoBehaviour
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
                _continueButton.onClick.AddListener(() => Invoke(_onContinue));
                _newGameButton.onClick.AddListener(() => Invoke(_onNewGame));
                _initialized = true;
            }

            _continueButton.gameObject.SetActive(hasSavedGame);
            LocalizedText.Bind(_continueButton.GetComponentInChildren<TMP_Text>(), "common.continue");
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "domino.new_game");
            LocalizedText.Bind(_menuButton.GetComponentInChildren<TMP_Text>(), "domino.back");
            LocalizedText.Bind(_title, "game.domino");

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Invoke(Action action)
        {
            Hide();
            if (action != null) action();
        }
    }
}
