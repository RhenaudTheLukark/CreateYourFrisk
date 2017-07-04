using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class to contain a Lua error message, which is displayed as soon as the Error scene loads.
/// </summary>
public class ErrorDisplay : MonoBehaviour {
    public static string Message;
	void Start() {
        if (GameObject.Find("Main Camera OW")) {
            Destroy(GameObject.Find("Main Camera OW"));
            Destroy(GameObject.Find("Canvas OW"));
            Destroy(GameObject.Find("Player"));
        }
        bool ow = !GlobalControls.modDev;
        UnitaleUtil.firstErrorShown = false;
        string mess;
        if (ow) mess = "\n\nPressing ESC to go back to the overworld after an error is now forbidden. Restart CYF and use the modDev mode if you want to debug this error.";
        else    mess = "\n\nPress ESC to reload";
        GetComponent<Text>().text = Message + mess;
	}
}
