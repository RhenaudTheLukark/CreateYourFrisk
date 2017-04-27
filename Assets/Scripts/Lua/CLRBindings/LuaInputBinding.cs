using UnityEngine;
using System.Collections;

public class LuaInputBinding {
    private UndertaleInput input;
    public LuaInputBinding(UndertaleInput baseInput) { this.input = baseInput; }

    public int Confirm { get { return (int)this.input.Confirm; } }
    public int Cancel { get { return (int)this.input.Cancel; } }
    public int Menu { get { return (int)this.input.Menu; } }
    public int Up { get { return (int)this.input.Up; } }
    public int Down { get { return (int)this.input.Down; } }
    public int Left { get { return (int)this.input.Left; } }
    public int Right { get { return (int)this.input.Right; } }

    public int GetKey(string Key) { return (int)this.input.Key(Key); }
    public int MousePosX { get { return (int)Input.mousePosition.x; } }
    public int MousePosY { get { return (int)Input.mousePosition.y; } }
    public bool isMouseInWindow {
        get {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            return screenRect.Contains(Input.mousePosition);
        }
    }
}
