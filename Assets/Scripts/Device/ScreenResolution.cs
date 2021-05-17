using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Disables vertical sync, sets resolution to 640x480 and sets the target framerate to 60FPS.
/// Mostly here to prevent high refresh rate screens from being unable to play the game as a lot of scripts are tied to per-frame Update loops.
/// CYF v0.6.4: Houses resolution-based variables and functions.
/// </summary>
public class ScreenResolution : MonoBehaviour {
    public  static bool          hasInitialized;
    public  static bool          perfectFullscreen = true;    //"Blurless Fullscreen" option.
    public  static int           windowScale = 1;             //Global "Window Scale" option.
    public  static int           tempWindowScale = 1;         //Window Scale option used right now
    public  static bool          wideFullscreen;              //Enabled/disabled by means of Misc.SetWideFullscreen.
    public  static int           lastMonitorWidth = 640;      //The user's monitor width.  Becomes Misc.MonitorWidth.
    public  static int           lastMonitorHeight = 480;     //The user's monitor height. Becomes Misc.MonitorHeight.
    public  static Vector2       displayedSize;               //Width/Height of the "normal" bounds of the screen.
    public  static Vector2       mousePosShift;               //Offset to start measuring Mouse Position from.
    public  static int           windowWidth = 640;
    public  static int           windowHeight = 480;
    public  static float         battleCanvasScale = 1;       //Scale value of the Canvas object in CYF's Battle scene.
    private static float         userAspectRatio;             //The aspect ratio of the user's monitor.
    private static float         userDisplayWidth;            //Width of the user's monitor if it were compressed horizontally to match a 4:3 aspect ratio.
    private static Rect          FSBorderRect = new Rect(0f, 0f, 1f, 1f); //Rect to apply to cameras in fullscreen (with pillarboxing).
    private static readonly Rect NoBorderRect = new Rect(0f, 0f, 1f, 1f); //Rect to apply to cameras in windowed (or wide fullscreen).

    private void Start() {
        if (hasInitialized) {
            Destroy(this);
            return;
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        //Grab the user's monitor resolution, and calculate some things early
        lastMonitorWidth  = Display.main.systemWidth;
        lastMonitorHeight = Display.main.systemHeight;
        displayedSize     = new Vector3(Screen.width, Screen.height, 0);
        userAspectRatio   = lastMonitorWidth / (float)lastMonitorHeight;
        userDisplayWidth  = System.Math.Min((int)RoundToNearestEven(lastMonitorHeight / (double)3 * 4), lastMonitorWidth);
        ProjectileHitboxRenderer.fsScreenWidth = System.Math.Min((int)RoundToNearestEven((double)(480 / (float)lastMonitorHeight) * lastMonitorWidth), lastMonitorWidth);

        //Calculate a cropping camera rect to apply to cameras when entering fullscreen
        if (userAspectRatio > 1.333334f) {
            float inset = 1f - 1.333334f / userAspectRatio;
            FSBorderRect = new Rect(inset/2, 0f, 1f-inset, 1f);
        }

        #if !UNITY_EDITOR
            SceneManager.sceneLoaded += BoxCameras2;
        #endif

        //Load BGCamera Prefab and have it be in every scene, from the moment CYF starts.
        //This is necessary so BGCamera will clear out old frames outside of the Main Camera's display rect.
        GameObject BGCamera = Instantiate(Resources.Load<GameObject>("Prefabs/BGCamera"));
        BGCamera.name = "BGCamera";
        #if UNITY_EDITOR
            BGCamera.GetComponent<Camera>().rect = NoBorderRect;
        #endif
        DontDestroyOnLoad(BGCamera);

        //If this is the user's first time EVER opening the engine, force 640x480 windowed
        if (!PlayerPrefs.HasKey("once")) {
            SetFullScreen(false, 2);
            PlayerPrefs.SetInt("once", 1);
        }
        hasInitialized = true;
    }

    private void OnDestroy() {
        #if !UNITY_EDITOR
            SceneManager.sceneLoaded -= BoxCameras2;
        #endif
    }

    /// <summary>
    /// Enters or exits fullscreen, whilst accounting for .
    /// </summary>
    /// <param name="fullscreen">Whether or not the user is in fullscreen</param>
    /// <param name="fswitch"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void SetFullScreen(bool fullscreen, int fswitch = 1, int width = -1, int height = -1) {
        if (width == -1) width = windowWidth;
        else             windowWidth = width;

        if (height == -1) height = windowHeight;
        else              windowHeight = height;

        //Regular FS and windowed operations
        if (!fullscreen) {
            Screen.SetResolution(width * tempWindowScale, height * tempWindowScale, false, 0);
            displayedSize = new Vector2(640 * tempWindowScale, 480 * tempWindowScale);
            mousePosShift = new Vector2(width / 2f - 320, height / 2f - 240);
            battleCanvasScale = 480f / height;
        //Enter FS
        } else {
            //Blurless FS
            if (perfectFullscreen) {
                Screen.SetResolution(lastMonitorWidth, lastMonitorHeight, true, 0);
                displayedSize = new Vector2(userDisplayWidth, lastMonitorHeight);
                // TODO: Use screen's current res to get the content's bottom left corner & required battleCanvasScale
                mousePosShift = new Vector2((lastMonitorWidth - userDisplayWidth) / 2, 0);
                battleCanvasScale = 1;
            //Blurry FS
            } else {
                int downscaledAspectWidth = System.Math.Min((int)RoundToNearestEven(((double)(height * tempWindowScale) / lastMonitorHeight) * lastMonitorWidth), lastMonitorWidth);
                Screen.SetResolution(downscaledAspectWidth, height * tempWindowScale, true, 0);
                displayedSize = new Vector2(640 * tempWindowScale, 480 * tempWindowScale);
                // TODO: Extend mouse position shift & battleCanvasScale here too
                mousePosShift = new Vector2((downscaledAspectWidth - 640 * tempWindowScale) / 2f, 0);
                battleCanvasScale = 1;
            }
        }

        #if UNITY_STANDALONE_WIN
            GlobalControls.fullscreenSwitch = fswitch;
            // Update the canvas' size
            if (GameObject.Find("arena")) {
                GameObject.Find("Canvas").transform.localScale = new Vector3(battleCanvasScale, battleCanvasScale, 1);
                GameOverBehavior.gameOverContainer.transform.GetChild(1).localScale = new Vector3(battleCanvasScale, battleCanvasScale, 1);
                // TODO: Study why this doesn't work properly
            }
        #elif UNITY_EDITOR
            mousePosShift = new Vector2();
            battleCanvasScale = 1;
        #endif

        BoxCameras(fullscreen, width, height);
    }

    public static void ResetAfterBattle() {
        wideFullscreen  = false;
        tempWindowScale = windowScale;
        if (Screen.width != 640 * windowWidth || Screen.height != 480 * windowWidth)
            SetFullScreen(Screen.fullScreen, 0, 640, 480);
    }

    private static double RoundToNearestEven(double value) {
        return System.Math.Truncate(value) + (System.Math.Truncate(value) % 2);
    }

    /// <summary>
    /// Returns a modified mousePosition that counts the bottom-left of the "play area" as (0, 0), rather than the bottom-left of the screen.
    /// </summary>
    public static Vector2 mousePosition {
        get { return (Vector2)Input.mousePosition - mousePosShift; }
    }

    /// <summary>
    /// Applies (or un-applies) pillarboxing to applicable cameras.
    /// </summary>
    /// <param name="fullscreen">Whether or not the user is in fullscreen</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void BoxCameras(bool fullscreen, int width = -1, int height = -1) {
        if (width == -1) width = windowWidth;
        else             windowWidth = width;

        if (height == -1) height = windowHeight;
        else              windowHeight = height;
        //Grab the right camera to edit
        Camera cam;
        if (GlobalControls.isInFight && (UIController.instance.encounter == null || !UIController.instance.encounter.gameOverStance))
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        else
            cam = Camera.main;

        //Set displayed rect
        if (fullscreen && !wideFullscreen && ((perfectFullscreen && userAspectRatio > (float)width / height) || Screen.currentResolution.width > (width * tempWindowScale)))
            cam.rect = FSBorderRect;
        else
            cam.rect = NoBorderRect;
    }

    private static void BoxCameras2(Scene scene, LoadSceneMode mode) {
        BoxCameras(Screen.fullScreen);
    }
}
