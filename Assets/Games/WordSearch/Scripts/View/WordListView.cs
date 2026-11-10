using System.Collections.Generic;
using MobileParadise.Games.WordSearch.Logic;
using TMPro;
using UnityEngine;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// The list of words to find, under the grid. Words already found are crossed out and grayed.
    /// Put it on an object that has a GridLayoutGroup.
    /// </summary>
    public class WordListView : MonoBehaviour
    {
        private static readonly Color OpenColor = new Color(0.17f, 0.11f, 0.07f);
        private static readonly Color FoundColor = new Color(0.55f, 0.50f, 0.45f);

        private readonly List<TextMeshProUGUI> _texts = new List<TextMeshProUGUI>();

        public void Render(WordSearchPuzzle puzzle)
        {
            while (_texts.Count < puzzle.WordCount)
            {
                _texts.Add(CreateText());
            }

            for (int i = 0; i < _texts.Count; i++)
            {
                bool used = i < puzzle.WordCount;
                _texts[i].gameObject.SetActive(used);
                if (!used) continue;

                PlacedWord word = puzzle.Word(i);
                _texts[i].text = word.Found ? "<s>" + word.Text + "</s>" : word.Text;
                _texts[i].color = word.Found ? FoundColor : OpenColor;
            }
        }

        private TextMeshProUGUI CreateText()
        {
            GameObject go = new GameObject("Word", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Midline;
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = 24;
            text.fontSizeMax = 44;
            text.raycastTarget = false;
            return text;
        }
    }
}
