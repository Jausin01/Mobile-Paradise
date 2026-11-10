using System;
using MobileParadise.Games.Breakout.Logic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileParadise.Games.Breakout.View
{
    /// <summary>
    /// Turns touches on this object into "move the paddle to this x" (finger down or dragging)
    /// and "tap" (touch and release without moving). x is in field units (0..1000).
    /// The object needs a raycast-target Image (it can be fully transparent) to receive the touches.
    /// </summary>
    public class BreakoutInputArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _field;

        public event Action<float> PaddleMoved;
        public event Action Tapped;

        public void OnPointerDown(PointerEventData eventData)
        {
            MoveTo(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveTo(eventData);
        }

        // A click only arrives when the finger did not drag far, so a drag never launches the ball.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (Tapped != null) Tapped();
        }

        private void MoveTo(PointerEventData eventData)
        {
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_field, eventData.position, eventData.pressEventCamera, out local)) return;

            if (PaddleMoved != null) PaddleMoved(local.x + BreakoutWorld.FieldWidth / 2f);
        }
    }
}
