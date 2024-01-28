using UnityEngine;
using System;
using MoonSharp.Interpreter;
using System.Linq;
using System.Collections.Generic;

public class LuaInputBinding {
    private readonly IUndertaleInput input;
    public LuaInputBinding(IUndertaleInput baseInput) { input = baseInput; }

    //////////////////
    // Basic inputs //
    //////////////////

    // Create Your Frisk's basic keys
    public int Confirm { get { return (int)input.Confirm; } }
    public int Cancel  { get { return (int)input.Cancel;  } }
    public int Menu    { get { return (int)input.Menu;    } }
    public int Up      { get { return (int)input.Up;      } }
    public int Down    { get { return (int)input.Down;    } }
    public int Left    { get { return (int)input.Left;    } }
    public int Right   { get { return (int)input.Right;   } }

    public int GetKey(string key) {
        try { return (int)input.Key(key); }
        catch { throw new CYFException("Input.GetKey(): The key \"" + key + "\" doesn't exist."); }
    }

    public float GetAxisRaw(string axis) {
        if (!KeyboardInput.axes.ContainsKey(axis))
            throw new CYFException("Input.GetAxisRaw(): The axis \"" + axis + "\" doesn't exist.");
        return Input.GetAxisRaw(axis);
    }

    //////////////////////////
    // Mouse-related inputs //
    //////////////////////////

    // X and Y position of the mouse
    // The X position of the mouse is taken from ScreenResolution so that the value is correct even if WideScreen is enabled
    public int MousePosX { get { return (int) (ScreenResolution.mousePosition.x / ScreenResolution.displayedSize.x * 640); } }
    public int MousePosY { get { return (int) (ScreenResolution.mousePosition.y / ScreenResolution.displayedSize.y * 480); } }

    public int MousePosAbsX { get { return (int) (Input.mousePosition.x / ScreenResolution.displayedSize.x * 640); } }
    public int MousePosAbsY { get { return (int) (Input.mousePosition.y / ScreenResolution.displayedSize.y * 480); } }

    public bool isMouseInWindow {
        get {
            if (ScreenResolution.wideFullscreen && Screen.fullScreen) return true;
            Rect screenRect = new Rect(0, 0, ScreenResolution.displayedSize.x, Screen.height);
            return screenRect.Contains(ScreenResolution.mousePosition);
        }
    }
    public bool IsMouseInWindow { get { return isMouseInWindow; } }

    public bool isMouseVisible {
        get { return Cursor.visible; }
        set { Cursor.visible = value; }
    }
    public bool IsMouseVisible {
        get { return isMouseVisible; }
        set { isMouseVisible = value; }
    }

    public float mouseScroll { get { return Input.mouseScrollDelta.y; } }
    public float MouseScroll { get { return mouseScroll; } }

    //////////////
    // Keybinds //
    //////////////
    public void CreateKeybind(string keybind, string[] keysToBind = null) { KeyboardInput.CreateKeybind(keybind, keysToBind); }
    public void RemoveKeybind(string keybind) { KeyboardInput.DeleteKeybind(keybind); }
    public void SetKeybindKeys(string keybind, string[] keysToBind = null) { KeyboardInput.SetKeybindKeys(keybind, keysToBind); }

    public bool BindKeyToKeybind(string keybind, string keyToAdd) { return KeyboardInput.AddKeyToKeybind(keybind, keyToAdd); }
    public bool UnbindKeyFromKeybind(string keybind, string keyToRemove) { return KeyboardInput.RemoveKeyFromKeybind(keybind, keyToRemove); }

    public int GetKeybind(string keybind) { return (int)KeyboardInput.StateFor(keybind); }

    public string[] GetKeybindKeys(string keybind) { return KeyboardInput.GetKeybindKeys(keybind); }
    public string[][] GetKeybindConflicts() {
        Dictionary<string, string[]> conflicts = KeyboardInput.GetConflicts(KeyboardInput.encounterKeys);
        return conflicts.Select(
            (p) => {
                List<string> temp = p.Value.ToList();
                temp.Insert(0, p.Key);
                return temp.ToArray();
            }).ToArray();
    }

    public void ResetKeybinds() { KeyboardInput.LoadPlayerKeys(); }

    public DynValue this[string keybind] {
        get { return DynValue.NewNumber(GetKeybind(keybind)); }
        set {
            if (value.Type != DataType.Table || !value.Table.Values.All(d => d.Type == DataType.String))
                throw new CYFException("You need to provide a table of keys as strings to set the keybind to.");
            SetKeybindKeys(keybind, value.Table.Values.Select(d => d.String).ToArray());
        }
    }
}
