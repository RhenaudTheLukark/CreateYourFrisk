using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public static class SpriteUtil {
    public const float PIXELS_PER_UNIT = 100.0f;

    public static void SwapSpriteFromFile(Component target, string filename, int bubbleID = -1) {
        try {
            if (bubbleID != -1) {
                FileInfo fi = new FileInfo(Path.ChangeExtension(FileLoader.pathToModFile("Sprites/" + filename + ".png"), "xml"));
                if (!fi.Exists)
                    fi = new FileInfo(Path.ChangeExtension(FileLoader.pathToDefaultFile("Sprites/" + filename + ".png"), "xml"));
                if (fi.Exists) {
                    XmlDocument xmld = new XmlDocument();
                    xmld.Load(fi.FullName);
                    if (xmld["spritesheet"] != null && "single".Equals(xmld["spritesheet"].GetAttribute("type")))
                        if (!UnitaleUtil.IsOverworld)
                            UIController.instance.encounter.EnabledEnemies[bubbleID].bubbleWidth = ParseUtil.GetFloat(xmld["spritesheet"].GetElementsByTagName("width").Count > 0
                                ? xmld["spritesheet"].GetElementsByTagName("width")[0].InnerText
                                : xmld["spritesheet"].GetElementsByTagName("wideness")[0].InnerText);
                } else
                    UIController.instance.encounter.EnabledEnemies[bubbleID].bubbleWidth = 0;
            }
        } catch (Exception) {
            UIController.instance.encounter.EnabledEnemies[bubbleID].bubbleWidth = 0;
        }
        Sprite newSprite = SpriteRegistry.Get(filename);
        if (newSprite == null) {
            if (filename.Length == 0) {
                Debug.LogError("SwapSprite: Filename is empty!");
                return;
            }
            newSprite = FromFile(FileLoader.pathToModFile("Sprites/" + filename + ".png"));
            if (newSprite == null)
                throw new CYFException("The sprite Sprites/" + filename + ".png doesn't exist.");
            SpriteRegistry.Set(filename, newSprite);
        }

        Image img = target.GetComponent<Image>();
        if (!img) {
            SpriteRenderer img2 = target.GetComponent<SpriteRenderer>();
            Vector2 pivot = img2.GetComponent<RectTransform>().pivot;
            img2.sprite = newSprite;
            img2.GetComponent<RectTransform>().sizeDelta = new Vector2(newSprite.texture.width, newSprite.texture.height);
            img2.GetComponent<RectTransform>().pivot = pivot;
        } else {
            Vector2 pivot = img.rectTransform.pivot;
            img.sprite = newSprite;
            //enemyImg.SetNativeSize();
            img.rectTransform.sizeDelta = new Vector2(newSprite.texture.width, newSprite.texture.height);
            img.rectTransform.pivot = pivot;
        }

    }

    public static Sprite SpriteWithXml(XmlNode spriteNode, Sprite source) {
        XmlNode xmlRect = spriteNode.SelectSingleNode("rect");
        Rect spriteRect = new Rect(0, 0, source.texture.width, source.texture.height);
        if (xmlRect != null)
            spriteRect = new Rect(int.Parse(xmlRect.Attributes["x"].Value), int.Parse(xmlRect.Attributes["y"].Value),
                                  int.Parse(xmlRect.Attributes["w"].Value), int.Parse(xmlRect.Attributes["h"].Value));
        XmlNode xmlBorder = spriteNode.SelectSingleNode("border");
        Vector4 spriteBorder = Vector4.zero;
        if (xmlBorder != null)
            spriteBorder = new Vector4(int.Parse(xmlBorder.Attributes["x"].Value), int.Parse(xmlBorder.Attributes["y"].Value),
                                       int.Parse(xmlBorder.Attributes["z"].Value), int.Parse(xmlBorder.Attributes["w"].Value));

        Sprite s = Sprite.Create(source.texture, spriteRect, new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT, 0, SpriteMeshType.FullRect, spriteBorder);
        if (spriteNode.Attributes["name"] != null)
            s.name = spriteNode.Attributes["name"].Value;
        return s;
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

    public static Sprite FromFile(string filename) {
        Texture2D SpriteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        SpriteTexture.LoadImage(FileLoader.getBytesFrom(filename));
        SpriteTexture.filterMode = FilterMode.Point;
        SpriteTexture.wrapMode = TextureWrapMode.Clamp;

        Sprite newSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, UnitaleUtil.IsOverworld ? 0 : 0.5f), PIXELS_PER_UNIT);
        filename = filename.Contains("File at ") ? filename.Substring(8) : filename;
        newSprite.name = FileLoader.getRelativePathWithoutExtension(filename);
        //optional XML loading
        FileInfo fi = new FileInfo(Path.ChangeExtension(filename, "xml"));
        if (fi.Exists) {
            XmlDocument xmld = new XmlDocument();
            xmld.Load(fi.FullName);
            if (xmld["spritesheet"] != null && "single".Equals(xmld["spritesheet"].GetAttribute("type")))
                return SpriteWithXml(xmld["spritesheet"].GetElementsByTagName("sprite")[0], newSprite);
        }
        return newSprite;
    }
    public static DynValue MakeIngameSprite(string filename, int childNumber = -1) { return MakeIngameSprite(filename, "BelowArena", childNumber); }

    public static DynValue MakeIngameSprite(string filename, string tag = "BelowArena", int childNumber = -1) {
        string canvas = UnitaleUtil.IsOverworld ? "Canvas Two/" : "Canvas/";
        tag = (UnitaleUtil.IsOverworld && tag == "BelowArena") ? "Default" : tag;
        if (ParseUtil.TestInt(tag) && childNumber == -1) {
            childNumber = ParseUtil.GetInt(tag);
            tag = UnitaleUtil.IsOverworld ? "Default" : "BelowArena";
        }
        Image i = GameObject.Instantiate<Image>(SpriteRegistry.GENERIC_SPRITE_PREFAB);
        if (!string.IsNullOrEmpty(filename))
            SwapSpriteFromFile(i, filename);
        else
            throw new CYFException("You can't create a sprite object with a nil sprite!");
        if (!GameObject.Find(tag + "Layer") && tag != "none")
            if ((!UnitaleUtil.IsOverworld && tag == "BelowArena") || (UnitaleUtil.IsOverworld && tag == "Default"))
                i.transform.SetParent(GameObject.Find(canvas).transform);
            else
                UnitaleUtil.DisplayLuaError("Creating a sprite", "The sprite layer " + tag + " doesn't exist.");
        else {
            if (tag == "none")
                i.transform.SetParent(GameObject.Find(canvas).transform, true);
            else
                i.transform.SetParent(GameObject.Find(tag + "Layer").transform, true);
            if (childNumber != -1)
                i.transform.SetSiblingIndex(childNumber - 1);
        }
        return UserData.Create(new LuaSpriteController(i), LuaSpriteController.data);
    }

    public static bool CreateLayer(string name, string relatedTag = "BasisNewest", bool before = false) {
        string canvas = UnitaleUtil.IsOverworld ? "Canvas Two/" : "Canvas/";
        if (name == null || GameObject.Find(canvas + name + "Layer") != null)
            return false;
        else if (relatedTag != "VeryHighest" && relatedTag != "VeryLowest" && relatedTag != "BasisNewest" && GameObject.Find(canvas + relatedTag + "Layer") == null)
            return false; // throw new CYFException("CreateLayer: Tried to make a new layer " + (before ? "below" : "above") + " the layer \"" + relatedTag + "\", but it didn't exist.");

        GameObject go = new GameObject(name + "Layer", typeof(RectTransform));
        string testName = relatedTag + "Layer";
        Transform[] rts = UnitaleUtil.GetFirstChildren(GameObject.Find(canvas).transform);
        if (relatedTag != "VeryHighest" && relatedTag != "VeryLowest") {
            if (relatedTag == "BasisNewest")
                testName = "BelowArenaLayer";
            for (int j = 0; j < rts.Length; j++) {
                if (rts[j].name == testName) {
                    go.transform.SetParent(GameObject.Find(canvas).transform, true);
                    go.transform.SetSiblingIndex(j + (before ? 0 : 1));
                    break;
                }
            }
        } else {
            go.transform.SetParent(GameObject.Find(canvas).transform, true);
            if (relatedTag == "VeryHighest")
                go.transform.SetAsLastSibling();
            else if (relatedTag == "VeryLowest")
                go.transform.SetAsFirstSibling();
        }

        go.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        go.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        go.transform.position = new Vector2(0, 0);
        return true;
    }

    public static void CreateProjectileLayer(string name, string relatedTag = "", bool before = false) {
        GameObject go = new GameObject(name + "Bullet", typeof(RectTransform));
        RectTransform[] rts = GameObject.Find("Canvas").GetComponentsInChildren<RectTransform>();
        for (int j = 0; j < rts.Length; j++) {
            string testName;
            if (relatedTag == "") testName = "BulletPool";
            else                  testName = relatedTag + "Bullet";

            if (rts[j].name == testName) {
                go.transform.SetParent(GameObject.Find("Canvas").transform, true);
                go.transform.SetSiblingIndex(j + (before ? 0 : 1));
                break;
            }
        }
        go.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        go.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        go.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        go.transform.position = new Vector3(0, 0, -0.005f);
    }
}