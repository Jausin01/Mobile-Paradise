using MobileParadise.Games.Chess.View;
using UnityEditor;
using UnityEngine;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Builds one PieceSpriteSet asset per theme folder in Assets/Games/Chess/Art/Pieces
    /// (e.g. Wood, Marble). Each folder holds white_pawn.png ... black_king.png.
    /// A new theme = a new folder with the same 12 file names, then run the command again.
    /// </summary>
    public static class PieceSpriteSetBuilder
    {
        private const string ThemesFolder = "Assets/Games/Chess/Art/Pieces";
        private const string SetsFolder = "Assets/Games/Chess/PieceSets";
        private static readonly string[] Names = { "pawn", "knight", "bishop", "rook", "queen", "king" };

        [MenuItem("Mobile Paradise/Chess/Build Piece Sprite Sets")]
        public static void BuildAll()
        {
            EditorProjectUtil.EnsureFolder(SetsFolder);

            foreach (string themeFolder in AssetDatabase.GetSubFolders(ThemesFolder))
            {
                BuildTheme(themeFolder);
            }

            AssetDatabase.SaveAssets();
        }

        private static void BuildTheme(string themeFolder)
        {
            string theme = System.IO.Path.GetFileName(themeFolder);
            string assetPath = SetsFolder + "/" + theme + ".asset";

            PieceSpriteSet set = AssetDatabase.LoadAssetAtPath<PieceSpriteSet>(assetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<PieceSpriteSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }

            SerializedObject so = new SerializedObject(set);
            Fill(so.FindProperty("_white"), themeFolder, "white");
            Fill(so.FindProperty("_black"), themeFolder, "black");
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(set);
            Debug.Log("Mobile Paradise: piece set '" + theme + "' updated.");
        }

        private static void Fill(SerializedProperty array, string themeFolder, string prefix)
        {
            array.arraySize = Names.Length;
            for (int i = 0; i < Names.Length; i++)
            {
                string path = themeFolder + "/" + prefix + "_" + Names[i] + ".png";

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    PixelArtImporter.ApplySettings(importer);
                    importer.SaveAndReimport();
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    Debug.LogWarning("Missing piece sprite: " + path);
                }
                array.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
            }
        }
    }
}
