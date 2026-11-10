using System.Collections.Generic;
using MobileParadise.Games.Memory.View;
using UnityEditor;
using UnityEngine;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Builds one MemoryCardSet asset per theme folder (Art/Fruits, Art/Animals) from the PNGs inside.
    /// Run again after adding pictures. Existing assets are updated in place.
    /// </summary>
    public static class MemoryCardSetBuilder
    {
        public const string ArtRoot = "Assets/Games/Memory/Art";
        public const string BackPath = ArtRoot + "/card_back.png";
        public const string SetsFolder = "Assets/Games/Memory/CardSets";
        public const string FruitsPath = SetsFolder + "/Fruits.asset";
        public const string AnimalsPath = SetsFolder + "/Animals.asset";

        [MenuItem("Mobile Paradise/Memory/1. Build Card Sets")]
        public static void Build()
        {
            EditorProjectUtil.EnsureFolder(SetsFolder);

            // Order matters: it matches the theme buttons (Fruits, then Animals).
            int fruits = BuildSet("Fruits", FruitsPath);
            int animals = BuildSet("Animals", AnimalsPath);

            ImportAsPixelArt(BackPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Mobile Paradise: card sets built (" + fruits + " fruits, " + animals + " animals).");
        }

        private static int BuildSet(string folderName, string assetPath)
        {
            string folder = ArtRoot + "/" + folderName;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

            List<string> paths = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            }
            paths.Sort();

            List<Sprite> sprites = new List<Sprite>();
            for (int i = 0; i < paths.Count; i++)
            {
                ImportAsPixelArt(paths[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);
                if (sprite != null) sprites.Add(sprite);
            }

            MemoryCardSet set = AssetDatabase.LoadAssetAtPath<MemoryCardSet>(assetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<MemoryCardSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }

            SerializedObject so = new SerializedObject(set);
            SerializedProperty faces = so.FindProperty("_faces");
            faces.arraySize = sprites.Count;
            for (int i = 0; i < sprites.Count; i++)
            {
                faces.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            return sprites.Count;
        }

        private static void ImportAsPixelArt(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            PixelArtImporter.ApplySettings(importer);
            importer.SaveAndReimport();
        }
    }
}
