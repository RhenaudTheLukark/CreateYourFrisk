using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public static class SpriteFontRegistry {
    public const string UI_DEFAULT_NAME = "uidialog";
    public const string UI_DAMAGETEXT_NAME = "uidamagetext";
    public const string UI_MONSTERTEXT_NAME = "monster";
    public const string UI_SMALLTEXT_NAME = "uibattlesmall";

    public static GameObject LETTER_OBJECT;
    public static GameObject BUBBLE_OBJECT;
    private static Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    private static Dictionary<string, UnderFont> dict = new Dictionary<string, UnderFont>();
    //private static bool initialized;

    public static void Start() {
        LoadAllFrom(FileLoader.pathToDefaultFile("Sprites/UI/Fonts"));
    }

    public static UnderFont Get(string key) {
        string k = key;
        key = key.ToLower();
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return TryLoad(k);
        //return null;
    }

    public static void init() {
        dict.Clear();
        /*if (initialized)
            return;*/
        LETTER_OBJECT = Resources.Load<GameObject>("Prefabs/letter");
        BUBBLE_OBJECT = Resources.Load<GameObject>("Prefabs/DialogBubble");

        //string modPath = FileLoader.pathToModFile("Sprites/UI/Fonts");
        //string defaultPath = FileLoader.pathToDefaultFile("Sprites/UI/Fonts");
        //loadAllFrom(defaultPath);
        LoadAllFrom(FileLoader.pathToModFile("Sprites/UI/Fonts"), true);

        //initialized = true;
    }

    private static void LoadAllFrom(string directoryPath, bool mod = false) {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);

        if (!dInfo.Exists) {
            return;
        }

        FileInfo[] fInfo = dInfo.GetFiles("*.png", SearchOption.TopDirectoryOnly);

        if (mod) {
            dictMod.Clear();
            foreach (FileInfo file in fInfo)
                dictMod[Path.GetFileNameWithoutExtension(file.FullName).ToLower()] = file;
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfo)
                dictDefault[Path.GetFileNameWithoutExtension(file.FullName).ToLower()] = file;
        }
        /*foreach (FileInfo file in fInfo) {
            string fontName = Path.GetFileNameWithoutExtension(file.FullName);
            UnderFont underfont = getUnderFont(fontName);
            if (underfont == null)
                continue;
            dict[fontName.ToLower()] = underfont;
        }*/
    }

    public static UnderFont TryLoad(string key) {
        string k = key;
        key = key.ToLower();
        if (dictMod.ContainsKey(key) || dictDefault.ContainsKey(key)) {
            UnderFont underfont = GetUnderFont(k);
            //if (underfont != null)
            dict[key] = underfont;
        } else
            return null;
        return dict[key];
    }

    private static UnderFont GetUnderFont(string fontName) {
        XmlDocument xml = new XmlDocument();
        string fontPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".png");
        string xmlPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".xml", false);
        if (xmlPath == null)
            return null;
        try { xml.Load(xmlPath); }
        catch (XmlException ex) {
            UnitaleUtil.DisplayLuaError("Instanciating a font", "An error was encountered while loading the font \"" + fontName + "\":\n\n" + ex.Message);
            return null;
        }
        Dictionary<char, Sprite> fontMap = LoadBuiltInFont(xml["font"]["spritesheet"], fontPath);

        UnderFont underfont = null;
        try { underfont = new UnderFont(fontMap, fontName); }
        catch {
            UnitaleUtil.DisplayLuaError("Instanciating a font", "The fonts need a space character to compute their line height, and the font '" + fontName + "' doesn't have one.");
            return null;
        }

        if (xml["font"]["voice"] != null)        underfont.Sound = AudioClipRegistry.GetVoice(xml["font"]["voice"].InnerText);
        if (xml["font"]["linespacing"] != null)  underfont.LineSpacing = ParseUtil.GetFloat(xml["font"]["linespacing"].InnerText);
        if (xml["font"]["charspacing"] != null)  underfont.CharSpacing = ParseUtil.GetFloat(xml["font"]["charspacing"].InnerText);
        if (xml["font"]["color"] != null)        underfont.DefaultColor = ParseUtil.GetColor(xml["font"]["color"].InnerText);

        return underfont;
    }

    private static Dictionary<char, Sprite> LoadBuiltInFont(XmlNode sheetNode, string fontPath) {
        Sprite[] letterSprites = SpriteUtil.AtlasFromXml(sheetNode, SpriteUtil.FromFile(fontPath));
        Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
        foreach (Sprite s in letterSprites) {
            string name = s.name;
            if (name.Length == 1) {
                letters.Add(name[0], s);
                continue;
            } else
                switch (name) {
                    case "slash":         letters.Add('/', s);   break;
                    case "dot":           letters.Add('.', s);   break;
                    case "pipe":          letters.Add('|', s);   break;
                    case "backslash":     letters.Add('\\', s);  break;
                    case "colon":         letters.Add(':', s);   break;
                    case "questionmark":  letters.Add('?', s);   break;
                    case "doublequote":   letters.Add('"', s);   break;
                    case "asterisk":      letters.Add('*', s);   break;
                    case "space":         letters.Add(' ', s);   break;
                    case "lt":            letters.Add('<', s);   break;
                    case "rt":            letters.Add('>', s);   break;
                    case "ampersand":     letters.Add('&', s);   break;
                }
        }
        return letters;
    }
}