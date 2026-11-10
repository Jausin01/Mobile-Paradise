using MobileParadise.Core.UI;
using MobileParadise.Games.Sudoku.View;
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
    /// Builds the whole Sudoku_Game scene (start screen, grid, number pad, buttons, win panel),
    /// wires it to SudokuGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class SudokuSceneBuilder
    {
        private const string SceneName = "Sudoku_Game";
        private const string ScenePath = "Assets/Games/Sudoku/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Sudoku/Art/icon.png";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

        [MenuItem("Mobile Paradise/Sudoku/Build Sudoku Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportIcon();
            EditorProjectUtil.EnsureFolder("Assets/Games/Sudoku/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            SudokuBoardView boardView = CreateBoard(root);
            SudokuHud hud = CreateHud(root);
            SudokuNumberPad numberPad = CreateNumberPad(root);
            SudokuWinPanel winPanel = CreateWinPanel(root);
            SudokuStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, boardView, numberPad, hud, winPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Sudoku_Game scene built and added to the main menu.");
        }

        private static void ImportIcon()
        {
            TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null) return;

            PixelArtImporter.ApplySettings(importer);
            importer.SaveAndReimport();
        }

        // ---- Pieces of the scene ----------------------------------------------

        private static void CreateTitle(Transform parent)
        {
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Sudoku", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(700, 150);
            rect.anchoredPosition = new Vector2(60, -40);
            LocalizedText.Bind(title, "game.sudoku");
        }

        /// <summary>
        /// The grid sits in an area between the level name and the number pad;
        /// the holder (fixed size) shrinks to fit it on every screen shape.
        /// </summary>
        private static SudokuBoardView CreateBoard(Transform parent)
        {
            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent, typeof(SudokuBoardView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 530);
            areaRect.offsetMax = new Vector2(0, -390);

            GameObject holder = EditorUiFactory.CreateUiObject("BoardHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(984, 984);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(new Vector2(100, 100));   // room for the frame

            // The frame goes first so it is drawn behind the cells, and sticks out 40 px around them.
            GameObject frame = EditorUiFactory.CreatePanel(holder.transform, "BoardFrame", EditorUiFactory.WoodColor);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(-40, -40);
            frameRect.offsetMax = new Vector2(40, 40);
            frame.GetComponent<Image>().raycastTarget = false;

            SudokuBoardView view = area.GetComponent<SudokuBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static SudokuHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(SudokuHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Level name
            GameObject panel = EditorUiFactory.CreatePanel(root.transform, "LevelPanel", EditorUiFactory.PanelColor);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 1);
            panelRect.offsetMin = new Vector2(60, -370);
            panelRect.offsetMax = new Vector2(-60, -210);

            TextMeshProUGUI level = EditorUiFactory.CreateText(panel.transform, "LevelText", "", 72);
            EditorUiFactory.Stretch(level.rectTransform);

            // Buttons: New game, Hint, Back
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

            Button newGame = CreateHudButton(row.transform, "NewGameButton", EditorUiFactory.ButtonColor);
            Button hint = CreateHudButton(row.transform, "HintButton", EditorUiFactory.EasyColor);
            Button back = CreateHudButton(row.transform, "BackButton", EditorUiFactory.ButtonColor);

            SudokuHud hud = root.GetComponent<SudokuHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_levelText").objectReferenceValue = level;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_hintButton").objectReferenceValue = hint;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static Button CreateHudButton(Transform parent, string name, Color tint)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, 140, tint);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 52);
            return button;
        }

        /// <summary>
        /// Two rows: 1-5, then 6-9 and Erase. Big buttons for big fingers.
        /// </summary>
        private static SudokuNumberPad CreateNumberPad(Transform parent)
        {
            GameObject pad = EditorUiFactory.CreateUiObject("NumberPad", parent, typeof(VerticalLayoutGroup), typeof(SudokuNumberPad));
            RectTransform padRect = pad.GetComponent<RectTransform>();
            padRect.anchorMin = new Vector2(0, 0);
            padRect.anchorMax = new Vector2(1, 0);
            padRect.pivot = new Vector2(0.5f, 0);
            padRect.offsetMin = new Vector2(40, 220);
            padRect.offsetMax = new Vector2(-40, 510);

            VerticalLayoutGroup layout = pad.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            GameObject firstRow = EditorUiFactory.CreateRow(pad.transform, "Row1", 137);
            GameObject secondRow = EditorUiFactory.CreateRow(pad.transform, "Row2", 137);

            SudokuNumberPad view = pad.GetComponent<SudokuNumberPad>();
            SerializedObject so = new SerializedObject(view);
            SerializedProperty digitButtons = so.FindProperty("_digitButtons");
            digitButtons.arraySize = 9;

            for (int digit = 1; digit <= 9; digit++)
            {
                Transform row = digit <= 5 ? firstRow.transform : secondRow.transform;
                Button button = EditorUiFactory.CreateButton(row, "Digit" + digit, digit.ToString(), 137);
                digitButtons.GetArrayElementAtIndex(digit - 1).objectReferenceValue = button;
            }

            Button erase = EditorUiFactory.CreateButton(secondRow.transform, "EraseButton", "Erase", 137, EditorUiFactory.HardColor);
            so.FindProperty("_eraseButton").objectReferenceValue = erase;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static SudokuWinPanel CreateWinPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("WinPanel", parent, typeof(Image), typeof(SudokuWinPanel));
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

            TextMeshProUGUI title = EditorUiFactory.CreateText(box.transform, "Title", "", 100);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 160);
            Button again = EditorUiFactory.CreateButton(box.transform, "PlayAgainButton", "", 150, EditorUiFactory.EasyColor);
            Button back = EditorUiFactory.CreateButton(box.transform, "BackButton", "", 130);

            SudokuWinPanel panel = root.GetComponent<SudokuWinPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static SudokuStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(SudokuStartPanel));
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

            SudokuStartPanel panel = root.GetComponent<SudokuStartPanel>();
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

        private static void CreateController(SudokuStartPanel start, SudokuBoardView board, SudokuNumberPad pad, SudokuHud hud, SudokuWinPanel win)
        {
            GameObject go = new GameObject("SudokuGameController", typeof(SudokuGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<SudokuGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_boardView").objectReferenceValue = board;
            so.FindProperty("_numberPad").objectReferenceValue = pad;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_winPanel").objectReferenceValue = win;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Sudoku";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.sudoku";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
