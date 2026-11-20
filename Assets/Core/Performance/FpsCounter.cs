using UnityEngine;

namespace MobileParadise.Core.Performance
{
    /// <summary>
    /// Debug helper: shows the average fps and the slowest frame of the last second in the
    /// top-left corner. Only created in Development Builds (Debug.isDebugBuild), never in the release APK.
    /// </summary>
    public class FpsCounter : MonoBehaviour
    {
        private const float Interval = 1f;

        private float _elapsed;
        private int _frames;
        private float _worstFrame;
        private string _text = "";
        private GUIStyle _style;

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _elapsed += dt;
            _frames++;
            if (dt > _worstFrame) _worstFrame = dt;

            if (_elapsed < Interval) return;

            _text = ((int)(_frames / _elapsed)) + " fps   worst " + ((int)(_worstFrame * 1000f)) + " ms";
            _elapsed = 0f;
            _frames = 0;
            _worstFrame = 0f;
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = Screen.height / 40;
                _style.normal.textColor = Color.yellow;
            }

            GUI.Label(new Rect(20, Screen.height / 12, Screen.width, Screen.height / 20), _text, _style);
        }
    }
}
