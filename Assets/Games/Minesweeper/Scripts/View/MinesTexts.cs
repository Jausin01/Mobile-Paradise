using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Minesweeper.View
{
    /// <summary>
    /// All texts of the Minesweeper game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class MinesTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.minesweeper", "Campo Minado", "Minesweeper");
            Texts.Set("mines.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("mines.easy", "Fácil", "Easy");
            Texts.Set("mines.medium", "Médio", "Medium");
            Texts.Set("mines.hard", "Difícil", "Hard");
            Texts.Set("mines.open", "Abrir", "Open");
            Texts.Set("mines.flag", "Bandeira", "Flag");
            Texts.Set("mines.left", "Minas", "Mines");
            Texts.Set("mines.new_game", "Novo jogo", "New game");
            Texts.Set("mines.back", "Voltar", "Back");
            Texts.Set("mines.win", "Parabéns!", "Well done!");
            Texts.Set("mines.lose", "Boom! Tente de novo", "Boom! Try again");
            Texts.Set("mines.play_again", "Jogar de novo", "Play again");
        }
    }
}
