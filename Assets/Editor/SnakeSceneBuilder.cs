using MobileParadise.Core.UI;
using MobileParadise.Games.Snake.View;
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
    /// Builds the whole Snake_Game scene (start screen, field, buttons, result panel),
    /// wires it to SnakeGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class SnakeSceneBuilder
    {
        private const string SceneName = "Snake_Game";
        private const string ScenePath = "Assets/Games/Snake/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Snake/Art/icon.png";
        private const string HeadPath = "Assets/Games/Snake/Art/head.png";
        private const string BodyPath = "Assets/Games/Snake/Art/body.png";
        private const string TailPath = "Assets/Games/Snake/Art/tail.png";
        private const string CurvePath = "Assets/Games/Snake/Art/curve.png";
        private const string ApplePath = "Assets/Games/Snake/Art/apple.png";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        [MenuItem("Mobile Paradise/Snake/Build Snake Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Snake/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            SnakeBoardView boardView = CreateBoard(root, out SnakeSwipeArea swipeArea);
            SnakeHud hud = CreateHud(root);
            SnakeResultPanel resultPanel = CreateResultPanel(root);
            SnakeStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, boardView, swipeArea, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Snake_Game scene built and added to the main menu.");
        }

        private static void ImportArt()
        {
            string[] paths = { IconPath, HeadPath, BodyPath, TailPath, CurvePath, ApplePath };
            for (int i = 0; i < paths.Length; i++)
            {
                TextureImporter importer = AssetImporter.GetAtPath(paths[i]) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning("Missing art: " + paths[i]);
                    continue;
                }

                PixelArtImporter.ApplySettings(importer);
                importer.SaveAndReimport();
            }
        }

        // ---- Pieces of the scene ----------------------------------------------

        private static void CreateTitle(Transform parent)
        {
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Snake", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stops before the music note and gear buttons (350 px on the right).
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.snake");
        }

        /// <summary>
        /// The board sits between the score panel and the buttons; the holder (fixed size) shrinks to fit.
        /// The whole area also catches the swipes.
        /// </summary>
        private static SnakeBoardView CreateBoard(Transform parent, out SnakeSwipeArea swipeArea)
        {
            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent, typeof(Image), typeof(SnakeSwipeArea), typeof(SnakeBoardView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 330);
            areaRect.offsetMax = new Vector2(0, -390);

            // Invisible, but it receives the drag.
            Image catcher = area.GetComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            catcher.raycastTarget = true;

            GameObject holder = EditorUiFactory.CreateUiObject("BoardHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(984, 1312);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // The frame goes first so it is drawn behind the tiles.
            GameObject frame = EditorUiFactory.CreatePanel(holder.transform, "BoardFrame", EditorUiFactory.WoodColor);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.sizeDelta = new Vector2(984, 1312);
            frame.GetComponent<Image>().raycastTarget = false;

            SnakeBoardView view = area.GetComponent<SnakeBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_frame").objectReferenceValue = frameRect;
            so.FindProperty("_headSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(HeadPath);
            so.FindProperty("_bodySprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(BodyPath);
            so.FindProperty("_tailSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(TailPath);
            so.FindProperty("_curveSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(CurvePath);
            so.FindProperty("_appleSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ApplePath);
            so.ApplyModifiedPropertiesWithoutUndo();

            swipeArea = area.GetComponent<SnakeSwipeArea>();
            return view;
        }

        private static SnakeHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(SnakeHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Score (left) and best (right)
            GameObject panel = EditorUiFactory.CreatePanel(root.transform, "InfoPanel", EditorUiFactory.PanelColor);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 1);
            panelRect.offsetMin = new Vector2(60, -370);
            panelRect.offsetMax = new Vector2(-60, -210);

            HorizontalLayoutGroup infoLayout = panel.AddComponent<HorizontalLayoutGroup>();
            infoLayout.padding = new RectOffset(30, 30, 10, 10);
            infoLayout.spacing = 20;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = true;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = true;

            TextMeshProUGUI score = EditorUiFactory.CreateText(panel.transform, "ScoreText", "", 64);
            TextMeshProUGUI best = EditorUiFactory.CreateText(panel.transform, "BestText", "", 64);

            // Swipe hint above the buttons
            TextMeshProUGUI hint = EditorUiFactory.CreateText(root.transform, "HintText", "", 52, EditorUiFactory.TextLight);
            RectTransform hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0, 0);
            hintRect.anchorMax = new Vector2(1, 0);
            hintRect.pivot = new Vector2(0.5f, 0);
            hintRect.offsetMin = new Vector2(60, 215);
            hintRect.offsetMax = new Vector2(-60, 300);

            // Undo / New game / Back
            GameObject row = EditorUiFactory.CreateUiObject("Buttons", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.offsetMin = new Vector2(40, 60);
            rowRect.offsetMax = new Vector2(-40, 200);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            Button pause = CreateHudButton(row.transform, "PauseButton", EditorUiFactory.EasyColor);
            Button newGame = CreateHudButton(row.transform, "NewGameButton", EditorUiFactory.ButtonColor);
            Button back = CreateHudButton(row.transform, "BackButton", EditorUiFactory.ButtonColor);

            SnakeHud hud = root.GetComponent<SnakeHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_scoreText").objectReferenceValue = score;
            so.FindProperty("_bestText").objectReferenceValue = best;
            so.FindProperty("_hintText").objectReferenceValue = hint;
            so.FindProperty("_pauseButton").objectReferenceValue = pause;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static Button CreateHudButton(Transform parent, string name, Color tint)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, 140, tint);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 56);
            return button;
        }

        private static SnakeResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(SnakeResultPanel));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = DimColor;

            GameObject box = EditorUiFactory.CreatePanel(root.transform, "Box", EditorUiFactory.PanelColor);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(900, 620);
            box.AddComponent<FitInParent>();

            VerticalLayoutGroup layout = box.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 50, 50);
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = EditorUiFactory.CreateText(box.transform, "Title", "", 90);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 160);
            Button again = EditorUiFactory.CreateButton(box.transform, "PlayAgainButton", "", 150, EditorUiFactory.EasyColor);
            Button back = EditorUiFactory.CreateButton(box.transform, "BackButton", "", 130);

            SnakeResultPanel panel = root.GetComponent<SnakeResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static SnakeStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(SnakeStartPanel));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = EditorUiFactory.SceneBackground;

            // Fixed-size content that shrinks to fit the free area (below the settings gear).
            GameObject area = EditorUiFactory.CreateUiObject("Area", root.transform);
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(30, 30);
            areaRect.offsetMax = new Vector2(-30, -200);

            GameObject content = EditorUiFactory.CreateUiObject("Content", area.transform, typeof(VerticalLayoutGroup), typeof(FitInParent));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(960, 1050);
            contentRect.anchoredPosition = Vector2.zero;
            content.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = EditorUiFactory.CreateText(content.transform, "Title", "", 80, EditorUiFactory.TextLight);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 120);

            SnakeStartPanel panel = root.GetComponent<SnakeStartPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;

            // Shown only when there is a saved game (the start panel decides).
            Button continueButton = EditorUiFactory.CreateButton(content.transform, "ContinueButton", "Continue", 150, EditorUiFactory.PanelColor);
            so.FindProperty("_continueButton").objectReferenceValue = continueButton;

            SerializedProperty levelButtons = so.FindProperty("_levelButtons");
            levelButtons.arraySize = 3;
            string[] names = { "EasyButton", "MediumButton", "HardButton" };
            Color[] tints = { EditorUiFactory.EasyColor, EditorUiFactory.MediumColor, EditorUiFactory.HardColor };
            for (int i = 0; i < 3; i++)
            {
                Button button = EditorUiFactory.CreateButton(content.transform, names[i], names[i], 170, tints[i]);
                levelButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
            }

            Button menu = EditorUiFactory.CreateButton(content.transform, "MenuButton", "Back", 120);
            menu.gameObject.AddComponent<BackToMenuButton>();
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static void CreateController(SnakeStartPanel start, SnakeBoardView board, SnakeSwipeArea swipe, SnakeHud hud, SnakeResultPanel result)
        {
            GameObject go = new GameObject("SnakeGameController", typeof(SnakeGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<SnakeGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_boardView").objectReferenceValue = board;
            so.FindProperty("_swipeArea").objectReferenceValue = swipe;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Snake";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.snake";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
