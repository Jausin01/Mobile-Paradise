using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// All texts of the Snake game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class SnakeTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.snake", "Cobrinha", "Snake");
            Texts.Set("snake.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("snake.easy", "Fácil", "Easy");
            Texts.Set("snake.medium", "Médio", "Medium");
            Texts.Set("snake.hard", "Difícil", "Hard");
            Texts.Set("snake.score", "Pontos", "Score");
            Texts.Set("snake.best", "Recorde", "Best");
            Texts.Set("snake.pause", "Pausar", "Pause");
            Texts.Set("snake.new_game", "Novo jogo", "New game");
            Texts.Set("snake.back", "Voltar", "Back");
            Texts.Set("snake.hint", "Deslize o dedo para começar", "Swipe to start");
            Texts.Set("snake.lose", "Game over", "Game over");
            Texts.Set("snake.win", "Parabéns! Tabuleiro cheio!", "Well done! Board full!");
            Texts.Set("snake.play_again", "Jogar de novo", "Play again");
        }
    }
}
