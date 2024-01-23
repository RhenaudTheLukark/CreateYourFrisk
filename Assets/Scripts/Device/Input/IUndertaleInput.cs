public enum ButtonState {
    RELEASED = -1,
    NONE = 0,
    PRESSED = 1,
    HELD = 2
}

public interface IUndertaleInput {
    ButtonState Confirm { get; }
    ButtonState Cancel { get; }
    ButtonState Menu { get; }
    ButtonState Up { get; }
    ButtonState Down { get; }
    ButtonState Left { get; }
    ButtonState Right { get; }
    ButtonState Key(string key);

    void LateUpdate();
}
