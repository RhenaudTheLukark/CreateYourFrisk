using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class SpriteRegistry {
    private static readonly Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
    public static Image GENERIC_SPRITE_PREFAB = Resources.Load<Image>("Prefabs/generic_sprite");
    public static Sprite EMPTY_SPRITE;
    private static readonly Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static readonly Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    public static void Start() { LoadAllFrom(FileLoader.PathToDefaultFile("Sprites")); }

    public static void Set(string key, Sprite value) {
        key += key.EndsWith(".png") ? "" : ".png";
        FileLoader.SanitizePath(ref key, "Sprites/");
        key = key.ToLower();
        dict[(UnitaleUtil.IsOverworld ? "ow::" : "b::") + key] = value;
    }

    public static Sprite Get(string key) {
        string dictKey = key + (key.EndsWith(".png") ? "" : ".png");
        FileLoader.SanitizePath(ref dictKey, "Sprites/");
        dictKey = (UnitaleUtil.IsOverworld ? "ow::" : "b::") + dictKey.ToLower();
        return dict.ContainsKey(dictKey) ? dict[dictKey] : TryLoad(key);
    }

    public static Sprite GetMugshot(string key) { return Get("mugshots/" + key.ToLower()); }

    private static Sprite TryLoad(string key) {
        string fileName = key;
        string dictKey = (UnitaleUtil.IsOverworld ? "ow::" : "b::") + key;

        key += key.EndsWith(".png") ? "" : ".png";
        FileLoader.SanitizePath(ref key, "Sprites/");
        key = key.ToLower();
        if (dictMod.ContainsKey(key))          dict[dictKey] = SpriteUtil.FromFile(fileName + ".png");
        else if (dictDefault.ContainsKey(key)) dict[dictKey] = SpriteUtil.FromFile(fileName + ".png");
        else                                   return TryFetchFromMod(fileName, dictKey);
        return dict[dictKey];
    }

    private static Sprite TryFetchFromMod(string key, string dictKey) {
        FileInfo tryF = new FileInfo(Path.Combine(FileLoader.PathToModFile("Sprites"), key) + (key.EndsWith(".png") ? "" : ".png"));
        if (!tryF.Exists) return null;

        dictDefault[key.ToLower()] = tryF;
        dict[dictKey]              = SpriteUtil.FromFile(key + ".png");
        return dict[dictKey];
    }

    public static void Init() {
        LoadAllFrom(FileLoader.PathToModFile("Sprites"), true);
        if (EMPTY_SPRITE == null) EMPTY_SPRITE = Get("empty");
    }

    private static void LoadAllFrom(string directoryPath, bool mod = false) {
        dict.Clear();
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);

        if (!dInfo.Exists) {
            UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found, or at least its \"Sprites\" folder can't be found.\nAre you sure it exists?");
            throw new CYFException("mod loading");
        }

        FileInfo[] fInfoTest = dInfo.GetFiles("*.png", SearchOption.AllDirectories);

        if (mod) {
            dictMod.Clear();
            foreach (FileInfo file in fInfoTest) {
                string k = file.FullName.Substring(directoryPath.Length + 1);
                FileLoader.SanitizePath(ref k, "Sprites/");
                dictMod[k.ToLower()] = file;
            }
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfoTest) {
                string k = file.FullName.Substring(directoryPath.Length + 1);
                FileLoader.SanitizePath(ref k, "Sprites/");
                dictDefault[k.ToLower()] = file;
            }
        }
    }
}