using UnityEngine;

public class FPSDisplay : MonoBehaviour {
    private const float updateTime = 1;
    private float currentTime = .9999f, lastValue;

    private void OnGUI() {
        if (currentTime >= updateTime) {
            currentTime %= updateTime;
            lastValue = Mathf.RoundToInt(1 / Time.deltaTime);
        }

        GUIStyle style = new GUIStyle();

        #if !UNITY_EDITOR
            Rect rect = new Rect(0, 0, 640 * ScreenResolution.tempWindowScale, 24 * ScreenResolution.tempWindowScale);
        #else
            Rect rect = new Rect(0, 0, Screen.width, Screen.height / 20f);
        #endif
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = Screen.fullScreen ? 24 * Screen.height / 480 : 24 * ScreenResolution.tempWindowScale;
        style.normal.textColor = new Color(1, 1, 1, 1);
        string text = string.Format("{0:0.} fps", lastValue);
        GUI.Label(rect, text, style);
        currentTime += Time.deltaTime;
    }
}
