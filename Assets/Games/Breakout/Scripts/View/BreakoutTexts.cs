using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// All texts of the Breakout game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class BreakoutTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.breakout", "Breakout", "Breakout");
            Texts.Set("breakout.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("breakout.easy", "Fácil", "Easy");
            Texts.Set("breakout.medium", "Médio", "Medium");
            Texts.Set("breakout.hard", "Difícil", "Hard");
            Texts.Set("breakout.level", "Fase", "Level");
            Texts.Set("breakout.score", "Pontos", "Score");
            Texts.Set("breakout.lives", "Vidas", "Lives");
            Texts.Set("breakout.pause", "Pausar", "Pause");
            Texts.Set("breakout.new_game", "Novo jogo", "New game");
            Texts.Set("breakout.back", "Voltar", "Back");
            Texts.Set("breakout.hint_launch", "Arraste para mover e toque para lançar", "Drag to move, tap to launch");
            Texts.Set("breakout.hint_paused", "Pausado. Toque para continuar", "Paused. Tap to continue");
            Texts.Set("breakout.game_over", "Fim de jogo", "Game over");
            Texts.Set("breakout.play_again", "Jogar de novo", "Play again");
        }
    }
}
