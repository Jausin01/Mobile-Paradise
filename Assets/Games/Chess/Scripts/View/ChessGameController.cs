using System.Collections.Generic;
using System.Threading.Tasks;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Chess.AI;
using MobileParadise.Games.Chess.Logic;
using UnityEngine;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// Connects game logic, AI and screens. The human plays White against the AI.
    /// The AI thinks on a background thread (on a copy of the board) so the screen never freezes.
    /// </summary>
    public class ChessGameController : MonoBehaviour
    {
        private enum Phase
        {
            ChoosingDifficulty,
            HumanTurn,
            Promotion,
            AiThinking,
            GameOver
        }

        private const float MinThinkSeconds = 0.5f;   // so instant replies do not feel jumpy

        [SerializeField] private ChessBoardView _boardView;
        [SerializeField] private ChessHud _hud;
        [SerializeField] private PromotionPanel _promotionPanel;
        [SerializeField] private DifficultyPanel _difficultyPanel;

        private readonly List<Move> _selectedMoves = new List<Move>();
        private ChessGame _game;
        private PieceColor _humanColor = PieceColor.White;
        private Difficulty _difficulty;
        private int _pieceSetIndex;
        private int _boardThemeIndex;
        private IChessAI _ai;
        private Task<Move> _aiTask;
        private float _aiStartTime;
        private Phase _phase;
        private int _selected = -1;
        private int _promotionFrom;
        private int _promotionTo;

        private void Awake()
        {
            _game = new ChessGame();
        }

        private void Start()
        {
            _boardView.Build();
            _boardView.SquareClicked += OnSquareClicked;

            _hud.Init();
            _hud.UndoClicked += OnUndoClicked;
            _hud.NewGameClicked += RestartGame;
            _hud.BackClicked += ShowDifficultyPanel;

            ShowDifficultyPanel();
        }

        private void Update()
        {
            if (_phase != Phase.AiThinking || _aiTask == null || !_aiTask.IsCompleted) return;
            if (Time.unscaledTime - _aiStartTime < MinThinkSeconds) return;

            Move move;
            if (_aiTask.IsFaulted)
            {
                Debug.LogError("Chess AI failed: " + _aiTask.Exception);
                move = _game.LegalMoves[0];
            }
            else
            {
                move = _aiTask.Result;
            }

            _aiTask = null;
            int piecesBefore = CountPieces(_game.Board);
            _game.Play(move);
            PlayMoveSound(CountPieces(_game.Board) < piecesBefore);
            BeginTurn();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowDifficultyPanel()
        {
            _aiTask = null;   // a search still running is simply ignored when it ends
            _phase = Phase.ChoosingDifficulty;
            _promotionPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _difficultyPanel.Show(OnDifficultyChosen, OnContinue, ChessSaveData.Load() != null);
        }

        private void OnDifficultyChosen(Difficulty difficulty)
        {
            _difficulty = difficulty;
            _ai = AIFactory.Create(difficulty);
            _humanColor = _difficultyPanel.SelectedColor;
            _pieceSetIndex = _difficultyPanel.SelectedPieceIndex;
            _boardThemeIndex = _difficultyPanel.SelectedBoardIndex;
            _boardView.SetAppearance(_difficultyPanel.SelectedPieceSet, _difficultyPanel.SelectedTheme, _humanColor == PieceColor.Black);
            RestartGame();
        }

        /// <summary>
        /// Picks up the saved game: same difficulty, side and look, moves replayed.
        /// </summary>
        private void OnContinue()
        {
            ChessSaveData save = ChessSaveData.Load();
            if (save == null)
            {
                ShowDifficultyPanel();
                return;
            }

            _difficulty = (Difficulty)save.Difficulty;
            _ai = AIFactory.Create(_difficulty);
            _humanColor = save.HumanColor == 0 ? PieceColor.White : PieceColor.Black;
            _pieceSetIndex = save.PieceSet;
            _boardThemeIndex = save.BoardTheme;

            int boardIndex = Mathf.Clamp(_boardThemeIndex, 0, BoardTheme.All.Length - 1);
            _boardView.SetAppearance(_difficultyPanel.PieceSetAt(_pieceSetIndex), BoardTheme.All[boardIndex], _humanColor == PieceColor.Black);

            _aiTask = null;
            _promotionPanel.Hide();
            AudioManager.PlayMusic("music_game");
            _game.LoadMoves(save.Moves);   // a broken save simply gives a fresh game
            ClearSelection();
            BeginTurn();
        }

        /// <summary>
        /// New game with the same difficulty and look.
        /// </summary>
        private void RestartGame()
        {
            _aiTask = null;   // a search still running is simply ignored when it ends
            _promotionPanel.Hide();
            AudioManager.PlayMusic("music_game");
            _game.Reset();
            ClearSelection();
            BeginTurn();
        }

        private void BeginTurn()
        {
            ClearSelection();
            SaveProgress();

            if (_game.IsOver)
            {
                _phase = Phase.GameOver;
                _hud.SetStatus(GetGameOverKey());
            }
            else if (_game.SideToMove == _humanColor)
            {
                _phase = Phase.HumanTurn;
                _hud.SetStatus(_game.Status == GameStatus.Check ? "chess.check" : "chess.your_turn");
            }
            else
            {
                StartAiTurn();
            }

            Refresh();
        }

        /// <summary>
        /// Saves the game after every move; a finished game or an untouched board is not worth continuing.
        /// </summary>
        private void SaveProgress()
        {
            if (_game.IsOver || _game.MoveCount == 0)
            {
                ChessSaveData.Clear();
                return;
            }

            ChessSaveData data = new ChessSaveData();
            data.Difficulty = (int)_difficulty;
            data.HumanColor = _humanColor == PieceColor.White ? 0 : 1;
            data.PieceSet = _pieceSetIndex;
            data.BoardTheme = _boardThemeIndex;
            data.Moves = _game.ExportMoves();
            ChessSaveData.Save(data);
        }

        private void StartAiTurn()
        {
            _phase = Phase.AiThinking;
            _hud.SetStatus("chess.thinking");

            Board copy = _game.Board.Clone();
            IChessAI ai = _ai;
            _aiStartTime = Time.unscaledTime;
            _aiTask = Task.Run(() => ai.ChooseMove(copy));
        }

        private string GetGameOverKey()
        {
            switch (_game.Status)
            {
                case GameStatus.Checkmate:
                    return _game.SideToMove == _humanColor ? "chess.lose" : "chess.win";
                case GameStatus.Stalemate:
                    return "chess.stalemate";
                case GameStatus.DrawRepetition:
                    return "chess.draw_repetition";
                default:
                    return "chess.draw";
            }
        }

        // ---- Input ------------------------------------------------------------

        private void OnSquareClicked(int square)
        {
            if (_phase != Phase.HumanTurn) return;

            if (_selected >= 0 && IsTarget(square))
            {
                if (_game.IsPromotionMove(_selected, square))
                {
                    _promotionFrom = _selected;
                    _promotionTo = square;
                    _phase = Phase.Promotion;
                    _promotionPanel.Show(_boardView.PieceSet, _humanColor, OnPromotionChosen);
                }
                else
                {
                    PlayHumanMove(_selected, square, PieceType.None);
                }
                return;
            }

            Piece piece = _game.Board.Get(square);
            if (!piece.IsNone && piece.Color == _humanColor && square != _selected)
            {
                _selected = square;
                AudioManager.PlaySfx("chess_select");
                _selectedMoves.Clear();
                _selectedMoves.AddRange(_game.GetLegalMovesFrom(square));
            }
            else
            {
                ClearSelection();
            }
            Refresh();
        }

        private void OnPromotionChosen(PieceType type)
        {
            PlayHumanMove(_promotionFrom, _promotionTo, type);
        }

        private void PlayHumanMove(int from, int to, PieceType promotion)
        {
            int piecesBefore = CountPieces(_game.Board);
            _game.TryMove(from, to, promotion);
            PlayMoveSound(CountPieces(_game.Board) < piecesBefore);
            BeginTurn();
        }

        private void OnUndoClicked()
        {
            if (_phase != Phase.HumanTurn && _phase != Phase.GameOver) return;

            // Take back the AI's reply and the human's move (or just the human's move
            // if the game ended on it), so it is the human's turn again.
            do
            {
                if (!_game.Undo()) break;
            }
            while (_game.SideToMove != _humanColor);

            BeginTurn();
        }

        // ---- Helpers ----------------------------------------------------------

        // When the human has Black, the AI's opening move cannot be taken back.
        private int FirstHumanMoveIndex
        {
            get { return _humanColor == PieceColor.White ? 0 : 1; }
        }

        private static int CountPieces(Board board)
        {
            int count = 0;
            for (int square = 0; square < 64; square++)
            {
                if (!board.Get(square).IsNone) count++;
            }
            return count;
        }

        /// <summary>
        /// One sound per move: game end, check, capture or a plain move.
        /// </summary>
        private void PlayMoveSound(bool capture)
        {
            switch (_game.Status)
            {
                case GameStatus.Checkmate:
                    AudioManager.PlaySfx(_game.SideToMove == _humanColor ? "chess_lose" : "chess_win");
                    break;
                case GameStatus.Stalemate:
                case GameStatus.DrawFiftyMoves:
                case GameStatus.DrawInsufficientMaterial:
                case GameStatus.DrawRepetition:
                    AudioManager.PlaySfx("chess_draw");
                    break;
                case GameStatus.Check:
                    AudioManager.PlaySfx("chess_check");
                    break;
                default:
                    AudioManager.PlaySfx(capture ? "chess_capture" : "chess_move");
                    break;
            }
        }

        private bool IsTarget(int square)
        {
            for (int i = 0; i < _selectedMoves.Count; i++)
            {
                if (_selectedMoves[i].To == square) return true;
            }
            return false;
        }

        private void ClearSelection()
        {
            _selected = -1;
            _selectedMoves.Clear();
        }

        private void Refresh()
        {
            _boardView.Render(_game, _selected, _selectedMoves);
            _hud.SetUndoEnabled((_phase == Phase.HumanTurn || _phase == Phase.GameOver) && _game.MoveCount > FirstHumanMoveIndex);
        }
    }
}
