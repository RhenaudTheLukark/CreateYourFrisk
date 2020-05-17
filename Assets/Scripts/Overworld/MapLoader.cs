using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public static class MapLoader {

    /*
    private static UnderFont GetUnderFont(string fontName) {
        XmlDocument xml = new XmlDocument();
        string fontPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".png");
        string xmlPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".xml", false);
        if (xmlPath == null)
            return null;
        xml.Load(xmlPath);
        Dictionary<char, Sprite> fontMap = LoadBuiltInFont(xml["font"]["spritesheet"], fontPath);

        UnderFont underfont = null;
        try { underfont = new UnderFont(fontMap, fontName); }
        catch {
            UnitaleUtil.DisplayLuaError("Instanciating a font", "The fonts need a space character to compute their line height, and '" + "' doesn't have one.");
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

    public static Sprite[] AtlasFromXml(XmlNode sheetNode, Sprite source) {
        try {
            List<Sprite> tempSprites = new List<Sprite>();
            foreach (XmlNode child in sheetNode.ChildNodes)
                if (child.Name.Equals("sprite")) {
                    Sprite s = SpriteWithXml(child, source);
                    tempSprites.Add(s);
                }
            return tempSprites.ToArray();
        } catch (Exception ex) {
            UnitaleUtil.DisplayLuaError("[XML document]", "One of the sprites' XML documents was invalid. This could be a corrupt or edited file.\n\n" + ex.Message);
            return null;
        }
    }
    */

    public static void LoadMap(string path) {
        XmlDocument xml = new XmlDocument();
        string xmlPath = FileLoader.requireFile("Sprites/UI/Fonts/" + path + ".xml", false);
        if (xmlPath == null)
            return;
        xml.Load(xmlPath);
    }
}
