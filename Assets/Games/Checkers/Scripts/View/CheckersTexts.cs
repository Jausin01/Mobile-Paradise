using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Checkers.View
{
    /// <summary>
    /// All texts of the Checkers (Damas) game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class CheckersTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.checkers", "Damas", "Checkers");
            Texts.Set("checkers.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("checkers.easy", "Fácil", "Easy");
            Texts.Set("checkers.medium", "Médio", "Medium");
            Texts.Set("checkers.hard", "Difícil", "Hard");
            Texts.Set("checkers.your_turn", "Sua vez", "Your turn");
            Texts.Set("checkers.must_capture", "Capture!", "Capture!");
            Texts.Set("checkers.thinking", "Pensando...", "Thinking...");
            Texts.Set("checkers.you", "Você", "You");
            Texts.Set("checkers.computer", "PC", "CPU");
            Texts.Set("checkers.undo", "Desfazer", "Undo");
            Texts.Set("checkers.new_game", "Novo jogo", "New game");
            Texts.Set("checkers.back", "Voltar", "Back");
            Texts.Set("checkers.win", "Você ganhou!", "You win!");
            Texts.Set("checkers.lose", "O computador ganhou", "The computer wins");
            Texts.Set("checkers.draw", "Empate", "Draw");
            Texts.Set("checkers.play_again", "Jogar de novo", "Play again");
        }
    }
}
