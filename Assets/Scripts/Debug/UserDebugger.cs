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
    public int maxLines;
    public Queue<string> dbgContent = new Queue<string>();
    private bool firstActive = false;
    private string originalText;

    void Start(){
        instance = this;
        originalText = text.text;
        gameObject.SetActive(false);
    }

    public static void warn(string line){
        instance.writeLine("[Warn] " + line);
    }

    public void userWriteLine(string line) {
        // activation of the debug window if you're printing to it for the first time
        if (!firstActive) {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Camera.main.GetComponent<FPSDisplay>().enabled = true;
            GameObject.Find("Text").transform.SetParent(transform);
            firstActive = true;
        }

        writeLine(line);
        transform.SetAsLastSibling();
    }

    private void writeLine(string line) {
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
