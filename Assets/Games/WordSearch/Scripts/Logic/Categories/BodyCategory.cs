namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class BodyCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("body", "Corpo humano", "Human body",
                new string[]
                {
                    "cabeça", "olho", "nariz", "boca", "orelha", "mão", "braço", "perna", "pé", "dedo",
                    "joelho", "cotovelo", "ombro", "costas", "peito", "barriga", "cabelo", "dente", "língua", "pescoço",
                    "unha", "pele", "coração", "rosto"
                },
                new string[]
                {
                    "head", "eye", "nose", "mouth", "ear", "hand", "arm", "leg", "foot", "finger",
                    "knee", "elbow", "shoulder", "back", "chest", "belly", "hair", "tooth", "tongue", "neck",
                    "nail", "skin", "heart", "face"
                });
        }
    }
}
