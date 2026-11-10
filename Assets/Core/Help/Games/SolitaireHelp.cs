using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class SolitaireHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Solitaire_Game", "game.solitaire",
                "Objetivo: levar todas as cartas para as 4 pilhas de cima, uma para cada naipe, em ordem: do Ás até o Rei.\n\n" +
                "Nas colunas de baixo, monte sequências em ordem decrescente alternando as cores: por exemplo, um 6 preto sobre um 7 vermelho. Só um Rei pode ir para uma coluna vazia.\n\n" +
                "Toque numa carta para escolhê-la e depois toque no lugar de destino. Tocar de novo na carta escolhida tenta mandá-la para as pilhas de cima.\n\n" +
                "Toque no monte para comprar cartas: uma de cada vez no Fácil, três no Difícil.\n\n" +
                "Todo jogo novo tem solução. O botão Desfazer volta a última jogada.",
                "Goal: move all the cards to the 4 piles at the top, one for each suit, in order: from Ace up to King.\n\n" +
                "In the columns below, build sequences going down and alternating colors: for example, a black 6 on a red 7. Only a King can go to an empty column.\n\n" +
                "Tap a card to choose it, then tap where it should go. Tapping the chosen card again tries to send it to the piles at the top.\n\n" +
                "Tap the stock pile to draw cards: one at a time on Easy, three on Hard.\n\n" +
                "Every new game can be solved. The Undo button takes back the last move.");
        }
    }
}
