using MobileParadise.Core.UI;
using MobileParadise.Games.Blackjack.View;
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
    /// Builds the whole Blackjack_Game scene (start screen, table, buttons, out-of-chips panel),
    /// wires it to BlackjackGameController and adds the game to the build and to the main menu.
    /// Safe to run again: it rebuilds the scene.
    /// </summary>
    public static class BlackjackSceneBuilder
    {
        private const string SceneName = "Blackjack_Game";
        private const string ScenePath = "Assets/Games/Blackjack/Scenes/" + SceneName + ".unity";
        private const string IconPath = "Assets/Games/Blackjack/Art/icon.png";
        private static readonly string[] SuitPaths =
        {
            "Assets/Games/Solitaire/Art/suit_clubs.png",
            "Assets/Games/Solitaire/Art/suit_diamonds.png",
            "Assets/Games/Solitaire/Art/suit_hearts.png",
            "Assets/Games/Solitaire/Art/suit_spades.png"
        };

        private static readonly Color FeltColor = new Color(0.16f, 0.42f, 0.28f);

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        [MenuItem("Mobile Paradise/Blackjack/Build Blackjack Scene")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorUiFactory.PrepareArt();
            ImportArt();
            EditorProjectUtil.EnsureFolder("Assets/Games/Blackjack/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorUiFactory.CreateCamera();
            Canvas canvas = EditorUiFactory.CreateCanvas();
            RectTransform root = EditorUiFactory.CreateSafeArea(canvas);

            CreateTitle(root);
            BlackjackTableView tableView = CreateTable(root);
            BlackjackHud hud = CreateHud(root);
            BlackjackResultPanel resultPanel = CreateResultPanel(root);
            BlackjackStartPanel startPanel = CreateStartPanel(root);
            CreateController(startPanel, tableView, hud, resultPanel);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorProjectUtil.AddSceneToBuild(ScenePath, false);
            AddToCatalog();
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: Blackjack_Game scene built and added to the main menu.");
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
            TextMeshProUGUI title = EditorUiFactory.CreateText(parent, "Title", "Blackjack", 96, EditorUiFactory.TextLight);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rect = title.rectTransform;
            // Stops before the music note and gear buttons (350 px on the right).
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.offsetMin = new Vector2(60, -190);
            rect.offsetMax = new Vector2(-380, -40);
            LocalizedText.Bind(title, "game.blackjack");
        }

        /// <summary>
        /// The green table sits between the chips panel and the buttons; the holder (fixed size) shrinks to fit.
        /// The table view creates the cards inside the holder.
        /// </summary>
        private static BlackjackTableView CreateTable(Transform parent)
        {
            GameObject area = EditorUiFactory.CreateUiObject("TableArea", parent, typeof(BlackjackTableView));
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0, 540);
            areaRect.offsetMax = new Vector2(0, -390);

            GameObject holder = EditorUiFactory.CreateUiObject("TableHolder", area.transform, typeof(FitInParent));
            RectTransform holderRect = holder.GetComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(1032, 1310);
            holderRect.anchoredPosition = Vector2.zero;
            holder.GetComponent<FitInParent>().SetPadding(Vector2.zero);

            // The felt goes first so it is drawn behind everything.
            GameObject felt = EditorUiFactory.CreatePanel(holder.transform, "Felt", FeltColor);
            EditorUiFactory.Stretch(felt.GetComponent<RectTransform>());
            felt.GetComponent<Image>().raycastTarget = false;

            TextMeshProUGUI dealerLabel = CreateTableText(holder.transform, "DealerLabel", 56, -30f, 90f);
            TextMeshProUGUI message = CreateTableText(holder.transform, "Message", 80, -470f, 220f);
            TextMeshProUGUI playerLabel = CreateTableText(holder.transform, "PlayerLabel", 56, -710f, 90f);

            BlackjackTableView view = area.GetComponent<BlackjackTableView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_holder").objectReferenceValue = holderRect;
            so.FindProperty("_frameSprite").objectReferenceValue = EditorUiFactory.LoadSprite(EditorUiFactory.FramePath);
            so.FindProperty("_dealerLabel").objectReferenceValue = dealerLabel;
            so.FindProperty("_playerLabel").objectReferenceValue = playerLabel;
            so.FindProperty("_messageText").objectReferenceValue = message;

            // The suits come from the Solitaire art.
            SerializedProperty suits = so.FindProperty("_suitSprites");
            suits.arraySize = SuitPaths.Length;
            for (int i = 0; i < SuitPaths.Length; i++)
            {
                suits.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SuitPaths[i]);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        // A light text on the felt: a band across the table, 'top' pixels from its top edge.
        private static TextMeshProUGUI CreateTableText(Transform parent, string name, float size, float top, float height)
        {
            TextMeshProUGUI text = EditorUiFactory.CreateText(parent, name, "", size, EditorUiFactory.TextLight);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(30, top - height);
            rect.offsetMax = new Vector2(-30, top);
            return text;
        }

        private static BlackjackHud CreateHud(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("Hud", parent, typeof(BlackjackHud));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());

            // Chips (left) and bet (right)
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

            TextMeshProUGUI chips = EditorUiFactory.CreateText(panel.transform, "ChipsText", "", 64);
            TextMeshProUGUI bet = EditorUiFactory.CreateText(panel.transform, "BetText", "", 64);

            // Row of bet buttons, above the Back button
            GameObject betRow = CreateButtonRow(root.transform, "BetRow", 200, 330);
            Button minusBig = CreateHudButton(betRow.transform, "Minus50Button", "-50", EditorUiFactory.HardColor);
            Button minusSmall = CreateHudButton(betRow.transform, "Minus10Button", "-10", EditorUiFactory.HardColor);
            Button plusSmall = CreateHudButton(betRow.transform, "Plus10Button", "+10", EditorUiFactory.EasyColor);
            Button plusBig = CreateHudButton(betRow.transform, "Plus50Button", "+50", EditorUiFactory.EasyColor);

            // Deal, or Hit / Stand / Double (same place, only one of them is shown)
            GameObject dealRow = CreateButtonRow(root.transform, "DealRow", 350, 510);
            Button deal = CreateHudButton(dealRow.transform, "DealButton", "Deal", EditorUiFactory.EasyColor);

            GameObject actionRow = CreateButtonRow(root.transform, "ActionRow", 350, 510);
            Button hit = CreateHudButton(actionRow.transform, "HitButton", "Hit", EditorUiFactory.EasyColor);
            Button stand = CreateHudButton(actionRow.transform, "StandButton", "Stand", EditorUiFactory.MediumColor);
            Button doubleDown = CreateHudButton(actionRow.transform, "DoubleButton", "Double", EditorUiFactory.ButtonColor);

            GameObject backRow = CreateButtonRow(root.transform, "BackRow", 60, 180);
            Button back = CreateHudButton(backRow.transform, "BackButton", "Back", EditorUiFactory.ButtonColor);

            BlackjackHud hud = root.GetComponent<BlackjackHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_chipsText").objectReferenceValue = chips;
            so.FindProperty("_betText").objectReferenceValue = bet;
            so.FindProperty("_betRow").objectReferenceValue = betRow;
            so.FindProperty("_minusBigButton").objectReferenceValue = minusBig;
            so.FindProperty("_minusSmallButton").objectReferenceValue = minusSmall;
            so.FindProperty("_plusSmallButton").objectReferenceValue = plusSmall;
            so.FindProperty("_plusBigButton").objectReferenceValue = plusBig;
            so.FindProperty("_dealRow").objectReferenceValue = dealRow;
            so.FindProperty("_dealButton").objectReferenceValue = deal;
            so.FindProperty("_actionRow").objectReferenceValue = actionRow;
            so.FindProperty("_hitButton").objectReferenceValue = hit;
            so.FindProperty("_standButton").objectReferenceValue = stand;
            so.FindProperty("_doubleButton").objectReferenceValue = doubleDown;
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

        private static Button CreateHudButton(Transform parent, string name, string label, Color tint)
        {
            Button button = EditorUiFactory.CreateButton(parent, name, label, 120, tint);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 56);
            return button;
        }

        private static BlackjackResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("ResultPanel", parent, typeof(Image), typeof(BlackjackResultPanel));
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

            BlackjackResultPanel panel = root.GetComponent<BlackjackResultPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_againButton").objectReferenceValue = again;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static BlackjackStartPanel CreateStartPanel(Transform parent)
        {
            GameObject root = EditorUiFactory.CreateUiObject("StartPanel", parent, typeof(Image), typeof(BlackjackStartPanel));
            EditorUiFactory.Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = EditorUiFactory.SceneBackground;

            // Fixed-size content that shrinks to fit the free area (below the settings buttons).
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
            contentRect.sizeDelta = new Vector2(960, 800);
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

            Button continueButton = EditorUiFactory.CreateButton(content.transform, "ContinueButton", "Continue", 150, EditorUiFactory.PanelColor);
            Button newGame = EditorUiFactory.CreateButton(content.transform, "NewGameButton", "New game", 170, EditorUiFactory.EasyColor);
            Button menu = EditorUiFactory.CreateButton(content.transform, "MenuButton", "Back", 120);
            menu.gameObject.AddComponent<BackToMenuButton>();

            BlackjackStartPanel panel = root.GetComponent<BlackjackStartPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_continueButton").objectReferenceValue = continueButton;
            so.FindProperty("_newGameButton").objectReferenceValue = newGame;
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return panel;
        }

        private static void CreateController(BlackjackStartPanel start, BlackjackTableView table, BlackjackHud hud, BlackjackResultPanel result)
        {
            GameObject go = new GameObject("BlackjackGameController", typeof(BlackjackGameController));

            SerializedObject so = new SerializedObject(go.GetComponent<BlackjackGameController>());
            so.FindProperty("_startPanel").objectReferenceValue = start;
            so.FindProperty("_tableView").objectReferenceValue = table;
            so.FindProperty("_hud").objectReferenceValue = hud;
            so.FindProperty("_resultPanel").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToCatalog()
        {
            GameCatalog catalog = EditorProjectUtil.LoadOrCreateCatalog();
            if (catalog.Games.Exists(g => g.SceneName == SceneName)) return;

            GameEntry entry = new GameEntry();
            entry.DisplayName = "Blackjack";
            entry.SceneName = SceneName;
            entry.TitleKey = "game.blackjack";
            entry.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            catalog.Games.Add(entry);
            EditorUtility.SetDirty(catalog);
        }
    }
}
