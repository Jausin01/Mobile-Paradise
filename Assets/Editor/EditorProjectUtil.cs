using System.Collections.Generic;
using System.IO;
using MobileParadise.Menu;
using UnityEditor;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Asset/folder/build-settings helpers shared by the editor tools.
    /// </summary>
    public static class EditorProjectUtil
    {
        public const string CatalogPath = "Assets/Menu/GameCatalog.asset";

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static GameCatalog LoadOrCreateCatalog()
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog != null) return catalog;

            EnsureFolder("Assets/Menu");
            catalog = UnityEngine.ScriptableObject.CreateInstance<GameCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }

        public static void AddSceneToBuild(string scenePath, bool makeFirst)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == scenePath);

            EditorBuildSettingsScene entry = new EditorBuildSettingsScene(scenePath, true);
            if (makeFirst) scenes.Insert(0, entry);
            else scenes.Add(entry);

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static void RemoveSceneFromBuild(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == scenePath);
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
