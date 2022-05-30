using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AudioClipRegistry {
    public static Dictionary<string, AudioClip> dict = new Dictionary<string, AudioClip>();
    private static readonly string[] extensions = { ".ogg", ".wav" }; // Note: also requires support from FileLoader.getAudioClip().
    private static readonly Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static readonly Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    public static void Start() { LoadAllFrom(FileLoader.PathToDefaultFile("")); }

    public static void Set(string key, AudioClip value) {
        if (dict.ContainsKey(key.ToLower()))
            dict.Remove(key.ToLower());
        dict[key.ToLower()] = value;
    }

    private static AudioClip Get(string key) {
        string k = key;

        string oggKey = key + (key.EndsWith(".ogg") ? "" : ".ogg");
        string wavKey = key + (key.EndsWith(".wav") ? "" : ".wav");
        if (!FileLoader.SanitizePath(ref oggKey, "", false)) {
            FileLoader.SanitizePath(ref wavKey, "");
            key = wavKey.ToLower();
        } else
            key = oggKey.ToLower();

        return dict.ContainsKey(key) ? dict[key] : TryLoad(key, k);
    }

    public static AudioClip GetVoice(string key) {
        if (key.Length < 14 || key.Substring(0, 14).ToLower() != "sounds/voices/") key = "Sounds/Voices/" + key;
        return Get(key);
    }

    public static AudioClip GetSound(string key) {
        key = (string)MusicManager.hiddenDictionary[key] ?? key;
        if (key.Length < 7 || key.Substring(0, 7).ToLower() != "sounds/") key = "Sounds/" + key;
        return Get(key);
    }

    public static AudioClip GetMusic(string key) {
        if (key.Length < 6 || key.Substring(0, 6).ToLower() != "audio/") key = "Audio/" + key;
        return Get(key);
    }

    public static AudioClip TryLoad(string key, string k) {
        if (dictMod.ContainsKey(key))          dict[key] = GetAudioClip(dictMod[key].FullName);
        else if (dictDefault.ContainsKey(key)) dict[key] = GetAudioClip(dictDefault[key].FullName);
        else {
            if (GlobalControls.retroMode) UnitaleUtil.Warn("The audio file \"" + k + "\" doesn't exist.");
            else                          throw new CYFException("Attempted to load the audio file \"" + k + "\" from either a mod or default directory, but it was missing in both.");
            return null;
        }
        return dict[key];
    }

    public static void Init() { LoadAllFrom(FileLoader.PathToModFile(""), true); }

    private static void LoadAllFrom(string directoryPath, bool mod = false) {
        dict.Clear();
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);

        if (!dInfo.Exists) {
            UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found.\nAre you sure it exists?");
            throw new CYFException("mod loading");
        }

        FileInfo[] fInfo = dInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(file => extensions.Contains(file.Extension)).ToArray();

        if (mod) {
            dictMod.Clear();
            foreach (FileInfo file in fInfo) {
                string k = file.FullName.Substring(directoryPath.Length + 1);
                FileLoader.SanitizePath(ref k, "");
                dictMod[k.ToLower()] = file;
            }
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfo) {
                string k = file.FullName.Substring(directoryPath.Length + 1);
                FileLoader.SanitizePath(ref k, "");
                dictDefault[k.ToLower()] = file;
            }
        }
    }

    /// <summary>
    /// Get an AudioClip at the given full path. Attempts to retrieve it from the AudioClipRegistry first by using folderRoot to extract the clip's name, otherwise attempts to load from disk.
    /// </summary>
    /// <param name="musicFilePath">Full path to a file.</param>
    /// <returns>AudioClip object on successful load, otherwise null.</returns>
    public static AudioClip GetAudioClip(string musicFilePath) {
        AudioType type;
        if (musicFilePath.EndsWith(".ogg"))      type = AudioType.OGGVORBIS;
        else if (musicFilePath.EndsWith(".wav")) type = AudioType.WAV;
        else                                     return null;

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(new Uri(musicFilePath).AbsoluteUri.Replace("+", "%2B"), type);
        www.SendWebRequest();
        while (!www.isDone) { } // hold up a bit while it's loading; delay isn't noticeable and loading will fail otherwise

        AudioClip music = DownloadHandlerAudioClip.GetContent(www);
        music.name = musicFilePath;
        music.LoadAudioData();

        Set(musicFilePath, music);
        return music;
    }
}