using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Solitaire.Logic;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// The game in progress, saved after every action so "Continue" works even if the app is closed.
    /// (The undo history is not saved: after Continue, Undo starts empty.)
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class SolitaireSaveData
    {
        private const string Key = "solitaire_save";

        public bool HasGame;
        public int Level;   // 0 Easy, 1 Hard
        public KlondikeState State;

        public static SolitaireSaveData Load()
        {
            SolitaireSaveData data = SaveSystem.Load<SolitaireSaveData>(Key);
            return data.IsUsable() ? data : null;
        }

        public static void Save(KlondikeGame game, SolitaireLevel level)
        {
            SolitaireSaveData data = new SolitaireSaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.State = game.Capture();
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new SolitaireSaveData());
        }

        public KlondikeGame ToGame()
        {
            return new KlondikeGame(State);
        }

        private bool IsUsable()
        {
            return HasGame && Level >= 0 && Level <= 1 && State != null && State.IsValid();
        }
    }
}
