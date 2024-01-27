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
    public static string CYFversion       = "0.6.6";    // Current version of CYF displayed in the main menu and usable in scripts
    public static string OverworldVersion = "0.6.6";    // Last version in which the overworld was changed, notifying any user with an old save to delete it
    public static int    LTSversion       = 2;          // LTS version, mainly used for CYF 0.6.6
    public static int    BetaVersion      = 24;         // Only used for beta versions

    public static int frame;                        // Frame counter used for logging purposes
    public static float overworldTimestamp = 0f;    // Timestamp of the creation of the save file, mostly used to know the time spent in this save in the save and load screen

    public static IUndertaleInput input = new KeyboardInput();              // KeyboardInput singleton, registering any key press the Player does and handling them
    public static LuaInputBinding luaInput = new LuaInputBinding(input);    // Input Lua object, usable on the Lua side

    public static string realName;      // Player's name in the overworld, given through the scene EnterName
    public static bool modDev;          // True if we entered the mod selection screen and not the overworld, false otherwise
    public static bool crate;           // True if CrateYourFrisk mode is active, false otherwise
    public static bool retroMode;       // True if the Unitale 0.2.1a retrocompatibility mode is active, false otherwise
    public static bool stopScreenShake; // Used to stop any screenshake currently ongoing
    public static bool isInFight;       // True if we're in a battle, false otherwise
    public static bool isInShop;        // True if we're in a shop, false otherwise
    public static bool allowWipeSave;   // Allows you to wipe your save in the Error scene if it couldn't load properly
    private bool screenShaking;         // True if a screenshake is occuring, false otherwise

    public static string[] nonOWScenes = { "Battle", "Error", "ModSelect", "Options", "TitleScreen", "Disclaimer", "EnterName", "TransitionOverworld", "Intro", "KeybindSettings" };   // Scenes in which you're not considered to be in the overworld
    public static string[] canTransOW = { "Battle", "Error" };  // Scenes from which you can enter the overworld

    public static Dictionary<string, GameState.MapData> GameMapData = new Dictionary<string, GameState.MapData>();              // Main save data on each map the Player has visited before
    public static Dictionary<string, GameState.EventInfos> EventData = new Dictionary<string, GameState.EventInfos>();          // Data stored for each event in the current map, used for data saving
    public static Dictionary<string, GameState.TempMapData> TempGameMapData = new Dictionary<string, GameState.TempMapData>();  // Data used to save changes applied to maps the Player hasn't visited yet

    private static bool awakened;   // Used to only run Awake() once

    public void Awake() {
        if (awakened) return;
        // Create all singletons (classes that only have one instance across the entire app)
        StaticInits.Start();
        SaveLoad.Start();
        new ControlPanel();
        new PlayerCharacter();
        // Load AlMighty globals
        SaveLoad.LoadAlMighty();
        LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString("@Title"));

        KeyboardInput.LoadPlayerKeys();

        // Load map names for the overworld
        UnitaleUtil.AddKeysToMapCorrespondanceList();

        // Use AlMightyGlobals to load Crate Your Frisk, Safe Mode, Retromode and Fullscreen mode preferences
        ReloadCrate();

        // Check if safe mode has a stored preference that is a boolean
        if (LuaScriptBinder.GetAlMighty(null, "CYFSafeMode")      != null
         && LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Type == DataType.Boolean)
            ControlPanel.instance.Safe = LuaScriptBinder.GetAlMighty(null, "CYFSafeMode").Boolean;

        // Check if retro mode has a stored preference that is a boolean
        if (LuaScriptBinder.GetAlMighty(null, "CYFRetroMode")      != null
         && LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Type == DataType.Boolean)
            retroMode = LuaScriptBinder.GetAlMighty(null, "CYFRetroMode").Boolean;

        // Check if window scale has a stored preference that is a number
        if (LuaScriptBinder.GetAlMighty(null, "CYFWindowScale")      != null
         && LuaScriptBinder.GetAlMighty(null, "CYFWindowScale").Type == DataType.Number) {
            ScreenResolution.windowScale = (int) System.Math.Max(LuaScriptBinder.GetAlMighty(null, "CYFWindowScale").Number, 1);
            if (!ScreenResolution.hasInitialized) {
                Screen.SetResolution(640, 480, Screen.fullScreen, 0);
                ScreenResolution scrRes = FindObjectOfType<ScreenResolution>();
                if (scrRes) scrRes.Start();
            }
            ScreenResolution.ResetAfterBattle();
        }

        // Start Discord RPC (also checks for an AlMightyGlobal within)
        DiscordControls.Start();

        awakened = true;
    }

    public static void ReloadCrate() {
        if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null && LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean)
            crate = true;
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Misc.WindowName = crate ? ControlPanel.instance.WinodwBsaisNmae : ControlPanel.instance.WindowBasisName;
        #endif
    }

    #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        /// <summary>
        /// Used to reposition the window in the middle of the screen after exiting fullscreen.
        /// </summary>
        /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
        public static int fullscreenSwitch;

        static IEnumerator RepositionWindow() {
            yield return new WaitForEndOfFrame();
            try {
                Misc.MoveWindowTo(Screen.currentResolution.width / 2 - Screen.width / 2, Screen.currentResolution.height / 2 - Screen.height / 2);
            } catch { /* ignored */ }
    }
    #endif

    /// <summary>
    /// Updates the stored size of the monitor.
    /// </summary>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    public static IEnumerator UpdateMonitorSize() {
        yield return new WaitForEndOfFrame();

        try {
            ScreenResolution.lastMonitorSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        } catch { /* ignored */ }
    }

    /// <summary>
    /// Run once per frame.
    /// </summary>
    private void Update () {
        // Update Discord RPC
        DiscordControls.Update();

        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            // Reposition the window to the middle of the screen after exiting fullscreen
            if (fullscreenSwitch == 1)
                StartCoroutine(RepositionWindow());
            if (fullscreenSwitch > 0)
                fullscreenSwitch--;
        #endif

        // Frame counter used for logging purposes
        if (isInFight || UnitaleUtil.IsOverworld)
            frame ++;

        string sceneName = SceneManager.GetActiveScene().name;

        // Activate Debugger
        if (UserDebugger.instance && Input.GetKeyDown(KeyCode.F9) && UserDebugger.instance.canShow) {
            UserDebugger.instance.gameObject.SetActive(!UserDebugger.instance.gameObject.activeSelf);
            Camera.main.GetComponent<FPSDisplay>().enabled = UserDebugger.instance.gameObject.activeSelf;
        }
        // Activate Hitbox Debugger
        else if (isInFight && Input.GetKeyDown(KeyCode.H) && sceneName != "Error" && UserDebugger.instance.gameObject.activeSelf)
            gameObject.GetComponent<ProjectileHitboxRenderer>().enabled = !gameObject.GetComponent<ProjectileHitboxRenderer>().enabled;
        // Exit a battle or the Error scene
        else if (Input.GetKeyDown(KeyCode.Escape) && (canTransOW.Contains(sceneName) || isInFight)) {
            if (isInFight && EnemyEncounter.script.GetVar("unescape").Boolean && sceneName != "Error") return;
            // The Error scene can only be exited if we entered the mod through the mod selection screen
            if (sceneName == "Error" && !modDev) {
                ScreenResolution.ResetAfterBattle();
                UnitaleUtil.ExitOverworld();
                SceneManager.LoadScene("Disclaimer");
                DiscordControls.StartTitle();
                Destroy(GameObject.Find("SpritePivot"));
                return;
            }

            if (GameOverBehavior.gameOverContainer)
                if (GameOverBehavior.gameOverContainer.activeInHierarchy) FindObjectOfType<GameOverBehavior>().EndGameOver();
                else                                                      UIController.EndBattle();
            else                                                          UIController.EndBattle();
        }
        // Open the Menu in the Overworld
        else if (input.Menu == ButtonState.PRESSED && !nonOWScenes.Contains(sceneName) && !isInFight && !isInShop && (!GameOverBehavior.gameOverContainerOw || !GameOverBehavior.gameOverContainerOw.activeInHierarchy)) {
            if (!PlayerOverworld.instance.PlayerNoMove && EventManager.instance.script == null && !PlayerOverworld.instance.menuRunning[2] && !PlayerOverworld.instance.menuRunning[4] && (GameObject.Find("FadingBlack") == null || GameObject.Find("FadingBlack").GetComponent<Fading>().alpha <= 0))
                StartCoroutine(PlayerOverworld.LaunchMenu());
        }
        // Wipe save and close CYF in the Error scene if save failed to load
        else if (sceneName == "Error" && allowWipeSave && Input.GetKeyDown(KeyCode.R)) {
            System.IO.File.Delete(Application.persistentDataPath + "/save.gd");
            Application.Quit();
        }

        // Enter fullscreen using given shortcuts
        if (!ScreenResolution.hasInitialized) return;
        if (Input.GetKeyDown(KeyCode.F4) || (Input.GetKeyDown(KeyCode.Return) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))) {
            ScreenResolution.SetFullScreen(!Screen.fullScreen);
            if (!Screen.fullScreen)
                StartCoroutine(UpdateMonitorSize());
        }
    }

    /// <summary>
    /// Runs pnce per frame, after all other update functions are run.
    /// </summary>
    public void LateUpdate() {
        input.LateUpdate();
    }

    /// <summary>
    /// Shakes the screen for a given amount of frames.
    /// </summary>
    /// <param name="args">Most coroutines have the same argument, which is a table of values.
    /// This one should include, in order:
    /// * float frames - The amount of frames the screenshake effect will be active for.
    /// * float intensity - The amount of pixels the screen can move out of its original position at maximum.
    /// * bool fade - True if the screenshake effect should be reduced over time, false otherwise.
    /// </param>
    /// <returns>All coroutines must return an IEnumerator object, don't mind it.</returns>
    private IEnumerator IShakeScreen(IList<object> args) {
        float frames, intensity;
        bool fade;

        try { frames = (float)args[0]; } catch { throw new CYFException("The argument \"seconds\" must be a number."); }
        try { intensity = (float)args[1]; } catch { throw new CYFException("The argument \"intensity\" must be a number."); }
        try { fade = (bool)args[2]; } catch { throw new CYFException("The argument \"fade\" must be a boolean."); }

        Vector2 totalShift = new Vector2(0, 0);
        float frameCount = 0, intensityBasis = intensity;
        while (frameCount < frames) {
            if (stopScreenShake)
                break;
            if (fade)
                intensity = intensityBasis * (1 - (frameCount / frames));
            Vector2 shift = new Vector2((Random.value - 0.5f) * 2 * intensity, (Random.value - 0.5f) * 2 * intensity);

            Misc.MoveCamera(shift.x - totalShift.x, shift.y - totalShift.y);
            totalShift = shift;
            frameCount++;
            yield return 0;
        }
        Misc.MoveCamera(-totalShift.x, -totalShift.y);
        screenShaking = false;
    }

    /// <summary>
    /// Starts the screen shaking coroutine.
    /// </summary>
    /// <param name="duration">The amount of frames the screenshake effect will be active for</param>
    /// <param name="intensity">The amount of pixels the screen can move out of its original position at maximum.</param>
    /// <param name="isIntensityDecreasing">True if the screenshake effect should be reduced over time, false otherwise.</param>
    public void ShakeScreen(float duration, float intensity, bool isIntensityDecreasing) {
        if (screenShaking) return;
        screenShaking   = true;
        stopScreenShake = false;
        StartCoroutine("IShakeScreen", new object[] { duration, intensity, isIntensityDecreasing });
    }

    /// <summary>
    /// Only run when the application is closed.
    /// </summary>
    private void OnApplicationQuit() {
        if (DiscordControls.isActive)
            DiscordControls.discord.Dispose();
    }
}
