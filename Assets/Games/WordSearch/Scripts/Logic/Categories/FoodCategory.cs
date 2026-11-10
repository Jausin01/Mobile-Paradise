namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class FoodCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("food", "Comidas", "Food",
                new string[]
                {
                    "arroz", "feijão", "pão", "queijo", "leite", "ovo", "carne", "frango", "massa", "bolo",
                    "sopa", "salada", "pizza", "batata", "tomate", "cenoura", "café", "suco", "doce", "mel",
                    "biscoito", "macarrão", "manteiga", "peixe"
                },
                new string[]
                {
                    "rice", "beans", "bread", "cheese", "milk", "egg", "meat", "chicken", "pasta", "cake",
                    "soup", "salad", "pizza", "potato", "tomato", "carrot", "coffee", "juice", "candy", "honey",
                    "cookie", "butter", "sugar", "cream"
                });
        }
    }
}
