using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class to contain a Lua error message, which is displayed as soon as the Error scene loads.
/// </summary>
public class ErrorDisplay : MonoBehaviour {
    public static string Message;
	void Start () {
        UnitaleUtil.firstErrorShown = false;
        GetComponent<Text>().text = Message + "\n\nPress ESC to reload";
	}
}
