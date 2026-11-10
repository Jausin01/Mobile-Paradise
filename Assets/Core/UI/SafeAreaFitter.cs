using UnityEngine;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Keeps this rect inside the part of the screen that is safe to use
    /// (not under the camera notch, rounded corners or system bars).
    /// Put all screen content under an object with this component.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void OnEnable()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea || Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect safe = Screen.safeArea;
            _lastSafeArea = safe;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            _rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            _rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
