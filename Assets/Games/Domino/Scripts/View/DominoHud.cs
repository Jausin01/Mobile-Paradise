using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.UI;
using MobileParadise.Games.Domino.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Domino.View
{
    public enum DominoAction
    {
        None,
        Draw,
        Pass
    }

    /// <summary>
    /// Whose turn / tile counts on top, then the score and the open ends of the chain.
    /// At the bottom: Draw-or-Pass, New game and Back. A Left / Right pair appears when the player must choose a side.
    /// </summary>
    public class DominoHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _endsText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private GameObject _endChoice;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        private string _statusKey = "";
        private DominoAction _action = DominoAction.None;
        private int _computerTiles;
        private int _stock;
        private int _playerScore;
        private int _computerScore;
        private int _leftEnd;
        private int _rightEnd;

        public event Action ActionClicked;
        public event Action NewGameClicked;
        public event Action BackClicked;
        public event Action<DominoEnd> EndChosen;

        public void Init()
        {
            LocalizedText.Bind(_newGameButton.GetComponentInChildren<TMP_Text>(), "domino.new_game");
            LocalizedText.Bind(_backButton.GetComponentInChildren<TMP_Text>(), "domino.back");
            LocalizedText.Bind(_leftButton.GetComponentInChildren<TMP_Text>(), "domino.left");
            LocalizedText.Bind(_rightButton.GetComponentInChildren<TMP_Text>(), "domino.right");

            _actionButton.onClick.AddListener(() => { if (ActionClicked != null) ActionClicked(); });
            _newGameButton.onClick.AddListener(() => { if (NewGameClicked != null) NewGameClicked(); });
            _backButton.onClick.AddListener(() => { if (BackClicked != null) BackClicked(); });
            _leftButton.onClick.AddListener(() => { if (EndChosen != null) EndChosen(DominoEnd.Left); });
            _rightButton.onClick.AddListener(() => { if (EndChosen != null) EndChosen(DominoEnd.Right); });

            ShowEndChoice(false);
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += ShowTexts;
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= ShowTexts;
        }

        public void SetStatus(string key)
        {
            _statusKey = key;
            ShowTexts();
        }

        public void SetCounts(int computerTiles, int stock)
        {
            _computerTiles = computerTiles;
            _stock = stock;
            ShowTexts();
        }

        public void SetScores(int player, int computer)
        {
            _playerScore = player;
            _computerScore = computer;
            ShowTexts();
        }

        public void SetEnds(int left, int right)
        {
            _leftEnd = left;
            _rightEnd = right;
            ShowTexts();
        }

        /// <summary>
        /// The big button shows Draw or Pass when that is the only thing the player can do, and is disabled otherwise.
        /// </summary>
        public void SetAction(DominoAction action)
        {
            _action = action;
            _actionButton.interactable = action != DominoAction.None;
            ShowTexts();
        }

        public void ShowEndChoice(bool visible)
        {
            _endChoice.SetActive(visible);
        }

        private void ShowTexts()
        {
            _statusText.text = _statusKey.Length > 0 ? Texts.Get(_statusKey) : "";
            _countText.text = Texts.Get("domino.computer") + " " + _computerTiles + "   " + Texts.Get("domino.stock") + " " + _stock;
            _scoreText.text = Texts.Get("domino.you") + " " + _playerScore + " x " + _computerScore + " " + Texts.Get("domino.computer");
            _endsText.text = Texts.Get("domino.ends") + ": " + _leftEnd + " | " + _rightEnd;

            string actionKey = _action == DominoAction.Pass ? "domino.pass" : "domino.draw";
            _actionButton.GetComponentInChildren<TMP_Text>().text = Texts.Get(actionKey);
        }
    }
}
