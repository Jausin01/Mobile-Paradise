using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class CheckersHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Checkers_Game", "game.checkers",
                "Você joga com as peças brancas, de baixo, e começa o jogo. O computador joga com as pretas.\n\n" +
                "Toque numa peça sua e depois no quadrado para onde quer movê-la. As peças andam na diagonal, só para a frente, uma casa por vez.\n\n" +
                "Para capturar, pule por cima de uma peça do adversário que esteja ao lado, caindo na casa vazia logo atrás. A captura é obrigatória, e dá para capturar várias peças seguidas. Quando aparece \"Capture!\", você precisa capturar.\n\n" +
                "Uma peça que chega à última fileira vira dama: ela anda várias casas na diagonal, para frente e para trás.\n\n" +
                "Ganha quem capturar todas as peças do adversário ou deixá-lo sem jogadas. O botão Desfazer volta a sua última jogada.",
                "You play the white pieces, at the bottom, and you move first. The computer plays black.\n\n" +
                "Tap one of your pieces, then tap the square you want to move it to. Pieces move diagonally, forward only, one square at a time.\n\n" +
                "To capture, jump over an opponent's piece next to yours, landing on the empty square right behind it. Capturing is mandatory, and you can capture several pieces in a row. When \"Capture!\" appears, you must capture.\n\n" +
                "A piece that reaches the last row becomes a king: it moves several squares diagonally, forward and backward.\n\n" +
                "You win by capturing all the opponent's pieces or leaving them with no moves. The Undo button takes back your last move.");
        }
    }
}
