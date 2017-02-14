using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour {
    float deltaTime = 0.0f;
    float timer = 0.5f;
    float lilTimer = 1;

    void Update() {
        if (lilTimer >= timer) {
            lilTimer = 0;
            deltaTime = Time.deltaTime;
        }
        lilTimer += Time.deltaTime;
    }

    void OnGUI() {
        //if (lilTimer >= timer) {
            //print("Yup");
            //lilTimer = 0;
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 40);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 40;
            style.normal.textColor = new Color(1, 1, 1, 1);
            string text = string.Format("{0:0.} fps", 1.0f / deltaTime);
            GUI.Label(rect, text, style);
        //}
    }
}
