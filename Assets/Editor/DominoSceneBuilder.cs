using MobileParadise.Core.UI;
using MobileParadise.Games.Domino.View;
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
    /// Builds the whole Domino_Game scene (start screen, chain, hand, buttons, result panel),
    /// wires it to DominoGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class DominoSceneBuilder
    {
        private const string SceneName = "Domino_Game";
        private const string ScenePath = "Assets/Games/Domino/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Domino/Art/icon.png";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color FeltColor = new Color(0.20f, 0.45f, 0.32f);

        [MenuItem("Mobile Paradise/Dominoes/Build Dominoes Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Domino/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            DominoChainView chainView = CreateChain(root);
            DominoHandView handView = CreateHand(root);
            DominoHud hud = CreateHud(root);
            DominoResultPanel resultPanel = CreateResultPanel(root);
            DominoStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, chainView, handView, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Domino_Game scene built and added to the main menu.");
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
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Dominoes", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stops before the music note and gear buttons (350 px on the right).
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.domino");
        }

        /// <summary>
        /// The green table where the chain of tiles is laid out; the holder (fixed size) shrinks to fit.
        /// </summary>
        private static DominoChainView CreateChain(Transform parent)
        {
            GameObject table = EditorUiFactory.CreatePanel(parent, "ChainTable", FeltColor);
            RectTransform tableRect = table.GetComponent<RectTransform>();
            tableRect.anchorMin = Vector2.zero;
            tableRect.anchorMax = Vector2.one;
            tableRect.offsetMin = new Vector2(40, 745);
            tableRect.offsetMax = new Vector2(-40, -470);
            table.GetComponent<Image>().raycastTarget = false;

            GameObject holder = EditorUiFactory.CreateUiObject("ChainHolder", table.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(1000, 340);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(new Vector2(30, 30));

            DominoChainView view = table.AddComponent<DominoChainView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        /// <summary>
        /// The player's tiles in a grid at the bottom, above the buttons.
        /// </summary>
        private static DominoHandView CreateHand(Transform parent)
        {
            GameObject panel = EditorUiFactory.CreatePanel(parent, "HandPanel", EditorUiFactory.PanelColor);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.offsetMin = new Vector2(40, 215);
            panelRect.offsetMax = new Vector2(-40, 575);
            panel.GetComponent<Image>().raycastTarget = false;

            GameObject grid = EditorUiFactory.CreateUiObject("HandGrid", panel.transform, typeof(GridLayoutGroup));
            RectTransform gridRect = grid.GetComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = new Vector2(15, 15);
            gridRect.offsetMax = new Vector2(-15, -15);

            GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.constraint = GridLayoutGroup.Constraint.Flexible;
            layout.cellSize = new Vector2(120, 228);
            layout.spacing = new Vector2(10, 10);

            DominoHandView view = panel.AddComponent<DominoHandView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_container").objectReferenceValue = gridRect;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static DominoHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(DominoHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Whose turn (left) and tile counts (right)
            GameObject info = CreateInfoPanel(root.transform, "InfoPanel", -370, -210);
            TextMeshProUGUI status = EditorUiFactory.CreateText(info.transform, "StatusText", "", 52);
            TextMeshProUGUI counts = EditorUiFactory.CreateText(info.transform, "CountText", "", 48);

            // Score (left) and open ends (right)
            GameObject scorePanel = CreateInfoPanel(root.transform, "ScorePanel", -455, -385);
            TextMeshProUGUI score = EditorUiFactory.CreateText(scorePanel.transform, "ScoreText", "", 44);
            TextMeshProUGUI ends = EditorUiFactory.CreateText(scorePanel.transform, "EndsText", "", 44);

            // Left / Right: only visible when the player must choose a side
            GameObject choice = EditorUiFactory.CreateUiObject("EndChoice", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform choiceRect = choice.GetComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0, 0);
            choiceRect.anchorMax = new Vector2(1, 0);
            choiceRect.pivot = new Vector2(0.5f, 0);
            choiceRect.offsetMin = new Vector2(40, 590);
            choiceRect.offsetMax = new Vector2(-40, 730);
            SetupRow(choice.GetComponent<HorizontalLayoutGroup>());
            Button left = CreateHudButton(choice.transform, "LeftButton", EditorUiFactory.MediumColor);
            Button right = CreateHudButton(choice.transform, "RightButton", EditorUiFactory.MediumColor);

            // Draw-or-Pass / New game / Back
            GameObject row = EditorUiFactory.CreateUiObject("Buttons", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.offsetMin = new Vector2(40, 60);
            rowRect.offsetMax = new Vector2(-40, 200);
            SetupRow(row.GetComponent<HorizontalLayoutGroup>());
            Button action = CreateHudButton(row.transform, "ActionButton", EditorUiFactory.EasyColor);
            Button newGame = CreateHudButton(row.transform, "NewGameButton", EditorUiFactory.ButtonColor);
            Button back = CreateHudButton(row.transform, "BackButton", EditorUiFactory.ButtonColor);

            DominoHud hud = root.GetComponent<DominoHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_statusText").objectReferenceValue = status;
            so.FindProperty("_countText").objectReferenceValue = counts;
            so.FindProperty("_scoreText").objectReferenceValue = score;
            so.FindProperty("_endsText").objectReferenceValue = ends;
            so.FindProperty("_actionButton").objectReferenceValue = action;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.FindProperty("_endChoice").objectReferenceValue = choice;
            so.FindProperty("_leftButton").objectReferenceValue = left;
            so.FindProperty("_rightButton").objectReferenceValue = right;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        // A wide panel at the top with two texts side by side.
        private static GameObject CreateInfoPanel(Transform parent, string name, float bottom, float top)
        {
            GameObject panel = EditorUiFactory.CreatePanel(parent, name, EditorUiFactory.PanelColor);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(60, bottom);
            rect.offsetMax = new Vector2(-60, top);

            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 6, 6);
            layout.spacing = 20;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return panel;
        }

        private static void SetupRow(HorizontalLayoutGroup layout)
        {
            layout.spacing = 24;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        private static Button CreateHudButton(Transform parent, string name, Color tint)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, name, 140, tint);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 52);
            return button;
        }

        private static DominoResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(DominoResultPanel));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = DimColor;

            GameObject box = EditorUiFactory.CreatePanel(root.transform, "Box", EditorUiFactory.PanelColor);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(900, 860);
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
            TextMeshProUGUI detail = EditorUiFactory.CreateText(box.transform, "Detail", "", 52);
            EditorUiFactory.SetPreferredHeight(detail.gameObject, 220);
            Button next = EditorUiFactory.CreateButton(box.transform, "NextButton", "", 150, EditorUiFactory.EasyColor);
            Button back = EditorUiFactory.CreateButton(box.transform, "BackButton", "", 130);

            DominoResultPanel panel = root.GetComponent<DominoResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_detail").objectReferenceValue = detail;
            so.FindProperty("_nextButton").objectReferenceValue = next;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static DominoStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(DominoStartPanel));
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
            contentRect.sizeDelta = new Vector2(960, 700);
            contentRect.anchoredPosition = Vector2.zero;
            content.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = EditorUiFactory.CreateText(content.transform, "Title", "", 100, EditorUiFactory.TextLight);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 150);

            DominoStartPanel panel = root.GetComponent<DominoStartPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;

            // Shown only when there is a saved match (the start panel decides).
            Button continueButton = EditorUiFactory.CreateButton(content.transform, "ContinueButton", "Continue", 150, EditorUiFactory.PanelColor);
            so.FindProperty("_continueButton").objectReferenceValue = continueButton;

            Button newGame = EditorUiFactory.CreateButton(content.transform, "NewGameButton", "New game", 170, EditorUiFactory.EasyColor);
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;

            Button menu = EditorUiFactory.CreateButton(content.transform, "MenuButton", "Back", 120);
            menu.gameObject.AddComponent<BackToMenuButton>();
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static void CreateController(DominoStartPanel start, DominoChainView chain, DominoHandView hand,
            DominoHud hud, DominoResultPanel result)
        {
            GameObject go = new GameObject("DominoGameController", typeof(DominoGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<DominoGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_chainView").objectReferenceValue = chain;
            so.FindProperty("_handView").objectReferenceValue = hand;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Dominoes";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.domino";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
