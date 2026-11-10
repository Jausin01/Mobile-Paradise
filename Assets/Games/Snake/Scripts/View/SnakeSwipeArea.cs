using System;
using MobileParadise.Games.Snake.Logic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileParadise.Games.Snake.View
{
    /// <summary>
    /// Turns a finger drag (or a mouse drag) on this object into a swipe: Up / Right / Down / Left.
    /// The object needs a raycast-target Image (it can be fully transparent) to receive the drag.
    /// </summary>
    public class SnakeSwipeArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float MinDistance = 50f;   // screen pixels

        public event Action<SnakeDirection> Swiped;

        // IBeginDragHandler / IDragHandler must exist for the drag to reach OnEndDrag.
        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - eventData.pressPosition;
            if (delta.magnitude < MinDistance) return;

            SnakeDirection direction;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                direction = delta.x > 0f ? SnakeDirection.Right : SnakeDirection.Left;
            }
            else
            {
                direction = delta.y > 0f ? SnakeDirection.Up : SnakeDirection.Down;
            }

            if (Swiped != null) Swiped(direction);
        }
    }
}
