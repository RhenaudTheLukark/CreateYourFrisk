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
        dict[ProcessKey(key + (key.EndsWith(".png") ? "" : ".png"))] = value;
    }

    public static Sprite Get(string origKey) {
        origKey += origKey.EndsWith(".png") ? "" : ".png";
        string key = ProcessKey(origKey);
        return dict.ContainsKey(key) ? dict[key] : TryLoad(origKey, key);
    }

    public static Sprite GetMugshot(string key) { return Get("Mugshots/" + key); }

    private static Sprite TryLoad(string origKey, string key) {
        if (dictMod.ContainsKey(key))          dict[key] = SpriteUtil.FromFile(origKey);
        else if (dictDefault.ContainsKey(key)) dict[key] = SpriteUtil.FromFile(origKey);
        else                                   return TryFetchFromMod(origKey, key);
        return dict[key];
    }

    private static Sprite TryFetchFromMod(string origKey, string key) {
        FileInfo tryF = new FileInfo(Path.Combine(FileLoader.PathToModFile("Sprites"), origKey) + (origKey.EndsWith(".png") ? "" : ".png"));
        if (!tryF.Exists) return null;

        dictMod[key] = tryF;
        dict[key] = SpriteUtil.FromFile(origKey);
        return dict[key];
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
            foreach (FileInfo file in fInfoTest)
                dictMod[ProcessKey(file.FullName.Substring(directoryPath.Length + 1))] = file;
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfoTest) {
                string key = ProcessKey(file.FullName.Substring(directoryPath.Length + 1));
                dictDefault[key] = file;
            }
        }
    }

    private static string ProcessKey(string key) {
        FileLoader.SanitizePath(ref key, "Sprites/");
        return key.ToLower();
    }
}