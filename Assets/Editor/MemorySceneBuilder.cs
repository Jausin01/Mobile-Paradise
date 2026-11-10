using MobileParadise.Core.UI;
using MobileParadise.Games.Memory.View;
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
    /// Builds the whole Memory_Game scene (start screen, board area, move counter, win panel),
    /// wires it to MemoryGameController and adds the game to the build and to the main menu.
    /// Run "1. Build Card Sets" first. Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class MemorySceneBuilder
    {
        private const string SceneName = "Memory_Game";
        private const string ScenePath = "Assets/Games/Memory/Scenes/" + SceneName + ".unity";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

        [MenuItem("Mobile Paradise/Memory/2. Build Memory Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;

            if (AssetDatabase.LoadAssetAtPath<MemoryCardSet>(MemoryCardSetBuilder.FruitsPath) == null ||
                AssetDatabase.LoadAssetAtPath<MemoryCardSet>(MemoryCardSetBuilder.AnimalsPath) == null)
            {
                EditorUtility.DisplayDialog("Card sets missing",
                    "Run 'Mobile Paradise > Memory > 1. Build Card Sets' first.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Memory/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            MemoryBoardView boardView = CreateBoard(root);
            MemoryHud hud = CreateHud(root);
            MemoryWinPanel winPanel = CreateWinPanel(root);
            MemoryStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, boardView, hud, winPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Memory_Game scene built and added to the main menu.");
        }

        // ---- Pieces of the scene ----------------------------------------------

        private static void CreateTitle(Transform parent)
        {
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Memory", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(700, 150);
            rect.anchoredPosition = new Vector2(60, -40);
            LocalizedText.Bind(title, "game.memory");
        }

        /// <summary>
        /// The card grid sits in an area between the move counter and the buttons;
        /// the holder (sized by code for each level) shrinks to fit it.
        /// </summary>
        private static MemoryBoardView CreateBoard(Transform parent)
        {
            // Loaded here, after the new scene exists: NewScene unloads assets nothing references yet.
            Sprite back = AssetDatabase.LoadAssetAtPath<Sprite>(MemoryCardSetBuilder.BackPath);
            Sprite frame = EditorUiFactory.LoadSprite(EditorUiFactory.FramePath);

            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent, typeof(MemoryBoardView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 290);
            areaRect.offsetMax = new Vector2(0, -390);

            GameObject holder = EditorUiFactory.CreateUiObject("BoardHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(968, 1216);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(new Vector2(120, 120));   // room for the frame around the cards

            // The wooden frame goes first so it is drawn behind the cards, and sticks out 40 px around them.
            GameObject frameObject = EditorUiFactory.CreatePanel(holder.transform, "BoardFrame", EditorUiFactory.WoodColor);
            RectTransform frameRect = frameObject.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(-40, -40);
            frameRect.offsetMax = new Vector2(40, 40);
            frameObject.GetComponent<Image>().raycastTarget = false;

            MemoryBoardView view = area.GetComponent<MemoryBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_backSprite").objectReferenceValue = back;
            so.FindProperty("_frameSprite").objectReferenceValue = frame;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static MemoryHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(MemoryHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Move counter: "Moves  12"
            GameObject panel = EditorUiFactory.CreatePanel(root.transform, "MovesPanel", EditorUiFactory.PanelColor);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 1);
            panelRect.offsetMin = new Vector2(60, -370);
            panelRect.offsetMax = new Vector2(-60, -210);

            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 20, 20);
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            TextMeshProUGUI label = EditorUiFactory.CreateText(panel.transform, "MovesLabel", "Moves", 64);
            TextMeshProUGUI value = EditorUiFactory.CreateText(panel.transform, "MovesValue", "0", 72);
            label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            value.gameObject.AddComponent<LayoutElement>().minWidth = 160;

            // Buttons: New game, Back
            GameObject row = EditorUiFactory.CreateUiObject("Buttons", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.offsetMin = new Vector2(40, 60);
            rowRect.offsetMax = new Vector2(-40, 260);

            HorizontalLayoutGroup rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 30;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            Button newGame = CreateHudButton(row.transform, "NewGameButton");
            Button back = CreateHudButton(row.transform, "BackButton");

            MemoryHud hud = root.GetComponent<MemoryHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_movesLabel").objectReferenceValue = label;
            so.FindProperty("_movesValue").objectReferenceValue = value;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static Button CreateHudButton(Transform parent, string name)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, 200);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 52);
            return button;
        }

        private static MemoryWinPanel CreateWinPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("WinPanel", parent, typeof(Image), typeof(MemoryWinPanel));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = DimColor;

            GameObject box = EditorUiFactory.CreatePanel(root.transform, "Box", EditorUiFactory.PanelColor);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(900, 800);
            box.AddComponent<FitInParent>();

            VerticalLayoutGroup layout = box.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 50, 50);
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = EditorUiFactory.CreateText(box.transform, "Title", "", 96);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 140);

            GameObject movesRow = EditorUiFactory.CreateUiObject("MovesRow", box.transform, typeof(HorizontalLayoutGroup));
            EditorUiFactory.SetPreferredHeight(movesRow, 100);
            HorizontalLayoutGroup movesLayout = movesRow.GetComponent<HorizontalLayoutGroup>();
            movesLayout.spacing = 30;
            movesLayout.childAlignment = TextAnchor.MiddleCenter;
            movesLayout.childControlWidth = true;
            movesLayout.childControlHeight = true;
            movesLayout.childForceExpandWidth = false;
            movesLayout.childForceExpandHeight = true;
            TextMeshProUGUI movesLabel = EditorUiFactory.CreateText(movesRow.transform, "MovesLabel", "", 64);
            TextMeshProUGUI movesValue = EditorUiFactory.CreateText(movesRow.transform, "MovesValue", "0", 72);
            movesLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            movesValue.gameObject.AddComponent<LayoutElement>().minWidth = 160;

            Button again = EditorUiFactory.CreateButton(box.transform, "PlayAgainButton", "", 150, EditorUiFactory.EasyColor);
            Button back = EditorUiFactory.CreateButton(box.transform, "BackButton", "", 130);

            MemoryWinPanel panel = root.GetComponent<MemoryWinPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_movesLabel").objectReferenceValue = movesLabel;
            so.FindProperty("_movesValue").objectReferenceValue = movesValue;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static MemoryStartPanel CreateStartPanel(Transform parent)
        {
            // Loaded here, after the new scene exists (see CreateBoard).
            MemoryCardSet[] sets =
            {
                AssetDatabase.LoadAssetAtPath<MemoryCardSet>(MemoryCardSetBuilder.FruitsPath),
                AssetDatabase.LoadAssetAtPath<MemoryCardSet>(MemoryCardSetBuilder.AnimalsPath)
            };

            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(MemoryStartPanel));
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
            contentRect.sizeDelta = new Vector2(960, 1350);
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

            MemoryStartPanel panel = root.GetComponent<MemoryStartPanel>();
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

            so.FindProperty("_themeLabel").objectReferenceValue = EditorUiFactory.CreateSectionLabel(content.transform, "ThemeLabel");
            GameObject themeRow = EditorUiFactory.CreateRow(content.transform, "ThemeRow", 120);
            SerializedProperty cardSets = so.FindProperty("_cardSets");
            SerializedProperty themeButtons = so.FindProperty("_themeButtons");
            SerializedProperty themeIcons = so.FindProperty("_themeIcons");
            cardSets.arraySize = 2;
            themeButtons.arraySize = 2;
            themeIcons.arraySize = 2;
            string[] themeNames = { "FruitsButton", "AnimalsButton" };
            for (int i = 0; i < 2; i++)
            {
                Image icon;
                Button button = EditorUiFactory.CreateIconButton(themeRow.transform, themeNames[i], out icon);
                cardSets.GetArrayElementAtIndex(i).objectReferenceValue = sets[i];
                themeButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
                themeIcons.GetArrayElementAtIndex(i).objectReferenceValue = icon;
            }

            Button menu = EditorUiFactory.CreateButton(content.transform, "MenuButton", "Back", 120);
            menu.gameObject.AddComponent<BackToMenuButton>();
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static void CreateController(MemoryStartPanel start, MemoryBoardView board, MemoryHud hud, MemoryWinPanel win)
        {
            GameObject go = new GameObject("MemoryGameController", typeof(MemoryGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<MemoryGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_boardView").objectReferenceValue = board;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_winPanel").objectReferenceValue = win;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Memory";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.memory";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(MemoryCardSetBuilder.BackPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
