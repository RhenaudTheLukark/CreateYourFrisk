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

        if (GlobalControls.crate) {
            Edit.GetComponentInChildren<Text>().text = "GO";
            Reset.GetComponentInChildren<Text>().text = "OLD";
            Clear.GetComponentInChildren<Text>().text = "BYEE";
            switch (Text.text) {
                case "Confirm": Text.text = "YASS GO"; break;
                case "Cancel":  Text.text = "RATIO'D"; break;
                case "Menu":    Text.text = "YUMMY";   break;
                case "Up":      Text.text = "EYUP";    break;
                case "Down":    Text.text = "DONN";    break;
                case "Left":    Text.text = "LETFE";   break;
                case "Right":   Text.text = "RITE";    break;
                default:                               break;
            }
            KeyList.text = Temmify.Convert(KeyList.text);
        }
    }

    public void SetKeyList(string keyList) { KeyList.text = keyList; }
    public void SetEditText(string text) { Edit.GetComponentInChildren<Text>().text = text; }

    public void SetColor(Color c) {
        Image.color = c;
        Text.color = c;
        KeyList.color = c;
    }
}
