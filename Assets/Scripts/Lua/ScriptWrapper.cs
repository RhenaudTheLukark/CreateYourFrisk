using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

public class ScriptWrapper {
    [MoonSharpHidden] public static List<ScriptWrapper> instances = new List<ScriptWrapper>();
    [MoonSharpHidden] public Script script;
    public string scriptname = "???";
    private const string toDoString = "setmetatable({}, {__index=function(t, name) return _getv(name) end}) ";

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }

    public ScriptWrapper(/*bool overworld = false*/) {
        script = LuaScriptBinder.BoundScript(/*overworld*/);
        Bind("_getv", (Func<Script, string, DynValue>)GetVar);
        DoString(toDoString);
        instances.Add(this);
    }

    public void Remove() {
        instances.Remove(this);
    }

    internal DynValue DoString(string source) {
        DynValue d = DynValue.Nil, res = DynValue.Nil;
        try {
            res = script.DoString(source, null, scriptname != "???" ? scriptname : null);
        } catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(scriptname, ex.DecoratedMessage == null ?
                    ex.Message :
                    UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message,
                ex.DoNotDecorateMessage);
        } catch (Exception ex) {
            if (GlobalControls.retroMode)
                return d;
            if (ex.GetType().ToString() == "System.IndexOutOfRangeException" && ex.StackTrace.StartsWith("  at (wrapper stelemref) object:stelemref (object,intptr,object)"
                + "\r\n  at MoonSharp.Interpreter.DataStructs.FastStack`1[MoonSharp.Interpreter.DynValue].Push"))
                UnitaleUtil.DisplayLuaError(scriptname, "<b>Possible infinite loop</b>\n\nThis is a " + ex.GetType() + " error.\n\n"
                    + "You almost definitely have an infinite loop in your code. A function tried to call itself infinitely. It could be a normal function or a metatable function."
                    + "\n\n\nFull stracktrace (see CYF output log at <b>" + Application.persistentDataPath + "/output_log.txt</b>):\n\n" + ex.StackTrace);
            else
                UnitaleUtil.DisplayLuaError(scriptname, "This is a " + ex.GetType() + " error. Contact a dev and show them this screen, this must be an engine-side error.\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
        }

        return res;
    }

    public void SetVar(string key, DynValue value) {
        if (key == null)
            throw new CYFException("script.SetVar: The first argument (key) is nil.\n\nSee the documentation for proper usage.");
        script.Globals.Set(key, MoonSharpUtil.CloneIfRequired(script, value));
    }

    public DynValue GetVar(string key) { return GetVar(null, key); }

    public DynValue GetVar(Script caller, string key) {
        if (key == null)
            throw new CYFException("script.GetVar: The first argument (key) is nil.\n\nSee the documentation for proper usage.");
        DynValue value = script.Globals.Get(key);
        if (value == null || value.IsNil())  return DynValue.NewNil();
        if (caller == null)                  return value;
        return script.Globals[key] != null ? MoonSharpUtil.CloneIfRequired(caller, value) : null;
    }

    public DynValue Call(string function, DynValue arg) { return Call(function, new[] { arg }); }

    public DynValue Call(string function, DynValue[] args = null, bool checkExist = false) { return Call(script.Globals.Get(function), function, args, checkExist); }

    public DynValue Call(DynValue function, string functionName, DynValue arg, bool checkExist = false) { return Call(function, functionName, new[] { arg }, checkExist); }

    public DynValue Call(DynValue function, string functionName, DynValue[] args = null, bool checkExist = false) {
        if ((function.Type & (DataType.ClrFunction | DataType.Function)) == 0) {
            if (checkExist && !GlobalControls.retroMode)
                UnitaleUtil.DisplayLuaError(scriptname, "Attempted to call the function \"" + functionName + "\", but it didn't exist.");
        } else
            try { return script.Call(function, args ?? new DynValue[0]); } catch (Exception e) {
                if (args != null && args[0].Type == DataType.Table && args.Length == 1) {
                    DynValue[] argsNew = UnitaleUtil.TableToDynValueArray(args[0].Table);
                    try { return script.Call(function, argsNew); } catch (Exception e2) {
                        if (e2 as InterpreterException != null)
                            e = e2;
                    }
                }

                UnitaleUtil.HandleError(scriptname, functionName, e);
            }
        return DynValue.Nil;
    }

    internal void Bind(string key, object func)      { script.Globals[key] = func; }
}