using System;
using System.Collections.Generic;
using MobileParadise.Core.Save;
using MobileParadise.Games.WordSearch.Logic;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// The puzzle in progress, saved after every word found so "Continue" works even if the app is closed.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class WordSearchSaveData
    {
        private const string Key = "wordsearch_save";

        public bool HasGame;
        public int Level;          // 0 Easy, 1 Medium, 2 Hard
        public int Category;       // index in WordBank.All, or -1 for all themes
        public string Letters;     // size * size capital letters, row by row
        public string[] Words;
        public int[] Starts;
        public int[] Dxs;
        public int[] Dys;
        public bool[] Founds;

        public static WordSearchSaveData Load()
        {
            WordSearchSaveData data = SaveSystem.Load<WordSearchSaveData>(Key);
            return data.HasGame && data.IsValid() ? data : null;
        }

        public static void Save(WordSearchPuzzle puzzle, WordSearchLevel level, int category)
        {
            WordSearchSaveData data = new WordSearchSaveData();
            data.HasGame = true;
            data.Level = (int)level;
            data.Category = category;

            char[] letters = new char[puzzle.CellCount];
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i] = puzzle.Letter(i);
            }
            data.Letters = new string(letters);

            int count = puzzle.WordCount;
            data.Words = new string[count];
            data.Starts = new int[count];
            data.Dxs = new int[count];
            data.Dys = new int[count];
            data.Founds = new bool[count];
            for (int i = 0; i < count; i++)
            {
                PlacedWord word = puzzle.Word(i);
                data.Words[i] = word.Text;
                data.Starts[i] = word.StartCell;
                data.Dxs[i] = word.Dx;
                data.Dys[i] = word.Dy;
                data.Founds[i] = word.Found;
            }
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new WordSearchSaveData());
        }

        public WordSearchPuzzle ToPuzzle()
        {
            List<PlacedWord> words = new List<PlacedWord>();
            for (int i = 0; i < Words.Length; i++)
            {
                PlacedWord word = new PlacedWord();
                word.Text = Words[i];
                word.StartCell = Starts[i];
                word.Dx = Dxs[i];
                word.Dy = Dys[i];
                word.Found = Founds[i];
                words.Add(word);
            }
            return new WordSearchPuzzle(WordSearchLevels.Size((WordSearchLevel)Level), Letters.ToCharArray(), words);
        }

        // Every word must lie inside the grid and match the letters; anything odd means "no saved game".
        private bool IsValid()
        {
            if (Level < 0 || Level > 2) return false;
            if (Category < -1 || Category >= WordBank.All.Length) return false;

            WordSearchLevel level = (WordSearchLevel)Level;
            int size = WordSearchLevels.Size(level);
            if (Letters == null || Letters.Length != size * size) return false;
            for (int i = 0; i < Letters.Length; i++)
            {
                if (Letters[i] < 'A' || Letters[i] > 'Z') return false;
            }

            int count = WordSearchLevels.WordCount(level);
            if (Words == null || Starts == null || Dxs == null || Dys == null || Founds == null) return false;
            if (Words.Length != count || Starts.Length != count || Dxs.Length != count
                || Dys.Length != count || Founds.Length != count) return false;

            for (int i = 0; i < count; i++)
            {
                if (!WordFits(i, size)) return false;
            }
            return true;
        }

        private bool WordFits(int index, int size)
        {
            string text = Words[index];
            int dx = Dxs[index];
            int dy = Dys[index];
            if (string.IsNullOrEmpty(text) || text.Length > size) return false;
            if (dx < -1 || dx > 1 || dy < -1 || dy > 1 || (dx == 0 && dy == 0)) return false;
            if (Starts[index] < 0 || Starts[index] >= size * size) return false;

            int column = Starts[index] % size;
            int row = Starts[index] / size;
            for (int i = 0; i < text.Length; i++)
            {
                int c = column + dx * i;
                int r = row + dy * i;
                if (c < 0 || c >= size || r < 0 || r >= size) return false;
                if (Letters[r * size + c] != text[i]) return false;
            }
            return true;
        }
    }
}
