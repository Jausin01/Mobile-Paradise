using System;
using MobileParadise.Core.Save;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// The game in progress, saved after every move so "Continue" works even if the app is closed.
    /// The moves are replayed from the starting position, so the full history (undo) comes back too.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class ChessSaveData
    {
        private const string Key = "chess_save";

        public bool HasGame;
        public int Difficulty;   // 0 Easy, 1 Medium, 2 Hard
        public int HumanColor;   // 0 White, 1 Black
        public int PieceSet;
        public int BoardTheme;
        public int[] Moves;

        public static ChessSaveData Load()
        {
            ChessSaveData data = SaveSystem.Load<ChessSaveData>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(ChessSaveData data)
        {
            data.HasGame = true;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new ChessSaveData());
        }

        private bool IsUsable()
        {
            return HasGame && Moves != null && Moves.Length > 0
                && Difficulty >= 0 && Difficulty <= 2
                && HumanColor >= 0 && HumanColor <= 1
                && PieceSet >= 0 && BoardTheme >= 0;
        }
    }
}
