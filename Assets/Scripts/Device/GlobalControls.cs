using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls that should be active on all screens. Pretty much a hack to allow people to reset. Now it's more useful.
/// </summary>
public class GlobalControls : MonoBehaviour {
    public static int frame = 0;
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
    
    // used to only call Awake once
    private bool awakened = false;
    
    void Awake() {
        if (!awakened) {
            SceneManager.sceneLoaded += LoadScene;
            
            // use AlMightyGlobals to load Safe Mode, Retromode and Fullscreen mode preferences
            
            // check if safe mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFSafeMode") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Type.ToString() == "Boolean")
                ControlPanel.instance.Safe = LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Boolean;
            
            // check if retro mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFRetroMode") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Type.ToString() == "Boolean")
                retroMode = LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Boolean;
            
            // check if fullscreen mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen").Type.ToString() == "Boolean")
                perfectFullscreen = LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen").Boolean;
            
            awakened = true;
        }
    }
    
    // blurless fullscreen variables
    public static bool perfectFullscreen = true;
    public static int fullscreenSwitch = 0;
    
    #if UNITY_STANDALONE_WIN
        static IEnumerator RepositionWindow() {
            yield return new WaitForEndOfFrame();
            
            try {
                Misc.MoveWindowTo((int)(Screen.currentResolution.width/2 - 320), (int)(Screen.currentResolution.height/2 - 240));
            } catch {}
        }
    #endif
    
    public static void SetFullScreen(bool fullscreen, int newSwitch = 1) {
        if (perfectFullscreen) {
            if (!fullscreen)
                Screen.SetResolution(640, 480, false, 0);
            else
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true, 0);
        } else
            Screen.SetResolution(640, 480, fullscreen, 0);
        
        fullscreenSwitch = newSwitch;
	}

	private static double RoundToNearestEven(double value) {
		return System.Math.Truncate(value) + (System.Math.Truncate(value) % 2);
	}

	static IEnumerator ChangeAspectRatio() {
        yield return new WaitForFixedUpdate();
        
		if (!Application.isEditor) {
			double ScreenWidth = (Screen.height / (double)3) * (double)4;
			Screen.SetResolution((int)RoundToNearestEven(ScreenWidth), Screen.height, Screen.fullScreen, 0);
		}
	}

    /// <summary>
    /// Control checking, and way more.
    /// </summary>
    void Update () {
        if (fullscreenSwitch != 0) {
            StartCoroutine(ChangeAspectRatio());
            
            #if UNITY_STANDALONE_WIN
                if (!Screen.fullScreen && fullscreenSwitch == 1)
                    StartCoroutine(RepositionWindow());
            #endif
            
            fullscreenSwitch--;
        }
        
        stopScreenShake = false;
        if (isInFight)
            frame ++;
        if (SceneManager.GetActiveScene().name == "ModSelect")        lastSceneUnitale = true;
        else                                                          lastSceneUnitale = false;
        if (UserDebugger.instance && Input.GetKeyDown(KeyCode.F9)) {
            if (UserDebugger.instance.gameObject.activeSelf)
                GameObject.Find("Text").transform.SetParent(UserDebugger.instance.gameObject.transform);
            UserDebugger.instance.gameObject.SetActive(!UserDebugger.instance.gameObject.activeSelf);
            Camera.main.GetComponent<FPSDisplay>().enabled = !Camera.main.GetComponent<FPSDisplay>().enabled;
        } else if (isInFight && Input.GetKeyDown(KeyCode.H))
            GameObject.Find("Main Camera").GetComponent<ProjectileHitboxRenderer>().enabled = !GameObject.Find("Main Camera").GetComponent<ProjectileHitboxRenderer>().enabled;
        else if (Input.GetKeyDown(KeyCode.Escape) && (canTransOW.Contains(SceneManager.GetActiveScene().name) || isInFight)) {
            if (isInFight && LuaEnemyEncounter.script.GetVar("unescape").Boolean && SceneManager.GetActiveScene().name == "Battle")
                return;
            if (SceneManager.GetActiveScene().name == "Error" && !modDev)
                return;

            if (GameOverBehavior.gameOverContainer)
                if (GameOverBehavior.gameOverContainer.activeInHierarchy)
                    GameObject.FindObjectOfType<GameOverBehavior>().EndGameOver();
                else
                    UIController.EndBattle();
            else {
                UIController.EndBattle();
            }
            //StaticInits.Reset();
        } else if (input.Menu == UndertaleInput.ButtonState.PRESSED && !nonOWScenes.Contains(SceneManager.GetActiveScene().name) && !isInFight)
            if (!PlayerOverworld.instance.PlayerNoMove && EventManager.instance.script == null && !PlayerOverworld.instance.menuRunning[2] && !PlayerOverworld.instance.menuRunning[4] && EventManager.instance.script == null && GameObject.Find("FadingBlack").GetComponent<Fading>().alpha <= 0)
                StartCoroutine(PlayerOverworld.LaunchMenu());
        
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
          || Input.GetKey(KeyCode.RightAlt))))   // RAlt  + Enter
			SetFullScreen(!Screen.fullScreen);
    }

    void LoadScene(Scene scene, LoadSceneMode mode) {
        if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null)  crate = LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean;
        else                                                              crate = false;
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
                screenShaking = false;
                yield break;
            }
            if (fade)
                intensity = intensityBasis * (1 - (frameCount / frames));
            shift = new Vector2((Random.value - 0.5f) * 2 * intensity, (Random.value - 0.5f) * 2 * intensity);

            if (UnitaleUtil.IsOverworld)
                PlayerOverworld.instance.cameraShift = new Vector2(PlayerOverworld.instance.cameraShift.x + shift.x - totalShift.x, PlayerOverworld.instance.cameraShift.y + shift.y - totalShift.y);
            else
                tf.position = new Vector3(tf.position.x + shift.x - totalShift.x, tf.position.y + shift.y - totalShift.y, tf.position.z);
            //print(totalShift + " + " + shift + " = " + (totalShift + shift));
            totalShift = shift;
            frameCount++;
            yield return 0;
        }
        screenShaking = false;
        tf.position = new Vector3(tf.position.x - totalShift.x, tf.position.y - totalShift.y, tf.position.z);
    }

    public void ShakeScreen(float duration, float intensity, bool isIntensityDecreasing) {
        if (!screenShaking) {
            screenShaking = true;
            StartCoroutine("IShakeScreen", new object[] { duration, intensity, isIntensityDecreasing });
        }
    }

    void OnApplicationQuit() { /*UnitaleUtil.closeFile();*/ }
}
