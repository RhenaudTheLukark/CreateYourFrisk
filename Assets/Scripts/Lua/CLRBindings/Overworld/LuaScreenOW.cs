using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public class LuaScreenOW {
    public ScriptWrapper appliedScript;

    [MoonSharpHidden] public LuaScreenOW() { }

    public delegate void LoadedAction(string coroName, object args, string evName);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

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
    [CYFEventFunction] public void DispImg(string path, int id, float posX, float posY, int toneR = 255, int toneG = 255, int toneB = 255, int toneA = 255) {
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
            EventManager.instance.sprCtrls.Add("Image" + id, new LuaSpriteController(image.GetComponent<Image>()));
        }

        image.GetComponent<Image>().sprite = SpriteRegistry.Get(path);
        if (image.GetComponent<Image>().sprite == null)
            throw new CYFException("Screen.DispImg: The sprite given doesn't exist.");
        if (toneR < 0 || toneR > 255 || toneR % 1 != 0 || toneG < 0 || toneG > 255 || toneG % 1 != 0 || toneB < 0 || toneB > 255 || toneB % 1 != 0)
            throw new CYFException("Screen.DispImg: You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
        if (toneA >= 0 && toneA <= 255 && toneR % 1 == 0)
            image.GetComponent<Image>().color = new Color32((byte)toneR, (byte)toneG, (byte)toneB, (byte)toneA);
        image.GetComponent<RectTransform>().sizeDelta = image.GetComponent<Image>().sprite.bounds.size * 100;
        image.GetComponent<RectTransform>().position = (Vector2)Camera.main.transform.position + new Vector2(posX - 320, posY - 240);

        if (newImage)
            EventManager.instance.events.Add(image);

        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Remove an image from the screen
    /// </summary>
    /// <param name="id"></param>
    [CYFEventFunction] public void SupprImg(int id) {
        if (GameObject.Find("Image" + id))
            EventManager.instance.luaevow.Remove("Image" + id);
        else
            UnitaleUtil.Warn("The image #" + id + " doesn't exist.", false);
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Sets a tone directly, without transition
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    [CYFEventFunction] public void SetTone(bool anim, bool waitEnd, int r = 255, int g = 255, int b = 255, int a = 128) {
        if (r < 0 || r > 255 || r % 1 != 0 || g < 0 || g > 255 || g % 1 != 0 || b < 0 || b > 255 || b % 1 != 0)
            throw new CYFException("Screen.SetTone: You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
        if (GameObject.Find("Tone") == null) {
            GameObject image = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ImageEvent"));
            image.GetComponent<Image>().color = new Color(r / 255f, g / 255f, b / 255f, 0);
            image.name = "Tone";
            image.tag = "Event";
            image.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas OW").transform);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 800);
            image.GetComponent<RectTransform>().position = (Vector2)Camera.main.transform.position;
            EventManager.instance.events.Add(image);
        }
        if (!anim) {
            GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            if (a == 0)
                EventManager.instance.luaevow.Remove("Tone");
            appliedScript.Call("CYFEventNextCommand");
        } else
            StCoroutine("ISetTone", new object[] { waitEnd, r, g, b, a }, appliedScript.GetVar("_internalScriptName").String);
    }

    /*/// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="intensity"></param>
    [CYFEventFunction]
    public void Rumble(float frames, float intensity = 3, bool fade = false) { StCoroutine("IRumble", new object[] { frames, intensity, fade }, appliedScript.GetVar("_internalScriptName").String); }*/

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="secondsOrFrames"></param>
    /// <param name="intensity"></param>
    [CYFEventFunction] public void Flash(int frames, int colorR = 255, int colorG = 255, int colorB = 255, int colorA = 255, bool waitEnd = true) {
        StCoroutine("IFlash", new object[] { frames, colorR, colorG, colorB, colorA, waitEnd }, appliedScript.GetVar("_internalScriptName").String);
    }

    [CYFEventFunction] public void CenterEventOnCamera(string name, int speed = 5, bool straightLine = false, bool waitEnd = true, string info = "Screen.CenterEventOnCamera") {
        if (!GameObject.Find(name))
            throw new CYFException("Screen.CenterEventOnCamera: The given event doesn't exist.");

        if (!EventManager.instance.events.Contains(GameObject.Find(name)))
            throw new CYFException("Screen.CenterEventOnCamera: The given event doesn't exist.");

        StCoroutine("IMoveCamera", new object[] { (int)(GameObject.Find(name).transform.position.x - PlayerOverworld.instance.transform.position.x),
                                                  (int)(GameObject.Find(name).transform.position.y - PlayerOverworld.instance.transform.position.y),
                                                  speed, straightLine, waitEnd, info }, appliedScript.GetVar("_internalScriptName").String);
    }

    [CYFEventFunction] public void MoveCamera(int pixX, int pixY, int speed = 5, bool straightLine = false, bool waitEnd = true) {
        StCoroutine("IMoveCamera", new object[] { pixX, pixY, speed, straightLine, waitEnd, "Screen.MoveCamera" }, appliedScript.GetVar("_internalScriptName").String);
    }

    [CYFEventFunction] public void ResetCameraPosition(int speed = 5, bool straightLine = false, bool waitEnd = true) {
        StCoroutine("IMoveCamera", new object[] { 0, 0, speed, straightLine, waitEnd, "Screen.ResetCameraPosition" }, appliedScript.GetVar("_internalScriptName").String);
    }
}
