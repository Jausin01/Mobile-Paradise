using MobileParadise.Core.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MobileParadise.EditorTools
{
    /// <summary>
    /// The look of the game in one place (colors, pixel-art frames) and the basic UI pieces
    /// (camera, canvas, panel, text, button) used by all editor tools.
    /// Sprites are always loaded at the moment of use, after the new scene exists.
    /// </summary>
    public static class EditorUiFactory
    {
        public const string FramePath = "Assets/Core/Art/ui_frame.png";
        public const string GearPath = "Assets/Core/Art/gear.png";
        public const string NotePath = "Assets/Core/Art/icon_note.png";
        public const string PlayPath = "Assets/Core/Art/icon_play.png";
        public const string PausePath = "Assets/Core/Art/icon_pause.png";
        public const string RestartPath = "Assets/Core/Art/icon_restart.png";
        public const string NextPath = "Assets/Core/Art/icon_next.png";
        public const string RepeatPath = "Assets/Core/Art/icon_repeat.png";

        public static readonly Color SceneBackground = new Color(0.17f, 0.13f, 0.10f);
        public static readonly Color PanelColor = new Color(0.96f, 0.91f, 0.80f);
        public static readonly Color ButtonColor = new Color(0.93f, 0.80f, 0.50f);
        public static readonly Color WoodColor = new Color(0.47f, 0.31f, 0.18f);
        public static readonly Color TextDark = new Color(0.17f, 0.11f, 0.07f);
        public static readonly Color TextLight = new Color(0.96f, 0.91f, 0.80f);
        public static readonly Color EasyColor = new Color(0.62f, 0.82f, 0.50f);
        public static readonly Color MediumColor = new Color(0.95f, 0.78f, 0.38f);
        public static readonly Color HardColor = new Color(0.88f, 0.50f, 0.38f);

        // The frame sprite is 32 px per unit and the canvas uses 100: this makes each art pixel 4 screen pixels.
        private const float FrameScale = 100f / 32f / 4f;

        // ---- Setup -------------------------------------------------------------

        public static bool EnsureTmpReady()
        {
            if (TMP_Settings.instance != null) return true;

            EditorUtility.DisplayDialog("TextMeshPro",
                "Import TMP Essentials first:\nWindow > TextMeshPro > Import TMP Essential Resources.\nThen run this command again.",
                "OK");
            return false;
        }

        /// <summary>
        /// Makes sure the shared UI art (frame, gear, music icons) is imported as pixel art.
        /// </summary>
        public static void PrepareArt()
        {
            string[] paths = { FramePath, GearPath, NotePath, PlayPath, PausePath, RestartPath, NextPath, RepeatPath };
            for (int i = 0; i < paths.Length; i++)
            {
                TextureImporter importer = AssetImporter.GetAtPath(paths[i]) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning("Missing UI art: " + paths[i]);
                    continue;
                }

                PixelArtImporter.ApplySettings(importer);
                importer.SaveAndReimport();
            }
        }

        public static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---- Scene basics --------------------------------------------------------

        public static Camera CreateCamera()
        {
            GameObject go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";

            Camera cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SceneBackground;
            return cam;
        }

        public static Canvas CreateCanvas()
        {
            GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;

            GameObject es = new GameObject("EventSystem", typeof(EventSystem));
            System.Type inputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            es.AddComponent(inputModule != null ? inputModule : typeof(StandaloneInputModule));
        }

        // ---- UI pieces -------------------------------------------------------------

        public static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
        {
            System.Type[] types = new System.Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            for (int i = 0; i < components.Length; i++)
            {
                types[i + 1] = components[i];
            }

            GameObject go = new GameObject(name, types);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void SetPreferredHeight(GameObject go, float height)
        {
            LayoutElement element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
        }

        public static void ApplyFrame(Image image, Color tint)
        {
            image.sprite = LoadSprite(FramePath);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = FrameScale;
            image.color = tint;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color tint)
        {
            GameObject go = CreateUiObject(name, parent, typeof(Image));
            ApplyFrame(go.GetComponent<Image>(), tint);
            return go;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size)
        {
            return CreateText(parent, name, text, size, TextDark);
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color)
        {
            GameObject go = CreateUiObject(name, parent);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            SetFontSize(tmp, size);
            tmp.fontStyle = FontStyles.Bold;
            tmp.margin = new Vector4(12, 4, 12, 4);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.raycastTarget = false;

            go.AddComponent<ScaledText>();
            return tmp;
        }

        /// <summary>
        /// Texts use Auto Size: "size" is the biggest size, and the text shrinks to fit its box.
        /// (This is what keeps long words and the large-font setting from breaking buttons.)
        /// </summary>
        public static void SetFontSize(TMP_Text text, float size)
        {
            text.enableAutoSizing = true;
            text.fontSizeMax = size;
            text.fontSizeMin = size * 0.4f;
            text.fontSize = size;
        }

        /// <summary>
        /// Container for all screen content: stays inside the notch / rounded corners.
        /// </summary>
        public static RectTransform CreateSafeArea(Canvas canvas)
        {
            GameObject go = CreateUiObject("SafeArea", canvas.transform, typeof(SafeAreaFitter));
            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            return rect;
        }

        public static Button CreateButton(Transform parent, string name, string label, float height)
        {
            return CreateButton(parent, name, label, height, ButtonColor);
        }

        public static Button CreateButton(Transform parent, string name, string label, float height, Color tint)
        {
            GameObject go = CreatePanel(parent, name, tint);

            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            go.AddComponent<ButtonSound>();
            SetPreferredHeight(go, height);

            TextMeshProUGUI text = CreateText(go.transform, "Label", label, 64);
            Stretch(text.rectTransform);
            return button;
        }

        // ---- Shared screen pieces (start screens) -------------------------------

        /// <summary>
        /// A row of equal-width buttons.
        /// </summary>
        public static GameObject CreateRow(Transform parent, string name, float height)
        {
            GameObject row = CreateUiObject(name, parent, typeof(HorizontalLayoutGroup));
            SetPreferredHeight(row, height);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return row;
        }

        public static TextMeshProUGUI CreateSectionLabel(Transform parent, string name)
        {
            TextMeshProUGUI label = CreateText(parent, name, "", 44, TextLight);
            SetPreferredHeight(label.gameObject, 56);
            return label;
        }

        /// <summary>
        /// Button with a picture on the left and its name on the right.
        /// </summary>
        public static Button CreateIconButton(Transform parent, string name, out Image icon)
        {
            Button button = CreateButton(parent, name, "", 120);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            SetFontSize(label, 44);
            label.rectTransform.offsetMin = new Vector2(110, 0);

            GameObject iconObject = CreateUiObject("Icon", button.transform, typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0, 1);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.offsetMin = new Vector2(14, 10);
            iconRect.offsetMax = new Vector2(114, -10);
            icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return button;
        }
    }
}
