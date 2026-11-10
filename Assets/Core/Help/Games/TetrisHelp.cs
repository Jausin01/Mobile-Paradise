using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class TetrisHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Tetris_Game", "game.tetris",
                "Peças de 4 quadradinhos caem de cima. Encaixe as peças para completar linhas inteiras: cada linha completa some e dá pontos.\n\n" +
                "Controles:\n" +
                "Esquerda e Direita: movem a peça (segure para continuar andando).\n" +
                "Girar: vira a peça.\n" +
                "Descer: faz a peça cair mais rápido (segure).\n\n" +
                "Você também pode usar o dedo no tabuleiro: arraste para os lados para mover, arraste para baixo para descer e toque para girar.\n\n" +
                "A peça seguinte aparece no quadro Próxima. No nível Fácil, uma sombra mostra onde a peça vai cair.\n\n" +
                "Fazer várias linhas de uma vez dá muito mais pontos! A cada 10 linhas o jogo fica mais rápido. O jogo termina quando as peças chegam ao topo.\n\n" +
                "Pausar guarda o jogo: você pode continuar depois.",
                "Pieces made of 4 squares fall from the top. Fit them together to complete whole lines: each complete line disappears and gives points.\n\n" +
                "Controls:\n" +
                "Left and Right: move the piece (hold to keep moving).\n" +
                "Rotate: turns the piece.\n" +
                "Down: makes the piece fall faster (hold).\n\n" +
                "You can also use your finger on the board: drag sideways to move, drag down to drop and tap to rotate.\n\n" +
                "The next piece is shown in the Next box. On Easy, a shadow shows where the piece will land.\n\n" +
                "Clearing several lines at once gives many more points! Every 10 lines the game gets faster. The game ends when the pieces reach the top.\n\n" +
                "Pause saves the game: you can continue later.");
        }
    }
}
