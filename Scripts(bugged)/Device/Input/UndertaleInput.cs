using UnityEngine;
using System.Collections;

public abstract class UndertaleInput {
    public enum ButtonState
    {
        RELEASED = -1,
        NONE = 0,
        PRESSED = 1,
        HELD = 2
    }

    public abstract ButtonState Confirm { get; }
    public abstract ButtonState Cancel { get; }
    public abstract ButtonState Menu { get; }
    public abstract ButtonState Up { get; }
    public abstract ButtonState Down { get; }
    public abstract ButtonState Left { get; }
    public abstract ButtonState Right { get; }
    public abstract ButtonState Key(string Key);
}
