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
        UnitaleUtil.firstErrorShown = false;
        string mess;
        if (!GlobalControls.modDev) mess = "restart CYF";
        else                        mess = "reload";
        GetComponent<Text>().text = Message + "\n\nPress ESC to " + mess;
	}
}
