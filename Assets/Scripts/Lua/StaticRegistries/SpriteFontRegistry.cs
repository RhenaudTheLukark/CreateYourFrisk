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
        key += key.EndsWith(".png") ? "" : ".png";
        FileLoader.SanitizePath(ref key, "Sprites/UI/Fonts/", false);
        key = key.ToLower();
        return dict.ContainsKey(key) ? dict[key] : TryLoad(k);
    }

    public static UnderFont TryLoad(string key) {
        string k = key;
        key += key.EndsWith(".png") ? "" : ".png";
        FileLoader.SanitizePath(ref key, "Sprites/UI/Fonts/", false);
        key = key.ToLower();
        if (dictMod.ContainsKey(key) || dictDefault.ContainsKey(key)) dict[key] = GetUnderFont(k);
        else return null;
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
                string k = file.FullName.Substring(directoryPath.Length + 1);
                FileLoader.SanitizePath(ref k, "Sprites/UI/Fonts/");
                dictMod[k.ToLower()] = file;
            }
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfo) {
                string k = file.FullName.Substring(directoryPath.Length + 1);
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

    private static Dictionary<char, Sprite> LoadBuiltInFont(XmlNode sheetNode, string fontPath) {
        Sprite[] letterSprites = SpriteUtil.AtlasFromXml(sheetNode, SpriteUtil.FromFile(fontPath, "Sprites/UI/Fonts/"));
        Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
        foreach (Sprite s in letterSprites) {
            string name = s.name;
            if (name.Length == 1)
                letters.Add(name[0], s);
            else
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