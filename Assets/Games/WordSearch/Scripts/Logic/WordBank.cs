using System;
using System.Collections.Generic;
using MobileParadise.Games.WordSearch.Logic.Categories;

namespace MobileParadise.Games.WordSearch.Logic
{
    /// <summary>
    /// The list of all word categories. To add a theme: write a new class like AnimalsCategory
    /// (one file in the Categories folder) and add its Create() to the list below.
    /// </summary>
    public static class WordBank
    {
        public static readonly WordCategory[] All =
        {
            AnimalsCategory.Create(),
            FruitsCategory.Create(),
            CountriesCategory.Create(),
            FoodCategory.Create(),
            BodyCategory.Create(),
            NatureCategory.Create(),
            HomeCategory.Create()
        };

        /// <summary>
        /// The words to use for a category: -1 means "mixed", every category together.
        /// </summary>
        public static List<string> WordsFor(int categoryIndex, bool portuguese)
        {
            List<string> words = new List<string>();
            if (categoryIndex >= 0 && categoryIndex < All.Length)
            {
                words.AddRange(All[categoryIndex].Words(portuguese));
                return words;
            }

            for (int i = 0; i < All.Length; i++)
            {
                words.AddRange(All[i].Words(portuguese));
            }
            return words;
        }
    }
}
