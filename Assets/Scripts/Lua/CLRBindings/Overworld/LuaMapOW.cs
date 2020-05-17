using UnityEngine;
using MoonSharp.Interpreter;
using UnityEngine.SceneManagement;

public class LuaMapOW {
    public ScriptWrapper appliedScript;

    [MoonSharpHidden] public LuaMapOW() { }

    public delegate void LoadedAction(string coroName, object args, string evName);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    [CYFEventFunction] public string GetMusic() { try { return Object.FindObjectOfType<MapInfos>().music; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetMusic(string value) { Object.FindObjectOfType<MapInfos>().music = value; appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public string GetModToLoad() { try { return Object.FindObjectOfType<MapInfos>().modToLoad; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetModToLoad(string value) { Object.FindObjectOfType<MapInfos>().modToLoad = value; appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public bool GetNoRandomEncounter() { try { return Object.FindObjectOfType<MapInfos>().noRandomEncounter; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetNoRandomEncounter(bool value) { Object.FindObjectOfType<MapInfos>().noRandomEncounter = value; appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public bool GetMusicKept() { try { return Object.FindObjectOfType<MapInfos>().isMusicKeptBetweenBattles; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetMusicKept(bool value) {
        if (value != Object.FindObjectOfType<MapInfos>().isMusicKeptBetweenBattles) {
            Object.FindObjectOfType<MapInfos>().isMusicKeptBetweenBattles = value;
            AudioSource oldAs = value ? (AudioSource)(NewMusicManager.audiolist["src"]) : (AudioSource)(NewMusicManager.audiolist["StaticKeptAudio"]);
            AudioSource newAs = value ? (AudioSource)(NewMusicManager.audiolist["StaticKeptAudio"]) : (AudioSource)(NewMusicManager.audiolist["src"]);
            if (value) {
                newAs.clip = oldAs.clip;
                newAs.volume = oldAs.volume;
                newAs.pitch = oldAs.pitch;
                newAs.loop = oldAs.loop;
                newAs.Play();
                newAs.time = oldAs.time;
                oldAs.Stop();
            }
        }
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public string GetName() {
        try { return SceneManager.GetActiveScene().name; }
        finally { appliedScript.Call("CYFEventNextCommand"); }
    }
    [CYFEventFunction] public string GetSaveName(string mapName) {
        try {
            string result;
            UnitaleUtil.MapCorrespondanceList.TryGetValue(mapName, out result);
            return result;
        }
        finally { appliedScript.Call("CYFEventNextCommand"); }
    }
    [CYFEventFunction] public string GetMusicMap(string mapName) {
        if (SceneManager.GetActiveScene().name == mapName) return GetMusic();
        else
            try { return (string)EventManager.TryGetMapValue(mapName, "Music"); }
            finally { appliedScript.Call("CYFEventNextCommand"); }
    }
    [CYFEventFunction] public void SetMusicMap(string mapName, string value) {
        if (SceneManager.GetActiveScene().name == mapName) SetMusic(value);
        else                                               EventManager.TrySetMapValue(mapName, "Music", value);
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public string GetModToLoadMap(string mapName) {
        if (SceneManager.GetActiveScene().name == mapName) return GetModToLoad();
        else
            try { return (string)EventManager.TryGetMapValue(mapName, "ModToLoad"); } finally { appliedScript.Call("CYFEventNextCommand"); }
    }
    [CYFEventFunction] public void SetModToLoadMap(string mapName, string value) {
        if (SceneManager.GetActiveScene().name == mapName) SetModToLoad(value);
        else                                               EventManager.TrySetMapValue(mapName, "ModToLoad", value);
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public bool GetNoRandomEncounterMap(string mapName) {
        if (SceneManager.GetActiveScene().name == mapName) return GetMusicKept();
        else
            try { return (bool)EventManager.TryGetMapValue(mapName, "MusicKept"); } finally { appliedScript.Call("CYFEventNextCommand"); }
    }
    [CYFEventFunction] public void SetNoRandomEncounterMap(string mapName, bool value) {
        if (SceneManager.GetActiveScene().name == mapName) SetNoRandomEncounter(value);
        else                                               EventManager.TrySetMapValue(mapName, "NoRandomEncounter", value);
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public bool GetMusicKeptMap(string mapName) {
        if (SceneManager.GetActiveScene().name == mapName) return GetNoRandomEncounter();
        else
            try { return (bool)EventManager.TryGetMapValue(mapName, "NoRandomEncounter"); } finally { appliedScript.Call("CYFEventNextCommand"); }
    }
    [CYFEventFunction] public void SetMusicKeptMap(string mapName, bool value) {
        if (SceneManager.GetActiveScene().name == mapName) SetMusicKept(value);
        else                                               EventManager.TrySetMapValue(mapName, "MusicKept", value);
        appliedScript.Call("CYFEventNextCommand");
    }
    [CYFEventFunction] public bool HasPlayerBeenInMap(string mapName) {
        try {
            if (SceneManager.GetActiveScene().name == mapName)
                return true;
            foreach (GameState.MapData md in GlobalControls.GameMapData.Values)
                if (md.Name == mapName)
                    return true;
            return false;
        } finally { appliedScript.Call("CYFEventNextCommand"); }
    }
}
