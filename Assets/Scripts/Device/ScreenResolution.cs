using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Disables vertical sync, sets resolution to 640x480 and sets the target framerate to 60FPS.
/// Mostly here to prevent high refresh rate screens from being unable to play the game as a lot of scripts are tied to per-frame Update loops.
/// CYF v0.6.4: Houses resolution-based variables and functions.
/// </summary>
public class ScreenResolution : MonoBehaviour {
    public static bool hasInitialized = false;

    public  static bool    perfectFullscreen = true;
    public  static int     windowScale = 1;
    public  static bool    wideFullscreen = false;
    public  static int     lastMonitorWidth = 640;
    public  static int     lastMonitorHeight = 480;
    public  static Vector3 displayedSize;   // x, y: width/height of the "normal" bounds of the screen. z: x offset to start measuring Mouse Position from
    private static float   userAspectRatio;
    private static float   userDisplayWidth;
    private static Rect    FSBorderRect     = new Rect(0f, 0f, 1f, 1f);
    private static Rect    BlurFSBorderRect = new Rect(0f, 0f, 1f, 1f);
    private static Rect    NoBorderRect     = new Rect(0f, 0f, 1f, 1f);

    const int   aspectWidth  = 640;
    const int   aspectHeight = 480;
    const float aspectRatio  = 1.333334f;

    private void Start() {
        if (hasInitialized) {
            Destroy(this);
            return;
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        SetFullScreen(false, Screen.fullScreen ? 2 : 0);
        StartCoroutine(TwoFrameDelay());
    }

    private static IEnumerator TwoFrameDelay() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        //Grab the user's monitor resolution, and calculate some things prematurely
        lastMonitorWidth      = Screen.currentResolution.width;
        lastMonitorHeight     = Screen.currentResolution.height;
        displayedSize         = new Vector3(Screen.width, Screen.height, 0);
        userAspectRatio       = (float)lastMonitorWidth / (float)lastMonitorHeight;
        userDisplayWidth      = System.Math.Min((int)RoundToNearestEven((lastMonitorHeight / (double)3) * (double)4), lastMonitorWidth);

        //Calculate a cropping camera rect to apply to cameras when entering fullscreen
        if (userAspectRatio > aspectRatio) {
            float inset = 1f - (aspectRatio/userAspectRatio);
            FSBorderRect = new Rect(inset/2, 0f, 1f-inset, 1f);
        }

        #if !UNITY_EDITOR
            SceneManager.sceneLoaded += BoxCameras2;
        #endif

        //Display collected monitor width and height to make sure it worked well
        GameObject.Find("Version").GetComponent<UnityEngine.UI.Text>().text = Screen.currentResolution.width + ", " + Screen.currentResolution.height;

        hasInitialized = true;
    }

    public static void SetFullScreen(bool fullscreen, int fswitch = 1) {
        //Regular FS and windowed operations
        if (!fullscreen) {
            Screen.SetResolution(aspectWidth * windowScale, aspectHeight * windowScale, false, 0);
            displayedSize = new Vector3(aspectWidth * windowScale, aspectHeight * windowScale, 0);
        //Enter Blurless FS
        } else {
            if (perfectFullscreen) {
                //Try to shave off anything outside of 4:3
                Screen.SetResolution(lastMonitorWidth, lastMonitorHeight, true, 0);
                displayedSize = new Vector3(userDisplayWidth, lastMonitorHeight, (lastMonitorWidth - userDisplayWidth) / 2);
            //Blurry FS
            } else {
                int downscaledAspectWidth = (int)System.Math.Min((int)RoundToNearestEven(((double)(aspectHeight * windowScale) / lastMonitorHeight) * lastMonitorWidth), lastMonitorWidth);
                Screen.SetResolution(downscaledAspectWidth, aspectHeight * windowScale, true, 0);
                displayedSize = new Vector3(aspectWidth * windowScale, aspectHeight * windowScale, (downscaledAspectWidth - (aspectWidth * windowScale)) / 2);

                if (downscaledAspectWidth > (aspectWidth * windowScale)) {
                    float inset = 1f - (aspectRatio / ((float)downscaledAspectWidth / (aspectHeight * windowScale)));
                    BlurFSBorderRect = new Rect(inset/2, 0f, 1f-inset, 1f);
                }
            }
        }
        BoxCameras(fullscreen);

        #if UNITY_STANDALONE_WIN
            GlobalControls.fullscreenSwitch = fswitch;
        #endif
	}

	private static double RoundToNearestEven(double value) {
		return System.Math.Truncate(value) + (System.Math.Truncate(value) % 2);
	}

    public static Vector3 mousePosition {
        get {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x -= displayedSize.z;
            return mousePos;
        }
    }

    /// Applies (or un-applies) pillarboxing to applicable cameras.
    public static void BoxCameras(bool fullscreen) {
        Camera cam;
        if (GlobalControls.isInFight && (UIController.instance.encounter == null || !UIController.instance.encounter.gameOverStance))
        // if (GlobalControls.isInFight && GameObject.Find("GameOverContainer/Main Camera GameOver") == null)
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        else
            cam = Camera.main;

        if (fullscreen && !wideFullscreen && ((perfectFullscreen && userAspectRatio > aspectRatio) || Screen.currentResolution.width > (aspectWidth * windowScale)))
            cam.rect = perfectFullscreen ? FSBorderRect : BlurFSBorderRect; 
        else
            cam.rect = NoBorderRect;
    }
    private static void BoxCameras2(Scene scene, LoadSceneMode mode) { BoxCameras(Screen.fullScreen); }
}
