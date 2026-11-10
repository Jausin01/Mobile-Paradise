using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class MinesweeperHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Minesweeper_Game", "game.minesweeper",
                "Há minas escondidas no tabuleiro. Abra todos os quadrados que não têm mina.\n\n" +
                "O número num quadrado aberto mostra quantas minas existem nos quadrados vizinhos (em volta, inclusive nas diagonais). Use os números para descobrir onde estão as minas.\n\n" +
                "Escolha o botão Abrir para abrir quadrados, ou o botão Bandeira para marcar onde você acha que tem uma mina. O contador Minas mostra quantas ainda faltam marcar.\n\n" +
                "Se abrir uma mina, você perde. Você ganha quando todos os quadrados seguros estiverem abertos.",
                "There are hidden mines on the board. Open every square that has no mine.\n\n" +
                "The number on an open square tells how many mines are in the neighboring squares (all around it, including diagonals). Use the numbers to work out where the mines are.\n\n" +
                "Choose the Open button to open squares, or the Flag button to mark where you think a mine is. The Mines counter shows how many are still unmarked.\n\n" +
                "If you open a mine, you lose. You win when every safe square is open.");
        }
    }
}
