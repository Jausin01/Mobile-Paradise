using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Solitaire.Logic;
using UnityEngine;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// Connects the Solitaire rules (KlondikeGame) to the screens.
    /// Flow: start screen -> playing -> win panel -> (play again | back to the start screen).
    /// Playing: tap a card to pick it up (with the cards on top of it), then tap where it should go.
    /// Tap the picked card again to send it to its foundation. Tap the stock to turn over cards.
    /// </summary>
    public class SolitaireGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            Won
        }

        private const float WinPauseSeconds = 0.8f;

        [SerializeField] private SolitaireStartPanel _startPanel;
        [SerializeField] private SolitaireBoardView _boardView;
        [SerializeField] private SolitaireHud _hud;
        [SerializeField] private SolitaireWinPanel _winPanel;

        private KlondikeGame _game;
        private SolitaireLevel _level;
        private Phase _phase;
        private bool _hasSelection;
        private PileRef _selectedPile;
        private int _selectedIndex;

        private void Start()
        {
            _boardView.Build();
            _boardView.PileClicked += OnPileClicked;

            _hud.Init();
            _hud.UndoClicked += OnUndoClicked;
            _hud.NewGameClicked += StartGame;
            _hud.BackClicked += ShowStartPanel;

            ShowStartPanel();
        }

        // ---- Flow -------------------------------------------------------------

        private void ShowStartPanel()
        {
            StopAllCoroutines();
            _phase = Phase.Choosing;
            _winPanel.Hide();
            AudioManager.PlayMusic("music_menu");
            _startPanel.Show(OnLevelChosen, OnContinue, SolitaireSaveData.Load() != null);
        }

        private void OnLevelChosen(SolitaireLevel level)
        {
            _level = level;
            StartGame();
        }

        private void StartGame()
        {
            BeginGame(new KlondikeGame(SolitaireLevels.DrawCount(_level), SolvableDeals.Pick(SolitaireLevels.DrawCount(_level), new System.Random())));
            SolitaireSaveData.Save(_game, _level);   // a new game replaces the saved one
        }

        /// <summary>
        /// Picks up the saved game with every card where it was.
        /// </summary>
        private void OnContinue()
        {
            SolitaireSaveData save = SolitaireSaveData.Load();
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _level = (SolitaireLevel)save.Level;
            BeginGame(save.ToGame());
        }

        private void BeginGame(KlondikeGame game)
        {
            StopAllCoroutines();
            _winPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _game = game;
            ClearSelection();
            _phase = Phase.Playing;
            _hud.SetLevel(SolitaireStartPanel.LevelKeys[(int)_level]);
            Refresh();
        }

        // ---- Input ------------------------------------------------------------

        private void OnPileClicked(PileRef pile, int index)
        {
            if (_phase != Phase.Playing) return;

            if (pile.Kind == PileKind.Stock)
            {
                ClearSelection();
                DrawFromStock();
                return;
            }

            if (!_hasSelection)
            {
                if (TrySelect(pile, index)) AudioManager.PlaySfx("solitaire_select");
                Refresh();
                return;
            }

            // The picked card tapped again: send it to its foundation if it fits, otherwise put it down.
            if (IsSelected(pile, index))
            {
                PileRef from = _selectedPile;
                int fromIndex = _selectedIndex;
                ClearSelection();

                if (_game.MoveToFoundation(from, fromIndex)) AfterAction("solitaire_found");
                else Refresh();
                return;
            }

            // Another place: try to move the picked cards there.
            if (pile.Kind == PileKind.Tableau || pile.Kind == PileKind.Foundation)
            {
                if (_game.Move(_selectedPile, _selectedIndex, pile))
                {
                    ClearSelection();
                    AfterAction(pile.Kind == PileKind.Foundation ? "solitaire_found" : "solitaire_place");
                    return;
                }
            }

            // It does not fit: pick up the tapped card instead, or just put the picked cards down.
            ClearSelection();
            if (TrySelect(pile, index)) AudioManager.PlaySfx("solitaire_select");
            else AudioManager.PlaySfx("solitaire_wrong");
            Refresh();
        }

        private void OnUndoClicked()
        {
            if (_phase != Phase.Playing || !_game.Undo()) return;

            ClearSelection();
            AudioManager.PlaySfx("solitaire_select");
            Refresh();
            SolitaireSaveData.Save(_game, _level);
        }

        private void DrawFromStock()
        {
            if (_game.Draw() == DrawResult.None) return;

            AfterAction("solitaire_draw");
        }

        // ---- Helpers ----------------------------------------------------------

        private bool TrySelect(PileRef pile, int index)
        {
            switch (pile.Kind)
            {
                case PileKind.Waste:
                    if (_game.WasteCount == 0) return false;
                    Select(pile, 0);
                    return true;

                case PileKind.Foundation:
                    if (_game.FoundationCount(pile.Index) == 0) return false;
                    Select(pile, 0);
                    return true;

                case PileKind.Tableau:
                    if (index < 0 || !_game.IsFaceUp(pile.Index, index)) return false;
                    Select(pile, index);
                    return true;

                default:
                    return false;
            }
        }

        private void Select(PileRef pile, int index)
        {
            _hasSelection = true;
            _selectedPile = pile;
            _selectedIndex = index;
        }

        private bool IsSelected(PileRef pile, int index)
        {
            if (pile.Kind != _selectedPile.Kind || pile.Index != _selectedPile.Index) return false;
            return pile.Kind != PileKind.Tableau || index == _selectedIndex;
        }

        private void ClearSelection()
        {
            _hasSelection = false;
        }

        private void AfterAction(string sound)
        {
            AudioManager.PlaySfx(sound);
            Refresh();

            if (_game.IsWon)
            {
                _phase = Phase.Won;
                SolitaireSaveData.Clear();   // finished: nothing to continue
                StartCoroutine(WinRoutine());
            }
            else
            {
                SolitaireSaveData.Save(_game, _level);
            }
        }

        private IEnumerator WinRoutine()
        {
            yield return new WaitForSecondsRealtime(WinPauseSeconds);
            AudioManager.PlaySfx("solitaire_win");
            _winPanel.Show(StartGame, ShowStartPanel);
        }

        private void Refresh()
        {
            _boardView.Render(_game, _hasSelection, _selectedPile, _selectedIndex);
            _hud.SetMoves(_game.Moves);
            _hud.SetUndoEnabled(_game.CanUndo);
        }
    }
}
