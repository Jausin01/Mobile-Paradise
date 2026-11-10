namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class FruitsCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("fruits", "Frutas", "Fruits",
                new string[]
                {
                    "maçã", "banana", "laranja", "uva", "pera", "manga", "abacaxi", "melancia", "morango", "limão",
                    "mamão", "goiaba", "caju", "kiwi", "cereja", "ameixa", "pêssego", "coco", "abacate", "amora",
                    "acerola", "jabuticaba", "figo", "melão"
                },
                new string[]
                {
                    "apple", "banana", "orange", "grape", "pear", "mango", "pineapple", "watermelon", "strawberry", "lemon",
                    "papaya", "guava", "cherry", "plum", "peach", "coconut", "avocado", "blueberry", "fig", "lime",
                    "melon", "apricot", "raspberry", "kiwi"
                });
        }
    }
}
