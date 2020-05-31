using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour {
    TextManager text;
    Image img;
    public string[] imagePaths, textsToDisplay, specialEffects, goToNextDirect;
    bool finish = false, start = true, pause = false, fadeMusic = false, sameImage = false, mask = false;
    float timer = 0.0f, timerEffect = 0.0f;
    int currentIndex = 0;

    enum Effect { NONE, SCROLLUP, SCROLLDOWN, SCROLLLEFT, SCROLLRIGHT };
    Effect currentEffect = Effect.NONE;

    // Use this for initialization
    void Start () {
        if (!SaveLoad.started) {
            StaticInits.Start();
            SaveLoad.Start();
            new ControlPanel();
            new PlayerCharacter();
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (GlobalControls.crate) Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
                else Misc.WindowName = ControlPanel.instance.WindowBasisName;
            #endif
            SaveLoad.LoadAlMighty();
            LuaScriptBinder.Set(null, "ModFolder", MoonSharp.Interpreter.DynValue.NewString("@Title"));
            UnitaleUtil.AddKeysToMapCorrespondanceList();
        }
        Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_intro");
        Camera.main.GetComponent<AudioSource>().Play();
        if (imagePaths.Length != textsToDisplay.Length)
            throw new Exception("You need to have the same number of images and lines of text.");
        text = GameObject.FindObjectOfType<TextManager>();
        img = GameObject.Find("CutsceneImages").GetComponent<Image>();
        text.SetVerticalSpacing(6);
        text.SetHorizontalSpacing(6);
        if (SpriteRegistry.Get("Intro/mask") != null) {
            mask = true;
            GameObject.Find("Mask").GetComponent<Image>().sprite = SpriteRegistry.Get("Intro/mask");
            GameObject.Find("Mask").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }

        TextMessage[] mess = new TextMessage[textsToDisplay.Length];
        for (int i = 0; i < mess.Length; i ++)
            mess[i] = new TextMessage("[waitall:2]" + textsToDisplay[i], false, false);
        text.SetTextQueue(mess);
        img.sprite = SpriteRegistry.Get("Intro/" + imagePaths[0]);
        img.SetNativeSize();
        if (specialEffects[0] != string.Empty)
            try { ApplyEffect((Effect)Enum.Parse(typeof(Effect), specialEffects[currentIndex].ToUpper())); }
            catch { UnitaleUtil.DisplayLuaError("IntroManager", "The effect " + specialEffects[currentIndex] + " doesn't exist."); }
        if (goToNextDirect[0] == "Y")
            timer = 0.5f;
    }

    // Update is called once per frame
    void Update () {
        timer += Time.deltaTime;
        //Effect update
        if (CheckEffect() &&!start &&!finish &&!fadeMusic)
            UpdateEffect();
        //Stop the intro
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.F4)) {
            Camera.main.GetComponent<AudioSource>().Stop();
            Camera.main.GetComponent<AudioSource>().volume = 1;
            SceneManager.LoadScene("TitleScreen");
        }
        //Fade out
        if (finish &&!fadeMusic &&!CheckEffect()) {
            if (timer < 1.75f &&!pause)
                pause = true;

            if (timer < 1.75f && timer > 1 &&!sameImage)
                img.color = new Color(img.color.r, img.color.g, img.color.b, (-timer + 1.75f) * 4/3);
            else if (timer > 1.75f && pause) {
                pause = false;
                timer = 0.0f;
                finish = false;
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
                //Check end of intro
                if (text.AllLinesComplete()) {
                    fadeMusic = true;
                    text.DestroyChars();
                } else {
                    img.sprite = SpriteRegistry.Get("Intro/" + imagePaths[++currentIndex]);
                    img.SetNativeSize();
                    img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    img.rectTransform.position = new Vector2(320, 240);
                    timerEffect = 0.0f;
                    if (specialEffects[currentIndex] != string.Empty)
                        try { ApplyEffect((Effect)Enum.Parse(typeof(Effect), specialEffects[currentIndex])); }
                        catch { UnitaleUtil.DisplayLuaError("IntroManager", "The effect " + specialEffects[currentIndex] + " doesn't exist."); }
                    text.NextLineText();
                    start = true;
                }
            }
        //Fade in
        } else if (start &&!fadeMusic) {
            if (timer < 0.5f &&!sameImage)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 2 * timer);
            else {
                start = false;
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1);
                timer = 0.0f;
                if (currentIndex != textsToDisplay.Length - 1)
                    if (goToNextDirect[currentIndex + 1] == "Y")                        sameImage = true;
                    else if (goToNextDirect[currentIndex + 1] == "N")                   sameImage = false;
                    else if (imagePaths[currentIndex] == imagePaths[currentIndex + 1])  sameImage = true;
                    else                                                                sameImage = false;
                else                                                                    sameImage = false;
            }
        //End of intro
        } else if (fadeMusic) {
            if (timer < 1)
                Camera.main.GetComponent<AudioSource>().volume = 1 - timer;
            else {
                Camera.main.GetComponent<AudioSource>().Stop();
                Camera.main.GetComponent<AudioSource>().volume = 1;
                SceneManager.LoadScene("TitleScreen");
            }
        //End of current page
        } else if (text.LineComplete() &&!start &&!CheckEffect()) {
            finish = true;
            timer = 0;
        }
    }

    void ApplyEffect(Effect e) {
        currentEffect = e;
        switch(e) {
            case Effect.SCROLLUP:
                if (img.rectTransform.sizeDelta.y < (mask ? 220 : 480))
                    UnitaleUtil.DisplayLuaError("IntroManager", "You can't apply a scroll down effect on an image which has a lower y boundary than the screen's y boundary.");
                img.rectTransform.pivot = new Vector2(0.5f, 1);
                img.rectTransform.position = new Vector2(img.rectTransform.position.x, mask ? img.rectTransform.sizeDelta.y + 204 : img.rectTransform.sizeDelta.y);
                break;
            case Effect.SCROLLDOWN:
                if (img.rectTransform.sizeDelta.y < (mask ? 220 : 480))
                    UnitaleUtil.DisplayLuaError("IntroManager", "You can't apply a scroll down effect on an image which has a lower y boundary than the screen's y boundary.");
                img.rectTransform.pivot = new Vector2(0.5f, 0);
                img.rectTransform.position = new Vector2(img.rectTransform.position.x, mask ? 424 - img.rectTransform.sizeDelta.y : 480 - img.rectTransform.sizeDelta.y);
                break;
            case Effect.SCROLLLEFT:
                if (img.rectTransform.sizeDelta.x < (mask ? 400 : 640))
                    UnitaleUtil.DisplayLuaError("IntroManager", "You can't apply a scroll down effect on an image which has a lower x boundary than the screen's x boundary.");
                img.rectTransform.pivot = new Vector2(0, 0.5f);
                img.rectTransform.position = new Vector2(mask ? 520 - img.rectTransform.sizeDelta.x : 640 - img.rectTransform.sizeDelta.y, img.rectTransform.position.y);
                break;
            case Effect.SCROLLRIGHT:
                if (img.rectTransform.sizeDelta.x < (mask ? 400 : 640))
                    UnitaleUtil.DisplayLuaError("IntroManager", "You can't apply a scroll down effect on an image which has a lower x boundary than the screen's x boundary.");
                img.rectTransform.pivot = new Vector2(1, 0.5f);
                img.rectTransform.position = new Vector2(mask ? img.rectTransform.sizeDelta.x + 120 : img.rectTransform.sizeDelta.x, img.rectTransform.position.y);
                break;
        }
    }

    void UpdateEffect() {
        if (timerEffect < 4)
            timerEffect += Time.deltaTime;
        else
            switch (currentEffect) {
                case Effect.SCROLLUP:    img.rectTransform.position = new Vector2(img.rectTransform.position.x, img.rectTransform.position.y - 1);  break;
                case Effect.SCROLLDOWN:  img.rectTransform.position = new Vector2(img.rectTransform.position.x, img.rectTransform.position.y + 1);  break;
                case Effect.SCROLLRIGHT: img.rectTransform.position = new Vector2(img.rectTransform.position.x + 1, img.rectTransform.position.y);  break;
                case Effect.SCROLLLEFT:  img.rectTransform.position = new Vector2(img.rectTransform.position.x - 1, img.rectTransform.position.y);  break;
            }
    }

    bool CheckEffect() {
        switch (currentEffect) {
            case Effect.SCROLLUP:    return img.rectTransform.position.y > (mask ? 424 : 480);
            case Effect.SCROLLDOWN:  return img.rectTransform.position.y < (mask ? 204 : 0);
            case Effect.SCROLLRIGHT: return img.rectTransform.position.x < (mask ? 120 : 0);
            case Effect.SCROLLLEFT:  return img.rectTransform.position.x > (mask ? 520 : 640);
            default:                 return false;
        }
    }
}
