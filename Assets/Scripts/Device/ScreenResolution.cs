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

    public  static bool    perfectFullscreen = true;    //"Blurless Fullscreen" option.
    public  static int     windowScale = 1;             //"Window Scale" option.
    public  static bool    wideFullscreen = false;      //Enabled/disabled by means of Misc.SetWideFullscreen.
    public  static int     lastMonitorWidth = 640;      //The user's monitor  width. Becomes  Misc.MonitorWidth.
    public  static int     lastMonitorHeight = 480;     //The user's monitor height. Becomes Misc.MonitorHeight.
    public  static Vector3 displayedSize;               //x, y: width/height of the "normal" bounds of the screen. z: x offset to start measuring Mouse Position from
    private static float   userAspectRatio;             //The aspect ratio of the user's monitor.
    private static float   userDisplayWidth;            //Width of the user's monitor if it were compressed horizontally to match a 4:3 aspect ratio.
    private static Rect    FSBorderRect = new Rect(0f, 0f, 1f, 1f); //Rect to apply to cameras in fullscreen (with pillarboxing).
    private static Rect    NoBorderRect = new Rect(0f, 0f, 1f, 1f); //Rect to apply to cameras in windowed (or wide fullscreen).

    const int   ASPECT_WIDTH  = 640;
    const int   ASPECT_HEIGHT = 480;
    const float ASPECT_RATIO  = 1.333334f;

    private void Start() {
        if (hasInitialized) {
            Destroy(this);
            return;
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        //Grab the user's monitor resolution, and calculate some things early
        lastMonitorWidth      = Display.main.systemWidth;
        lastMonitorHeight     = Display.main.systemHeight;
        displayedSize         = new Vector3(Screen.width, Screen.height, 0);
        userAspectRatio       = (float)lastMonitorWidth / (float)lastMonitorHeight;
        userDisplayWidth      = System.Math.Min((int)RoundToNearestEven((lastMonitorHeight / (double)3) * (double)4), lastMonitorWidth);
        ProjectileHitboxRenderer.fsScreenWidth = System.Math.Min((int)RoundToNearestEven((double)(ASPECT_HEIGHT / (float)lastMonitorHeight) * lastMonitorWidth), lastMonitorWidth);

        //Calculate a cropping camera rect to apply to cameras when entering fullscreen
        if (userAspectRatio > ASPECT_RATIO) {
            float inset = 1f - (ASPECT_RATIO/userAspectRatio);
            FSBorderRect = new Rect(inset/2, 0f, 1f-inset, 1f);
        }

        #if !UNITY_EDITOR
            SceneManager.sceneLoaded += BoxCameras2;
        #endif

        //Load BGCamera Prefab and have it be in every scene, from the moment CYF starts.
        //This is necessary so BGCamera will clear out old frames outside of the Main Camera's display rect.
        GameObject BGCamera = Instantiate(Resources.Load<GameObject>("Prefabs/BGCamera"));
        BGCamera.name = "BGCamera";
        GameObject.DontDestroyOnLoad(BGCamera);

        //If this is the user's first time EVER opening the engine, force 640x480 windowed
        if (!PlayerPrefs.HasKey("once")) {
            SetFullScreen(false, 2);
            PlayerPrefs.SetInt("once", 1);
        }
        hasInitialized = true;
    }

    /// <summary>
    /// Enters or exits fullscreen, whilst accounting for .
    /// </summary>
    /// <param name="fullscreen">Whether or not the user is in fullscreen</param>
    public static void SetFullScreen(bool fullscreen, int fswitch = 1) {
        //Regular FS and windowed operations
        if (!fullscreen) {
            Screen.SetResolution(ASPECT_WIDTH * windowScale, ASPECT_HEIGHT * windowScale, false, 0);
            displayedSize = new Vector3(ASPECT_WIDTH * windowScale, ASPECT_HEIGHT * windowScale, 0);
        //Enter FS
        } else {
            //Blurless FS
            if (perfectFullscreen) {
                Screen.SetResolution(lastMonitorWidth, lastMonitorHeight, true, 0);
                displayedSize = new Vector3(userDisplayWidth, lastMonitorHeight, (lastMonitorWidth - userDisplayWidth) / 2);
            //Blurry FS
            } else {
                int downscaledAspectWidth = (int)System.Math.Min((int)RoundToNearestEven(((double)(ASPECT_HEIGHT * windowScale) / lastMonitorHeight) * lastMonitorWidth), lastMonitorWidth);
                Screen.SetResolution(downscaledAspectWidth, ASPECT_HEIGHT * windowScale, true, 0);
                displayedSize = new Vector3(ASPECT_WIDTH * windowScale, ASPECT_HEIGHT * windowScale, (downscaledAspectWidth - (ASPECT_WIDTH * windowScale)) / 2);
            }
        }
        BoxCameras(fullscreen);

        #if UNITY_STANDALONE_WIN
            GlobalControls.fullscreenSwitch = fswitch;
        #elif UNITY_EDITOR
            displayedSize.z = 0;
        #endif
	}

	private static double RoundToNearestEven(double value) {
		return System.Math.Truncate(value) + (System.Math.Truncate(value) % 2);
	}

    /// <summary>
    /// Returns a modified mousePosition that counts the bottom-left of the "play area" as (0, 0), rather than the bottom-left of the screen.
    /// </summary>
    public static Vector3 mousePosition {
        get {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x -= displayedSize.z;
            return mousePos;
        }
    }

    private static string lastScene = "Disclaimer";

    /// <summary>
    /// Applies (or un-applies) pillarboxing to applicable cameras.
    /// </summary>
    /// <param name="fullscreen">Whether or not the user is in fullscreen</param>
    public static void BoxCameras(bool fullscreen) {
        //Grab the right camera to edit
        Camera cam;
        if (GlobalControls.isInFight && (UIController.instance.encounter == null || !UIController.instance.encounter.gameOverStance))
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        else
            cam = Camera.main;

        //Set displayed rect
        if (fullscreen && !wideFullscreen && ((perfectFullscreen && userAspectRatio > ASPECT_RATIO) || Screen.currentResolution.width > (ASPECT_WIDTH * windowScale)) && lastScene != "Options")
            cam.rect = FSBorderRect;
        else
            cam.rect = NoBorderRect;
    }
    private static void BoxCameras2(Scene scene, LoadSceneMode mode) {
        lastScene = scene.name;
        BoxCameras(Screen.fullScreen);
    }
}
