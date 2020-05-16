using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using System.Reflection;

public class EventManager : MonoBehaviour {
    public  static EventManager instance;
    private int EventLayer;                 //The id of the Event Layer
    public  List<string> Page0Done = new List<string>();
    public  ScriptWrapper script;           //The script we have to load
    public  Dictionary<GameObject, ScriptWrapper> eventScripts = new Dictionary<GameObject, ScriptWrapper>();
    public  Dictionary<string, UnityEngine.Coroutine> cSharpCoroutines = new Dictionary<string, UnityEngine.Coroutine>();
    public  Dictionary<ScriptWrapper, int> coroutines = new Dictionary<ScriptWrapper, int>();
    public  List<GameObject> events = new List<GameObject>(); //This map's events
    public  Dictionary<string, LuaSpriteController> sprCtrls = new Dictionary<string, LuaSpriteController>();
    private TextManager textmgr;            //The current TextManager
    public  int actualEventIndex = -1;      //ID of the actual event we're running
    private static string eventCodeFirst;   //The internal Lua code loaded into event scripts (first part)
    private static string eventCodeLast;    //The internal Lua code loaded into event scripts (last part)
    public  bool readyToReLaunch = false;   //Used to prevent overworld GameOver errors
    public  bool bgmCoroutine = false;      //Check if the BGM is already fading
    public  bool passPressOnce = false;     //Boolean used because events are boring
    public  bool _scriptLaunched = false;
    public  bool ScriptLaunched {
        get { return _scriptLaunched || PlayerOverworld.instance.forceNoAction; }
        set { _scriptLaunched = value; }
    }
    public  bool onceReload = false;
    public  bool nextFadeTransition = true;
    public  bool LoadLaunched = false;
    public  static bool inited = false;
    public  Dictionary<GameObject, bool> autoDone = new Dictionary<GameObject, bool>();

    public LuaPlayerOW luaplow;
    public LuaEventOW luaevow;
    public LuaGeneralOW luagenow;
    public LuaInventoryOW luainvow;
    public LuaScreenOW luascrow;
    public LuaMapOW luamapow;

    private Dictionary<Type, string> boundValueName = new Dictionary<Type, string>();

    // Use this for initialization
    public void LateStart() {
        EventLayer = LayerMask.GetMask("EventLayer");                               //Get the layer that'll interact with our object, here EventLayer
        textmgr = GameObject.Find("TextManager OW").GetComponent<TextManager>();
        if (boundValueName.Count == 0) {
            boundValueName.Add(typeof(LuaEventOW), "Event");
            boundValueName.Add(typeof(LuaPlayerOW), "Player");
            boundValueName.Add(typeof(LuaGeneralOW), "General");
            boundValueName.Add(typeof(LuaInventoryOW), "Inventory");
            boundValueName.Add(typeof(LuaScreenOW), "Screen");
            boundValueName.Add(typeof(LuaMapOW), "Map");
            luaplow = new LuaPlayerOW();
            luaevow = new LuaEventOW();
            luagenow = new LuaGeneralOW(textmgr);
            luainvow = new LuaInventoryOW();
            luascrow = new LuaScreenOW();
            luamapow = new LuaMapOW();
        }
        //waitForReload = true;
        instance = this;
    }

    void OnEnable() {
        LuaEventOW.StCoroutine += StCoroutine;
        LuaPlayerOW.StCoroutine += StCoroutine;
        LuaGeneralOW.StCoroutine += StCoroutine;
        LuaInventoryOW.StCoroutine += StCoroutine;
        LuaScreenOW.StCoroutine += StCoroutine;
        LuaMapOW.StCoroutine += StCoroutine;
        StaticInits.Loaded += AfterLoad;
    }
    void OnDisable() {
        LuaEventOW.StCoroutine -= StCoroutine;
        LuaPlayerOW.StCoroutine -= StCoroutine;
        LuaGeneralOW.StCoroutine -= StCoroutine;
        LuaInventoryOW.StCoroutine -= StCoroutine;
        LuaScreenOW.StCoroutine -= StCoroutine;
        LuaMapOW.StCoroutine -= StCoroutine;
        StaticInits.Loaded -= AfterLoad;
    }

    public void AfterLoad() {
        LoadLaunched = true;
        if (script == null) {
            if (!onceReload) {
                onceReload = true;
                ResetEvents();
                TestEventDestruction();
            }
            try { PlayerOverworld.instance.utHeart = GameObject.Find("utHeart").GetComponent<Image>(); }
            catch { return; }
            for (int i = 0; i < events.Count; i++)
                if (events[i] != null) {
                    if (TestContainsListVector2(events[i].GetComponent<EventOW>().eventTriggers, 0) && !Page0Done.Contains(events[i].name)) {
                        Page0Done.Add(events[i].name);
                        ExecuteEvent(events[i], 0);
                        return;
                    }
                }
            if (inited || events.Count != 0)
                if (nextFadeTransition)
                    GameObject.FindObjectOfType<Fading>().BeginFade(-1);
                else
                    GameObject.FindObjectOfType<Fading>().FadeInstant(-1, true);
            inited = true;
            nextFadeTransition = true;
            LoadLaunched = false;
        } else
            CheckEndEvent();
    }

    void CheckEndEvent() {
        if (script != null) {
            Table t = script.script.Globals;
            //print((t.Get(DynValue.NewString("CYFEventCoroutine")).Coroutine.State == CoroutineState.Dead) + " && " + (GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0));
            if (t.Get(DynValue.NewString("CYFEventCoroutine")).Coroutine.State == CoroutineState.Dead && GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0)
                EndEvent();
        }
    }

    public void CheckCurrentEvent() {
        for (int count = 0; count < coroutines.Count; count++) {
            ScriptWrapper scr = coroutines.ElementAt(count).Key;
            Table t = scr.script.Globals;
            if (t.Get(DynValue.NewString("CYFEventCheckRefresh")).Boolean) {
                SetCurrentScript(scr);
                t.Set(DynValue.NewString("CYFEventCheckRefresh"), DynValue.NewBoolean(false));
            }
        }
        if (script != null) {
            Table t2 = script.script.Globals;
            if (t2.Get(DynValue.NewString("CYFEventCheckRefresh")).Boolean) {
                SetCurrentScript(script);
                t2.Set(DynValue.NewString("CYFEventCheckRefresh"), DynValue.NewBoolean(false));
            }
        }
    }

    void SetCurrentScript(ScriptWrapper scr) {
        luaevow.appliedScript = scr;
        luagenow.appliedScript = scr;
        luainvow.appliedScript = scr;
        luaplow.appliedScript = scr;
        luascrow.appliedScript = scr;
        luamapow.appliedScript = scr;
    }

    // Update is called once per frame
    void Update() {
        try {
            if (readyToReLaunch && SceneManager.GetActiveScene().name != "TransitionOverworld") {
                readyToReLaunch = false;
                LateStart();
            }
            if (LoadLaunched) {
                AfterLoad();
                return;
            }
            CheckCurrentEvent();
            TestEventDestruction();
            RunCoroutines();
            if (script == null && !ScriptLaunched && !PlayerOverworld.instance.inBattleAnim && !PlayerOverworld.instance.menuRunning[2]) {
                if (TestEventAuto()) return;
                if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && !passPressOnce && (GameObject.Find("FadingBlack") == null || GameObject.Find("FadingBlack").GetComponent<Fading>().alpha <= 0)) {
                    RaycastHit2D hit;
                    TestEventPress(PlayerOverworld.instance.lastMove.x, PlayerOverworld.instance.lastMove.y, out hit);
                } else
                    passPressOnce = false;

                if (events.Count != 0)
                    for (int i = 0; i < events.Count; i ++) {
                        GameObject go = events[i];
                        EventOW ev = go.GetComponent<EventOW>();
                        if (ev.actualPage < -1) { }
                        else if (ev.actualPage == -1) {
                            events.Remove(go);
                            i--;
                            Destroy(go);
                        } else if (!TestContainsListVector2(ev.eventTriggers, ev.actualPage) && ev.eventTriggers.Count != 0) {
                            UnitaleUtil.DisplayLuaError(ev.name, "The trigger of the page #" + ev.actualPage + " doesn't exist.\nYou'll need to add it via Unity, on this event's EventOW Component.");
                            return;
                        }
                    }
            } else
                passPressOnce = false;
            CheckEndEvent();
        } catch (InvalidOperationException e) { Debug.LogError(e.Message); }
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

    public static bool TestContainsListVector2(List<Vector2> list, int testValue) {
        foreach (Vector2 v in list)
            if (v.x == testValue)
                return true;
        return false;
    }

    // TODO: Fix the array problem, forcing arrays to be printed "(Table)" in the text.
    // WHY: WAY less arguments on few functions like DispImg
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
    public bool TestEventPress(float xDir, float yDir, out RaycastHit2D hit) {
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
            if (GetTrigger(go, go.GetComponent<EventOW>().actualPage) > 0 && go.GetComponent<Collider2D>())
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
            if (GetTrigger(go, go.GetComponent<EventOW>().actualPage) > 0 && go.GetComponent<Collider2D>())
                go.GetComponent<Collider2D>().enabled = true;

        //Executes the event that our cast collided with
        if (hit.collider == null)
            return false;
        else
            return ExecuteEvent(hit.collider.gameObject);
        /*} catch {
            hit = new RaycastHit2D();
            print("error press button event");
            return false;
        }*/
    }

    public bool TestEventAuto() {
        GameObject gameobject = null;
        try {
            foreach (GameObject go in events) {
                gameobject = go;
                if (GetTrigger(go, go.GetComponent<EventOW>().actualPage) == 2)
                    if (!autoDone.ContainsKey(go)) {
                        autoDone.Add(go, true);
                        return ExecuteEvent(go);
                    }
            }
        } catch (InterpreterException e) {
            UnitaleUtil.DisplayLuaError(gameobject.name + ", page #" + gameobject.GetComponent<EventOW>().actualPage, UnitaleUtil.FormatErrorSource(e.DecoratedMessage, e.Message) + e.Message);
        } catch (Exception e) {
            UnitaleUtil.DisplayLuaError(gameobject.name + ", page #" + gameobject.GetComponent<EventOW>().actualPage,
                                        "Unknown error of type " + e.GetType() + ". Please send this to the main dev.\n\n" + e.Message + "\n\n" + e.StackTrace);
        }
        return false;
    }

    public void TestEventDestruction() {
        for (int i = 0; i < events.Count; i++) {
            GameObject go = events[i];
            if (!go)                                              events.Remove(go);
            else if (!go.GetComponent<EventOW>())                 events.Remove(go);
            else if (go.GetComponent<EventOW>().actualPage == -1) luaevow.Remove(go.name);
            else                                                  i ++;
            i --;
        }
    }

    private void RunCoroutines() {
        GameObject gameobject = null;
        try {
            try {
                for (int count = 0; count < coroutines.Count; count++) {
                    ScriptWrapper scr = coroutines.ElementAt(count).Key;
                    if (scr == script)
                        continue;
                    GameObject go = eventScripts.FirstOrDefault(x => x.Value == scr).Key;
                    gameobject = go;
                    ExecuteEvent(go, coroutines[scr], true);
                }
            } catch (Exception e) { Debug.LogError(e.Message); }
            for (int i = 0; i < events.Count; i++) {
                if (GetTrigger(events[i], events[i].GetComponent<EventOW>().actualPage) == 3 && !coroutines.ContainsKey(eventScripts[events[i]]) && eventScripts[events[i]] != script) {
                    gameobject = events[i];
                    ExecuteEvent(events[i], -1, true);
                }
            }
        } catch (InterpreterException e) { UnitaleUtil.DisplayLuaError(gameobject.name + ", page #" + gameobject.GetComponent<EventOW>().actualPage, e.DecoratedMessage); }
        catch (Exception e) {
            UnitaleUtil.DisplayLuaError(gameobject.name + ", page #" + gameobject.GetComponent<EventOW>().actualPage,
                                        "Unknown error of type " + e.GetType() + ". Please send this to the main dev.\n\n" + e.Message + "\n\n" + e.StackTrace);
        }
    }

    public int GetTrigger(GameObject go, int index) {
        foreach (Vector2 vec in go.GetComponent<EventOW>().eventTriggers)
            if (vec.x == index)
                return (int)vec.y;
        return -2;
    }

    /// <summary>
    /// Resets the events by counting them all again, stopping the current event and destroying all the current images
    /// </summary>
    public void ResetEvents(bool resetScripts = true) {
        //GameObject[] eventsTemp = GameObject.FindGameObjectsWithTag("Event");
        coroutines.Clear();
        Page0Done.Clear();
        events.Clear();
        autoDone.Clear();
        sprCtrls.Clear();
        if (resetScripts) {
            eventScripts.Clear();
            ScriptWrapper.instances.Clear();
            LuaScriptBinder.scriptlist.Clear();
        }
        PlayerOverworld.instance.parallaxes.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Event")) {
            events.Add(go);
            if (go.GetComponent<BoxCollider2D>()) {
                if (go.GetComponent<BoxCollider2D>().size == new Vector2(0, 0))
                    go.GetComponent<BoxCollider2D>().size = new Vector2(go.GetComponent<RectTransform>().sizeDelta.x / go.GetComponent<RectTransform>().localScale.x * 100,
                                                                        go.GetComponent<RectTransform>().sizeDelta.y / go.GetComponent<RectTransform>().localScale.y * 100);
                if (go.GetComponent<BoxCollider2D>().offset == new Vector2(-1, -1))
                    go.GetComponent<BoxCollider2D>().offset = new Vector2(0, go.GetComponent<BoxCollider2D>().size.y / 2);
            }
            if (go.GetComponent<SpriteRenderer>()) {
                if (go.name == "Player")
                    sprCtrls[go.name] = PlayerOverworld.instance.sprctrl;
                else
                    sprCtrls[go.name] = new LuaSpriteController(go.GetComponent<SpriteRenderer>());
            } else if (go.GetComponent<Image>())
                sprCtrls[go.name] = new LuaSpriteController(go.GetComponent<Image>());

            if (resetScripts)
                if (go.GetComponent<EventOW>().scriptToLoad != "none") {
                    string scriptToLoad = go.GetComponent<EventOW>().scriptToLoad;
                    eventScripts.Add(go, InitScript(scriptToLoad, go.GetComponent<EventOW>()));
                }

            if (go.GetComponent<CYFAnimator>())
                go.GetComponent<CYFAnimator>().LateStart();
        }
        foreach (Transform t in UnitaleUtil.GetFirstChildren(null)) {
            if (!t)
                continue;
            if (!t.gameObject)
                continue;
            if (t.gameObject.name.Contains("Parallax"))
                PlayerOverworld.instance.parallaxes.Add(t);
        }
    }

    /// <summary>
    /// Function that executes the event "go"
    /// </summary>
    /// <param name="go"></param>
    [HideInInspector]
    public bool ExecuteEvent(GameObject go, int page = -1, bool isCoroutine = false) {
        if (script != null && !isCoroutine)
            return false;
        int actualEventIndex = -1;
        for (int i = 0; i < events.Count; i++)
            if (events[i].Equals(go)) {
                actualEventIndex = i;
                break;
            }
        if (actualEventIndex == -1) {
            if (!isCoroutine)
                UnitaleUtil.DisplayLuaError("Overworld engine", "Whoops! There is an error with event indexing.");
            return false;
        }
        if (UnitaleUtil.IsSpecialAnnouncement(go.name) && page != 0) {
            StartCoroutine(SpecialAnnouncementEvent());
            return true;
        }

        //If the script we have to load exists, let's initialize it and then execute it
        if (!isCoroutine) {
            this.actualEventIndex = actualEventIndex;
            PlayerOverworld.instance.PlayerNoMove = true; //Event launched
            ScriptLaunched = true;
        }
        try {
            ScriptWrapper scr;
            if (UnitaleUtil.IsSpecialAnnouncement(go.name)) scr = InitScript(go.name, go.GetComponent<EventOW>());
            else                                            scr = eventScripts[go];

            if (isCoroutine && !coroutines.ContainsKey(scr)) coroutines.Add(scr, go.GetComponent<EventOW>().actualPage);
            else if (!isCoroutine)                           script = scr;

            SetCurrentScript(scr);
            scr.Call("CYFEventStartEvent", DynValue.NewString("EventPage" + (page == -1 ? go.GetComponent<EventOW>().actualPage : page)));
            //scr.Call("EventPage" + go.GetComponent<EventOW>().actualPage);
        } catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(go.GetComponent<EventOW>().scriptToLoad, ex.DecoratedMessage != null ? ex.DecoratedMessage : ex.Message);
            return false;
        } catch (Exception ex) {
            UnitaleUtil.DisplayLuaError(go.GetComponent<EventOW>().scriptToLoad, ex.Message);
            return false;
        }
        if (!isCoroutine) {
            textmgr.SetCaller(script);
            textmgr.transform.parent.parent.SetAsLastSibling();
            passPressOnce = true;
        }
        return true;
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
    /// Checks if the given member (function) has the CYFEventFunction attribute
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private bool MethodHasCYFEventFunctionAttribute(MemberInfo mb) {
        const bool includeInherited = false;
        return mb.GetCustomAttributes(typeof(CYFEventFunction), includeInherited).Any();
    }

    /// <summary>
    /// Generates the internal Lua code used by CYF events one time, and one time only
    /// </summary>
    private void GenerateEventCode() {
        eventCodeFirst = string.Empty;
        string lameFunctionBinding = string.Empty;
        bool once = false;
        foreach (Type t in boundValueName.Keys) {
            List<string> members = CreateBindListMember(t);
            eventCodeFirst += "\n" + boundValueName[t] + " = { }";
            foreach (string member in members) {
                string completeMember = boundValueName[t] + "." + member;
                eventCodeFirst += "\n" + "function " + completeMember + "(...) return CYFEventForwarder(\"" + completeMember + "\",...) end";
                lameFunctionBinding += "\n    " + (once ? "elseif" : "if") + " func == '" + completeMember + "' then x = F" + completeMember;
                once = true;
            }
            eventCodeFirst += "\nsetmetatable(" + boundValueName[t] + @", {
    __index = function(t, k)
        error(""cannot access field "" .. tostring(k) .. "" of userdata <" + t.ToString() + @">"", 2)
    end,
    __newindex = function(t, k)
        error(""cannot access field "" .. tostring(k) .. "" of userdata <" + t.ToString() + @">"", 2)
    end
})";
        }
        once = false;
        lameFunctionBinding += "\n    end";
        eventCodeFirst += @"
CYFEventCoroutine = coroutine.create(DEBUG)
CYFEventCheckRefresh = true
CYFEventLastAction = """"
local CYFEventAlreadyLaunched = false
function CYFFormatError(err)
    local pattern = ':%([%d%-,]+%):'
    local code = err:match(pattern)
    if code then
        local before = err:sub(0, err:find(pattern) + (code:sub(0, 2) == ':(' and 1 or 0))
        local numbers = err:match('[%d,%-]+%)'):sub(0, -2)
        local after = err:sub(err:find(numbers:gsub('%-', '%%-'), #before) + #numbers)

        -- There are only 3 possible formats for error messages
        -- See Assets/Plugins/MoonSharp/Interpreter/Debugging/SourceRef.cs line 178
        local allNums = {}
        for num in numbers:gmatch('%d+') do
            table.insert(allNums, num)
        end
        if numbers == numbers:match('%d+,%d+') then
            numbers = 'line ' .. allNums[1] .. ', char ' .. allNums[2]
        elseif numbers == numbers:match('%d+,%d+%-%d+') then
            numbers = 'line ' .. allNums[1] .. ', char ' .. allNums[2] .. '-' .. allNums[3]
        elseif numbers == numbers:match('%d+,%d+%-%d+,%d+') then
            numbers = 'line ' .. allNums[1] .. ', char ' .. allNums[2] .. '-line ' .. allNums[3] .. ', char ' .. allNums[4]
        end

        return ""error in script "" .. _internalScriptName .. ""\n\n"" .. before .. numbers .. after
    else
        return err
    end
end
function CYFEventFuncToLaunch(x)
    if _internalScriptName == nil then
        _internalScriptName = '";
        eventCodeLast = @"'
    end
    local err
    if not xpcall(x, function(err2) err = err2 end) then
        error(CYFFormatError(err), 0)
    end
end
function CYFEventNextCommand()
    CYFEventAlreadyLaunched = true
    if tostring(coroutine.status(CYFEventCoroutine)) == 'suspended' then
        local ok, errorMsg = coroutine.resume(CYFEventCoroutine)
        if not ok then error(errorMsg, 0) end
    end
end
function CYFEventStopCommand() coroutine.yield() end --currently unused
function CYFEventStartEvent(func)
    if _G[func] == nil then error(""error in script "" .. _internalScriptName .. ""\n\nThe function "" .. func .. "" doesn't exist in the Event script."", 0) end
    CYFEventCoroutine = coroutine.create(function() CYFEventFuncToLaunch(_G[func]) end)
    local ok, errorMsg = coroutine.resume(CYFEventCoroutine)
    if not ok then error(errorMsg, 0) end
end
function CYFEventForwarder(func, ...)
    CYFEventAlreadyLaunched = false
    CYFEventCheckRefresh = true
    FGeneral.HiddenReloadAppliedScript()
    CYFEventLastAction = func
    local x" + lameFunctionBinding + "\n"
+ @"local ok
    local result
    local hasArgs = false
    for k, v in pairs(({...})) do
        hasArgs = true
        ok, result = pcall(x, ...)
        break
    end
    if not hasArgs then
        ok, result = pcall(x)
    end
    if not ok then
        error(CYFFormatError(result), 0)
    end
    if not CYFEventAlreadyLaunched then coroutine.yield() end
    return result
end";
    }

    /// <summary>
    /// Function that permits to initialize the event script to be used later
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Returns true if no error were encountered.</returns>
    private ScriptWrapper InitScript(string name, EventOW ev) {
        ScriptWrapper scr = new ScriptWrapper() { scriptname = name };
        string scriptText = UnitaleUtil.IsSpecialAnnouncement(name) ? CYFReleaseScript : ScriptRegistry.Get(ScriptRegistry.EVENT_PREFIX + name);
        if (scriptText == null) {
            UnitaleUtil.DisplayLuaError("Launching an event", "The event \"" + name + "\" doesn't exist.");
            return null;
        }

        //Run engine-provided Lua code for Event scripts (generate it if needed)
        if (eventCodeFirst == null)
            GenerateEventCode();
        try { scr.script.DoString(eventCodeFirst + ev.gameObject.name + eventCodeLast, null, "CYF internal event code (please report!)"); }
        catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(name, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            return null;
        } catch (Exception ex) {
            UnitaleUtil.DisplayLuaError(name, ex.Message);
            return null;
        }

        scr.script.Globals["CreateLayer"] = (Func<string, string, bool, bool>)SpriteUtil.CreateLayer;
        scr.script.Globals["CreateSprite"] = (Func<string, string, int, DynValue>)SpriteUtil.MakeIngameSprite;
        scr.script.Globals["CreateText"] = (Func<Script, DynValue, DynValue, int, string, int, LuaTextManager>)LuaScriptBinder.CreateText;

        try { scr.DoString(scriptText); }
        catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(name, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            return null;
        } catch (Exception ex) {
            UnitaleUtil.DisplayLuaError(name, ex.Message);
            return null;
        }

        return scr;
    }

    private delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg, T6 arg6);

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
    public void SetPlayerOnSelection(int selection, bool question = false, bool threeLines = false) {
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
        if (dir.x == 0 && dir.y == 0)
                           return 0;
        else if (dir.x == 0) {
            if (dir.y > 0) return 8;
            else           return 2;
        } else if (dir.y == 0) {
            if (dir.x > 0) return 6;
            else           return 4;
        }
        float tempDir = dir.y / dir.x;
        if (tempDir > 1 || tempDir < -1) {
            if (dir.y > 0) return 8;
            else           return 2;
        } else {
            if (dir.x > 0) return 6;
            else           return 4;
        }
    }

    //Used to add event states before unloading the map
    public void SetEventStates(bool addPlayer = false) {
        // int id = SceneManager.GetActiveScene().buildIndex;
        string id = SceneManager.GetActiveScene().name;
        EventOW[] events = (EventOW[])GameObject.FindObjectsOfType(typeof(EventOW));
        //MapDataAnalyser();

        GameState.MapData mapInfos = GlobalControls.GameMapData.ContainsKey(id) ? GlobalControls.GameMapData[id] : new GameState.MapData();

        if (GlobalControls.GameMapData.ContainsKey(id))
            GlobalControls.GameMapData.Remove(id);

        MapInfos mi = GameObject.FindObjectOfType<MapInfos>();
        mapInfos.Name = SceneManager.GetActiveScene().name;
        mapInfos.Music = mi.music;
        mapInfos.ModToLoad = mi.modToLoad;
        mapInfos.MusicKept = mi.isMusicKeptBetweenBattles;
        mapInfos.NoRandomEncounter = mi.noRandomEncounter;

        Dictionary<string, GameState.EventInfos> eis = new Dictionary<string, GameState.EventInfos>();
        foreach (string str in GlobalControls.EventData.Keys)
            eis.Add(str, GlobalControls.EventData[str]);

        // Add the Player to the current map
        if (addPlayer) {
            GameState.EventInfos eiPlayer = new GameState.EventInfos() {
                CurrPage = 0,
                CurrSpriteNameOrCYFAnim = GameObject.Find("Player").GetComponent<CYFAnimator>().specialHeader,
                NoCollision = false,
                Anchor = UnitaleUtil.VectorToVect(GameObject.Find("Player").GetComponent<RectTransform>().anchorMax),
                Pivot = UnitaleUtil.VectorToVect(GameObject.Find("Player").GetComponent<RectTransform>().pivot)
            };
            eis.Add("Player", eiPlayer);
        }

        foreach (EventOW ev in events) {
            if (ev.name.Contains("Image") || ev.name.Contains("Tone"))
                continue;
            if (eis.ContainsKey(ev.name))
                eis.Remove(ev.name);
            try {
                GameState.EventInfos ei = new GameState.EventInfos() {
                    CurrPage = ev.actualPage,
                    CurrSpriteNameOrCYFAnim = ev.GetComponent<CYFAnimator>()
                        ? ev.GetComponent<CYFAnimator>().specialHeader
                        : instance.sprCtrls[ev.name].img.GetComponent<SpriteRenderer>()
                            ? instance.sprCtrls[ev.name].img.GetComponent<SpriteRenderer>().sprite.name
                            : instance.sprCtrls[ev.name].img.GetComponent<Image>().sprite.name,
                    NoCollision = ev.gameObject.layer == 0,
                    Anchor = UnitaleUtil.VectorToVect(ev.GetComponent<RectTransform>().anchorMax),
                    Pivot = UnitaleUtil.VectorToVect(ev.GetComponent<RectTransform>().pivot)
                };
                eis.Add(ev.name, ei);
            } catch { }
        }
        mapInfos.EventInfo = eis;
        GlobalControls.GameMapData.Add(id, mapInfos);
        //MapDataAnalyser();
        instance.sprCtrls.Clear();
    }

    public static void TrySetMapValue(string mapName, string var, object val) {
        var = var.ToLower();
        if (var != "music" && var != "modtoload" && var != "musickept" && var != "norandomencounter")
            throw new CYFException("You tried to change a map's \"" + var + "\" value but it doesn't exist.\nYou can only choose between \"Music\", \"ModToLoad\", \"MusicKept\" and \"NoRandomEncounter\".");
        if (var == "musickept" || var == "norandomencounter") {
            if (val.ToString().ToLower() == "true")       val = true;
            else if (val.ToString().ToLower() == "false") val = false;
            else                                          throw new CYFException("\"MusicKept\" and \"NoRandomEncounter\" are boolean values. You can only enter \"true\" or \"false\".");
        }

        foreach (KeyValuePair<string, GameState.MapData> kvp in GlobalControls.GameMapData) {
            if (kvp.Value.Name == mapName) {
                GameState.MapData mi = kvp.Value;
                GlobalControls.GameMapData.Remove(kvp.Key);

                if (var == "music")          mi.Music = val.ToString();
                else if (var == "modtoload") mi.ModToLoad = val.ToString();
                else if (var == "musickept") mi.MusicKept = (bool)val;
                else                         mi.NoRandomEncounter = (bool)val;
                GlobalControls.GameMapData.Add(kvp.Key, mi);
                return;
            }
        }

        foreach (KeyValuePair<string, GameState.TempMapData> kvp in GlobalControls.TempGameMapData) {
            Debug.Log(kvp.Key + " == " + mapName);
            if (kvp.Key == mapName) {
                GameState.TempMapData tmi = kvp.Value;
                GlobalControls.TempGameMapData.Remove(kvp.Key);

                if (var == "music") {
                    tmi.Music = val.ToString();
                    tmi.MusicChanged = true;
                } else if (var == "modtoload") {
                    tmi.ModToLoad = val.ToString();
                    tmi.ModToLoadChanged = true;
                } else if (var == "musickept") {
                    tmi.MusicKept = (bool)val;
                    tmi.MusicKeptChanged = true;
                } else {
                    tmi.NoRandomEncounter = (bool)val;
                    tmi.NoRandomEncounterChanged = true;
                }
                GlobalControls.TempGameMapData.Add(kvp.Key, tmi);
                return;
            }
        }

        GameState.TempMapData tmi2 = new GameState.TempMapData {
            MusicChanged = false,
            ModToLoadChanged = false,
            MusicKeptChanged = false,
            NoRandomEncounterChanged = false
        };

        if (var == "music") {
            tmi2.Music = val.ToString();
            tmi2.MusicChanged = true;
        } else if (var == "modtoload") {
            tmi2.ModToLoad = val.ToString();
            tmi2.ModToLoadChanged = true;
        } else if (var == "musickept") {
            tmi2.MusicKept = (bool)val;
            tmi2.MusicKeptChanged = true;
        } else {
            tmi2.NoRandomEncounter = (bool)val;
            tmi2.NoRandomEncounterChanged = true;
        }
        GlobalControls.TempGameMapData.Add(mapName, tmi2);
    }

    public static object TryGetMapValue(string mapName, string var) {
        var = var.ToLower();
        foreach (GameState.MapData md in GlobalControls.GameMapData.Values)
            if (md.Name == mapName)
                if (var == "music")          return md.Music;
                else if (var == "modtoload") return md.ModToLoad;
                else if (var == "musickept") return md.MusicKept;
                else                         return md.NoRandomEncounter;

        foreach (GameState.TempMapData tmd in GlobalControls.TempGameMapData.Values)
            if (tmd.Name == mapName)
                if (var == "music")          return tmd.Music;
                else if (var == "modtoload") return tmd.ModToLoad;
                else if (var == "musickept") return tmd.MusicKept;
                else                         return tmd.NoRandomEncounter;

        int buildIndex = -1;
        //Start map tester
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            if (mapName == System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i))) {
                buildIndex = i;
                break;
            }
            if (i == SceneManager.sceneCountInBuildSettings - 1)
                throw new CYFException("The scene \"" + mapName + "\" doesn't exist.\nYou must enter the scene's file name, not its alias.");
        }
        //End map tester
        foreach (GameObject go in SceneManager.GetSceneByBuildIndex(buildIndex).GetRootGameObjects()) {
            if (go.GetComponent<MapInfos>())
                if (var == "music")          return go.GetComponent<MapInfos>().music;
                else if (var == "modtoload") return go.GetComponent<MapInfos>().modToLoad;
                else if (var == "musickept") return go.GetComponent<MapInfos>().isMusicKeptBetweenBattles;
                else                         return go.GetComponent<MapInfos>().noRandomEncounter;
        }
        return null;
    }

    public static void MapDataAnalyser() {
        string str = "MapData = {\n";
        bool once = false, once2 = false;
        foreach (string id in GlobalControls.GameMapData.Keys) {
            str += once ? ",\n" : "";
            if (!once) once = true;
            str += "  scene " + id + " for\n";
            GameState.MapData mi = GlobalControls.GameMapData[id];
            str += "    Name = \"" + mi.Name + "\"\n";
            str += "    Music = \"" + mi.Music + "\"\n";
            str += "    ModToLoad = \"" + mi.ModToLoad + "\"\n";
            str += "    MusicKept = " + mi.MusicKept + "\n";
            str += "    NoRandomEncounter = " + mi.NoRandomEncounter + "\n";
            str += "    EventInfo = {\n";
            foreach (string str2 in mi.EventInfo.Keys) {
                str += once2 ? ",\n" : "";
                if (!once2) once2 = true;
                GameState.EventInfos ei = mi.EventInfo[str2];
                str += "      name = \"" + str2 + "\" for \n";
                str += "        CurrPage = " + ei.CurrPage + "\n";
                str += "        CurrSpriteNameOrCYFAnim = \"" + ei.CurrSpriteNameOrCYFAnim + "\"\n";
                str += "        NoCollision = " + ei.NoCollision + "\n";
                str += "        Anchor = " + UnitaleUtil.VectToVector(ei.Anchor) + "\n";
                str += "        Pivot = " + UnitaleUtil.VectToVector(ei.Pivot) + "";
            }
            str += "\n    }";
            once2 = false;
        }
        str += "\n}";
        print(str);
    }

    public static void GetMapState(MapInfos mi, string id) {
        if (!GlobalControls.GameMapData.ContainsKey(id)) {
            if (GlobalControls.TempGameMapData.ContainsKey(SceneManager.GetActiveScene().name)) {
                GameState.TempMapData tmd = GlobalControls.TempGameMapData[SceneManager.GetActiveScene().name];
                GlobalControls.TempGameMapData.Remove(SceneManager.GetActiveScene().name);
                if (tmd.MusicChanged)             mi.music = tmd.Music;
                if (tmd.ModToLoadChanged)         mi.modToLoad = tmd.ModToLoad;
                if (tmd.MusicKeptChanged)         mi.isMusicKeptBetweenBattles = tmd.MusicKept;
                if (tmd.NoRandomEncounterChanged) mi.noRandomEncounter = tmd.NoRandomEncounter;
            }
            return;
        }

        GameState.MapData misave = GlobalControls.GameMapData[id];
        mi.music = misave.Music;
        mi.modToLoad = misave.ModToLoad;
        mi.isMusicKeptBetweenBattles = misave.MusicKept;
        mi.noRandomEncounter = misave.NoRandomEncounter;

        //print("GetMapState: " + SceneManager.GetSceneByBuildIndex(id).name);
        foreach (string str in misave.EventInfo.Keys) {
            try {
                GameObject go = GameObject.Find(str);
                if (go == null)
                    continue;

                GameState.EventInfos ei = misave.EventInfo[str];
                if (str == "Player")
                    go.GetComponent<CYFAnimator>().specialHeader = ei.CurrSpriteNameOrCYFAnim;
                else {
                    EventOW ev = go.GetComponent<EventOW>();
                    if (!ev)
                        continue;
                    ev.actualPage = ei.CurrPage;
                    if (ev.GetComponent<CYFAnimator>())                       ev.GetComponent<CYFAnimator>().specialHeader = ei.CurrSpriteNameOrCYFAnim;
                    else {
                        if (ev.GetComponent<AutoloadResourcesFromRegistry>()) ev.GetComponent<AutoloadResourcesFromRegistry>().SpritePath = ei.CurrSpriteNameOrCYFAnim;
                        else if (UnitaleUtil.IsSpecialAnnouncement(ev.name)) { // TODO: Remove this condition?
                            if (ev.GetComponent<Image>())                     ev.GetComponent<Image>().sprite = SpriteRegistry.Get(ei.CurrSpriteNameOrCYFAnim);
                            else                                              ev.GetComponent<SpriteRenderer>().sprite = SpriteRegistry.Get(ei.CurrSpriteNameOrCYFAnim);
                        }
                    }
                    ev.gameObject.layer = ei.NoCollision ? 0 : 21;
                }
                go.GetComponent<RectTransform>().anchorMax = UnitaleUtil.VectToVector(ei.Anchor);
                go.GetComponent<RectTransform>().anchorMin = UnitaleUtil.VectToVector(ei.Anchor);
                go.GetComponent<RectTransform>().pivot = UnitaleUtil.VectToVector(ei.Pivot);
            } catch (Exception e) { Debug.LogError(e); }
        }
    }

    /// <summary>
    /// Used to end a current event
    /// </summary>
    public void EndEvent() {
        PlayerOverworld.instance.textmgr.SetTextFrameAlpha(0);
        PlayerOverworld.instance.textmgr.textQueue = new TextMessage[] { };
        PlayerOverworld.instance.textmgr.DestroyChars();
        PlayerOverworld.instance.PlayerNoMove = false; //Event finished
        PlayerOverworld.instance.UIPos = 0;
        ScriptLaunched = false;
        script = null;
    }

    public void StCoroutine(string coroName, object args, string evName) {
        string key = evName + "." + coroName;
        ForceEndCoroutine(key);
        UnityEngine.Coroutine newCoro;
        if (args == null)                 newCoro = StartCoroutine(coroName);
        else if (!args.GetType().IsArray) newCoro = StartCoroutine(coroName, args);
        else                              newCoro = StartCoroutine(coroName, (object[])args);
        cSharpCoroutines.Add(key, newCoro);
    }

    public void ForceEndCoroutine(string key) {
        if (cSharpCoroutines.ContainsKey(key)) {
            UnityEngine.Coroutine existingCoro;
            cSharpCoroutines.TryGetValue(key, out existingCoro);
            if (existingCoro != null)
                StopCoroutine(existingCoro);
            cSharpCoroutines.Remove(key);
        }
    }

    private IEnumerator SpecialAnnouncementEvent() {
        luaplow.CanMove(false);
        LuaScriptBinder.Set(null, "1a6377e26b5119334e651552be9f17f8d92e83c9", DynValue.NewBoolean(false));
        Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        Sprite[] sprs = Resources.LoadAll<Sprite>("Sprites");
        foreach (Sprite spr in sprs)
            Sprites.Add(spr.name, spr);
        GameObject go = GameObject.Find("4eab1af3ab6a932c23b3cdb8ef618b1af9c02088");
        Dictionary<string, AudioClip> Audios = new Dictionary<string, AudioClip>();
        AudioClip[] adcs = Resources.LoadAll<AudioClip>("Audios");
        foreach (AudioClip adc in adcs)
            Audios.Add(adc.name, adc);
        go.GetComponent<SpriteRenderer>().sprite = Sprites["mm2"];
        go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
        NewMusicManager.StopAll();
        AudioSource audio = NewMusicManager.CreateChannelAndGetAudioSource("4eab1af3ab6a932c23b3cdb8ef618b1af9c02088");
        audio.loop = false;
        audio.clip = Audios["sound"];
        audio.Play();
        while (audio.isPlaying)
            yield return 0;
        NewMusicManager.DestroyChannel("4eab1af3ab6a932c23b3cdb8ef618b1af9c02088");
        SceneManager.LoadScene("SpecialAnnouncement");
        GameObject.Destroy(GameObject.Find("Player"));
        GameObject.Destroy(GameObject.Find("Canvas OW"));
        GameObject.Destroy(GameObject.Find("Canvas Two"));
        GameObject.Destroy(GameObject.Find("Main Camera OW"));
    }

    private string CYFReleaseScript = "function EventPage0()\n" +
                                      "    if not GetRealGlobal(\"1a6377e26b5119334e651552be9f17f8d92e83c9\") then\n" +
                                      "        Event.Remove(Event.GetName())\n" +
                                      "    end\n" +
                                      "end\n" +
                                      "function EventPage1() end";

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
        ScriptWrapper scr = luagenow.appliedScript;

        bool question;
        bool[] oneLiners;
        try { question = (bool)args[0]; }   catch { throw new CYFException("The argument \"question\" must be a boolean."); }
        try { oneLiners = (bool[])args[1]; } catch { throw new CYFException("The argument \"oneLiners\" must be a boolean table."); }

        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.SetChoice: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
            yield break;
        } else if (LoadLaunched) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.SetChoice: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
            yield break;
        }

        while (!textmgr.LineComplete())
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
        SetPlayerOnSelection(0, question, !oneLiners[0]);
        while (true) {
            if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
                actualChoice = (actualChoice + 1) % 2;
                SetPlayerOnSelection(actualChoice, question, !oneLiners[actualChoice]);
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED)
                if (!textmgr.blockSkip && !textmgr.LineComplete() && textmgr.CanSkip())
                    textmgr.SkipLine();
                else
                    break;
            yield return 0;
        }
        //Hide the heart, we don't need it anymore
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
        //Add a new variable that can be used in lua functions named Temp plus the index we gave earlier (or else a free index)
        script.script.Globals.Set(DynValue.NewString("lastChoice"), DynValue.NewNumber(actualChoice));
        //HEARTBROKEN
        Destroy(tempHeart);
        yield return 0;
    }

    IEnumerator IMoveEventToPoint(object[] args) { //NEED PARENTAL REMOVE
        ScriptWrapper scr = (ScriptWrapper)args[0];

        string name;
        float dirX, dirY;
        bool wallPass, waitEnd;
        try { name = (string)args[1];   } catch { throw new CYFException("The argument \"name\" must be a string."); }
        try { dirX = (float)args[2];    } catch { throw new CYFException("The argument \"dirX\" must be a number."); }
        try { dirY = (float)args[3];    } catch { throw new CYFException("The argument \"dirY\" must be a number."); }
        try { wallPass = (bool)args[4]; } catch { throw new CYFException("The argument \"wallPass\" must be a boolean."); }
        try { waitEnd = (bool)args[5];  } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.MoveToPoint: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            } else if (LoadLaunched) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.MoveToPoint: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        for (int i = 0; i < events.Count || name == "Player"; i++)
            if (name == events[i].name || name == "Player") {
                GameObject go;
                if (name == "Player") go = GameObject.Find("Player");
                else                  go = events[i];
                Transform target = null;
                if (go.transform.parent != null)
                    if (go.transform.parent.name == "SpritePivot")
                        target = go.transform.parent;
                target = target ?? go.transform; //oof

                //Check if target is already in the middle of IMoveEventToPoint
                if (name == "Player") {
                    ScriptWrapper isMovingSource = go.GetComponent<PlayerOverworld>().isMovingSource;
                    Vector2 roundedPos = new Vector2(Mathf.Round(target.position.x * 1000) / 1000, Mathf.Round(target.position.y * 1000) / 1000);
                    Vector2 roundedEnd = new Vector2(Mathf.Round(dirX              * 1000) / 1000, Mathf.Round(dirY              * 1000) / 1000);
                    if (roundedPos == roundedEnd)
                        go.GetComponent<PlayerOverworld>().isMovingSource = null;
                    if (isMovingSource != null && isMovingSource != scr && go.GetComponent<PlayerOverworld>().isMovingWaitEnd)
                        isMovingSource.Call("CYFEventNextCommand");
                    go.GetComponent<PlayerOverworld>().isMovingSource  = null;
                    go.GetComponent<PlayerOverworld>().isMovingWaitEnd = waitEnd;
                } else {
                    ScriptWrapper isMovingSource = go.GetComponent<EventOW>().isMovingSource;
                    Vector2 roundedPos = new Vector2(Mathf.Round(target.position.x * 1000) / 1000, Mathf.Round(target.position.y * 1000) / 1000);
                    Vector2 roundedEnd = new Vector2(Mathf.Round(dirX              * 1000) / 1000, Mathf.Round(dirY              * 1000) / 1000);
                    if (roundedPos == roundedEnd)
                        go.GetComponent<EventOW>().isMovingSource = null;
                    if (isMovingSource != null && isMovingSource != scr && go.GetComponent<EventOW>().isMovingWaitEnd)
                        isMovingSource.Call("CYFEventNextCommand");
                    go.GetComponent<EventOW>().isMovingSource  = null;
                    go.GetComponent<EventOW>().isMovingWaitEnd = waitEnd;
                }

                if (!waitEnd)
                    scr.Call("CYFEventNextCommand");

                Vector2 endPoint = new Vector2(dirX - target.position.x, dirY - target.position.y);//, endPointFromNow = endPoint;

                // store the event's initial position
                Vector2 originalPosition = new Vector2(target.position.x, target.position.y);

                //The animation process is automatic, if you renamed the Animation's triggers and animations as the Player's
                if (go.GetComponent<CYFAnimator>()) {
                    int direction = CheckDirection(endPoint);
                    go.GetComponent<CYFAnimator>().movementDirection = direction;
                }

                /*
                float speed;
                try {
                    speed = go != GameObject.Find("Player").transform.gameObject ? go.GetComponent<EventOW>().moveSpeed : PlayerOverworld.instance.speed;
                } catch { yield break; }
                */
                while (true) {
                    if (go == null) {
                        scr.Call("CYFEventNextCommand");
                        yield break;
                    }
                    //Test is used to know if movement is possible or not
                    Vector2 clamped = Vector2.ClampMagnitude(endPoint, 1);
                    bool test2 = false;
                    Vector2 distanceFromStart = new Vector2(0, 0);

                    test2 = PlayerOverworld.instance.AttemptMove(clamped.x, clamped.y, go, wallPass);
                    distanceFromStart = new Vector2(target.position.x - originalPosition.x, target.position.y - originalPosition.y);

                    if (name == "Player") {
                        go.GetComponent<PlayerOverworld>().isBeingMoved    = (test2 || wallPass) && distanceFromStart.magnitude > 0;
                        go.GetComponent<PlayerOverworld>().isMoving        = (test2 || wallPass) && distanceFromStart.magnitude > 0;
                        go.GetComponent<PlayerOverworld>().isMovingSource  = scr;
                        go.GetComponent<PlayerOverworld>().isMovingWaitEnd = waitEnd;
                    } else {
                        go.GetComponent<EventOW>().isMovingSource  = ((test2 || wallPass) && distanceFromStart.magnitude > 0) ? scr : null;
                        go.GetComponent<EventOW>().isMovingWaitEnd = waitEnd;
                    }

                    //If we have reached the destination, stop the function
                    if (distanceFromStart.magnitude >= endPoint.magnitude) {
                        // if this code is run, that means the player must have reached their destination

                        target.position = new Vector3(dirX, dirY, target.position.z);

                        if (name == "Player") {
                            go.GetComponent<PlayerOverworld>().isBeingMoved    = false;
                            go.GetComponent<PlayerOverworld>().isMoving        = false;
                            go.GetComponent<PlayerOverworld>().isMovingSource  = null;
                            go.GetComponent<PlayerOverworld>().isMovingWaitEnd = false;
                        } else {
                            go.GetComponent<EventOW>().isMovingSource  = null;
                            go.GetComponent<EventOW>().isMovingWaitEnd = false;
                        }
                        if (waitEnd)
                            scr.Call("CYFEventNextCommand");
                        yield break;
                    }
                    yield return 0;

                    if (!test2 && !wallPass) {
                        if (name == "Player") {
                            go.GetComponent<PlayerOverworld>().isBeingMoved    = false;
                            go.GetComponent<PlayerOverworld>().isMoving        = false;
                            go.GetComponent<PlayerOverworld>().isMovingSource  = null;
                            go.GetComponent<PlayerOverworld>().isMovingWaitEnd = false;
                        } else {
                            go.GetComponent<EventOW>().isMovingSource  = null;
                            go.GetComponent<EventOW>().isMovingWaitEnd = false;
                        }
                        if (waitEnd)
                            scr.Call("CYFEventNextCommand");
                        yield break;
                    }
                }
            }
        UnitaleUtil.WriteInLogAndDebugger("Event.MoveToPoint: The name you entered in the function doesn't exist. Did you forget to add the 'Event' tag?");
        scr.Call("CYFEventNextCommand");
    }

    IEnumerator IWaitForInput() {
        ScriptWrapper scr = luagenow.appliedScript;
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.WaitForInput: This function cannot be used in a coroutine.");
            yield break;
        } else if (LoadLaunched) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.WaitForInput: This function cannot be used in EventPage0.");
            yield break;
        }

        while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED)
            yield return 0;

        scr.Call("CYFEventNextCommand");
    }

    IEnumerator ISetTone(object[] args) {
        ScriptWrapper scr = luascrow.appliedScript;
        //REAL ARGS
        bool waitEnd;
        int r, g, b, a;
        try { waitEnd = (bool)args[0]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }
        try { r = (int)args[1];        } catch { throw new CYFException("The argument \"r\" must be a number."); }
        try { g = (int)args[2];        } catch { throw new CYFException("The argument \"g\" must be a number."); }
        try { b = (int)args[3];        } catch { throw new CYFException("The argument \"b\" must be a number."); }
        try { a = (int)args[4];        } catch { throw new CYFException("The argument \"a\" must be a number."); }

        if (waitEnd)
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Screen.SetTone: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            } else if (LoadLaunched) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Screen.SetTone: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        Color c = GameObject.Find("Tone").GetComponent<Image>().color;

        int[] currents = new int[] { (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), (int)(c.a * 255) };
        int[] lacks = new int[] { r - currents[0], g - currents[1], b - currents[2], a - currents[3] };
        int[] beginLacks = lacks;
        float[] realLacks = new float[] { lacks[0], lacks[1], lacks[2], lacks[3] };

        float highest = 0;
        foreach (int i in lacks)
            highest = Mathf.Abs(i) > highest ? Mathf.Abs(i) : highest;
        float beginHighest = highest;

        if (!waitEnd)
            scr.Call("CYFEventNextCommand");

        while (GameObject.Find("Tone") != null && GameObject.Find("Tone").GetComponent<Image>().color != new Color32((byte)r, (byte)g, (byte)b, (byte)a)) {
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

        if (GameObject.Find("Tone") != null && a == 0)
            luaevow.Remove("Tone");
        if (waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    IEnumerator IRotateEvent(object[] args) {
        ScriptWrapper scr = (ScriptWrapper)args[0];

        string name;
        float rotateX, rotateY, rotateZ;
        bool waitEnd;
        try { name = (string)args[1];   } catch { throw new CYFException("The argument \"name\" must be a string."); }
        try { rotateX = (float)args[2]; } catch { throw new CYFException("The argument \"rotateX\" must be a number."); }
        try { rotateY = (float)args[3]; } catch { throw new CYFException("The argument \"rotateY\" must be a number."); }
        try { rotateZ = (float)args[4]; } catch { throw new CYFException("The argument \"rotateZ\" must be a number."); }
        try { waitEnd = (bool)args[5]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.Rotate: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            } else if (LoadLaunched) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.Rotate: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        for (int i = 0; i < events.Count || name == "Player"; i++) {
            GameObject go = events[i];
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");

                //Check if target is already in the middle of IRotateEvent
                if (name == "Player") {
                    ScriptWrapper isRotatingSource = go.GetComponent<PlayerOverworld>().isRotatingSource;
                    if (go.transform.rotation.eulerAngles.x == rotateX && go.transform.rotation.eulerAngles.y == rotateY && go.transform.rotation.eulerAngles.z == rotateZ)
                        go.GetComponent<PlayerOverworld>().isRotatingSource = null;
                    if (isRotatingSource != null && isRotatingSource != scr && go.GetComponent<PlayerOverworld>().isRotatingWaitEnd)
                        isRotatingSource.Call("CYFEventNextCommand");
                    go.GetComponent<PlayerOverworld>().isRotatingSource = null;
                    go.GetComponent<PlayerOverworld>().isRotatingWaitEnd = false;
                } else {
                    ScriptWrapper isRotatingSource = go.GetComponent<EventOW>().isRotatingSource;
                    if (go.transform.rotation.eulerAngles.x == rotateX && go.transform.rotation.eulerAngles.y == rotateY && go.transform.rotation.eulerAngles.z == rotateZ)
                        go.GetComponent<EventOW>().isRotatingSource = null;
                    if (isRotatingSource != null && isRotatingSource != scr && go.GetComponent<EventOW>().isRotatingWaitEnd)
                        isRotatingSource.Call("CYFEventNextCommand");
                    go.GetComponent<EventOW>().isRotatingSource = null;
                    go.GetComponent<EventOW>().isRotatingWaitEnd = false;
                }

                if (name == "Player") {
                    go.GetComponent<PlayerOverworld>().isRotatingSource  = scr;
                    go.GetComponent<PlayerOverworld>().isRotatingWaitEnd = waitEnd;
                } else {
                    go.GetComponent<EventOW>().isRotatingSource  = scr;
                    go.GetComponent<EventOW>().isRotatingWaitEnd = waitEnd;
                }
                if (!waitEnd)
                    scr.Call("CYFEventNextCommand");

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

                if (name == "Player") {
                    go.GetComponent<PlayerOverworld>().isRotatingSource  = null;
                    go.GetComponent<PlayerOverworld>().isRotatingWaitEnd = false;
                } else {
                    go.GetComponent<EventOW>().isRotatingSource  = null;
                    go.GetComponent<EventOW>().isRotatingWaitEnd = false;
                }
                if (waitEnd)
                    scr.Call("CYFEventNextCommand");

                yield break;
            }
        }
        UnitaleUtil.WriteInLogAndDebugger("Event.Rotate: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
        scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    IEnumerator IFadeBGM(object[] args) {
        ScriptWrapper scr = luascrow.appliedScript;
        int fadeFrames;
        bool waitEnd;
        try { fadeFrames = (int)args[0]; } catch { throw new CYFException("The argument \"fadeFrames\" must be an integer."); }
        try { waitEnd = (bool)args[1]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "General.StopBGM: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            } else if (LoadLaunched) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "General.StopBGM: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        bgmCoroutine = true;
        AudioSource audio = UnitaleUtil.GetCurrentOverworldAudio();
        float frames = 0, startVolume = audio.volume;
        while (frames < fadeFrames) {
            audio.volume = startVolume - (startVolume * frames / fadeFrames);
            frames++;
            yield return 0;
        }
        audio.Stop();
        audio.volume = 1;
        bgmCoroutine = false;
        yield return 0;
    }

    IEnumerator IFlash(object[] args) {
        ScriptWrapper scr = luascrow.appliedScript;
        //REAL ARGS
        int frames, colorR, colorG, colorB, colorA;
        bool waitEnd;
        try { frames = (int)args[0]; } catch { throw new CYFException("The argument \"frames\" must be a number."); }
        try { colorR = (int)args[1]; } catch { throw new CYFException("The argument \"colorR\" must be a number."); }
        try { colorG = (int)args[2]; } catch { throw new CYFException("The argument \"colorG\" must be a number."); }
        try { colorB = (int)args[3]; } catch { throw new CYFException("The argument \"colorB\" must be a number."); }
        try { colorA = (int)args[4]; } catch { throw new CYFException("The argument \"colorA\" must be a number."); }
        try { waitEnd = (bool)args[5]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Screen.Flash: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            } else if (LoadLaunched) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Screen.Flash: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        GameObject flash = new GameObject("flash", new Type[] { typeof(Image) });
        flash.transform.SetParent(GameObject.Find("Canvas OW").transform);
        flash.transform.position = Camera.main.transform.position + new Vector3(0, 0, 1);
        flash.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 800);

        flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)colorA);
        for (int frame = 0; frame < frames; frame++) {
            if (frame != 0)
                flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)(colorA - colorA * frame / frames));
            yield return 0;
        }
        Destroy(flash);
        if (waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    IEnumerator ISave(object[] args) {
        ScriptWrapper scr = luagenow.appliedScript;
        //REAL ARGS
        bool forced;
        try { forced = (bool)args[0]; } catch { throw new CYFException("The argument \"forced\" must be a boolean."); }

        if (forced) {
            SaveLoad.Save(true);
            if (scr != null)
                scr.Call("CYFEventNextCommand");
            yield break;
        } else if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Save: This function cannot be used in a coroutine.");
            yield break;
        } else if (LoadLaunched) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Save: This function cannot be used in EventPage0.");
            yield break;
        }
        bool save = true;
        Color c = PlayerOverworld.instance.utHeart.color;
        PlayerOverworld.instance.utHeart.transform.position = new Vector3(151 + Camera.main.transform.position.x - 320, 224 + Camera.main.transform.position.y - 240,
                                                                          PlayerOverworld.instance.utHeart.transform.position.z);
        PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 1);
        GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
        GameObject.Find("save_border_outer").transform.SetAsLastSibling();
        PlayerOverworld.instance.utHeart.transform.SetAsLastSibling();
        TextManager txtLevel = GameObject.Find("TextManagerLevel").GetComponent<TextManager>(), txtTime = GameObject.Find("TextManagerTime").GetComponent<TextManager>(),
                    txtMap = GameObject.Find("TextManagerMap").GetComponent<TextManager>(), txtName = GameObject.Find("TextManagerName").GetComponent<TextManager>(),
                    txtSave = GameObject.Find("TextManagerSave").GetComponent<TextManager>(), txtReturn = GameObject.Find("TextManagerReturn").GetComponent<TextManager>();
        //foreach (RectTransform t in GameObject.Find("save_interior").transform)
            //t.sizeDelta = new Vector2(t.sizeDelta.x, t.sizeDelta.y + 1);

        string playerName = ""; double playerLevel = 0;
        if (SaveLoad.savedGame != null) {
            playerName = SaveLoad.savedGame.player.Name;
            playerLevel = SaveLoad.savedGame.player.LV;

            txtName.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]" + playerName, false, true) });
            txtLevel.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]LV" + playerLevel, false, true) });
            txtTime.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]" + UnitaleUtil.TimeFormatter(SaveLoad.savedGame.playerTime), false, true) });
            GameObject.Find("TextManagerTime").transform.localPosition = new Vector3(180f - UnitaleUtil.CalcTextWidth(txtTime), 68, 0f);
            txtMap.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]" + SaveLoad.savedGame.lastScene, false, true) });
        } else {
            txtName.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]EMPTY", false, true) });
            txtLevel.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]LV0", false, true) });
            txtTime.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]0:00", false, true) });
            GameObject.Find("TextManagerTime").transform.localPosition = new Vector3(130f, 68, 0f);
            txtMap.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]--", false, true) });
        }
        txtSave.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]Save", false, true) });
        txtReturn.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]Return", false, true) });

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
                txtName.DestroyChars(); txtLevel.DestroyChars(); txtTime.DestroyChars(); txtMap.DestroyChars(); txtSave.DestroyChars(); txtReturn.DestroyChars();
                GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                script.Call("CYFEventNextCommand");
                yield break;
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                if (save) {
                    SaveLoad.Save(true);
                    PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 0);
                    txtName.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]" + PlayerCharacter.instance.Name, false, true) });
                    txtLevel.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]LV" + PlayerCharacter.instance.LV, false, true) });
                    txtTime.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]" + UnitaleUtil.TimeFormatter(SaveLoad.savedGame.playerTime), false, true) });
                    GameObject.Find("TextManagerTime").transform.localPosition = new Vector3(180f - UnitaleUtil.CalcTextWidth(txtTime), 68, 0f);
                    txtMap.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]" + SaveLoad.savedGame.lastScene, false, true) });
                    txtSave.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]File saved.", false, true) });
                    txtReturn.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][charspacing:2]", false, true) });
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
                txtName.DestroyChars(); txtLevel.DestroyChars(); txtTime.DestroyChars(); txtMap.DestroyChars(); txtSave.DestroyChars(); txtReturn.DestroyChars();
                GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                script.Call("CYFEventNextCommand");
                yield break;
            }
            yield return 0;
        }
    }

    public IEnumerator IMoveCamera(object[] args) {
        ScriptWrapper scr = luascrow.appliedScript;
        //REAL ARGS
        int pixX, pixY, speed;
        bool straightLine, waitEnd;
        string info;
        try { pixX = (int)args[0];          } catch { throw new CYFException("The argument \"pixX\" must be a number."); }
        try { pixY = (int)args[1];          } catch { throw new CYFException("The argument \"pixY\" must be a number."); }
        try { speed = (int)args[2];         } catch { throw new CYFException("The argument \"speed\" must be a number."); }
        try { straightLine = (bool)args[3]; } catch { throw new CYFException("The argument \"straightLine\" must be a boolean."); }
        try { waitEnd = (bool)args[4];      } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }
        try { info = (string)args[5];       } catch { throw new CYFException("The argument \"info\" must be a string."); }

        if (coroutines.ContainsKey(scr) && script != scr && waitEnd) {
            UnitaleUtil.DisplayLuaError(instance.events[instance.actualEventIndex].name, info + ": This function cannot be used in a coroutine with \"waitEnd\" set to true.");
            yield break;
        } else if (LoadLaunched) {
            UnitaleUtil.DisplayLuaError(instance.events[instance.actualEventIndex].name, info + ": This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
            yield break;
        }

        if (speed <= 0) {
            UnitaleUtil.DisplayLuaError(instance.events[instance.actualEventIndex].name, info + ": The speed of the camera must be strictly positive.");
            yield break;
        }
        float currentX = PlayerOverworld.instance.cameraShift.x, currentY = PlayerOverworld.instance.cameraShift.y,
              xSpeed = currentX > pixX ? -speed : speed, ySpeed = currentY > pixY ? -speed : speed;
        if (straightLine) {
            Vector2 clamped = Vector2.ClampMagnitude(new Vector2(pixX - currentX, pixY - currentY), speed);
            xSpeed = clamped.x;
            ySpeed = clamped.y;
        }
        if (!waitEnd)
            scr.Call("CYFEventNextCommand");
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
        if (waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    public IEnumerator IWait(int frames) {
        ScriptWrapper scr = luagenow.appliedScript;
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Wait: This function cannot be used in a coroutine.");
            yield break;
        } else if (LoadLaunched) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Wait: This function cannot be used in EventPage0.");
            yield break;
        }
        int curr = 0;
        while (curr != frames) {
            curr++;
            yield return 0;
        }
        if (scr.GetVar("CYFEventLastAction").String == "General.Wait")
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    public IEnumerator IEnterShop(bool instant) {
        ScriptWrapper scr = luagenow.appliedScript;
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.EnterShop: This function cannot be used in a coroutine.");
            yield break;
        } else if (LoadLaunched) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.EnterShop: This function cannot be used in EventPage0.");
            yield break;
        }
        if (!instant) {
            Fading fade = FindObjectOfType<Fading>();
            float fadeTime = fade.BeginFade(1);
            yield return new WaitForSeconds(fadeTime);
        }
        EndEvent();

        PlayerOverworld.HideOverworld("Shop");
        GlobalControls.isInShop = true;
        SceneManager.LoadScene("Shop", LoadSceneMode.Additive);
        yield return 0;
    }

    IEnumerator ISpawnBoxMenu() {
        ScriptWrapper scr = luainvow.appliedScript;
        GameObject.Find("itembox").AddComponent<ItemBoxUI>();
        PlayerOverworld.instance.PlayerNoMove = true; //Start box menu

        yield return 0;
        PlayerOverworld.instance.PlayerNoMove = true; //Start box menu #2
        while (ItemBoxUI.active)
            yield return 0;

        scr.Call("CYFEventNextCommand");
        yield return 0;
    }
}