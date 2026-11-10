using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// All texts of the Tetris game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class TetrisTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.tetris", "Tetris", "Tetris");
            Texts.Set("tetris.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("tetris.easy", "Fácil", "Easy");
            Texts.Set("tetris.medium", "Médio", "Medium");
            Texts.Set("tetris.hard", "Difícil", "Hard");
            Texts.Set("tetris.score", "Pontos", "Score");
            Texts.Set("tetris.best", "Recorde", "Best");
            Texts.Set("tetris.lines", "Linhas", "Lines");
            Texts.Set("tetris.level", "Nível", "Level");
            Texts.Set("tetris.next", "Próxima", "Next");
            Texts.Set("tetris.left", "Esquerda", "Left");
            Texts.Set("tetris.right", "Direita", "Right");
            Texts.Set("tetris.rotate", "Girar", "Rotate");
            Texts.Set("tetris.down", "Descer", "Down");
            Texts.Set("tetris.pause", "Pausar", "Pause");
            Texts.Set("tetris.paused", "Pausado", "Paused");
            Texts.Set("tetris.new_game", "Novo jogo", "New game");
            Texts.Set("tetris.back", "Voltar", "Back");
            Texts.Set("tetris.over", "Fim de jogo", "Game over");
            Texts.Set("tetris.play_again", "Jogar de novo", "Play again");
        }
    }
}
