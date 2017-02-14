using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StaticInits : MonoBehaviour {
    //private static string CurrMODFOLDER;
    //private static string CurrENCOUNTER;
    public static string MODFOLDER;
    public static string ENCOUNTER;
    public string EDITOR_MODFOLDER;
    public string EDITOR_ENCOUNTER;
    private static bool firstInit = false;

    public static bool Initialized { get; set; }
    
    public delegate void LoadedAction();
    public static event LoadedAction Loaded;

    void OnEnable() {  UIController.SendToStaticInits += SendLoaded; }
    void OnDisable() { UIController.SendToStaticInits -= SendLoaded; }

    public void Awake() {
        if (!firstInit) {
            firstInit = true;
            SpriteRegistry.Start();
            AudioClipRegistry.Start();
            SpriteFontRegistry.Start();
        }
        if (FindObjectsOfType<StaticInits>().Length != 1) {
            Initialized = true;
            return;
        }
        if (MODFOLDER == null || MODFOLDER == "")
            MODFOLDER = EDITOR_MODFOLDER;
        if (ENCOUNTER == null || ENCOUNTER == "")
            ENCOUNTER = EDITOR_ENCOUNTER;
        //if (CurrMODFOLDER != MODFOLDER || CurrENCOUNTER != ENCOUNTER)
        initAll();
        Initialized = true;
    }

    public void initAll() {
        if (!Initialized && (SceneManager.GetActiveScene().name != "Battle" || GlobalControls.lastSceneUnitale)) {
            UnitaleUtil.createFile();
            if (GlobalControls.lastSceneUnitale)
                GlobalControls.lastSceneUnitale = false;
            Stopwatch sw = new Stopwatch(); //benchmarking terrible loading times
            sw.Start();
            ScriptRegistry.init();
            sw.Stop();
            UnitaleUtil.writeInLog("Script registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            SpriteRegistry.init();
            sw.Stop();
            UnitaleUtil.writeInLog("Sprite registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            AudioClipRegistry.init();
            sw.Stop();
            UnitaleUtil.writeInLog("Audio clip registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            SpriteFontRegistry.init();
            sw.Stop();
            UnitaleUtil.writeInLog("Sprite font registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();
        } else 
            Initialized = true;
        LateUpdater.init(); // must be last; lateupdater's initialization is for classes that depend on the above registries
        MusicManager.src = Camera.main.GetComponent<AudioSource>();
        if (Loaded != null)
            Loaded();
        //CurrENCOUNTER = ENCOUNTER;
        //CurrMODFOLDER = MODFOLDER;
    }

    public void SendLoaded() {
        if (Loaded != null)
            Loaded();
    }

    /*public static void Reset() {
        Initialized = false;
        LuaScriptBinder.Clear();
        PlayerCharacter.instance.Reset();
    }*/
}