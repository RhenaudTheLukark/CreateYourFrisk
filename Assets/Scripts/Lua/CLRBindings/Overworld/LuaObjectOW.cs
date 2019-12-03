public class LuaObjectOW {
    public ScriptWrapper appliedScript;

    public delegate void LoadedAction(string coroName, object args, string evName, LuaObjectOW luaobjow);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    [MoonSharpHidden] public LuaObjectOW() { }
}
