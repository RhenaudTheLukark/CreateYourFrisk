using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuaEventOW {
    private TextManager textmgr;
    
    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden]
    public static event LoadedAction StCoroutine;

    [MoonSharpHidden]
    public LuaEventOW(TextManager textmgr) {
        this.textmgr = textmgr;
    }

    /// <summary>
    /// Permits to teleport an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    [CYFEventFunction]
    public void Teleport(string name, float dirX, float dirY) {
        for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = EventManager.instance.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                go.transform.position = new Vector3(dirX, dirY, go.transform.position.z);
                EventManager.instance.script.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function doesn't exists. Did you forget to add the 'Event' tag ?");
        EventManager.instance.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Move the event from a point to another directly.
    /// Stops if the player can't move to that direction.
    /// The animation process is automatic, if you renamed the triggers that the EventManager.instance.script needs to animate your event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    [CYFEventFunction]
    public void MoveToPoint(string name, float dirX, float dirY, bool wallPass = false) { StCoroutine("IMoveEventToPoint", new object[] { name, dirX, dirY, wallPass }); }

    /*/// <summary>
    /// Function that permits to put an animation on an event
    /// </summary>
    /// <param name="name"></param>
    /// <param name="anim"></param>
    [CYFEventFunction]
    public void SetAnim(string name, string anim) {
        for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = EventManager.instance.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                try { go.GetComponent<Animator>().Play(anim); } 
                catch {
                    //If the GameObject's Animator component already exists
                    if (go.GetComponent<Animator>())  UnitaleUtil.writeInLog("The current anim doesn't exist.");
                    else                              UnitaleUtil.writeInLog("This GameObject doesn't have an Animator component!");
                }
                EventManager.instance.script.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag?");
        EventManager.instance.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Triggers a specific anim switch on a chosen event. (uses Animator)
    /// </summary>
    /// <param name="name">The name of the event</param>
    /// <param name="triggerName">The name of the trigger</param>
    [CYFEventFunction]
    public void SetAnimSwitch(string name, string triggerName) {
        for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
            GameObject go = null;
            try { go = EventManager.instance.events[i]; } catch { }
            if (name == go.name || name == "Player") {
                if (name == "Player")
                    go = GameObject.Find("Player");
                Animator anim = go.GetComponent<Animator>();
                if (go == null) {
                    UnitaleUtil.displayLuaError(EventManager.instance.script.scriptname, "This event doesn't have an Animator component !");
                    EventManager.instance.script.Call("CYFEventNextCommand");
                    return;
                }
                PlayerOverworld.instance.forcedAnim = true;
                anim.SetTrigger(triggerName);
                EventManager.instance.script.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
        EventManager.instance.script.Call("CYFEventNextCommand");
    }*/


    /*/// <summary>
    /// Set a return point for the program. If you have to use while iterations, use this instead, with GetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    [CYFEventFunction]
    public void SetReturnPoint(int index) {
        LuaScriptBinder.Set(null, "ReturnPoint" + index, DynValue.NewNumber(textmgr.currentLine));
        EventManager.instance.script.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Forces the program to go back to the return point of the chosen index. If you have to use while iterations, use this instead, with SetReturnPoint
    /// </summary>
    /// <param name="index"></param>
    [CYFEventFunction]
    public void GetReturnPoint(int index) {
        textmgr.currentLine = (int)LuaScriptBinder.Get(null, "ReturnPoint" + index).Number;
        EventManager.instance.script.Call("CYFEventNextCommand");
    }*/

    /// <summary>
    /// Rotates the sprite of an event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rotateX"></param>
    /// <param name="rotateY"></param>
    /// <param name="rotateZ"></param>
    /// <param name="axisAnim"></param>
    [CYFEventFunction]
    public void Rotate(string name, float rotateX, float rotateY, float rotateZ, bool anim = true) {
        if (anim) {
            StCoroutine("IRotateEvent", new object[] { name, rotateX, rotateY, rotateZ });
        } else {
            for (int i = 0; i < EventManager.instance.events.Count || name == "Player"; i++) {
                GameObject go = null;
                try { go = EventManager.instance.events[i]; } catch { }
                if (name == go.name || name == "Player") {
                    if (name == "Player")
                        go = GameObject.Find("Player");
                    go.GetComponent<RectTransform>().rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
                    EventManager.instance.script.Call("CYFEventNextCommand");
                    return;
                }
            }
            UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
            EventManager.instance.script.Call("CYFEventNextCommand");
        }
    }

    [CYFEventFunction]
    public void Stop() { EventManager.instance.endEvent(); }

    [CYFEventFunction]
    public void Remove(string eventName) {
        GameObject go = GameObject.Find(eventName);
        if (!go)
            Debug.LogError("The event " + eventName + " doesn't exist but you tried to remove it.");
        else {
            EventManager.instance.events.Remove(go);
            GameObject.Destroy(go);
        }
        if (EventManager.instance.script != null && EventManager.instance.scriptLaunched)
            EventManager.instance.script.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public void SetPage(string ev, int page) {
        if (ev == "wowyoucanttakeitdudeyeahnoyoucant")
            ev = EventManager.instance.events[EventManager.instance.actualEventIndex].name;
        SetPage2(ev, page);
        EventManager.instance.script.Call("CYFEventNextCommand");
    }

    [MoonSharpHidden]
    public static void SetPage2(string eventName, int page) {
        if (!GameObject.Find(eventName)) {
            UnitaleUtil.displayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        if (!EventManager.instance.events.Contains(GameObject.Find(eventName))) {
            UnitaleUtil.displayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "The given event doesn't exist.");
            return;
        }

        if (!GlobalControls.MapEventPages.ContainsKey(SceneManager.GetActiveScene().buildIndex))
            GlobalControls.MapEventPages.Add(SceneManager.GetActiveScene().buildIndex, new Dictionary<string, int>());

        if (GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex].ContainsKey(eventName))
            GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex][eventName] = page;
        else
            GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex].Add(eventName, page);

        GameObject.Find(eventName).GetComponent<EventOW>().actualPage = page;
        if (EventManager.instance.scriptLaunched)
            EventManager.instance.script.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public DynValue GetSprite(string name) {
        foreach (object key in EventManager.instance.sprCtrls.Keys) {
            if (key.ToString() == name) {
                EventManager.instance.script.script.Globals.Set("test", UserData.Create((LuaSpriteController)EventManager.instance.sprCtrls[name], LuaSpriteController.data));
                try { return UserData.Create((LuaSpriteController)EventManager.instance.sprCtrls[name], LuaSpriteController.data); }
                finally {
                    EventManager.instance.script.script.Globals.Set(DynValue.NewString("CYFEventLameNeedReturn"), DynValue.NewBoolean(true));
                    EventManager.instance.script.Call("CYFEventNextCommand");
                }
            }
        }
        UnitaleUtil.displayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "The event " + name + " doesn't have a sprite.");
        return null;
    }

    [CYFEventFunction]
    public void CenterOnCamera(string name, int speed = 5, bool straightLine = false) {
        EventManager.instance.luascrow.CenterEventOnCamera(name, speed, straightLine);
    }

    [CYFEventFunction]
    public DynValue GetPosition(string name) {
        DynValue result = DynValue.NewTable(new Table(null));
        bool done = false;
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player") && !done; i++) {
            GameObject go = name == "Player" ? PlayerOverworld.instance.gameObject : EventManager.instance.events[i];
            done = true;
            result.Table.Set(1, DynValue.NewNumber(go.transform.position.x));
            result.Table.Set(2, DynValue.NewNumber(go.transform.position.y));
            LuaScriptBinder.SetBattle(null, "GetPosition" + name, UserData.Create((LuaSpriteController)EventManager.instance.sprCtrls[name], LuaSpriteController.data));
        }
        try { return result; } 
        finally { EventManager.instance.script.Call("CYFEventNextCommand"); }
    }
}
