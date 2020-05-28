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
    public static string CYFversion       = "0.6.5";
    public static string OverworldVersion = "0.6.4";
    public static int frame = 0;
    public static float overworldTimestamp = 0f;
    public static PlayerOverworld po;
    public static UndertaleInput input = new KeyboardInput();
    public static LuaInputBinding luaInput = new LuaInputBinding(input);
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
    //public static bool samariosNightmare = false;
    public static string[] nonOWScenes = new string[] { "Battle", "Error", "ModSelect", "Options", "TitleScreen", "Disclaimer", "EnterName", "TransitionOverworld", "Intro" };
    public static string[] canTransOW = new string[] { "Battle", "Error" };
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

            //Use AlMightyGlobals to load Crate Your Frisk, Safe Mode, Retromode and Fullscreen mode preferences

            if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null && LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean)
                crate = true;
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Misc.WindowName = crate ? ControlPanel.instance.WinodwBsaisNmae : ControlPanel.instance.WindowBasisName;
            #endif

            //Check if safe mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFSafeMode") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Type == DataType.Boolean)
                ControlPanel.instance.Safe = LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Boolean;

            //Check if retro mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFRetroMode") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Type == DataType.Boolean)
                retroMode = LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Boolean;

            //Check if fullscreen mode has a stored preference that is a boolean
            if (LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen").Type == DataType.Boolean)
                ScreenResolution.perfectFullscreen = LuaScriptBinder.GetAlMighty(null, "CYFPerfectFullscreen").Boolean;

            //Check if window scale has a stored preference that is a number
            if (LuaScriptBinder.GetAlMighty(null, "CYFWindowScale") != null
             && LuaScriptBinder.GetAlMighty(null, "CYFWindowScale").Type == DataType.Number)
                ScreenResolution.windowScale = (int)LuaScriptBinder.GetAlMighty(null, "CYFWindowScale").Number;

            //Start Discord RPC (also checks for an AlMightyGlobal within)
            DiscordControls.Start();

            awakened = true;
        }
    }

    public static int fullscreenSwitch = 0;

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
            ScreenResolution.lastMonitorWidth = Screen.currentResolution.width;
            ScreenResolution.lastMonitorHeight = Screen.currentResolution.height;
        } catch {}
    }

    /// <summary>
    /// Control checking, and way more.
    /// </summary>
    void Update () {
        DiscordControls.Update();
        #if UNITY_STANDALONE_WIN
            if (fullscreenSwitch == 1)
                StartCoroutine(RepositionWindow());
            if (fullscreenSwitch > 0)
                fullscreenSwitch--;
        #endif

        if (isInFight || UnitaleUtil.IsOverworld)
            frame ++;

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "ModSelect") lastSceneUnitale = true;
        else                          lastSceneUnitale = false;

        // Activate Debugger
        if (UserDebugger.instance && Input.GetKeyDown(KeyCode.F9) && UserDebugger.instance.canShow) {
            UserDebugger.instance.gameObject.SetActive(!UserDebugger.instance.gameObject.activeSelf);
            Camera.main.GetComponent<FPSDisplay>().enabled = UserDebugger.instance.gameObject.activeSelf;
        // Activate Hitbox Debugger
        } else if (isInFight && Input.GetKeyDown(KeyCode.H) && sceneName != "Error" && UserDebugger.instance.gameObject.activeSelf)
            gameObject.GetComponent<ProjectileHitboxRenderer>().enabled = !gameObject.GetComponent<ProjectileHitboxRenderer>().enabled;
        // Exit a battle or the Error scene
        else if (Input.GetKeyDown(KeyCode.Escape) && (canTransOW.Contains(sceneName) || isInFight)) {
            if (isInFight && LuaEnemyEncounter.script.GetVar("unescape").Boolean && sceneName != "Error")
                return;
            if (sceneName == "Error" && !modDev) {
                UnitaleUtil.ExitOverworld();
                SceneManager.LoadScene("Disclaimer");
                DiscordControls.StartTitle();
                GameObject.Destroy(GameObject.Find("SpritePivot"));
                return;
            }

            if (GameOverBehavior.gameOverContainer)
                if (GameOverBehavior.gameOverContainer.activeInHierarchy)
                    GameObject.FindObjectOfType<GameOverBehavior>().EndGameOver();
                else
                    UIController.EndBattle();
            else
                UIController.EndBattle();
            //StaticInits.Reset();
        //Open the Menu in the Overworld
        } else if (input.Menu == UndertaleInput.ButtonState.PRESSED && !nonOWScenes.Contains(sceneName) && !isInFight && !isInShop && (!GameOverBehavior.gameOverContainerOw || !GameOverBehavior.gameOverContainerOw.activeInHierarchy)) {
            if (!PlayerOverworld.instance.PlayerNoMove && EventManager.instance.script == null && !PlayerOverworld.instance.menuRunning[2] && !PlayerOverworld.instance.menuRunning[4] && (GameObject.Find("FadingBlack") == null || GameObject.Find("FadingBlack").GetComponent<Fading>().alpha <= 0))
                StartCoroutine(PlayerOverworld.LaunchMenu());
        //Wipe save and close CYF in the Error scene if save failed to load
        } else if (sceneName == "Error" && allowWipeSave && Input.GetKeyDown(KeyCode.R)) {
            System.IO.File.Delete(Application.persistentDataPath + "/save.gd");
            Application.Quit();
        }

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
        if (ScreenResolution.hasInitialized)
            if  (Input.GetKeyDown(KeyCode.F4)        // F4
              || (Input.GetKeyDown(KeyCode.Return)
              &&(Input.GetKey(KeyCode.LeftAlt)       // LAlt  + Enter
              || Input.GetKey(KeyCode.RightAlt)))) { // RAlt  + Enter
                ScreenResolution.SetFullScreen(!Screen.fullScreen);
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
            if (stopScreenShake)
                break;
            if (fade)
                intensity = intensityBasis * (1 - (frameCount / frames));
            shift = new Vector2((Random.value - 0.5f) * 2 * intensity, (Random.value - 0.5f) * 2 * intensity);

            Misc.MoveCamera(shift.x - totalShift.x, shift.y - totalShift.y);
            totalShift = shift;
            frameCount++;
            yield return 0;
        }
        Misc.MoveCamera(-totalShift.x, -totalShift.y);
        screenShaking = false;
    }

    public void ShakeScreen(float duration, float intensity, bool isIntensityDecreasing) {
        if (!screenShaking) {
            screenShaking = true;
            stopScreenShake = false;
            StartCoroutine("IShakeScreen", new object[] { duration, intensity, isIntensityDecreasing });
        }
    }

    void OnApplicationQuit() { DiscordControls.discord.Dispose(); }
}
