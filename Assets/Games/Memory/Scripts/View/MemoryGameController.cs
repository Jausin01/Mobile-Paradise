using System.Collections;
using MobileParadise.Core.Audio;
using MobileParadise.Games.Memory.Logic;
using UnityEngine;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// Connects the Memory rules (MemoryBoard) to the screens.
    /// Flow: start screen -> playing -> win panel -> (play again | back to the start screen).
    /// </summary>
    public class MemoryGameController : MonoBehaviour
    {
        private enum Phase
        {
            Choosing,
            Playing,
            ShowingMismatch,   // a wrong pair is face up for a moment: taps are ignored
            Won
        }

        private const float MismatchSeconds = 1.3f;   // long enough to see and remember both cards
        private const float MatchPauseSeconds = 0.35f;
        private const float WinPauseSeconds = 0.7f;

        [SerializeField] private MemoryStartPanel _startPanel;
        [SerializeField] private MemoryBoardView _boardView;
        [SerializeField] private MemoryHud _hud;
        [SerializeField] private MemoryWinPanel _winPanel;

        private MemoryBoard _board;
        private MemoryLevel _level;
        private MemoryCardSet _cardSet;
        private Sprite[] _faceSprites;   // face id -> picture, for the current game
        private int[] _pictureIndexes;   // face id -> picture index inside the card set (what gets saved)
        private int _themeIndex;
        private Phase _phase;

        private void Start()
        {
            _hud.Init();
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
            _startPanel.Show(OnLevelChosen, OnContinue, MemorySaveData.Load(_startPanel.PictureCounts()) != null);
        }

        private void OnLevelChosen(MemoryLevel level)
        {
            _level = level;
            _themeIndex = _startPanel.SelectedThemeIndex;
            _cardSet = _startPanel.SelectedSet;
            StartGame();
        }

        /// <summary>
        /// Picks up the saved game: same cards, matched pairs still turned up, same move count.
        /// </summary>
        private void OnContinue()
        {
            MemorySaveData save = MemorySaveData.Load(_startPanel.PictureCounts());
            if (save == null)
            {
                ShowStartPanel();
                return;
            }

            _level = (MemoryLevel)save.Level;
            _themeIndex = save.Theme;
            _cardSet = _startPanel.SetAt(_themeIndex);

            MemoryBoard board = new MemoryBoard(save.Faces, save.Matched, save.Moves);
            BeginGame(board, (int[])save.Pictures.Clone());
        }

        private void StartGame()
        {
            int pairs = MemoryLevels.PairCount(_level);
            BeginGame(new MemoryBoard(pairs, new System.Random()), PickPictureIndexes(_cardSet, pairs));
        }

        /// <summary>
        /// Puts a board on the screen (new or loaded) and starts playing it.
        /// </summary>
        private void BeginGame(MemoryBoard board, int[] pictureIndexes)
        {
            StopAllCoroutines();
            _winPanel.Hide();
            AudioManager.PlayMusic("music_game");

            _board = board;
            _pictureIndexes = pictureIndexes;
            _faceSprites = new Sprite[pictureIndexes.Length];
            for (int i = 0; i < pictureIndexes.Length; i++)
            {
                _faceSprites[i] = _cardSet.GetFace(pictureIndexes[i]);
            }

            _boardView.Build(MemoryLevels.Columns(_level), MemoryLevels.Rows(_level));
            for (int i = 0; i < _boardView.Cards.Length; i++)
            {
                _boardView.Cards[i].Clicked += OnCardClicked;
                if (_board.StateAt(i) == CardState.Matched)
                {
                    _boardView.Cards[i].ShowMatched(_faceSprites[_board.FaceAt(i)]);
                }
            }

            _hud.SetMoves(_board.Moves);
            _phase = Phase.Playing;
            MemorySaveData.Save(_level, _themeIndex, _board, _pictureIndexes);
        }

        /// <summary>
        /// Picks 'pairs' different pictures from the theme, at random (their indexes inside the card set).
        /// </summary>
        private static int[] PickPictureIndexes(MemoryCardSet set, int pairs)
        {
            System.Random random = new System.Random();
            int[] order = new int[set.Count];
            for (int i = 0; i < order.Length; i++)
            {
                order[i] = i;
            }
            for (int i = order.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }

            int[] picked = new int[pairs];
            for (int i = 0; i < pairs; i++)
            {
                picked[i] = order[i];
            }
            return picked;
        }

        // ---- Playing ----------------------------------------------------------

        private void OnCardClicked(int index)
        {
            if (_phase != Phase.Playing) return;

            int other;
            FlipResult result = _board.Flip(index, out other);
            if (result == FlipResult.Ignored) return;

            _boardView.Cards[index].ShowFace(_faceSprites[_board.FaceAt(index)]);
            AudioManager.PlaySfx("memory_flip");
            _hud.SetMoves(_board.Moves);

            if (_board.IsComplete)
            {
                MemorySaveData.Clear();   // finished: nothing to continue
            }
            else if (result != FlipResult.FirstCard)
            {
                MemorySaveData.Save(_level, _themeIndex, _board, _pictureIndexes);   // a turn is over
            }

            if (result == FlipResult.Match)
            {
                StartCoroutine(MatchRoutine(index, other));
            }
            else if (result == FlipResult.Mismatch)
            {
                StartCoroutine(MismatchRoutine(index, other));
            }
        }

        private IEnumerator MatchRoutine(int a, int b)
        {
            yield return new WaitForSecondsRealtime(MatchPauseSeconds);

            _boardView.Cards[a].SetMatched();
            _boardView.Cards[b].SetMatched();
            AudioManager.PlaySfx("memory_match");

            if (!_board.IsComplete) yield break;

            _phase = Phase.Won;
            yield return new WaitForSecondsRealtime(WinPauseSeconds);
            AudioManager.PlaySfx("memory_win");
            _winPanel.Show(_board.Moves, StartGame, ShowStartPanel);
        }

        private IEnumerator MismatchRoutine(int a, int b)
        {
            _phase = Phase.ShowingMismatch;
            yield return new WaitForSecondsRealtime(MismatchSeconds);

            _boardView.Cards[a].Hide();
            _boardView.Cards[b].Hide();
            _board.HideMismatch();
            _phase = Phase.Playing;
        }
    }
}
