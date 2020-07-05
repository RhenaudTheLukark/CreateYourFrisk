using System.Diagnostics;
using UnityEngine;

public static class StaticInits {
    public static string MODFOLDER;
    public static string ENCOUNTER = "";
    public static string EDITOR_MODFOLDER = "@Title";
    private static bool firstInit;

    public static bool Initialized { get; set; }

    public delegate void LoadedAction();
    public static event LoadedAction Loaded;

    private static void OnEnable() {  UIController.SendToStaticInit += SendLoaded; }
    private static void OnDisable() { UIController.SendToStaticInit -= SendLoaded; }

    public static void Start() {
        if (!firstInit) {
            firstInit = true;
            SpriteRegistry.Start();
            AudioClipRegistry.Start();
            SpriteFontRegistry.Start();
            ShaderRegistry.Start();
        }
        if (string.IsNullOrEmpty(MODFOLDER))
            MODFOLDER = EDITOR_MODFOLDER;
        //if (CurrMODFOLDER != MODFOLDER || CurrENCOUNTER != ENCOUNTER)
        InitAll();
        Initialized = true;
    }

    public static void InitAll(bool shaders = false) {
        if (!Initialized && (!GlobalControls.isInFight || GlobalControls.modDev)) {
            //UnitaleUtil.createFile();
            Stopwatch sw = new Stopwatch(); //benchmarking terrible loading times
            sw.Start();
            ScriptRegistry.Init();
            sw.Stop();
            UnityEngine.Debug.Log("Script registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            SpriteRegistry.Init();
            sw.Stop();
            UnityEngine.Debug.Log("Sprite registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            AudioClipRegistry.Init();
            sw.Stop();
            UnityEngine.Debug.Log("Audio clip registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            SpriteFontRegistry.Init();
            sw.Stop();
            UnityEngine.Debug.Log("Sprite font registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            if (shaders) {
                sw.Start();
                ShaderRegistry.Init();
                sw.Stop();
                UnityEngine.Debug.Log("Shader registry loading time: " + sw.ElapsedMilliseconds + "ms");
                sw.Reset();
            }
        } else
            Initialized = true;
        LateUpdater.Init(); // must be last; lateupdater's initialization is for classes that depend on the above registries
        MusicManager.src = Camera.main.GetComponent<AudioSource>();
        SendLoaded();
        //CurrENCOUNTER = ENCOUNTER;
        //CurrMODFOLDER = MODFOLDER;
    }

    public static void SendLoaded() {
        if (Loaded != null)
            Loaded();
    }

    /*public static void Reset() {
        Initialized = false;
        LuaScriptBinder.Clear();
        PlayerCharacter.instance.Reset();
    }*/
}