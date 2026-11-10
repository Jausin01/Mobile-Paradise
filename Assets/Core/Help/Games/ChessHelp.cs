using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class ChessHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Chess_Game", "game.chess",
                "Você joga contra o computador.\n\n" +
                "Toque numa peça sua e depois no quadrado para onde quer movê-la.\n\n" +
                "Objetivo: dar xeque-mate, ou seja, atacar o rei do adversário de um jeito que ele não consiga escapar. Se o seu rei estiver sob ataque (xeque), você precisa protegê-lo.\n\n" +
                "Quando um peão chega ao outro lado do tabuleiro, você escolhe em qual peça ele se transforma.\n\n" +
                "O botão Desfazer volta a sua última jogada. Antes de começar você escolhe a dificuldade, se joga de brancas ou de pretas, e o estilo das peças e do tabuleiro.",
                "You play against the computer.\n\n" +
                "Tap one of your pieces, then tap the square you want to move it to.\n\n" +
                "Goal: checkmate, which means attacking the opponent's king so it cannot escape. If your king is under attack (check), you must protect it.\n\n" +
                "When a pawn reaches the other side of the board, you choose which piece it becomes.\n\n" +
                "The Undo button takes back your last move. Before starting you choose the difficulty, whether you play white or black, and the style of the pieces and the board.");
        }
    }
}
