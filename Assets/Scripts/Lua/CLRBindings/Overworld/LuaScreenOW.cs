using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public class LuaScreenOW {

    [MoonSharpHidden]
    public LuaScreenOW() { }

    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden]
    public static event LoadedAction StCoroutine;
    
    //I know, there's WAY too much parameters in here, but I don't have the choice right now.
    //If I find a way to get the Table's text from DynValues, I'll gladly reduce the number of
    //parameters of this, but right now, even if it is very painful to enter directly 6 or 10 parameters,
    //I don't find a proper way to do this. (Erm...plus, I have to say that if I put arrays into this,
    //you'll have to write braces in the function, so just think that I give you a favor xP)
    /// <summary>
    /// Permits to display an image on the screen at given dimensions, position and color
    /// </summary>
    /// <param name="path"></param>
    /// <param name="id"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="dimX"></param>
    /// <param name="dimY"></param>
    /// <param name="toneR"></param>
    /// <param name="toneG"></param>
    /// <param name="toneB"></param>
    /// <param name="toneA"></param>
    [CYFEventFunction]
    public void DispImg(string path, int id, float posX, float posY, int toneR = 255, int toneG = 255, int toneB = 255, int toneA = -1) {
        GameObject image;
        bool newImage = false;

        if (GameObject.Find("Image" + id) != null)
            image = GameObject.Find("Image" + id);
        else {
            newImage = true;
            image = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ImageEvent"));
            image.name = "Image" + id;
            image.tag = "Event";
            image.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas OW").transform);
        }

        image.GetComponent<Image>().sprite = SpriteRegistry.Get(path);
        if (toneA >= 0 && toneA <= 255 && toneR % 1 == 0)
            if (toneR < 0 || toneR > 255 || toneR % 1 != 0 || toneG < 0 || toneG > 255 || toneG % 1 != 0 || toneB < 0 || toneB > 255 || toneB % 1 != 0)
                UnitaleUtil.displayLuaError(EventManager.instance.script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
            else
                image.GetComponent<Image>().color = new Color32((byte)toneR, (byte)toneG, (byte)toneB, (byte)toneA);
        image.GetComponent<RectTransform>().sizeDelta = image.GetComponent<Image>().sprite.bounds.size * 100;
        image.GetComponent<RectTransform>().position = (Vector2)Camera.main.transform.position + new Vector2(posX - 320, posY - 240);

        if (newImage)
            EventManager.instance.events.Add(image);

        EventManager.instance.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Remove an image from the screen
    /// </summary>
    /// <param name="id"></param>
    [CYFEventFunction]
    public void SupprImg(int id) {
        if (GameObject.Find("Image" + id))
            EventManager.instance.luaevow.Remove("Image" + id);
        else
            UnitaleUtil.writeInLog("The given image doesn't exists.");
        EventManager.instance.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Sets a tone directly, without transition
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    [CYFEventFunction]
    public void SetTone(bool anim, bool waitEnd, int r = 255, int g = 255, int b = 255, int a = 0) {
        if (r < 0 || r > 255 || r % 1 != 0 || g < 0 || g > 255 || g % 1 != 0 || b < 0 || b > 255 || b % 1 != 0) {
            UnitaleUtil.displayLuaError(EventManager.instance.script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
            EventManager.instance.script.Call("CYFEventNextCommand");
        } else {
            if (!anim) {
                if (GameObject.Find("Tone") == null) {
                    GameObject tone = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ImageEvent"));
                    tone.name = "Tone";
                    tone.tag = "Event";
                    tone.GetComponent<RectTransform>().parent = Camera.main.transform;
                    tone.GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    tone.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
                    tone.GetComponent<RectTransform>().localPosition = new Vector2();
                    EventManager.instance.events.Add(tone);
                } else
                    GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                EventManager.instance.script.Call("CYFEventNextCommand");
            } else 
                StCoroutine("ISetTone", new object[] { waitEnd, r, g, b, a });
        }
    }

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="intensity"></param>
    [CYFEventFunction]
    public void Rumble(float seconds, float intensity = 3, bool fade = false) { StCoroutine("IRumble", new object[] { seconds, intensity, fade }); }

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="secondsOrFrames"></param>
    /// <param name="intensity"></param>
    [CYFEventFunction]
    public void Flash(float secondsOrFrames, bool isSeconds = false, int colorR = 255, int colorG = 255, int colorB = 255, int colorA = 255) {
        StCoroutine("IFlash", new object[] { secondsOrFrames, isSeconds, colorR, colorG, colorB, colorA });
    }

    [CYFEventFunction]
    public void CenterEventOnCamera(string name, int speed = 5, bool straightLine = false) {
        if (!GameObject.Find(name)) {
            UnitaleUtil.displayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        if (!EventManager.instance.events.Contains(GameObject.Find(name))) {
            UnitaleUtil.displayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        StCoroutine("IMoveCamera", new object[] { (int)(GameObject.Find(name).transform.position.x - PlayerOverworld.instance.transform.position.x),
                                                  (int)(GameObject.Find(name).transform.position.y - PlayerOverworld.instance.transform.position.y),
                                                  speed, straightLine });
    }

    [CYFEventFunction]
    public void MoveCamera(int pixX, int pixY, int speed = 5, bool straightLine = false) {
        StCoroutine("IMoveCamera", new object[] { pixX, pixY, speed, straightLine });
    }

    [CYFEventFunction]
    public void ResetCameraPosition(int speed = 5, bool straightLine = false) { StCoroutine("IMoveCamera", new object[] { 0, 0, speed, straightLine }); }
}
