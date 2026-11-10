using System.Text.RegularExpressions;
using MobileParadise.Core.UI;
using MobileParadise.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Window that creates a new game: folders, scene with a back button,
    /// build settings entry and catalog entry.
    /// </summary>
    public class GameScaffolder : EditorWindow
    {
        private static readonly string[] SubFolders = { "Scripts", "Scenes", "Prefabs", "Art", "Audio" };

        private string _gameName = "Memory";

        [MenuItem("Mobile Paradise/2. New Game...")]
        public static void Open()
        {
            GetWindow<GameScaffolder>("New Game");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Game name (PascalCase, English)", EditorStyles.boldLabel);
            _gameName = EditorGUILayout.TextField(_gameName);
            EditorGUILayout.Space();

            if (GUILayout.Button("Create", GUILayout.Height(30)))
            {
                Create(_gameName.Trim());
            }
        }

        private static void Create(string gameName)
        {
            if (!Regex.IsMatch(gameName, "^[A-Z][A-Za-z0-9]*$"))
            {
                EditorUtility.DisplayDialog("Invalid name", "Use PascalCase letters/numbers only, e.g. Memory or WordSearch.", "OK");
                return;
            }

            string root = "Assets/Games/" + gameName;
            string sceneName = gameName + "_Game";
            string scenePath = root + "/Scenes/" + sceneName + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
            {
                EditorUtility.DisplayDialog("Already exists", "Scene " + sceneName + " already exists.", "OK");
                return;
            }

            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();

            foreach (string sub in SubFolders)
            {
                EditorProjectUtil.EnsureFolder(root + "/" + sub);
            }

            BuildScene(gameName, scenePath);

            EditorProjectUtil.AddSceneToBuild(scenePath, false);
            AddToCatalog(gameName, sceneName);
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: created game '" + gameName + "' (scene " + sceneName + ").");
        }

        private static void BuildScene(string gameName, string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform safeArea = EditorUiFactory.CreateSafeArea(canvas);

            TextMeshProUGUI title = EditorUiFactory.CreateText(safeArea, "Title", SplitWords(gameName), 80, EditorUiFactory.TextLight);
            LocalizedText.Bind(title, "game." + gameName.ToLowerInvariant());
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.offsetMin = new Vector2(60, -260);
            titleRect.offsetMax = new Vector2(-240, -60);   // leaves room for the settings gear

            Button back = EditorUiFactory.CreateButton(safeArea, "BackButton", "Back to menu", 200);
            LocalizedText.Bind(back.GetComponentInChildren<TMP_Text>(), "common.back_to_menu");
            RectTransform backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(1, 0);
            backRect.pivot = new Vector2(0.5f, 0);
            backRect.offsetMin = new Vector2(60, 60);
            backRect.offsetMax = new Vector2(-60, 260);
            back.gameObject.AddComponent<BackToMenuButton>();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void AddToCatalog(string gameName, string sceneName)
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == sceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = SplitWords(gameName);
            entry.SceneName = sceneName;
            entry.TitleKey = "game." + gameName.ToLowerInvariant();
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }

        private static string SplitWords(string pascal)
        {
            return Regex.Replace(pascal, "(?<!^)([A-Z])", " $1");
        }
    }
}
