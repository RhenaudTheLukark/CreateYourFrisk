using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

public class EventManager : MonoBehaviour {
    private int EventLayer;                 //The id of the Event Layer
    public ScriptWrapper script;            //The script we have to load
    private PlayerOverworld po;             //The current PlayerOverworld
    public List<GameObject> events;         //This map's events
    public Hashtable sprCtrls = new Hashtable();
    private TextManager textmgr;            //The current TextManager
    public int actualEventIndex = -1;       //ID of the actual event we're running
    public bool readyToReLaunch = false;    //Used to prevent overworld GameOver errors
    private bool bgmCoroutine = false;      //Check if the BGM is already fading
    public bool passPressOnce = false;      //Boolean used because events are boring
    public bool scriptLaunched = false;
    public bool relaunchReset1 = false, relaunchReset2 = false;
    public Hashtable autoDone = new Hashtable();

    //Don't judge me please
    string[] bindList = new string[] { "SetDialog", "SetAnimOW", "SetChoice", "TeleportEvent", "MoveEventToPoint", "GetReturnPoint",
                                       "SetReturnPoint", "SupprVarOW", "SetBattle", "DispImg", "SupprImg", "WaitForInput", "SetTone", "RotateEvent",
                                       "GameOver", "SetAnimSwitch", "PlayBGMOW", "StopBGMOW", "PlaySoundOW", "Rumble", "Flash", "Save", "Heal",
                                       "Hurt", "AddItem", "RemoveItem", "StopEvent", "RemoveEvent", "AddMoney", "SetEventPage", "GetSpriteOfEvent",
                                       "CenterEventOnCamera", "ResetCameraPosition", "MoveCamera", "Wait"};

    // Use this for initialization
    public void Start() {
        po = GameObject.Find("Player").GetComponent<PlayerOverworld>();
        EventLayer = LayerMask.GetMask("EventLayer");                               //Get the layer that'll interact with our object, here EventLayer

        textmgr = GameObject.Find("TextManager OW").GetComponent<TextManager>();
        relaunchReset1 = true;
    }

    // Update is called once per frame
    void Update() {
        if (readyToReLaunch && SceneManager.GetActiveScene().name != "TransitionOverworld") {
            readyToReLaunch = false;
            Start();
        }
        if (relaunchReset1) {
            relaunchReset1 = false;
            relaunchReset2 = true;
        } else if (relaunchReset2) {
            relaunchReset2 = false;
            ResetEvents();
        }
        if (GlobalControls.fadeAuto)
            testEventAuto();
        else if (!PlayerOverworld.inText) {
            if (testEventAuto()) return;
            if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && !passPressOnce) {
                RaycastHit2D hit;
                testEventPress(po.lastMove.x, po.lastMove.y, out hit);
            } else if (passPressOnce && GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0)
                passPressOnce = false;

            if (events.Count != 0)
                foreach (GameObject go in events) {
                    EventOW ev = go.GetComponent<EventOW>();
                    if (ev.actualPage == -1) {
                        events.Remove(go);
                        Destroy(go);
                    } else if (!testContainsListVector2(ev.eventTriggers, ev.actualPage)) {
                        UnitaleUtil.displayLuaError(ev.name, "The trigger of the page n°" + ev.actualPage + " doesn't exist.\nYou'll need to add it via Unity, on this event's EventOW Component.");
                        return;
                    }
                }
        }
    }

    public static bool testContainsListVector2(List<Vector2> list, int testValue) {
        foreach (Vector2 v in list)
            if (v.x == testValue)
                return true;
        return false;
    }

    //TODO: Fix the array problem, forcing arrays to be printed "(Table)" in the text.
    //WHY: WAY less arguments on few functions like DispImg
    //Yeah, I need more Action delegates. Why are you looking me that way ? 
    //If you have a better idea than passing 10 thousands arguments, just tell me how I can 
    //fix my Table to string problem, then we'll be able to talk about these...things :/
    //(Yeah, I'm a bit aggressive in here, but just forget about that xD)
    private delegate void Action<T1, T2, T3, T4, T5>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    private delegate void Action<T1, T2, T3, T4, T5, T6>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    private delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    private delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    private delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    private delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    /// <summary>
    /// Tests if the player activates an event while he was hitting the "Confirm" button on the map
    /// </summary>
    /// <param name="xDir"></param>
    /// <param name="yDir"></param>
    /// <param name="hit"></param>
    public bool testEventPress(float xDir, float yDir, out RaycastHit2D hit) {
        try {
            BoxCollider2D boxCollider = GameObject.Find("Player").GetComponent<BoxCollider2D>();
            Transform transform = GameObject.Find("Player").transform;

            //Store start position to move from, based on objects current transform position
            Vector2 start = new Vector2(transform.position.x + transform.localScale.x * boxCollider.offset.x,
                                        transform.position.y + transform.localScale.x * boxCollider.offset.y);

            //Calculate end position based on the direction parameters passed in when calling Move and using our boxCollider
            Vector2 dir = new Vector2(xDir, yDir);

            //Calculate the current size of the object's boxCollider
            Vector2 size = new Vector2(boxCollider.size.x * po.PlayerPos.localScale.x, boxCollider.size.y * po.PlayerPos.localScale.y);

            //Disable boxCollider so that linecast doesn't hit this object's own collider and disable the non touching events' colliders
            boxCollider.enabled = false;
            foreach (GameObject go in events)
                if (getTrigger(go, go.GetComponent<EventOW>().actualPage) != 0)
                    go.GetComponent<Collider2D>().enabled = false;

            //Cast a box from start point to end point checking collision on blockingLayer
            //hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(xDir * PlayerOverworld.instance.speed, 2) + 
            //                                                        Mathf.Pow(yDir * PlayerOverworld.instance.speed, 2)), EventLayer);          
            hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(boxCollider.size.x * po.transform.localScale.x * xDir, 2) +
                                                                    Mathf.Pow(boxCollider.size.y * po.transform.localScale.x * yDir, 2)), EventLayer);

            //Re-enable the disabled colliders after BoxCast
            boxCollider.enabled = true;
            foreach (GameObject go in events)
                if (getTrigger(go, go.GetComponent<EventOW>().actualPage) != 0)
                    go.GetComponent<Collider2D>().enabled = true;

            //Executes the event that our cast collided with
            if (hit.collider == null) {
                UnitaleUtil.writeInLog("No event was hit.");
                return false;
            } else
                return executeEvent(hit.collider.gameObject);
        } catch {
            hit = new RaycastHit2D();
            return false;
        }
    }

    public bool testEventAuto() {
        try {
            foreach (GameObject go in events) {
                if (!go)                               events.Remove(go);
                else if (!go.GetComponent<EventOW>())  events.Remove(go);
                else if (getTrigger(go, go.GetComponent<EventOW>().actualPage) == 2)
                    if (!autoDone.ContainsKey(new object[] { go, go.GetComponent<EventOW>().actualPage }))
                        return executeEvent(go, 0, true);
            }
        } catch { }
        return false;
    }

    public int getTrigger(GameObject go, int index) {
        foreach (Vector2 vec in go.GetComponent<EventOW>().eventTriggers)
            if (vec.x == index)
                return (int)vec.y;
        return -2;
    }

    /// <summary>
    /// Resets the events by counting them all again, stopping the current event and destroying all the current images
    /// </summary>
    public void ResetEvents() {
        if (events != null)
            for (int i = 0; i < events.Count; i++)
                try {
                    if (events[i].name.Contains("Image") || events[i].name.Contains("Tone"))
                        GameObject.Destroy(events[i]);
                } catch { events.RemoveAt(i--); }
        //GameObject[] eventsTemp = GameObject.FindGameObjectsWithTag("Event");
        events.Clear();
        autoDone.Clear();
        sprCtrls.Clear();
        foreach (EventOW ev in GameObject.FindObjectsOfType<EventOW>()) {
            events.Add(ev.gameObject);
            if (ev.gameObject.GetComponent<SpriteRenderer>())
                sprCtrls[ev.gameObject.name] = new LuaSpriteController(ev.gameObject.GetComponent<SpriteRenderer>(), "empty"); //TODO: Find a way to get the sprite's name
        }
    }

    /// <summary>
    /// Function that executes the event "go"
    /// </summary>
    /// <param name="go"></param>
    [HideInInspector]
    public bool executeEvent(GameObject go, int beginText = 0, bool auto = false) {
        if (scriptLaunched)
            return false;
        string scriptToLoad = go.GetComponent<EventOW>().scriptToLoad;
        actualEventIndex = -1;
        for (int i = 0; i < events.Count; i++)
            if (events[i].Equals(go)) {
                actualEventIndex = i;
                break;
            }
        if (actualEventIndex == -1) {
            UnitaleUtil.displayLuaError("Overworld engine", "Whoops! There is an error with event indexing.");
            return false;
        }
        //If the script we have to load exists, let's initialize it and then execute it
        if (scriptToLoad != null) {
            if (go.GetComponent<EventOW>().actualPage == -1) {
                events.Remove(go);
                Destroy(go);
                return false;
            }
            PlayerOverworld.inText = true;  //UnitaleUtil.writeInLogAndDebugger("executeEvent true:" + go.GetComponent<EventOW>().actualPage);
            scriptLaunched = true;
            //print(go.name);
            textmgr.textQueue = new TextMessage[] { };
            try {
                initScript(scriptToLoad);
                script.Call("EventPage" + go.GetComponent<EventOW>().actualPage);
            } catch (InterpreterException ex) {
                UnitaleUtil.displayLuaError(scriptToLoad, ex.DecoratedMessage);
                return false;
            }
            textmgr.setCaller(script);
            if (beginText != 0)  textmgr.setTextQueueAfterValue(beginText);
            else                 textmgr.setTextQueue(textmgr.textQueue);
            passPressOnce = true;
            if (auto)
                autoDone.Add(new object[] { go, go.GetComponent<EventOW>().actualPage }, true);
            return true;
        } else
            UnitaleUtil.writeInLog("There is no script to load!");
        return false;
    }

    /// <summary>
    /// Function that permits to initialize the event script to be used later
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Returns true if no error were encountered.</returns>
    private bool initScript(string name) {
        script = new ScriptWrapper(true);
        script.scriptname = name;
        string scriptText = ScriptRegistry.Get(ScriptRegistry.EVENT_PREFIX + name);
        if (scriptText == null) {
            UnitaleUtil.displayLuaError("Launching an event", "The event " + name + " doesn't exist.");
            return false;
        }
        try { script.DoString(scriptText); } 
        catch (InterpreterException ex) {
            UnitaleUtil.displayLuaError(name, ex.DecoratedMessage);
            return false;
        }

        foreach (string str in bindList)
            script.Bind(str, new Action<DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue>(AddToDoListByText));

        script.Bind("FSetChoice", new Action<bool, bool, int>(SetChoice));
        script.Bind("FSetAnimOW", new Action<string, string>(SetAnimOW));
        script.Bind("FSetAnimSwitch", new Action<string, string>(SetAnimSwitch));
        script.Bind("FTeleportEvent", new Action<string, float, float>(TeleportEvent));
        script.Bind("FMoveEventToPoint", new Action<string, float, float, bool>(MoveEventToPoint));
        script.Bind("FDispImg", new Action<string, int, float, float, float, float, int, int, int, int>(DispImg));
        script.Bind("FSupprImg", new Action<int>(SupprImg));
        script.Bind("FRotateEvent", new Action<string, float, float, float, bool>(RotateEvent));
        script.Bind("FSetTone", new Action<bool, bool, int, int, int, int>(SetTone));
        script.Bind("FSetBattle", new Action<string, bool, bool>(SetBattle));
        script.Bind("FGameOver", new Action<string, string>(GameOver));
        script.Bind("FGetReturnPoint", new Action<int>(GetReturnPoint));
        script.Bind("FSetReturnPoint", new Action<int>(SetReturnPoint));
        script.Bind("FWaitForInput", new Action(WaitForInput));
        script.Bind("FPlayBGMOW", new Action<string, float>(PlayBGMOW));
        script.Bind("FStopBGMOW", new Action<float>(StopBGMOW));
        script.Bind("FPlaySoundOW", new Action<string, float>(PlaySoundOW));
        script.Bind("FRumble", new Action<float, float, bool>(Rumble));
        script.Bind("FFlash", new Action<float, bool, int, int, int, int>(Flash));
        script.Bind("FSave", new Action(Save));
        script.Bind("FHeal", new Action<int>(Heal));
        script.Bind("FHurt", new Action<int>(Hurt));
        script.Bind("FAddItem", new Func<string, bool>(AddItem));
        script.Bind("FRemoveItem", new Action<int>(RemoveItem));
        script.Bind("FStopEvent", new Action(StopEvent));
        script.Bind("FRemoveEvent", new Action<string>(RemoveEvent));
        script.Bind("FAddMoney", new Action<int>(AddMoney));
        script.Bind("FSetEventPage", new Action<string, int>(SetEventPage));
        script.Bind("FGetSpriteOfEvent", new Func<string, LuaSpriteController>(GetSpriteOfEvent));
        script.Bind("FCenterEventOnCamera", new Action<string, int, bool>(CenterEventOnCamera));
        script.Bind("FMoveCamera", new Action<int, int, int, bool>(MoveCamera));
        script.Bind("FResetCameraPosition", new Action<int, bool>(ResetCameraPosition));
        script.Bind("FWait", new Action<int>(Wait));

        return true;
    }

    /// <summary>
    /// Only used for identification
    /// </summary>
    public void AddToDoListByText(DynValue parameter1, DynValue parameter2, DynValue parameter3, DynValue parameter4, DynValue parameter5, 
                                  DynValue parameter6, DynValue parameter7, DynValue parameter8, DynValue parameter9, DynValue parameter10) { }

    /// <summary>
    /// Used to add an element into the Text Event
    /// </summary>
    /// <param name="function">Name of the function</param>
    /// <param name="parameters">Parameters of the function</param>
    public void AddToDoListByText (string function, DynValue[] parameters = null) {
        if (parameters == null)
            parameters = new DynValue[] { };
        int paramNumber = 0;
        for (; paramNumber < parameters.Length; paramNumber++)
            if (parameters[paramNumber].Type == DataType.Void)
                break;
        Array.Resize(ref parameters, paramNumber);

        //Everybody don't care about spaces, so let's just remove them from the function's name
        function = function.TrimStart('"').TrimEnd('"').Replace(" ", string.Empty);
        switch (function) {
            //Set a normal dialogue
            case "SetDialog":
                TextMessage[] textmsgs = new TextMessage[parameters[0].Table.Length];
                string[] texts = UnitaleUtil.TableToList(parameters[0].Table, v => v.String).ToArray(), mugshots = new string[texts.Length];
                if (parameters.Length > 2)
                    mugshots = UnitaleUtil.TableToList(parameters[2].Table, v => v.String).ToArray();
                for (int i = 0; i < parameters[0].GetLength().Number; i++)
                    textmsgs[i] = new TextMessage(texts[i], parameters[1].Boolean, false, mugshots[i]);
                //foreach (TextMessage txt in textmsgs)
                //    UnitaleUtil.writeInLog(txt.Text);
                textmgr.addToTextQueue(textmsgs);
                break;
            //Set a dialogue
            case "SetChoice":
                //Works with 2 choices for now, I don't think you'll need more
                TextMessage textMsgChoice = new TextMessage("", false, false, true);
                textMsgChoice.addToText("[noskipatall][mugshot:null]");
                string[] finalText = new string[3]; int index = -1;
                bool question = false, threeLines = false;
                //Do not put more than 3 lines and 2 choices
                //If the 3rd parameter is a string, it has to be a question
                if (parameters.Length > 2 && parameters[2].Type == DataType.String) {
                    if (parameters[2].Type == DataType.String) {
                        textMsgChoice.addToText(parameters[2].String);

                        int lengthAfter = parameters[0].String.Split('\n').Length;
                        if (parameters[1].String.Split('\n').Length > lengthAfter) lengthAfter = parameters[1].String.Split('\n').Length;

                        if (lengthAfter > 2)  textMsgChoice.addToText(finalText[0] + "\n");
                        else                  textMsgChoice.addToText(finalText[0] + "\n" + "\n");
                        question = true;

                    }
                }

                //If the last parameter is a number, this is because you wanted to choose the index of the Temp
                if (parameters[parameters.Length - 1].Type == DataType.Number)
                    index = (int)parameters[parameters.Length - 1].Number;
                else {
                    UnitaleUtil.displayLuaError("Overworld : SetChoice", "You need an index to register the result of the choice !");
                    return;
                }
                
                for (int i = 0; i < paramNumber; i ++) {
                    //If there's no text, just don't print it
                    if (i == 2 && question)
                        break;
                    if (parameters[i].String == null)
                        continue;

                    string[] preText = parameters[i].String.Split('\n'), text = new string[3];
                    if (preText.Length == 3)
                        threeLines = true;
                    for (int j = 0; j < 3; j++) {
                        if (j < preText.Length)  text[j] = preText[j];
                        else                     text[j] = "";
                    }

                    for (int k = 0; k < 3; k++) {
                        if (text[k] != "")
                            if (k == 0)  text[k] = "* " + text[k];
                            else         text[k] = "  " + text[k];
                                
                        finalText[k] += text[k] + '\t';
                        if (k == text.Length - 1)
                            break;
                    }
                }
                //Add the text to the text to print then the SetChoice function with its parameters
                textMsgChoice.addToText(finalText[0] + "\n" + finalText[1] + "\n" + finalText[2]);
                textMsgChoice.addToText("[func:FSetChoice," + DynValue.NewBoolean(question) + "," + DynValue.NewBoolean(threeLines) + "," + index + "]");
                //UnitaleUtil.writeInLog(textMsgChoice.Text);
                textmgr.addToTextQueue(textMsgChoice);
                break;
            //Else, it's juste a normal function
            default:
                TextMessage textmsg;
                if (!bindList.Contains(function)) {
                    UnitaleUtil.displayLuaError(events[actualEventIndex].GetComponent<EventOW>().scriptToLoad, "The function " + function + " doesn't exist.");
                    return;
                }
                textmsg = new TextMessage("[noskipatall][func:F" + function, true, false, false);
                foreach (DynValue v in parameters)
                    textmsg.addToText("," + v);
                textmsg.addToText("]");
                //UnitaleUtil.writeInLog(textmsg.Text);
                textmgr.addToTextQueue(textmsg);
                break;
            }
        textmgr.ResetCurrentCharacter();
    }

    /// <summary>
    /// Used in SetChoice, permits to set the temporary GameObject we created to stars' positions
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="question"></param>
    private void setPlayerOnSelection(int selection, bool question = false, bool threeLines = false) {
        if (question) {
            if (threeLines)  selection += 2;
            else             selection += 4;
        }
        Vector2 upperLeft = new Vector2(61, GameObject.Find("letter(Clone)").GetComponent<RectTransform>().position.y + (GameObject.Find("letter(Clone)").GetComponent<RectTransform>().sizeDelta.y / 2) - 1);
        int xMv = selection % 2; // remainder safe again, selection is never negative
        int yMv = selection / 2;
        // HACK: remove hardcoding of this sometime, ever... probably not happening lmao
        GameObject.Find("tempHeart").GetComponent<RectTransform>().position = new Vector3(upperLeft.x + xMv * 303, upperLeft.y - yMv * textmgr.Charset.LineSpacing, 0);
    }

    /// <summary>
    /// Enter a vector, returns the direction of the vector accordingly to computer's numeric pads (yeah, how great for those who don't have one)
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private int CheckDirection(Vector2 dir) {
        //2 = Down, 4 = Left, 6 = Right, 8 = Up
        float tempDir = dir.y / dir.x;
        if (tempDir > 1)        return 2;
        else if (tempDir < -1)  return 8;
        else if (dir.x > 0)     return 6;
        else                    return 4;
    }

    //Used to add event states before unloading the map
    public static void SetEventStates() {
        int id = SceneManager.GetActiveScene().buildIndex;
        EventOW[] events = (EventOW[])GameObject.FindObjectsOfType(typeof(EventOW));
        PlayerOverworld.instance.eventmgr.sprCtrls.Clear();

        if (!GlobalControls.MapEventPages.ContainsKey(id))
            GlobalControls.MapEventPages.Add(id, new Dictionary<string, int>());

        foreach (EventOW ev in events) {
            if (GlobalControls.MapEventPages[id].ContainsKey(ev.gameObject.name))
                GlobalControls.MapEventPages[id].Remove(ev.gameObject.name);
            GlobalControls.MapEventPages[id].Add(ev.gameObject.name, ev.actualPage);
        }
    }

    public static void GetEventStates(int id) {
        if (!GlobalControls.MapEventPages.ContainsKey(id))
            return;

        foreach (string str in GlobalControls.MapEventPages[id].Keys)
            GameObject.Find(str).GetComponent<EventOW>().actualPage = GlobalControls.MapEventPages[id][str];
    }

    /// <summary>
    /// Used to end a current event
    /// </summary>
    public void endTextEvent() {
        try {
            foreach (string key in LuaScriptBinder.GetDictionary().Keys)
                if (key.Contains("Choice") || key.Contains("ReturnPoint") || key.Contains("GetSpriteOfEvent"))
                    LuaScriptBinder.Remove(key);
        } catch { }
        scriptLaunched = false;
        PlayerOverworld.instance.textmgr.setTextFrameAlpha(0);
        PlayerOverworld.instance.textmgr.textQueue = new TextMessage[] { };
        PlayerOverworld.instance.textmgr.destroyText();
        PlayerOverworld.inText = false; //UnitaleUtil.writeInLogAndDebugger("endTextEvent false");
    }

    //-----------------------------------------------------------------------------------------------------------
    //                                        ---   Lua Functions   ---
    //
    //                If the function isn't a text, the function have to be ended manually with 
    //                "textmgr.skipNowIfBlocked = true;" : if you don't do that, the text will
    //                   be blocked in an infinite loop, waiting for this value to be true.
    //
    //                 Plus, if you want to create functions, test first if the GameObject the
    //                  player is accessing to is an event : if you don't, you'll be a really
    //                            nasty person and you'll go to hell. Dont ask why.
    //-----------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Permits to teleport an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    private void TeleportEvent(string name, float dirX, float dirY) {
        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                go.transform.position = new Vector3(dirX, dirY, go.transform.position.z);
                textmgr.skipNowIfBlocked = true;
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function doesn't exists. Did you forget to add the 'Event' tag ?");
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Move the event from a point to another directly.
    /// Stops if the player can't move to that direction.
    /// The animation process is automatic, if you renamed the triggers that the script needs to animate your event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    private void MoveEventToPoint(string name, float dirX, float dirY, bool wallPass = false) { StartCoroutine(IMoveEventToPoint(name, dirX, dirY, wallPass)); }
    
    IEnumerator IMoveEventToPoint(string name, float dirX, float dirY, bool wallPass) {
        //This function moves the player : this boolean is used to disable encounter generation during this event
        po.forcedMove = true;
        GameObject[] colliders = new GameObject[GameObject.Find("Background").transform.childCount];
        for (int i = 0; i < colliders.Length; i++)
            colliders[i] = GameObject.Find("Background").transform.GetChild(i).gameObject;
        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                int direction = -1;
                if (wallPass)
                    foreach (GameObject go2 in colliders)
                        go2.SetActive(false);
                go.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                Vector2 endPoint = new Vector2(dirX - go.transform.position.x, dirY - go.transform.position.y), endPointFromNow = endPoint;
                //The animation process is automatic, if you renamed the Animation's triggers and animations as the Player's
                if (go.GetComponent<Animator>() != null)
                    direction = CheckDirection(endPoint);
                //While the current position is different from the one we want our player to have
                while ((Vector2)go.transform.position != endPoint) {
                    Vector2 clamped = Vector2.ClampMagnitude(endPoint, 1);
                    //Test is used to know if the deplacement is possible or not
                    bool test = false;

                    if (go != GameObject.Find("Player")) {
                        if (go.GetComponent<EventOW>().moveSpeed < endPointFromNow.magnitude)
                            test = po.AttemptMove(clamped.x, clamped.y, go, wallPass);
                        //If we reached the destination, stop the function
                        else {
                            test = po.AttemptMove(endPointFromNow.x, endPointFromNow.y, go, wallPass);
                            po.forcedMove = false;
                            if (wallPass)
                                foreach (GameObject go2 in colliders)
                                    go2.SetActive(true);
                            textmgr.skipNowIfBlocked = true;
                            go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                            yield break;
                        }
                    } else {
                        if (PlayerOverworld.instance.speed < endPointFromNow.magnitude)
                            test = po.AttemptMove(clamped.x, clamped.y, go, wallPass);
                        //If we reached the destination, stop the function
                        else {
                            test = po.AttemptMove(endPointFromNow.x, endPointFromNow.y, go, wallPass);
                            po.forcedMove = false;
                            if (wallPass)
                                foreach (GameObject go2 in colliders)
                                    go2.SetActive(true);
                            textmgr.skipNowIfBlocked = true;
                            go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                            yield break;
                        }
                    }
                    yield return 0;

                    if (!test && !wallPass) {
                        po.forcedMove = false;
                        textmgr.skipNowIfBlocked = true;
                        go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                        yield break;
                    } else {
                        //The animations are here !
                        switch (direction) {
                            case 2:  go.GetComponent<Animator>().SetTrigger("MovingDown");  break;
                            case 4:  go.GetComponent<Animator>().SetTrigger("MovingLeft");  break;
                            case 6:  go.GetComponent<Animator>().SetTrigger("MovingRight"); break;
                            case 8:  go.GetComponent<Animator>().SetTrigger("MovingUp");    break;
                        }
                    }
                    endPointFromNow = new Vector2(dirX - go.transform.position.x, dirY - go.transform.position.y);
                }
                go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function doesn't exists. Did you forget to add the 'Event' tag ?");
        po.forcedMove = false;
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Function that permits to put an animation on an event
    /// </summary>
    /// <param name="name"></param>
    /// <param name="anim"></param>
    private void SetAnimOW(string name, string anim) {
        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                try { go.GetComponent<Animator>().Play(anim); } 
                catch {
                    //If the GameObject's Animator component already exists
                    if (go.GetComponent<Animator>())  UnitaleUtil.writeInLog("The current anim doesn't exists.");
                    else                              UnitaleUtil.writeInLog("This GameObject doesn't have an Animator component!");
                }
                textmgr.skipNowIfBlocked = true;
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag?");
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Makes a choice, as when you have to choose the blue or the red pill
    /// </summary>
    /// <param name="question"></param>
    /// <param name="varIndex"></param>
    private void SetChoice(bool question = false, bool threeLines = false, int varIndex = 1) { StartCoroutine(ISetChoice(varIndex, question, threeLines)); }

    IEnumerator ISetChoice(int varIndex, bool question, bool threeLines) {
        //Omg a new GameObject ! One more heart on the screen ! Wooh !
        GameObject tempHeart = new GameObject("tempHeart", typeof(RectTransform));
        tempHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(16, 16);
        tempHeart.transform.SetParent(GameObject.Find("Canvas OW").transform);
        Image img = tempHeart.AddComponent<Image>();
        img.sprite = GameObject.Find("utHeart").GetComponent<Image>().sprite;
        img.color = new Color(1, 0, 0, 1);
        int actualChoice = 0;

        //We'll need to set the heart to the good positions, to be able to know where is our selection
        setPlayerOnSelection(0, question, threeLines);
        while (true) {
            if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
                actualChoice = (actualChoice + 1) % 2;
                setPlayerOnSelection(actualChoice, question, threeLines);
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED)
                break;
            yield return 0;
        }
        //Hide the heart, we don't need it anymore
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
        //Add a new variable that can be used in lua functions named Temp plus the index we gave earlier (or else a free index)
        /*if (varIndex == -1) {
            for (int i = 1; i > -1; i++)
                if (LuaScriptBinder.Get(null, "Choice" + i) == null) {
                    print("Choice" + i);
                    LuaScriptBinder.Set(null, "Choice" + i, DynValue.NewNumber(actualChoice));
                    break;
                }
        } else {*/
        LuaScriptBinder.Set(null, "Choice" + varIndex, DynValue.NewNumber(actualChoice));
        //}
        //HEARTBROKEN
        GameObject.Destroy(tempHeart);
        //Here we don't need our textmgr.skipNowIfBlocked = true, because we execute the event again, and doing this will display the next page correctly
        //try {
        scriptLaunched = false;
        executeEvent(events[actualEventIndex], textmgr.currentLine + 1);
        //} catch { }
    }

    /// <summary>
    /// Set a return point for the program. If you have to use while iterations, use this instead, with GetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    private void SetReturnPoint(int index) { LuaScriptBinder.Set(null, "ReturnPoint" + index, DynValue.NewNumber(textmgr.currentLine));   textmgr.skipNowIfBlocked = true; }

    /// <summary>
    /// Forces the program to go back to the return point of the chosen index. If you have to use while iterations, use this instead, with SetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    private void GetReturnPoint(int index) { textmgr.currentLine = (int)LuaScriptBinder.Get(null, "ReturnPoint" + index).Number;   textmgr.skipNowIfBlocked = true; }

    /// <summary>
    /// Sets an encounter of the current mod folder, with a given encounter name
    /// The boolean is used to tell if the encounter anim will be short
    /// </summary>
    /// <param name="encounterName"></param>
    /// <param name="quickAnim"></param>
    private void SetBattle(string encounterName, bool quickAnim = false, bool ForceNoFlee = false) { po.SetEncounterAnim(encounterName, quickAnim, ForceNoFlee); }

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
    private void DispImg(string path, int id, float posX, float posY, float dimX, float dimY, int toneR = 255, int toneG = 255, int toneB = 255, int toneA = -1) {
        if (GameObject.Find("Image" + id) != null) {
            GameObject image1 = GameObject.Find("Image" + id);
            image1.GetComponent<Image>().sprite = SpriteRegistry.Get(path);
            if (toneA >= 0 && toneA <= 255 && toneR % 1 == 0)
                if (toneR < 0 || toneR > 255 || toneR % 1 != 0 || toneG < 0 || toneG > 255 || toneG % 1 != 0 || toneB < 0 || toneB > 255 || toneB % 1 != 0)
                    UnitaleUtil.displayLuaError(script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
                else
                    image1.GetComponent<Image>().color = new Color32((byte)toneR, (byte)toneG, (byte)toneB, (byte)toneA);
            image1.GetComponent<RectTransform>().sizeDelta = new Vector2(dimX, dimY);
            image1.GetComponent<RectTransform>().position = new Vector2(posX, posY);
            textmgr.skipNowIfBlocked = true;
            return;
        } else {
            GameObject image = Instantiate(Resources.Load<GameObject>("Prefabs/Image 1"));
            image.name = "Image" + id;
            image.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas OW").transform);
            image.GetComponent<Image>().sprite = SpriteRegistry.Get(path);
            if (toneA >= 0 && toneA <= 255 && toneR % 1 == 0)
                if (toneR < 0 || toneR > 255 || toneR % 1 != 0 || toneG < 0 || toneG > 255 || toneG % 1 != 0 || toneB < 0 || toneB > 255 || toneB % 1 != 0)
                    UnitaleUtil.displayLuaError(script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
                else
                    image.GetComponent<Image>().color = new Color32((byte)toneR, (byte)toneG, (byte)toneB, (byte)toneA);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(dimX, dimY);
            image.GetComponent<RectTransform>().position = new Vector2(posX, posY);
            events.Add(image);
        }
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Remove an image from the screen
    /// </summary>
    /// <param name="id"></param>
    private void SupprImg(int id) {
        if (GameObject.Find("Image" + id))
            RemoveEvent("Image" + id);
        else {
            UnitaleUtil.writeInLog("The given image doesn't exists.");
            textmgr.skipNowIfBlocked = true;
        }
    }

    /// <summary>
    /// Function that ends when the player press the button "Confirm"
    /// </summary>
    private void WaitForInput() { StartCoroutine(IWaitForInput()); }

    IEnumerator IWaitForInput() {
        while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED)
            yield return 0;

        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Sets a tone directly, without transition
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    private void SetTone(bool anim, bool waitEnd, int r = 255, int g = 255, int b = 255, int a = 0) {
        if (r < 0 || r > 255 || r % 1 != 0 || g < 0 || g > 255 || g % 1 != 0 || b < 0 || b > 255 || b % 1 != 0) {
            UnitaleUtil.displayLuaError(script.scriptname, "You can't input a value out of [0; 255] for a color value, as it is clamped from 0 to 255.\nThe number have to be an integer.");
            textmgr.skipNowIfBlocked = true;
        } else {
            if (!anim)
                if (GameObject.Find("Tone") == null) {
                    GameObject tone = Instantiate(Resources.Load<GameObject>("Prefabs/Image"));
                    tone.name = "Tone";
                    tone.GetComponent<RectTransform>().parent = GameObject.Find("Canvas OW").transform;
                    tone.GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    tone.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
                    tone.GetComponent<RectTransform>().position = new Vector2(320, 240);
                    events.Add(tone);
                } else
                    GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            else
                StartCoroutine(ISetTone(waitEnd, r, g, b, a));
            if (!(anim && waitEnd))
                textmgr.skipNowIfBlocked = true;
        }
    }

    IEnumerator ISetTone(bool waitEnd, int r, int g, int b, int a) {
        int alpha, lack;
        if (GameObject.Find("Tone") == null) {
            GameObject image = Instantiate(Resources.Load<GameObject>("Prefabs/Image"));
            image.GetComponent<Image>().color = new Color(image.GetComponent<Image>().color.r, image.GetComponent<Image>().color.g, image.GetComponent<Image>().color.b, 0);
            alpha = 0; lack = a;
            image.name = "Tone";
            image.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas OW").transform);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
            image.GetComponent<RectTransform>().position = new Vector2(320, 240);
            events.Add(image);
            bool darker = false;
            if (lack > 0)
                darker = true;
            while (GameObject.Find("Tone").GetComponent<Image>().color != new Color32((byte)r, (byte)g, (byte)b, (byte)a)) {
                if (darker) {
                    if (lack <= 4)
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    else {
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)(alpha + 4));
                        alpha += 4;
                        lack -= 4;
                    }
                } else {
                    if (lack >= -4)
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);

                    else {
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)(alpha - 4));
                        alpha -= 4;
                        lack += 4;
                    }
                }
                yield return 0;
            }
        } else {
            alpha = (int)(GameObject.Find("Tone").GetComponent<Image>().color.a * 255);
            lack = a - alpha;
            bool darker = false;
            if (lack > 0)
                darker = true;
            while (GameObject.Find("Tone").GetComponent<Image>().color != new Color32((byte)r, (byte)g, (byte)b, (byte)a)) {
                if (darker) {
                    if (lack <= 4)
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    else {
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)(alpha + 4));
                        alpha += 4;
                        lack -= 4;
                    }
                } else {
                    if (lack >= -4)
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    else {
                        GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)(alpha - 4));
                        alpha -= 4;
                        lack += 4;
                    }
                }
                yield return 0;
            }
        }
        if (waitEnd)
            textmgr.skipNowIfBlocked = true;
    }
    
    /// <summary>
    /// Rotates the sprite of an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rotateX"></param>
    /// <param name="rotateY"></param>
    /// <param name="rotateZ"></param>
    /// <param name="axisAnim"></param>
    private void RotateEvent(string name, float rotateX, float rotateY, float rotateZ, bool anim = true) {
        if (anim) {
            StartCoroutine(IRotateEvent(name, rotateX, rotateY, rotateZ));
        } else {
            for (int i = 0; i < events.Count || name == "Player"; i++) {
                GameObject go = null;
                try { go = events[i]; } catch { }
                if (name == go.name || name == "Player") {
                    if (name == "Player")
                        go = GameObject.Find("Player");
                    go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                    textmgr.skipNowIfBlocked = true;
                    return;
                }
            }
            UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
            textmgr.skipNowIfBlocked = true;
        }
    }

    IEnumerator IRotateEvent(string name, float rotateX, float rotateY, float rotateZ) {
        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");

                float lackX = rotateX - go.transform.rotation.eulerAngles.x;
                float lackY = rotateY - go.transform.rotation.eulerAngles.y;
                float lackZ = rotateZ - go.transform.rotation.eulerAngles.z;

                float best, basisBest;
                if (Mathf.Abs(lackX) > Mathf.Abs(lackY))  best = lackX;
                else                                      best = lackY;
                if (Mathf.Abs(best) < Mathf.Abs(lackZ))   best = lackZ;

                bool reverse = false;
                if (best > 0) reverse = true;
                basisBest = Mathf.Abs(best);

                while (best != 0) {
                    if ((best > -4 && best < 4))
                        break;
                    go.transform.rotation = Quaternion.Euler(rotateX + ((basisBest - Mathf.Abs(best - basisBest)) * lackX / basisBest), 
                                                             rotateY + ((basisBest - Mathf.Abs(best - basisBest)) * lackY / basisBest), 
                                                             rotateZ + ((basisBest - Mathf.Abs(best - basisBest)) * lackZ / basisBest));
                    if (reverse) best -= 4;
                    else         best += 4;
                    yield return 0;
                }
                go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                textmgr.skipNowIfBlocked = true;
                yield break;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Launch the GameOver screen
    /// </summary>
    private void GameOver(string deathText = null, string deathMusic = null) {
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
        po.enabled = false;

        UnitaleUtil.writeInLogAndDebugger(GameObject.FindObjectOfType<GameOverBehavior>().name);
        UnitaleUtil.writeInLogAndDebugger("Before launch");

        GameObject.FindObjectOfType<GameOverBehavior>().StartDeath(deathTable2, deathMusic);
        UnitaleUtil.writeInLogAndDebugger("After launch");
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Triggers a specific anim switch on a chosen event. (uses Animator)
    /// </summary>
    /// <param name="name">The name of the event</param>
    /// <param name="triggerName">The name of the trigger</param>
    private void SetAnimSwitch(string name, string triggerName) {
        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                Animator anim = go.GetComponent<Animator>();
                if (go == null) {
                    UnitaleUtil.displayLuaError(script.scriptname, "This event doesn't have an Animator component !");
                    textmgr.skipNowIfBlocked = true;
                    return;
                }
                po.forcedAnim = true;
                anim.SetTrigger(triggerName);
                textmgr.skipNowIfBlocked = true;
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Plays and adjust the volume of a chosen bgm.
    /// </summary>
    /// <param name="bgm">The name of the chosen BGM to play.</param>
    /// <param name="volume">The volume of the BGM. Clamped from 0 to 1.</param>
    private void PlayBGMOW(string bgm, float volume) {
        if (volume > 1 || volume < 0)
            UnitaleUtil.displayLuaError(script.scriptname, "You can't input a value out of [0; 1] for the volume, as it is clamped from 0 to 1.");
        else if (AudioClipRegistry.GetMusic(bgm) == null)
            UnitaleUtil.displayLuaError(script.scriptname, "The given BGM doesn't exists. Please check if you didn't mispelled it.");
        else {
            AudioSource audio = GameObject.Find("Main Camera OW").GetComponent<AudioSource>();
            audio.clip = AudioClipRegistry.GetMusic(bgm);
            audio.volume = volume;
            audio.Play();
        }
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Stops the current BGM.
    /// </summary>
    /// <param name="fadeTime"></param>
    private void StopBGMOW(float fadeTime) {
        if (bgmCoroutine)
            UnitaleUtil.displayLuaError(script.scriptname, "The music is already fading.");
        else if (!GameObject.Find("Main Camera OW").GetComponent<AudioSource>().isPlaying)
            UnitaleUtil.displayLuaError(script.scriptname, "There is no current BGM.");
        else if (fadeTime < 0)
            UnitaleUtil.displayLuaError(script.scriptname, "The fade time has to be positive or equal to 0.");
        else
            StartCoroutine(fadeBGM(fadeTime));
        textmgr.skipNowIfBlocked = true;
    }

    IEnumerator fadeBGM(float fadeTime) {
        bgmCoroutine = true;
        AudioSource audio = GameObject.Find("Main Camera OW").GetComponent<AudioSource>();
        float time = 0, startVolume = audio.volume;
        while (time < fadeTime) {
            audio.volume = startVolume - (startVolume * time / fadeTime);
            time += Time.deltaTime;
            yield return 0;
        }
        audio.Stop();
        audio.volume = 1;
        bgmCoroutine = false;
    }

    /// <summary>
    /// Plays a selected sound at a given volume.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="volume"></param>
    private void PlaySoundOW(string sound, float volume) {
        if (volume > 1 || volume < 0)
            UnitaleUtil.displayLuaError(script.scriptname, "You can't input a value out of [0; 1] for the volume, as it is clamped from 0 to 1.");
        else if (AudioClipRegistry.GetMusic(sound) == null)
            UnitaleUtil.displayLuaError(script.scriptname, "The given BGM doesn't exists. Please check if you didn't mispelled it.");
        else
            UnitaleUtil.PlaySound("OverworldSound", AudioClipRegistry.GetSound(sound), volume);
        GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetMusic(sound), volume);
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="intensity"></param>
    private void Rumble(float seconds, float intensity = 3, bool fade = false) { StartCoroutine(IRumble(seconds, intensity, fade)); }

    IEnumerator IRumble(float seconds, float intensity, bool fade) {
        Vector2 shift = new Vector2(0, 0), shiftOld = new Vector2(0, 0); float time = 0, intensityBasis = intensity;
        while (time < seconds) {
            shiftOld = shift;

            if (fade)
                intensity = intensityBasis * (1 - (time / seconds));
            shift = new Vector2(((UnityEngine.Random.value - 0.5f) * intensity) - shift.x, ((UnityEngine.Random.value - 0.5f) * intensity) - shift.y);
            
            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                go.transform.position = new Vector3(go.transform.position.x + shift.x, go.transform.position.y + shift.y, go.transform.position.z);
            shift += shiftOld;
            time += Time.deltaTime;
            yield return 0;
        }
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            go.transform.position = new Vector3(go.transform.position.x - shiftOld.x, go.transform.position.y + shiftOld.y, go.transform.position.z);
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Rumbles the screen.
    /// </summary>
    /// <param name="secondsOrFrames"></param>
    /// <param name="intensity"></param>
    private void Flash(float secondsOrFrames, bool isSeconds = false, int colorR = 255, int colorG = 255, int colorB = 255, int colorA = 255) {
        StartCoroutine(IFlash(secondsOrFrames, isSeconds, colorR, colorG, colorB, colorA));
    }

    IEnumerator IFlash(float secondsOrFrames, bool isSeconds, int colorR, int colorG, int colorB, int colorA) {
        GameObject flash = new GameObject("flash", new Type[] { typeof(Image) });
        flash.transform.SetParent(GameObject.Find("Canvas OW").transform);
        flash.transform.position = new Vector3(320, 240);
        flash.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
        flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)colorA);
        if (isSeconds) {
            float time = 0;
            while (time < secondsOrFrames) {
                if (time != 0)
                    flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)(colorA - colorA * time / secondsOrFrames));
                time += Time.deltaTime;
                yield return 0;

            }
        } else
            for (int frame = 0; frame < secondsOrFrames; frame ++) {
                if (frame != 0) 
                    flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)(colorA - colorA * frame / secondsOrFrames));
                yield return 0;
                    
            }
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Saves the game. Pretty obvious, heh.
    /// </summary>
    public void Save() {
        StartCoroutine(ISave());
    }

    IEnumerator ISave() {
        bool save = true;  Color c = GameObject.Find("utHeart").GetComponent<Image>().color;
        GameObject.Find("utHeart").transform.position = new Vector3(151, 233, GameObject.Find("utHeart").transform.position.z);
        GameObject.Find("utHeart").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
        GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
        TextManager txtLevel = GameObject.Find("TextManagerLevel").GetComponent<TextManager>(), txtTime = GameObject.Find("TextManagerTime").GetComponent<TextManager>(),
                    txtMap = GameObject.Find("TextManagerMap").GetComponent<TextManager>(), txtName = GameObject.Find("TextManagerName").GetComponent<TextManager>(),
                    txtSave = GameObject.Find("TextManagerSave").GetComponent<TextManager>(), txtReturn = GameObject.Find("TextManagerReturn").GetComponent<TextManager>();
        txtLevel.setHorizontalSpacing(2); txtTime.setHorizontalSpacing(2); txtMap.setHorizontalSpacing(2);
        txtName.setHorizontalSpacing(2); txtSave.setHorizontalSpacing(2); txtReturn.setHorizontalSpacing(2);
        foreach (RectTransform t in GameObject.Find("save_interior").transform)
            t.sizeDelta = new Vector2(t.sizeDelta.x, t.sizeDelta.y + 1);
        GameObject.Find("save_interior").GetComponent<RectTransform>().sizeDelta = new Vector2(GameObject.Find("save_interior").GetComponent<RectTransform>().sizeDelta.x, 
                                                                                               GameObject.Find("save_interior").GetComponent<RectTransform>().sizeDelta.y - 1);

        string playerName = ""; double playerLevel = 0;//, playerTime = 0;
        bool isAlreadySave = false;
        if (SaveLoad.savedGame != null) isAlreadySave = true;
        if (isAlreadySave) {
            playerName = SaveLoad.savedGame.player.Name;
            playerLevel = SaveLoad.savedGame.player.LV;
            //SaveLoad.savedGame.playerVariablesNum.TryGetValue("PlayerTime", out playerTime);

            txtName.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + playerName, false, true) });
            txtLevel.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]LV" + playerLevel, false, true) });
            txtTime.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]0:00", false, true) });
            txtMap.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + SaveLoad.savedGame.lastScene, false, true) });
        } else {
            txtName.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]EMPTY", false, true) });
            txtLevel.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]LV0", false, true) });
            txtTime.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]0:00", false, true) });
            txtMap.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]--", false, true) });
        }
        txtSave.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]Save", false, true) });
        txtReturn.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]Return", false, true) });
        
        GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        
        while (true) {
            if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
                if (!save)
                    GameObject.Find("utHeart").transform.position = new Vector3(151, GameObject.Find("utHeart").transform.position.y, GameObject.Find("utHeart").transform.position.z);
                else
                    GameObject.Find("utHeart").transform.position = new Vector3(331, GameObject.Find("utHeart").transform.position.y, GameObject.Find("utHeart").transform.position.z);
                save = !save;
            } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
                GameObject.Find("utHeart").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                txtName.destroyText(); txtLevel.destroyText(); txtTime.destroyText(); txtMap.destroyText(); txtSave.destroyText(); txtReturn.destroyText();
                GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                textmgr.skipNowIfBlocked = true;
                yield break;
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                if (save) {
                    SaveLoad.Save();
                    GameObject.Find("utHeart").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    txtName.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + PlayerCharacter.instance.Name, false, true) });
                    txtLevel.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]LV" + PlayerCharacter.instance.LV, false, true) });
                    txtTime.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]0:00", false, true) });
                    txtMap.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + SaveLoad.savedGame.lastScene, false, true) });
                    txtSave.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]File saved.", false, true) });
                    txtReturn.setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]", false, true) });
                    foreach (Image img in GameObject.Find("save_interior").transform.GetComponentsInChildren<Image>())
                        img.color = new Color(1, 1, 0, 1);
                    GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                    GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("saved"));
                    GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    do {
                        passPressOnce = true;
                        yield return 0;
                    } while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED);
                }
                GameObject.Find("utHeart").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                txtName.destroyText(); txtLevel.destroyText(); txtTime.destroyText(); txtMap.destroyText(); txtSave.destroyText(); txtReturn.destroyText();
                GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                textmgr.skipNowIfBlocked = true;
                yield break;
            }
            yield return 0;
        }
    }

    /// <summary>
    /// Hurts the player with the given amount of damage. This is the reverse of Heal().
    /// </summary>
    /// <param name="damage">This one seems obvious too</param>
    public void Hurt(int damage) {
        if (damage == 0) {
            textmgr.skipNowIfBlocked = true;
            return;
        } else if (damage > 0) UnitaleUtil.PlaySound("HealthSound", AudioClipRegistry.GetSound("hurtsound"));
        else                   UnitaleUtil.PlaySound("HealthSound", AudioClipRegistry.GetSound("healsound"));

        if (-damage + PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP)  PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        else if (-damage + PlayerCharacter.instance.HP <= 0)                PlayerCharacter.instance.HP = 1;
        else                                                       PlayerCharacter.instance.HP -= damage;
        textmgr.skipNowIfBlocked = true;
    }

    /// <summary>
    /// Heals the player with the given amount of heal. This is the reverse of Hurt().
    /// </summary>
    /// <param name="heal">This one seems obvious too</param>
    public void Heal(int heal) { Hurt(-heal); }

    public bool AddItem(string Name) { textmgr.skipNowIfBlocked = true; return Inventory.AddItem(Name); }

    public void RemoveItem(int ID) { Inventory.RemoveItem(ID-1); textmgr.skipNowIfBlocked = true; }

    public void StopEvent() { endTextEvent(); }

    public void RemoveEvent(string eventName) {
        if (!GameObject.Find(eventName)) {
            print("Doesn't exist.");
            textmgr.skipNowIfBlocked = true;
        } else {
            events.Remove(GameObject.Find(eventName));
            Destroy(GameObject.Find(eventName));
        }
        textmgr.skipNowIfBlocked = true;
    }

    public void AddMoney(int amount) {
        if (PlayerCharacter.instance.Gold + amount < 0) PlayerCharacter.instance.Gold = 0;
        else if (PlayerCharacter.instance.Gold + amount > ControlPanel.instance.GoldLimit) PlayerCharacter.instance.Gold = ControlPanel.instance.GoldLimit;
        else PlayerCharacter.instance.Gold += amount;
        textmgr.skipNowIfBlocked = true;
    }

    public void SetEventPage(string ev, int page) {
        if (ev == "wowyoucanttakeitdudeyeahnoyoucant")
            ev = events[actualEventIndex].name;
        SetEventPage2(ev, page);
        textmgr.skipNowIfBlocked = true;
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
    }

    public void TitleScreen() { SceneManager.LoadScene(SceneManager.GetSceneByName("TransitionTitleScreen").buildIndex); }

    public LuaSpriteController GetSpriteOfEvent(string name) {
        foreach (object key in sprCtrls.Keys) {
            if (key.ToString() == name) {
                LuaScriptBinder.SetBattle(null, "GetSpriteOfEvent" + name, UserData.Create((LuaSpriteController)sprCtrls[name], LuaSpriteController.data));
                scriptLaunched = false;
                executeEvent(events[actualEventIndex], textmgr.currentLine + 1);
                return (LuaSpriteController)sprCtrls[name];
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

        StartCoroutine(IMoveCamera((int)(GameObject.Find(name).transform.position.x - po.transform.position.x), (int)(GameObject.Find(name).transform.position.y - po.transform.position.y), 
                                   speed, straightLine));
    }

    public void MoveCamera(int pixX, int pixY, int speed = 5, bool straightLine = false) {
        StartCoroutine(IMoveCamera(pixX, pixY, speed, straightLine)); 
}

    public void ResetCameraPosition(int speed = 5, bool straightLine = false) { StartCoroutine(IMoveCamera(0, 0, speed, straightLine)); }

    public IEnumerator IMoveCamera(int pixX, int pixY, int speed, bool straightLine) {
        if (speed <= 0) {
            UnitaleUtil.displayLuaError(PlayerOverworld.instance.eventmgr.events[PlayerOverworld.instance.eventmgr.actualEventIndex].name, "The speed of the camera has to be strictly positive.");
            textmgr.skipNowIfBlocked = true;
            yield break;
        }
        float xSpeed = speed, ySpeed = speed, currentX = 0, currentY = 0;
        if (straightLine) {
            Vector2 clamped = Vector2.ClampMagnitude(new Vector2(pixX, pixY), speed);
            xSpeed = clamped.x;
            ySpeed = clamped.y;
        }
        while (currentX != pixX || currentY != pixY) {
            if (currentX != pixX) {
                if (xSpeed > pixX - currentX)
                    xSpeed = pixX - currentX;
                currentX += xSpeed;
                po.cameraShift.x += xSpeed;
            }
            if (currentY != pixY) {
                if (ySpeed > pixY - currentY)
                    ySpeed = pixY - currentY;
                currentY += ySpeed;
                po.cameraShift.y += ySpeed;
            }
            yield return 0;
        }
        textmgr.skipNowIfBlocked = true;
    }

    public void Wait(int frames) { StartCoroutine(IWait(frames)); }

    public IEnumerator IWait(int frames) {
        int curr = 0;
        while(curr != frames) {
            curr++;
            yield return 0;
        }
        textmgr.skipNowIfBlocked = true;
    }
}