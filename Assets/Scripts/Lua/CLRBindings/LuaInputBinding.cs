﻿using UnityEngine;
using System;

public class LuaInputBinding {
    private readonly UndertaleInput input;
    public LuaInputBinding(UndertaleInput baseInput) { input = baseInput; }

    public int Confirm { get { return (int)input.Confirm; } }
    public int Cancel  { get { return (int)input.Cancel;  } }
    public int Menu    { get { return (int)input.Menu;    } }
    public int Up      { get { return (int)input.Up;      } }
    public int Down    { get { return (int)input.Down;    } }
    public int Left    { get { return (int)input.Left;    } }
    public int Right   { get { return (int)input.Right;   } }

    public int GetKey(string Key) {
        try { return (int)input.Key(Key); }
        catch (Exception) { throw new CYFException("Input.GetKey(): The key \"" + Key + "\" doesn't exist."); }
    }

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

    public float mouseScroll { get { return Input.mouseScrollDelta.y; } }
    public float MouseScroll { get { return mouseScroll; } }
}
