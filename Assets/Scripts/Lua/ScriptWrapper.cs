using UnityEngine;
using MoonSharp.Interpreter;
using System;

public class ScriptWrapper {
    public Script script;
    public string scriptname = "???";
    public string text = "";

    public DynValue this[string key] {
        get { return this.GetVar(key); }
        set { this.SetVar(key, value); }
    }

    public ScriptWrapper(/*bool overworld = false*/) {
        script = LuaScriptBinder.boundScript(/*overworld*/);
        this.Bind("_getv", (Func<Script, string, DynValue>)this.GetVar);
        string toDoString = "setmetatable({}, {__index=function(t, name) return _getv(name) end}) ";
        text = toDoString;
        script.DoString(toDoString);
    }

    internal DynValue DoString(string source) { return script.DoString(source); }

    public void SetVar(string key, DynValue value) { script.Globals.Set(key, MoonSharpUtil.CloneIfRequired(script, value)); }

    public DynValue GetVar(string key) { return GetVar(null, key); }

    public DynValue GetVar(Script caller, string key) {
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
                UnitaleUtil.displayLuaError(scriptname, "Attempted to call the function " + function + " but it didn't exist.");
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
                    catch (InterpreterException ex) { UnitaleUtil.displayLuaError(scriptname, ex.DecoratedMessage == null ? 
                                                                                                  ex.Message : 
                                                                                                  ex.DecoratedMessage.Substring(5).Contains("chunk_") ? 
                                                                                                      ex.Message : 
                                                                                                      ex.DecoratedMessage); } 
                    catch (Exception ex) {
                        if (!GlobalControls.retroMode)
                            UnitaleUtil.displayLuaError(scriptname + ", calling the function " + function, "This is a " + ex.GetType() + " error. Contact the dev and show him this screen, this must be an engine-side error.\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
                    }
                } else if (e.GetType() == typeof(InterpreterException) || e.GetType().BaseType == typeof(InterpreterException) || e.GetType().BaseType.BaseType == typeof(InterpreterException))
                    UnitaleUtil.displayLuaError(scriptname, ((InterpreterException)e).DecoratedMessage == null ? 
                                                                ((InterpreterException)e).Message : 
                                                                ((InterpreterException)e).DecoratedMessage.Substring(5).Contains("chunk_") ? 
                                                                    ((InterpreterException)e).Message : 
                                                                    ((InterpreterException)e).DecoratedMessage);
                else if (!GlobalControls.retroMode)
                    UnitaleUtil.displayLuaError(scriptname + ", calling the function " + function, "This is a " + e.GetType() + " error. Contact the dev and show him this screen, this must be an engine-side error.\n\n" + e.Message + "\n\n" + e.StackTrace + "\n");
            }
            return d;
        } else {
            DynValue d = DynValue.Nil;
            try { d = script.Call(script.Globals[function]); } 
            catch (InterpreterException ex) {
                UnitaleUtil.displayLuaError(scriptname, ex.DecoratedMessage == null ? 
                                                            ex.Message : 
                                                            ex.DecoratedMessage.Substring(5).Contains("chunk_") ? 
                                                                ex.Message : 
                                                                ex.DecoratedMessage);
            } catch (Exception ex) {
                if (!GlobalControls.retroMode)
                    UnitaleUtil.displayLuaError(scriptname + ", calling the function " + function, "This is a " + ex.GetType() + " error. Contact the dev and show him this screen, this must be an engine-side error.\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
            }
            return d;
        }
    }
    
    //Used for enemies
    public LuaSpriteController monstersprite;

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