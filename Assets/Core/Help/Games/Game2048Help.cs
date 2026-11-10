using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class Game2048Help
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Game2048_Game", "game.2048",
                "Deslize o dedo para qualquer lado: todas as peças andam até onde puderem.\n\n" +
                "Quando duas peças com o mesmo número se encontram, elas se juntam numa só com a soma: 2 + 2 = 4, 4 + 4 = 8, e assim por diante.\n\n" +
                "A cada movimento aparece uma peça nova.\n\n" +
                "Objetivo: chegar à peça 2048. Depois disso você pode continuar jogando para fazer mais pontos.\n\n" +
                "O jogo termina quando não há mais movimentos possíveis. O botão Desfazer volta a última jogada.",
                "Swipe your finger in any direction: all the tiles slide as far as they can.\n\n" +
                "When two tiles with the same number meet, they merge into one with the sum: 2 + 2 = 4, 4 + 4 = 8, and so on.\n\n" +
                "A new tile appears after every move.\n\n" +
                "Goal: reach the 2048 tile. After that you can keep playing to score more.\n\n" +
                "The game ends when no more moves are possible. The Undo button takes back the last move.");
        }
    }
}
