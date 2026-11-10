using MobileParadise.Core.UI;
using MobileParadise.Games.Solitaire.View;
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
    /// Builds the whole Solitaire_Game scene (start screen, table, buttons, win panel),
    /// wires it to SolitaireGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class SolitaireSceneBuilder
    {
        private const string SceneName = "Solitaire_Game";
        private const string ScenePath = "Assets/Games/Solitaire/Scenes/" + SceneName + ".unity";
        private const string ArtFolder = "Assets/Games/Solitaire/Art/";
        private const string IconPath = ArtFolder + "icon.png";

        private static readonly string[] SuitPaths =
        {
            ArtFolder + "suit_clubs.png",
            ArtFolder + "suit_diamonds.png",
            ArtFolder + "suit_hearts.png",
            ArtFolder + "suit_spades.png"
        };

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color FeltColor = new Color(0.20f, 0.42f, 0.28f);

        [MenuItem("Mobile Paradise/Solitaire/Build Solitaire Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Solitaire/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            SolitaireBoardView boardView = CreateBoard(root);
            SolitaireHud hud = CreateHud(root);
            SolitaireWinPanel winPanel = CreateWinPanel(root);
            SolitaireStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, boardView, hud, winPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Solitaire_Game scene built and added to the main menu.");
        }

        private static void ImportArt()
        {
            string[] paths = { IconPath, SuitPaths[0], SuitPaths[1], SuitPaths[2], SuitPaths[3] };
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
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Solitaire", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;

            // Stretches from the left edge up to the music note and gear buttons (350 px on the right).
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.solitaire");
        }

        /// <summary>
        /// The table sits in an area between the level / moves panel and the buttons;
        /// the holder (fixed size) shrinks to fit it on every screen shape.
        /// The board view creates the cards inside the holder.
        /// </summary>
        private static SolitaireBoardView CreateBoard(Transform parent)
        {
            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent, typeof(SolitaireBoardView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 220);
            areaRect.offsetMax = new Vector2(0, -390);

            GameObject holder = EditorUiFactory.CreateUiObject("BoardHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(1032, 1310);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // The green felt goes first so it is drawn behind everything.
            GameObject felt = EditorUiFactory.CreatePanel(holder.transform, "Felt", FeltColor);
            EditorUiFactory.Stretch(felt.GetComponent<RectTransform>());
            felt.GetComponent<Image>().raycastTarget = false;

            SolitaireBoardView view = area.GetComponent<SolitaireBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_frameSprite").objectReferenceValue = EditorUiFactory.LoadSprite(EditorUiFactory.FramePath);
            so.FindProperty("_recycleSprite").objectReferenceValue = EditorUiFactory.LoadSprite(EditorUiFactory.RepeatPath);

            SerializedProperty suits = so.FindProperty("_suitSprites");
            suits.arraySize = SuitPaths.Length;
            for (int i = 0; i < SuitPaths.Length; i++)
            {
                suits.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SuitPaths[i]);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static SolitaireHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(SolitaireHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Level (left) and moves (right)
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
            TextMeshProUGUI moves = EditorUiFactory.CreateText(panel.transform, "MovesText", "", 64);

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

            Button undo = CreateHudButton(row.transform, "UndoButton");
            Button newGame = CreateHudButton(row.transform, "NewGameButton");
            Button back = CreateHudButton(row.transform, "BackButton");

            SolitaireHud hud = root.GetComponent<SolitaireHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_levelText").objectReferenceValue = level;
            so.FindProperty("_movesText").objectReferenceValue = moves;
            so.FindProperty("_undoButton").objectReferenceValue = undo;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static Button CreateHudButton(Transform parent, string name)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, 140);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 52);
            return button;
        }

        private static SolitaireWinPanel CreateWinPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("WinPanel", parent, typeof(Image), typeof(SolitaireWinPanel));
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

            SolitaireWinPanel panel = root.GetComponent<SolitaireWinPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static SolitaireStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(SolitaireStartPanel));
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
            contentRect.sizeDelta = new Vector2(960, 880);
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

            SolitaireStartPanel panel = root.GetComponent<SolitaireStartPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;

            // Shown only when there is a saved game (the start panel decides).
            Button continueButton = EditorUiFactory.CreateButton(content.transform, "ContinueButton", "Continue", 150, EditorUiFactory.PanelColor);
            so.FindProperty("_continueButton").objectReferenceValue = continueButton;

            SerializedProperty levelButtons = so.FindProperty("_levelButtons");
            levelButtons.arraySize = 2;
            string[] names = { "EasyButton", "HardButton" };
            Color[] tints = { EditorUiFactory.EasyColor, EditorUiFactory.HardColor };
            for (int i = 0; i < 2; i++)
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

        private static void CreateController(SolitaireStartPanel start, SolitaireBoardView board, SolitaireHud hud, SolitaireWinPanel win)
        {
            GameObject go = new GameObject("SolitaireGameController", typeof(SolitaireGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<SolitaireGameController>());
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
            entry.DisplayName = "Solitaire";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.solitaire";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
