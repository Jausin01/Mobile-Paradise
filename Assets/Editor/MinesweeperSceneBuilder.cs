using MobileParadise.Core.UI;
using MobileParadise.Games.Minesweeper.View;
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
    /// Builds the whole Minesweeper_Game scene (start screen, grid, buttons, result panel),
    /// wires it to MinesGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class MinesweeperSceneBuilder
    {
        private const string SceneName = "Minesweeper_Game";
        private const string ScenePath = "Assets/Games/Minesweeper/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Minesweeper/Art/icon.png";
        private const string FlagPath = "Assets/Games/Minesweeper/Art/flag.png";
        private const string MinePath = "Assets/Games/Minesweeper/Art/mine.png";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.4f);   // light: the mines stay visible behind it

        [MenuItem("Mobile Paradise/Minesweeper/Build Minesweeper Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Minesweeper/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            MinesBoardView boardView = CreateBoard(root);
            MinesHud hud = CreateHud(root);
            MinesResultPanel resultPanel = CreateResultPanel(root);
            MinesStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, boardView, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Minesweeper_Game scene built and added to the main menu.");
        }

        private static void ImportArt()
        {
            string[] paths = { IconPath, FlagPath, MinePath };
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
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Minesweeper", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stretches from the left edge up to the music note and gear buttons (350 px on the right),
            // so a long name shrinks to fit instead of going under them.
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.minesweeper");
        }

        /// <summary>
        /// The grid sits in an area between the level / mines panel and the buttons;
        /// the holder (fixed size) shrinks to fit it on every screen shape.
        /// The board view sizes the cells and the wood frame itself, per level.
        /// </summary>
        private static MinesBoardView CreateBoard(Transform parent)
        {
            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent, typeof(MinesBoardView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 400);
            areaRect.offsetMax = new Vector2(0, -390);

            GameObject holder = EditorUiFactory.CreateUiObject("BoardHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(984, 1130);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // The frame goes first so it is drawn behind the cells.
            GameObject frame = EditorUiFactory.CreatePanel(holder.transform, "BoardFrame", EditorUiFactory.WoodColor);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.sizeDelta = new Vector2(984, 1130);
            frame.GetComponent<Image>().raycastTarget = false;

            MinesBoardView view = area.GetComponent<MinesBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_frame").objectReferenceValue = frameRect;
            so.FindProperty("_flagSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(FlagPath);
            so.FindProperty("_mineSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(MinePath);
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static MinesHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(MinesHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Level name (left) and mines left (right)
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

            TextMeshProUGUI level = EditorUiFactory.CreateText(panel.transform, "LevelText", "", 64);
            TextMeshProUGUI mines = EditorUiFactory.CreateText(panel.transform, "MinesText", "", 64);

            // Open / Flag (what a tap does)
            GameObject modeRow = CreateButtonRow(root.transform, "ModeRow", 220, 380);
            Button open = CreateHudButton(modeRow.transform, "OpenButton", EditorUiFactory.EasyColor, 160);
            Button flag = CreateHudButton(modeRow.transform, "FlagButton", EditorUiFactory.ButtonColor, 160);

            // New game / Back
            GameObject bottomRow = CreateButtonRow(root.transform, "Buttons", 60, 200);
            Button newGame = CreateHudButton(bottomRow.transform, "NewGameButton", EditorUiFactory.ButtonColor, 140);
            Button back = CreateHudButton(bottomRow.transform, "BackButton", EditorUiFactory.ButtonColor, 140);

            MinesHud hud = root.GetComponent<MinesHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_levelText").objectReferenceValue = level;
            so.FindProperty("_minesText").objectReferenceValue = mines;
            so.FindProperty("_openButton").objectReferenceValue = open;
            so.FindProperty("_flagButton").objectReferenceValue = flag;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static GameObject CreateButtonRow(Transform parent, string name, float bottom, float top)
        {
            GameObject row = EditorUiFactory.CreateUiObject(name, parent, typeof(HorizontalLayoutGroup));
            RectTransform rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.offsetMin = new Vector2(40, bottom);
            rect.offsetMax = new Vector2(-40, top);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return row;
        }

        private static Button CreateHudButton(Transform parent, string name, Color tint, float height)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, height, tint);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 56);
            return button;
        }

        private static MinesResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(MinesResultPanel));
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

            MinesResultPanel panel = root.GetComponent<MinesResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static MinesStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(MinesStartPanel));
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

            MinesStartPanel panel = root.GetComponent<MinesStartPanel>();
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

        private static void CreateController(MinesStartPanel start, MinesBoardView board, MinesHud hud, MinesResultPanel result)
        {
            GameObject go = new GameObject("MinesGameController", typeof(MinesGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<MinesGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_boardView").objectReferenceValue = board;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Minesweeper";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.minesweeper";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
