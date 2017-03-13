using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour {
    void OnGUI() {
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, Screen.width, Screen.height / 20);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = Screen.height / 20;
        style.normal.textColor = new Color(1, 1, 1, 1);
        string text = string.Format("{0:0.} fps", 1.0f / Time.deltaTime);
        GUI.Label(rect, text, style);
    }
}
