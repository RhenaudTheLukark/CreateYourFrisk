using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Behaviour you can use to print lines into a UnityEngine.UI.Text component for debugging, keeping only a set amount of lines in memory.
/// </summary>
public class UserDebugger : MonoBehaviour {
    public static UserDebugger instance;
    public Text text;
    public int maxLines = 7;
    public Queue<string> dbgContent = new Queue<string>();
    public bool canShow = true;
    private bool firstActive;
    private string originalText;
    public static Vector2 offset;

    public void Warn(string line) {
        Debug.LogWarning("Frame " + GlobalControls.frame + ": " + line);
        WriteLine(line);
    }

    public void Start() {
        instance = this;
        if (originalText == null)
            originalText = text.text;
        text.text = originalText;
        dbgContent.Clear();
        gameObject.SetActive(false);
        firstActive = false;
        if (UnitaleUtil.printDebuggerBeforeInit == "") return;
        UserWriteLine(UnitaleUtil.printDebuggerBeforeInit);
        UnitaleUtil.printDebuggerBeforeInit = "";
    }

    public void Update() {
        transform.SetAsLastSibling();
    }

    public void UserWriteLine(string line) {
        line = line ?? "nil";
        foreach (string str in line.Split('\n'))
            WriteLine(str);
        Debug.Log("Frame " + GlobalControls.frame + ": " + line);
        // activation of the debug window if you're printing to it for the first time
        if (!firstActive && canShow) {
            gameObject.SetActive(true);
            try { Camera.main.GetComponent<FPSDisplay>().enabled = true; }
            catch { /* ignored */ }

            firstActive = true;
        }
    }

    private void WriteLine(string line) {
        // enqueue the new line and keep queue at capacity
        dbgContent.Enqueue(line);
        if (dbgContent.Count > maxLines)
            dbgContent.Dequeue();

        // print to debug console
        text.text = originalText;
        foreach (string dbgLine in dbgContent)
            text.text += "\n" + dbgLine;
    }

    public static float x {
        get { return instance.transform.position.x - Misc.cameraX; }
        set {
            offset.x = value - Misc.WindowWidth / 2f - 300;
            instance.transform.position = new Vector3(value + Misc.cameraX, instance.transform.position.y, instance.transform.position.z);
        }
    }

    public static float y {
        get { return instance.transform.position.y - Misc.cameraY; }
        set {
            offset.y = value - Misc.WindowHeight / 2f - 240;
            instance.transform.position = new Vector3(instance.transform.position.x, value + Misc.cameraY, instance.transform.position.z);
        }
    }

    public static float absx {
        get { return instance.transform.position.x; }
        set {
            offset.x = value - Misc.WindowWidth / 2f - Misc.cameraX - 300;
            instance.transform.position = new Vector3(value, instance.transform.position.y, instance.transform.position.z);
        }
    }

    public static float absy {
        get { return instance.transform.position.y; }
        set {
            offset.y = value - Misc.WindowHeight / 2f - Misc.cameraY - 240;
            instance.transform.position = new Vector3(instance.transform.position.x, value, instance.transform.position.z);
        }
    }

    public static void MoveTo(float new_x, float new_y) {
        x = new_x;
        y = new_y;
    }

    public static void Move(float new_x, float new_y) {
        absx += new_x;
        absy += new_y;
    }

    public static void MoveToAbs(float new_x, float new_y) {
        absx = new_x;
        absy = new_y;
    }
}
