using UnityEngine;

/// <summary>
/// Lua binding to retrieve engine timing information. Not marked as static so Lua may have an instance to refer to. Silly, I know.
/// </summary>
public class LuaUnityTime {
    /// <summary>
    /// Time in seconds since the application started.
    /// </summary>
    public static float time { get { return Time.time; } }

    /// <summary>
    /// The time in seconds it took to complete the last update.
    /// </summary>
    public static float dt { get { return Time.deltaTime; } }

    /// <summary>
    /// A multiplier you can use to ensure equal movement even when framerate drops. 1 when 60FPS, 2 when 30FPS, 0.5 when 120FPS, etc.
    /// </summary>
    public static float mult { get { return Time.deltaTime * 60; } }

    public static float wave {
        get {
            if (UIController.instance.state != UIController.UIState.DEFENDING) return -1f;
            else                                                               return Time.time - UIController.instance.encounter.waveBeginTime;
        }
    }
}