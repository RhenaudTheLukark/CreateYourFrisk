using UnityEngine;
using UnityEngine.UI;

public class KeybindEntry : MonoBehaviour {
    public static KeybindSettings controller;
    public string Name;
    public Button Edit, Reset, Clear;
    public Text Text, KeyList;
    public Image Image;

    void Start() {
        Name = gameObject.name;
        if (controller == null)
            controller = FindObjectOfType<KeybindSettings>();

        Edit.onClick.AddListener(() => {
            if (controller.listening == this) controller.StopListening();
            else                              controller.StartListening(this);
        });
        Reset.onClick.AddListener(() => {
            controller.ResetKeybind(this);
        });
        Clear.onClick.AddListener(() => {
            controller.ClearKeybind(this);
        });
    }

    public void SetKeyList(string keyList) { KeyList.text = keyList; }
    public void SetEditText(string text) { Edit.GetComponentInChildren<Text>().text = text; }

    public void SetColor(Color c) {
        Image.color = c;
        Text.color = c;
        KeyList.color = c;
    }
}
