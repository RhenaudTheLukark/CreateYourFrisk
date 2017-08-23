public static class InputUtil {
    public static bool Released(UndertaleInput.ButtonState s) { return s == UndertaleInput.ButtonState.RELEASED; }
    public static bool Pressed(UndertaleInput.ButtonState s)  { return s == UndertaleInput.ButtonState.PRESSED; }
    public static bool Held(UndertaleInput.ButtonState s)     { return s > 0; }
}