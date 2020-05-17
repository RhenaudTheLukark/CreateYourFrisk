/// <summary>
/// Lua binding to set and retrieve information for the game's arena.
/// </summary>
public class LuaArenaStatus {
    public float width         { get { return ArenaManager.instance.newWidth;                            } }
    public float height        { get { return ArenaManager.instance.newHeight;                           } }

    public float x             { get { return ArenaManager.instance.newX;                                } }
    public float y             { get { return ArenaManager.instance.newY;                                } }

    public float currentwidth  { get { return ArenaManager.arenaAbs.width;                               } }
    public float currentheight { get { return ArenaManager.arenaAbs.height;                              } }

    public float currentx      { get { return ArenaManager.arenaAbs.x + ArenaManager.arenaAbs.width / 2; } } //this being a rect value, it's centered on the bottom left corner of the object.
    public float currenty      { get { return ArenaManager.arenaAbs.y;                                   } }

    public bool isResizing  { get { return ArenaManager.instance.isResizeInProgress(); } }
    public bool isresizing  { get { return isResizing;  } }

    public bool isMoving    { get { return ArenaManager.instance.isMoveInProgress();   } }
    public bool ismoving    { get { return isMoving;    } }

    public bool isModifying { get { return isMoving || isResizing;                     } }
    public bool ismodifying { get { return isModifying; } }

    /// <summary>
    /// Resize the arena to the new width/height. Throws a hilarious (read: not hilarious) error message if user was sneaky, bound it globally and tried using it outside of a wave script.
    /// </summary>
    /// <param name="w">New width for arena.</param>
    /// <param name="h">New height for arena.</param>
    public void Resize(int w, int h) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) ArenaManager.instance.Resize(w, h);
        else                                                                    UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void ResizeImmediate(int w, int h) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) ArenaManager.instance.ResizeImmediate(w, h);
        else                                                                    UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void Hide() {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) ArenaManager.instance.Hide();
        else                                                                    UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void Show() {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) ArenaManager.instance.Show();
        else                                                                    UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void Move(float x, float y, bool movePlayer = true, bool immediate = false) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) {
            if (immediate) ArenaManager.instance.MoveImmediate(x, y, movePlayer);
            else           ArenaManager.instance.Move(x, y, movePlayer);
        } else
            UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void MoveTo(float x, float y, bool movePlayer = true, bool immediate = false) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) {
            if (immediate) ArenaManager.instance.MoveToImmediate(x, y, movePlayer);
            else           ArenaManager.instance.MoveTo(x, y, movePlayer);
        } else
            UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void MoveAndResize(float x, float y, int width, int height, bool movePlayer = true, bool immediate = false) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) {
            if (immediate) ArenaManager.instance.MoveAndResizeImmediate(x, y, width, height, movePlayer);
            else           ArenaManager.instance.MoveAndResize(x, y, width, height, movePlayer);
        } else
            UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }

    public void MoveToAndResize(float x, float y, int width, int height, bool movePlayer = true, bool immediate = false) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) {
            if (immediate) ArenaManager.instance.MoveToAndResizeImmediate(x, y, width, height, movePlayer);
            else           ArenaManager.instance.MoveToAndResize(x, y, width, height, movePlayer);
        } else
            UnitaleUtil.DisplayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
    }
}