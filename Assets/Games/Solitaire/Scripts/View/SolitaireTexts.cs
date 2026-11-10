using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Solitaire.View
{
    /// <summary>
    /// All texts of the Solitaire game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class SolitaireTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.solitaire", "Paciência", "Solitaire");
            Texts.Set("solitaire.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("solitaire.easy", "Fácil (1 carta)", "Easy (1 card)");
            Texts.Set("solitaire.hard", "Difícil (3 cartas)", "Hard (3 cards)");
            Texts.Set("solitaire.level_easy", "Fácil", "Easy");
            Texts.Set("solitaire.level_hard", "Difícil", "Hard");
            Texts.Set("solitaire.moves", "Jogadas", "Moves");
            Texts.Set("solitaire.undo", "Desfazer", "Undo");
            Texts.Set("solitaire.new_game", "Novo jogo", "New game");
            Texts.Set("solitaire.back", "Voltar", "Back");
            Texts.Set("solitaire.win", "Parabéns!", "Well done!");
            Texts.Set("solitaire.play_again", "Jogar de novo", "Play again");
        }
    }
}
