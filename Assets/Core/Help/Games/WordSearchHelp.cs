using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class WordSearchHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("WordSearch_Game", "game.wordsearch",
                "Ache todas as palavras da lista que estão escondidas entre as letras.\n\n" +
                "Para marcar uma palavra, arraste o dedo da primeira até a última letra. Você também pode tocar na primeira letra e depois na última.\n\n" +
                "As palavras podem estar na horizontal, na vertical e, nos níveis Médio e Difícil, na diagonal. No Difícil elas também podem estar de trás para frente.\n\n" +
                "Quando você acerta, a palavra fica colorida e riscada na lista. O botão Dica mostra a primeira letra de uma palavra que falta.\n\n" +
                "Você escolhe o tema das palavras antes de começar. Cada jogo novo é diferente!",
                "Find all the words from the list hidden among the letters.\n\n" +
                "To mark a word, drag your finger from its first letter to its last. You can also tap the first letter and then the last.\n\n" +
                "Words can run horizontally, vertically and, on Medium and Hard, diagonally. On Hard they can also run backwards.\n\n" +
                "When you get it right, the word is colored and crossed out in the list. The Hint button shows the first letter of a word you have not found.\n\n" +
                "You choose the theme of the words before starting. Every new game is different!");
        }
    }
}
