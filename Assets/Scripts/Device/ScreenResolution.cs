using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Disables vertical sync, sets resolution to 640x480 and sets the target framerate to 60FPS.
/// Mostly here to prevent high refresh rate screens from being unable to play the game as a lot of scripts are tied to per-frame Update loops.
/// CYF v0.6.4: Houses resolution-based variables and functions.
/// </summary>
public class ScreenResolution : MonoBehaviour {
    public  static bool          hasInitialized;
    public  static int           windowScale = 1;             // Global "Window Scale" option.
    public  static int           tempWindowScale = 1;         // Window Scale option used right now
    public  static bool          wideFullscreen;              // Enabled/disabled by means of Misc.SetWideFullscreen.
    public  static Vector2       lastMonitorSize;             // The user's monitor size. Becomes Misc.MonitorWidth and Misc.MonitorHeight.
    public  static Vector2       displayedSize;               // Width/Height of the "normal" bounds of the screen.
    public  static Vector2       mousePosShift;               // Offset to start measuring Mouse Position from.
    public  static Vector2       windowSize = new Vector2(640, 480);  // Size of the window.
    private static float         userAspectRatio;             // The aspect ratio of the user's monitor.
    private static Rect          FSBorderRect = new Rect(0f, 0f, 1f, 1f); // Rect to apply to cameras in fullscreen (with pillarboxing).
    private static readonly Rect NoBorderRect = new Rect(0f, 0f, 1f, 1f); // Rect to apply to cameras in windowed (or wide fullscreen).
    private static GameObject BGCamera;
    private static float orthographicSize = 240;

    public void Start() {
        if (hasInitialized)
            return;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Grab the user's monitor resolution, and calculate some things early
        lastMonitorSize  = new Vector2(Display.main.systemWidth, Display.main.systemHeight);
        displayedSize    = new Vector2(Screen.width, Screen.height);
        userAspectRatio  = lastMonitorSize.x / lastMonitorSize.y;
        ProjectileHitboxRenderer.fsScreenWidth = (int)System.Math.Min(RoundToNearestEven((double)(480 / lastMonitorSize.y) * lastMonitorSize.x), lastMonitorSize.x);

        // Calculate a cropping camera rect to apply to cameras when entering fullscreen
        if (userAspectRatio > 1.333334f) {
            float inset = 1f - 1.333334f / userAspectRatio;
            FSBorderRect = new Rect(inset/2, 0f, 1f-inset, 1f);
        }

        #if !UNITY_EDITOR
            SceneManager.sceneLoaded += BoxCameras2;
        #endif

        // Load BGCamera Prefab and have it be in every scene, from the moment CYF starts.
        // This is necessary so BGCamera will clear out old frames outside of the Main Camera's display rect.
        BGCamera = Instantiate(Resources.Load<GameObject>("Prefabs/BGCamera"));
        BGCamera.name = "BGCamera";
        #if UNITY_EDITOR
            BGCamera.GetComponent<Camera>().rect = NoBorderRect;
        #endif
        DontDestroyOnLoad(BGCamera);

        // If this is the user's first time EVER opening the engine, force 640x480 windowed
        SetFullScreen(PlayerPrefs.HasKey("once") && Screen.fullScreen, PlayerPrefs.HasKey("once") ? 0 : 2);
        PlayerPrefs.SetInt("once", 1);
        hasInitialized = true;
    }

    private static bool setSize;

    /// <summary>
    /// Enters or exits fullscreen, whilst accounting for the screen's dimensions.
    /// </summary>
    /// <param name="fullscreen">Whether or not the user is in fullscreen</param>
    /// <param name="fswitch"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void SetFullScreen(bool fullscreen, int fswitch = 1, int width = -1, int height = -1) {
        // Failsafe if the data we need hasn't been provided yet
        if (userAspectRatio == 0)
            Camera.main.gameObject.AddComponent<ScreenResolution>().Start();

        if (width == -1)
            width = (int)windowSize.x;
        else if (width != (int)windowSize.x) {
            Misc.cameraXWindowSizeShift = -width % 2 / 2f;
            windowSize.x                = width;
            setSize = true;
        }

        if (height == -1)
            height = (int)windowSize.y;
        else if (height != (int)windowSize.y) {
            Misc.cameraYWindowSizeShift = -height % 2 / 2f;
            windowSize.y                = height;
            setSize = true;
        }

        Rect rect;
        if (!fullscreen) {
            // Windowed
            int maxScale = Mathf.FloorToInt(Mathf.Min(Display.main.systemWidth / (float)width, Display.main.systemHeight / (float)height));
            tempWindowScale = Mathf.Min(windowScale, maxScale);

            Screen.SetResolution(width * tempWindowScale, height * tempWindowScale, false, 0);
            displayedSize = new Vector2(640 * tempWindowScale, 480 * tempWindowScale);
            mousePosShift = new Vector2(width / 2f - 320, height / 2f - 240);
            rect = new Rect(0f, 0f, 1f, 1f);
        } else {
            int newWidth, newHeight;
            if (!wideFullscreen) {
                // 4:3 FS
                float maxScale = Mathf.Min(lastMonitorSize.x / width, lastMonitorSize.y / height);
                newWidth = Mathf.RoundToInt(width * maxScale);
                newHeight = Mathf.RoundToInt(height * maxScale);
            } else {
                // Wide FS
                newWidth = (int)lastMonitorSize.x;
                newHeight = (int)lastMonitorSize.y;
            }

            float currRatio = (float)newWidth / newHeight;
            float newScaledHeight = newHeight * Mathf.Min(1, userAspectRatio / currRatio);
            displayedSize = new Vector2(newScaledHeight * 1.333333f, newScaledHeight);

            Screen.SetResolution((int)lastMonitorSize.x, (int)lastMonitorSize.y, true, 0);
            mousePosShift = new Vector2((lastMonitorSize.x - displayedSize.x) / 2f, (lastMonitorSize.y - displayedSize.y) / 2f);

            if (currRatio <= userAspectRatio) {
                float inset = 1f - currRatio / userAspectRatio;
                rect = new Rect(inset / 2, 0f, 1f - inset, 1f);
            } else {
                float inset = 1f - userAspectRatio / currRatio;
                rect = new Rect(0f, inset / 2, 1f, 1f - inset);
            }
        }

        #if UNITY_STANDALONE_WIN
            GlobalControls.fullscreenSwitch = fswitch;
        #elif UNITY_EDITOR
            mousePosShift = new Vector2();
        #endif

        BoxCameras(height / 2f, rect);
    }

    public static void ResetAfterBattle() {
        wideFullscreen  = false;
        tempWindowScale = windowScale;
        if (setSize)
            SetFullScreen(Screen.fullScreen, 0, 640, 480);
        setSize = false;
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
    public static void BoxCameras(float newOrthographicSize, Rect rect) {
        // Grab the right camera to edit
        Camera cam, cam2 = null;
        if (GlobalControls.isInFight && UIController.instance != null && (UIController.instance.encounter == null || !UIController.instance.encounter.gameOverStance)) {
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            cam2 = GameOverBehavior.gameOverContainer.transform.GetComponentInChildren<Camera>(true);
        } else
            cam = Camera.main;

        // Set displayed rect
        cam.rect = rect;
        cam.orthographicSize = newOrthographicSize;
        if (cam2 != null) {
            cam2.rect = rect;
            cam2.orthographicSize = newOrthographicSize;
        }
        orthographicSize = newOrthographicSize;
    }

    private static void BoxCameras2(Scene scene, LoadSceneMode mode) {
        BoxCameras(orthographicSize, !Screen.fullScreen || wideFullscreen ? NoBorderRect : FSBorderRect);
    }
}
