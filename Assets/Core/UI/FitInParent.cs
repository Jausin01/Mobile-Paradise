using UnityEngine;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Scales this object (uniformly, around its center) so it always fits inside its parent,
    /// minus a padding. It never grows past MaxScale. Use it on fixed-size content
    /// (the chess board, a settings window) so it works on tall, short and narrow screens.
    /// The object must have a fixed size and be centered in its parent.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class FitInParent : MonoBehaviour
    {
        [SerializeField] private Vector2 _padding = new Vector2(40, 40);
        [SerializeField] private float _maxScale = 1f;

        private RectTransform _rect;
        private RectTransform _parent;
        private Vector2 _lastSize;
        private Vector2 _lastParentSize;

        public void SetPadding(Vector2 padding)
        {
            _padding = padding;
            _lastSize = Vector2.zero;   // forces the next Apply to recalculate
        }

        private void OnEnable()
        {
            _rect = (RectTransform)transform;
            _parent = _rect.parent as RectTransform;
            _lastSize = Vector2.zero;
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            if (_parent == null) return;

            Vector2 size = _rect.rect.size;
            if (size.x <= 0f || size.y <= 0f) return;

            // Nothing changed since the last calculation: skip the work.
            Vector2 parentSize = _parent.rect.size;
            if (size == _lastSize && parentSize == _lastParentSize) return;
            _lastSize = size;
            _lastParentSize = parentSize;

            Vector2 available = parentSize - _padding;
            float scale = Mathf.Min(available.x / size.x, available.y / size.y, _maxScale);
            if (scale <= 0f) return;

            _rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
