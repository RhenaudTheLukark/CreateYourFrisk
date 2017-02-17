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
                FileInfo fi = new FileInfo(Path.ChangeExtension(FileLoader.pathToDefaultFile("Sprites/" + filename + ".png"), "xml"));
                if (!fi.Exists)
                    fi = new FileInfo(Path.ChangeExtension(FileLoader.pathToModFile("Sprites/" + filename + ".png"), "xml"));
                if (fi.Exists) {
                    XmlDocument xmld = new XmlDocument();
                    xmld.Load(fi.FullName);
                    if (xmld["spritesheet"] != null && "single".Equals(xmld["spritesheet"].GetAttribute("type")))
                        if (!GameObject.FindObjectOfType<TextManager>().overworld)
                            UIController.instance.encounter.enabledEnemies[bubbleID].bubbleWideness
                                = ParseUtil.getFloat(xmld["spritesheet"].GetElementsByTagName("wideness")[0].InnerText);
                } else
                    UIController.instance.encounter.enabledEnemies[bubbleID].bubbleWideness = 0;
            }
        } catch (Exception) {
            UIController.instance.encounter.enabledEnemies[bubbleID].bubbleWideness = 0;
        }

        Sprite newSprite = SpriteRegistry.Get(filename);
        if (newSprite == null) {
            if (filename.Length == 0) {
                Debug.LogError("SwapSprite: Filename is empty!");
                return;
            }
            newSprite = fromFile(FileLoader.pathToModFile("Sprites/" + filename + ".png"));
            SpriteRegistry.Set(filename, newSprite);
        }

        Image img = target.GetComponent<Image>();
        if (!img) {
            SpriteRenderer img2 = target.GetComponent<SpriteRenderer>();
            img2.sprite = newSprite;
            img2.GetComponent<RectTransform>().sizeDelta = new Vector2(newSprite.texture.width, newSprite.texture.height);
        } else {
            img.sprite = newSprite;
            //enemyImg.SetNativeSize();
            img.rectTransform.sizeDelta = new Vector2(newSprite.texture.width, newSprite.texture.height);
        }
    }

    public static Sprite spriteWithXml(XmlNode spriteNode, Sprite source) {
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

    public static Sprite[] atlasFromXml(XmlNode sheetNode, Sprite source) {
        try {
            List<Sprite> tempSprites = new List<Sprite>();
            foreach (XmlNode child in sheetNode.ChildNodes)
                if (child.Name.Equals("sprite")) {
                    //Sprite s = Sprite.Create(source.texture, 
                    Sprite s = spriteWithXml(child, source);
                    tempSprites.Add(s);
                }
            return tempSprites.ToArray();
        } catch (Exception ex) {
            UnitaleUtil.displayLuaError("[XML document]", "One of the sprites' XML documents was invalid. This could be a corrupt or edited file.\n\n" + ex.Message);
            return null;
        }
    }

    public static Sprite fromFile(string filename) {
        Sprite newSprite = new Sprite();
        Texture2D SpriteTexture = new Texture2D(1, 1);
        SpriteTexture.LoadImage(FileLoader.getBytesFrom(filename));
        SpriteTexture.filterMode = FilterMode.Point;
        SpriteTexture.wrapMode = TextureWrapMode.Clamp;
        newSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);
        //optional XML loading
        FileInfo fi = new FileInfo(Path.ChangeExtension(filename, "xml"));
        if (fi.Exists) {
            XmlDocument xmld = new XmlDocument();
            xmld.Load(fi.FullName);
            if (xmld["spritesheet"] != null && "single".Equals(xmld["spritesheet"].GetAttribute("type")))
                return spriteWithXml(xmld["spritesheet"].GetElementsByTagName("sprite")[0], newSprite);
        }
        return newSprite;
    }
    public static DynValue MakeIngameSprite(string filename, int childNumber = -1) { return MakeIngameSprite(filename, "BelowArena", childNumber); }

    public static DynValue MakeIngameSprite(string filename, string tag = "BelowArena", int childNumber = -1) {
        if (ParseUtil.testInt(tag)) {
            childNumber = ParseUtil.getInt(tag);
            tag = "BelowArena";
        }
        Image i = GameObject.Instantiate<Image>(SpriteRegistry.GENERIC_SPRITE_PREFAB);
        if (!string.IsNullOrEmpty(filename))
            SwapSpriteFromFile(i, filename);
        if (!GameObject.Find(tag + "Layer"))
            UnitaleUtil.displayLuaError("Creating a sprite", "The sprite layer " + tag + " doesn't exists.");
        else {
            if (childNumber == -1)
                i.transform.SetParent(GameObject.Find(tag + "Layer").transform, true);
            else {
                RectTransform[] rts = GameObject.Find(tag + "Layer").GetComponentsInChildren<RectTransform>();
                for (int j = 0; j < rts.Length; j++)
                    if (j >= childNumber)
                        rts[j].SetParent(null, true);
                i.transform.SetParent(GameObject.Find(tag + "Layer").transform, true);
                for (int j = 0; j < rts.Length; j++)
                    if (j >= childNumber)
                        rts[j].SetParent(GameObject.Find(tag + "Layer").transform, true);
            }
        }
        return UserData.Create(new LuaSpriteController(i, filename), LuaSpriteController.data);
    }

    public static void CreateLayer(string name, string relatedTag = "BasisNewest", bool before = false) {
        GameObject go = new GameObject(name + "Layer", typeof(RectTransform));
        int index = -1;  bool wentIn = false; string testName = relatedTag + "Layer";
        Transform[] rts = UnitaleUtil.GetFirstChildren(GameObject.Find("Canvas").transform);
        if (relatedTag == "BasisNewest") testName = "BelowArenaLayer";
        if (relatedTag != "VeryHighest" && relatedTag != "VeryLowest")
            for (int j = 0; j < rts.Length; j++) {
                if (rts[j].name == testName || wentIn) {
                    wentIn = true;
                    if (relatedTag == "BasisNewest" && rts[j].name.Contains("Layer") && !rts[j].name.Contains("Audio"))
                        continue;
                    wentIn = false;
                    index = j;
                    if (before)
                        rts[j].SetParent(null, true);
                } else if (index != -1)
                    rts[j].SetParent(null, true);
            }

        go.transform.SetParent(GameObject.Find("Canvas").transform, true);

        if (index != -1) {
            if (before) index--;
            for (int j = index; j < rts.Length; j++) {
                if (rts[j].gameObject.name == "Text") {
                    rts[j].SetParent(GameObject.Find("Debugger").transform);
                    continue;
                }
                rts[j].SetParent(GameObject.Find("Canvas").transform, true);
            }
        }
        if (relatedTag == "VeryHighest")     go.transform.SetAsLastSibling();
        else if (relatedTag == "VeryLowest") go.transform.SetAsFirstSibling();
        go.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        go.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        go.transform.position = new Vector2(0, 0);
    }

    public static void CreateProjectileLayer(string name, string relatedTag = "", bool before = false) {
        GameObject go = new GameObject(name + "Bullet", typeof(RectTransform));
        int index = -1;
        RectTransform[] rts = GameObject.Find("Canvas").GetComponentsInChildren<RectTransform>();
        for (int j = 0; j < rts.Length; j++) {
            string testName;
            if (relatedTag == "") testName = "BulletPool";
            else                  testName = relatedTag + "Bullet";

            if (rts[j].name == testName) {
                index = j;
                if (before)
                    rts[j].SetParent(null, true);
            } else if (index != -1)
                rts[j].SetParent(null, true);
        }
        go.transform.SetParent(GameObject.Find("Canvas").transform, true);
        if (index != -1) {
            if (before) index--;
            for (int j = index; j < rts.Length; j++)
                rts[j].SetParent(GameObject.Find("Canvas").transform, true);
        }
        go.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        go.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        go.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        go.transform.position = new Vector3(0, 0, -0.005f);
    }
}