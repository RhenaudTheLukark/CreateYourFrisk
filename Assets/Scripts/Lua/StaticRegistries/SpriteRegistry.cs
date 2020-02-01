using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class SpriteRegistry {
    private static Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
    public static Image GENERIC_SPRITE_PREFAB;
    public static Sprite EMPTY_SPRITE;
    private static Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    public static void Start() {
        loadAllFrom(FileLoader.pathToDefaultFile("Sprites"));
    }

    public static void Set(string key, Sprite value) { dict[(UnitaleUtil.IsOverworld ? "ow" : "b") + key.ToLower()] = value; }

    public static Sprite Get(string key) {
        key = key.ToLower();
        string dictKey = (UnitaleUtil.IsOverworld ? "ow" : "b") + key;
        if (dict.ContainsKey(dictKey))  return dict[dictKey];
        else                            return tryLoad(key);
        //return null;
    }

    private static Sprite tryLoad(string key) {
        string dictKey = (UnitaleUtil.IsOverworld ? "ow" : "b") + key;
        if      (dictMod.ContainsKey(key))
            dict[dictKey] = SpriteUtil.FromFile(dictMod[key].FullName);
        else if (dictDefault.ContainsKey(key))
            dict[dictKey] = SpriteUtil.FromFile(dictDefault[key].FullName);
        else
            return null;
        return dict[dictKey];
    }

    public static Sprite GetMugshot(string key) {
        return Get("mugshots/" + key.ToLower());
    }

    public static void init() {
        //dict.Clear();
        GENERIC_SPRITE_PREFAB = Resources.Load<Image>("Prefabs/generic_sprite");
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, new Color(0, 0, 0, 0));
        tex.Apply();
        EMPTY_SPRITE = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        EMPTY_SPRITE.name = "blank";
        string modPath = FileLoader.pathToModFile("Sprites");
        //string defaultPath = FileLoader.pathToDefaultFile("Sprites");
        //loadAllFrom(defaultPath);
        prepareMod(modPath);
    }

    private static void prepareMod(string directoryPath) {
        dict.Clear();

        loadAllFrom(directoryPath, true);
    }

    private static void loadAllFrom(string directoryPath, bool mod = false) {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfoTest;

        if (!dInfo.Exists) {
            UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found, or at least its \"Sprites\" folder can't be found.\nAre you sure it exists?");
            throw new CYFException("mod loading");
        }

        fInfoTest = dInfo.GetFiles("*.png", SearchOption.AllDirectories);

        if (mod) {
            dictMod.Clear();
            foreach (FileInfo file in fInfoTest)
                dictMod[FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower()] = file;
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfoTest)
                dictDefault[FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower()] = file;
        }
        /*foreach (FileInfo file in fInfoTest) {
            string imageName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            Sprite temp;
            dict.TryGetValue(imageName, out temp);

            if (dict.ContainsKey(imageName) && temp == SpriteUtil.fromFile(file.FullName) &&!mod)
                continue;
            else if (dict.ContainsKey(imageName))
                dict.Remove(imageName);

            //Set(script_prefix + scriptName, FileLoader.getTextFrom(file.FullName));
            //string imageName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            //if (dict.ContainsKey(imageName))
            //    continue;

            dict.Add(imageName, SpriteUtil.fromFile(file.FullName));
        }*/
    }
}