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

    private static AudioClip Get(string key, string prefix) {
        string k = key;

        key = key.TrimStart('/', '\\');
        string oggKey = key + (key.ToLower().EndsWith(".ogg") ? "" : ".ogg");
        string wavKey = key + (key.ToLower().EndsWith(".wav") ? "" : ".wav");
        if (!FileLoader.SanitizePath(ref oggKey, "", false)) {
            FileLoader.SanitizePath(ref wavKey, "", !GlobalControls.retroMode);
            key = wavKey;
        } else
            key = oggKey;

        string lowerKey = key.ToLower();
        return dict.ContainsKey(lowerKey) ? dict[lowerKey] : TryLoad(k, key, prefix);
    }

    public static AudioClip GetVoice(string key) {
        if (key.Length < 14 || key.Substring(0, 14).ToLower() != "sounds/voices/") key = "Sounds/Voices/" + key;
        return Get(key, "Sounds/Voices/");
    }

    public static AudioClip GetSound(string key) {
        key = (string)MusicManager.hiddenDictionary[key] ?? key;
        if (key.Length < 7 || key.Substring(0, 7).ToLower() != "sounds/") key = "Sounds/" + key;
        return Get(key, "Sounds/");
    }

    public static AudioClip GetMusic(string key) {
        if (key.Length < 6 || key.Substring(0, 6).ToLower() != "audio/") key = "Audio/" + key;
        return Get(key, "Audio/");
    }

    public static AudioClip TryLoad(string origKey, string key, string prefix) {
        string lowerKey = key.ToLower();
        if (dictMod.ContainsKey(lowerKey))          dict[lowerKey] = GetAudioClip(dictMod[lowerKey].FullName);
        else if (dictDefault.ContainsKey(lowerKey)) dict[lowerKey] = GetAudioClip(dictDefault[lowerKey].FullName);
        else {
            AudioClip audio = TryFetchFromMod(origKey, key, prefix);
            if (audio == null) audio = TryFetchFromDefault(origKey, key, prefix);
            if (audio == null) {
                UnitaleUtil.Warn("The audio file \"" + origKey + "\" doesn't exist.");
                return null;
            }
            dict[lowerKey] = audio;
        }
        return dict[lowerKey];
    }

    private static AudioClip TryFetchFromDefault(string origKey, string key, string prefix) {
        FileInfo tryF = new FileInfo(Path.Combine(FileLoader.PathToDefaultFile(""), key));
        if (!tryF.Exists)
            return null;

        dictDefault[key] = tryF;
        dict[key] = GetAudioClip(tryF.FullName);
        return dict[key];
    }

    private static AudioClip TryFetchFromMod(string origKey, string key, string prefix) {
        FileInfo tryF = new FileInfo(Path.Combine(FileLoader.PathToModFile(""), key));
        if (!tryF.Exists)
            return null;

        dictMod[key] = tryF;
        dict[key] = GetAudioClip(tryF.FullName);
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
        if (musicFilePath.ToLower().EndsWith(".ogg"))      type = AudioType.OGGVORBIS;
        else if (musicFilePath.ToLower().EndsWith(".wav")) type = AudioType.WAV;
        else                                               return null;

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