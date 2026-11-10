using System.Collections.Generic;
using MobileParadise.Games.Domino.Logic;
using UnityEngine;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// Draws the chain of played tiles in a snake: 7 tiles per row, the first row left to right, the next right to left, and so on.
    /// The tile played last has a yellow border.
    /// </summary>
    public class DominoChainView : MonoBehaviour
    {
        private const int Columns = 7;
        private const int Rows = 4;
        private const float TileWidth = 130f;
        private const float TileHeight = 65f;
        private const float GapX = 10f;
        private const float GapY = 14f;

        [SerializeField] private RectTransform _holder;

        private readonly List<GameObject> _tiles = new List<GameObject>();

        public void Render(IList<PlacedTile> chain, int lastPlacedId)
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                Destroy(_tiles[i]);
            }
            _tiles.Clear();

            float gridWidth = Columns * TileWidth + (Columns - 1) * GapX;
            float gridHeight = Rows * TileHeight + (Rows - 1) * GapY;

            for (int i = 0; i < chain.Count; i++)
            {
                PlacedTile placed = chain[i];
                int row = i / Columns;
                bool reversed = row % 2 == 1;
                int slot = reversed ? Columns - 1 - i % Columns : i % Columns;

                // On a right-to-left row the tile is shown turned around.
                int leftValue = reversed ? placed.Right : placed.Left;
                int rightValue = reversed ? placed.Left : placed.Right;
                Color border = placed.Id == lastPlacedId ? DominoTileFactory.HighlightColor : DominoTileFactory.BorderColor;

                RectTransform tile = DominoTileFactory.Create(_holder, "Tile " + placed.Id, leftValue, rightValue, false,
                    TileWidth, TileHeight, DominoTileFactory.FaceColor, border);
                tile.anchorMin = new Vector2(0.5f, 0.5f);
                tile.anchorMax = new Vector2(0.5f, 0.5f);
                tile.pivot = new Vector2(0, 1);
                tile.anchoredPosition = new Vector2(-gridWidth / 2f + slot * (TileWidth + GapX), gridHeight / 2f - row * (TileHeight + GapY));
                tile.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                _tiles.Add(tile.gameObject);
            }
        }
    }
}
