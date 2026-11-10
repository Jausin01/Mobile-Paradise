using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class BreakoutHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Breakout_Game", "game.breakout",
                "Quebre todos os blocos com a bola.\n\n" +
                "Arraste o dedo para mover a raquete embaixo. Toque na tela para lançar a bola.\n\n" +
                "A bola bate na raquete e volta. Quanto mais perto da ponta da raquete ela bater, mais de lado ela vai.\n\n" +
                "Se a bola cair, você perde uma vida. Quando as vidas acabam, o jogo termina. Quebrando todos os blocos, você passa para a próxima fase.\n\n" +
                "O botão Pausar para o jogo.",
                "Break all the bricks with the ball.\n\n" +
                "Drag your finger to move the paddle at the bottom. Tap the screen to launch the ball.\n\n" +
                "The ball bounces off the paddle. The closer to the edge of the paddle it hits, the more sideways it goes.\n\n" +
                "If the ball falls, you lose a life. When the lives run out, the game is over. Break all the bricks to go to the next level.\n\n" +
                "The Pause button stops the game.");
        }
    }
}
