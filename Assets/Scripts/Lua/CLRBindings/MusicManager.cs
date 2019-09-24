using UnityEngine;
using System.Collections;

/// <summary>
/// Lua binding to manipulate in-game music and play sounds.
/// </summary>
public class MusicManager {
    public static MusicManager instance;
    public static AudioSource src;
    public static Hashtable hiddenDictionary = new Hashtable();
    public static string filename = "empty";

    public static bool IsPlaying {
        get { return src.isPlaying; }
    }
    public static bool isplaying {
        get { return IsPlaying; }
    }

    public static void Play() { src.Play(); }
    public static void Stop() { src.Stop(); }
    public static void Pause() { src.Pause(); }
    public static void Unpause() { src.UnPause(); }

    public static void Volume(float value) {
        if (value < 0) value = 0;
        if (value > 1) value = 1;
        src.volume = value;
    }

    public static void Pitch(float value) {
        if (value < -3) value = -3;
        if (value > 3)  value = 3;
        src.pitch = value;
    }

    public static void LoadFile(string name) {
        if (name == null) {
            UnitaleUtil.WriteInLogAndDebugger("[WARN]Attempted to load a nil value as an Audio file.");
            return;
        }
        
        src.Stop();
        src.clip = AudioClipRegistry.GetMusic(name);
        filename = "music:" + name.ToLower();
        NewMusicManager.audioname["src"] = filename;
        src.Play();
    }

    public static void PlaySound(string name, float volume = 0.65f) {
        if (name == null) {
            UnitaleUtil.WriteInLogAndDebugger("[WARN]Attempted to load a nil value as a sound.");
            return;
        }
        
        try { UnitaleUtil.PlaySound("MusicPlaySound", AudioClipRegistry.GetSound(name), volume); }
        catch {  }
    }

    public static float playtime {
        get { return src.time; }
        set { src.time = value; }
    }

    public static float totaltime {
        get { return src.clip.length; }
    }
    
    public static bool IsStoppedOrNull(AudioSource audio) {
        if (audio != null) {
            if (audio.ToString().ToLower() == "null")  return true;
            if (!audio.isPlaying)                      return true;
            else                                       return false;
        }
        return true;
    }

    public static void StopAll() {
        foreach (AudioSource audioSrc in GameObject.FindObjectsOfType<AudioSource>())
            audioSrc.Stop();
    }

    public static void PauseAll() {
        foreach (AudioSource audioSrc in GameObject.FindObjectsOfType<AudioSource>())
            audioSrc.Pause();
    }

    public static void UnpauseAll() {
        foreach (AudioSource audioSrc in GameObject.FindObjectsOfType<AudioSource>())
            audioSrc.UnPause();
    }

    public static string GetSoundDictionary(string key) {
        if (key == null)
            throw new CYFException("Audio.GetSoundDictionary: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (hiddenDictionary.ContainsKey(key.ToLower()))  return (string)hiddenDictionary[key.ToLower()];
        else                                              return key;
    }

    public static void SetSoundDictionary(string key, string value) {
        if (key == null)
            throw new CYFException("Audio.SetSoundDictionary: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (key == "RESETDICTIONARY")
            hiddenDictionary.Clear();
        else {
            key = key.ToLower();
            if (hiddenDictionary.ContainsKey(key))
                hiddenDictionary.Remove(key);
            hiddenDictionary.Add(key, value);
        }
    }

    //[System.Runtime.CompilerServices.IndexerName("SoundDictionary")]
    public string this[string key] {
        get { return GetSoundDictionary(key); }
        set { SetSoundDictionary(key, value); }
    }
}