using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour {
    float updateTime = 1, currentTime = .9999f, lastValue = 0;
    void OnGUI() {
        if (currentTime >= updateTime) {
            currentTime %= updateTime;
            lastValue = Mathf.RoundToInt(1 / Time.deltaTime);
        }

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, Screen.width, Screen.height / 20);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = Screen.height / 20;
        style.normal.textColor = new Color(1, 1, 1, 1);
        string text = string.Format("{0:0.} fps", lastValue);
        GUI.Label(rect, text, style);
        currentTime += Time.deltaTime;
    }
}
