using UnityEngine;
using MoonSharp.Interpreter;

public class LuaMapOW {
    public ScriptWrapper appliedScript;

    [MoonSharpHidden] public LuaMapOW() { }

    public delegate void LoadedAction(string name, object args);
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
}
