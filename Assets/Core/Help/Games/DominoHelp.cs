using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class DominoHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Domino_Game", "game.domino",
                "Você joga contra o computador. Cada um começa com 7 peças, e as outras 14 ficam no monte.\n\n" +
                "Quem tem o maior duplo começa, e essa peça já é colocada na mesa. Depois cada um joga uma peça por vez, encostando um lado dela num número igual de uma das pontas da fila. Os números das pontas aparecem no topo (Pontas).\n\n" +
                "Toque numa peça sua para jogá-la. As peças acinzentadas não servem agora. Se a peça serve nas duas pontas, escolha Esquerda ou Direita.\n\n" +
                "Se você não tem peça para jogar, toque em Comprar para pegar do monte. Quando o monte acabar, toque em Passar.\n\n" +
                "A rodada termina quando alguém fica sem peças: essa pessoa ganha os pontos das peças que sobraram com o adversário. Se ninguém mais puder jogar (jogo trancado), ganha quem tiver menos pontos na mão, e a diferença vale como pontos. O placar soma as rodadas até você tocar em Novo jogo.",
                "You play against the computer. Each of you starts with 7 tiles, and the other 14 stay in the stock.\n\n" +
                "Whoever has the highest double starts, and that tile is placed on the table for them. Then you take turns playing one tile, matching one of its numbers to a number at one end of the line. The numbers at the ends are shown at the top (Ends).\n\n" +
                "Tap one of your tiles to play it. Grayed-out tiles do not fit right now. If a tile fits both ends, choose Left or Right.\n\n" +
                "If you have nothing to play, tap Draw to take from the stock. When the stock is empty, tap Pass.\n\n" +
                "The round ends when someone runs out of tiles: that person wins the points of the tiles left in the opponent's hand. If nobody can play any more (blocked game), whoever has fewer points in hand wins, and the difference counts as points. The score adds up round after round until you tap New game.");
        }
    }
}
