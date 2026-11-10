using System;
using MobileParadise.Games.Game2048.Logic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileParadise.Games.Game2048.View
{
    /// <summary>
    /// Turns a finger drag (or a mouse drag) on this object into a Left / Right / Up / Down swipe.
    /// The object needs a raycast-target Image (it can be fully transparent) to receive the drag.
    /// </summary>
    public class SwipeArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float MinDistance = 60f;   // screen pixels

        public event Action<SlideDirection> Swiped;

        // IBeginDragHandler / IDragHandler must exist for the drag to reach OnEndDrag.
        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - eventData.pressPosition;
            if (delta.magnitude < MinDistance) return;

            SlideDirection direction;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                direction = delta.x > 0f ? SlideDirection.Right : SlideDirection.Left;
            }
            else
            {
                direction = delta.y > 0f ? SlideDirection.Up : SlideDirection.Down;
            }

            if (Swiped != null) Swiped(direction);
        }
    }
}
