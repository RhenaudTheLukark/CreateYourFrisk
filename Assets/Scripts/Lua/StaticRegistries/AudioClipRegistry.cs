using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AudioClipRegistry {
    public static Dictionary<string, AudioClip> dict = new Dictionary<string, AudioClip>();
    private static string currentPath;
    private static string[] extensions = new string[] { ".ogg", ".wav" }; // Note: also requires support from FileLoader.getAudioClip().
    private static Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    public static void Start() { loadAllFrom(FileLoader.pathToDefaultFile("")); }

    public static AudioClip Get(string key) {
        string k = key;
        key = key.ToLower();
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return tryLoad(k);
    }

    public static AudioClip tryLoad(string key) {
        string k = key;
        key = key.ToLower();
        if (dictMod.ContainsKey(key))
            dict[key] = FileLoader.getAudioClip(currentPath, dictMod[key].FullName);
        else if (dictDefault.ContainsKey(key))
            dict[key] = FileLoader.getAudioClip(currentPath, dictDefault[key].FullName);
        else {
            if (GlobalControls.retroMode)
                UnitaleUtil.Warn("The audio file \"" + k + "\" doesn't exist.");
            else
                throw new CYFException("Attempted to load the audio file \"" + k + "\" from either a mod or default directory, but it was missing in both.");
            return null;
        }
        return dict[key];
    }

    public static AudioClip GetVoice(string key) {
        if (key.Length < 14)                                         key = "Sounds/Voices/" + key;
        else if (key.Substring(0, 14).ToLower() != "sounds/voices/") key = "Sounds/Voices/" + key;
        return Get(key);
    }

    public static AudioClip GetSound(string key) {
        string key2 = key;
        key = (string)MusicManager.hiddenDictionary[key];
        if (key == null)                            key = key2;
        if (key.Length < 7)                         key = "Sounds/" + key;
        else if (key.Substring(0, 7).ToLower() != "sounds/")  key = "Sounds/" + key;
        return Get(key);
    }

    public static AudioClip GetMusic(string key) {
        if (key.Length < 6) key = "Audio/" + key;
        else if (key.Substring(0, 6).ToLower() != "audio/")
            key = "Audio/" + key;
        return Get(key);
    }

    public static void Set(string key, AudioClip value) {
        if (dict.ContainsKey(key.ToLower()))
            dict.Remove(key.ToLower());
        dict[key.ToLower()] = value;
    }

    public static void init() {
        dict.Clear();
        loadAllFrom(FileLoader.pathToModFile(""), true);
    }

    /*private static void loadAllFrom(string directoryPath, bool mod = false) {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfo = dInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(file => extensions.Contains(file.Extension)).ToArray();
        foreach (FileInfo file in fInfo) {
            string voiceName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            AudioClip temp;
            dict.TryGetValue(voiceName, out temp);

            if (dict.ContainsKey(voiceName) && temp == FileLoader.getAudioClip(directoryPath, file.FullName) &&!mod)
                continue;

            //string voiceName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            //if (dict.ContainsKey(voiceName))
            //    continue;
            FileLoader.getAudioClip(directoryPath, file.FullName);
        }
    }*/

    private static void loadAllFrom(string directoryPath, bool mod = false) {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfo;

        if (!dInfo.Exists) {
            UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found.\nAre you sure it exists?");
            throw new CYFException("mod loading");
        }

        fInfo = dInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(file => extensions.Contains(file.Extension)).ToArray();

        if (mod) {
            currentPath = directoryPath;
            dictMod.Clear();
            foreach (FileInfo file in fInfo)
                dictMod[FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower()] = file;
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfo)
                dictDefault[FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower()] = file;
        }
    }
}