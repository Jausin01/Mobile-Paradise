using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class SudokuHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Sudoku_Game", "game.sudoku",
                "Preencha todos os quadrados com números de 1 a 9.\n\n" +
                "Regra: em cada linha, em cada coluna e em cada bloco de 3x3 quadrados, os números não podem se repetir.\n\n" +
                "Toque num quadrado vazio e depois no número que você quer colocar. O botão Apagar tira o número do quadrado escolhido, e o botão Dica ajuda quando você está travado.\n\n" +
                "Os números que já vêm no tabuleiro não podem ser mudados.",
                "Fill every square with a number from 1 to 9.\n\n" +
                "Rule: in each row, each column and each 3x3 block, no number can repeat.\n\n" +
                "Tap an empty square, then tap the number you want to put in it. The Erase button clears the chosen square, and the Hint button helps when you are stuck.\n\n" +
                "The numbers that come on the board cannot be changed.");
        }
    }
}
