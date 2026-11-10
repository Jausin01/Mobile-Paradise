using MobileParadise.Core.UI;
using MobileParadise.Games.Tetris.View;
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
    /// Builds the whole Tetris_Game scene (start screen, board, next piece, buttons, result panel),
    /// wires it to TetrisGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class TetrisSceneBuilder
    {
        private const string SceneName = "Tetris_Game";
        private const string ScenePath = "Assets/Games/Tetris/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Tetris/Art/icon.png";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        [MenuItem("Mobile Paradise/Tetris/Build Tetris Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Tetris/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            Playfield playfield = CreatePlayfield(root);
            TetrisHud hud = CreateHud(root, playfield);
            TetrisResultPanel resultPanel = CreateResultPanel(root);
            TetrisStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, playfield, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Tetris_Game scene built and added to the main menu.");
        }

        // What CreatePlayfield makes, so the other steps can wire to it.
        private class Playfield
        {
            public TetrisBoardView Board;
            public TetrisNextView Next;
            public TetrisTouchArea Touch;
            public TMP_Text NextLabel;
            public GameObject PausedPanel;
            public TMP_Text PausedText;
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
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Tetris", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stops before the music note and gear buttons (350 px on the right).
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.tetris");
        }

        /// <summary>
        /// The board (left) and the next-piece box (right) in one fixed-size holder that shrinks to fit the free area.
        /// </summary>
        private static Playfield CreatePlayfield(Transform parent)
        {
            Playfield result = new Playfield();

            GameObject area = EditorUiFactory.CreateUiObject("PlayfieldArea", parent);
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 385);
            areaRect.offsetMax = new Vector2(0, -445);

            GameObject holder = EditorUiFactory.CreateUiObject("Playfield", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(880, 1140);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // Board: wooden frame, the touch layer, and (made by the board view when the game starts) the cells.
            GameObject board = EditorUiFactory.CreateUiObject("BoardHolder", holder.transform, typeof(TetrisBoardView));
            RectTransform boardRect = board.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0, 0.5f);
            boardRect.anchorMax = new Vector2(0, 0.5f);
            boardRect.pivot = new Vector2(0, 0.5f);
            boardRect.sizeDelta = new Vector2(580, 1140);
            boardRect.anchoredPosition = Vector2.zero;

            GameObject frame = EditorUiFactory.CreatePanel(board.transform, "BoardFrame", EditorUiFactory.WoodColor);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.sizeDelta = new Vector2(580, 1140);
            frame.GetComponent<Image>().raycastTarget = false;

            GameObject touch = EditorUiFactory.CreateUiObject("TouchArea", board.transform, typeof(Image), typeof(TetrisTouchArea));
            EditorUiFactory.Stretch(touch.GetComponent<RectTransform>());
            Image touchImage = touch.GetComponent<Image>();
            touchImage.color = new Color(0, 0, 0, 0);
            touchImage.raycastTarget = true;

            TetrisBoardView boardView = board.GetComponent<TetrisBoardView>();
            SerializedObject boardSo = new SerializedObject(boardView);
            boardSo.FindProperty("_holder").objectReferenceValue = boardRect;
            boardSo.FindProperty("_frame").objectReferenceValue = frameRect;
            boardSo.ApplyModifiedPropertiesWithoutUndo();

            CreateNextBox(holder.transform, result);
            CreatePausedPanel(holder.transform, result);

            result.Board = boardView;
            result.Touch = touch.GetComponent<TetrisTouchArea>();
            return result;
        }

        private static void CreateNextBox(Transform parent, Playfield result)
        {
            GameObject panel = EditorUiFactory.CreatePanel(parent, "NextPanel", EditorUiFactory.PanelColor);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(270, 330);
            rect.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().raycastTarget = false;

            TextMeshProUGUI label = EditorUiFactory.CreateText(panel.transform, "NextLabel", "", 48);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(0.5f, 1);
            labelRect.offsetMin = new Vector2(10, -80);
            labelRect.offsetMax = new Vector2(-10, -10);

            GameObject nextHolder = EditorUiFactory.CreateUiObject("NextHolder", panel.transform);
            RectTransform nextRect = nextHolder.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(0.5f, 1);
            nextRect.anchorMax = new Vector2(0.5f, 1);
            nextRect.pivot = new Vector2(0.5f, 1);
            nextRect.sizeDelta = new Vector2(200, 200);
            nextRect.anchoredPosition = new Vector2(0, -95);

            TetrisNextView view = panel.AddComponent<TetrisNextView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = nextRect;
            so.ApplyModifiedPropertiesWithoutUndo();

            result.Next = view;
            result.NextLabel = label;
        }

        // A panel with "Paused" over the board (a sibling after the board, so it is drawn above the cells).
        private static void CreatePausedPanel(Transform parent, Playfield result)
        {
            GameObject panel = EditorUiFactory.CreatePanel(parent, "PausedPanel", EditorUiFactory.PanelColor);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(580, 220);
            rect.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().raycastTarget = false;

            TextMeshProUGUI text = EditorUiFactory.CreateText(panel.transform, "PausedText", "", 90);
            EditorUiFactory.Stretch(text.rectTransform);

            result.PausedPanel = panel;
            result.PausedText = text;
        }

        private static TetrisHud CreateHud(Transform parent, Playfield playfield)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(TetrisHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Score, best, lines, level: a 2 x 2 grid on top
            GameObject panel = EditorUiFactory.CreatePanel(root.transform, "InfoPanel", EditorUiFactory.PanelColor);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 1);
            panelRect.offsetMin = new Vector2(60, -430);
            panelRect.offsetMax = new Vector2(-60, -210);

            GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(20, 20, 10, 10);
            grid.spacing = new Vector2(10, 0);
            grid.cellSize = new Vector2(450, 100);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.MiddleCenter;

            TextMeshProUGUI score = EditorUiFactory.CreateText(panel.transform, "ScoreText", "", 50);
            TextMeshProUGUI best = EditorUiFactory.CreateText(panel.transform, "BestText", "", 50);
            TextMeshProUGUI lines = EditorUiFactory.CreateText(panel.transform, "LinesText", "", 50);
            TextMeshProUGUI level = EditorUiFactory.CreateText(panel.transform, "LevelText", "", 50);

            // Left / Rotate / Right / Down
            GameObject controls = EditorUiFactory.CreateUiObject("Controls", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform controlsRect = controls.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0, 0);
            controlsRect.anchorMax = new Vector2(1, 0);
            controlsRect.pivot = new Vector2(0.5f, 0);
            controlsRect.offsetMin = new Vector2(30, 215);
            controlsRect.offsetMax = new Vector2(-30, 365);
            SetupRow(controls.GetComponent<HorizontalLayoutGroup>(), 14);

            Button left = CreateControlButton(controls.transform, "LeftButton", EditorUiFactory.ButtonColor, 150);
            Button rotate = CreateControlButton(controls.transform, "RotateButton", EditorUiFactory.MediumColor, 150);
            Button right = CreateControlButton(controls.transform, "RightButton", EditorUiFactory.ButtonColor, 150);
            Button down = CreateControlButton(controls.transform, "DownButton", EditorUiFactory.EasyColor, 150);
            TetrisHoldButton leftHold = left.gameObject.AddComponent<TetrisHoldButton>();
            TetrisHoldButton rightHold = right.gameObject.AddComponent<TetrisHoldButton>();
            TetrisHoldButton downHold = down.gameObject.AddComponent<TetrisHoldButton>();

            // Pause / New game / Back
            GameObject row = EditorUiFactory.CreateUiObject("Buttons", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.offsetMin = new Vector2(40, 60);
            rowRect.offsetMax = new Vector2(-40, 200);
            SetupRow(row.GetComponent<HorizontalLayoutGroup>(), 24);

            Button pause = CreateControlButton(row.transform, "PauseButton", EditorUiFactory.ButtonColor, 140);
            Button newGame = CreateControlButton(row.transform, "NewGameButton", EditorUiFactory.ButtonColor, 140);
            Button back = CreateControlButton(row.transform, "BackButton", EditorUiFactory.ButtonColor, 140);

            TetrisHud hud = root.GetComponent<TetrisHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_scoreText").objectReferenceValue = score;
            so.FindProperty("_bestText").objectReferenceValue = best;
            so.FindProperty("_linesText").objectReferenceValue = lines;
            so.FindProperty("_levelText").objectReferenceValue = level;
            so.FindProperty("_pausedText").objectReferenceValue = playfield.PausedText;
            so.FindProperty("_pausedPanel").objectReferenceValue = playfield.PausedPanel;
            so.FindProperty("_nextLabel").objectReferenceValue = playfield.NextLabel;
            so.FindProperty("_leftButton").objectReferenceValue = leftHold;
            so.FindProperty("_rightButton").objectReferenceValue = rightHold;
            so.FindProperty("_downButton").objectReferenceValue = downHold;
            so.FindProperty("_rotateButton").objectReferenceValue = rotate;
            so.FindProperty("_pauseButton").objectReferenceValue = pause;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static void SetupRow(HorizontalLayoutGroup layout, float spacing)
        {
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        private static Button CreateControlButton(Transform parent, string name, Color tint, float height)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, height, tint);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 50);
            return button;
        }

        private static TetrisResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(TetrisResultPanel));
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

            TetrisResultPanel panel = root.GetComponent<TetrisResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static TetrisStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(TetrisStartPanel));
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

            TetrisStartPanel panel = root.GetComponent<TetrisStartPanel>();
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

        private static void CreateController(TetrisStartPanel start, Playfield playfield, TetrisHud hud, TetrisResultPanel result)
        {
            GameObject go = new GameObject("TetrisGameController", typeof(TetrisGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<TetrisGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_boardView").objectReferenceValue = playfield.Board;
            so.FindProperty("_nextView").objectReferenceValue = playfield.Next;
            so.FindProperty("_touchArea").objectReferenceValue = playfield.Touch;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Tetris";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.tetris";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
