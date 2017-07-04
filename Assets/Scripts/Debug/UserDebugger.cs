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
    private string originalText = null;

    public void Start(){
        instance = this;
        if (originalText == null)
            originalText = text.text;
        text.text = originalText;
        dbgContent.Clear();
        gameObject.SetActive(false);
        firstActive = false;
    }

    public static void Warn(string line){
        instance.WriteLine("[Warn] " + line);
    }

    public void UserWriteLine(string line) {
        // activation of the debug window if you're printing to it for the first time
        if (!firstActive) {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Camera.main.GetComponent<FPSDisplay>().enabled = true;
            GameObject.Find("Text").transform.SetParent(transform);
            firstActive = true;
        }

        WriteLine(line);
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
