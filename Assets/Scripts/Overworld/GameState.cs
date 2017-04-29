using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;

/// <summary>
/// Class used as a database that is saved and loaded during the game. 
/// Is used as the savefile in SaveLoad.
/// </summary>
[System.Serializable] public class GameState {
    public static GameState current;
    public Hashtable soundDictionary;
    public ControlPanel controlpanel;
    public PlayerCharacter player;
    public string playerHeader;
    public Dictionary<string, string> playerVariablesStr = new Dictionary<string, string>();
    public Dictionary<string, double> playerVariablesNum = new Dictionary<string, double>();
    public Dictionary<string, bool> playerVariablesBool = new Dictionary<string, bool>();

    //GlobalControls values, unserializable so I have to copy them
    public string lastScene = null;
    public Dictionary<int, Dictionary<string, int>> MapEventPages = new Dictionary<int, Dictionary<string, int>>();
    public float playerPosX = 0, playerPosY = 0, playerPosZ = 0;

    public void SaveGameVariables() {
        playerVariablesStr.Clear();
        playerVariablesNum.Clear();
        playerVariablesBool.Clear();
        
        LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(GameObject.Find("Player").transform.position.x));
        LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(GameObject.Find("Player").transform.position.y));

        try {
            foreach (string key in LuaScriptBinder.GetDictionary().Keys) {
                DynValue dv;
                LuaScriptBinder.GetDictionary().TryGetValue(key, out dv);
                switch (dv.Type) {
                    case DataType.Number:   playerVariablesNum.Add(key, dv.Number);    break;
                    case DataType.String:   playerVariablesStr.Add(key, dv.String);    break;
                    case DataType.Boolean:  playerVariablesBool.Add(key, dv.Boolean);  break;
                    default:                UnitaleUtil.writeInLog("This DynValue can't be added to the save because it is unserializable.");  break;
                }
            }
        } catch { }
        
        string mapName;
        if (UnitaleUtil.MapCorrespondanceList.ContainsKey(SceneManager.GetActiveScene().name))  mapName = UnitaleUtil.MapCorrespondanceList[SceneManager.GetActiveScene().name];
        else                                                                                    mapName = SceneManager.GetActiveScene().name;
        lastScene = mapName;
        MapEventPages = GlobalControls.MapEventPages;
        soundDictionary = MusicManager.hiddenDictionary;
        player = PlayerCharacter.instance;
        controlpanel = ControlPanel.instance;
        playerHeader = CYFAnimator.specialPlayerHeader;

        Vector3 playerPos = GameObject.Find("Player").transform.position;
        playerPosX = playerPos.x;
        playerPosY = playerPos.y;
        playerPosZ = playerPos.z;
    }

    public void LoadGameVariables() {
        GlobalControls.MapEventPages = MapEventPages;

        foreach (string key in playerVariablesNum.Keys) {
            double a;
            playerVariablesNum.TryGetValue(key, out a);
            LuaScriptBinder.Set(null, key, DynValue.NewNumber(a));
        }

        foreach (string key in playerVariablesStr.Keys) {
            string a;
            playerVariablesStr.TryGetValue(key, out a);
            LuaScriptBinder.Set(null, key, DynValue.NewString(a));
        }

        foreach (string key in playerVariablesBool.Keys) {
            bool a;
            playerVariablesBool.TryGetValue(key, out a);
            LuaScriptBinder.Set(null, key, DynValue.NewBoolean(a));
        }

        PlayerCharacter.instance = player;
        ControlPanel.instance = controlpanel;
        MusicManager.hiddenDictionary = soundDictionary;
        string mapName;
        if (UnitaleUtil.MapCorrespondanceList.ContainsValue(lastScene)) mapName = UnitaleUtil.MapCorrespondanceList.FirstOrDefault(x => x.Value == lastScene).Key;
        else                                                            mapName = lastScene;
        GlobalControls.lastScene = mapName;
        LuaScriptBinder.Set(null, "PlayerMap", DynValue.NewString(mapName));
        CYFAnimator.specialPlayerHeader = playerHeader;
    }
}
