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
[System.Serializable]
public class GameState {
    public static GameState current;
    public Hashtable soundDictionary;
    public ControlPanel controlpanel;
    public PlayerCharacter player;
    public Dictionary<string, string> playerVariablesStr = new Dictionary<string, string>();
    public Dictionary<string, double> playerVariablesNum = new Dictionary<string, double>();
    public Dictionary<string, bool> playerVariablesBool = new Dictionary<string, bool>();
    public string lastScene = null;
    public Dictionary<string, MapData> mapInfos = new Dictionary<string, MapData>();
    public Dictionary<string, TempMapData> tempMapInfos = new Dictionary<string, TempMapData>();
    public List<string> inventory = new List<string>();
    public List<string> boxContents = new List<string>();
    public float playerTime;
    public string CYFversion;

    [System.Serializable]
    public struct EventInfos {
        public int CurrPage;
        public bool NoCollision;
        public string CurrSpriteNameOrCYFAnim;
        public Vect Anchor;
        public Vect Pivot;
    }

    [System.Serializable]
    public struct MapData {
        public string Name;
        public string Music;
        public string ModToLoad;
        public bool MusicKept;
        public bool NoRandomEncounter;
        public Dictionary<string, EventInfos> EventInfo;
    }

    [System.Serializable]
    public struct TempMapData {
        public string Name;
        public string Music;
        public bool MusicChanged;
        public string ModToLoad;
        public bool ModToLoadChanged;
        public bool MusicKept;
        public bool MusicKeptChanged;
        public bool NoRandomEncounter;
        public bool NoRandomEncounterChanged;
    }

    [System.Serializable]
    public struct Vect {
        public float x;
        public float y;
        public float z;
    }

    public void SaveGameVariables() {
        CYFversion = GlobalControls.CYFversion;

        try {
            GameObject Player = GameObject.Find("Player");
            LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(Player.transform.position.x));
            LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(Player.transform.position.y));
            LuaScriptBinder.Set(null, "PlayerPosZ", DynValue.NewNumber(Player.transform.position.z));
        } catch {
            LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(SaveLoad.savedGame.playerVariablesNum["PlayerPosX"]));
            LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(SaveLoad.savedGame.playerVariablesNum["PlayerPosY"]));
            LuaScriptBinder.Set(null, "PlayerPosZ", DynValue.NewNumber(SaveLoad.savedGame.playerVariablesNum["PlayerPosZ"]));
        }

        string mapName;
        if (UnitaleUtil.MapCorrespondanceList.ContainsKey(SceneManager.GetActiveScene().name))                        mapName = UnitaleUtil.MapCorrespondanceList[SceneManager.GetActiveScene().name];
        else if (GlobalControls.nonOWScenes.Contains(SceneManager.GetActiveScene().name) || GlobalControls.isInFight) mapName = SaveLoad.savedGame.lastScene;
        else                                                                                                          mapName = SceneManager.GetActiveScene().name;
        lastScene = mapName;

        soundDictionary = MusicManager.hiddenDictionary;
        controlpanel = ControlPanel.instance;
        player = PlayerCharacter.instance;

        inventory.Clear();
        foreach (UnderItem item in Inventory.inventory)
            inventory.Add(item.Name);

        boxContents.Clear();
        foreach (UnderItem item in ItemBox.items)
            boxContents.Add(item.Name);

        playerTime = Time.time - GlobalControls.overworldTimestamp;

        try {
            foreach (string key in LuaScriptBinder.GetSavedDictionary().Keys) {
                DynValue dv;
                LuaScriptBinder.GetSavedDictionary().TryGetValue(key, out dv);
                switch (dv.Type) {
                    case DataType.Number: playerVariablesNum.Add(key, dv.Number); break;
                    case DataType.String: playerVariablesStr.Add(key, dv.String); break;
                    case DataType.Boolean: playerVariablesBool.Add(key, dv.Boolean); break;
                    default: UnitaleUtil.WriteInLogAndDebugger("SaveLoad: This DynValue can't be added to the save because it is unserializable."); break;
                }
            }
        } catch { }

        mapInfos = GlobalControls.GameMapData;
        tempMapInfos = GlobalControls.TempGameMapData;
    }

    public void LoadGameVariables(bool loadGlobals = true) {
        GlobalControls.TempGameMapData = tempMapInfos;
        GlobalControls.GameMapData = mapInfos;

        foreach (string key in playerVariablesNum.Keys) {
            if (loadGlobals || key.Contains("PlayerPos")) {
                double a;
                playerVariablesNum.TryGetValue(key, out a);
                LuaScriptBinder.Set(null, key, DynValue.NewNumber(a));
            }
        }
        if (loadGlobals) {
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
        }

        Inventory.inventory.Clear();
        foreach (string str in inventory)
            Inventory.inventory.Add(new UnderItem(str));

        ItemBox.items.Clear();
        foreach (string str in boxContents)
            ItemBox.items.Add(new UnderItem(str));

        PlayerCharacter.instance = player;
        ControlPanel.instance = controlpanel;
        MusicManager.hiddenDictionary = soundDictionary;

        string mapName;
        if (UnitaleUtil.MapCorrespondanceList.ContainsValue(lastScene)) mapName = UnitaleUtil.MapCorrespondanceList.FirstOrDefault(x => x.Value == lastScene).Key;
        else                                                            mapName = lastScene;
        GlobalControls.lastScene = mapName;

        LuaScriptBinder.Set(null, "PlayerMap", DynValue.NewString(mapName));
    }
}

