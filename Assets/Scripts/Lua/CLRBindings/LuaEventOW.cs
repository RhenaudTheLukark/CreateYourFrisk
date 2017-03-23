using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuaEventOW {
    private EventManager eventmgr;
    private TextManager textmgr;

    public delegate void LoadedAction(string name, object args);
    public static event LoadedAction StCoroutine;


    public LuaEventOW(EventManager eventmgr, TextManager textmgr) {
        this.eventmgr = eventmgr;
        this.textmgr = textmgr;
    }

    /// <summary>
    /// Displays a text.
    /// </summary>
    /// <param name="texts"></param>
    /// <param name="formatted"></param>
    /// <param name="mugshots"></param>
    public void SetDialog(DynValue texts, bool formatted, DynValue mugshots = null) {
        TextMessage[] textmsgs = new TextMessage[texts.Table.Length];
        for (int i = 0; i < texts.GetLength().Number; i++)
            textmsgs[i] = new TextMessage(texts.Table.Get(i + 1).String, formatted, false, mugshots.ToString() != "void" ? mugshots.Table.Get(i + 1).String : null);
        textmgr.setTextQueue(textmsgs);
    }

    public void SetChoice(DynValue choices, string question = null) {
        bool threeLines = false;
        TextMessage textMsgChoice = new TextMessage("", false, false, true);
        textMsgChoice.addToText("[mugshot:null]");
        string[] finalText = new string[3];

        //Do not put more than 3 lines and 2 choices
        //If the 3rd parameter is a string, it has to be a question
        if (question != null) {
            textMsgChoice.addToText(question);

            int lengthAfter = question.Split('\n').Length;
            if (question.Split('\n').Length > lengthAfter) lengthAfter = question.Split('\n').Length;

            if (lengthAfter > 2) textMsgChoice.addToText(finalText[0] + "\n");
            else textMsgChoice.addToText(finalText[0] + "\n\n");
        }

        for (int i = 0; i < choices.Table.Length; i++) {
            //If there's no text, just don't print it
            if (i == 2 && question != null)
                break;
            if (choices.Table.Get(i + 1).String == null)
                continue;

            string[] preText = choices.Table.Get(i + 1).String.Split('\n'), text = new string[3];
            if (preText.Length == 3)
                threeLines = true;
            for (int j = 0; j < 3; j++) {
                if (j < preText.Length) text[j] = preText[j];
                else text[j] = "";
            }

            for (int k = 0; k < 3; k++) {
                if (text[k] != "")
                    if (k == 0) text[k] = "* " + text[k];
                    else text[k] = "  " + text[k];

                finalText[k] += text[k] + '\t';
                if (k == text.Length - 1)
                    break;
            }
        }

        //Add the text to the text to print then the SetChoice function with its parameters
        textMsgChoice.addToText(finalText[0] + "\n" + finalText[1] + "\n" + finalText[2]);
        textmgr.setText(textMsgChoice);

        StCoroutine("ISetChoice", new object[] { question != null, threeLines });
    }

    /// <summary>
    /// Permits to teleport an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    public void TeleportEvent(string name, float dirX, float dirY) {
        for (int i = 0; i < eventmgr.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = eventmgr.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                go.transform.position = new Vector3(dirX, dirY, go.transform.position.z);
                eventmgr.script.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function doesn't exists. Did you forget to add the 'Event' tag ?");
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Move the event from a point to another directly.
    /// Stops if the player can't move to that direction.
    /// The animation process is automatic, if you renamed the triggers that the eventmgr.script needs to animate your event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    [CYFEventFunction]
    public void MoveEventToPoint(string name, float dirX, float dirY, bool wallPass = false) { StCoroutine("IMoveEventToPoint", new object[] { name, dirX, dirY, wallPass }); }

    /// <summary>
    /// Function that permits to put an animation on an event
    /// </summary>
    /// <param name="name"></param>
    /// <param name="anim"></param>
    public void SetAnimOW(string name, string anim) {
        for (int i = 0; i < eventmgr.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = eventmgr.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                try { go.GetComponent<Animator>().Play(anim); } 
                catch {
                    //If the GameObject's Animator component already exists
                    if (go.GetComponent<Animator>())  UnitaleUtil.writeInLog("The current anim doesn't exist.");
                    else                              UnitaleUtil.writeInLog("This GameObject doesn't have an Animator component!");
                }
                eventmgr.script.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag?");
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Makes a choice, like when you have to choose the blue or the red pill
    /// </summary>
    /// <param name="question"></param>
    /// <param name="varIndex"></param>


    /// <summary>
    /// Set a return point for the program. If you have to use while iterations, use this instead, with GetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    public void SetReturnPoint(int index) {
        LuaScriptBinder.Set(null, "ReturnPoint" + index, DynValue.NewNumber(textmgr.currentLine));
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Forces the program to go back to the return point of the chosen index. If you have to use while iterations, use this instead, with SetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    public void GetReturnPoint(int index) {
        textmgr.currentLine = (int)LuaScriptBinder.Get(null, "ReturnPoint" + index).Number;
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Sets an encounter of the current mod folder, with a given encounter name
    /// The boolean is used to tell if the encounter anim will be short
    /// </summary>
    /// <param name="encounterName"></param>
    /// <param name="quickAnim"></param>
    public void SetBattle(string encounterName, bool quickAnim = false, bool ForceNoFlee = false) { PlayerOverworld.instance.SetEncounterAnim(encounterName, quickAnim, ForceNoFlee); }

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
    public void DispImg(string path, int id, float posX, float posY, float dimX, float dimY, int toneR = 255, int toneG = 255, int toneB = 255, int toneA = -1) {
        if (GameObject.Find("Image" + id) != null) {
            GameObject image1 = GameObject.Find("Image" + id);
            image1.GetComponent<Image>().sprite = SpriteRegistry.Get(path);
            if (toneA >= 0 && toneA <= 255 && toneR % 1 == 0)
                if (toneR < 0 || toneR > 255 || toneR % 1 != 0 || toneG < 0 || toneG > 255 || toneG % 1 != 0 || toneB < 0 || toneB > 255 || toneB % 1 != 0)
                    UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
                else
                    image1.GetComponent<Image>().color = new Color32((byte)toneR, (byte)toneG, (byte)toneB, (byte)toneA);
            image1.GetComponent<RectTransform>().sizeDelta = new Vector2(dimX, dimY);
            image1.GetComponent<RectTransform>().position = new Vector2(posX, posY);
            eventmgr.script.Call("CYFEventNextCommand");
            return;
        } else {
            GameObject image = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Image 1"));
            image.name = "Image" + id;
            image.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas OW").transform);
            image.GetComponent<Image>().sprite = SpriteRegistry.Get(path);
            if (toneA >= 0 && toneA <= 255 && toneR % 1 == 0)
                if (toneR < 0 || toneR > 255 || toneR % 1 != 0 || toneG < 0 || toneG > 255 || toneG % 1 != 0 || toneB < 0 || toneB > 255 || toneB % 1 != 0)
                    UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
                else
                    image.GetComponent<Image>().color = new Color32((byte)toneR, (byte)toneG, (byte)toneB, (byte)toneA);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(dimX, dimY);
            image.GetComponent<RectTransform>().position = new Vector2(posX, posY);
            eventmgr.events.Add(image);
        }
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Remove an image from the screen
    /// </summary>
    /// <param name="id"></param>
    public void SupprImg(int id) {
        if (GameObject.Find("Image" + id))
            RemoveEvent("Image" + id);
        else {
            UnitaleUtil.writeInLog("The given image doesn't exists.");
            eventmgr.script.Call("CYFEventNextCommand");
        }
    }

    /// <summary>
    /// Function that ends when the player press the button "Confirm"
    /// </summary>
    public void WaitForInput() { StCoroutine("IWaitForInput", null); }

    /// <summary>
    /// Sets a tone directly, without transition
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    public void SetTone(bool anim, bool waitEnd, int r = 255, int g = 255, int b = 255, int a = 0) {
        if (r < 0 || r > 255 || r % 1 != 0 || g < 0 || g > 255 || g % 1 != 0 || b < 0 || b > 255 || b % 1 != 0) {
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
            eventmgr.script.Call("CYFEventNextCommand");
        } else {
            if (!anim)
                if (GameObject.Find("Tone") == null) {
                    GameObject tone = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Image"));
                    tone.name = "Tone";
                    tone.GetComponent<RectTransform>().parent = GameObject.Find("Canvas OW").transform;
                    tone.GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    tone.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
                    tone.GetComponent<RectTransform>().position = new Vector2(320, 240);
                    eventmgr.events.Add(tone);
                } else
                    GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            else
                StCoroutine("ISetTone", new object[] { waitEnd, r, g, b, a });
            if (!(anim && waitEnd))
                eventmgr.script.Call("CYFEventNextCommand");
        }
    }
    
    /// <summary>
    /// Rotates the sprite of an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rotateX"></param>
    /// <param name="rotateY"></param>
    /// <param name="rotateZ"></param>
    /// <param name="axisAnim"></param>
    public void RotateEvent(string name, float rotateX, float rotateY, float rotateZ, bool anim = true) {
        if (anim) {
            StCoroutine("IRotateEvent", new object[] { name, rotateX, rotateY, rotateZ });
        } else {
            for (int i = 0; i < eventmgr.events.Count || name == "Player"; i++) {
                GameObject go = null;
                try { go = eventmgr.events[i]; } catch { }
                if (name == go.name || name == "Player") {
                    if (name == "Player")
                        go = GameObject.Find("Player");
                    go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                    eventmgr.script.Call("CYFEventNextCommand");
                    return;
                }
            }
            UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
            eventmgr.script.Call("CYFEventNextCommand");
        }
    }

    /// <summary>
    /// Launch the GameOver screen
    /// </summary>
    public void GameOver(string deathText = null, string deathMusic = null) {
        UnitaleUtil.writeInLogAndDebugger("GameOver time");
        PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        Transform rt = GameObject.Find("Player").GetComponent<Transform>();
        rt.position = new Vector3(rt.position.x, rt.position.y, -1000);
        int index = 0;
        string[] deathTable2 = null;

        if (deathText != null) {
            List<string> deathTable = new List<string>();
            int j = deathText.Length;
            for (int i = 0; i < j; i ++) {
                if (deathText[i] == '\r' && deathText[i] != '\n') {
                    deathTable.Add(deathText.Substring(index, i - index));
                    index = i+1;
                }
            }
            deathTable.Add(deathText.Substring(index, deathText.Length - index));
            deathTable2 = new string[deathTable.Count];
            for (int i = 0; i < deathTable.Count; i++)
                deathTable2[i] = deathTable[i];
        }

        GlobalControls.Music = GameObject.Find("Background").GetComponent<MapInfos>().isMusicKeptBetweenBattles ? PlayerOverworld.audioKept.clip : MusicManager.src.clip;
        PlayerOverworld.instance.enabled = false;

        UnitaleUtil.writeInLogAndDebugger(GameObject.FindObjectOfType<GameOverBehavior>().name);
        UnitaleUtil.writeInLogAndDebugger("Before launch");

        GameObject.FindObjectOfType<GameOverBehavior>().StartDeath(deathTable2, deathMusic);
        UnitaleUtil.writeInLogAndDebugger("After launch");
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Triggers a specific anim switch on a chosen event. (uses Animator)
    /// </summary>
    /// <param name="name">The name of the event</param>
    /// <param name="triggerName">The name of the trigger</param>
    public void SetAnimSwitch(string name, string triggerName) {
        for (int i = 0; i < eventmgr.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = eventmgr.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                Animator anim = go.GetComponent<Animator>();
                if (go == null) {
                    UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "This event doesn't have an Animator component !");
                    eventmgr.script.Call("CYFEventNextCommand");
                    return;
                }
                PlayerOverworld.instance.forcedAnim = true;
                anim.SetTrigger(triggerName);
                eventmgr.script.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Plays and adjust the volume of a chosen bgm.
    /// </summary>
    /// <param name="bgm">The name of the chosen BGM to play.</param>
    /// <param name="volume">The volume of the BGM. Clamped from 0 to 1.</param>
    public void PlayBGMOW(string bgm, float volume) {
        if (volume > 1 || volume < 0)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "You can't input a value out of [0; 1] for the volume, as it is clamped from 0 to 1.");
        else if (AudioClipRegistry.GetMusic(bgm) == null)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "The given BGM doesn't exist. Please check if you haven't mispelled it.");
        else {
            AudioSource audio = GameObject.Find("Main Camera OW").GetComponent<AudioSource>();
            audio.clip = AudioClipRegistry.GetMusic(bgm);
            audio.volume = volume;
            audio.Play();
        }
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Stops the current BGM.
    /// </summary>
    /// <param name="fadeTime"></param>
    public void StopBGMOW(float fadeTime) {
        if (eventmgr.bgmCoroutine)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "The music is already fading.");
        else if (!GameObject.Find("Main Camera OW").GetComponent<AudioSource>().isPlaying)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "There is no current BGM.");
        else if (fadeTime < 0)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "The fade time has to be positive or equal to 0.");
        else
            StCoroutine("fadeBGM", fadeTime);
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Plays a selected sound at a given volume.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="volume"></param>
    public void PlaySoundOW(string sound, float volume) {
        if (volume > 1 || volume < 0)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "You can't input a value out of [0; 1] for the volume, as it is clamped from 0 to 1.");
        else if (AudioClipRegistry.GetMusic(sound) == null)
            UnitaleUtil.displayLuaError(eventmgr.script.scriptname, "The given BGM doesn't exiss. Please check if you haven't mispelled it.");
        else
            UnitaleUtil.PlaySound("OverworldSound", AudioClipRegistry.GetSound(sound), volume);
        GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetMusic(sound), volume);
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="intensity"></param>
    public void Rumble(float seconds, float intensity = 3, bool fade = false) { StCoroutine("IRumble", new object[] { seconds, intensity, fade }); }

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="secondsOrFrames"></param>
    /// <param name="intensity"></param>
    public void Flash(float secondsOrFrames, bool isSeconds = false, int colorR = 255, int colorG = 255, int colorB = 255, int colorA = 255) {
        StCoroutine("IFlash", new object[] { secondsOrFrames, isSeconds, colorR, colorG, colorB, colorA });
    }

    /// <summary>
    /// Saves the game. Pretty obvious, heh.
    /// </summary>
    public void Save() {
        StCoroutine("ISave", null);
    }

    /// <summary>
    /// Hurts the player with the given amount of damage. Heal()'s opposite.
    /// </summary>
    /// <param name="damage">This one seems obvious too</param>
    public void Hurt(int damage) {
        if (damage == 0) {
            eventmgr.script.Call("CYFEventNextCommand");
            return;
        } else if (damage > 0) UnitaleUtil.PlaySound("HealthSound", AudioClipRegistry.GetSound("hurtsound"));
        else                   UnitaleUtil.PlaySound("HealthSound", AudioClipRegistry.GetSound("healsound"));

        if (-damage + PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP)  PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        else if (-damage + PlayerCharacter.instance.HP <= 0)                PlayerCharacter.instance.HP = 1;
        else                                                       PlayerCharacter.instance.HP -= damage;
        eventmgr.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Heals the player with the given amount of heal. Hurt()'s opposite.
    /// </summary>
    /// <param name="heal">This one seems obvious too</param>
    public void Heal(int heal) { Hurt(-heal); eventmgr.script.Call("CYFEventNextCommand"); }

    public DynValue AddItem(string Name) {
        try { return DynValue.NewBoolean(Inventory.AddItem(Name)); } 
        finally {
            eventmgr.script.script.Globals.Set(DynValue.NewString("CYFEventLameNeedReturn"), DynValue.NewBoolean(true));
            eventmgr.script.Call("CYFEventNextCommand");
        }
    }

    public void RemoveItem(int ID) { Inventory.RemoveItem(ID-1); eventmgr.script.Call("CYFEventNextCommand"); }

    public void StopEvent() { eventmgr.endTextEvent(); }

    public void RemoveEvent(string eventName) {
        GameObject go = GameObject.Find(eventName);
        if (!go)
            Debug.LogError("The event " + eventName + " doesn't exist but you tried to remove it.");
        else {
            eventmgr.events.Remove(go);
            GameObject.Destroy(go);
        }
        if (eventmgr.script != null && eventmgr.scriptLaunched)
            eventmgr.script.Call("CYFEventNextCommand");
    }

    public void AddMoney(int amount) {
        if (PlayerCharacter.instance.Gold + amount < 0) PlayerCharacter.instance.Gold = 0;
        else if (PlayerCharacter.instance.Gold + amount > ControlPanel.instance.GoldLimit) PlayerCharacter.instance.Gold = ControlPanel.instance.GoldLimit;
        else PlayerCharacter.instance.Gold += amount;
        eventmgr.script.Call("CYFEventNextCommand");
    }

    public void SetEventPage(string ev, int page) {
        if (ev == "wowyoucanttakeitdudeyeahnoyoucant")
            ev = eventmgr.events[eventmgr.actualEventIndex].name;
        SetEventPage2(ev, page);
        eventmgr.script.Call("CYFEventNextCommand");
    }

    public static void SetEventPage2(string eventName, int page) {
        if (!GameObject.Find(eventName)) {
            UnitaleUtil.displayLuaError(PlayerOverworld.instance.eventmgr.events[PlayerOverworld.instance.eventmgr.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        if (!GameObject.Find(eventName).GetComponent<EventOW>()) {
            UnitaleUtil.displayLuaError(PlayerOverworld.instance.eventmgr.events[PlayerOverworld.instance.eventmgr.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        if (!GlobalControls.MapEventPages.ContainsKey(SceneManager.GetActiveScene().buildIndex))
            GlobalControls.MapEventPages.Add(SceneManager.GetActiveScene().buildIndex, new Dictionary<string, int>());

        if (GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex].ContainsKey(eventName))
            GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex][eventName] = page;
        else
            GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex].Add(eventName, page);

        GameObject.Find(eventName).GetComponent<EventOW>().actualPage = page;
        if (PlayerOverworld.instance.eventmgr.scriptLaunched)
            PlayerOverworld.instance.eventmgr.script.Call("CYFEventNextCommand");
    }

    public void TitleScreen() {
        SceneManager.LoadScene(SceneManager.GetSceneByName("TransitionTitleScreen").buildIndex);
        eventmgr.script.Call("CYFEventNextCommand");
    }

    public DynValue GetSpriteOfEvent(string name) {
        foreach (object key in eventmgr.sprCtrls.Keys) {
            if (key.ToString() == name) {
                eventmgr.script.script.Globals.Set("test", UserData.Create((LuaSpriteController)eventmgr.sprCtrls[name], LuaSpriteController.data));
                try { return UserData.Create((LuaSpriteController)eventmgr.sprCtrls[name], LuaSpriteController.data); }
                finally {
                    eventmgr.script.script.Globals.Set(DynValue.NewString("CYFEventLameNeedReturn"), DynValue.NewBoolean(true));
                    eventmgr.script.Call("CYFEventNextCommand");
                }
            }
        }
        UnitaleUtil.displayLuaError(PlayerOverworld.instance.eventmgr.events[PlayerOverworld.instance.eventmgr.actualEventIndex].name, "The event " + name + " doesn't have a sprite.");
        return null;
    }

    public void CenterEventOnCamera(string name, int speed = 5, bool straightLine = false) {
        if (!GameObject.Find(name)) {
            UnitaleUtil.displayLuaError(PlayerOverworld.instance.eventmgr.events[PlayerOverworld.instance.eventmgr.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        if (!GameObject.Find(name).GetComponent<EventOW>()) {
            UnitaleUtil.displayLuaError(PlayerOverworld.instance.eventmgr.events[PlayerOverworld.instance.eventmgr.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        StCoroutine("IMoveCamera", new object[] { (int)(GameObject.Find(name).transform.position.x - PlayerOverworld.instance.transform.position.x),
                                                  (int)(GameObject.Find(name).transform.position.y - PlayerOverworld.instance.transform.position.y),
                                                  speed, straightLine });
    }

    public void MoveCamera(int pixX, int pixY, int speed = 5, bool straightLine = false) {
        StCoroutine("IMoveCamera", new object[] { pixX, pixY, speed, straightLine }); 
    }

    public void ResetCameraPosition(int speed = 5, bool straightLine = false) { StCoroutine("IMoveCamera", new object[] { 0, 0, speed, straightLine }); }

    public void Wait(int frames) { StCoroutine("IWait", frames); }

    public DynValue GetPosition(string name) {
        DynValue result = DynValue.NewTable(new Table(null));
        bool done = false;
        for (int i = 0; (i < eventmgr.events.Count || name == "Player") && !done; i++) {
            GameObject go = name == "Player" ? PlayerOverworld.instance.gameObject : eventmgr.events[i];
            done = true;
            result.Table.Set(1, DynValue.NewNumber(go.transform.position.x));
            result.Table.Set(2, DynValue.NewNumber(go.transform.position.y));
            LuaScriptBinder.SetBattle(null, "GetPosition" + name, UserData.Create((LuaSpriteController)eventmgr.sprCtrls[name], LuaSpriteController.data));
        }
        try { return result; } 
        finally { eventmgr.script.Call("CYFEventNextCommand"); }
    }
}
