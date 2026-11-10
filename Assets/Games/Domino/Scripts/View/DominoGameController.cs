using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Core.Localization;
using MobileParadise.Games.Domino.Logic;
using UnityEngine;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// Connects the dominoes rules (DominoRound) and the computer player (DominoAI) to the screens.
    /// Tap a tile to play it (if it fits both ends, then choose Left or Right). With nothing to play, the big
    /// button draws from the stock, or passes when the stock is empty. Scores add up over rounds until New game.
    /// The match is saved after every move.
    /// </summary>
    public class DominoGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            PlayerTurn,
            ComputerTurn,
            Over
        }

        private const float ThinkPauseSeconds = 0.9f;
        private const float DrawPauseSeconds = 0.5f;
        private const float ResultPauseSeconds = 1.2f;

        [SerializeField] private DominoStartPanel _startPanel;
        [SerializeField] private DominoChainView _chainView;
        [SerializeField] private DominoHandView _handView;
        [SerializeField] private DominoHud _hud;
        [SerializeField] private DominoResultPanel _resultPanel;

        private DominoRound _round;
        private Phase _phase;
        private int _playerScore;
        private int _computerScore;
        private int _pendingTile = -1;
        private readonly System.Random _random = new System.Random();

        private void Start()
        {
            _handView.TileClicked += OnTileClicked;

            _hud.Init();
            _hud.ActionClicked += OnAction;
            _hud.EndChosen += OnEndChosen;
            _hud.NewGameClicked += OnNewGame;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            StopAllCoroutines();
            _phase = Phase.Choosing;
            _resultPanel.Hide();
            _hud.ShowEndChoice(false);
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(OnNewGame, OnContinue, DominoSaveData.Load() != null);
        }

        // A new match: scores back to zero.
        private void OnNewGame()
        {
            _playerScore = 0;
            _computerScore = 0;
            StartRound();
        }

        private void StartRound()
        {
            BeginRound(new DominoRound(_random));
        }

        private void OnContinue()
        {
            DominoSaveData save = DominoSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _playerScore = save.PlayerScore;
            _computerScore = save.ComputerScore;
            if (save.HasRound) BeginRound(save.ToRound());
            else StartRound();
        }

        private void BeginRound(DominoRound round)
        {
            StopAllCoroutines();
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _round = round;
            _pendingTile = -1;
            Save();
            NextTurn();
        }

        // Decides what happens now: the round ended, the computer plays, or the player plays.
        private void NextTurn()
        {
            if (_round.IsOver)
            {
                _phase = Phase.Over;
                Refresh();
                EndRound();
                return;
            }

            _phase = _round.Turn == DominoSide.Computer ? Phase.ComputerTurn : Phase.PlayerTurn;
            Refresh();
            if (_phase == Phase.ComputerTurn) StartCoroutine(ComputerRoutine());
        }

        // Adds the points to the winner and saves; the result panel comes after a short pause.
        private void EndRound()
        {
            if (_round.Result == DominoResult.PlayerWins) _playerScore += _round.Points;
            else if (_round.Result == DominoResult.ComputerWins) _computerScore += _round.Points;
            Save();

            StartCoroutine(ResultRoutine());
        }

        private IEnumerator ResultRoutine()
        {
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);

            string titleKey;
            switch (_round.Result)
            {
                case DominoResult.PlayerWins: titleKey = "domino.round_win"; AudioManager.PlaySfx("chess_win"); break;
                case DominoResult.ComputerWins: titleKey = "domino.round_lose"; AudioManager.PlaySfx("chess_lose"); break;
                default: titleKey = "domino.round_tie"; AudioManager.PlaySfx("chess_draw"); break;
            }

            _hud.SetScores(_playerScore, _computerScore);
            _resultPanel.Show(titleKey, DetailText(), StartRound, ShowStartPanel);
        }

        private string DetailText()
        {
            string text = "";
            if (_round.Blocked) text += Texts.Get("domino.blocked") + "\n";
            if (_round.Points > 0) text += Texts.Get("domino.points") + ": " + _round.Points + "\n";
            text += Texts.Get("domino.you") + " " + _playerScore + " x " + _computerScore + " " + Texts.Get("domino.computer");
            return text;
        }

        // ---- Player input -----------------------------------------------------

        private void OnTileClicked(int id)
        {
            if (_phase != Phase.PlayerTurn) return;

            if (_pendingTile == id)
            {
                _pendingTile = -1;   // tapping the chosen tile again cancels
                Refresh();
                return;
            }

            bool left = _round.CanPlace(id, DominoEnd.Left);
            bool right = _round.CanPlace(id, DominoEnd.Right);
            if (!left && !right)
            {
                AudioManager.PlaySfx("solitaire_wrong");
                _pendingTile = -1;
                Refresh();
                return;
            }

            // It fits both ends (with different numbers): ask which side.
            if (left && right && _round.LeftValue != _round.RightValue)
            {
                _pendingTile = id;
                AudioManager.PlaySfx("chess_select");
                Refresh();
                return;
            }

            PlayerPlay(id, left ? DominoEnd.Left : DominoEnd.Right);
        }

        private void OnEndChosen(DominoEnd end)
        {
            if (_phase != Phase.PlayerTurn || _pendingTile < 0) return;

            int id = _pendingTile;
            if (_round.CanPlace(id, end)) PlayerPlay(id, end);
        }

        private void PlayerPlay(int id, DominoEnd end)
        {
            _pendingTile = -1;
            if (!_round.Play(DominoSide.Player, id, end)) return;

            AudioManager.PlaySfx("chess_move");
            Save();
            NextTurn();
        }

        // The big button: draw a tile, or pass when the stock is empty.
        private void OnAction()
        {
            if (_phase != Phase.PlayerTurn) return;

            if (_round.CanDraw(DominoSide.Player))
            {
                _round.Draw(DominoSide.Player);
                AudioManager.PlaySfx("solitaire_draw");
                Save();
                Refresh();
            }
            else if (_round.Pass(DominoSide.Player))
            {
                AudioManager.PlaySfx("solitaire_select");
                Save();
                NextTurn();
            }
        }

        // ---- Computer ---------------------------------------------------------

        private IEnumerator ComputerRoutine()
        {
            yield return new WaitForSecondsRealtime(ThinkPauseSeconds);

            while (!_round.IsOver)
            {
                DominoMove move = DominoAI.Choose(_round, DominoSide.Computer, _random);
                if (move != null)
                {
                    _round.Play(DominoSide.Computer, move.Id, move.End);
                    AudioManager.PlaySfx("chess_move");
                    break;
                }

                if (_round.CanDraw(DominoSide.Computer))
                {
                    _round.Draw(DominoSide.Computer);
                    AudioManager.PlaySfx("solitaire_draw");
                    Refresh();
                    yield return new WaitForSecondsRealtime(DrawPauseSeconds);
                    continue;
                }

                _round.Pass(DominoSide.Computer);
                break;
            }

            Save();
            NextTurn();
        }

        // ---- Helpers ----------------------------------------------------------

        private void Save()
        {
            DominoSaveData.Save(_round, _playerScore, _computerScore);
        }

        private void Refresh()
        {
            bool playerTurn = _phase == Phase.PlayerTurn;
            _chainView.Render(_round.Chain, _round.LastPlacedId);
            _handView.Render(_round.Hand(DominoSide.Player), _round, playerTurn, _pendingTile);

            _hud.SetCounts(_round.HandCount(DominoSide.Computer), _round.StockCount);
            _hud.SetScores(_playerScore, _computerScore);
            _hud.SetEnds(_round.LeftValue, _round.RightValue);
            _hud.ShowEndChoice(playerTurn && _pendingTile >= 0);

            DominoAction action = DominoAction.None;
            string status = "";
            if (playerTurn)
            {
                if (_pendingTile >= 0)
                {
                    status = "domino.choose_end";
                }
                else if (_round.CanDraw(DominoSide.Player))
                {
                    status = "domino.must_draw";
                    action = DominoAction.Draw;
                }
                else if (_round.MustPass(DominoSide.Player))
                {
                    status = "domino.must_pass";
                    action = DominoAction.Pass;
                }
                else
                {
                    status = "domino.your_turn";
                }
            }
            else if (_phase == Phase.ComputerTurn)
            {
                status = "domino.thinking";
            }

            _hud.SetStatus(status);
            _hud.SetAction(action);
        }
    }
}
