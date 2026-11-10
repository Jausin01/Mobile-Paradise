using MobileParadise.Core.Localization;
using MobileParadise.Games.WordSearch.Logic;
using UnityEngine;

namespace MobileParadise.Games.WordSearch.View
{
    /// <summary>
    /// All texts of the Word Search game, Portuguese and English. Registered once at startup.
    /// The theme names come from the word categories, so a new category needs no change here.
    /// </summary>
    public static class WordSearchTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.wordsearch", "Caça-palavras", "Word Search");
            Texts.Set("ws.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("ws.easy", "Fácil", "Easy");
            Texts.Set("ws.medium", "Médio", "Medium");
            Texts.Set("ws.hard", "Difícil", "Hard");
            Texts.Set("ws.theme", "Tema", "Theme");
            Texts.Set("ws.cat.mix", "Todos", "All");
            Texts.Set("ws.found", "Achadas", "Found");
            Texts.Set("ws.hint", "Dica", "Hint");
            Texts.Set("ws.new_game", "Novo jogo", "New game");
            Texts.Set("ws.back", "Voltar", "Back");
            Texts.Set("ws.win", "Parabéns! Achou todas!", "Great! You found them all!");
            Texts.Set("ws.play_again", "Jogar de novo", "Play again");

            for (int i = 0; i < WordBank.All.Length; i++)
            {
                WordCategory category = WordBank.All[i];
                Texts.Set("ws.cat." + category.Key, category.NamePortuguese, category.NameEnglish);
            }
        }

        /// <summary>
        /// The name of a theme in the current language. -1 is "all themes mixed".
        /// </summary>
        public static string CategoryName(int categoryIndex)
        {
            if (categoryIndex < 0 || categoryIndex >= WordBank.All.Length) return Texts.Get("ws.cat.mix");
            return Texts.Get("ws.cat." + WordBank.All[categoryIndex].Key);
        }
    }
}
