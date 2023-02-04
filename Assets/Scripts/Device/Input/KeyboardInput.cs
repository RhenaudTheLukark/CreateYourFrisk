using System.Linq;
using UnityEngine;

public class KeyboardInput : UndertaleInput {
    private const KeyCode KC_CONFIRM = KeyCode.Z, KC_CONFIRM_ALT = KeyCode.Return,
                          KC_CANCEL = KeyCode.X,  KC_CANCEL_ALT = KeyCode.LeftShift, KC_CANCEL_ALT2 = KeyCode.RightShift,
                          KC_MENU = KeyCode.C,    KC_MENU_ALT = KeyCode.LeftControl, KC_MENU_ALT2 = KeyCode.RightControl,
                          KC_UP = KeyCode.W,      KC_UP_ALT = KeyCode.UpArrow,
                          KC_DOWN = KeyCode.S,    KC_DOWN_ALT = KeyCode.DownArrow,
                          KC_LEFT = KeyCode.A,    KC_LEFT_ALT = KeyCode.LeftArrow,
                          KC_RIGHT = KeyCode.D,   KC_RIGHT_ALT = KeyCode.RightArrow;

    public override ButtonState Confirm { get { return StateFor(KC_CONFIRM, KC_CONFIRM_ALT); } }
    public override ButtonState Cancel  { get { return StateFor(KC_CANCEL,  KC_CANCEL_ALT,   KC_CANCEL_ALT2); } }
    public override ButtonState Menu    { get { return StateFor(KC_MENU,    KC_MENU_ALT,     KC_MENU_ALT2);   } }
    public override ButtonState Up      { get { return StateFor(KC_UP,      KC_UP_ALT);      } }
    public override ButtonState Down    { get { return StateFor(KC_DOWN,    KC_DOWN_ALT);    } }
    public override ButtonState Left    { get { return StateFor(KC_LEFT,    KC_LEFT_ALT);    } }
    public override ButtonState Right   { get { return StateFor(KC_RIGHT,   KC_RIGHT_ALT);   } }

    public override ButtonState Key(string Key) { return StateFor((KeyCode)System.Enum.Parse(typeof(KeyCode), Key)); }

    private static ButtonState StateFor(KeyCode c) {
        if (Input.GetKeyDown(c)) return ButtonState.PRESSED;
        if (Input.GetKeyUp(c))   return ButtonState.RELEASED;
        return Input.GetKey(c)        ? ButtonState.HELD
                                      : ButtonState.NONE;
    }

    private static ButtonState StateFor(params KeyCode[] keys) {
        return keys.Select(StateFor).FirstOrDefault(state => state != ButtonState.NONE);
    }
}
