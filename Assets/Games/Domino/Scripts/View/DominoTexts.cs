using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// All texts of the Dominoes game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class DominoTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.domino", "Dominó", "Dominoes");
            Texts.Set("domino.you", "Você", "You");
            Texts.Set("domino.computer", "PC", "CPU");
            Texts.Set("domino.stock", "Monte", "Stock");
            Texts.Set("domino.score", "Placar", "Score");
            Texts.Set("domino.ends", "Pontas", "Ends");
            Texts.Set("domino.draw", "Comprar", "Draw");
            Texts.Set("domino.pass", "Passar", "Pass");
            Texts.Set("domino.left", "Esquerda", "Left");
            Texts.Set("domino.right", "Direita", "Right");
            Texts.Set("domino.your_turn", "Sua vez", "Your turn");
            Texts.Set("domino.thinking", "Pensando...", "Thinking...");
            Texts.Set("domino.must_draw", "Compre uma peça", "Draw a tile");
            Texts.Set("domino.must_pass", "Sem jogada: passe", "No move: pass");
            Texts.Set("domino.choose_end", "Escolha o lado", "Choose a side");
            Texts.Set("domino.new_game", "Novo jogo", "New game");
            Texts.Set("domino.back", "Voltar", "Back");
            Texts.Set("domino.round_win", "Você ganhou a rodada!", "You win the round!");
            Texts.Set("domino.round_lose", "O computador ganhou a rodada", "The computer wins the round");
            Texts.Set("domino.round_tie", "Rodada empatada", "Round tied");
            Texts.Set("domino.blocked", "Jogo trancado", "Blocked game");
            Texts.Set("domino.points", "Pontos", "Points");
            Texts.Set("domino.next_round", "Próxima rodada", "Next round");
        }
    }
}
