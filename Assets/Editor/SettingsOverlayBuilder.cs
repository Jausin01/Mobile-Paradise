using MobileParadise.Core.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// Builds Assets/Core/Resources/SettingsOverlay.prefab: the gear button (top right)
    /// and the settings window. SettingsOverlayBootstrap adds it to every scene at runtime.
    /// </summary>
    public static class SettingsOverlayBuilder
    {
        private const string PrefabPath = "Assets/Core/Resources/SettingsOverlay.prefab";

        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.65f);
        private static readonly Color SliderBackColor = new Color(0.35f, 0.25f, 0.18f);

        [MenuItem("Mobile Paradise/4. Build Settings Overlay")]
        public static void Build()
        {
            if (!EditorUiFactory.EnsureTmpReady()) return;

            EditorUiFactory.PrepareArt();
            EditorProjectUtil.EnsureFolder("Assets/Core/Resources");

            GameObject root = EditorUiFactory.CreateUiObject("SettingsOverlay", null,
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(SettingsPanel), typeof(MusicPanel), typeof(HelpPanel));

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;   // above every scene canvas

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform safeArea = EditorUiFactory.CreateSafeArea(root.GetComponent<Canvas>());
            Button gear = CreateGearButton(safeArea);
            Button note = CreateNoteButton(safeArea);
            GameObject window = CreateWindow(root.transform);
            GameObject musicWindow = CreateMusicWindow(root.transform);
            Button help = CreateHelpButton(safeArea);
            GameObject helpWindow = CreateHelpWindow(root.transform);

            Transform t = window.transform;
            SerializedObject so = new SerializedObject(root.GetComponent<SettingsPanel>());
            so.FindProperty("_panel").objectReferenceValue = window;
            so.FindProperty("_openButton").objectReferenceValue = gear;
            so.FindProperty("_closeButton").objectReferenceValue = t.Find("Window/CloseButton").GetComponent<Button>();
            so.FindProperty("_portugueseButton").objectReferenceValue = t.Find("Window/LanguageRow/PortugueseButton").GetComponent<Button>();
            so.FindProperty("_englishButton").objectReferenceValue = t.Find("Window/LanguageRow/EnglishButton").GetComponent<Button>();
            so.FindProperty("_fontText").objectReferenceValue = t.Find("Window/FontLabel").GetComponent<TMP_Text>();
            SerializedProperty fontButtons = so.FindProperty("_fontButtons");
            fontButtons.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                fontButtons.GetArrayElementAtIndex(i).objectReferenceValue = t.Find("Window/FontRow/Font" + i + "Button").GetComponent<Button>();
            }
            so.FindProperty("_titleText").objectReferenceValue = t.Find("Window/Title").GetComponent<TMP_Text>();
            so.FindProperty("_languageText").objectReferenceValue = t.Find("Window/LanguageLabel").GetComponent<TMP_Text>();
            so.FindProperty("_closeText").objectReferenceValue = t.Find("Window/CloseButton/Label").GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            WireMusicPanel(root.GetComponent<MusicPanel>(), musicWindow, note);

            WireHelpPanel(root.GetComponent<HelpPanel>(), helpWindow, help);

            window.SetActive(false);
            musicWindow.SetActive(false);
            helpWindow.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();

            Debug.Log("Mobile Paradise: settings overlay built (" + PrefabPath + "). It appears in every scene when you press Play.");
        }

        private static Button CreateGearButton(Transform parent)
        {
            Button button = EditorUiFactory.CreateButton(parent, "GearButton", "", 150);
            Object.DestroyImmediate(button.transform.Find("Label").gameObject);

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(150, 150);
            rect.anchoredPosition = new Vector2(-30, -30);

            GameObject iconObject = EditorUiFactory.CreateUiObject("Icon", button.transform, typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            EditorUiFactory.Stretch(iconRect);
            iconRect.offsetMin = new Vector2(16, 16);
            iconRect.offsetMax = new Vector2(-16, -16);

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = EditorUiFactory.LoadSprite(EditorUiFactory.GearPath);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return button;
        }


        private static Button CreateNoteButton(Transform parent)
        {
            Button button = EditorUiFactory.CreateButton(parent, "NoteButton", "", 150);

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(150, 150);
            rect.anchoredPosition = new Vector2(-200, -30);   // left of the gear (gear is at -30, 150 wide)

            AddIcon(button, EditorUiFactory.NotePath, 16);
            return button;
        }

        /// <summary>
        /// Replaces the button's text with a centered picture.
        /// </summary>
        private static Image AddIcon(Button button, string spritePath, float margin)
        {
            Transform label = button.transform.Find("Label");
            if (label != null) Object.DestroyImmediate(label.gameObject);

            GameObject iconObject = EditorUiFactory.CreateUiObject("Icon", button.transform, typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            EditorUiFactory.Stretch(iconRect);
            iconRect.offsetMin = new Vector2(margin, margin);
            iconRect.offsetMax = new Vector2(-margin, -margin);

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = EditorUiFactory.LoadSprite(spritePath);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return icon;
        }

        private static GameObject CreateMusicWindow(Transform parent)
        {
            GameObject dim = EditorUiFactory.CreateUiObject("MusicDim", parent, typeof(Image));
            EditorUiFactory.Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = DimColor;

            GameObject window = EditorUiFactory.CreatePanel(dim.transform, "Window", EditorUiFactory.PanelColor);
            RectTransform rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(940, 1300);
            window.AddComponent<FitInParent>();

            VerticalLayoutGroup layout = window.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 40, 40);
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLabel(window.transform, "Title", "Music", 72, 100);
            EditorUiFactory.CreateButton(window.transform, "MusicToggleButton", "Music: on", 120);
            AddLabel(window.transform, "TrackLabel", "", 44, 90);

            GameObject row = CreateRow(window.transform, "ControlsRow", 140);
            Button restart = EditorUiFactory.CreateButton(row.transform, "RestartButton", "", 140);
            Button pause = EditorUiFactory.CreateButton(row.transform, "PauseButton", "", 140);
            Button next = EditorUiFactory.CreateButton(row.transform, "NextButton", "", 140);
            Button repeat = EditorUiFactory.CreateButton(row.transform, "RepeatButton", "", 140);
            AddIcon(restart, EditorUiFactory.RestartPath, 28);
            AddIcon(pause, EditorUiFactory.PausePath, 28).gameObject.name = "PauseIcon";
            AddIcon(next, EditorUiFactory.NextPath, 28);
            AddIcon(repeat, EditorUiFactory.RepeatPath, 28);
            AddLabel(window.transform, "RepeatLabel", "Repeat: on", 44, 60);

            AddLabel(window.transform, "MusicVolumeLabel", "Music volume", 48, 60);
            CreateSlider(window.transform, "MusicSlider");
            AddLabel(window.transform, "SfxVolumeLabel", "Effects volume", 48, 60);
            CreateSlider(window.transform, "SfxSlider");

            EditorUiFactory.CreateButton(window.transform, "CloseButton", "Close", 130);
            return dim;
        }

        private static void WireMusicPanel(MusicPanel panel, GameObject dim, Button note)
        {
            Transform t = dim.transform;
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_panel").objectReferenceValue = dim;
            so.FindProperty("_openButton").objectReferenceValue = note;
            so.FindProperty("_closeButton").objectReferenceValue = t.Find("Window/CloseButton").GetComponent<Button>();
            so.FindProperty("_closeText").objectReferenceValue = t.Find("Window/CloseButton/Label").GetComponent<TMP_Text>();
            so.FindProperty("_titleText").objectReferenceValue = t.Find("Window/Title").GetComponent<TMP_Text>();
            so.FindProperty("_musicToggleButton").objectReferenceValue = t.Find("Window/MusicToggleButton").GetComponent<Button>();
            so.FindProperty("_musicToggleText").objectReferenceValue = t.Find("Window/MusicToggleButton/Label").GetComponent<TMP_Text>();
            so.FindProperty("_trackText").objectReferenceValue = t.Find("Window/TrackLabel").GetComponent<TMP_Text>();
            so.FindProperty("_restartButton").objectReferenceValue = t.Find("Window/ControlsRow/RestartButton").GetComponent<Button>();
            so.FindProperty("_pauseButton").objectReferenceValue = t.Find("Window/ControlsRow/PauseButton").GetComponent<Button>();
            so.FindProperty("_pauseIcon").objectReferenceValue = t.Find("Window/ControlsRow/PauseButton/PauseIcon").GetComponent<Image>();
            so.FindProperty("_playSprite").objectReferenceValue = EditorUiFactory.LoadSprite(EditorUiFactory.PlayPath);
            so.FindProperty("_pauseSprite").objectReferenceValue = EditorUiFactory.LoadSprite(EditorUiFactory.PausePath);
            so.FindProperty("_nextButton").objectReferenceValue = t.Find("Window/ControlsRow/NextButton").GetComponent<Button>();
            so.FindProperty("_repeatButton").objectReferenceValue = t.Find("Window/ControlsRow/RepeatButton").GetComponent<Button>();
            so.FindProperty("_repeatText").objectReferenceValue = t.Find("Window/RepeatLabel").GetComponent<TMP_Text>();
            so.FindProperty("_musicVolumeText").objectReferenceValue = t.Find("Window/MusicVolumeLabel").GetComponent<TMP_Text>();
            so.FindProperty("_musicSlider").objectReferenceValue = t.Find("Window/MusicSlider").GetComponent<Slider>();
            so.FindProperty("_sfxVolumeText").objectReferenceValue = t.Find("Window/SfxVolumeLabel").GetComponent<TMP_Text>();
            so.FindProperty("_sfxSlider").objectReferenceValue = t.Find("Window/SfxSlider").GetComponent<Slider>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // The "?" button: left of the music note (the note is at -200, 150 wide).
        private static Button CreateHelpButton(Transform parent)
        {
            Button button = EditorUiFactory.CreateButton(parent, "HelpButton", "?", 150);
            EditorUiFactory.SetFontSize(button.GetComponentInChildren<TMP_Text>(), 110);

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(150, 150);
            rect.anchoredPosition = new Vector2(-370, -30);
            return button;
        }

        /// <summary>
        /// The how-to-play window: a title, a scrolling area (the list of games or one explanation),
        /// a "game list" button (only when an explanation was opened from the list) and Close.
        /// </summary>
        private static GameObject CreateHelpWindow(Transform parent)
        {
            GameObject dim = EditorUiFactory.CreateUiObject("HelpDim", parent, typeof(Image));
            EditorUiFactory.Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = DimColor;

            GameObject window = EditorUiFactory.CreatePanel(dim.transform, "Window", EditorUiFactory.PanelColor);
            RectTransform rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(960, 1600);
            window.AddComponent<FitInParent>();

            VerticalLayoutGroup layout = window.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLabel(window.transform, "Title", "How to play", 72, 100);
            CreateHelpScroll(window.transform);
            EditorUiFactory.CreateButton(window.transform, "ListButton", "Game list", 120, EditorUiFactory.EasyColor);
            EditorUiFactory.CreateButton(window.transform, "CloseButton", "Close", 130);
            return dim;
        }

        // The scrolling area; its content holds the body text and (hidden) the template for the game buttons.
        private static void CreateHelpScroll(Transform parent)
        {
            GameObject scrollObject = EditorUiFactory.CreateUiObject("Scroll", parent, typeof(ScrollRect));
            LayoutElement element = scrollObject.AddComponent<LayoutElement>();
            element.minHeight = 700;
            element.flexibleHeight = 1;

            GameObject viewport = EditorUiFactory.CreateUiObject("Viewport", scrollObject.transform, typeof(Image), typeof(RectMask2D));
            EditorUiFactory.Stretch(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0);   // invisible, but it receives the finger drag

            GameObject content = EditorUiFactory.CreateUiObject("Content", viewport.transform, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // The explanation: normal size, left aligned, wraps to several lines (no auto size).
            TextMeshProUGUI body = EditorUiFactory.CreateText(content.transform, "BodyText", "", 46);
            body.enableAutoSizing = false;
            body.fontSize = 46;
            body.alignment = TextAlignmentOptions.TopLeft;

            Button template = EditorUiFactory.CreateButton(content.transform, "GameButtonTemplate", "Game", 120);
            template.gameObject.SetActive(false);   // copied for each game when the list is shown

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
        }

        private static void WireHelpPanel(HelpPanel panel, GameObject dim, Button open)
        {
            Transform t = dim.transform;
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_panel").objectReferenceValue = dim;
            so.FindProperty("_openButton").objectReferenceValue = open;
            so.FindProperty("_closeButton").objectReferenceValue = t.Find("Window/CloseButton").GetComponent<Button>();
            so.FindProperty("_closeText").objectReferenceValue = t.Find("Window/CloseButton/Label").GetComponent<TMP_Text>();
            so.FindProperty("_listButton").objectReferenceValue = t.Find("Window/ListButton").GetComponent<Button>();
            so.FindProperty("_listButtonText").objectReferenceValue = t.Find("Window/ListButton/Label").GetComponent<TMP_Text>();
            so.FindProperty("_titleText").objectReferenceValue = t.Find("Window/Title").GetComponent<TMP_Text>();
            so.FindProperty("_bodyText").objectReferenceValue = t.Find("Window/Scroll/Viewport/Content/BodyText").GetComponent<TMP_Text>();
            so.FindProperty("_scroll").objectReferenceValue = t.Find("Window/Scroll").GetComponent<ScrollRect>();
            so.FindProperty("_content").objectReferenceValue = t.Find("Window/Scroll/Viewport/Content").GetComponent<RectTransform>();
            so.FindProperty("_gameButtonTemplate").objectReferenceValue = t.Find("Window/Scroll/Viewport/Content/GameButtonTemplate").GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateWindow(Transform parent)
        {
            GameObject dim = EditorUiFactory.CreateUiObject("Panel", parent, typeof(Image));
            EditorUiFactory.Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = DimColor;

            GameObject window = EditorUiFactory.CreatePanel(dim.transform, "Window", EditorUiFactory.PanelColor);
            RectTransform rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(940, 880);
            window.AddComponent<FitInParent>();   // shrinks on small screens

            VerticalLayoutGroup layout = window.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 40, 40);
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLabel(window.transform, "Title", "Settings", 72, 100);

            AddLabel(window.transform, "LanguageLabel", "Language", 48, 60);
            GameObject row = CreateRow(window.transform, "LanguageRow", 120);
            EditorUiFactory.CreateButton(row.transform, "PortugueseButton", "Português", 120);
            EditorUiFactory.CreateButton(row.transform, "EnglishButton", "English", 120);

            AddLabel(window.transform, "FontLabel", "Text size", 48, 60);
            GameObject fontRow = CreateRow(window.transform, "FontRow", 120);
            EditorUiFactory.CreateButton(fontRow.transform, "Font0Button", "A", 120);
            EditorUiFactory.CreateButton(fontRow.transform, "Font1Button", "A+", 120);
            EditorUiFactory.CreateButton(fontRow.transform, "Font2Button", "A++", 120);

            EditorUiFactory.CreateButton(window.transform, "CloseButton", "Close", 130);
            return dim;
        }

        private static GameObject CreateRow(Transform parent, string name, float height)
        {
            GameObject row = EditorUiFactory.CreateUiObject(name, parent, typeof(HorizontalLayoutGroup));
            EditorUiFactory.SetPreferredHeight(row, height);
            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return row;
        }

        private static void AddLabel(Transform parent, string name, string text, float size, float height)
        {
            TextMeshProUGUI label = EditorUiFactory.CreateText(parent, name, text, size);
            EditorUiFactory.SetPreferredHeight(label.gameObject, height);
        }

        /// <summary>
        /// A large, easy-to-grab slider (0 to 1) built from the pixel-art frame.
        /// </summary>
        private static void CreateSlider(Transform parent, string name)
        {
            GameObject sliderObject = EditorUiFactory.CreateUiObject(name, parent, typeof(Slider));
            EditorUiFactory.SetPreferredHeight(sliderObject, 100);

            GameObject background = CreateSliderPart("Background", sliderObject.transform, SliderBackColor);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.5f);
            backgroundRect.anchorMax = new Vector2(1, 0.5f);
            backgroundRect.sizeDelta = new Vector2(0, 48);

            GameObject fillArea = EditorUiFactory.CreateUiObject("Fill Area", sliderObject.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1, 0.5f);
            fillAreaRect.offsetMin = new Vector2(35, -24);
            fillAreaRect.offsetMax = new Vector2(-35, 24);

            GameObject fill = CreateSliderPart("Fill", fillArea.transform, EditorUiFactory.EasyColor);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            EditorUiFactory.Stretch(fillRect);

            GameObject handleArea = EditorUiFactory.CreateUiObject("Handle Slide Area", sliderObject.transform);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0, 0.5f);
            handleAreaRect.anchorMax = new Vector2(1, 0.5f);
            handleAreaRect.offsetMin = new Vector2(35, -40);
            handleAreaRect.offsetMax = new Vector2(-35, 40);

            GameObject handle = CreateSliderPart("Handle", handleArea.transform, EditorUiFactory.ButtonColor);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 0);
            handleRect.anchorMax = new Vector2(0, 1);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(70, 0);

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
        }

        private static GameObject CreateSliderPart(string name, Transform parent, Color tint)
        {
            return EditorUiFactory.CreatePanel(parent, name, tint);
        }
    }
}
