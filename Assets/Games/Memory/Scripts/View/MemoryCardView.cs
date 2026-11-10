using System;
using System.Collections;
using MobileParadise.Games.Memory.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// One card on screen: face down (card back art), face up (cream frame + picture) or matched (green frame).
    /// Turning a card is a short animation (the card narrows, changes, widens).
    /// </summary>
    public class MemoryCardView : MonoBehaviour
    {
        private const float FlipSeconds = 0.16f;
        private const float FrameScale = 100f / 32f / 4f;   // art pixel = 4 canvas pixels, like the other frames
        private static readonly Color FaceUpColor = new Color(0.96f, 0.91f, 0.80f);
        private static readonly Color MatchedColor = new Color(0.62f, 0.82f, 0.50f);

        private Image _background;
        private Image _face;
        private Sprite _backSprite;
        private Sprite _frameSprite;
        private CardState _shown = CardState.Hidden;
        private Sprite _faceSprite;

        public event Action<int> Clicked;

        public int Index { get; private set; }

        public void Init(int index, Image background, Image face, Button button, Sprite backSprite, Sprite frameSprite)
        {
            Index = index;
            _background = background;
            _face = face;
            _backSprite = backSprite;
            _frameSprite = frameSprite;

            button.onClick.AddListener(OnClicked);
            Apply(CardState.Hidden);
        }

        public void ShowFace(Sprite face)
        {
            _faceSprite = face;
            Turn(CardState.Revealed);
        }

        /// <summary>
        /// Shows a card as already matched, with no turn (used when a saved game is loaded).
        /// </summary>
        public void ShowMatched(Sprite face)
        {
            StopAllCoroutines();
            transform.localScale = Vector3.one;
            _faceSprite = face;
            Apply(CardState.Matched);
        }

        public void Hide()
        {
            Turn(CardState.Hidden);
        }

        public void SetMatched()
        {
            Apply(CardState.Matched);   // no turn: the card is already face up
        }

        private void OnClicked()
        {
            if (Clicked != null) Clicked(Index);
        }

        private void Turn(CardState target)
        {
            StopAllCoroutines();
            if (!gameObject.activeInHierarchy)
            {
                Apply(target);
                return;
            }
            StartCoroutine(TurnRoutine(target));
        }

        private IEnumerator TurnRoutine(CardState target)
        {
            yield return Scale(1f, 0f);
            Apply(target);
            yield return Scale(0f, 1f);
        }

        private IEnumerator Scale(float from, float to)
        {
            float time = 0f;
            while (time < FlipSeconds / 2f)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / (FlipSeconds / 2f));
                transform.localScale = new Vector3(Mathf.Lerp(from, to, t), 1f, 1f);
                yield return null;
            }
            transform.localScale = new Vector3(to, 1f, 1f);
        }

        private void Apply(CardState state)
        {
            _shown = state;

            if (state == CardState.Hidden)
            {
                _background.sprite = _backSprite;
                _background.type = Image.Type.Simple;
                _background.color = Color.white;
                _face.enabled = false;
                return;
            }

            _background.sprite = _frameSprite;
            _background.type = Image.Type.Sliced;
            _background.pixelsPerUnitMultiplier = FrameScale;
            _background.color = state == CardState.Matched ? MatchedColor : FaceUpColor;
            _face.sprite = _faceSprite;
            _face.enabled = true;
        }
    }
}
