using System;
using MobileParadise.Core.UI;
using MobileParadise.Games.Chess.AI;
using MobileParadise.Games.Chess.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// Start screen: pick Easy / Medium / Hard, plus the look of the game
    /// (piece set, board colors) and which side to play. Choices are remembered between sessions.
    /// </summary>
    public class DifficultyPanel : MonoBehaviour
    {
        private static readonly Difficulty[] Levels = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
        private static readonly string[] LevelKeys = { "chess.easy", "chess.medium", "chess.hard" };
        private static readonly string[] PieceSetKeys = { "chess.set.wood", "chess.set.marble" };
        private static readonly string[] ColorKeys = { "chess.color.white", "chess.color.black" };

        private static readonly Color ButtonColor = new Color(0.93f, 0.80f, 0.50f);
        private const float DimFactor = 0.6f;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _continueButton;

        [Header("Look")]
        [SerializeField] private TMP_Text _piecesLabel;
        [SerializeField] private TMP_Text _boardLabel;
        [SerializeField] private PieceSpriteSet[] _pieceSets;
        [SerializeField] private Button[] _pieceButtons;
        [SerializeField] private Image[] _pieceIcons;
        [SerializeField] private Button[] _boardButtons;
        [SerializeField] private Image[] _boardSwatches;   // the light half of each board button
        [SerializeField] private TMP_Text _colorLabel;
        [SerializeField] private Button[] _colorButtons;   // 0 = White, 1 = Black
        [SerializeField] private Image[] _colorIcons;

        private Action<Difficulty> _onChosen;
        private Action _onContinue;
        private bool _initialized;
        private int _pieceIndex;
        private int _boardIndex;
        private int _colorIndex;

        public PieceSpriteSet SelectedPieceSet
        {
            get { return _pieceSets[_pieceIndex]; }
        }

        public BoardTheme SelectedTheme
        {
            get { return BoardTheme.All[_boardIndex]; }
        }

        public int SelectedPieceIndex
        {
            get { return _pieceIndex; }
        }

        public int SelectedBoardIndex
        {
            get { return _boardIndex; }
        }

        public PieceSpriteSet PieceSetAt(int index)
        {
            return _pieceSets[Mathf.Clamp(index, 0, _pieceSets.Length - 1)];
        }

        public PieceColor SelectedColor
        {
            get { return _colorIndex == 0 ? PieceColor.White : PieceColor.Black; }
        }

        public void Show(Action<Difficulty> onChosen, Action onContinue, bool hasSavedGame)
        {
            _onChosen = onChosen;
            _onContinue = onContinue;

            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            _continueButton.gameObject.SetActive(hasSavedGame);
            SetLabel(_continueButton, "common.continue");

            LocalizedText.Bind(_title, "chess.choose_level");
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                SetLabel(_levelButtons[i], LevelKeys[i]);
            }
            SetLabel(_menuButton, "chess.back");

            LocalizedText.Bind(_piecesLabel, "chess.pieces");
            LocalizedText.Bind(_boardLabel, "chess.board");
            for (int i = 0; i < _pieceButtons.Length; i++)
            {
                SetLabel(_pieceButtons[i], PieceSetKeys[i]);
            }

            LocalizedText.Bind(_colorLabel, "chess.play_as");
            for (int i = 0; i < _colorButtons.Length; i++)
            {
                SetLabel(_colorButtons[i], ColorKeys[i]);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Initialize()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int index = i;
                _levelButtons[i].onClick.AddListener(() => OnLevelClicked(index));
            }

            for (int i = 0; i < _pieceButtons.Length; i++)
            {
                int index = i;
                _pieceButtons[i].onClick.AddListener(() => OnPieceSetClicked(index));
                _pieceIcons[i].sprite = _pieceSets[i].Get(PieceType.Knight, PieceColor.White);
            }

            for (int i = 0; i < _boardButtons.Length; i++)
            {
                int index = i;
                _boardButtons[i].onClick.AddListener(() => OnBoardClicked(index));
            }

            for (int i = 0; i < _colorButtons.Length; i++)
            {
                int index = i;
                _colorButtons[i].onClick.AddListener(() => OnColorClicked(index));
            }

            _colorIndex = ChessAppearance.GetPlayerColor();
            _pieceIndex = ChessAppearance.GetPieceSet(_pieceSets.Length);
            _boardIndex = ChessAppearance.GetBoardTheme();
            RefreshSelection();
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
            Action<Difficulty> callback = _onChosen;
            Hide();

            if (callback != null)
            {
                callback(Levels[index]);
            }
        }

        private void OnPieceSetClicked(int index)
        {
            _pieceIndex = index;
            SaveChoices();
            RefreshSelection();
        }

        private void OnColorClicked(int index)
        {
            _colorIndex = index;
            SaveChoices();
            RefreshSelection();
        }

        private void SaveChoices()
        {
            ChessAppearance.Save(_pieceIndex, _boardIndex, _colorIndex);
        }

        private void OnBoardClicked(int index)
        {
            _boardIndex = index;
            SaveChoices();
            RefreshSelection();
        }

        /// <summary>
        /// The chosen option is bright and a bit larger; the others are dimmed.
        /// </summary>
        private void RefreshSelection()
        {
            for (int i = 0; i < _pieceButtons.Length; i++)
            {
                bool selected = i == _pieceIndex;
                _pieceButtons[i].transform.localScale = Scale(selected);
                _pieceButtons[i].GetComponent<Image>().color = Shade(ButtonColor, selected);
            }

            for (int i = 0; i < _colorButtons.Length; i++)
            {
                bool selected = i == _colorIndex;
                _colorButtons[i].transform.localScale = Scale(selected);
                _colorButtons[i].GetComponent<Image>().color = Shade(ButtonColor, selected);
                _colorIcons[i].sprite = SelectedPieceSet.Get(PieceType.King, i == 0 ? PieceColor.White : PieceColor.Black);
            }

            for (int i = 0; i < _boardButtons.Length; i++)
            {
                bool selected = i == _boardIndex;
                _boardButtons[i].transform.localScale = Scale(selected);
                _boardButtons[i].GetComponent<Image>().color = Shade(BoardTheme.All[i].Dark, selected);
                _boardSwatches[i].color = Shade(BoardTheme.All[i].Light, selected);
            }
        }

        private static Vector3 Scale(bool selected)
        {
            return selected ? Vector3.one : new Vector3(0.88f, 0.88f, 1f);
        }

        private static Color Shade(Color color, bool selected)
        {
            if (selected) return color;
            return new Color(color.r * DimFactor, color.g * DimFactor, color.b * DimFactor, 1f);
        }

        private static void SetLabel(Button button, string textKey)
        {
            LocalizedText.Bind(button.GetComponentInChildren<TMP_Text>(), textKey);
        }
    }
}
