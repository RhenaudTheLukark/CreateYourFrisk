public static class InputUtil {
    public static bool Released(ButtonState s) { return s == ButtonState.RELEASED; }
    public static bool Pressed(ButtonState s)  { return s == ButtonState.PRESSED; }
    public static bool Held(ButtonState s)     { return s > 0; }
}