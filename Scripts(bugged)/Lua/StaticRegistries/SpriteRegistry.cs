using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class SpriteRegistry {
    private static Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
    public static Image GENERIC_SPRITE_PREFAB;
    public static Sprite DEFAULT_SPRITE;
    public static Sprite EMPTY_SPRITE;
    private static Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    public static void Start() {
        loadAllFrom(FileLoader.pathToDefaultFile("Sprites"));
    }

    public static Sprite Get(string key) {
        key = key.ToLower();
        if (dict.ContainsKey(key))  return dict[key];
        else                        return tryLoad(key);
        //return null;
    }

    public static Sprite GetMugshot(string key) {
        key = key.ToLower();
        key = "mugshots/" + key;
        return Get(key);
    }

    public static Sprite tryLoad(string key) {
        if      (dictMod.ContainsKey(key))      dict[key] = SpriteUtil.FromFile(dictMod[key].FullName);
        else if (dictDefault.ContainsKey(key))  dict[key] = SpriteUtil.FromFile(dictDefault[key].FullName);
        else                                    return null;
        return dict[key];
    }

    public static void Set(string key, Sprite value) { dict[key.ToLower()] = value; }

    public static void init() {
        //dict.Clear();
        Image go = Resources.Load<Image>("Prefabs/generic_sprite");
        if (go != null && go.ToString().ToLower() != "null")
            GENERIC_SPRITE_PREFAB = go;
        EMPTY_SPRITE = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
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