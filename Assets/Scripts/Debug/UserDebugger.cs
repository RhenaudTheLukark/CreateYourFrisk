using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Behaviour you can use to print lines into a UnityEngine.UI.Text component for debugging, keeping only a set amount of lines in memory.
/// </summary>
public class UserDebugger : MonoBehaviour{
    public static UserDebugger instance;
    public Text text;
    public int maxLines = 7;
    public Queue<string> dbgContent = new Queue<string>();
    public bool canShow = true;
    private bool firstActive = false;
    private string originalText = null;

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
        if (UnitaleUtil.printDebuggerBeforeInit != "") {
            UserWriteLine(UnitaleUtil.printDebuggerBeforeInit);
            UnitaleUtil.printDebuggerBeforeInit = "";
        }
    }

    public void UserWriteLine(string line) {
        line = line ?? "nil";
        foreach (string str in line.Split('\n'))
            WriteLine(str);
        Debug.Log("Frame " + GlobalControls.frame + ": " + line);
        // activation of the debug window if you're printing to it for the first time
        if (!firstActive && canShow) {
            gameObject.SetActive(true);
            try {
                Camera.main.GetComponent<FPSDisplay>().enabled = true;
            } catch { }
            firstActive = true;
        }
        transform.SetAsLastSibling();
    }

    private void WriteLine(string line) {
        // enqueue the new line and keep queue at capacity
        dbgContent.Enqueue(line);
        if (dbgContent.Count > maxLines)
            dbgContent.Dequeue();

        // print to debug console
        text.text = originalText;
        foreach(string dbgLine in dbgContent){
            text.text += "\n" + dbgLine;
        }
    }
}
