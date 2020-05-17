using UnityEngine;

public class TextSceneInsertion : MonoBehaviour {
    // Use this for initialization
    private void Start() {
        SpriteFontRegistry.init();
        TextManager tm = FindObjectOfType<TextManager>();
        tm.SetFont(SpriteFontRegistry.Get("wingdings"));
        //tm.setText(new TextMessage("the quick brown fox jumps over\rthe lazy dog.\nTHE QUICK BROWN FOX JUMPS OVER\rTHE LAZY DOG.\nJerry.", true, false));
        //tm.setText(new RegularMessage("THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG?\nTEST! TEST? TEST:TEST, TEST/TEST. TEST\\TEST\n0123456789"));
        tm.SetText(new RegularMessage("THE QUICK BROWN\rFOX JUMPS OVER\rTHE LAZY DOG\nthe quick brown\rfox jumps over\rthe lazy dog\n0123456789"));
    }

    // Update is called once per frame
    private void Update() { }
}