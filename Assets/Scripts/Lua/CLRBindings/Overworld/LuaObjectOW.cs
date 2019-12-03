using MoonSharp.Interpreter;
public class LuaObjectOW {
    public static ScriptWrapper appliedScript;

    public delegate void LoadedAction(string coroName, object args, string evName, LuaObjectOW luaobjow);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    protected void OnStCoroutine(string coroName, object args, string evName, LuaObjectOW luaobjow) {
        LoadedAction act = StCoroutine;
        if (act != null)
            StCoroutine(coroName, args, evName, luaobjow);
    }

    [MoonSharpHidden] public LuaObjectOW() { }
}
