using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Coroutine = UnityEngine.Coroutine;

public class EventManager : MonoBehaviour {
    public  static EventManager instance; // The instance of this class, only one EventManager should exist at all times
    private static int _eventLayer;       // The id of the Event Layer
    public  ScriptWrapper script;         // The non-coroutine script which is currently being run

    public  List<GameObject> autoDone = new List<GameObject>();          // All auto event pages which have been run
    public  List<GameObject> events = new List<GameObject>();            // This map's events
    public  List<GameObject> initializedEvents = new List<GameObject>(); // All events with an initialization event page which have been initialized

    public  Dictionary<GameObject, ScriptWrapper> eventScripts = new Dictionary<GameObject, ScriptWrapper>();             // Lua scripts loaded for event GameObjects
    public  Dictionary<string, Coroutine> cSharpCoroutines = new Dictionary<string, Coroutine>(); // Coroutines run by usual overworld functions for each event
    public  Dictionary<ScriptWrapper, int> coroutines = new Dictionary<ScriptWrapper, int>();                             // Event coroutines currently running
    public  Dictionary<string, LuaSpriteController> spriteControllers = new Dictionary<string, LuaSpriteController>();    // Sprite objects spawned in the overworld

    private static string _eventCodeFirst, _eventCodeLast;    // The internal Lua code loaded into event scripts (prefix and suffix)

    private TextManager _textManager;   // The current TextManager
    public  int actualEventIndex = -1;  // ID of the actual event we're running
    public  bool readyToReLaunch;       // Used to prevent overworld GameOver errors
    public  bool bgmCoroutine;          // Check if the BGM is already fading
    public  bool passPressOnce;         // Used in order to prevent Confirm key actions to be run when they shouldn't
    public  bool scriptRunning;         // Defines whether a script is being executed or not
    public  bool ScriptRunning {        // This value takes in account whether the player can do something or not on top of checking if a script is being executed or not
        get { return scriptRunning || PlayerOverworld.instance.forceNoAction; }
        set { scriptRunning = value; }
    }
    public  bool onceReload;            // Reloads all events when a new map is entered
    public  bool fadeOutToMap = true;   // Defines whether the next map transition will be instant or not
    public  bool eventsLoading;         // True if initialization pages (event page 0) are currently being executed
    public  bool eventsLoaded;          // True if all initialization pages (event page 0) have been executed
    public  bool initialized;           // True if EventManager has been initialized properly. Might be useless?

    private readonly Dictionary<Type, string> _boundValueName = new Dictionary<Type, string>();   // Used to generate the prefix and suffix of any event script
    public LuaPlayerOW luaPlayerOw;         // Instance of LuaPlayerOW used to execute event functions
    public LuaEventOW luaEventOw;           // Instance of LuaEventOW used to execute event functions
    public LuaGeneralOW luaGeneralOw;       // Instance of LuaGeneralOW used to execute event functions
    public LuaInventoryOW luaInventoryOw;   // Instance of LuaInventoryOW used to execute event functions
    public LuaScreenOW luaScreenOw;         // Instance of LuaScreenOW used to execute event functions
    public LuaMapOW luaMapOw;               // Instance of LuaMapOW used to execute event functions


    /// <summary>
    /// Run once when this component is created.
    /// </summary>
    public void LateStart() {
        // Get the layer that will interact with our object, here EventLayer
        _eventLayer = LayerMask.GetMask("EventLayer");
        _textManager = GameObject.Find("TextManager OW").GetComponent<TextManager>();
        // Create all instances of event function classes and store them
        if (_boundValueName.Count == 0) {
            _boundValueName.Add(typeof(LuaEventOW), "Event");
            _boundValueName.Add(typeof(LuaPlayerOW), "Player");
            _boundValueName.Add(typeof(LuaGeneralOW), "General");
            _boundValueName.Add(typeof(LuaInventoryOW), "Inventory");
            _boundValueName.Add(typeof(LuaScreenOW), "Screen");
            _boundValueName.Add(typeof(LuaMapOW), "Map");
            luaPlayerOw = new LuaPlayerOW();
            luaEventOw = new LuaEventOW();
            luaGeneralOw = new LuaGeneralOW(_textManager);
            luaInventoryOw = new LuaInventoryOW();
            luaScreenOw = new LuaScreenOW();
            luaMapOw = new LuaMapOW();
        }
        instance = this;
    }

    /// <summary>
    /// Run whenever this Component is enabled.
    /// </summary>
    private void OnEnable() {
        LuaEventOW.StCoroutine += StCoroutine;
        LuaPlayerOW.StCoroutine += StCoroutine;
        LuaGeneralOW.StCoroutine += StCoroutine;
        LuaInventoryOW.StCoroutine += StCoroutine;
        LuaScreenOW.StCoroutine += StCoroutine;
        LuaMapOW.StCoroutine += StCoroutine;
        StaticInits.Loaded += AfterLoad;
    }

    /// <summary>
    /// Run whenever this Component is disabled.
    /// </summary>
    private void OnDisable() {
        LuaEventOW.StCoroutine -= StCoroutine;
        LuaPlayerOW.StCoroutine -= StCoroutine;
        LuaGeneralOW.StCoroutine -= StCoroutine;
        LuaInventoryOW.StCoroutine -= StCoroutine;
        LuaScreenOW.StCoroutine -= StCoroutine;
        LuaMapOW.StCoroutine -= StCoroutine;
        StaticInits.Loaded -= AfterLoad;
    }

    /// <summary>
    /// Run whenever the map's mod is loaded.
    /// It resets all current events, initializes all events with an initialization page and fade the map out when it's done.
    /// </summary>
    public void AfterLoad() {
        eventsLoading = true;
        if (script == null) {
            // Only run once, when the map is loaded for the first time
            if (!onceReload) {
                onceReload = true;
                ResetEvents();
                TestEventDestruction();
                PlayerOverworld.instance.utHeart = GameObject.Find("utHeart").GetComponent<Image>();
            }
            // Execute all initialization pages of events having one one after the other, one per frame
            foreach (GameObject t in events)
                if (t != null) {
                    if (!UnitaleUtil.TestContainsListVector2(t.GetComponent<EventOW>().eventTriggers, 0) || initializedEvents.Contains(t))
                        continue;
                    initializedEvents.Add(t);
                    ExecuteEvent(t, 0);
                    return;
                }

            // Fade the map out when the map is initialized or there are events
            if (initialized || events.Count != 0)
                if (fadeOutToMap) FindObjectOfType<Fading>().BeginFade(-1);
                else              FindObjectOfType<Fading>().FadeInstant(-1, true);
            initialized = true;
            fadeOutToMap = true;
            eventsLoading = false;
            eventsLoaded = true;
        // If there's a current script, that means the map is already initialized, so we just check if said script's event is done
        } else
            CheckEndEvent();
    }

    /// <summary>
    /// Checks if the current event script is done, and ends the event properly if it is.
    /// </summary>
    private void CheckEndEvent() {
        if (script == null) return;
        Table t = script.script.Globals;
        if (t.Get(DynValue.NewString("CYFEventCoroutine")).Coroutine.State == CoroutineState.Dead && GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0)
            EndEvent();
    }

    /// <summary>
    /// Checks which script will be the target of future event function calls.
    /// Might cause problems, CYFEventCheckRefresh might be skipped for some events given how it's set up.
    /// </summary>
    public void CheckCurrentEvent() {
        // Focus on coroutine events
        for (int count = 0; count < coroutines.Count; count++) {
            ScriptWrapper scr = coroutines.ElementAt(count).Key;
            Table t = scr.script.Globals;
            if (!t.Get(DynValue.NewString("CYFEventCheckRefresh")).Boolean) continue;
            SetCurrentScript(scr);
            t.Set(DynValue.NewString("CYFEventCheckRefresh"), DynValue.NewBoolean(false));
        }
        // Focus on the current non-coroutine event
        if (script == null) return;
        Table t2 = script.script.Globals;
        if (!t2.Get(DynValue.NewString("CYFEventCheckRefresh")).Boolean) return;
        SetCurrentScript(script);
        t2.Set(DynValue.NewString("CYFEventCheckRefresh"), DynValue.NewBoolean(false));
    }

    /// <summary>
    /// Changes the target script of future event function calls.
    /// </summary>
    /// <param name="scr">New target script.</param>
    private void SetCurrentScript(ScriptWrapper scr) {
        luaEventOw.appliedScript = scr;
        luaGeneralOw.appliedScript = scr;
        luaInventoryOw.appliedScript = scr;
        luaPlayerOw.appliedScript = scr;
        luaScreenOw.appliedScript = scr;
        luaMapOw.appliedScript = scr;
    }

    /// <summary>
    /// Called once every frame.
    /// </summary>
    private void Update() {
        try {
            // Executed once on map load
            if (readyToReLaunch && SceneManager.GetActiveScene().name != "TransitionOverworld") {
                readyToReLaunch = false;
                LateStart();
            }
            // Stall the Update function as long as initialization pages have to be run
            if (eventsLoading) {
                AfterLoad();
                return;
            }
            if (!eventsLoaded) return;

            // Update the current non-coroutine event
            CheckCurrentEvent();
            // Remove any invalid event from the events list
            TestEventDestruction();
            // Update coroutine events
            RunCoroutines();

            if (script == null && !ScriptRunning && !PlayerOverworld.instance.inBattleAnim && !PlayerOverworld.instance.menuRunning[2]) {
                // Run an available auto event
                if (TestEventAuto()) return;
                // If the Player pressed the Confirm key, check if it's in range of a button press event
                if (GlobalControls.input.Confirm == ButtonState.PRESSED && !passPressOnce && (GameObject.Find("FadingBlack") == null || GameObject.Find("FadingBlack").GetComponent<Fading>().alpha <= 0)) {
                    RaycastHit2D hit;
                    TestEventPress(PlayerOverworld.instance.lastMove.x, PlayerOverworld.instance.lastMove.y, out hit);
                } else
                    passPressOnce = false;

                if (events.Count != 0)
                    for (int i = 0; i < events.Count; i ++) {
                        GameObject go = events[i];
                        EventOW ev = go.GetComponent<EventOW>();
                        if (ev.actualPage < -1) { }
                        // Remove all events which current event page is exactly -1
                        else if (ev.actualPage == -1) {
                            events.Remove(go);
                            i--;
                            Destroy(go);
                        // Throw an error if an event's current event page doesn't exist
                        } else if (!UnitaleUtil.TestContainsListVector2(ev.eventTriggers, ev.actualPage) && ev.eventTriggers.Count != 0) {
                            UnitaleUtil.DisplayLuaError(ev.name, "The trigger of the page #" + ev.actualPage + " doesn't exist.\nYou'll need to add it via Unity, on this event's EventOW Component.");
                            return;
                        }
                    }
            } else
                passPressOnce = false;
            CheckEndEvent();
        } catch (InvalidOperationException e) { Debug.LogError(e.Message); }
    }

    /// <summary>
    /// Tests if the player activates an event while he was hitting the Confirm button on the map.
    /// </summary>
    /// <param name="xDir">Horizontal direction the Player is looking at.</param>
    /// <param name="yDir">Vertical direction the Player is looking at.</param>
    /// <param name="hit">RaycastHit2D object used to do stuff.</param>
    public bool TestEventPress(float xDir, float yDir, out RaycastHit2D hit) {
        BoxCollider2D boxCollider = GameObject.Find("Player").GetComponent<BoxCollider2D>();
        Transform playerTransform = GameObject.Find("Player").transform;

        // Store the start position to move from, based on the Player's current transform position
        Vector2 start = new Vector2(playerTransform.position.x + playerTransform.localScale.x * boxCollider.offset.x,
                                    playerTransform.position.y + playerTransform.localScale.y * boxCollider.offset.y);

        // Calculate the end position based on the direction parameters passed in when calling Move and using our boxCollider
        Vector2 dir = new Vector2(xDir, yDir);

        // Calculate the current size of the Player's boxCollider
        Vector2 size = new Vector2(boxCollider.size.x * PlayerOverworld.instance.PlayerPos.localScale.x, boxCollider.size.y * PlayerOverworld.instance.PlayerPos.localScale.y);

        // Disable boxCollider so that the line cast doesn't hit the Player's own collider, and disable the non touching events' colliders
        boxCollider.enabled = false;
        foreach (GameObject go in events) {
            if (GetTrigger(go, go.GetComponent<EventOW>().actualPage) > 0 && go.GetComponent<Collider2D>()) {
                go.GetComponent<Collider2D>().enabled = false;
            }
        }

        // Cast a box from start point to end point, checking collision on blockingLayer
        hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(boxCollider.size.x * PlayerOverworld.instance.transform.localScale.x * xDir, 2) +
                                                                Mathf.Pow(boxCollider.size.y * PlayerOverworld.instance.transform.localScale.y * yDir, 2)), _eventLayer);

        // Re-enable the disabled colliders after BoxCast
        boxCollider.enabled = true;
        foreach (GameObject go in events)
            if (GetTrigger(go, go.GetComponent<EventOW>().actualPage) > 0 && go.GetComponent<Collider2D>())
                go.GetComponent<Collider2D>().enabled = true;

        // Execute the event that our cast collided with if there's any
        return hit.collider != null && ExecuteEvent(hit.collider.gameObject);
    }

    /// <summary>
    /// Checks if an auto event page is ready to be executed, and runs it if there's any.
    /// </summary>
    /// <returns>True if an auto event page is run, false otherwise.</returns>
    public bool TestEventAuto() {
        GameObject go1 = null;
        try {
            foreach (GameObject go2 in events) {
                go1 = go2;
                if (GetTrigger(go2, go2.GetComponent<EventOW>().actualPage) != 2) continue;
                if (autoDone.Contains(go2))                                       continue;
                autoDone.Add(go2);
                return ExecuteEvent(go2);
            }

            if (!go1) {
                return false;
            }
        }
        // Catch any Lua exception and display it on screen
        catch (InterpreterException e) {
            UnitaleUtil.DisplayLuaError((go1 != null ? go1.name : "Unknown event") + ", page #" + (go1 != null ? go1.GetComponent<EventOW>().actualPage : 0),
                                        UnitaleUtil.FormatErrorSource(e.DecoratedMessage, e.Message) + e.Message); }
        // Catch any engine exception and display it on screen
        catch (Exception e) {
            UnitaleUtil.DisplayLuaError((go1 != null ? go1.name : "Unknown event") + ", page #" + (go1 != null ? go1.GetComponent<EventOW>().actualPage : 0),
                                        "Unknown error of type " + e.GetType() + ". Please send this to the main dev.\n\n" + e.Message + "\n\n" + e.StackTrace);
        }
        return false;
    }

    /// <summary>
    /// Removes any invalid event or any event ready to be destroyed in the events table .
    /// </summary>
    public void TestEventDestruction() {
        for (int i = 0; i < events.Count; i++) {
            GameObject go = events[i];
            if (!go)                                              events.Remove(go);
            else if (!go.GetComponent<EventOW>())                 events.Remove(go);
            else if (go.GetComponent<EventOW>().actualPage == -1) luaEventOw.Remove(go.name);
            else                                                  i ++;
            i --;
        }
    }

    /// <summary>
    /// Runs all coroutine events one after the other.
    /// </summary>
    private void RunCoroutines() {
        GameObject go1 = null;
        try {
            try {
                // Run each coroutine
                for (int count = 0; count < coroutines.Count; count++) {
                    ScriptWrapper scr = coroutines.ElementAt(count).Key;
                    if (scr == script)
                        continue;
                    GameObject go2 = eventScripts.FirstOrDefault(x => x.Value == scr).Key;
                    go1 = go2;
                    ExecuteEvent(go2, coroutines[scr], true);
                }
            } catch (Exception e) { Debug.LogError(e.Message); }
            // If a coroutine has been deleted, remove the event
            for (int i = events.Count - 1; i >= 0; i--) {
                if (GetTrigger(events[i], events[i].GetComponent<EventOW>().actualPage) != 3 || coroutines.ContainsKey(eventScripts[events[i]]) || eventScripts[events[i]] == script) continue;
                go1 = events[i];
                ExecuteEvent(events[i], -1, true);
            }
        }
        // Catch any Lua exception and display it on screen
        catch (InterpreterException e) { UnitaleUtil.DisplayLuaError((go1 != null ? go1.name : "Unknown event") + ", page #" + (go1 != null ? go1.GetComponent<EventOW>().actualPage : 0), e.DecoratedMessage); }
        // Catch any engine exception and display it on screen
        catch (Exception e) {
            UnitaleUtil.DisplayLuaError((go1 != null ? go1.name : "Unknown event") + ", page #" + (go1 != null ? go1.GetComponent<EventOW>().actualPage : 0),
                                        "Unknown error of type " + e.GetType() + ". Please send this to the main dev.\n\n" + e.Message + "\n\n" + e.StackTrace);
        }
    }

    /// <summary>
    /// Gets an event's given page trigger and returns it.
    /// </summary>
    /// <param name="go">Target GameObject.</param>
    /// <param name="index">Index of the page to get the trigger of.</param>
    /// <returns>The trigger of the event's given page.</returns>
    public int GetTrigger(GameObject go, int index) {
        foreach (Vector2 vec in go.GetComponent<EventOW>().eventTriggers)
            if (System.Math.Abs(vec.x - index) < 0.0001f)
                return (int)vec.y;
        return -2;
    }

    /// <summary>
    /// Resets the events by counting them all again, stopping the current events and destroying all the current images.
    /// </summary>
    /// <param name="resetScripts">Set to true if you want all scripts to be reloaded as well.</param>
    public void ResetEvents(bool resetScripts = true) {
        coroutines.Clear();
        initializedEvents.Clear();
        events.Clear();
        autoDone.Clear();
        // Reset all loaded scripts in order to reload them later
        if (resetScripts) {
            spriteControllers.Clear();
            eventScripts.Clear();
            ScriptWrapper.instances.Clear();
        }
        PlayerOverworld.instance.parallaxes.Clear();
        // Load all events
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Event")) {
            events.Add(go);
            if (go.GetComponent<BoxCollider2D>()) {
                // Create the event's collider automatically if it's not set
                if (go.GetComponent<BoxCollider2D>().size == new Vector2(0, 0))
                    go.GetComponent<BoxCollider2D>().size = new Vector2(go.GetComponent<RectTransform>().sizeDelta.x / go.GetComponent<RectTransform>().localScale.x * 100,
                                                                        go.GetComponent<RectTransform>().sizeDelta.y / go.GetComponent<RectTransform>().localScale.y * 100);
                // Center the event's collider on the y axis if it's offset by -1 in both axis
                // TODO: Pick another value? People might encounter this by accident
                if (go.GetComponent<BoxCollider2D>().offset == new Vector2(-1, -1))
                    go.GetComponent<BoxCollider2D>().offset = new Vector2(0, go.GetComponent<BoxCollider2D>().size.y / 2);
            }

            // Repopulate the spriteControllers table
            spriteControllers[go.name] = LuaSpriteController.GetOrCreate(go);

            // Initialize the event's script if scripts have been reset
            if (resetScripts)
                if (go.GetComponent<EventOW>().scriptToLoad != "none") {
                    string scriptToLoad = go.GetComponent<EventOW>().scriptToLoad;
                    eventScripts.Add(go, InitScript(scriptToLoad, go.GetComponent<EventOW>()));
                }

            // Start the event's animator if it has one
            if (go.GetComponent<CYFAnimator>())
                go.GetComponent<CYFAnimator>().LateStart();
        }

        // Store parallaxes if there are any
        foreach (Transform t in UnitaleUtil.GetFirstChildren(null)) {
            if (!t)            continue;
            if (!t.gameObject) continue;
            if (t.gameObject.name.Contains("Parallax"))
                PlayerOverworld.instance.parallaxes.Add(t);
        }
    }

    /// <summary>
    /// Executes an event page of a given GameObject.
    /// </summary>
    /// <param name="go">The target GameObject.</param>
    /// <param name="page">Set to a given value if you want to start a given event page, otherwise triggers the event's current event page.</param>
    /// <param name="isCoroutine">Defines whether this event page should be run as a coroutine or not.</param>
    /// <returns>True if it was successful, false otherwise.</returns>
    [HideInInspector]
    public bool ExecuteEvent(GameObject go, int page = -1, bool isCoroutine = false) {
        // If there is a script running and the event isn't a coroutine, the event can't be run
        if (script != null && !isCoroutine)
            return false;
        int eventIndex = -1;
        // Retrieve the event's index in the events table
        for (int i = 0; i < events.Count; i++)
            if (events[i].Equals(go)) {
                eventIndex = i;
                break;
            }
        // If the event can't be found in the events table, something's wrong
        if (eventIndex == -1) {
            if (!isCoroutine)
                UnitaleUtil.DisplayLuaError("Overworld engine", "Whoops! There is an error with event indexing.");
            return false;
        }

        // Related to CYF v0.6's secret
        if (UnitaleUtil.IsSpecialAnnouncement(go.name) && page != 0) {
            StartCoroutine(SpecialAnnouncementEvent());
            return true;
        }

        // If the script we have to load exists, let's initialize it and then execute it
        if (!isCoroutine) {
            actualEventIndex = eventIndex;
            PlayerOverworld.instance.PlayerNoMove = true;
            ScriptRunning = true;
        }
        try {
            // Retrieve the script we have to run the event from
            // Special case for CYF v0.6's secret event, which code is hidden in the source code
            var scr = UnitaleUtil.IsSpecialAnnouncement(go.name) ? InitScript(go.name, go.GetComponent<EventOW>()) : eventScripts[go];

            // Add the coroutine to the coroutines table if it's not in it
            if (isCoroutine && !coroutines.ContainsKey(scr)) coroutines.Add(scr, go.GetComponent<EventOW>().actualPage);
            // Update the current script if the event isn't a coroutine
            else if (!isCoroutine)                           script = scr;

            // Focus all overworld function objects on the script we retrieved, then call the event's current or given page
            SetCurrentScript(scr);
            scr.Call("CYFEventStartEvent", DynValue.NewString("EventPage" + (page == -1 ? go.GetComponent<EventOW>().actualPage : page)));

        // Catch any Lua exception and display it on screen
        } catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(go.GetComponent<EventOW>().scriptToLoad, ex.DecoratedMessage ?? ex.Message);
            return false;
        // Catch any engine exception and display it on screen
        } catch (Exception ex) {
            UnitaleUtil.DisplayLuaError(go.GetComponent<EventOW>().scriptToLoad, ex.Message);
            return false;
        }
        // If the current script is not a coroutine, prevent the Confirm key press from doing any other action and set the text manager up
        if (isCoroutine) return true;
        _textManager.SetCaller(script);
        _textManager.transform.parent.parent.SetAsLastSibling();
        passPressOnce = true;
        return true;
    }

    /// <summary>
    /// Retrieves all functions from all event function objects and return them as strings.
    /// </summary>
    /// <param name="t">Event function object to parse.</param>
    /// <returns>A list of strings with the names of all functions of this object.</returns>
    private IEnumerable<string> CreateBindListMember(Type t) {
        MethodInfo[] methods = t.GetMethods();
        return (from method in methods where MethodHasCyfEventFunctionAttribute(method) select method.Name).ToList();
    }

    /// <summary>
    /// Checks if the given member (function) has the CYFEventFunction attribute.
    /// </summary>
    /// <param name="mb">Function to check.</param>
    /// <returns>True if the function has the CYFEventFunction attribute, false otherwise.</returns>
    private static bool MethodHasCyfEventFunctionAttribute(MemberInfo mb) {
        const bool includeInherited = false;
        return mb.GetCustomAttributes(typeof(CYFEventFunction), includeInherited).Any();
    }

    /// <summary>
    /// Generates the internal Lua code used by CYF events.
    /// </summary>
    private void GenerateEventCode() {
        _eventCodeFirst = string.Empty;
        // Parse all event function objects
        foreach (Type t in _boundValueName.Keys) {
            IEnumerable<string> members = CreateBindListMember(t);
            _eventCodeFirst += "\n" + _boundValueName[t] + " = {";
            // Get all the functions of the current event function object and add it to the event script's prefix
            foreach (string member in members)
                // Store all functions as a call to CYFEventForwarder
                _eventCodeFirst += "\n    " + member + " = function(...) CYFEventLastAction = '" + _boundValueName[t] + "." + member + "' return CYFEventForwarder(F" + _boundValueName[t] + "." + member + ", ...) end,";
            // Throw an error if the user tries to add or get a value from the new, table-based object
            _eventCodeFirst += "\n}\nsetmetatable(" + _boundValueName[t] + @", {
    __index = function(t, k)
        error(""cannot access field "" .. tostring(k) .. "" of userdata <" + t + @">"", 2)
    end,
    __newindex = function(t, k)
        error(""cannot access field "" .. tostring(k) .. "" of userdata <" + t + @">"", 2)
    end
})";
        }
        _eventCodeFirst += @"
CYFEventCoroutine = coroutine.create(DEBUG) -- Coroutine for the current event script
CYFEventCheckRefresh = true                 -- Variable checked for by the C# scripts
CYFEventLastAction = """"                   -- Checked for in a workaround for General.Wait
local CYFEventAlreadyLaunched = false

-- Function to read regular Lua error messages and make them say 'line' and 'char' for readability
local errorPattern = ':%([%d%-,]+%):'
function CYFFormatError(err)
    local code = err:match(errorPattern)
    if code then
        local before = err:sub(0, err:find(errorPattern) + (code:sub(0, 2) == ':(' and 1 or 0))
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

-- Function called by the coroutine created in CYFEventStartEvent
function CYFEventFuncToLaunch(x)
    if _internalScriptName == nil then
        _internalScriptName = '";
        _eventCodeLast = @"'
    end
    local err
    if not xpcall(x, function(err2) err = err2 end) then
        error(CYFFormatError(err), 0)
    end
end

-- Signals the end of an asynchronous event on the C# side and resumes the coroutine on the Lua side
function CYFEventNextCommand()
    CYFEventAlreadyLaunched = true
    if tostring(coroutine.status(CYFEventCoroutine)) == 'suspended' then
        local ok, errorMsg = coroutine.resume(CYFEventCoroutine)
        if not ok then error(errorMsg, 0) end
    end
end

-- Currently unused
function CYFEventStopCommand() coroutine.yield() end

-- Called whenever activating an overworld Event object's event pages; Creates a coroutine that runs the matching EventPage function
function CYFEventStartEvent(func)
    if _G[func] == nil then error(""error in script "" .. _internalScriptName .. ""\n\nThe function "" .. func .. "" doesn't exist in the Event script."", 0) end
    CYFEventCoroutine = coroutine.create(function() CYFEventFuncToLaunch(_G[func]) end)
    local ok, errorMsg = coroutine.resume(CYFEventCoroutine)
    if not ok then error(errorMsg, 0) end
end

-- Function called by the fake table-based overworld objects, which runs necessary code for the C# side before running the real function (like General.Wait)
function CYFEventForwarder(func, ...)
    CYFEventAlreadyLaunched = false
    CYFEventCheckRefresh = true
    FGeneral.HiddenReloadAppliedScript()
    local ok
    local result
    local hasArgs = false
    for k, v in pairs(({...})) do
        hasArgs = true
        ok, result = pcall(func, ...)
        break
    end
    if not hasArgs then
        ok, result = pcall(func)
    end
    if not ok then
        error(CYFFormatError(result), 0)
    end
    if not CYFEventAlreadyLaunched then coroutine.yield() end
    return result
end";
    }

    // Handles 6 arguments for CreateText()
    private delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg, T6 arg6);

    /// <summary>
    /// Initializes event scripts so they can be used later.
    /// </summary>
    /// <param name="eventName">Name of the event script.</param>
    /// <param name="ev">Component of the event GameObject, it's best to use an EventOW.</param>
    /// <returns>A new script wrapper allowing the user to run script functions.</returns>
    private ScriptWrapper InitScript(string eventName, Component ev) {
        // Create a ScriptWrapper object
        ScriptWrapper scr = new ScriptWrapper { scriptname = eventName };
        // Load a special script hidden within CYF's internals if we're loading CYF 0.6.5's secret
        string scriptText = UnitaleUtil.IsSpecialAnnouncement(eventName) ? CYF_RELEASE_SCRIPT : FileLoader.GetScript("Events/" + eventName, "Loading an event", "event");
        if (scriptText == null) {
            UnitaleUtil.DisplayLuaError("Launching an event", "The event \"" + eventName + "\" doesn't exist.");
            return null;
        }

        // Run engine-provided Lua code for Event scripts (generate it if needed)
        if (_eventCodeFirst == null)
            GenerateEventCode();
        try { scr.script.DoString(_eventCodeFirst + ev.gameObject.name + _eventCodeLast, null, "CYF internal event code (please report!)"); }
        // Catch any Lua exception and display it on screen
        catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(eventName, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            return null;
            // Catch any engine exception and display it on screen
        } catch (Exception ex) {
            UnitaleUtil.DisplayLuaError(eventName, ex.Message);
            return null;
        }

        // Add a few useful functions to event scripts
        scr.script.Globals["CreateLayer"] = (Func<string, string, bool, bool>) SpriteUtil.CreateLayer;
        scr.script.Globals["CreateSprite"] = (Func<string, string, int, DynValue>) SpriteUtil.MakeIngameSprite;
        scr.script.Globals["CreateText"] = (Func<Script, DynValue, DynValue, int, string, int, LuaTextManager>) LuaScriptBinder.CreateText;

        // Actually execute the loaded script
        try { scr.DoString(scriptText); }
        // Catch any Lua exception and display it on screen
        catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(eventName, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            return null;
        }
        // Catch any engine exception and display it on screen
        catch (Exception ex) {
            UnitaleUtil.DisplayLuaError(eventName, ex.Message);
            return null;
        }

        return scr;
    }

    /// <summary>
    /// Used in SetChoice, moves the soul sprite during a selection.
    /// </summary>
    /// <param name="selection">Soul's position</param>
    /// <param name="question">Defines whether there's a question or not.</param>
    /// <param name="threeLines">True if the question spans over three lines.</param>
    public void SetPlayerOnSelection(int selection, bool question = false, bool threeLines = false) {
        if (question) {
            if (threeLines)  selection += 2;
            else             selection += 4;
        }

        int xMv = selection % _textManager.columnNumber;
        int yMv = selection / _textManager.columnNumber;

        if (_textManager.letters.Count > 0)
            GameObject.Find("tempHeart").GetComponent<RectTransform>().position =
                new Vector3(_textManager.letters[0].image.transform.position.x + xMv * _textManager.columnShift,
                            _textManager.letters[0].image.transform.position.y - yMv * _textManager.font.LineSpacing + 9,
                            GameObject.Find("tempHeart").GetComponent<RectTransform>().position.z);
    }

    /// <summary>
    /// Computes the direction of a given vector following a computer's numeric pad. (2: Down, 4: Left, 6: Right, 8: Up, 0: Zero)
    /// TODO: Change this function so it doesn't follow a numeric pad anymore.
    /// </summary>
    /// <param name="dir">Vector to analyze.</param>
    /// <returns>Return the direction of the vector following a computer's numeric pad. (2: Down, 4: Left, 6: Right, 8: Up, 0: Zero)</returns>
    public int CheckDirection(Vector2 dir) {
        // Case x = y = 0
        if (dir == Vector2.zero)
            return 0;

        // Case x = 0
        if (dir.x == 0)
            return dir.y > 0 ? 8 : 2;
        // Case y = 0
        if (dir.y == 0)
            return dir.x > 0 ? 6 : 4;

        // Compare both magnitudes
        float tempDir = dir.y / dir.x;
        // Case y magnitude > x magnitude
        if (tempDir > 1 || tempDir < -1)
            return dir.y > 0 ? 8 : 2;
        // Case x magnitude > y magnitude
        return dir.x > 0 ? 6 : 4;
    }

    /// <summary>
    /// Used to store various data when saving the game or unloading the map.
    /// </summary>
    /// <param name="addPlayer">True if you want to save the Player's data in this map as well.</param>
    public void SetEventStates(bool addPlayer = false) {
        string id = SceneManager.GetActiveScene().name;
        EventOW[] eventOws = (EventOW[])FindObjectsOfType(typeof(EventOW));

        // Create or retrieve a MapData object to store all of this map's data
        GameState.MapData mapInfos = GlobalControls.GameMapData.ContainsKey(id) ? GlobalControls.GameMapData[id] : new GameState.MapData();

        // Remove this map from the current save data if it exists
        if (GlobalControls.GameMapData.ContainsKey(id))
            GlobalControls.GameMapData.Remove(id);

        // Store this map's data
        MapInfos mi = FindObjectOfType<MapInfos>();
        mapInfos.Name = SceneManager.GetActiveScene().name;
        mapInfos.Music = mi.music;
        mapInfos.ModToLoad = mi.modToLoad;
        mapInfos.MusicKept = mi.isMusicKeptBetweenBattles;
        mapInfos.NoRandomEncounter = mi.noRandomEncounter;

        // Copy the data of each event which has been saved before
        Dictionary<string, GameState.EventInfos> eis = new Dictionary<string, GameState.EventInfos>();
        foreach (string str in GlobalControls.EventData.Keys)
            eis.Add(str, GlobalControls.EventData[str]);

        // Add the Player's data to the map if it has been requested
        if (addPlayer) {
            GameState.EventInfos eiPlayer = new GameState.EventInfos {
                CurrPage = 0,
                CurrSpriteNameOrCYFAnim = GameObject.Find("Player").GetComponent<CYFAnimator>().specialHeader,
                NoCollision = false,
                Anchor = UnitaleUtil.VectorToVect(GameObject.Find("Player").GetComponent<RectTransform>().anchorMax),
                Pivot = UnitaleUtil.VectorToVect(GameObject.Find("Player").GetComponent<RectTransform>().pivot)
            };
            eis.Add("Player", eiPlayer);
        }

        foreach (EventOW ev in eventOws) {
            // Only save events linked to scripts
            if (ev.name.Contains("Image") || ev.name.Contains("Tone"))
                continue;
            // If this event has been saved before, remove it from the save file in order to overwrite the event's data
            if (eis.ContainsKey(ev.name))
                eis.Remove(ev.name);
            try {
                // Fill in the event's data
                GameState.EventInfos ei = new GameState.EventInfos {
                    CurrPage = ev.actualPage,
                    CurrSpriteNameOrCYFAnim = ev.GetComponent<CYFAnimator>()
                        ? ev.GetComponent<CYFAnimator>().specialHeader
                        : spriteControllers[ev.name].spritename != "empty"
                            ? spriteControllers[ev.name].spritename
                            : instance.spriteControllers[ev.name].img.GetComponent<SpriteRenderer>()
                                ? instance.spriteControllers[ev.name].img.GetComponent<SpriteRenderer>().sprite.name
                                : instance.spriteControllers[ev.name].img.GetComponent<Image>().sprite.name,
                    NoCollision = ev.gameObject.layer == 0,
                    Anchor = UnitaleUtil.VectorToVect(ev.GetComponent<RectTransform>().anchorMax),
                    Pivot = UnitaleUtil.VectorToVect(ev.GetComponent<RectTransform>().pivot)
                };
                // Add it to the saved events dictionary
                eis.Add(ev.name, ei);
            } catch { /* ignored */ }
        }
        // Store all of the map's data in the current save data object
        mapInfos.EventInfo = eis;
        GlobalControls.GameMapData.Add(id, mapInfos);
        //MapDataParser();
        // Clear all sprites
        spriteControllers.Clear();
    }

    /// <summary>
    /// Tries to change a map value.
    /// </summary>
    /// <param name="mapName">Name of the target map.</param>
    /// <param name="var">Name of the variable to change.</param>
    /// <param name="val">New value of the target variable.</param>
    public static void TrySetMapValue(string mapName, string var, object val) {
        var = var.ToLower();
        // Stop the function if the value we're trying to change doesn't exist
        if (var != "music" && var != "modtoload" && var != "musickept" && var != "norandomencounter")
            throw new CYFException("You tried to change a map's \"" + var + "\" value but it doesn't exist.\nYou can only choose between \"Music\", \"ModToLoad\", \"MusicKept\" and \"NoRandomEncounter\".");
        // MusicKept and NoRandomEncounter are both booleans so we check for their value
        if (var == "musickept" || var == "norandomencounter") {
            switch (val.ToString().ToLower()) {
                case "true":  val = true;  break;
                case "false": val = false; break;
                default:
                    throw new CYFException("\"MusicKept\" and \"NoRandomEncounter\" are boolean values. You can only enter \"true\" or \"false\".");
            }
        }

        // Update the value in the saved map dictionary
        foreach (KeyValuePair<string, GameState.MapData> kvp in GlobalControls.GameMapData) {
            if (kvp.Value.Name != mapName) continue;

            // Get the right map
            GameState.MapData mapData = kvp.Value;
            GlobalControls.GameMapData.Remove(kvp.Key);

            // Change the right value and update the map
            switch (var) {
                case "music":     mapData.Music = val.ToString();        break;
                case "modtoload": mapData.ModToLoad = val.ToString();    break;
                case "musickept": mapData.MusicKept = (bool)val;         break;
                default:          mapData.NoRandomEncounter = (bool)val; break;
            }
            GlobalControls.GameMapData.Add(kvp.Key, mapData);
            return;
        }

        GameState.TempMapData tmi = new GameState.TempMapData();
        bool found = false;
        // Update the value in the map changes dictionary if it exists
        foreach (KeyValuePair<string, GameState.TempMapData> kvp in GlobalControls.TempGameMapData) {
            if (kvp.Key != mapName) continue;
            // Get the right map
            tmi = kvp.Value;
            found = true;
            break;
        }

        // If this map isn't in the map changes dictionary, add it
        if (!found) {
            tmi = new GameState.TempMapData {
                MusicChanged = false,
                ModToLoadChanged = false,
                MusicKeptChanged = false,
                NoRandomEncounterChanged = false
            };
        }

        switch (var) {
            // Change the right value and update
            case "music":
                tmi.Music = val.ToString();
                tmi.MusicChanged = true;
                break;
            case "modtoload":
                tmi.ModToLoad = val.ToString();
                tmi.ModToLoadChanged = true;
                break;
            case "musickept":
                tmi.MusicKept = (bool)val;
                tmi.MusicKeptChanged = true;
                break;
            default:
                tmi.NoRandomEncounter = (bool)val;
                tmi.NoRandomEncounterChanged = true;
                break;
        }
        GlobalControls.TempGameMapData.Add(mapName, tmi);
    }

    /// <summary>
    /// Tries to get a map value.
    /// </summary>
    /// <param name="mapName">Name of the target map.</param>
    /// <param name="var">Name of the variable to change.</param>
    /// <returns>Value of the target variable.</returns>
    public static object TryGetMapValue(string mapName, string var) {
        var = var.ToLower();
        // Get the value in the current save if the Player visited that map
        foreach (GameState.MapData md in GlobalControls.GameMapData.Values)
            if (md.Name == mapName)
                switch (var) {
                    case "music":     return md.Music;
                    case "modtoload": return md.ModToLoad;
                    case "musickept": return md.MusicKept;
                    default:          return md.NoRandomEncounter;
                }

        // Get the value in the current map changes dictionary if the map was changed at some point
        foreach (GameState.TempMapData tmd in GlobalControls.TempGameMapData.Values)
            if (tmd.Name == mapName)
                switch (var) {
                    case "music":     return tmd.Music;
                    case "modtoload": return tmd.ModToLoad;
                    case "musickept": return tmd.MusicKept;
                    default:          return tmd.NoRandomEncounter;
                }

        int buildIndex = -1;
        // Try to get the value from the unvisited map's scene if it exists
        // Test whether the map exists or not
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            if (mapName == Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i))) {
                buildIndex = i;
                break;
            }
            if (i == SceneManager.sceneCountInBuildSettings - 1)
                throw new CYFException("The scene \"" + mapName + "\" doesn't exist.\nYou must enter the scene's file name, not its alias.");
        }

        // Retrieve the value from the unvisited scene
        foreach (GameObject go in SceneManager.GetSceneByBuildIndex(buildIndex).GetRootGameObjects()) {
            if (!go.GetComponent<MapInfos>()) continue;
            switch (var) {
                case "music":     return go.GetComponent<MapInfos>().music;
                case "modtoload": return go.GetComponent<MapInfos>().modToLoad;
                case "musickept": return go.GetComponent<MapInfos>().isMusicKeptBetweenBattles;
                default:          return go.GetComponent<MapInfos>().noRandomEncounter;
            }
        }
        return null;
    }

    /// <summary>
    /// Debug function used to print all data saved for a map.
    /// </summary>
    public static void MapDataParser() {
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

    /// <summary>
    /// Copies the data saved for a map and apply it to the current map.
    /// </summary>
    /// <param name="mi">Map object to update.</param>
    /// <param name="id">Name of the map to retrieve data from.</param>
    public static void GetMapState(MapInfos mi, string id) {
        // Retrieve data from the map changes dictionary if it's the first time the Player enters it
        if (!GlobalControls.GameMapData.ContainsKey(id)) {
            if (!GlobalControls.TempGameMapData.ContainsKey(SceneManager.GetActiveScene().name)) return;
            GameState.TempMapData tmd = GlobalControls.TempGameMapData[SceneManager.GetActiveScene().name];
            GlobalControls.TempGameMapData.Remove(SceneManager.GetActiveScene().name);
            if (tmd.MusicChanged)             mi.music = tmd.Music;
            if (tmd.ModToLoadChanged)         mi.modToLoad = tmd.ModToLoad;
            if (tmd.MusicKeptChanged)         mi.isMusicKeptBetweenBattles = tmd.MusicKept;
            if (tmd.NoRandomEncounterChanged) mi.noRandomEncounter = tmd.NoRandomEncounter;
            return;
        }

        // Fill in the MapInfos object from the saved data for this map
        GameState.MapData miSave = GlobalControls.GameMapData[id];
        mi.music = miSave.Music;
        mi.modToLoad = miSave.ModToLoad;
        mi.isMusicKeptBetweenBattles = miSave.MusicKept;
        mi.noRandomEncounter = miSave.NoRandomEncounter;

        // Copy the data for each event into the given MapInfos
        foreach (string str in miSave.EventInfo.Keys) {
            try {
                GameObject go = GameObject.Find(str);
                // If the event doesn't exist, go to the next event
                if (go == null)
                    continue;

                GameState.EventInfos ei = miSave.EventInfo[str];
                // Fill in Player or event-specific data
                if (str == "Player")
                    go.GetComponent<CYFAnimator>().specialHeader = ei.CurrSpriteNameOrCYFAnim;
                else {
                    EventOW ev = go.GetComponent<EventOW>();
                    if (!ev)
                        continue;

                    ev.actualPage = ei.CurrPage;
                    ev.gameObject.layer = ei.NoCollision ? 0 : 21;

                    try {
                        // Sets data to the event's animator if it exists
                        if (ev.GetComponent<CYFAnimator>())
                            ev.GetComponent<CYFAnimator>().specialHeader = ei.CurrSpriteNameOrCYFAnim;
                        // Sets the event's sprite using auto-load if it exists
                        else if (ev.GetComponent<AutoloadResourcesFromRegistry>())
                            ev.GetComponent<AutoloadResourcesFromRegistry>().SpritePath = ei.CurrSpriteNameOrCYFAnim;
                        // Sets the event's sprite directly otherwise
                        else if (ev.GetComponent<Image>())
                            ev.GetComponent<Image>().sprite = SpriteRegistry.Get(ei.CurrSpriteNameOrCYFAnim);
                        else ev.GetComponent<SpriteRenderer>().sprite = SpriteRegistry.Get(ei.CurrSpriteNameOrCYFAnim);
                    } catch {
                        Debug.LogWarning("Map loading: Couldn't load sprite " + ei.CurrSpriteNameOrCYFAnim + " for object " + ev.name);
                        // ignored
                    }
                }
                go.GetComponent<RectTransform>().anchorMax = UnitaleUtil.VectToVector(ei.Anchor);
                go.GetComponent<RectTransform>().anchorMin = UnitaleUtil.VectToVector(ei.Anchor);
                go.GetComponent<RectTransform>().pivot = UnitaleUtil.VectToVector(ei.Pivot);
            } catch (Exception e) { Debug.LogError(e); }
        }
    }

    /// <summary>
    /// Ends the current event.
    /// </summary>
    public void EndEvent() {
        PlayerOverworld.instance.textmgr.SetTextFrameAlpha(0);
        PlayerOverworld.instance.textmgr.textQueue = new TextMessage[] { };
        PlayerOverworld.instance.textmgr.DestroyChars();
        PlayerOverworld.instance.PlayerNoMove = false;
        PlayerOverworld.instance.UIPos = 0;
        ScriptRunning = false;
        script = null;
    }

    /// <summary>
    /// Starts a coroutine triggered by a standard overworld function.
    /// </summary>
    /// <param name="coroutineName">Name of the coroutine to start.</param>
    /// <param name="args">Arguments of the coroutine.</param>
    /// <param name="evName">Name of the event calling this coroutine.</param>
    public void StCoroutine(string coroutineName, object args, string evName) {
        // Create the key that will be stored in the cSharpCoroutines table
        string key = evName + "." + coroutineName;
        // End this coroutine if this event is already running it
        ForceEndCoroutine(key);
        Coroutine newCoroutine;
        // Create the coroutine by itself, passing arguments if needed
        if (args == null)                 newCoroutine = StartCoroutine(coroutineName);
        else if (!args.GetType().IsArray) newCoroutine = StartCoroutine(coroutineName, args);
        else                              newCoroutine = StartCoroutine(coroutineName, (object[])args);
        cSharpCoroutines.Add(key, newCoroutine);
    }

    /// <summary>
    /// Stops a coroutine immediately.
    /// </summary>
    /// <param name="key">Key of the coroutine as it is stored in the cSharpCoroutines table.</param>
    public void ForceEndCoroutine(string key) {
        // Stops the coroutine if it exists and remove it from the table
        if (!cSharpCoroutines.ContainsKey(key)) return;
        Coroutine existingCoroutine;
        cSharpCoroutines.TryGetValue(key, out existingCoroutine);
        if (existingCoroutine != null)
            StopCoroutine(existingCoroutine);
        cSharpCoroutines.Remove(key);
    }

    /// <summary>
    /// Hidden C# event script starting the sequence related to CYF v0.6's secret.
    /// Don't tell anyone it's an actual event script function!
    /// </summary>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator SpecialAnnouncementEvent() {
        // Prevents the Player from moving
        luaPlayerOw.CanMove(false);
        // This very weird RealGlobal is used in order to know if we should delete the event spawning CYF v0.6's secret
        // Makes the event disappear the next time it's encountered
        LuaScriptBinder.Set(null, "1a6377e26b5119334e651552be9f17f8d92e83c9", DynValue.NewBoolean(false));

        // Get all sprites from the Resources/Sprites folder
        Sprite[] resourceSprites = Resources.LoadAll<Sprite>("Sprites");
        Dictionary<string, Sprite> sprites = resourceSprites.ToDictionary(spr => spr.name);
        // Get all audio files from the Resources/Audios folder
        AudioClip[] resourceAudios = Resources.LoadAll<AudioClip>("Audios");
        Dictionary<string, AudioClip> audios = resourceAudios.ToDictionary(audioClip => audioClip.name);

        // Retrieve the event object related to CYF v0.6's secret
        GameObject go = GameObject.Find("4eab1af3ab6a932c23b3cdb8ef618b1af9c02088");
        // Set the event's sprite to mm2
        go.GetComponent<SpriteRenderer>().sprite = sprites["mm2"];
        // Make the sprite more visible
        go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
        // Stop all audios
        NewMusicManager.StopAll();
        // Play the sound 4eab1af3ab6a932c23b3cdb8ef618b1af9c02088
        AudioSource audioSource = NewMusicManager.CreateChannelAndGetAudioSource("4eab1af3ab6a932c23b3cdb8ef618b1af9c02088");
        audioSource.loop = false;
        audioSource.clip = audios["sound"];
        audioSource.Play();
        // Wait until the audio's done playing
        while (audioSource.isPlaying)
            yield return 0;
        // Destroy the overworld, and enter CYF v0.6's secret scene
        NewMusicManager.DestroyChannel("4eab1af3ab6a932c23b3cdb8ef618b1af9c02088");
        SceneManager.LoadScene("SpecialAnnouncement");
        Destroy(GameObject.Find("Player"));
        Destroy(GameObject.Find("Canvas OW"));
        Destroy(GameObject.Find("Canvas Two"));
        Destroy(GameObject.Find("Main Camera OW"));
    }

    /// <summary>
    /// Hidden event initialization function used for the event related to CYF v0.6's secret.
    /// </summary>
    private const string CYF_RELEASE_SCRIPT = "function EventPage0()\n" +
                                              "    if not GetRealGlobal(\"1a6377e26b5119334e651552be9f17f8d92e83c9\") then\n" +
                                              "        Event.Remove(Event.GetName())\n" +
                                              "    end\n" +
                                              "end\n" +
                                              "function EventPage1() end";

    //-----------------------------------------------------------------------------------------------------------
    //                                        ---   Lua Functions   ---
    //
    //                All event commands have to be finished with script.Call("CYFEventNextCommand");
    //               If you need to return a value to the lua script, try to use try the finally block.
    //
    //                    Plus, if you want to create functions, test first if the GameObject the
    //                     player is accessing to is an event: if you don't, you'll be a really
    //                             bad person and you'll go to hell. Don't ask why tho.
    //                          (aka the function can cause errors and it'll be your fault)
    //-----------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Gives the Player a choice, the result will be stored in the variable 'lastChoice' in the event script after the choice is done.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * bool question - True if you want a question to be shown, false otherwise.
    /// * bool oneLiners - Two booleans in a table defining whether the two options are one line long or not.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator ISetChoice(object[] args) {
        ScriptWrapper scr = luaGeneralOw.appliedScript;

        // Retrieve all arguments
        bool question;
        bool[] oneLiners;
        try { question = (bool)args[0]; }   catch { throw new CYFException("The argument \"question\" must be a boolean."); }
        try { oneLiners = (bool[])args[1]; } catch { throw new CYFException("The argument \"oneLiners\" must be a boolean table."); }

        // Stop if this function is used in a coroutine
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.SetChoice: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
            yield break;
        }
        // Stop if this function is used in the initialization page of an event
        if (eventsLoading) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.SetChoice: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
            yield break;
        }

        // Wait for the message to be displayed
        while (!_textManager.LineComplete())
            yield return 0;

        int actualChoice = 0;

        // Spawn the choice soul and move it in front of the first (left) option
        GameObject tempHeart = new GameObject("tempHeart", typeof(RectTransform));
        tempHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(16, 16);
        tempHeart.transform.SetParent(GameObject.Find("Canvas OW").transform);
        Image img = tempHeart.AddComponent<Image>();
        img.sprite = PlayerOverworld.instance.utHeart.sprite;
        img.color = new Color(1, 0, 0, 1);
        SetPlayerOnSelection(actualChoice, question, !oneLiners[0]);

        // Main loop of the choice dialogue
        while (true) {
            int xMov = GlobalControls.input.Right == ButtonState.PRESSED ? 1 : GlobalControls.input.Left == ButtonState.PRESSED ? -1 : 0;
            // Move the soul in front of the current selected option if one of the Left or Right keys are pressed
            if (xMov != 0) {
                actualChoice = UnitaleUtil.SelectionChoice(2, actualChoice, xMov, 0, 1, 2, false);
                SetPlayerOnSelection(actualChoice, question, !oneLiners[actualChoice]);
            // Confirm the selected option if a Confirm key is pressed
            } else if (GlobalControls.input.Confirm == ButtonState.PRESSED)
                if (!_textManager.LineComplete() && _textManager.CanSkip())
                    _textManager.SkipLine();
                else
                    break;
            yield return 0;
        }
        // Sets the value of the variable 'lastChoice' in the current script to 0 if the left option was picked or to 1 otherwise
        script.script.Globals.Set(DynValue.NewString("lastChoice"), DynValue.NewNumber(actualChoice));
        Destroy(tempHeart);
        yield return 0;
    }

    /// <summary>
    /// Moves a given event to given coordinates.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * string eventName - The name of the event we're moving.
    /// * float dirX - The end x position of the event starting from the bottom left corner of the map.
    /// * float dirY - The end y position of the event starting from the bottom left corner of the map.
    /// * bool wallPass - True if the event can walk through walls, false otherwise.
    /// * bool waitEnd - True if the script has to wait for the end of the coroutine, false otherwise.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IMoveEventToPoint(object[] args) { //NEED PARENTAL REMOVE
        ScriptWrapper scr = (ScriptWrapper)args[0];

        // Retrieve all arguments
        string eventName;
        float dirX, dirY;
        bool wallPass, waitEnd;
        try { eventName = (string)args[1];   } catch { throw new CYFException("The argument \"name\" must be a string."); }
        try { dirX = (float)args[2];    } catch { throw new CYFException("The argument \"dirX\" must be a number."); }
        try { dirY = (float)args[3];    } catch { throw new CYFException("The argument \"dirY\" must be a number."); }
        try { wallPass = (bool)args[4]; } catch { throw new CYFException("The argument \"wallPass\" must be a boolean."); }
        try { waitEnd = (bool)args[5];  } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.MoveToPoint: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
            } else if (eventsLoading) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.MoveToPoint: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        for (int i = 0; i < events.Count || eventName == "Player"; i++)
            // Get the right event or the Player
            if (eventName == events[i].name || eventName == "Player") {
                GameObject go = eventName == "Player" ? GameObject.Find("Player") : events[i];
                // For usual events, we must move its parent, SpritePivot, instead of itself
                Transform target = null;
                if (go.transform.parent != null)
                    if (go.transform.parent.name == "SpritePivot")
                        target = go.transform.parent;
                target = target ?? go.transform;

                // Check if the target is already being moved in another MoveEventToPoint call
                ScriptWrapper isMovingSource = eventName == "Player" ? go.GetComponent<PlayerOverworld>().isMovingSource : go.GetComponent<EventOW>().isMovingSource;
                Vector2 roundedPos = new Vector2(Mathf.Round(target.position.x * 1000) / 1000, Mathf.Round(target.position.y * 1000) / 1000);
                Vector2 roundedEnd = new Vector2(Mathf.Round(dirX              * 1000) / 1000, Mathf.Round(dirY              * 1000) / 1000);
                // If it's where it should be, stop the target's movement
                if (roundedPos == roundedEnd)
                    if (eventName == "Player")
                        go.GetComponent<PlayerOverworld>().isMovingSource = null;
                    else
                        go.GetComponent<EventOW>().isMovingSource = null;
                // Properly end the current call of this coroutine if the target is already being moved
                if (isMovingSource != null && isMovingSource != scr && (eventName == "Player" ? go.GetComponent<PlayerOverworld>().isMovingWaitEnd : go.GetComponent<EventOW>().isMovingWaitEnd))
                    isMovingSource.Call("CYFEventNextCommand");
                // Update some values related to movement
                if (eventName == "Player") {
                    go.GetComponent<PlayerOverworld>().isMovingSource = null;
                    go.GetComponent<PlayerOverworld>().isMovingWaitEnd = waitEnd;
                } else {
                    go.GetComponent<EventOW>().isMovingSource = null;
                    go.GetComponent<EventOW>().isMovingWaitEnd = waitEnd;
                }

                // If we're not waiting for the end of function, go to the next line
                if (!waitEnd)
                    scr.Call("CYFEventNextCommand");

                // Store the target's initial position
                Vector2 originalPosition = new Vector2(target.position.x, target.position.y);
                // Store the distance between the target's initial and final position
                Vector2 endPoint = new Vector2(dirX - target.position.x, dirY - target.position.y);

                // The animation process is automatic, if you renamed the Animation's triggers and animations as the Player's
                if (go.GetComponent<CYFAnimator>()) {
                    int direction = CheckDirection(endPoint);
                    go.GetComponent<CYFAnimator>().movementDirection = direction;
                }

                while (true) {
                    Vector2 clamped = Vector2.ClampMagnitude(endPoint, 1);

                    // Try to move the target by 1 pixel
                    var moveTest = PlayerOverworld.instance.AttemptMove(clamped.x, clamped.y, go, wallPass);
                    // Increment the distance from the initial position
                    var distanceFromStart = new Vector2(target.position.x - originalPosition.x, target.position.y - originalPosition.y);

                    // Update some values related to movement
                    if (eventName == "Player") {
                        go.GetComponent<PlayerOverworld>().isBeingMoved    = (moveTest || wallPass) && distanceFromStart.magnitude > 0;
                        go.GetComponent<PlayerOverworld>().isMoving        = (moveTest || wallPass) && distanceFromStart.magnitude > 0;
                        go.GetComponent<PlayerOverworld>().isMovingSource  = scr;
                        go.GetComponent<PlayerOverworld>().isMovingWaitEnd = waitEnd;
                    } else {
                        go.GetComponent<EventOW>().isMovingSource  = ((moveTest || wallPass) && distanceFromStart.magnitude > 0) ? scr : null;
                        go.GetComponent<EventOW>().isMovingWaitEnd = waitEnd;
                    }

                    // If the distance between the initial position of the target and its current position is greater than
                    // the distance between the initial position of the target and its final position, the movement is done
                    // If the target collides with something and can't go through walls, end the movement
                    if (distanceFromStart.magnitude >= endPoint.magnitude || (!moveTest && !wallPass)) {
                        // Move the target to its final position if it reached it
                        if (distanceFromStart.magnitude >= endPoint.magnitude)
                            target.position = new Vector3(dirX, dirY, target.position.z);

                        // Update some values related to movement
                        if (eventName == "Player") {
                            go.GetComponent<PlayerOverworld>().isBeingMoved    = false;
                            go.GetComponent<PlayerOverworld>().isMoving        = false;
                            go.GetComponent<PlayerOverworld>().isMovingSource  = null;
                            go.GetComponent<PlayerOverworld>().isMovingWaitEnd = false;
                        } else {
                            go.GetComponent<EventOW>().isMovingSource  = null;
                            go.GetComponent<EventOW>().isMovingWaitEnd = false;
                        }

                        // If we're waiting for the end of function, go to the next line
                        if (waitEnd)
                            scr.Call("CYFEventNextCommand");
                        yield break;
                    }
                    yield return 0;

                    // If the target doesn't exist anymore, exit the function right now
                    if (go != null) continue;
                    scr.Call("CYFEventNextCommand");
                    yield break;
                }
            }
        // If the event can't be found, throw a warning in CYF's console
        UnitaleUtil.WriteInLogAndDebugger("Event.MoveToPoint: The name you entered in the function doesn't exist. Did you forget to add the 'Event' tag?");
        scr.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Wait for the Player to press a Confirm key.
    /// </summary>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IWaitForInput() {
        ScriptWrapper scr = luaGeneralOw.appliedScript;

        // Stop if this function is used in a coroutine
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.WaitForInput: This function cannot be used in a coroutine.");
            yield break;
        }
        // Stop if this function is used in the initialization page of an event
        if (eventsLoading) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.WaitForInput: This function cannot be used in EventPage0.");
            yield break;
        }

        // Wait until the Player presses a Confirm key
        while (GlobalControls.input.Confirm != ButtonState.PRESSED)
            yield return 0;

        scr.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Progressively sets the tone of the overworld map.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * bool waitEnd - True if the script has to wait for the end of the coroutine, false otherwise.
    /// * int r - Red value of the tone image, clamped between 0 and 255.
    /// * int g - Green value of the tone image, clamped between 0 and 255.
    /// * int b - Blue value of the tone image, clamped between 0 and 255.
    /// * int a - Alpha value of the tone image, clamped between 0 and 255.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator ISetTone(object[] args) {
        ScriptWrapper scr = luaScreenOw.appliedScript;

        // Retrieve all arguments
        bool waitEnd;
        int r, g, b, a;
        try { waitEnd = (bool)args[0]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }
        try { r = (int)args[1];        } catch { throw new CYFException("The argument \"r\" must be a number."); }
        try { g = (int)args[2];        } catch { throw new CYFException("The argument \"g\" must be a number."); }
        try { b = (int)args[3];        } catch { throw new CYFException("The argument \"b\" must be a number."); }
        try { a = (int)args[4];        } catch { throw new CYFException("The argument \"a\" must be a number."); }

        if (waitEnd) {
            // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Screen.SetTone: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            }
            // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
            if (eventsLoading) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Screen.SetTone: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }
        }

        // Get the tone's current color and compute the color differences between the current and future tone
        Color c = GameObject.Find("Tone").GetComponent<Image>().color;

        int[] currents = { (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), (int)(c.a * 255) };
        int[] lacks = { r - currents[0], g - currents[1], b - currents[2], a - currents[3] };
        int[] beginLacks = lacks;
        float[] realLacks = { lacks[0], lacks[1], lacks[2], lacks[3] };

        // Compute which value has the highest difference, which we will use to know when the function is done
        float highest = lacks.Aggregate<int, float>(0, (current, i) => Mathf.Abs(i) > current ? Mathf.Abs(i) : current);
        float beginHighest = highest;

        // If we're not waiting for the end of function, go to the next line
        if (!waitEnd)
            scr.Call("CYFEventNextCommand");

        // Slowly change the tone's color until we reach the requested one
        while (GameObject.Find("Tone") != null && highest > 0) {
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

        // If we're waiting for the end of function, go to the next line
        if (waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    /// <summary>
    /// Progressively rotates an event in 3D.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * string eventName - The name of the event to rotate.
    /// * float rotateX - Rotation value of the X axis in degrees. Might not end up as the same value as it was given due to how 3D rotation works.
    /// * float rotateY - Rotation value of the Y axis in degrees. Might not end up as the same value as it was given due to how 3D rotation works.
    /// * float rotateZ - Rotation value of the Z axis in degrees. Might not end up as the same value as it was given due to how 3D rotation works.
    /// * bool waitEnd - True if the script has to wait for the end of the coroutine, false otherwise.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IRotateEvent(object[] args) {
        ScriptWrapper scr = (ScriptWrapper)args[0];

        // Retrieve all arguments
        string eventName;
        float rotateX, rotateY, rotateZ;
        bool waitEnd;
        try { eventName = (string)args[1]; } catch { throw new CYFException("The argument \"name\" must be a string."); }
        try { rotateX = (float)args[2]; } catch { throw new CYFException("The argument \"rotateX\" must be a number."); }
        try { rotateY = (float)args[3]; } catch { throw new CYFException("The argument \"rotateY\" must be a number."); }
        try { rotateZ = (float)args[4]; } catch { throw new CYFException("The argument \"rotateZ\" must be a number."); }
        try { waitEnd = (bool)args[5]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.Rotate: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
            } else if (eventsLoading) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "Event.Rotate: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        for (int i = 0; i < events.Count || eventName == "Player"; i++) {
            // Get the right event or the Player
            GameObject go = events[i];
            if (eventName != go.name && eventName != "Player") continue;
            if (eventName == "Player")
                go = GameObject.Find("Player");

            // Check if the target is already being rotated in another RotateEvent call
            ScriptWrapper isRotatingSource = eventName == "Player" ? go.GetComponent<PlayerOverworld>().isRotatingSource : go.GetComponent<EventOW>().isRotatingSource;
            // If it's in the right rotation, stop the target's rotation
            if (go.transform.rotation.eulerAngles.x == rotateX && go.transform.rotation.eulerAngles.y == rotateY && go.transform.rotation.eulerAngles.z == rotateZ)
                if (eventName == "Player")
                    go.GetComponent<PlayerOverworld>().isRotatingSource = null;
                else
                    go.GetComponent<EventOW>().isRotatingSource = null;
            // Properly end the current call of this coroutine if the target is already being moved
            if (isRotatingSource != null && isRotatingSource != scr && (eventName == "Player" ? go.GetComponent<PlayerOverworld>().isRotatingWaitEnd : go.GetComponent<EventOW>().isRotatingWaitEnd))
                isRotatingSource.Call("CYFEventNextCommand");
            // Update some values related to rotation
            if (eventName == "Player") {
                go.GetComponent<PlayerOverworld>().isRotatingSource = scr;
                go.GetComponent<PlayerOverworld>().isRotatingWaitEnd = waitEnd;
            } else {
                go.GetComponent<EventOW>().isRotatingSource = scr;
                go.GetComponent<EventOW>().isRotatingWaitEnd = waitEnd;
            }

            // If we're not waiting for the end of function, go to the next line
            if (!waitEnd)
                scr.Call("CYFEventNextCommand");

            // Compute how many degrees are lacking before reaching the requested rotation
            float lackX = rotateX - go.transform.rotation.eulerAngles.x;
            float lackY = rotateY - go.transform.rotation.eulerAngles.y;
            float lackZ = rotateZ - go.transform.rotation.eulerAngles.z;

            // Compute which axis has the highest difference
            var highest = Mathf.Abs(lackX) > Mathf.Abs(lackY) ? lackX : lackY;
            highest = Mathf.Abs(highest) < Mathf.Abs(lackZ) ? lackZ : highest;

            bool reverse = highest > 0;
            var basisHighest = Mathf.Abs(highest);

            while (highest != 0) {
                // Progressively rotate the target 4 degrees at a time
                if ((highest > -4 && highest < 4))
                    break;
                go.transform.rotation = Quaternion.Euler(rotateX + ((basisHighest - Mathf.Abs(highest - basisHighest)) * lackX / basisHighest),
                    rotateY + ((basisHighest - Mathf.Abs(highest - basisHighest)) * lackY / basisHighest),
                    rotateZ + ((basisHighest - Mathf.Abs(highest - basisHighest)) * lackZ / basisHighest));
                highest += reverse ? -4 : 4;
                yield return 0;
            }
            // Set the target's rotation to the requested rotation
            go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);

            // Update some values related to rotation
            // TODO: Find a way to compress this code? Duplicate code
            if (eventName == "Player") {
                go.GetComponent<PlayerOverworld>().isRotatingSource  = null;
                go.GetComponent<PlayerOverworld>().isRotatingWaitEnd = false;
            } else {
                go.GetComponent<EventOW>().isRotatingSource  = null;
                go.GetComponent<EventOW>().isRotatingWaitEnd = false;
            }

            // If we're waiting for the end of function, go to the next line
            if (waitEnd)
                scr.Call("CYFEventNextCommand");

            yield break;
        }
        // If the event can't be found, throw a warning in CYF's console
        UnitaleUtil.WriteInLogAndDebugger("Event.Rotate: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
        scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    /// <summary>
    /// Fades the audio out over a given amount of frames.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * int fadeFrames - The amount of frames before the BGM is completely stopped.
    /// * bool waitEnd - True if the script has to wait for the end of the coroutine, false otherwise.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IFadeBGM(object[] args) {
        ScriptWrapper scr = luaScreenOw.appliedScript;

        // Retrieve all arguments
        int fadeFrames;
        bool waitEnd;
        try { fadeFrames = (int)args[0]; } catch { throw new CYFException("The argument \"fadeFrames\" must be an integer."); }
        try { waitEnd = (bool)args[1]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd)
            // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "General.StopBGM: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
            // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
            } else if (eventsLoading) {
                UnitaleUtil.DisplayLuaError(scr.scriptname, "General.StopBGM: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }

        bgmCoroutine = true;

        // If we're not waiting for the end of function, go to the next line
        if (!waitEnd)
            scr.Call("CYFEventNextCommand");

        // Get the current overworld audio
        AudioSource currentOverworldAudio = UnitaleUtil.GetCurrentOverworldAudio();
        float frames = 0, startVolume = currentOverworldAudio.volume;
        // Fade the audio out over fadeFrames frames
        while (frames < fadeFrames) {
            currentOverworldAudio.volume = startVolume - (startVolume * frames / fadeFrames);
            frames++;
            yield return 0;
        }
        // Stop the audio and reset the volume
        currentOverworldAudio.Stop();
        currentOverworldAudio.volume = startVolume;
        bgmCoroutine = false;

        // If we're waiting for the end of function, go to the next line
        if (!waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    /// <summary>
    /// Fades the audio out over a given amount of frames.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * int frames - The amount of frames during which the flash will be visible.
    /// * bool waitEnd - True if the script has to wait for the end of the coroutine, false otherwise.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IFlash(object[] args) {
        ScriptWrapper scr = luaScreenOw.appliedScript;

        // Retrieve all arguments
        int frames, colorR, colorG, colorB, colorA;
        bool waitEnd;
        try { frames = (int)args[0]; } catch { throw new CYFException("The argument \"frames\" must be a number."); }
        try { colorR = (int)args[1]; } catch { throw new CYFException("The argument \"colorR\" must be a number."); }
        try { colorG = (int)args[2]; } catch { throw new CYFException("The argument \"colorG\" must be a number."); }
        try { colorB = (int)args[3]; } catch { throw new CYFException("The argument \"colorB\" must be a number."); }
        try { colorA = (int)args[4]; } catch { throw new CYFException("The argument \"colorA\" must be a number."); }
        try { waitEnd = (bool)args[5]; } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }

        if (waitEnd) {
            // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
            if (coroutines.ContainsKey(scr) && script != scr) {
                UnitaleUtil.DisplayLuaError(scr.scriptname,
                    "Screen.Flash: This function cannot be used in a coroutine with \"waitEnd\" set to true.");
                yield break;
                // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
            }
            if (eventsLoading) {
                UnitaleUtil.DisplayLuaError(scr.scriptname,
                    "Screen.Flash: This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
                yield break;
            }
        }

        // If we're not waiting for the end of function, go to the next line
        if (!waitEnd)
            scr.Call("CYFEventNextCommand");

        // Create a new GameObject named 'flash' at the middle of the screen
        GameObject flash = new GameObject("flash", typeof(Image));
        flash.transform.SetParent(GameObject.Find("Canvas OW").transform);
        flash.transform.position = Camera.main.transform.position + new Vector3(0, 0, 1);
        flash.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 800);

        // Change the flash's color and fade it out over time
        flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)colorA);
        for (int frame = 0; frame < frames; frame++) {
            if (frame != 0)
                flash.GetComponent<Image>().color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, (byte)(colorA - colorA * frame / frames));
            yield return 0;
        }
        Destroy(flash);

        // If we're waiting for the end of function, go to the next line
        if (waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    /// <summary>
    /// Displays a window for the Player to save the game, or save the game forcefully.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * bool forced - True to prevent the save dialogue to appear and save regardless of the Player's choice.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator ISave(object[] args) {
        ScriptWrapper scr = luaGeneralOw.appliedScript;

        // Retrieve all arguments
        bool forced;
        try { forced = (bool)args[0]; } catch { throw new CYFException("The argument \"forced\" must be a boolean."); }

        if (forced) {
            // Save the game immediately if requested
            SaveLoad.Save(true);
            if (scr != null)
                scr.Call("CYFEventNextCommand");
            yield break;
        }
        // Stop if this function is used in a coroutine and the save prompt must be displayed
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Save: This function cannot be used in a coroutine.");
            yield break;
        }
        // Stop if this function is used in the initialization page of an event and the save prompt must be displayed
        if (eventsLoading) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Save: This function cannot be used in EventPage0.");
            yield break;
        }

        bool save = true;
        Color c = PlayerOverworld.instance.utHeart.color;
        // Move the Player to the first choice of the save dialogue box and display it
        PlayerOverworld.instance.utHeart.transform.position = new Vector3(151 + Camera.main.transform.position.x - 320, 224 + Camera.main.transform.position.y - 240,
                                                                          PlayerOverworld.instance.utHeart.transform.position.z);
        PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 1);
        // Display the save dialogue box
        GameObject.Find("save_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
        GameObject.Find("save_border_outer").transform.SetAsLastSibling();
        PlayerOverworld.instance.utHeart.transform.SetAsLastSibling();
        // Retrieve all of the save dialogue box's texts
        TextManager txtLevel = GameObject.Find("TextManagerLevel").GetComponent<TextManager>(), txtTime = GameObject.Find("TextManagerTime").GetComponent<TextManager>(),
                    txtMap = GameObject.Find("TextManagerMap").GetComponent<TextManager>(), txtName = GameObject.Find("TextManagerName").GetComponent<TextManager>(),
                    txtSave = GameObject.Find("TextManagerSave").GetComponent<TextManager>(), txtReturn = GameObject.Find("TextManagerReturn").GetComponent<TextManager>();

        // Update all of the save dialogue box's texts
        if (SaveLoad.savedGame != null) {
            var playerName = SaveLoad.savedGame.player.Name;
            double playerLevel = SaveLoad.savedGame.player.LV;

            txtName.SetTextQueue(new[] { new TextMessage("[charspacing:2]" + playerName, false, true) });
            txtLevel.SetTextQueue(new[] { new TextMessage("[charspacing:2]LV" + playerLevel, false, true) });
            txtTime.SetTextQueue(new[] { new TextMessage("[charspacing:2]" + UnitaleUtil.TimeFormatter(SaveLoad.savedGame.playerTime), false, true) });
            GameObject.Find("TextManagerTime").GetComponent<TextManager>().MoveTo(180f - UnitaleUtil.PredictTextWidth(txtTime), 68);
            txtMap.SetTextQueue(new[] { new TextMessage("[charspacing:2]" + SaveLoad.savedGame.lastScene, false, true) });
        } else {
            txtName.SetTextQueue(new[] { new TextMessage("[charspacing:2]EMPTY", false, true) });
            txtLevel.SetTextQueue(new[] { new TextMessage("[charspacing:2]LV0", false, true) });
            txtTime.SetTextQueue(new[] { new TextMessage("[charspacing:2]0:00", false, true) });
            GameObject.Find("TextManagerTime").GetComponent<TextManager>().MoveTo(130f, 68);
            txtMap.SetTextQueue(new[] { new TextMessage("[charspacing:2]--", false, true) });
        }
        txtSave.SetTextQueue(new[] { new TextMessage("[charspacing:2]Save", false, true) });
        txtReturn.SetTextQueue(new[] { new TextMessage("[charspacing:2]Return", false, true) });

        // Hide the text dialogue box
        GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        yield return 0;

        // Main loop of the save dialogue
        bool end = false;
        while (true) {
            // Move the soul in front of the current selected option if one of the Left or Right keys are pressed
            if (GlobalControls.input.Left == ButtonState.PRESSED || GlobalControls.input.Right == ButtonState.PRESSED) {
                PlayerOverworld.instance.utHeart.transform.position = new Vector3((save ? 331 : 151) + Camera.main.transform.position.x - 320,
                                                                                  PlayerOverworld.instance.utHeart.transform.position.y,
                                                                                  PlayerOverworld.instance.utHeart.transform.position.z);
                save = !save;
            // Select automatically "Return" if a Cancel key has been pressed
            } else if (GlobalControls.input.Cancel == ButtonState.PRESSED) {
                end = true;
            // Choose the currently selected option if a Confirm key has been pressed
            } else if (GlobalControls.input.Confirm == ButtonState.PRESSED) {
                if (save) {
                    // Save the game
                    SaveLoad.Save(true);
                    // Update the save dialogue box's data
                    PlayerOverworld.instance.utHeart.color = new Color(c.r, c.g, c.b, 0);
                    txtName.SetTextQueue(new[] { new TextMessage("[charspacing:2]" + PlayerCharacter.instance.Name, false, true) });
                    txtLevel.SetTextQueue(new[] { new TextMessage("[charspacing:2]LV" + PlayerCharacter.instance.LV, false, true) });
                    txtTime.SetTextQueue(new[] { new TextMessage("[charspacing:2]" + UnitaleUtil.TimeFormatter(SaveLoad.savedGame.playerTime), false, true) });
                    GameObject.Find("TextManagerTime").GetComponent<TextManager>().MoveTo(180f - UnitaleUtil.PredictTextWidth(txtTime), 68);
                    txtMap.SetTextQueue(new[] { new TextMessage("[charspacing:2]" + SaveLoad.savedGame.lastScene, false, true) });
                    txtSave.SetTextQueue(new[] { new TextMessage("[charspacing:2]File saved.", false, true) });
                    txtReturn.SetTextQueue(new[] { new TextMessage("[charspacing:2]", false, true) });
                    foreach (Image img in GameObject.Find("save_interior").transform.GetComponentsInChildren<Image>())
                        img.color = new Color(1, 1, 0, 1);
                    GameObject.Find("save_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                    GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("saved"));
                    GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    // Wait until the Player presses the Confirm key again
                    do {
                        passPressOnce = true;
                        yield return 0;
                    } while (GlobalControls.input.Confirm != ButtonState.PRESSED);
                }
                end = true;
            }

            // Hides the save dialogue box
            if (end) {
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

    /// <summary>
    /// Moves the overworld camera by a given amount of pixels horizontally and vertically, possibly in a straight line.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * int pixX - Amount of pixels to move the camera by horizontally.
    /// * int pixY - Amount of pixels to move the camera by vertically.
    /// * int speed - Amount of pixels the camera will move by each frame.
    /// * bool straightLine - True if you want the camera to move in a straight line to its destination, false otherwise.
    /// * bool waitEnd - True if the script has to wait for the end of the coroutine, false otherwise.
    /// * string info - Internal value used to display the name of the function calling this coroutine in case of an error.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IMoveCamera(object[] args) {
        ScriptWrapper scr = luaScreenOw.appliedScript;

        // Retrieve all arguments
        int pixX, pixY, speed;
        bool straightLine, waitEnd;
        string info;
        try { pixX = (int)args[0];          } catch { throw new CYFException("The argument \"pixX\" must be a number."); }
        try { pixY = (int)args[1];          } catch { throw new CYFException("The argument \"pixY\" must be a number."); }
        try { speed = (int)args[2];         } catch { throw new CYFException("The argument \"speed\" must be a number."); }
        try { straightLine = (bool)args[3]; } catch { throw new CYFException("The argument \"straightLine\" must be a boolean."); }
        try { waitEnd = (bool)args[4];      } catch { throw new CYFException("The argument \"waitEnd\" must be a boolean."); }
        try { info = (string)args[5];       } catch { throw new CYFException("The argument \"info\" must be a string."); }

        // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
        if (coroutines.ContainsKey(scr) && script != scr && waitEnd) {
            UnitaleUtil.DisplayLuaError(instance.events[instance.actualEventIndex].name, info + ": This function cannot be used in a coroutine with \"waitEnd\" set to true.");
            yield break;
        }
        // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
        if (eventsLoading) {
            UnitaleUtil.DisplayLuaError(instance.events[instance.actualEventIndex].name, info + ": This function cannot be used in EventPage0 with \"waitEnd\" set to true.");
            yield break;
        }
        // Stop if the camera's speed is negative as it will cause an infinite loop.
        if (speed <= 0) {
            UnitaleUtil.DisplayLuaError(instance.events[instance.actualEventIndex].name, info + ": The speed of the camera must be strictly positive.");
            yield break;
        }

        // Compute the horizontal and vertical moving speed of the camera
        float currentX = PlayerOverworld.instance.cameraShift.x, currentY = PlayerOverworld.instance.cameraShift.y,
              xSpeed = currentX > pixX ? -speed : speed, ySpeed = currentY > pixY ? -speed : speed;
        if (straightLine) {
            Vector2 clamped = Vector2.ClampMagnitude(new Vector2(pixX - currentX, pixY - currentY), speed);
            xSpeed = clamped.x;
            ySpeed = clamped.y;
        }

        // If we're not waiting for the end of function, go to the next line
        if (!waitEnd)
            scr.Call("CYFEventNextCommand");

        while (currentX != pixX || currentY != pixY) {
            // Move the camera horizontally if it's not where it should be
            if (currentX != pixX)
                if (Mathf.Abs(xSpeed) < Mathf.Abs(pixX - currentX)) {
                    currentX += xSpeed;
                    PlayerOverworld.instance.cameraShift.x += xSpeed;
                // Move the camera to its final x position if the camera's speed is greater than the distance between the camera's position and the camera's final position
                } else {
                    currentX = pixX;
                    PlayerOverworld.instance.cameraShift.x = pixX;
                }

            // Move the camera vertically if it's not where it should be
            if (currentY != pixY)
                if (Mathf.Abs(ySpeed) < Mathf.Abs(pixY - currentY)) {
                    currentY += ySpeed;
                    PlayerOverworld.instance.cameraShift.y += ySpeed;
                // Move the camera to its final y position if the camera's speed is greater than the distance between the camera's position and the camera's final position
                } else {
                    currentY = pixY;
                    PlayerOverworld.instance.cameraShift.y = pixY;
                }
            yield return 0;
        }
        // If we're waiting for the end of function, go to the next line
        if (waitEnd)
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    /// <summary>
    /// Waits for a given amount of frames before resuming the function's execution.
    /// </summary>
    /// <param name="frames">Frames to wait for before resuming the script's execution. There are 60 frames per second.</param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IWait(int frames) {
        ScriptWrapper scr = luaGeneralOw.appliedScript;
        // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Wait: This function cannot be used in a coroutine.");
            yield break;
        }
        // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
        if (eventsLoading) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.Wait: This function cannot be used in EventPage0.");
            yield break;
        }
        // Wait for a given amount of frames
        int curr = 0;
        while (curr != frames) {
            curr++;
            yield return 0;
        }
        // Go to the next function if this function hasn't been interrupted
        if (scr.GetVar("CYFEventLastAction").String == "General.Wait")
            scr.Call("CYFEventNextCommand");
        yield return 0;
    }

    /// <summary>
    /// Fades the screen out and sends the Player to a Shop.
    /// </summary>
    /// <param name="instant">True if you want to be sent to the Shop instantaneously.</param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IEnterShop(bool instant) {
        ScriptWrapper scr = luaGeneralOw.appliedScript;
        // Stop if this function is used in a coroutine and we have to wait for the end of the function to continue
        if (coroutines.ContainsKey(scr) && script != scr) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.EnterShop: This function cannot be used in a coroutine.");
            yield break;
        }
        // Stop if this function is used in the initialization page of an event and we have to wait for the end of the function to continue
        if (eventsLoading) {
            UnitaleUtil.DisplayLuaError(scr.scriptname, "General.EnterShop: This function cannot be used in EventPage0.");
            yield break;
        }
        // Fade the screen out for a second
        if (!instant) {
            Fading fade = FindObjectOfType<Fading>();
            float fadeTime = fade.BeginFade(1);
            yield return new WaitForSeconds(fadeTime);
        }
        // End this event
        EndEvent();

        // Teleport the Player to a standard Shop scene and configure the shop
        PlayerOverworld.HideOverworld("Shop");
        GlobalControls.isInShop = true;
        SceneManager.LoadScene("Shop", LoadSceneMode.Additive);
        yield return 0;
    }

    /// <summary>
    /// Spawns the box menu, allowing the Player to store items in it.
    /// </summary>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator ISpawnBoxMenu() {
        ScriptWrapper scr = luaInventoryOw.appliedScript;
        // Add the component that takes care of the item box to the item box UI
        GameObject.Find("itembox").AddComponent<ItemBoxUI>();
        // Prevent the Player from doing anything while the menu is up
        PlayerOverworld.instance.PlayerNoMove = true;

        yield return 0;
        // Prevent the Player from doing anything while the menu is up again, in case the first call was reverted
        PlayerOverworld.instance.PlayerNoMove = true;
        // Wait until the item box component is deleted
        while (ItemBoxUI.active)
            yield return 0;

        scr.Call("CYFEventNextCommand");
        yield return 0;
    }
}