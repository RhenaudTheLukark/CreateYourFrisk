using UnityEngine;
using System;

public class LuaInputBinding {
    private UndertaleInput input;
    public LuaInputBinding(UndertaleInput baseInput) { this.input = baseInput; }

    public int Confirm { get { return (int)input.Confirm; } }
    public int Cancel  { get { return (int)input.Cancel; } }
    public int Menu    { get { return (int)input.Menu; } }
    public int Up      { get { return (int)input.Up; } }
    public int Down    { get { return (int)input.Down; } }
    public int Left    { get { return (int)input.Left; } }
    public int Right   { get { return (int)input.Right; } }

    public int GetKey(string Key) {
        try {
            return (int)input.Key(Key);
        } catch (Exception) { throw new CYFException("Input.GetKey(): The key \"" + Key + "\" doesn't exist."); }
    }

    public int MousePosX { get { return (int)((Input.mousePosition.x /  Screen.width) * 640); } }

    public int MousePosY { get { return (int)((Input.mousePosition.y / Screen.height) * 480); } }

    public bool isMouseInWindow {
        get {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            return screenRect.Contains(Input.mousePosition);
        }
    }
    public bool IsMouseInWindow {
        get { return isMouseInWindow; }
    }
}
