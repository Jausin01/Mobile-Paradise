using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class SnakeHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Snake_Game", "game.snake",
                "Deslize o dedo para a direção que a cobrinha deve seguir. Ela anda sozinha, sem parar.\n\n" +
                "Coma as maçãs para crescer e ganhar pontos.\n\n" +
                "O jogo acaba se a cobrinha bater no próprio corpo. Nos níveis Médio e Difícil, bater na parede também acaba o jogo. No Fácil, a cobrinha atravessa a parede e aparece do outro lado.\n\n" +
                "Se a cobrinha encher o tabuleiro inteiro, você ganha! O botão Pausar para o jogo.",
                "Swipe your finger in the direction the snake should go. It keeps moving on its own.\n\n" +
                "Eat the apples to grow and score points.\n\n" +
                "The game ends if the snake hits its own body. On Medium and Hard, hitting a wall also ends the game. On Easy, the snake passes through the wall and comes out on the other side.\n\n" +
                "If the snake fills the whole board, you win! The Pause button stops the game.");
        }
    }
}
