using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// Finger control on the board: drag sideways to move the piece (one column per cell width),
    /// drag down to make it fall faster, tap to rotate. Put it on a transparent raycast Image over the board.
    /// </summary>
    public class TetrisTouchArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const float TapDistance = 0.4f;   // in cells: less movement than this is a tap

        private RectTransform _holder;
        private float _cell = 1f;
        private bool _pressed;
        private Vector2 _anchor;
        private float _travel;

        public event Action<int> MoveRequested;
        public event Action RotateRequested;
        public event Action DropRequested;

        public void Setup(RectTransform holder, float cellSize)
        {
            _holder = holder;
            _cell = cellSize;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 local;
            if (!ToLocal(eventData, out local)) return;

            _pressed = true;
            _anchor = local;
            _travel = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 local;
            if (!_pressed || !ToLocal(eventData, out local)) return;

            float dx = local.x - _anchor.x;
            float dy = _anchor.y - local.y;   // positive = finger went down
            _travel = Mathf.Max(_travel, Mathf.Abs(dx) / _cell, Mathf.Abs(dy) / _cell);

            while (dx >= _cell)
            {
                _anchor.x += _cell;
                dx -= _cell;
                if (MoveRequested != null) MoveRequested(1);
            }
            while (dx <= -_cell)
            {
                _anchor.x -= _cell;
                dx += _cell;
                if (MoveRequested != null) MoveRequested(-1);
            }
            while (dy >= _cell)
            {
                _anchor.y -= _cell;
                dy -= _cell;
                if (DropRequested != null) DropRequested();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pressed) return;
            _pressed = false;

            if (_travel < TapDistance && RotateRequested != null) RotateRequested();
        }

        private bool ToLocal(PointerEventData eventData, out Vector2 local)
        {
            local = Vector2.zero;
            if (_holder == null) return false;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_holder, eventData.position, eventData.pressEventCamera, out local);
        }
    }
}
