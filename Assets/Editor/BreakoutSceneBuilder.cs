using MobileParadise.Core.UI;
using MobileParadise.Games.Breakout.View;
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
    /// Builds the whole Breakout_Game scene (start screen, field, buttons, result panel),
    /// wires it to BreakoutGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class BreakoutSceneBuilder
    {
        private const string SceneName = "Breakout_Game";
        private const string ScenePath = "Assets/Games/Breakout/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Breakout/Art/icon.png";

        private static readonly Color BackdropColor = new Color(0.10f, 0.10f, 0.20f);
        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        [MenuItem("Mobile Paradise/Breakout/Build Breakout Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Breakout/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            BreakoutFieldView fieldView = CreateField(root, out BreakoutInputArea inputArea);
            BreakoutHud hud = CreateHud(root);
            BreakoutResultPanel resultPanel = CreateResultPanel(root);
            BreakoutStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, fieldView, inputArea, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Breakout_Game scene built and added to the main menu.");
        }

        private static void ImportArt()
        {
            TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("Missing art: " + IconPath);
                return;
            }

            PixelArtImporter.ApplySettings(importer);
            importer.SaveAndReimport();
        }

        // ---- Pieces of the scene ----------------------------------------------

        private static void CreateTitle(Transform parent)
        {
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Breakout", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stops before the music note and gear buttons (350 px on the right).
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.breakout");
        }

        /// <summary>
        /// The field sits between the info panel and the buttons; the holder (fixed 1000 x 1400, the size of the
        /// game field) shrinks to fit. The whole area catches the touches that move the paddle.
        /// </summary>
        private static BreakoutFieldView CreateField(Transform parent, out BreakoutInputArea inputArea)
        {
            GameObject area = EditorUiFactory.CreateUiObject("FieldArea", parent, typeof(Image), typeof(BreakoutInputArea));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 330);
            areaRect.offsetMax = new Vector2(0, -390);

            // Invisible, but it receives the touches.
            Image catcher = area.GetComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            catcher.raycastTarget = true;

            GameObject holder = EditorUiFactory.CreateUiObject("FieldHolder", area.transform, typeof(FitInParent), typeof(BreakoutFieldView));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(1000, 1400);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // The dark backdrop goes first so it is drawn behind the bricks.
            GameObject backdrop = EditorUiFactory.CreatePanel(holder.transform, "Backdrop", BackdropColor);
            EditorUiFactory.Stretch(backdrop.GetComponent<RectTransform>());
            backdrop.GetComponent<Image>().raycastTarget = false;

            BreakoutFieldView view = holder.GetComponent<BreakoutFieldView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_frameSprite").objectReferenceValue = EditorUiFactory.LoadSprite(EditorUiFactory.FramePath);
            so.ApplyModifiedPropertiesWithoutUndo();

            inputArea = area.GetComponent<BreakoutInputArea>();
            SerializedObject inputSo = new SerializedObject(inputArea);
            inputSo.FindProperty("_field").objectReferenceValue = holderRect;
            inputSo.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static BreakoutHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(BreakoutHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Level, score and lives
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

            TextMeshProUGUI level = EditorUiFactory.CreateText(panel.transform, "LevelText", "", 56);
            TextMeshProUGUI score = EditorUiFactory.CreateText(panel.transform, "ScoreText", "", 56);
            TextMeshProUGUI lives = EditorUiFactory.CreateText(panel.transform, "LivesText", "", 56);

            // Hint above the buttons
            TextMeshProUGUI hint = EditorUiFactory.CreateText(root.transform, "HintText", "", 52, EditorUiFactory.TextLight);
            RectTransform hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0, 0);
            hintRect.anchorMax = new Vector2(1, 0);
            hintRect.pivot = new Vector2(0.5f, 0);
            hintRect.offsetMin = new Vector2(60, 215);
            hintRect.offsetMax = new Vector2(-60, 300);

            // Pause / New game / Back
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

            BreakoutHud hud = root.GetComponent<BreakoutHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_levelText").objectReferenceValue = level;
            so.FindProperty("_scoreText").objectReferenceValue = score;
            so.FindProperty("_livesText").objectReferenceValue = lives;
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

        private static BreakoutResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(BreakoutResultPanel));
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

            BreakoutResultPanel panel = root.GetComponent<BreakoutResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static BreakoutStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(BreakoutStartPanel));
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

            BreakoutStartPanel panel = root.GetComponent<BreakoutStartPanel>();
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

        private static void CreateController(BreakoutStartPanel start, BreakoutFieldView field, BreakoutInputArea input, BreakoutHud hud, BreakoutResultPanel result)
        {
            GameObject go = new GameObject("BreakoutGameController", typeof(BreakoutGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<BreakoutGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_fieldView").objectReferenceValue = field;
            so.FindProperty("_inputArea").objectReferenceValue = input;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Breakout";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.breakout";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
