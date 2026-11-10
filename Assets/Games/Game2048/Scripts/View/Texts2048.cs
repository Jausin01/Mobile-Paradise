using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// All texts of the 2048 game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class Texts2048
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.2048", "2048", "2048");
            Texts.Set("g2048.ready", "Vamos jogar?", "Ready to play?");
            Texts.Set("g2048.new_game", "Novo jogo", "New game");
            Texts.Set("g2048.score", "Pontos", "Score");
            Texts.Set("g2048.best", "Recorde", "Best");
            Texts.Set("g2048.undo", "Desfazer", "Undo");
            Texts.Set("g2048.back", "Voltar", "Back");
            Texts.Set("g2048.hint", "Deslize o dedo para mover as peças", "Swipe to slide the tiles");
            Texts.Set("g2048.win", "Chegou ao 2048!", "You made 2048!");
            Texts.Set("g2048.keep_playing", "Continuar jogando", "Keep playing");
            Texts.Set("g2048.lose", "Sem movimentos", "No more moves");
            Texts.Set("g2048.play_again", "Jogar de novo", "Play again");
        }
    }
}
