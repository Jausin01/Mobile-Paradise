using System;
using MobileParadise.Core.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileParadise.Games.Tetris.View
{
    /// <summary>
    /// A button that acts when pressed and keeps repeating while the finger stays on it
    /// (to move a piece several columns, or to make it fall).
    /// </summary>
    public class TetrisHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float FirstDelay = 0.28f;
        private const float RepeatDelay = 0.09f;

        private bool _held;
        private float _nextTime;

        public event Action Triggered;

        public void OnPointerDown(PointerEventData eventData)
        {
            _held = true;
            _nextTime = Time.unscaledTime + FirstDelay;
            Fire();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _held = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _held = false;
        }

        private void Update()
        {
            if (!_held) return;
            if (UiPause.IsOpen)
            {
                _held = false;
                return;
            }

            if (Time.unscaledTime < _nextTime) return;

            _nextTime += RepeatDelay;
            Fire();
        }

        private void OnDisable()
        {
            _held = false;
        }

        private void Fire()
        {
            if (Triggered != null) Triggered();
        }
    }
}
