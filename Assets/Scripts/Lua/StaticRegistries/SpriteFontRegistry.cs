using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public static class SpriteFontRegistry {
    public const string UI_DEFAULT_NAME = "uidialog";
    public const string UI_DAMAGETEXT_NAME = "uidamagetext";
    public const string UI_MONSTERTEXT_NAME = "monster";
    public const string UI_SMALLTEXT_NAME = "uibattlesmall";

    public static GameObject LETTER_OBJECT = Resources.Load<GameObject>("Prefabs/letter");
    public static GameObject BUBBLE_OBJECT = Resources.Load<GameObject>("Prefabs/DialogBubble");
    private static readonly Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static readonly Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    private static readonly Dictionary<string, UnderFont> dict = new Dictionary<string, UnderFont>();

    public static void Start() { LoadAllFrom(FileLoader.PathToDefaultFile("Sprites/UI/Fonts")); }

    public static UnderFont Get(string key) {
        string k = key;
        key = key.TrimStart('/', '\\') + (key.ToLower().EndsWith(".png") ? "" : ".png");
        FileLoader.SanitizePath(ref key, "Sprites/UI/Fonts/", false);
        key = key.ToLower();
        return dict.ContainsKey(key) ? dict[key] : TryLoad(k, key);
    }

    public static UnderFont TryLoad(string origKey, string key) {
        if (!dictMod.ContainsKey(key) && !dictDefault.ContainsKey(key))
            return TryFetchFromMod(origKey, key) ?? TryFetchFromDefault(origKey, key);
        dict[key] = GetUnderFont(origKey);
        return dict[key];
    }

    private static UnderFont TryFetchFromDefault(string origKey, string key) {
        FileInfo tryF = new FileInfo(Path.Combine(FileLoader.PathToDefaultFile("Sprites/UI/Fonts/"), origKey) + (origKey.ToLower().EndsWith(".png") ? "" : ".png"));
        if (!tryF.Exists)
            return null;

        dictDefault[key] = tryF;
        dict[key] = GetUnderFont(origKey);
        return dict[key];
    }

    private static UnderFont TryFetchFromMod(string origKey, string key) {
        FileInfo tryF = new FileInfo(Path.Combine(FileLoader.PathToModFile("Sprites/UI/Fonts/"), origKey.TrimStart('/')) + (origKey.ToLower().EndsWith(".png") ? "" : ".png"));
        if (!tryF.Exists)
            return null;

        dictMod[key] = tryF;
        dict[key] = GetUnderFont(origKey);
        return dict[key];
    }

    public static void Init() { LoadAllFrom(FileLoader.PathToModFile("Sprites/UI/Fonts"), true); }

    private static void LoadAllFrom(string directoryPath, bool mod = false) {
        dict.Clear();
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);

        if (!dInfo.Exists)
            return;

        FileInfo[] fInfo = dInfo.GetFiles("*.png", SearchOption.TopDirectoryOnly);

        if (mod) {
            dictMod.Clear();
            foreach (FileInfo file in fInfo) {
                string k = file.FullName;
                FileLoader.SanitizePath(ref k, "Sprites/UI/Fonts/");
                dictMod[k.ToLower()] = file;
            }
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfo) {
                string k = file.FullName;
                FileLoader.SanitizePath(ref k, "Sprites/UI/Fonts/");
                dictDefault[k.ToLower()] = file;
            }
        }
    }

    private static UnderFont GetUnderFont(string fontName) {
        XmlDocument xml = new XmlDocument();
        string xmlPath = fontName + ".xml";
        if (!FileLoader.SanitizePath(ref xmlPath, "Sprites/UI/Fonts/", false, true))
            return null;
        try { xml.Load(xmlPath); }
        catch (XmlException ex) {
            UnitaleUtil.DisplayLuaError("Instanciating a font", "An error was encountered while loading the font \"" + fontName + "\":\n\n" + ex.Message);
            return null;
        }
        if (xml["font"] == null) {
            UnitaleUtil.DisplayLuaError("Instanciating a font", "The font '" + fontName + "' doesn't have a font element at its root.");
            return null;
        }
        Dictionary<char, Sprite> fontMap = LoadBuiltInFont(xml["font"]["spritesheet"], fontName + ".png");

        UnderFont underfont;
        try { underfont = new UnderFont(fontMap, fontName); }
        catch {
            UnitaleUtil.DisplayLuaError("Instanciating a font", "The fonts need a space character to compute their line height, and the font '" + fontName + "' doesn't have one.");
            return null;
        }

        if (xml["font"]["voice"] != null) {
            underfont.Sound = AudioClipRegistry.GetVoice(xml["font"]["voice"].InnerText);
            underfont.SoundName = xml["font"]["voice"].InnerText;
        }
        if (xml["font"]["linespacing"] != null)  underfont.LineSpacing = ParseUtil.GetFloat(xml["font"]["linespacing"].InnerText);
        if (xml["font"]["charspacing"] != null)  underfont.CharSpacing = ParseUtil.GetFloat(xml["font"]["charspacing"].InnerText);
        if (xml["font"]["color"] != null)        underfont.DefaultColor = ParseUtil.GetColor(xml["font"]["color"].InnerText);

        return underfont;
    }

    private static void AddLetter(Dictionary<char, Sprite> letters, char letter, Sprite s, string fontPath) {
        if (letters.ContainsKey(letter)) {
            UnitaleUtil.DisplayLuaError("", "Error while loading the font " + fontPath + "\n\nThe letter \" " + letter + " \" has been added several times to the font!", true);
            return;
        }
        letters.Add(letter, s);
    }

    private static Dictionary<char, Sprite> LoadBuiltInFont(XmlNode sheetNode, string fontPath) {
        Sprite[] letterSprites = SpriteUtil.AtlasFromXml(sheetNode, SpriteUtil.FromFile(fontPath, "Sprites/UI/Fonts/"));
        Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
        foreach (Sprite s in letterSprites) {
            string name = s.name;
            if (name.Length == 1)
                AddLetter(letters, name[0], s, fontPath);
            else
                switch (name) {
                    case "slash":         AddLetter(letters, '/', s, fontPath);   break;
                    case "dot":           AddLetter(letters, '.', s, fontPath);   break;
                    case "pipe":          AddLetter(letters, '|', s, fontPath);   break;
                    case "backslash":     AddLetter(letters, '\\', s, fontPath);  break;
                    case "colon":         AddLetter(letters, ':', s, fontPath);   break;
                    case "questionmark":  AddLetter(letters, '?', s, fontPath);   break;
                    case "doublequote":   AddLetter(letters, '"', s, fontPath);   break;
                    case "asterisk":      AddLetter(letters, '*', s, fontPath);   break;
                    case "space":         AddLetter(letters, ' ', s, fontPath);   break;
                    case "lt":            AddLetter(letters, '<', s, fontPath);   break;
                    case "rt":            AddLetter(letters, '>', s, fontPath);   break;
                    case "ampersand":     AddLetter(letters, '&', s, fontPath);   break;
                }
        }
        return letters;
    }
}