using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// Lua binding to set and retrieve information for an enemy in the game.
/// </summary>
public class LuaEnemyStatus
{
    private LuaEnemyController ctrl;

    public LuaEnemyStatus(LuaEnemyController ctrl)
    {
        this.ctrl = ctrl;
    }

    public bool isKilled
    {
        get
        {
            return ctrl.killed;
        }
    }

    public bool isSpared
    {
        get
        {
            return ctrl.spared;
        }
    }

    public bool isActive
    {
        get
        {
            return ctrl.inFight;
        }
    }

    public void SetSprite(string name)
    {
        ctrl.SetSprite(name);
    }

    public void SetActive(bool active)
    {
        ctrl.SetActive(active);
    }

    public void SetVar(string key, DynValue value)
    {
        //ctrl.script.Globals.Set(key, MoonSharpUtil.CloneIfRequired(ctrl.script, value));
        ctrl.script.SetVar(key, value);
    }

    public DynValue GetVar(Script newOwner, string key)
    {
        //return MoonSharpUtil.CloneIfRequired(newOwner, ctrl.script.Globals.Get(key));
        return ctrl.script.GetVar(key);
    }

    public void Kill()
    {
        ctrl.DoKill();
    }

    public void Spare()
    {
        ctrl.DoSpare();
    }
}