using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;

/// <summary>
/// Class used as a database that is saved and loaded during the game.
/// Is used as the savefile in SaveLoad.
/// </summary>
[System.Serializable] public class AlMightyGameState {
    public static GameState current;
    public Dictionary<string, string> AlMightyVariablesStr = new Dictionary<string, string>();
    public Dictionary<string, double> AlMightyVariablesNum = new Dictionary<string, double>();
    public Dictionary<string, bool> AlMightyVariablesBool = new Dictionary<string, bool>();

    public void UpdateVariables() {
        AlMightyVariablesNum.Clear();
        AlMightyVariablesStr.Clear();
        AlMightyVariablesBool.Clear();
        try {
            foreach (string key in LuaScriptBinder.GetAlMightyDictionary().Keys) {
                DynValue dv;
                LuaScriptBinder.GetAlMightyDictionary().TryGetValue(key, out dv);
                switch (dv.Type) {
                    case DataType.Number:   AlMightyVariablesNum.Add(key, dv.Number);    break;
                    case DataType.String:   AlMightyVariablesStr.Add(key, dv.String);    break;
                    case DataType.Boolean:  AlMightyVariablesBool.Add(key, dv.Boolean);  break;
                    default:                UnitaleUtil.WriteInLogAndDebugger("SaveLoad: This DynValue can't be added to the save because it is unserializable.");  break;
                }
            }
        } catch { }
    }

    public void LoadVariables() {
        foreach (string key in AlMightyVariablesNum.Keys) {
            double a;
            AlMightyVariablesNum.TryGetValue(key, out a);
            LuaScriptBinder.SetAlMighty(null, key, DynValue.NewNumber(a), false);
        }

        foreach (string key in AlMightyVariablesStr.Keys) {
            string a;
            AlMightyVariablesStr.TryGetValue(key, out a);
            LuaScriptBinder.SetAlMighty(null, key, DynValue.NewString(a), false);
        }

        foreach (string key in AlMightyVariablesBool.Keys) {
            bool a;
            AlMightyVariablesBool.TryGetValue(key, out a);
            LuaScriptBinder.SetAlMighty(null, key, DynValue.NewBoolean(a), false);
        }
    }
}
