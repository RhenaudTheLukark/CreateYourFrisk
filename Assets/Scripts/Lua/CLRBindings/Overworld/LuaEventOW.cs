using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class LuaEventOW {
    //private TextManager textmgr;
    public ScriptWrapper appliedScript;
    
    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    /// <summary>
    /// Permits to teleport an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    [CYFEventFunction] public void Teleport(string name, float dirX, float dirY) {
        for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = EventManager.instance.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                Transform target = null;
                if (go.transform.parent != null)
                    if (go.transform.parent.name == "SpritePivot")
                        target = go.transform.parent;
                target = target ?? go.transform; //oof
                target.position = new Vector3(dirX, dirY, target.position.z); //NEED PARENTAL REMOVE
                appliedScript.Call("CYFEventNextCommand");
                return;
            }
        }
        throw new CYFException("Event.Teleport: The name you entered in the function doesn't exist. Did you forget to add the 'Event' tag?");
    }

    /// <summary>
    /// Move the event from a point to another directly.
    /// Stops if the player can't move to that direction.
    /// The animation process is automatic, if you renamed the triggers that the appliedScript needs to animate your event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    [CYFEventFunction] public void MoveToPoint(string name, float dirX, float dirY, bool wallPass = false, bool waitEnd = true) { StCoroutine("IMoveEventToPoint", new object[] { name, dirX, dirY, wallPass, waitEnd }); }

    /// <summary>
    /// Function that permits to put an animation on an event
    /// </summary>
    /// <param name="name"></param>
    /// <param name="anim"></param>
    [CYFEventFunction] public void SetAnimHeader(string name, string anim) {
        for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = EventManager.instance.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                try {
                    if (go.name == "Player") CYFAnimator.specialPlayerHeader = anim;
                    else                     go.GetComponent<CYFAnimator>().specialHeader = anim;
                } catch {
                    //If the GameObject's Animator component already exists
                    if (go.GetComponent<CYFAnimator>()) throw new CYFException("Event.SetAnimHeader: The event given doesn't exist.");
                    else                                throw new CYFException("Event.SetAnimHeader: The event given doesn't have a CYFAnimator component.");
                }
                appliedScript.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.WriteInLogAndDebugger("Event.SetAnimHeader: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public string GetAnimHeader(string name) {
        if (!GameObject.Find(name))                             throw new CYFException("Event.GetAnimHeader: The event given doesn't exist.");
        if (!GameObject.Find(name).GetComponent<CYFAnimator>()) throw new CYFException("Event.GetAnimHeader: The event given doesn't have a CYFAnimator component.");
        try { return name == "Player" ? CYFAnimator.specialPlayerHeader : GameObject.Find(name).GetComponent<CYFAnimator>().specialHeader; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    [CYFEventFunction] public void SetDirection(string name, int dir) {
        if (!GameObject.Find(name))                             throw new CYFException("Event.SetDirection: The event given doesn't exist.");
        if (!GameObject.Find(name).GetComponent<CYFAnimator>()) throw new CYFException("Event.SetDirection: The event given doesn't have a CYFAnimator component.");
        if (dir != 2 && dir != 4 && dir != 6 && dir != 8)       throw new CYFException("Event.SetDirection: The direction must either be 2 (Down), 4 (Left), 6 (Right) or 8 (Up).");
        GameObject.Find(name).GetComponent<CYFAnimator>().movementDirection = dir;
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public int GetDirection(string name) {
        if (!GameObject.Find(name))                             throw new CYFException("Event.GetDirection: The event given doesn't exist.");
        if (!GameObject.Find(name).GetComponent<CYFAnimator>()) throw new CYFException("Event.GetDirection: The event given doesn't have a CYFAnimator component.");
        try { return GameObject.Find(name).GetComponent<CYFAnimator>().movementDirection; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    /*/// <summary>
    /// Set a return point for the program. If you have to use while iterations, use this instead, with GetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    [CYFEventFunction]
    public void SetReturnPoint(int index) {
        LuaScriptBinder.Set(null, "ReturnPoint" + index, DynValue.NewNumber(textmgr.currentLine));
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Forces the program to go back to the return point of the chosen index. If you have to use while iterations, use this instead, with SetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    [CYFEventFunction]
    public void GetReturnPoint(int index) {
        textmgr.currentLine = (int)LuaScriptBinder.Get(null, "ReturnPoint" + index).Number;
        appliedScript.Call("CYFEventNextCommand");
    }*/

    /// <summary>
    /// Rotates the sprite of an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rotateX"></param>
    /// <param name="rotateY"></param>
    /// <param name="rotateZ"></param>
    /// <param name="axisAnim"></param>
    [CYFEventFunction] public void Rotate(string name, float rotateX, float rotateY, float rotateZ, bool anim = true, bool waitEnd = true) {
        if (anim) {
            StCoroutine("IRotateEvent", new object[] { name, rotateX, rotateY, rotateZ, true });
        } else {
            for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
                GameObject go = null;
                try { go = EventManager.instance.events[i]; } catch { }
                if (name == go.name || name == "Player") {
                    if (name == "Player")
                        go = GameObject.Find("Player");
                    go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                    appliedScript.Call("CYFEventNextCommand");
                    return;
                }
            }
            throw new CYFException("Event.Rotate: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
        }
    }

    [CYFEventFunction] public void Stop() {
        if (EventManager.instance.coroutines.ContainsKey(appliedScript)) StopCoroutine();
        else                                                             EventManager.instance.EndEvent();                                                         
    }

    [CYFEventFunction] public void StopCoroutine(string eventName = "thisevent") {
        ScriptWrapper scr;
        if (eventName == "thisevent")
            scr = appliedScript;
        else {
            if (!GameObject.Find(eventName))
                throw new CYFException("Event.StopCoroutine: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
            if (!EventManager.instance.eventScripts.ContainsKey(GameObject.Find(eventName)))
                throw new CYFException("Event.StopCoroutine: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
            scr = EventManager.instance.eventScripts[GameObject.Find(eventName)];
        }

        if (EventManager.instance.coroutines.ContainsKey(scr)) EventManager.instance.coroutines.Remove(scr);
        else                                                   Debug.LogError("Event.StopCoroutine: You tried to remove the coroutine of an event which hadn't one.");
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public void Remove(string eventName) {
        GameObject go = GameObject.Find(eventName);
        if (!go)
            Debug.LogWarning("Event.Remove: The event " + eventName + " doesn't exist but you tried to remove it.");
        else {
            EventOW ev = go.GetComponent<EventOW>();
            if (ev != null)
                if (!(ev.name.Contains("Image") || ev.name.Contains("Tone"))) {
                    if (GlobalControls.EventData.ContainsKey(ev.name))
                        GlobalControls.EventData.Remove(ev.name);

                    try {
                        GameState.EventInfos ei = new GameState.EventInfos() {
                            CurrPage = ev.actualPage,
                            CurrSpriteNameOrCYFAnim = ev.GetComponent<CYFAnimator>()
                                ? ev.GetComponent<CYFAnimator>().specialHeader
                                : EventManager.instance.sprCtrls[ev.name].img.GetComponent<SpriteRenderer>()
                                    ? EventManager.instance.sprCtrls[ev.name].img.GetComponent<SpriteRenderer>().sprite.name
                                    : EventManager.instance.sprCtrls[ev.name].img.GetComponent<Image>().sprite.name,
                            NoCollision = ev.gameObject.layer == 0,
                            Anchor = UnitaleUtil.VectorToVect(ev.GetComponent<RectTransform>().anchorMax),
                            Pivot = UnitaleUtil.VectorToVect(ev.GetComponent<RectTransform>().pivot)
                        };
                        GlobalControls.EventData.Add(ev.name, ei);
                    } catch { }
                }

            if (EventManager.instance.eventScripts.ContainsKey(go)) {
                if (EventManager.instance.coroutines.ContainsKey(EventManager.instance.eventScripts[go]))
                    EventManager.instance.coroutines.Remove(EventManager.instance.eventScripts[go]);
                EventManager.instance.eventScripts.Remove(go);
            }
            EventManager.instance.sprCtrls.Remove(eventName);
            EventManager.instance.events.Remove(go);
            if (go.transform.parent != null)
                if (go.transform.parent.name == "SpritePivot")
                    go = go.transform.parent.gameObject;
            Object.Destroy(go); //NEED PARENTAL REMOVE
        }
        if (appliedScript != null && (EventManager.instance.ScriptLaunched || EventManager.instance.coroutines.ContainsKey(appliedScript)))
            appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public void SetPage(string ev, int page) {
        SetPage2(ev, page);
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public int GetPage(string ev) {
        if (!GameObject.Find(ev))
            throw new CYFException("Event.GetPage: The given event doesn't exist.");

        if (!EventManager.instance.events.Contains(GameObject.Find(ev)))
            throw new CYFException("Event.GetPage: The given event doesn't exist.");
        try { return GameObject.Find(ev).GetComponent<EventOW>().actualPage; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    [MoonSharpHidden] public static void SetPage2(string eventName, int page) {
        if (!GameObject.Find(eventName))
            throw new CYFException("Event.SetPage: The given event doesn't exist.");

        if (!EventManager.instance.events.Contains(GameObject.Find(eventName)))
            throw new CYFException("Event.SetPage: The given event doesn't exist.");

        GameObject go = GameObject.Find(eventName);
        if (!EventManager.instance.autoDone.ContainsKey(go))
            EventManager.instance.autoDone.Remove(go);
        go.GetComponent<EventOW>().actualPage = page;
        if (EventManager.instance.ScriptLaunched || EventManager.instance.coroutines.ContainsKey(EventManager.instance.luaevow.appliedScript))
            EventManager.instance.luaevow.appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public DynValue GetSprite(string name) {
        try {
            if (name == "Player")
                try { return UserData.Create(PlayerOverworld.instance.sprctrl); } finally { appliedScript.Call("CYFEventNextCommand"); }
            foreach (string key in EventManager.instance.sprCtrls.Keys)
                if (key == name)
                    try { return UserData.Create(EventManager.instance.sprCtrls[name]); } finally { appliedScript.Call("CYFEventNextCommand"); }
            EventManager.instance.ResetEvents(false);
            foreach (string key in EventManager.instance.sprCtrls.Keys)
                if (key == name)
                    try { return UserData.Create(EventManager.instance.sprCtrls[name]); } finally { appliedScript.Call("CYFEventNextCommand"); }
        } catch { }
        throw new CYFException("Event.GetSprite: The event " + name + " doesn't have a sprite.");
    }

    [CYFEventFunction] public void CenterOnCamera(string name, int speed = 5, bool straightLine = false, bool waitEnd = true, string info = "Event.CenterOnCamera") {
        EventManager.instance.luascrow.CenterEventOnCamera(name, speed, straightLine, waitEnd, info);
    }

    [CYFEventFunction] public string GetName() {
        try { return appliedScript.GetVar("_internalScriptName").String; }
        catch { return "4eab1af3ab6a932c23b3cdb8ef618b1af9c02088"; }
        finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    [CYFEventFunction] public DynValue GetPosition(string name) {
        DynValue result = DynValue.NewTable(new Table(null));
        bool done = false;
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player") && !done; i++) {
            if (name != "Player")
                if (EventManager.instance.events[i].gameObject == null || EventManager.instance.events[i].gameObject.name != name)
                    continue;
            done = true;
            GameObject go = name == "Player" ? PlayerOverworld.instance.gameObject : EventManager.instance.events[i];
            Transform target = null;
            if (go.transform.parent != null)
                if (go.transform.parent.name == "SpritePivot")
                    target = go.transform.parent;
            target = target ?? go.transform; //oof
            result.Table.Set(1, DynValue.NewNumber(Mathf.Round(target.position.x * 1000) / 1000));
            result.Table.Set(2, DynValue.NewNumber(Mathf.Round(target.position.y * 1000) / 1000));
            result.Table.Set(3, DynValue.NewNumber(Mathf.Round(target.position.z * 1000) / 1000));
        }
        if (result.Table == new Table(null))
            throw new CYFException("Event.GetPosition: The event \"" + name + "\" doesn't exist.");
        try { return result; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    [CYFEventFunction] public void IgnoreCollision(string name, bool ignore) {
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player"); i++) {
            if (name != "Player")
                if (EventManager.instance.events[i].gameObject == null || EventManager.instance.events[i].gameObject.name != name)
                    continue;
            GameObject go = name == "Player" ? PlayerOverworld.instance.gameObject : EventManager.instance.events[i];
            go.layer = ignore ? 0 : 21;
            //go.GetComponent<Rigidbody2D>().simulated = !ignore;
            appliedScript.Call("CYFEventNextCommand");
            return;
        }
        throw new CYFException("Event.IgnoreCollision: The event \"" + name + "\" doesn't exist.");
    }

    [CYFEventFunction] public void SetSpeed(string name, float speed) {
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player"); i++) {
            if (name != "Player") {
                if (EventManager.instance.events[i].gameObject == null || EventManager.instance.events[i].gameObject.name != name)
                    continue;
                EventManager.instance.events[i].GetComponent<EventOW>().moveSpeed = speed;
            } else
                PlayerOverworld.instance.speed = speed;
            appliedScript.Call("CYFEventNextCommand");
            return;
        }
        throw new CYFException("Event.SetSpeed: The event \"" + name + "\" doesn't exist.");
    }
}
