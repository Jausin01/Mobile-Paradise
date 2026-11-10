using MobileParadise.Core.UI;
using MobileParadise.Games.WordSearch.View;
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
    /// Builds the whole WordSearch_Game scene (start screen, letter grid, word list, buttons, result panel),
    /// wires it to WordSearchGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class WordSearchSceneBuilder
    {
        private const string SceneName = "WordSearch_Game";
        private const string ScenePath = "Assets/Games/WordSearch/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/WordSearch/Art/icon.png";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        [MenuItem("Mobile Paradise/Word Search/Build Word Search Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/WordSearch/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            WordSearchBoardView boardView = CreateBoard(root);
            WordListView wordList = CreateWordList(root);
            WordSearchHud hud = CreateHud(root);
            WordSearchResultPanel resultPanel = CreateResultPanel(root);
            WordSearchStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, boardView, wordList, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: WordSearch_Game scene built and added to the main menu.");
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
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Word Search", 90, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stops before the music note and gear buttons (350 px on the right).
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.wordsearch");
        }

        /// <summary>
        /// The grid sits between the info panel and the word list; the holder (fixed size) shrinks to fit.
        /// </summary>
        private static WordSearchBoardView CreateBoard(Transform parent)
        {
            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent, typeof(WordSearchBoardView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 530);
            areaRect.offsetMax = new Vector2(0, -390);

            GameObject holder = EditorUiFactory.CreateUiObject("GridHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(984, 984);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // The wooden frame goes first so it is drawn behind the letters.
            GameObject frame = EditorUiFactory.CreatePanel(holder.transform, "GridFrame", EditorUiFactory.WoodColor);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.sizeDelta = new Vector2(984, 984);
            frame.GetComponent<Image>().raycastTarget = false;

            // A transparent layer over the grid that reads the finger (the board view keeps it on top).
            GameObject input = EditorUiFactory.CreateUiObject("InputArea", holder.transform, typeof(Image), typeof(WordGridInput));
            EditorUiFactory.Stretch(input.GetComponent<RectTransform>());
            Image inputImage = input.GetComponent<Image>();
            inputImage.color = new Color(0, 0, 0, 0);
            inputImage.raycastTarget = true;

            WordSearchBoardView view = area.GetComponent<WordSearchBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_frame").objectReferenceValue = frameRect;
            so.FindProperty("_input").objectReferenceValue = input.GetComponent<WordGridInput>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        /// <summary>
        /// A panel with the words to find, in two columns, between the grid and the buttons.
        /// </summary>
        private static WordListView CreateWordList(Transform parent)
        {
            GameObject panel = EditorUiFactory.CreatePanel(parent, "WordList", EditorUiFactory.PanelColor);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.offsetMin = new Vector2(40, 215);
            rect.offsetMax = new Vector2(-40, 515);
            panel.GetComponent<Image>().raycastTarget = false;

            GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(20, 20, 8, 8);
            grid.spacing = new Vector2(20, 6);
            grid.cellSize = new Vector2(470, 52);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;

            return panel.AddComponent<WordListView>();
        }

        private static WordSearchHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(WordSearchHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Theme (left) and words found (right)
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

            TextMeshProUGUI theme = EditorUiFactory.CreateText(panel.transform, "ThemeText", "", 56);
            TextMeshProUGUI count = EditorUiFactory.CreateText(panel.transform, "CountText", "", 56);

            // Hint / New game / Back
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

            Button hint = CreateHudButton(row.transform, "HintButton", EditorUiFactory.EasyColor);
            Button newGame = CreateHudButton(row.transform, "NewGameButton", EditorUiFactory.ButtonColor);
            Button back = CreateHudButton(row.transform, "BackButton", EditorUiFactory.ButtonColor);

            WordSearchHud hud = root.GetComponent<WordSearchHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_themeText").objectReferenceValue = theme;
            so.FindProperty("_countText").objectReferenceValue = count;
            so.FindProperty("_hintButton").objectReferenceValue = hint;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
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

        private static WordSearchResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(WordSearchResultPanel));
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

            TextMeshProUGUI title = EditorUiFactory.CreateText(box.transform, "Title", "", 80);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 180);
            Button again = EditorUiFactory.CreateButton(box.transform, "PlayAgainButton", "", 150, EditorUiFactory.EasyColor);
            Button back = EditorUiFactory.CreateButton(box.transform, "BackButton", "", 130);

            WordSearchResultPanel panel = root.GetComponent<WordSearchResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static WordSearchStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(WordSearchStartPanel));
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
            contentRect.sizeDelta = new Vector2(960, 1220);
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

            WordSearchStartPanel panel = root.GetComponent<WordSearchStartPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;

            // Shown only when there is a saved puzzle (the start panel decides).
            Button continueButton = EditorUiFactory.CreateButton(content.transform, "ContinueButton", "Continue", 150, EditorUiFactory.PanelColor);
            so.FindProperty("_continueButton").objectReferenceValue = continueButton;

            // Taps cycle through the themes.
            Button themeButton = EditorUiFactory.CreateButton(content.transform, "ThemeButton", "Theme", 140, EditorUiFactory.ButtonColor);
            EditorUiFactory.SetFontSize(themeButton.GetComponentInChildren<TMP_Text>(), 56);
            so.FindProperty("_themeButton").objectReferenceValue = themeButton;

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

        private static void CreateController(WordSearchStartPanel start, WordSearchBoardView board, WordListView wordList,
            WordSearchHud hud, WordSearchResultPanel result)
        {
            GameObject go = new GameObject("WordSearchGameController", typeof(WordSearchGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<WordSearchGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_boardView").objectReferenceValue = board;
            so.FindProperty("_wordList").objectReferenceValue = wordList;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Word Search";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.wordsearch";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
