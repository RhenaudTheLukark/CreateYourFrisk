using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewMusicManager {
    public static Hashtable audiolist = new Hashtable();
    public static Dictionary<string, string> audioname = new Dictionary<string, string>();
   
    public static void CreateChannel(string name) {
        if (audiolist.ContainsKey(name)) {
            Debug.LogWarning("The audio channel " + name + " already exists.");
            return;
        }
        GameObject go = new GameObject("AudioChannel" + audiolist.Count + ": " + name, typeof(AudioSource));
        GameObject.DontDestroyOnLoad(go);
        audiolist.Add(name, go.GetComponent<AudioSource>());
        if (!audioname.ContainsKey(name))
            audioname.Add(name, "empty");
    }

    public static AudioSource CreateChannelAndGetAudioSource(string name) {
        if (audiolist.ContainsKey(name)) {
            Debug.LogWarning("The audio channel " + name + " already exists.");
            return GameObject.Find("AudioChannel" + audiolist.Count + ": " + name).GetComponent<AudioSource>();
        }
        GameObject go = new GameObject("AudioChannel" + audiolist.Count + ": " + name, typeof(AudioSource));
        audiolist.Add(name, go.GetComponent<AudioSource>());
        if (!audioname.ContainsKey(name))
            audioname.Add(name, "empty");
        return go.GetComponent<AudioSource>();
    }

    public static void DestroyChannel(string name) {
        if (name == "src")                throw new CYFException("You can't delete the audio channel \"src\".");
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        try {
            GameObject.Destroy(((AudioSource)audiolist[name]).gameObject);
        } catch { }        
        audiolist.Remove(name);
        audioname.Remove(name);        
    }

    public static bool Exists(string name) { return audiolist.ContainsKey(name); }

    public static string GetAudioName(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        return audioname[name];
    }

    public static float GetTotalTime(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        if (((AudioSource)audiolist[name]).clip != null) return ((AudioSource)audiolist[name]).clip.length;
        return 0;
    }

    public static void PlayMusic(string name, string music, bool loop = false, float volume = 1) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).Stop();
        ((AudioSource)audiolist[name]).loop = loop;
        ((AudioSource)audiolist[name]).volume = volume;
        ((AudioSource)audiolist[name]).clip = AudioClipRegistry.GetMusic(music);
        audiolist[name] = ((AudioSource)audiolist[name]);
        audioname[name] = "music:" + music.ToLower();
        if (name == "src")
            MusicManager.filename = "music:" + music.ToLower();
        ((AudioSource)audiolist[name]).Play();
    }

    public static void PlaySound(string name, string sound, bool loop = false, float volume = 0.65f) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).Stop();
        ((AudioSource)audiolist[name]).loop = loop;
        ((AudioSource)audiolist[name]).volume = volume;
        ((AudioSource)audiolist[name]).clip = AudioClipRegistry.GetSound(sound);
        audiolist[name] = ((AudioSource)audiolist[name]);
        audioname[name] = "sound:" + sound.ToLower();
        if (name == "src")
            MusicManager.filename = "sound:" + sound.ToLower();
        ((AudioSource)audiolist[name]).Play();
    }

    public static void PlayVoice(string name, string voice, bool loop = false, float volume = 0.65f) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).Stop();
        ((AudioSource)audiolist[name]).loop = loop;
        ((AudioSource)audiolist[name]).volume = volume;
        ((AudioSource)audiolist[name]).clip = AudioClipRegistry.GetVoice(voice);
        audiolist[name] = ((AudioSource)audiolist[name]);
        audioname[name] = "voice:" + voice.ToLower();
        if (name == "src")
            MusicManager.filename = "voice:" + voice.ToLower();
        ((AudioSource)audiolist[name]).Play();
    }

    public static void SetPitch(string name, float value) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        if (value < -3) value = -3;
        if (value > 3)  value = 3;
        ((AudioSource)audiolist[name]).pitch = value;
    }

    public static float GetPitch(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        return ((AudioSource)audiolist[name]).pitch;
    }

    public static void SetVolume(string name, float value) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        if (value < 0)      value = 0;
        else if (value > 1) value = 1;
        ((AudioSource)audiolist[name]).volume = value;
    }

    public static float GetVolume(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        return ((AudioSource)audiolist[name]).volume;
    }

    public static void Play(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).Play();
    }
    public static void Stop(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).Stop();
    }
    public static void Pause(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).Pause();
    }
    public static void Unpause(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).UnPause();
    }

    public static void SetPlayTime(string name, float value) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        ((AudioSource)audiolist[name]).time = value;
    }

    public static float GetPlayTime(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        return ((AudioSource)audiolist[name]).time;
    }

    public static float GetCurrentTime(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        return ((AudioSource)audiolist[name]).time;
    }

    public static void StopAll() {
        foreach (AudioSource audioSrc in audiolist.Values)
            audioSrc.Stop();
    }

    public static void PauseAll() {
        foreach (AudioSource audioSrc in audiolist.Values)
            audioSrc.Pause();
    }

    public static void UnpauseAll() {
        foreach (AudioSource audioSrc in audiolist.Values)
            audioSrc.UnPause();
    }

    public static bool isStopped(string name) {
        if (!audiolist.ContainsKey(name)) throw new CYFException("The audio channel " + name + " doesn't exist.");
        return !((AudioSource)audiolist[name]).isPlaying;
    }
    public static bool IsStopped(string name) { return isStopped(name); }

    public static void OnLevelWasLoaded() {
        audiolist.Clear();
        audioname.Clear();
        audiolist.Add("src", MusicManager.src);
        audioname.Add("src", MusicManager.filename);
        if (PlayerOverworld.audioKept) {
            audiolist.Add("StaticKeptAudio", PlayerOverworld.audioKept);
            audioname.Add("StaticKeptAudio", "Sorry, nyi");
        }
    }
}
