using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class MemoryHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Memory_Game", "game.memory",
                "Toque numa carta para virá-la e depois em outra.\n\n" +
                "Se as duas figuras forem iguais, o par fica aberto. Se forem diferentes, as cartas viram de volta: tente lembrar onde cada figura estava!\n\n" +
                "Objetivo: achar todos os pares com o menor número de jogadas.\n\n" +
                "O nível Fácil tem poucas cartas e o Difícil tem muitas. Você também pode escolher as figuras (frutas ou animais).",
                "Tap a card to flip it, then tap another one.\n\n" +
                "If the two pictures match, the pair stays open. If not, the cards flip back: try to remember where each picture was!\n\n" +
                "Goal: find all the pairs in as few moves as you can.\n\n" +
                "Easy has few cards and Hard has many. You can also choose the pictures (fruits or animals).");
        }
    }
}
