using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;

/// <summary>
/// Controls that should be active on all screens. Pretty much a hack to allow people to reset. Now it's more useful.
/// </summary>
public class GlobalControls : MonoBehaviour {
    public static string CYFversion       = "0.6.4";
    public static string OverworldVersion = "0.6.4";
    public static int frame = 0;
    public static float overworldTimestamp = 0f;
    public static PlayerOverworld po;
    public static UndertaleInput input = new KeyboardInput();
    public static LuaInputBinding luaInput = new LuaInputBinding(input);
    public static AudioClip Music;
    // public static Texture2D texBeforeEncounter;
    public static string realName;
    public static string lastScene = "test2";
    public static int uduu; //A secret for everyone :)
    public static int fleeIndex = 0;
    public static bool modDev = false;
    public static bool lastSceneUnitale = false;
    public static bool lastTitle = false;
    public static bool ppcollision = false;
    public static bool allowplayerdef = false;
    public static bool crate = false;
    public static bool retroMode = false;
    public static bool stopScreenShake = false;
    public static bool isInFight = false;
    public static bool isInShop = false;
    public static bool allowWipeSave = false;
    private bool screenShaking = false;
    public static Vector2 beginPosition;
    //public static bool samariosNightmare = false;
    public static string[] nonOWScenes = new string[] { "Battle", "Error", /*"EncounterSelect",*/ "ModSelect", "GameOver", "TitleScreen", "Disclaimer", "EnterName", "TransitionOverworld", "Intro" };
    public static string[] canTransOW = new string[] { "Battle", "Error", "GameOver" };
    //Wow what's this
    public static Dictionary<string, GameState.MapData> GameMapData = new Dictionary<string, GameState.MapData>();
    public static Dictionary<string, GameState.EventInfos> EventData = new Dictionary<string, GameState.EventInfos>();
    public static Dictionary<string, GameState.TempMapData> TempGameMapData = new Dictionary<string, GameState.TempMapData>();
    /*void Start() {
        if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && windows == null)
            windows = new Windows();
        else if (window == null
            misc = new Misc();
    }*/

    // used to only run Awake once
    private static bool awakened = false;

    public void Awake() {
        if (!awakened) {
            StaticInits.Start();
            SaveLoad.Start();
            new ControlPanel();
            new PlayerCharacter();
            SaveLoad.LoadAlMighty();
            LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString("@Title"));

            UnitaleUtil.AddKeysToMapCorrespondanceList();

            // use AlMightyGlobals to load Crate Your Frisk, Safe Mode, Retromode and Fullscreen mode preferences

            if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null && LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean)
                crate = true;
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Misc.WindowName = crate ? ControlPanel.instance.WinodwBsaisNmae : ControlPanel.instance.WindowBasisName;
            #endif

            // check if safe mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFSafeMode") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Type == DataType.Boolean)
                ControlPanel.instance.Safe = LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Boolean;

            // check if retro mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFRetroMode") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Type == DataType.Boolean)
                retroMode = LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Boolean;

            // check if fullscreen mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen").Type == DataType.Boolean)
                perfectFullscreen = LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen").Boolean;

            // check if window scale has a stored preference that is a number
            if (LuaScriptBinder.GetAlMighty(null, "CYFWindowScale") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFWindowScale").Type == DataType.Number)
                windowScale = (int)LuaScriptBinder.GetAlMighty(null, "CYFWindowScale").Number;

            awakened = true;
        }
    }

    // resolution variables
    public static bool perfectFullscreen = true;
    public static int  fullscreenSwitch = 0;
    public static int  windowScale = 1;
    public static bool wideFullscreen = false;
    public static int  lastMonitorWidth = 640;
    public static int  lastMonitorHeight = 480;

    #if UNITY_STANDALONE_WIN
        static IEnumerator RepositionWindow() {
            yield return new WaitForEndOfFrame();

            try {
                Misc.MoveWindowTo((int)(Screen.currentResolution.width/2 - (Screen.width/2)), (int)(Screen.currentResolution.height/2 - (Screen.height/2)));
            } catch {}
        }
    #endif

    public static IEnumerator UpdateMonitorSize() {
        yield return new WaitForEndOfFrame();

        try {
            lastMonitorWidth = Screen.currentResolution.width;
            lastMonitorHeight = Screen.currentResolution.height;
        } catch {}
    }

    public static void SetFullScreen(bool fullscreen, int fswitch = 1) {
        if (!wideFullscreen || ((float)lastMonitorWidth / (float)lastMonitorHeight) < 1.333334) {
            if (perfectFullscreen) {
                if (!fullscreen)
                    Screen.SetResolution(640 * windowScale, 480 * windowScale, false, 0);
                else {
                    double ScreenWidth  = (lastMonitorHeight / (double)3) * (double)4; // 1066
                    double ScreenHeight = (lastMonitorWidth / (double)4) * (double)3; // 960
                    Screen.SetResolution(System.Math.Min((int)RoundToNearestEven(ScreenWidth), lastMonitorWidth), System.Math.Min((int)RoundToNearestEven(ScreenHeight), lastMonitorHeight), true, 0);
                }
            } else
                Screen.SetResolution(640 * windowScale, 480 * windowScale, fullscreen, 0);
        } else {
            if (perfectFullscreen) {
                if (!fullscreen)
                    Screen.SetResolution(640 * windowScale, 480 * windowScale, false, 0);
                else
                    Screen.SetResolution(lastMonitorWidth, lastMonitorHeight, true, 0);
            } else {
                if (!fullscreen)
                    Screen.SetResolution(640 * windowScale, 480 * windowScale, false, 0);
                else {
                    double ScreenWidth  = ((double)480 / lastMonitorHeight) * lastMonitorWidth;
                    Screen.SetResolution(System.Math.Min((int)RoundToNearestEven(ScreenWidth), lastMonitorWidth), 480, true, 0);
                }
            }
        }

        #if UNITY_STANDALONE_WIN
            fullscreenSwitch = fswitch;
        #endif
	}

	private static double RoundToNearestEven(double value) {
		return System.Math.Truncate(value) + (System.Math.Truncate(value) % 2);
	}

    /// <summary>
    /// Control checking, and way more.
    /// </summary>
    void Update () {
        #if UNITY_STANDALONE_WIN
            if (fullscreenSwitch == 1)
                StartCoroutine(RepositionWindow());
            if (fullscreenSwitch > 0)
                fullscreenSwitch--;
        #endif

        stopScreenShake = false;
        if (isInFight)
            frame ++;
        if (SceneManager.GetActiveScene().name == "ModSelect")        lastSceneUnitale = true;
        else                                                          lastSceneUnitale = false;

        // Activate Debugger
        if (UserDebugger.instance && Input.GetKeyDown(KeyCode.F9)) {
            if (UserDebugger.instance.gameObject.activeSelf)
                GameObject.Find("Text").transform.SetParent(UserDebugger.instance.gameObject.transform);
            UserDebugger.instance.gameObject.SetActive(!UserDebugger.instance.gameObject.activeSelf);
            Camera.main.GetComponent<FPSDisplay>().enabled = UserDebugger.instance.gameObject.activeSelf;
        // Activate Hitbox Debugger
        } else if (isInFight && Input.GetKeyDown(KeyCode.H) && SceneManager.GetActiveScene().name != "Error" && UserDebugger.instance.gameObject.activeSelf)
            GameObject.Find("Main Camera").GetComponent<ProjectileHitboxRenderer>().enabled = !GameObject.Find("Main Camera").GetComponent<ProjectileHitboxRenderer>().enabled;
        // Exit a battle or the Error scene
        else if (Input.GetKeyDown(KeyCode.Escape) && (canTransOW.Contains(SceneManager.GetActiveScene().name) || isInFight)) {
            if (isInFight && LuaEnemyEncounter.script.GetVar("unescape").Boolean && SceneManager.GetActiveScene().name != "Error")
                return;
            if (SceneManager.GetActiveScene().name == "Error" && !modDev)
                return;

            if (GameOverBehavior.gameOverContainer)
                if (GameOverBehavior.gameOverContainer.activeInHierarchy)
                    GameObject.FindObjectOfType<GameOverBehavior>().EndGameOver();
                else
                    UIController.EndBattle();
            else
                UIController.EndBattle();
            //StaticInits.Reset();
        // Open the Menu in the Overworld
        } else if (input.Menu == UndertaleInput.ButtonState.PRESSED && !nonOWScenes.Contains(SceneManager.GetActiveScene().name) && !isInFight) {
            if (!PlayerOverworld.instance.PlayerNoMove && EventManager.instance.script == null && !PlayerOverworld.instance.menuRunning[2] && !PlayerOverworld.instance.menuRunning[4] && EventManager.instance.script == null && GameObject.Find("FadingBlack").GetComponent<Fading>().alpha <= 0)
                StartCoroutine(PlayerOverworld.LaunchMenu());
        // Wipe save and close CYF in the Error scene if ControlPanel does not exist yet
        } else if (SceneManager.GetActiveScene().name == "Error" && allowWipeSave && Input.GetKeyDown(KeyCode.R)) {
            System.IO.File.Delete(Application.persistentDataPath + "/save.gd");
            Application.Quit();
        }

        //else if (Input.GetKeyDown(KeyCode.L))
        //    MyFirstComponentClass.SpriteAnalyser();
        if (isInFight)
            switch (fleeIndex) {
                case 0:
                    if (Input.GetKeyDown(KeyCode.F)) fleeIndex++; break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.L)) fleeIndex++;
                    else if (Input.anyKeyDown)       fleeIndex = 0;
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.E)) fleeIndex++;
                    else if (Input.anyKeyDown)       fleeIndex = 0;
                    break;
                case 3:
                    if (Input.GetKeyDown(KeyCode.E)) fleeIndex++;
                    else if (Input.anyKeyDown)       fleeIndex = 0;
                    break;
                case 4:
                    if (Input.GetKeyDown(KeyCode.S)) { fleeIndex = -1; UIController.instance.SuperFlee(); }
                    else if (Input.anyKeyDown)       fleeIndex = 0;
                    break;
            }
        if  (Input.GetKeyDown(KeyCode.F4)        // F4
          || (Input.GetKeyDown(KeyCode.Return)
          &&(Input.GetKey(KeyCode.LeftAlt)       // LAlt  + Enter
          || Input.GetKey(KeyCode.RightAlt)))) { // RAlt  + Enter
			SetFullScreen(!Screen.fullScreen);
            if (!Screen.fullScreen)
                StartCoroutine(UpdateMonitorSize());
          }
    }

    private IEnumerator IShakeScreen(object[] args) {
        float frames, intensity;
        bool fade;

        try { frames = (float)args[0]; } catch { throw new CYFException("The argument \"seconds\" must be a number."); }
        try { intensity = (float)args[1]; } catch { throw new CYFException("The argument \"intensity\" must be a number."); }
        try { fade = (bool)args[2]; } catch { throw new CYFException("The argument \"fade\" must be a boolean."); }

        Transform tf = Camera.main.transform;
        Vector2 shift = new Vector2(0, 0), totalShift = new Vector2(0, 0);
        float frameCount = 0, intensityBasis = intensity;
        while (frameCount < frames) {
            if (stopScreenShake) {
                tf.position = new Vector3(tf.position.x - totalShift.x, tf.position.y - totalShift.y, tf.position.z);
                UserDebugger.instance.transform.position = new Vector3(UserDebugger.instance.transform.position.x - totalShift.x,
                                                                       UserDebugger.instance.transform.position.y - totalShift.y,
                                                                       UserDebugger.instance.transform.position.z);
                screenShaking = false;
                yield break;
            }
            if (fade)
                intensity = intensityBasis * (1 - (frameCount / frames));
            shift = new Vector2((Random.value - 0.5f) * 2 * intensity, (Random.value - 0.5f) * 2 * intensity);

            if (UnitaleUtil.IsOverworld)
                PlayerOverworld.instance.cameraShift = new Vector2(PlayerOverworld.instance.cameraShift.x + shift.x - totalShift.x, PlayerOverworld.instance.cameraShift.y + shift.y - totalShift.y);
            else {
                tf.position = new Vector3(tf.position.x + shift.x - totalShift.x, tf.position.y + shift.y - totalShift.y, tf.position.z);
                UserDebugger.instance.transform.position = new Vector3(UserDebugger.instance.transform.position.x + shift.x - totalShift.x,
                                                                       UserDebugger.instance.transform.position.y + shift.y - totalShift.y,
                                                                       UserDebugger.instance.transform.position.z);
            }
            //print(totalShift + " + " + shift + " = " + (totalShift + shift));
            totalShift = shift;
            frameCount++;
            yield return 0;
        }
        screenShaking = false;
        tf.position = new Vector3(tf.position.x - totalShift.x, tf.position.y - totalShift.y, tf.position.z);
        if (!UnitaleUtil.IsOverworld)
            UserDebugger.instance.transform.position = new Vector3(UserDebugger.instance.transform.position.x - totalShift.x,
                                                                   UserDebugger.instance.transform.position.y - totalShift.y,
                                                                   UserDebugger.instance.transform.position.z);
    }

    public void ShakeScreen(float duration, float intensity, bool isIntensityDecreasing) {
        if (!screenShaking) {
            screenShaking = true;
            StartCoroutine("IShakeScreen", new object[] { duration, intensity, isIntensityDecreasing });
        }
    }

    void OnApplicationQuit() { /*UnitaleUtil.closeFile();*/ }
}
