﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuaEventOW {
    private TextManager textmgr;
    public ScriptWrapper appliedScript;
    
    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden]
    public static event LoadedAction StCoroutine;

    [MoonSharpHidden]
    public LuaEventOW(TextManager textmgr) { this.textmgr = textmgr; }

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
                appliedScript.Call("CYFEventNextCommand");
                return;
            }
        }
        throw new CYFException("Event.Teleport: The name you entered into the function doesn't exist. Did you forget to add the 'Event' tag?");
    }

    /// <summary>
    /// Move the event from a point to another directly.
    /// Stops if the player can't move to that direction.
    /// The animation process is automatic, if you renamed the triggers that the appliedScript needs to animate your event.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dirX"></param>
    /// <param name="dirY"></param>
    [CYFEventFunction]
    public void MoveToPoint(string name, float dirX, float dirY, bool wallPass = false) { StCoroutine("IMoveEventToPoint", new object[] { name, dirX, dirY, wallPass }); }

    /// <summary>
    /// Function that permits to put an animation on an event
    /// </summary>
    /// <param name="name"></param>
    /// <param name="anim"></param>
    [CYFEventFunction]
    public void SetAnimHeader(string name, string anim) {
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
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag?");
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public string GetAnimHeader(string name) {
        if (!GameObject.Find(name))                             throw new CYFException("Event.GetAnimHeader: The event given doesn't exist.");
        if (!GameObject.Find(name).GetComponent<CYFAnimator>()) throw new CYFException("Event.GetAnimHeader: The event given doesn't have a CYFAnimator component.");
        try { return name == "Player" ? CYFAnimator.specialPlayerHeader : GameObject.Find(name).GetComponent<CYFAnimator>().specialHeader; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    /*/// <summary>
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
                    UnitaleUtil.displayLuaError(appliedScript.scriptname, "This event doesn't have an Animator component !");
                    appliedScript.Call("CYFEventNextCommand");
                    return;
                }
                PlayerOverworld.instance.forcedAnim = true;
                anim.SetTrigger(triggerName);
                appliedScript.Call("CYFEventNextCommand");
                return;
            }
        }
        UnitaleUtil.writeInLog("The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag ?");
        appliedScript.Call("CYFEventNextCommand");
    }*/


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
                    appliedScript.Call("CYFEventNextCommand");
                    return;
                }
            }
            throw new CYFException("Event.Rotate: The name you entered into the function isn't an event's name. Did you forget to add the 'Event' tag?");
        }
    }

    [CYFEventFunction]
    public void Stop() {
        if (EventManager.instance.coroutines.ContainsKey(appliedScript)) StopCoroutine();
        else                                                             EventManager.instance.endEvent();                                                         
    }

    [CYFEventFunction]
    public void StopCoroutine() {
        if (EventManager.instance.coroutines.ContainsKey(appliedScript)) EventManager.instance.coroutines.Remove(appliedScript);
        else                                                             Debug.LogError("You tried to remove the coroutine of an event which hadn't one.");
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public void Remove(string eventName) {
        GameObject go = GameObject.Find(eventName);
        if (!go)
            Debug.LogError("The event " + eventName + " doesn't exist but you tried to remove it.");
        else {
            if (EventManager.instance.eventScripts.ContainsKey(go)) {
                if (EventManager.instance.coroutines.ContainsKey(EventManager.instance.eventScripts[go]))
                    EventManager.instance.coroutines.Remove(EventManager.instance.eventScripts[go]);
                EventManager.instance.eventScripts.Remove(go);
            }
            EventManager.instance.sprCtrls.Remove(eventName);
            EventManager.instance.events.Remove(go);
            GameObject.Destroy(go);
        }
        if (appliedScript != null && (EventManager.instance.scriptLaunched || EventManager.instance.coroutines.ContainsKey(appliedScript)))
            appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public void SetPage(string ev, int page) {
        SetPage2(ev, page);
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public int GetPage(string ev) {
        if (!GameObject.Find(ev))
            throw new CYFException("Event.GetPage: The given event doesn't exist.");

        if (!EventManager.instance.events.Contains(GameObject.Find(ev)))
            throw new CYFException("Event.GetPage: The given event doesn't exist.");
        try { return GameObject.Find(ev).GetComponent<EventOW>().actualPage; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    [MoonSharpHidden]
    public static void SetPage2(string eventName, int page) {
        if (!GameObject.Find(eventName))
            throw new CYFException("Event.SetPage: The given event doesn't exist.");

        if (!EventManager.instance.events.Contains(GameObject.Find(eventName)))
            throw new CYFException("Event.SetPage: The given event doesn't exist.");

        if (!GlobalControls.MapEventPages.ContainsKey(SceneManager.GetActiveScene().buildIndex))
            GlobalControls.MapEventPages.Add(SceneManager.GetActiveScene().buildIndex, new Dictionary<string, int>());

        if (GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex].ContainsKey(eventName))
            GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex][eventName] = page;
        else
            GlobalControls.MapEventPages[SceneManager.GetActiveScene().buildIndex].Add(eventName, page);

        GameObject go = GameObject.Find(eventName);
        if (!EventManager.instance.autoDone.ContainsKey(go))
            EventManager.instance.autoDone.Remove(go);
        go.GetComponent<EventOW>().actualPage = page;
        if (EventManager.instance.scriptLaunched || EventManager.instance.coroutines.ContainsKey(EventManager.instance.luaevow.appliedScript))
            EventManager.instance.luaevow.appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction]
    public DynValue GetSprite(string name) {
        try {
            if (name == "Player")
                try { return UserData.Create(PlayerOverworld.instance.sprctrl); } finally { appliedScript.Call("CYFEventNextCommand"); }
            foreach (string key in EventManager.instance.sprCtrls.Keys)
                if (key == name)
                    try { return UserData.Create(EventManager.instance.sprCtrls[name]); } finally { appliedScript.Call("CYFEventNextCommand"); }
        } catch { }
        throw new CYFException("Event.GetSprite: The event " + name + " doesn't have a sprite.");
    }

    [CYFEventFunction]
    public void CenterOnCamera(string name, int speed = 5, bool straightLine = false) { EventManager.instance.luascrow.CenterEventOnCamera(name, speed, straightLine); }

    [CYFEventFunction]
    public string GetName() { try { return EventManager.instance.eventScripts.FirstOrDefault(x => x.Value == appliedScript).Key.name; } finally { appliedScript.Call("CYFEventNextCommand"); } }
        
    [CYFEventFunction]
    public DynValue GetPosition(string name) {
        DynValue result = DynValue.NewTable(new Table(null));
        bool done = false;
        for (int i = 0; (i < EventManager.instance.events.Count || name == "Player") && !done; i++) {
            if (name != "Player")
                if (EventManager.instance.events[i].gameObject.name != name)
                    continue;
            done = true;
            GameObject go = name == "Player" ? PlayerOverworld.instance.gameObject : EventManager.instance.events[i];
            result.Table.Set(1, DynValue.NewNumber(go.transform.position.x));
            result.Table.Set(2, DynValue.NewNumber(go.transform.position.y));
        }
        if (result.Table == new Table(null))
            throw new CYFException("Event.GetPosition: The event \"" + name + "\" doesn't exist.");
        try { return result; } finally { appliedScript.Call("CYFEventNextCommand"); }
    }
}