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
    /// Menu command that builds the button prefab, the catalog and the Main_Menu scene.
    /// The game list scrolls, so it still works when there are many games or a short screen.
    /// Safe to run again: it rebuilds the prefab and scene, but keeps the catalog entries.
    /// </summary>
    public static class MainMenuBuilder
    {
        private const string PrefabPath = "Assets/Menu/Prefabs/GameButton.prefab";
        private const string ScenePath = "Assets/Scenes/Main_Menu.unity";

        [MenuItem("Mobile Paradise/1. Build Main Menu")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            EditorProjectUtil.EnsureFolder("Assets/Menu/Prefabs");
            EditorProjectUtil.EnsureFolder("Assets/Scenes");

            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            GameButton prefab = CreateButtonPrefab();

            // Everything below is loaded/created after NewScene (it unloads unreferenced assets).
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform safeArea = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(safeArea);
            Transform list = CreateScrollList(safeArea);
            CreateMenuView(catalog, prefab, list);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, true);
            EditorProjectUtil.RemoveSceneFromBuild("Assets/Scenes/SampleScene.unity");
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Main_Menu built. Use 'Mobile Paradise > 2. New Game...' to add games.");
        }

        private static GameButton CreateButtonPrefab()
        {
            GameObject root = new GameObject("GameButton", typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(ButtonSound), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(GameButton));

            Image background = root.GetComponent<Image>();
            EditorUiFactory.ApplyFrame(background, EditorUiFactory.ButtonColor);
            root.GetComponent<Button>().targetGraphic = background;

            LayoutElement rootLayout = root.GetComponent<LayoutElement>();
            rootLayout.minHeight = 240;
            rootLayout.preferredHeight = 240;

            HorizontalLayoutGroup group = root.GetComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(30, 30, 30, 30);
            group.spacing = 30;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(root.transform, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 180;
            iconLayout.preferredHeight = 180;

            TextMeshProUGUI label = EditorUiFactory.CreateText(root.transform, "Label", "Game", 72);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            SerializedObject so = new SerializedObject(root.GetComponent<GameButton>());
            so.FindProperty("_label").objectReferenceValue = label;
            so.FindProperty("_icon").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameButton>(PrefabPath);
        }

        private static void CreateTitle(Transform parent)
        {
            GameObject titlePanel = EditorUiFactory.CreatePanel(parent, "TitlePanel", EditorUiFactory.WoodColor);
            RectTransform rect = titlePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(60, -440);
            rect.offsetMax = new Vector2(-60, -260);   // clearly below the music note and settings gear

            TextMeshProUGUI title = EditorUiFactory.CreateText(titlePanel.transform, "Title", "Mobile Paradise", 96, EditorUiFactory.TextLight);
            EditorUiFactory.Stretch(title.rectTransform);
            title.rectTransform.offsetMin = new Vector2(30, 10);
            title.rectTransform.offsetMax = new Vector2(-30, -10);
        }

        /// <summary>
        /// Scrolling area under the title; returns the object the game buttons go into.
        /// </summary>
        private static Transform CreateScrollList(Transform parent)
        {
            GameObject scrollObject = EditorUiFactory.CreateUiObject("GameList", parent, typeof(ScrollRect));
            RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(60, 60);
            scrollRect.offsetMax = new Vector2(-60, -480);

            GameObject viewport = EditorUiFactory.CreateUiObject("Viewport", scrollObject.transform, typeof(RectMask2D));
            EditorUiFactory.Stretch(viewport.GetComponent<RectTransform>());

            GameObject content = EditorUiFactory.CreateUiObject("Content", viewport.transform,
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup group = content.GetComponent<VerticalLayoutGroup>();
            group.spacing = 40;
            group.childAlignment = TextAnchor.UpperCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;
            return content.transform;
        }

        private static void CreateMenuView(GameCatalog catalog, GameButton prefab, Transform list)
        {
            GameObject go = new GameObject("MainMenuView");
            MainMenuView view = go.AddComponent<MainMenuView>();

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_catalog").objectReferenceValue = catalog;
            so.FindProperty("_buttonPrefab").objectReferenceValue = prefab;
            so.FindProperty("_buttonsParent").objectReferenceValue = list;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
