using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System.Reflection;

public class EventManager : MonoBehaviour {
    public static EventManager instance;
    private int EventLayer;                 //The id of the Event Layer
    public  ScriptWrapper script;           //The script we have to load
    public List<GameObject> events = new List<GameObject>(); //This map's events
    public Hashtable sprCtrls = new Hashtable();
    private TextManager textmgr;            //The current TextManager
    public int actualEventIndex = -1;      //ID of the actual event we're running
    private int page0IndexCount = 0;
    public  bool readyToReLaunch = false;   //Used to prevent overworld GameOver errors
    public bool bgmCoroutine = false;      //Check if the BGM is already fading
    public  bool passPressOnce = false;     //Boolean used because events are boring
    public  bool scriptLaunched = false;
    private bool relaunchReset1 = false, relaunchReset2 = false;
    private bool needReturn = false;
    private Hashtable autoDone = new Hashtable();


    public LuaPlayerOW luaplow;
    public LuaEventOW luaevow;
    public LuaGeneralOW luagenow;
    public LuaInventoryOW luainvow;
    public LuaScreenOW luascrow;

    private Dictionary<Type, string> boundValueName = new Dictionary<Type, string>();
    /*private Type[] bindTypeList = new Type[] {
        typeof(EventManager),
        typeof(LuaPlayerOverworld)
    };

    private List<object> instancesOfTypeList = new List<object>();*/

    //Don't judge me please
    /*private string[] bindList = new string[] { "SetDialog", "SetAnimOW", "SetChoice", "TeleportEvent", "MoveEventToPoint", "GetReturnPoint",
                                               "SetReturnPoint", "SupprVarOW", "SetBattle", "DispImg", "SupprImg", "WaitForInput", "SetTone", "RotateEvent",
                                               "GameOver", "SetAnimSwitch", "PlayBGMOW", "StopBGMOW", "PlaySoundOW", "Rumble", "Flash", "Save", "Heal",
                                               "Hurt", "AddItem", "RemoveItem", "StopEvent", "RemoveEvent", "AddMoney", "SetEventPage", "GetSpriteOfEvent",
                                               "CenterEventOnCamera", "ResetCameraPosition", "MoveCamera", "Wait", "GetPosition"};*/
    //private List<string> dynamicBindList = new List<string>();

    // Use this for initialization
    public void Start() {
        page0IndexCount = 0;
        /*for (int i = 1; i < bindTypeList.Length; i ++)
            instancesOfTypeList.Add(Activator.CreateInstance((Type)bindTypeList[i]));*/
        EventLayer = LayerMask.GetMask("EventLayer");                               //Get the layer that'll interact with our object, here EventLayer
        textmgr = GameObject.Find("TextManager OW").GetComponent<TextManager>();
        if (boundValueName.Count == 0) {
            boundValueName.Add(typeof(LuaEventOW), "Event");
            boundValueName.Add(typeof(LuaPlayerOW), "Player");
            boundValueName.Add(typeof(LuaGeneralOW), "General");
            boundValueName.Add(typeof(LuaInventoryOW), "Inventory");
            boundValueName.Add(typeof(LuaScreenOW), "Screen");
            luaplow = new LuaPlayerOW();
            luaevow = new LuaEventOW(textmgr);
            luagenow = new LuaGeneralOW(textmgr);
            luainvow = new LuaInventoryOW();
            luascrow = new LuaScreenOW();
        }
        relaunchReset1 = true;
        instance = this;
    }

    void OnEnable() {
        LuaEventOW.StCoroutine += StCoroutine;
        LuaPlayerOW.StCoroutine += StCoroutine;
        LuaGeneralOW.StCoroutine += StCoroutine;
        LuaInventoryOW.StCoroutine += StCoroutine;
        LuaScreenOW.StCoroutine += StCoroutine;
    }
    void OnDisable() {
        LuaEventOW.StCoroutine -= StCoroutine;
        LuaPlayerOW.StCoroutine -= StCoroutine;
        LuaGeneralOW.StCoroutine -= StCoroutine;
        LuaInventoryOW.StCoroutine -= StCoroutine;
        LuaScreenOW.StCoroutine -= StCoroutine;
    }

    // Update is called once per frame
    void Update() {
        try {
            if (readyToReLaunch && SceneManager.GetActiveScene().name != "TransitionOverworld") {
                readyToReLaunch = false;
                Start();
            }
            if (relaunchReset1 && !scriptLaunched) {
                for (; page0IndexCount < events.Count; page0IndexCount++)
                    if (events[page0IndexCount] != null)
                        if (testContainsListVector2(events[page0IndexCount].GetComponent<EventOW>().eventTriggers, 0)) {
                            executeEvent(events[page0IndexCount], false, 0);
                            page0IndexCount++;
                            return;
                        }
                relaunchReset1 = false;
                GameObject.FindObjectOfType<Fading>().BeginFade(-1);
                relaunchReset2 = true;
            } else if (relaunchReset2) {
                relaunchReset2 = false;
                PlayerOverworld.instance.utHeart = GameObject.Find("utHeart").GetComponent<Image>();
                ResetEvents();
            } else if (relaunchReset1 || relaunchReset2)
                return;
            testEventDestruction();
            if (!PlayerOverworld.inText) {
                if (testEventAuto()) return;
                if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && !passPressOnce) {
                    RaycastHit2D hit;
                    testEventPress(PlayerOverworld.instance.lastMove.x, PlayerOverworld.instance.lastMove.y, out hit);
                } else if (passPressOnce && GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0)
                    passPressOnce = false;

                if (events.Count != 0)
                    foreach (GameObject go in events) {
                        EventOW ev = go.GetComponent<EventOW>();
                        if (ev.actualPage < -1) { }                             
                        else if (ev.actualPage == -1) {
                            events.Remove(go);
                            Destroy(go);
                        } else if (!testContainsListVector2(ev.eventTriggers, ev.actualPage)) {
                            UnitaleUtil.displayLuaError(ev.name, "The trigger of the page n°" + ev.actualPage + " doesn't exist.\nYou'll need to add it via Unity, on this event's EventOW Component.");
                            return;
                        }
                    }
            } else {
                if (script != null && scriptLaunched) {
                    Table t = script.script.Globals;
                    if (t.Get(DynValue.NewString("CYFEventCoroutine")).Coroutine.State == CoroutineState.Dead)
                        endEvent();
                }
            }
        } catch (InvalidOperationException) { }
    }

    /*private object FunctionLauncher<T>(T classType, string name, object[] values = null) {
        T classInstance = (T)GetObjectOfType(typeof(T));
        if (classInstance == null) {
            Debug.LogError("There is no " + typeof(T).Name + " object in the object list.");
            return null;
        }

        try { return classType.GetType().GetMethod(name).Invoke(classInstance, values); }
        catch {
            Debug.LogError("There was an error when launching the function " + name + " of the class " + typeof(T).Name + ".");
            return null;
        }
    }

    private object GetObjectOfType(Type t) {
        foreach (object o in instancesOfTypeList)
            if (o.GetType() == t)
                return o;
        return null;
    }

    private List<MethodInfo> GetCYFEventFunctions(Type t) {
        List<MethodInfo> methods = new List<MethodInfo>();
        MethodInfo[] methodInfos = t.GetMethods();
        const bool includeInherited = false;
        foreach (MethodInfo mf in methodInfos)
            if (mf.GetCustomAttributes(typeof(CYFEventFunction), includeInherited).Any())
                methods.Add(mf);
        return methods;
    }*/

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
        //try {
            BoxCollider2D boxCollider = GameObject.Find("Player").GetComponent<BoxCollider2D>();
            Transform transform = GameObject.Find("Player").transform;

            //Store start position to move from, based on objects current transform position
            Vector2 start = new Vector2(transform.position.x + transform.localScale.x * boxCollider.offset.x,
                                        transform.position.y + transform.localScale.x * boxCollider.offset.y);

            //Calculate end position based on the direction parameters passed in when calling Move and using our boxCollider
            Vector2 dir = new Vector2(xDir, yDir);

            //Calculate the current size of the object's boxCollider
            Vector2 size = new Vector2(boxCollider.size.x * PlayerOverworld.instance.PlayerPos.localScale.x, boxCollider.size.y * PlayerOverworld.instance.PlayerPos.localScale.y);

            //Disable boxCollider so that linecast doesn't hit this object's own collider and disable the non touching events' colliders
            boxCollider.enabled = false;
            foreach (GameObject go in events) {
                if (getTrigger(go, go.GetComponent<EventOW>().actualPage) > 0 && go.GetComponent<Collider2D>())
                    go.GetComponent<Collider2D>().enabled = false;
            }

            //Cast a box from start point to end point checking collision on blockingLayer
            //hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(xDir * PlayerOverworld.instance.speed, 2) + 
            //                                                        Mathf.Pow(yDir * PlayerOverworld.instance.speed, 2)), EventLayer);          
            hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(boxCollider.size.x * PlayerOverworld.instance.transform.localScale.x * xDir, 2) +
                                                                    Mathf.Pow(boxCollider.size.y * PlayerOverworld.instance.transform.localScale.x * yDir, 2)), EventLayer);

            //Re-enable the disabled colliders after BoxCast
            boxCollider.enabled = true;
            foreach (GameObject go in events)
                if (getTrigger(go, go.GetComponent<EventOW>().actualPage) > 0 && go.GetComponent<Collider2D>())
                    go.GetComponent<Collider2D>().enabled = true;
            
            //Executes the event that our cast collided with
            if (hit.collider == null) {
                UnitaleUtil.writeInLog("No event was hit.");
                return false;
            } else
                return executeEvent(hit.collider.gameObject);
        /*} catch {
            hit = new RaycastHit2D();
            print("error press button event");
            return false;
        }*/
    }

    public bool testEventAuto() {
        GameObject gameobject = null;
        try {
            foreach (GameObject go in events) {
                gameobject = go;
                if (getTrigger(go, go.GetComponent<EventOW>().actualPage) == 2)
                    if (!autoDone.ContainsKey(new object[] { go, go.GetComponent<EventOW>().actualPage })) {
                        print("auto Event");
                        return executeEvent(go, true);
                    }
            }
        } catch (InterpreterException e) {
            UnitaleUtil.displayLuaError(gameobject.name + ", page n°" + gameobject.GetComponent<EventOW>().actualPage, e.DecoratedMessage);
        } catch (Exception e) {
            UnitaleUtil.displayLuaError(gameobject.name + ", page n°" + gameobject.GetComponent<EventOW>().actualPage,
                                        "Unknown error of type " + e.GetType() + ". Please send this to the main dev.\n\n" + e.Message + "\n\n" + e.StackTrace);
        }
        return false;
    }

    public void testEventDestruction() {
        foreach (GameObject go in events)
            if (!go)                                              events.Remove(go);
            else if (!go.GetComponent<EventOW>())                 events.Remove(go);
            else if (go.GetComponent<EventOW>().actualPage == -1) luaevow.Remove(go.name);
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
        //GameObject[] eventsTemp = GameObject.FindGameObjectsWithTag("Event");
        events.Clear();
        autoDone.Clear();
        sprCtrls.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Event")) {
            events.Add(go);
            if (go.GetComponent<SpriteRenderer>())
                sprCtrls[go.name] = new LuaSpriteController(go.GetComponent<SpriteRenderer>(), "empty"); //TODO: Find a way to get the sprite's name
        }
    }

    /// <summary>
    /// Function that executes the event "go"
    /// </summary>
    /// <param name="go"></param>
    [HideInInspector]
    public bool executeEvent(GameObject go, bool auto = false, int page = -1) {
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
            PlayerOverworld.inText = true;  //UnitaleUtil.writeInLogAndDebugger("executeEvent true:" + go.GetComponent<EventOW>().actualPage);
            scriptLaunched = true;
            textmgr.textQueue = new TextMessage[] { };
            try {
                initScript(scriptToLoad, go.GetComponent<EventOW>());
                script.Call("CYFEventStartEvent", DynValue.NewString("EventPage" + (page == -1 ? go.GetComponent<EventOW>().actualPage : page)));
                //script.Call("EventPage" + go.GetComponent<EventOW>().actualPage);
            } catch (InterpreterException ex) {
                UnitaleUtil.displayLuaError(scriptToLoad, ex.DecoratedMessage);
                return false;
            }
            textmgr.setCaller(script);
            textmgr.setTextQueue(textmgr.textQueue);
            passPressOnce = true;
            if (auto)
                autoDone.Add(new object[] { go, go.GetComponent<EventOW>().actualPage }, true);
            return true;
        } else
            UnitaleUtil.writeInLog("There is no script to load!");
        return false;
    }

    private List<string> CreateBindListMember(Type t) {
        List<string> result = new List<string>();
        MethodInfo[] methods = t.GetMethods();
        foreach (MethodInfo method in methods)
            if (MethodHasCYFEventFunctionAttribute(method))
                result.Add(method.Name);
        return result;
    }

    /// <summary>
    /// Checks if the expression given has the CYFEventFunction attribute I guess
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private bool MethodHasCYFEventFunctionAttribute(MemberInfo mb) {
        const bool includeInherited = false;
        return mb.GetCustomAttributes(typeof(CYFEventFunction), includeInherited).Any();
    }

    /// <summary>
    /// Function that permits to initialize the event script to be used later
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Returns true if no error were encountered.</returns>
    private bool initScript(string name, EventOW ev) {
        script = new ScriptWrapper(/*true*/);
        script.scriptname = name;
        string scriptText = ScriptRegistry.Get(ScriptRegistry.EVENT_PREFIX + name);
        if (scriptText == null) {
            UnitaleUtil.displayLuaError("Launching an event", "The event " + name + " doesn't exist.");
            return false;
        }
        string lameOverworldFunctionBinding = string.Empty;
        string lameFunctionBinding = string.Empty;
        bool once = false;
        //foreach (string str in bindList) {
        foreach (Type t in boundValueName.Keys) {
            List<string> members = CreateBindListMember(t);
            scriptText += "\n" + boundValueName[t] + " = { }";
            foreach (string member in members) {
                //script.Bind(str, new Action<DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue, DynValue>(OldFunctionLauncher));
                string completeMember = boundValueName[t] + "." + member;
                scriptText += "\n" /*+ "\nfunction F" + str2 + "() end\n"*/ + "function " + completeMember + "(p1,p2,p3,p4,p5,p6,p7,p8,p9,p10) return CYFEventForwarder(\"" + completeMember + "\",p1,p2,p3,p4,p5,p6,p7,p8,p9,p10) end";
                lameFunctionBinding += "\n    " + (once ? "elseif" : "if") + " func == '" + completeMember + "' then x = F" + completeMember;
                once = true;
            }
        }
        once = false;
        foreach (Vector2 v in ev.eventTriggers) {
            lameOverworldFunctionBinding += "\n    " + (once ? "elseif" : "if") + " func == 'EventPage" + v.x + "' then x = EventPage" + v.x;
            once = true;
        }
        lameOverworldFunctionBinding += "\n    end";
        lameFunctionBinding += "\n    end";
        scriptText += "\n\nCYFEventCoroutine = coroutine.create(DEBUG) " +
                      "\nCYFEventLameErrorContainer = nil" + 
                      "\nlocal CYFEventAlreadyLaunched = false " +
                      "\nfunction CYFEventFuncToLaunch(x)" +
                      "\n    err = nil" +
                      "\n    xpcall(x, " + 
                      "\n        function()" +
                      "\n            err = debug.traceback()" +
                      "\n        end) " + 
                      "\n    if err != nil then error(err) end" +
                      "\nend " +
                      "\nfunction CYFEventNextCommand() " +
                      "\n    CYFEventAlreadyLaunched = true " +
                      "\n    if tostring(coroutine.status(CYFEventCoroutine)) == 'suspended' then " +
                      "\n        local ok, errorMsg = coroutine.resume(CYFEventCoroutine) " +
                      "\n        if CYFEventLameErrorContainer != nil then error(CYFEventLameErrorContainer) end " +
                      "\n    end " +
                      "\nend " +
                      "\nfunction err (x)" +
                      "\n    CYFEventLameEroorContainer = x" +
                      "\nend" +
                      "\nfunction CYFEventStopCommand() coroutine.yield() end " +
                      "\nfunction CYFEventStartEvent(func) " +
                      "\n    local x = 'error' " +
                      //"\n    DEBUG(assert(loadstring('return '..x..'(...)'))(25)) " +
                      lameOverworldFunctionBinding +
                      "\n    if x == error then error(\"Don't look at your script, this code block is added at the script's compilation.\\n" + 
                                                       "The overworld function \\\"\" .. func .. \"\\\" doesn't exist. Did you forgot to add this function in the event triggers list?\") end " +
                      "\n    CYFEventCoroutine = coroutine.create(function() return xpcall(x,function(err) CYFEventLameErrorContainer = err end) end)" +
                      "\n    local ok, errorMsg = coroutine.resume(CYFEventCoroutine) " +
                      "\n    if CYFEventLameErrorContainer != nil then error(CYFEventLameErrorContainer) end " +
                      "\nend " +
                      "\nfunction CYFEventForwarder(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) " +
                      "\n    CYFEventAlreadyLaunched = false" + 
                      "\n    local x " +
                      lameFunctionBinding +
                      "\n    local result " +
                      "\n    if     arg1 == nil  then  result = x() " +
                      "\n    elseif arg2 == nil  then  result = x(arg1) " +
                      "\n    elseif arg3 == nil  then  result = x(arg1, arg2) " +
                      "\n    elseif arg4 == nil  then  result = x(arg1, arg2, arg3) " +
                      "\n    elseif arg5 == nil  then  result = x(arg1, arg2, arg3, arg4) " +
                      "\n    elseif arg6 == nil  then  result = x(arg1, arg2, arg3, arg4, arg5) " +
                      "\n    elseif arg7 == nil  then  result = x(arg1, arg2, arg3, arg4, arg5, arg6) " +
                      "\n    elseif arg8 == nil  then  result = x(arg1, arg2, arg3, arg4, arg5, arg6, arg7) " +
                      "\n    elseif arg9 == nil  then  result = x(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) " +
                      "\n    elseif arg10 == nil then  result = x(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) " +
                      "\n    else                      result = x(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) " +
                      "\n    end " +
                      "\n    if not CYFEventAlreadyLaunched then coroutine.yield() end" +
                      "\n    return result" +
                      "\nend";
        script.text = scriptText;
        Debug.Log(scriptText);

        try { script.DoString(scriptText); } 
        catch (InterpreterException ex) {
            UnitaleUtil.displayLuaError(name, ex.DecoratedMessage);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Only used for identification
    /// </summary>
    public void FunctionLauncher(DynValue parameter1, DynValue parameter2, DynValue parameter3, DynValue parameter4, DynValue parameter5, 
                                 DynValue parameter6, DynValue parameter7, DynValue parameter8, DynValue parameter9, DynValue parameter10) { }

    /*/// <summary>
    /// Used to add an element into the Text Event
    /// </summary>
    /// <param name="function">Name of the function</param>
    /// <param name="parameters">Parameters of the function</param>
    public void OldFunctionLauncher(string function, DynValue[] parameters = null) {
        //Nobody cares about spaces, so let's just remove them from the function's name
        function = function.TrimStart('"').TrimEnd('"').Replace(" ", string.Empty);

        Type thisType = this.GetType();
        MethodInfo theMethod = thisType.GetMethod(function);
        theMethod.Invoke(this, parameters);

       if (function == "SetChoice") {
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
                
            for (int i = 0; i < parameters.Length; i ++) {
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
        }
        textmgr.ResetCurrentCharacter();
    }*/

    /// <summary>
    /// Used in SetChoice, permits to set the temporary GameObject we created to stars' positions
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="question"></param>
    public void setPlayerOnSelection(int selection, bool question = false, bool threeLines = false) {
        if (question) {
            if (threeLines)  selection += 2;
            else             selection += 4;
        }
        Vector2 upperLeft = new Vector2(61 + Camera.main.transform.position.x - 320, 
                                        GameObject.Find("letter(Clone)").GetComponent<RectTransform>().position.y + (GameObject.Find("letter(Clone)").GetComponent<RectTransform>().sizeDelta.y / 2) - 1);
        int xMv = selection % 2; // remainder safe again, selection is never negative
        int yMv = selection / 2;
        // HACK: remove hardcoding of this sometime, ever... probably not happening lmao
        GameObject.Find("tempHeart").GetComponent<RectTransform>().position = new Vector2(upperLeft.x + xMv * 303, upperLeft.y - yMv * textmgr.Charset.LineSpacing);
    }

    /// <summary>
    /// Enter a vector, returns the direction of the vector accordingly to computer's numeric pads (yeah, how great for those who don't have one)
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public int CheckDirection(Vector2 dir) {
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
        EventManager.instance.sprCtrls.Clear();

        if (!GlobalControls.MapEventPages.ContainsKey(id))
            GlobalControls.MapEventPages.Add(id, new Dictionary<string, int>());

        foreach (EventOW ev in events) {
            if (ev.name.Contains("Image") || ev.name.Contains("Tone"))
                continue;
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
    public void endEvent() {
        try {
            foreach (string key in LuaScriptBinder.GetDictionary().Keys)
                if (key.Contains("Choice") || key.Contains("ReturnPoint") || key.Contains("GetSpriteOfEvent"))
                    LuaScriptBinder.Remove(key);
        } catch { }
        PlayerOverworld.instance.textmgr.setTextFrameAlpha(0);
        PlayerOverworld.instance.textmgr.textQueue = new TextMessage[] { };
        PlayerOverworld.instance.textmgr.destroyText();
        PlayerOverworld.inText = false;
        scriptLaunched = false;
        //StartCoroutine("ISetChoice", "1", "2");
    }

    public void StCoroutine(string name, object args) {
        if (args == null)                 StartCoroutine(name);
        else if (!args.GetType().IsArray) StartCoroutine(name, args);
        else                              StartCoroutine(name, (object[])args);
    }

    //-----------------------------------------------------------------------------------------------------------
    //                                        ---   Lua Functions   ---
    //
    //                All event commands have to be finished with script.Call("CYFEventNextCommand");
    //                If you need to return a value to the lua script, try to use try ... finally! :P
    //
    //                    Plus, if you want to create functions, test first if the GameObject the
    //                     player is accessing to is an event: if you don't, you'll be a really
    //                             bad person and you'll go to hell. Don't ask why tho.
    //-----------------------------------------------------------------------------------------------------------

    IEnumerator ISetChoice(object[] args) {
        //Real args
        bool question, threeLines;
        try { question = (bool)args[0]; }   catch { throw new CYFException("The argument \"question\" must be a boolean."); }
        try { threeLines = (bool)args[1]; } catch { throw new CYFException("The argument \"threeLines\" must be a boolean."); }

        yield return 0;
        //Omg a new GameObject! One more heart on the screen! Wooh!
        GameObject tempHeart = new GameObject("tempHeart", typeof(RectTransform));
        tempHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(16, 16);
        tempHeart.transform.SetParent(GameObject.Find("Canvas OW").transform);
        Image img = tempHeart.AddComponent<Image>();
        img.sprite = PlayerOverworld.instance.utHeart.sprite;
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
        script.script.Globals.Set(DynValue.NewString("lastChoice"), DynValue.NewNumber(actualChoice));
        //HEARTBROKEN
        Destroy(tempHeart);
        script.Call("CYFEventNextCommand");
    }

    IEnumerator IMoveEventToPoint(object[] args) {
        //Real args
        string name;
        float dirX, dirY;
        bool wallPass;
        try { name = (string)args[0]; }   catch { throw new CYFException("The argument \"name\" must be a string."); }
        try { dirX = (float)args[1]; }    catch { throw new CYFException("The argument \"dirX\" must be a number."); }
        try { dirY = (float)args[2]; }    catch { throw new CYFException("The argument \"dirY\" must be a number."); }
        try { wallPass = (bool)args[3]; } catch { throw new CYFException("The argument \"wallPass\" must be a boolean."); }

        //This function moves the player : this boolean is used to disable encounter generation during this event
        PlayerOverworld.instance.forcedMove = true;
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
                        if (go2.name != "Background")
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
                            test = PlayerOverworld.instance.AttemptMove(clamped.x, clamped.y, go, wallPass);
                        //If we reached the destination, stop the function
                        else {
                            test = PlayerOverworld.instance.AttemptMove(endPointFromNow.x, endPointFromNow.y, go, wallPass);
                            PlayerOverworld.instance.forcedMove = false;
                            if (wallPass)
                                foreach (GameObject go2 in colliders)
                                    if (go2.name != "Background")
                                        go2.SetActive(true);
                            go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                            script.Call("CYFEventNextCommand");
                            yield break;
                        }
                    } else {
                        if (PlayerOverworld.instance.speed < endPointFromNow.magnitude)
                            test = PlayerOverworld.instance.AttemptMove(clamped.x, clamped.y, go, wallPass);
                        //If we reached the destination, stop the function
                        else {
                            test = PlayerOverworld.instance.AttemptMove(endPointFromNow.x, endPointFromNow.y, go, wallPass);
                            PlayerOverworld.instance.forcedMove = false;
                            if (wallPass)
                                foreach (GameObject go2 in colliders)
                                    go2.SetActive(true);
                            go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                            script.Call("CYFEventNextCommand");
                            yield break;
                        }
                    }
                    yield return 0;

                    if (!test && !wallPass) {
                        PlayerOverworld.instance.forcedMove = false;
                        go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                        script.Call("CYFEventNextCommand");
                        yield break;
                    } else {
                        //The animations are here !
                        switch (direction) {
                            case 2: go.GetComponent<Animator>().SetTrigger("MovingDown"); break;
                            case 4: go.GetComponent<Animator>().SetTrigger("MovingLeft"); break;
                            case 6: go.GetComponent<Animator>().SetTrigger("MovingRight"); break;
                            case 8: go.GetComponent<Animator>().SetTrigger("MovingUp"); break;
                        }
                    }
                    endPointFromNow = new Vector2(dirX - go.transform.position.x, dirY - go.transform.position.y);
                }
                go.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function doesn't exists. Did you forget to add the 'Event' tag ?");
        PlayerOverworld.instance.forcedMove = false;
        script.Call("CYFEventNextCommand");
    }

    IEnumerator IWaitForInput() {
        while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED)
            yield return 0;

        script.Call("CYFEventNextCommand");
    }

    IEnumerator ISetTone(object[] args) {
        //REAL ARGS
        bool waitEnd;
        int r = 255, g = 255, b = 255, a = 0;
        try { waitEnd = (bool)args[0]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }
        if (args.Length >= 2)
            try { r = (int)args[1];    } catch { throw new CYFException("The argument \"r\" must be a number."); }
        if (args.Length >= 3)
            try { g = (int)args[2];    } catch { throw new CYFException("The argument \"g\" must be a number."); }
        if (args.Length >= 4)
            try { b = (int)args[3];    } catch { throw new CYFException("The argument \"b\" must be a number."); }
        if (args.Length >= 5)
            try { a = (int)args[4];    } catch { throw new CYFException("The argument \"a\" must be a number."); }

        int alpha, lack;
        float beginHighest = 0, highest = 0;
        int[] currents, lacks, beginLacks;
        float[] realLacks;
        if (GameObject.Find("Tone") == null) {
            GameObject image = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ImageEvent"));
            image.GetComponent<Image>().color = new Color(image.GetComponent<Image>().color.r, image.GetComponent<Image>().color.g, image.GetComponent<Image>().color.b, 0);
            alpha = 0; lack = a;
            image.name = "Tone";
            image.tag = "Event";
            image.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas OW").transform);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
            image.GetComponent<RectTransform>().position = new Vector2(320, 240);
            events.Add(image);
            currents = new int[] { 0, 0, 0, 0 };
            lacks = new int[] { r, g, b, a };
            if (!waitEnd)
                script.Call("CYFEventNextCommand");
        } else {
            Color c = GameObject.Find("Tone").GetComponent<Image>().color;
            currents = new int[] { (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), (int)(c.a * 255) };
            lacks = new int[] { r - currents[0], g - currents[1], b - currents[2], a - currents[3] };
            alpha = (int)(c.a * 255);
            lack = a - alpha;
        }
        beginLacks = lacks;
        realLacks = new float[] { lacks[0], lacks[1], lacks[2], lacks[3] };
        foreach (int i in lacks)
            highest = Mathf.Abs(i) > highest ? Mathf.Abs(i) : highest;
        beginHighest = highest;
        while (GameObject.Find("Tone").GetComponent<Image>().color != new Color32((byte)r, (byte)g, (byte)b, (byte)a)) {
            for (int i = 0; i < realLacks.Length; i++)
                realLacks[i] -= beginLacks[i] * 4 / beginHighest;
            if (highest <= 4)
                GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            else {
                GameObject.Find("Tone").GetComponent<Image>().color = new Color32((byte)(r - Mathf.RoundToInt(realLacks[0])), (byte)(g - Mathf.RoundToInt(realLacks[1])),
                                                                                  (byte)(b - Mathf.RoundToInt(realLacks[2])), (byte)(a - Mathf.RoundToInt(realLacks[3])));
            }
            highest -= 4;
            yield return 0;
        }
        
        if (a == 0)
            EventManager.instance.luaevow.Remove("Tone");
        if (waitEnd)
            script.Call("CYFEventNextCommand");
    }

    IEnumerator IRotateEvent(object[] args) {
        //REAL ARGS
        string name;
        float rotateX, rotateY, rotateZ;
        try { name = (string)args[0];   } catch { throw new CYFException("The argument \"name\" must be a string."); }
        try { rotateX = (float)args[1]; } catch { throw new CYFException("The argument \"rotateX\" must be a number."); }
        try { rotateY = (float)args[2]; } catch { throw new CYFException("The argument \"rotateY\" must be a number."); }
        try { rotateZ = (float)args[3]; } catch { throw new CYFException("The argument \"rotateZ\" must be a number."); }

        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = events[i];
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");

                float lackX = rotateX - go.transform.rotation.eulerAngles.x;
                float lackY = rotateY - go.transform.rotation.eulerAngles.y;
                float lackZ = rotateZ - go.transform.rotation.eulerAngles.z;

                float best, basisBest;
                if (Mathf.Abs(lackX) > Mathf.Abs(lackY)) best = lackX;
                else best = lackY;
                if (Mathf.Abs(best) < Mathf.Abs(lackZ)) best = lackZ;

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
                    else best += 4;
                    yield return 0;
                }
                go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                script.Call("CYFEventNextCommand");
                yield break;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
        script.Call("CYFEventNextCommand");
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

    IEnumerator IRumble(object[] args) {
        //REAL ARGS
        float seconds, intensity;
        bool fade;
        try { seconds = (float)args[0];   } catch { throw new CYFException("The argument \"seconds\" must be a number."); }
        try { intensity = (float)args[1]; } catch { throw new CYFException("The argument \"intensity\" must be a number."); }
        try { fade = (bool)args[2];       } catch { throw new CYFException("The argument \"fade\" must be a boolean."); }

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
        script.Call("CYFEventNextCommand");
    }

    IEnumerator IFlash(object[] args) {
        //REAL ARGS
        float secondsOrFrames;
        bool isSeconds;
        int colorR, colorG, colorB, colorA;
        try { secondsOrFrames = (float)args[0]; } catch { throw new CYFException("The argument \"secondsOrFrames\" must be a number."); }
        try { isSeconds = (bool)args[1];        } catch { throw new CYFException("The argument \"isSeconds\" must be a boolean."); }
        try { colorR = (int)args[2];            } catch { throw new CYFException("The argument \"colorR\" must be a number."); }
        try { colorG = (int)args[3];            } catch { throw new CYFException("The argument \"colorG\" must be a number."); }
        try { colorB = (int)args[4];            } catch { throw new CYFException("The argument \"colorB\" must be a number."); }
        try { colorA = (int)args[5];            } catch { throw new CYFException("The argument \"colorA\" must be a number."); }

        GameObject flash = new GameObject("flash", new Type[] { typeof(Image) });
        flash.transform.SetParent(Camera.main.transform);
        flash.transform.position = Camera.main.transform.position;
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
            for (int frame = 0; frame < secondsOrFrames; frame++) {
                if (frame != 0)
                    flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)(colorA - colorA * frame / secondsOrFrames));
                yield return 0;

            }
        script.Call("CYFEventNextCommand");
    }

    IEnumerator ISave() {
        bool save = true;
        Color c = PlayerOverworld.instance.utHeart.color;
        PlayerOverworld.instance.utHeart.transform.position = new Vector3(151 + Camera.main.transform.position.x - 320, 233 + Camera.main.transform.position.y - 240,
                                                                          PlayerOverworld.instance.utHeart.transform.position.z);
        PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 1);
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

        GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        yield return 0;
        while (true) {
            if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
                if (!save)
                    PlayerOverworld.instance.utHeart.transform.position = new Vector3(151 + Camera.main.transform.position.x - 320, PlayerOverworld.instance.utHeart.transform.position.y,
                                                                                      PlayerOverworld.instance.utHeart.transform.position.z);
                else
                    PlayerOverworld.instance.utHeart.transform.position = new Vector3(331 + Camera.main.transform.position.x - 320, PlayerOverworld.instance.utHeart.transform.position.y,
                                                                                      PlayerOverworld.instance.utHeart.transform.position.z);
                save = !save;
            } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
                PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 0);
                txtName.destroyText(); txtLevel.destroyText(); txtTime.destroyText(); txtMap.destroyText(); txtSave.destroyText(); txtReturn.destroyText();
                GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                script.Call("CYFEventNextCommand");
                yield break;
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                if (save) {
                    SaveLoad.Save();
                    PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 0);
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
                    GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    do {
                        passPressOnce = true;
                        yield return 0;
                    } while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED);
                }
                PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 0);
                txtName.destroyText(); txtLevel.destroyText(); txtTime.destroyText(); txtMap.destroyText(); txtSave.destroyText(); txtReturn.destroyText();
                GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                script.Call("CYFEventNextCommand");
                yield break;
            }
            yield return 0;
        }
    }

    public IEnumerator IMoveCamera(object[] args) {
        //REAL ARGS
        int pixX, pixY, speed;
        bool straightLine;
        try { pixX = (int)args[0];          } catch { throw new CYFException("The argument \"pixX\" must be a number."); }
        try { pixY = (int)args[1];          } catch { throw new CYFException("The argument \"pixY\" must be a number."); }
        try { speed = (int)args[2];         } catch { throw new CYFException("The argument \"speed\" must be a number."); }
        try { straightLine = (bool)args[3]; } catch { throw new CYFException("The argument \"straightLine\" must be a boolean."); }

        if (speed <= 0) {
            UnitaleUtil.displayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "The speed of the camera has to be strictly positive.");
            /*textmgr.skipNowIfBlocked = true;*/
            yield break;
        }
        float currentX = PlayerOverworld.instance.cameraShift.x, currentY = PlayerOverworld.instance.cameraShift.y,
              xSpeed = currentX > pixX ? -speed : speed, ySpeed = currentY > pixY ? -speed : speed;
        if (straightLine) {
            Vector2 clamped = Vector2.ClampMagnitude(new Vector2(pixX - currentX, pixY - currentY), speed);
            xSpeed = clamped.x;
            ySpeed = clamped.y;
        }
        while (currentX != pixX || currentY != pixY) {
            if (currentX != pixX)
                if (Mathf.Abs(xSpeed) < Mathf.Abs(pixX - currentX)) {
                    currentX += xSpeed;
                    PlayerOverworld.instance.cameraShift.x += xSpeed;
                } else {
                    currentX = pixX;
                    PlayerOverworld.instance.cameraShift.x = pixX;
                }
                
            if (currentY != pixY)
                if (Mathf.Abs(ySpeed) < Mathf.Abs(pixY - currentY)) {
                    currentY += ySpeed;
                    PlayerOverworld.instance.cameraShift.y += ySpeed;
                } else {
                    currentY = pixY;
                    PlayerOverworld.instance.cameraShift.y = pixY;
                }
            yield return 0;
        }
        script.Call("CYFEventNextCommand");
    }

    public IEnumerator IWait(int frames) {
        int curr = 0;
        while (curr != frames) {
            curr++;
            yield return 0;
        }

        script.Call("CYFEventNextCommand");
    }
}