using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

public class ScriptWrapper {
    [MoonSharpHidden] public static List<ScriptWrapper> instances = new List<ScriptWrapper>();
    [MoonSharpHidden] public Script script;
    public string scriptname = "???";

    public DynValue this[string key] {
        get { return this.GetVar(key); }
        set { this.SetVar(key, value); }
    }

    public ScriptWrapper(/*bool overworld = false*/) {
        script = LuaScriptBinder.BoundScript(/*overworld*/);
        this.Bind("_getv", (Func<Script, string, DynValue>)this.GetVar);
        string toDoString = "setmetatable({}, {__index=function(t, name) return _getv(name) end}) ";
        script.DoString(toDoString, null, scriptname);
        instances.Add(this);
    }

    ~ScriptWrapper() {
        instances.Remove(this);
    }

    internal DynValue DoString(string source) { return script.DoString(source, null, scriptname != "???" ? scriptname : null); }

    public void SetVar(string key, DynValue value) {
        if (key == null)
            throw new CYFException("script.SetVar: The first argument (key) is null.\n\nSee the documentation for proper usage.");
        script.Globals.Set(key, MoonSharpUtil.CloneIfRequired(script, value));
    }

    public DynValue GetVar(string key) { return GetVar(null, key); }

    public DynValue GetVar(Script caller, string key) {
        if (key == null)
            throw new CYFException("script.GetVar: The first argument (key) is null.\n\nSee the documentation for proper usage.");
        DynValue value = script.Globals.Get(key);
        if (value == null || value.IsNil())  return DynValue.NewNil();
        if (caller == null)                  return value;
        if (script.Globals[key] != null)     return MoonSharpUtil.CloneIfRequired(caller, value);
        return null;
    }

    public DynValue Call(string function, DynValue[] args = null, bool checkExist = false) { return Call(null, function, args, checkExist); }

    public DynValue Call(string function, DynValue arg) { return Call(null, function, new DynValue[] { arg }); }

    public DynValue Call(Script caller, string function, DynValue[] args = null, bool checkExist = false) {
        if (script.Globals[function] == null || script.Globals.Get(function) == null) {
            if (checkExist &&!GlobalControls.retroMode)
                UnitaleUtil.DisplayLuaError(scriptname, "Attempted to call the function \"" + function + "\", but it didn't exist.");
            //Debug.LogWarning("Attempted to call the function " + function + " but it didn't exist.");
            return DynValue.Nil;
        }
        if (args != null) {
            DynValue d = DynValue.Nil;
            try { d = script.Call(script.Globals[function], args); }
            catch (Exception e) {
                if (args[0].Type == DataType.Table && args.Length == 1) {
                    DynValue[] argsNew = UnitaleUtil.TableToDynValueArray(args[0].Table);
                    try { d = script.Call(script.Globals[function], argsNew); }
                    catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(scriptname, ex.DecoratedMessage == null ?
                                                                                                  ex.Message :
                                                                                                  UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message,
                                                                                              ex.DoNotDecorateMessage); }
                    catch (Exception ex) {
                        if (!GlobalControls.retroMode)
                            UnitaleUtil.DisplayLuaError(scriptname + ", calling the function " + function, "This is a " + ex.GetType() + " error. Contact a developer and show them this screen, this must be an engine-side error.\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
                    }
                } else if (e.GetType() == typeof(InterpreterException) || e.GetType().BaseType == typeof(InterpreterException) || e.GetType().BaseType.BaseType == typeof(InterpreterException)) {
                    UnitaleUtil.DisplayLuaError(scriptname, ((InterpreterException)e).DecoratedMessage == null ?
                                                            ((InterpreterException)e).Message :
                                                            UnitaleUtil.FormatErrorSource(((InterpreterException)e).DecoratedMessage, ((InterpreterException)e).Message) + ((InterpreterException)e).Message,
                                                            ((InterpreterException)e).DoNotDecorateMessage);
                } else if (!GlobalControls.retroMode)
                    UnitaleUtil.DisplayLuaError(scriptname + ", calling the function " + function, "This is a " + e.GetType() + " error. Contact the dev and show them this screen, this must be an engine-side error.\n\n" + e.Message + "\n\n" + e.StackTrace + "\n");
            }
            return d;
        } else {
            DynValue d = DynValue.Nil;
            try { d = script.Call(script.Globals[function]); }
            catch (InterpreterException ex) {
                UnitaleUtil.DisplayLuaError(scriptname, ex.DecoratedMessage == null ?
                                                            ex.Message :
                                                            UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message,
                                                        ex.DoNotDecorateMessage);
            } catch (Exception ex) {
                if (!GlobalControls.retroMode)
                    // Special case for infinite loop...
                    if (ex.GetType().ToString() == "System.IndexOutOfRangeException" && ex.StackTrace.StartsWith("  at (wrapper stelemref) object:stelemref (object,intptr,object)"
                      + "\r\n  at MoonSharp.Interpreter.DataStructs.FastStack`1[MoonSharp.Interpreter.DynValue].Push"))
                        UnitaleUtil.DisplayLuaError(scriptname + ", calling the function " + function, "<b>Possible infinite loop</b>\n\nThis is a " + ex.GetType() + " error.\n\n"
                          + "You almost definitely have an infinite loop in your code. A function tried to call itself infinitely. It could be a normal function or a metatable function."
                          + "\n\n\nFull stracktrace (see CYF output log at <b>" + Application.persistentDataPath + "/output_log.txt</b>):\n\n" + ex.StackTrace);
                    else
                        UnitaleUtil.DisplayLuaError(scriptname + ", calling the function " + function, "This is a " + ex.GetType() + " error. Contact the dev and show them this screen, this must be an engine-side error.\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
            }
            return d;
        }
    }

    internal void Bind(string key, object func) {
        script.Globals[key] = func;
        /*try {
            //The script where the function to test is
            EventManager em = GameObject.Find("Main Camera OW").GetComponent<EventManager>();
            //I have to keep the type only, I don't need the script where it belongs (it would not work if I don't do this)
            if (func.GetType().ToString().Split('+')[1] == typeof(Action<DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue>).ToString().Split('+')[1])
                script.Globals[key] = (Action<DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue>)
                                      ((v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => em.FunctionLauncher(key, new DynValue[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10 } ));
        } catch { }*/
    }

    internal void BindDyn(string key, DynValue func) { script.Globals[key] = func; }

    private delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
}