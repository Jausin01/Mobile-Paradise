using UnityEngine;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// The player's chosen piece set and board style, remembered between sessions.
    /// Stored as indexes (into the piece set list and BoardTheme.All).
    /// </summary>
    public static class ChessAppearance
    {
        private const string PieceKey = "chess.piece_set";
        private const string BoardKey = "chess.board_theme";
        private const string ColorKey = "chess.player_color";   // 0 = White, 1 = Black

        public static int GetPieceSet(int count)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(PieceKey, 0), 0, count - 1);
        }

        public static int GetBoardTheme()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(BoardKey, 0), 0, BoardTheme.All.Length - 1);
        }

        public static int GetPlayerColor()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(ColorKey, 0), 0, 1);
        }

        public static void Save(int pieceSet, int boardTheme, int playerColor)
        {
            PlayerPrefs.SetInt(ColorKey, playerColor);
            PlayerPrefs.SetInt(PieceKey, pieceSet);
            PlayerPrefs.SetInt(BoardKey, boardTheme);
            PlayerPrefs.Save();
        }
    }
}
