using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Blackjack.Logic;

namespace MobileParadise.Games.Blackjack.View
{
    /// <summary>
    /// The chips owned, saved when a round ends so "Continue" keeps them.
    /// A hand in progress is not saved: leaving in the middle of a hand costs nothing.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class BlackjackSaveData
    {
        private const string Key = "blackjack_save";

        public bool HasGame;
        public int Chips;

        public static BlackjackSaveData Load()
        {
            BlackjackSaveData data = SaveSystem.Load<BlackjackSaveData>(Key);
            return (data.HasGame && data.Chips >= BlackjackGame.MinBet) ? data : null;
        }

        public static void Save(int chips)
        {
            BlackjackSaveData data = new BlackjackSaveData();
            data.HasGame = true;
            data.Chips = chips;
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new BlackjackSaveData());
        }
    }
}
