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
            Destroy(GameObject.Find("Canvas Two"));
            Destroy(GameObject.Find("Player"));
        }
        bool ow = !GlobalControls.modDev;
        UnitaleUtil.firstErrorShown = false;
        string mess;
        if (ow) mess = "\n\nRestart CYF to further debug this error.";
        else    mess = "\n\nPress ESC to reload";
        GetComponent<Text>().text = Message + mess;
	}
}
