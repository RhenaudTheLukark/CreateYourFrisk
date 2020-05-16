using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class LuaEventOW {
    //private TextManager textmgr;
    public ScriptWrapper appliedScript;

    public delegate void LoadedAction(string coroName, object args, string evName);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    /// <summary>
    /// Checks if an event exists.
    /// </summary>
    /// <param name="name">Name of the event to check for.</param>
    /// <returns>True if the event exists, false otherwise.</returns>
    [CYFEventFunction] public bool Exists(string name) {
        try { return GameObject.Find(name) != null; }
        finally { appliedScript.Call("CYFEventNextCommand"); }
    }

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
    [CYFEventFunction] public void MoveToPoint(string name, float dirX, float dirY, bool wallPass = false, bool waitEnd = true) { StCoroutine("IMoveEventToPoint", new object[] { appliedScript, name, dirX, dirY, wallPass, waitEnd }, name); }

    /// <summary>
    /// Checks if an event is currently moving via Event.MoveToPoint.
    /// </summary>
    /// <param name="name">Name of the event to check for.</param>
    /// <returns>True if the event is moving, false otherwise.</returns>
    [CYFEventFunction] public bool isMoving(string name) {
        bool ismoving;
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player"); i++) {
            if (name != "Player") {
                if (EventManager.instance.events[i].gameObject == null || EventManager.instance.events[i].gameObject.name != name)
                    continue;
                ismoving = (EventManager.instance.events[i].GetComponent<EventOW>().isMovingSource != null);
            } else
                ismoving = PlayerOverworld.instance.isMoving;
            appliedScript.Call("CYFEventNextCommand");
            return ismoving;
        }
        throw new CYFException("Event.isMoving: The event \"" + name + "\" doesn't exist.");
    }
    [CYFEventFunction] public bool IsMoving(string name) { return isMoving(name); }

    /// <summary>
    /// Function that sets an event's animation prefix.
    /// If the header itself matches an animation this event has, it will play the animation instead of using it as a prefix.
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
                if (go == null)
                    throw new CYFException("Event.SetAnimHeader: The given event doesn't exist.");

                CYFAnimator animator = go.GetComponent<CYFAnimator>();
                if (animator == null)
                    throw new CYFException("Event.SetAnimHeader: The given event doesn't have a CYFAnimator component.");

                if (animator.AnimExists(animator.specialHeader))
                    animator.movementDirection = 2;
                go.GetComponent<CYFAnimator>().specialHeader = anim;
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
        try { return GameObject.Find(name).GetComponent<CYFAnimator>().specialHeader; } finally { appliedScript.Call("CYFEventNextCommand"); }
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

    /// <summary>
    /// Rotates the sprite of an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rotateX"></param>
    /// <param name="rotateY"></param>
    /// <param name="rotateZ"></param>
    /// <param name="axisAnim"></param>
    [CYFEventFunction] public void Rotate(string name, float rotateX, float rotateY, float rotateZ, bool anim = true, bool waitEnd = true) {
        if (anim)
            StCoroutine("IRotateEvent", new object[] { appliedScript, name, rotateX, rotateY, rotateZ, waitEnd }, name);
        else {
            for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
                GameObject go = null;
                try { go = EventManager.instance.events[i]; } catch { }
                if (name == go.name || name == "Player") {
                    if (name == "Player")
                        go = GameObject.Find("Player");
                    go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                    StCoroutine("IRotateEvent", new object[] { appliedScript, name, rotateX, rotateY, rotateZ, waitEnd }, name);
                    return;
                }
            }
            throw new CYFException("Event.Rotate: The name you entered in the function isn't an event's name. Did you forget to add the 'Event' tag?");
        }
    }

    /// <summary>
    /// Checks if an event is currently moving via Event.Rotate.
    /// </summary>
    /// <param name="name">Name of the event to check for.</param>
    /// <returns>True if the event is rotating, false otherwise.</returns>
    [CYFEventFunction] public bool isRotating(string name) {
        bool isrotating;
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player"); i++) {
            if (name != "Player") {
                if (EventManager.instance.events[i].gameObject == null || EventManager.instance.events[i].gameObject.name != name)
                    continue;
                isrotating = (EventManager.instance.events[i].GetComponent<EventOW>().isRotatingSource != null);
            } else
                isrotating = (PlayerOverworld.instance.isRotatingSource != null);
            appliedScript.Call("CYFEventNextCommand");
            return isrotating;
        }
        throw new CYFException("Event.isRotating: The event \"" + name + "\" doesn't exist.");
    }
    [CYFEventFunction] public bool IsRotating(string name) { return isRotating(name); }

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
        else                                                   Debug.LogError("Event.StopCoroutine: You tried to remove the coroutine of an event which did not have one.");
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public void Remove(string eventName) {
        GameObject go = GameObject.Find(eventName);
        if (!go)
            UnitaleUtil.Warn("Event.Remove: The event " + eventName + " doesn't exist but you tried to remove it.", false);
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
        if (EventManager.instance.autoDone.ContainsKey(go))
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
        throw new CYFException("Event.GetSprite: The event \"" + name + "\" doesn't have a sprite.");
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

    [CYFEventFunction] public float GetSpeed(string name) {
        float speed;
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player"); i++) {
            if (name != "Player") {
                if (EventManager.instance.events[i].gameObject == null || EventManager.instance.events[i].gameObject.name != name)
                    continue;
                speed = EventManager.instance.events[i].GetComponent<EventOW>().moveSpeed;
            } else
                speed = PlayerOverworld.instance.speed;
            appliedScript.Call("CYFEventNextCommand");
            return speed;
        }
        throw new CYFException("Event.GetSpeed: The event \"" + name + "\" doesn't exist.");
    }
}
