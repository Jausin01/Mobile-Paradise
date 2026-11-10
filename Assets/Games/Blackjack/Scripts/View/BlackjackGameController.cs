using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Blackjack.Logic;
using UnityEngine;

namespace MobileParadise.Games.Blackjack.View
{
    /// <summary>
    /// Connects the Blackjack rules (BlackjackGame) to the screens.
    /// Flow: start screen -> bet -> Deal -> Hit / Stand / Double -> dealer plays -> result message -> bet again.
    /// The chips are saved when a round ends. When they run out a panel offers to start over.
    /// </summary>
    public class BlackjackGameController : MonoBehaviour
    {
        private const float DealerPauseSeconds = 0.7f;
        private const float BrokePauseSeconds = 1.6f;

        [SerializeField] private BlackjackStartPanel _startPanel;
        [SerializeField] private BlackjackTableView _tableView;
        [SerializeField] private BlackjackHud _hud;
        [SerializeField] private BlackjackResultPanel _resultPanel;

        private BlackjackGame _game;
        private bool _busy;   // the dealer is playing: ignore buttons

        private void Start()
        {
            _hud.Init();
            _hud.BetChanged += OnBetChanged;
            _hud.DealClicked += OnDeal;
            _hud.HitClicked += OnHit;
            _hud.StandClicked += OnStand;
            _hud.DoubleClicked += OnDouble;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            StopAllCoroutines();
            _busy = false;
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(StartNewGame, OnContinue, BlackjackSaveData.Load() != null);
        }

        private void StartNewGame()
        {
            BeginGame(BlackjackGame.StartChips);
        }

        private void OnContinue()
        {
            BlackjackSaveData save = BlackjackSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            BeginGame(save.Chips);
        }

        private void BeginGame(int chips)
        {
            StopAllCoroutines();
            _busy = false;
            _resultPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _game = new BlackjackGame(chips, new System.Random());
            BlackjackSaveData.Save(chips);
            ShowBetting();
        }

        private void ShowBetting()
        {
            _tableView.Render(_game);
            _tableView.SetMessage("bj.place_bet", 0);
            _hud.SetMode(BlackjackHudMode.Betting);
            _hud.SetChips(_game.Chips, _game.Bet);
        }

        // ---- Input ------------------------------------------------------------

        private void OnBetChanged(int delta)
        {
            if (_busy || !_game.ChangeBet(delta)) return;

            _hud.SetChips(_game.Chips, _game.Bet);
        }

        private void OnDeal()
        {
            if (_busy || !_game.Deal()) return;

            AudioManager.PlaySfx("solitaire_draw");
            _tableView.Render(_game);
            _hud.SetChips(_game.Chips, _game.Bet);

            if (_game.State == BlackjackState.Finished)
            {
                // A blackjack on the first cards ends the round at once.
                StartCoroutine(FinishRoutine());
                return;
            }

            _tableView.SetMessage("bj.your_turn", 0);
            _hud.SetMode(BlackjackHudMode.Playing);
            _hud.SetDoubleEnabled(_game.CanDouble);
        }

        private void OnHit()
        {
            if (_busy || !_game.Hit()) return;

            AudioManager.PlaySfx("solitaire_place");
            _tableView.Render(_game);
            AfterPlayerAction();
        }

        private void OnStand()
        {
            if (_busy || !_game.Stand()) return;

            AfterPlayerAction();
        }

        private void OnDouble()
        {
            if (_busy || !_game.DoubleDown()) return;

            AudioManager.PlaySfx("solitaire_place");
            _tableView.Render(_game);
            _hud.SetChips(_game.Chips, _game.Bet);
            AfterPlayerAction();
        }

        // ---- Helpers ----------------------------------------------------------

        // After a Hit / Stand / Double: the round may be over, the dealer may have to play, or the player goes on.
        private void AfterPlayerAction()
        {
            if (_game.State == BlackjackState.PlayerTurn)
            {
                _hud.SetDoubleEnabled(_game.CanDouble);
                return;
            }

            StartCoroutine(FinishRoutine());
        }

        private IEnumerator FinishRoutine()
        {
            _busy = true;
            _hud.SetMode(BlackjackHudMode.Waiting);
            _tableView.SetMessage("", 0);

            if (_game.State == BlackjackState.DealerTurn)
            {
                _tableView.Render(_game);   // turns the dealer's hidden card
                yield return new WaitForSecondsRealtime(DealerPauseSeconds);

                while (_game.DealerPlay())
                {
                    AudioManager.PlaySfx("solitaire_place");
                    _tableView.Render(_game);
                    yield return new WaitForSecondsRealtime(DealerPauseSeconds);
                }
            }

            _tableView.Render(_game);
            ShowOutcome();
            BlackjackSaveData.Save(_game.Chips);
            _hud.SetChips(_game.Chips, _game.Bet);

            if (_game.IsBroke)
            {
                yield return new WaitForSecondsRealtime(BrokePauseSeconds);
                BlackjackSaveData.Clear();
                _busy = false;
                _resultPanel.Show(StartNewGame, ShowStartPanel);
                yield break;
            }

            _busy = false;
            _hud.SetMode(BlackjackHudMode.Betting);
        }

        private void ShowOutcome()
        {
            string key;
            string sound;

            switch (_game.Outcome)
            {
                case BlackjackOutcome.Blackjack: key = "bj.blackjack"; sound = "mines_win"; break;
                case BlackjackOutcome.Win: key = "bj.win"; sound = "solitaire_found"; break;
                case BlackjackOutcome.DealerBust: key = "bj.dealer_bust"; sound = "solitaire_found"; break;
                case BlackjackOutcome.Push: key = "bj.push"; sound = "solitaire_select"; break;
                case BlackjackOutcome.Bust: key = "bj.bust"; sound = "solitaire_wrong"; break;
                default: key = "bj.lose"; sound = "solitaire_wrong"; break;
            }

            AudioManager.PlaySfx(sound);
            _tableView.SetMessage(key, _game.LastChange);
        }
    }
}
