using UnityEngine;
using System.Collections;

/// <summary>
/// Disables vertical sync, sets resolution to 640x480 and sets the target framerate to 60FPS.
/// Mostly here to prevent high refresh rate screens from being unable to play the game as a lot of scripts are tied to per-frame Update loops.
/// </summary>
public class ScreenResolution : MonoBehaviour {
    private static bool hasInitialized = false;

    private static IEnumerator TwoFrameDelay() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try {
            GlobalControls.lastMonitorWidth = Screen.currentResolution.width;
            GlobalControls.lastMonitorHeight = Screen.currentResolution.height;
        } catch {}
    }

    private void Start() {
        if (hasInitialized) {
            Destroy(this);
            return;
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        GlobalControls.SetFullScreen(false, Screen.fullScreen ? 2 : 0);
        StartCoroutine(TwoFrameDelay());

        hasInitialized = true;
    }
}
