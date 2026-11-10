using MobileParadise.Core.UI;
using MobileParadise.Games.Chess.View;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Builds the whole Chess_Game scene (board, status line, buttons, promotion pop-up,
    /// difficulty screen) and wires it to ChessGameController.
    /// Run "Build Piece Sprite Sets" first. Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class ChessSceneBuilder
    {
        private const string ScenePath = "Assets/Games/Chess/Scenes/Chess_Game.unity";
        private const string PieceSetPath = "Assets/Games/Chess/PieceSets/Wood.asset";
        private const string MarbleSetPath = "Assets/Games/Chess/PieceSets/Marble.asset";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

        [MenuItem("Mobile Paradise/Chess/Build Chess Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;

            if (AssetDatabase.LoadAssetAtPath<PieceSpriteSet>(PieceSetPath) == null)
            {
                EditorUtility.DisplayDialog("Piece set missing",
                    "Run 'Mobile Paradise > Chess > Build Piece Sprite Sets' first.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Chess/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            ChessBoardView boardView = CreateBoard(root);
            ChessHud hud = CreateHud(root);
            PromotionPanel promotionPanel = CreatePromotionPanel(root);
            DifficultyPanel difficultyPanel = CreateDifficultyPanel(root);
            CreateController(boardView, hud, promotionPanel, difficultyPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Chess_Game scene built.");
        }

        // ---- Pieces of the scene ----------------------------------------------

        /// <summary>
        /// The board sits in an area between the status line and the buttons. A fixed-size holder
        /// (frame + board) shrinks to fit that area, so it works on every screen shape.
        /// </summary>
        private static ChessBoardView CreateBoard(Transform parent)
        {
            // Loaded here, after the new scene exists: NewScene unloads assets nothing references yet,
            // which would save an empty reference.
            PieceSpriteSet pieceSet = AssetDatabase.LoadAssetAtPath<PieceSpriteSet>(PieceSetPath);

            GameObject area = EditorUiFactory.CreateUiObject("BoardArea", parent);
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
            holderRect.sizeDelta = new Vector2(1080, 1080);
            holderRect.anchoredPosition = Vector2.zero;

            GameObject frame = EditorUiFactory.CreatePanel(holder.transform, "BoardFrame", EditorUiFactory.WoodColor);
            EditorUiFactory.Stretch(frame.GetComponent<RectTransform>());

            GameObject go = EditorUiFactory.CreateUiObject("Board", holder.transform, typeof(ChessBoardView));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1024, 1024);
            rect.anchoredPosition = Vector2.zero;

            ChessBoardView view = go.GetComponent<ChessBoardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_boardRoot").objectReferenceValue = rect;
            so.FindProperty("_pieceSet").objectReferenceValue = pieceSet;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        /// <summary>
        /// Game name at the top left, bound to the "game.chess" key so it follows the language.
        /// </summary>
        private static void CreateTitle(Transform parent)
        {
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Chess", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(700, 150);
            rect.anchoredPosition = new Vector2(60, -40);
            LocalizedText.Bind(title, "game.chess");
        }

        private static ChessHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(ChessHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            GameObject statusPanel = EditorUiFactory.CreatePanel(root.transform, "StatusPanel", EditorUiFactory.PanelColor);
            RectTransform statusPanelRect = statusPanel.GetComponent<RectTransform>();
            statusPanelRect.anchorMin = new Vector2(0, 1);
            statusPanelRect.anchorMax = new Vector2(1, 1);
            statusPanelRect.pivot = new Vector2(0.5f, 1);
            statusPanelRect.offsetMin = new Vector2(60, -370);
            statusPanelRect.offsetMax = new Vector2(-60, -210);

            TextMeshProUGUI status = EditorUiFactory.CreateText(statusPanel.transform, "StatusText", "", 64);
            EditorUiFactory.Stretch(status.rectTransform);

            GameObject row = EditorUiFactory.CreateUiObject("Buttons", root.transform, typeof(HorizontalLayoutGroup));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.offsetMin = new Vector2(40, 60);
            rowRect.offsetMax = new Vector2(-40, 260);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 30;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            Button undo = CreateHudButton(row.transform, "UndoButton");
            Button newGame = CreateHudButton(row.transform, "NewGameButton");
            Button back = CreateHudButton(row.transform, "BackButton");

            ChessHud hud = root.GetComponent<ChessHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_statusText").objectReferenceValue = status;
            so.FindProperty("_undoButton").objectReferenceValue = undo;
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

        private static PromotionPanel CreatePromotionPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("PromotionPanel", parent, typeof(Image), typeof(PromotionPanel));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = DimColor;

            GameObject box = EditorUiFactory.CreateUiObject("Box", root.transform, typeof(Image), typeof(VerticalLayoutGroup));
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(960, 440);
            box.AddComponent<FitInParent>();
            EditorUiFactory.ApplyFrame(box.GetComponent<Image>(), EditorUiFactory.PanelColor);

            VerticalLayoutGroup boxLayout = box.GetComponent<VerticalLayoutGroup>();
            boxLayout.padding = new RectOffset(30, 30, 30, 30);
            boxLayout.spacing = 20;
            boxLayout.childControlWidth = true;
            boxLayout.childControlHeight = true;
            boxLayout.childForceExpandWidth = true;
            boxLayout.childForceExpandHeight = false;

            TextMeshProUGUI title = EditorUiFactory.CreateText(box.transform, "Title", "", 64);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 100);

            GameObject choices = EditorUiFactory.CreateUiObject("Choices", box.transform, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            EditorUiFactory.SetPreferredHeight(choices, 220);
            HorizontalLayoutGroup choicesLayout = choices.GetComponent<HorizontalLayoutGroup>();
            choicesLayout.spacing = 20;
            choicesLayout.childControlWidth = true;
            choicesLayout.childControlHeight = true;
            choicesLayout.childForceExpandWidth = true;
            choicesLayout.childForceExpandHeight = true;

            PromotionPanel panel = root.GetComponent<PromotionPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;

            SerializedProperty buttons = so.FindProperty("_buttons");
            SerializedProperty icons = so.FindProperty("_icons");
            buttons.arraySize = 4;
            icons.arraySize = 4;

            for (int i = 0; i < 4; i++)
            {
                GameObject choice = EditorUiFactory.CreateUiObject("Choice" + i, choices.transform, typeof(Image), typeof(Button));
                Image background = choice.GetComponent<Image>();
                EditorUiFactory.ApplyFrame(background, EditorUiFactory.ButtonColor);
                choice.GetComponent<Button>().targetGraphic = background;

                GameObject iconObject = EditorUiFactory.CreateUiObject("Icon", choice.transform, typeof(Image));
                RectTransform iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(20, 20);
                iconRect.offsetMax = new Vector2(-20, -20);
                Image icon = iconObject.GetComponent<Image>();
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                buttons.GetArrayElementAtIndex(i).objectReferenceValue = choice.GetComponent<Button>();
                icons.GetArrayElementAtIndex(i).objectReferenceValue = icon;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static DifficultyPanel CreateDifficultyPanel(Transform parent)
        {
            // Loaded here, after the new scene exists (see CreateBoard).
            PieceSpriteSet[] pieceSets =
            {
                AssetDatabase.LoadAssetAtPath<PieceSpriteSet>(PieceSetPath),
                AssetDatabase.LoadAssetAtPath<PieceSpriteSet>(MarbleSetPath)
            };

            GameObject root = EditorUiFactory.CreateUiObject("DifficultyPanel", parent, typeof(Image), typeof(DifficultyPanel));
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
            contentRect.sizeDelta = new Vector2(960, 1650);
            contentRect.anchoredPosition = Vector2.zero;
            content.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = EditorUiFactory.CreateText(content.transform, "Title", "", 80, EditorUiFactory.TextLight);
            EditorUiFactory.SetPreferredHeight(title.gameObject, 110);

            DifficultyPanel panel = root.GetComponent<DifficultyPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;

            // Shown only when there is a saved game (the start panel decides).
            Button continueButton = EditorUiFactory.CreateButton(content.transform, "ContinueButton", "Continue", 130, EditorUiFactory.PanelColor);
            so.FindProperty("_continueButton").objectReferenceValue = continueButton;

            SerializedProperty levelButtons = so.FindProperty("_levelButtons");
            levelButtons.arraySize = 3;
            string[] names = { "EasyButton", "MediumButton", "HardButton" };
            Color[] tints = { EditorUiFactory.EasyColor, EditorUiFactory.MediumColor, EditorUiFactory.HardColor };
            for (int i = 0; i < 3; i++)
            {
                Button button = EditorUiFactory.CreateButton(content.transform, names[i], names[i], 150, tints[i]);
                levelButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
            }

            // Look: piece set
            so.FindProperty("_piecesLabel").objectReferenceValue = EditorUiFactory.CreateSectionLabel(content.transform, "PiecesLabel");
            GameObject pieceRow = EditorUiFactory.CreateRow(content.transform, "PieceRow", 120);
            SerializedProperty pieceSetsProp = so.FindProperty("_pieceSets");
            SerializedProperty pieceButtons = so.FindProperty("_pieceButtons");
            SerializedProperty pieceIcons = so.FindProperty("_pieceIcons");
            pieceSetsProp.arraySize = 2;
            pieceButtons.arraySize = 2;
            pieceIcons.arraySize = 2;
            string[] pieceNames = { "WoodButton", "MarbleButton" };
            for (int i = 0; i < 2; i++)
            {
                Image icon;
                Button button = EditorUiFactory.CreateIconButton(pieceRow.transform, pieceNames[i], out icon);
                pieceSetsProp.GetArrayElementAtIndex(i).objectReferenceValue = pieceSets[i];
                pieceButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
                pieceIcons.GetArrayElementAtIndex(i).objectReferenceValue = icon;
            }

            // Look: board colors (two-tone swatches)
            so.FindProperty("_boardLabel").objectReferenceValue = EditorUiFactory.CreateSectionLabel(content.transform, "BoardLabel");
            GameObject boardRow = EditorUiFactory.CreateRow(content.transform, "BoardRow", 120);
            SerializedProperty boardButtons = so.FindProperty("_boardButtons");
            SerializedProperty boardSwatches = so.FindProperty("_boardSwatches");
            int themeCount = BoardTheme.All.Length;
            boardButtons.arraySize = themeCount;
            boardSwatches.arraySize = themeCount;
            for (int i = 0; i < themeCount; i++)
            {
                Button button = EditorUiFactory.CreateButton(boardRow.transform, "Board" + i + "Button", "", 120);
                Object.DestroyImmediate(button.transform.Find("Label").gameObject);

                GameObject swatchObject = EditorUiFactory.CreateUiObject("Swatch", button.transform, typeof(Image));
                RectTransform swatchRect = swatchObject.GetComponent<RectTransform>();
                swatchRect.anchorMin = new Vector2(0, 0);
                swatchRect.anchorMax = new Vector2(0.5f, 1);
                swatchRect.offsetMin = new Vector2(18, 18);
                swatchRect.offsetMax = new Vector2(0, -18);
                Image swatch = swatchObject.GetComponent<Image>();
                swatch.raycastTarget = false;

                boardButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
                boardSwatches.GetArrayElementAtIndex(i).objectReferenceValue = swatch;
            }

            // Which side the player takes
            so.FindProperty("_colorLabel").objectReferenceValue = EditorUiFactory.CreateSectionLabel(content.transform, "ColorLabel");
            GameObject colorRow = EditorUiFactory.CreateRow(content.transform, "ColorRow", 120);
            SerializedProperty colorButtons = so.FindProperty("_colorButtons");
            SerializedProperty colorIcons = so.FindProperty("_colorIcons");
            colorButtons.arraySize = 2;
            colorIcons.arraySize = 2;
            string[] colorNames = { "WhiteButton", "BlackButton" };
            for (int i = 0; i < 2; i++)
            {
                Image icon;
                Button button = EditorUiFactory.CreateIconButton(colorRow.transform, colorNames[i], out icon);
                colorButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
                colorIcons.GetArrayElementAtIndex(i).objectReferenceValue = icon;
            }

            Button menu = EditorUiFactory.CreateButton(content.transform, "MenuButton", "Back", 110);
            menu.gameObject.AddComponent<BackToMenuButton>();
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static void CreateController(ChessBoardView board, ChessHud hud, PromotionPanel promotion, DifficultyPanel difficulty)
        {
            GameObject go = new GameObject("ChessGameController", typeof(ChessGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<ChessGameController>());
            so.FindProperty("_boardView").objectReferenceValue = board;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_promotionPanel").objectReferenceValue = promotion;
            so.FindProperty("_difficultyPanel").objectReferenceValue = difficulty;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
