using UnityEngine;

public class KeyboardInput : UndertaleInput {
    KeyCode KC_CONFIRM = KeyCode.Z, KC_CONFIRM_ALT = KeyCode.Return,
            KC_CANCEL = KeyCode.X,  KC_CANCEL_ALT = KeyCode.LeftShift, KC_CANCEL_ALT2 = KeyCode.RightShift,
            KC_MENU = KeyCode.C,    KC_MENU_ALT = KeyCode.LeftControl,
            KC_UP = KeyCode.W,      KC_UP_ALT = KeyCode.UpArrow,
            KC_DOWN = KeyCode.S,    KC_DOWN_ALT = KeyCode.DownArrow,
            KC_LEFT = KeyCode.A,    KC_LEFT_ALT = KeyCode.LeftArrow,
            KC_RIGHT = KeyCode.D,   KC_RIGHT_ALT = KeyCode.RightArrow;

    public override ButtonState Confirm { get { return stateFor(KC_CONFIRM, KC_CONFIRM_ALT); } }
    public override ButtonState Cancel { get { return stateFor(KC_CANCEL, KC_CANCEL_ALT, KC_CANCEL_ALT2); } }
    public override ButtonState Menu { get { return stateFor(KC_MENU, KC_MENU_ALT); } }
    public override ButtonState Up { get { return stateFor(KC_UP, KC_UP_ALT); } }
    public override ButtonState Down { get { return stateFor(KC_DOWN, KC_DOWN_ALT); } }
    public override ButtonState Left { get { return stateFor(KC_LEFT, KC_LEFT_ALT); } }
    public override ButtonState Right { get { return stateFor(KC_RIGHT, KC_RIGHT_ALT); } }

    public override ButtonState Key(string Key) { return stateFor((KeyCode)System.Enum.Parse(typeof(KeyCode), Key)); }

    private ButtonState stateFor(KeyCode c) {
        if (Input.GetKeyDown(c))    return ButtonState.PRESSED;
        else if (Input.GetKeyUp(c)) return ButtonState.RELEASED;
        else if (Input.GetKey(c))   return ButtonState.HELD;
        else                        return ButtonState.NONE;
    }

    private ButtonState stateFor(params KeyCode[] keys) {
        foreach(KeyCode key in keys){
            ButtonState state = stateFor(key);
            if (state != ButtonState.NONE)
                return state;
        }
        return ButtonState.NONE;
    }
}
