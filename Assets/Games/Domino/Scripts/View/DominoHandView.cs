using System;
using System.Collections.Generic;
using MobileParadise.Games.Domino.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// The player's tiles, standing up in a grid at the bottom. On the player's turn, tiles that cannot be played are grayed.
    /// The grid has few columns (big tiles) with 7 tiles or fewer, and more when the hand grows after drawing.
    /// The container needs a GridLayoutGroup.
    /// </summary>
    public class DominoHandView : MonoBehaviour
    {
        private const int WideColumns = 7;
        private const int NarrowColumns = 10;
        private const float Spacing = 10f;
        private const float FallbackWidth = 960f;
        private const float TileRatio = 1.9f;   // height / width

        [SerializeField] private RectTransform _container;

        private readonly List<GameObject> _tiles = new List<GameObject>();

        public event Action<int> TileClicked;

        /// <summary>
        /// Shows the hand. 'pendingId' is the tile waiting for the player to choose a side (or -1).
        /// </summary>
        public void Render(IList<int> hand, DominoRound round, bool playerTurn, int pendingId)
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].SetActive(false);   // the grid ignores it right away; Destroy happens at the end of the frame
                Destroy(_tiles[i]);
            }
            _tiles.Clear();

            int columns = hand.Count <= WideColumns ? WideColumns : NarrowColumns;
            float available = _container.rect.width > 0f ? _container.rect.width : FallbackWidth;
            float width = Mathf.Floor((available - (columns - 1) * Spacing) / columns);
            float height = Mathf.Floor(width * TileRatio);

            GridLayoutGroup grid = _container.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(width, height);
            grid.spacing = new Vector2(Spacing, Spacing);

            for (int i = 0; i < hand.Count; i++)
            {
                CreateTile(hand[i], width, height, round, playerTurn, pendingId);
            }
        }

        private void CreateTile(int id, float width, float height, DominoRound round, bool playerTurn, int pendingId)
        {
            bool dimmed = !playerTurn || !round.CanPlay(id);
            Color face = dimmed ? DominoTileFactory.DimmedFaceColor : DominoTileFactory.FaceColor;
            Color border = id == pendingId ? DominoTileFactory.HighlightColor : DominoTileFactory.BorderColor;

            RectTransform tile = DominoTileFactory.Create(_container, "Tile " + id, DominoTiles.Low(id), DominoTiles.High(id),
                true, width, height, face, border);

            Button button = tile.gameObject.AddComponent<Button>();
            button.targetGraphic = tile.GetComponent<Image>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => OnTileClicked(id));
            _tiles.Add(tile.gameObject);
        }

        private void OnTileClicked(int id)
        {
            if (TileClicked != null) TileClicked(id);
        }
    }
}
