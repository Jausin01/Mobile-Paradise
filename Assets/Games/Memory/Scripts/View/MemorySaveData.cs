using System;
using MobileParadise.Core.Save;
using MobileParadise.Games.Memory.Logic;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// The game in progress, saved after every turn so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class MemorySaveData
    {
        private const string Key = "memory_save";

        public bool HasGame;
        public int Level;        // 0 Easy, 1 Medium, 2 Hard
        public int Theme;        // index of the card set (Fruits, Animals)
        public int Moves;
        public int[] Faces;      // picture id on each card
        public int[] Pictures;   // picture id -> index of the picture inside the card set
        public bool[] Matched;   // per card

        /// <summary>
        /// pictureCounts: how many pictures each card set has (index = theme), to check the save against.
        /// </summary>
        public static MemorySaveData Load(int[] pictureCounts)
        {
            MemorySaveData data = SaveSystem.Load<MemorySaveData>(Key);
            return data.IsUsable(pictureCounts) ? data : null;
        }

        public static void Save(MemoryLevel level, int theme, MemoryBoard board, int[] pictures)
        {
            MemorySaveData data = new MemorySaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.Theme = theme;
            data.Moves = board.Moves;
            data.Faces = board.CopyFaces();
            data.Pictures = (int[])pictures.Clone();
            data.Matched = new bool[board.CardCount];
            for (int i = 0; i < data.Matched.Length; i++)
            {
                data.Matched[i] = board.StateAt(i) == CardState.Matched;
            }
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new MemorySaveData());
        }

        /// <summary>
        /// True when the numbers fit the level and the card set, so nothing can go out of range.
        /// </summary>
        private bool IsUsable(int[] pictureCounts)
        {
            if (!HasGame || Level < 0 || Level > 2 || Theme < 0 || Theme >= pictureCounts.Length) return false;

            MemoryLevel level = (MemoryLevel)Level;
            int cards = MemoryLevels.Columns(level) * MemoryLevels.Rows(level);
            int pairs = cards / 2;

            if (Faces == null || Faces.Length != cards) return false;
            if (Matched == null || Matched.Length != cards) return false;
            if (Pictures == null || Pictures.Length != pairs) return false;

            for (int i = 0; i < cards; i++)
            {
                if (Faces[i] < 0 || Faces[i] >= pairs) return false;
            }
            for (int i = 0; i < pairs; i++)
            {
                if (Pictures[i] < 0 || Pictures[i] >= pictureCounts[Theme]) return false;
            }
            return true;
        }
    }
}
