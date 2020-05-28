using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using MoonSharp.Interpreter;

public class TransitionOverworld : MonoBehaviour {
    public string FirstLevelToLoad;
    public Vector2 BeginningPosition;

    private void Start() {
        bool isStart = false;

        // Set timestamp for Overworld to calculate total play time
        GlobalControls.overworldTimestamp = Time.time - (SaveLoad.savedGame != null ? SaveLoad.savedGame.playerTime : 0f);
        // Forcefully disable retromode if it is on
        if (GlobalControls.retroMode) {
            GlobalControls.retroMode = false;
            try {
                LuaScriptBinder.SetAlMighty(null, "CYFRetroMode", DynValue.NewBoolean(false), true);
            } catch {}
        }

        GameOverBehavior.gameOverContainerOw = GameObject.Find("GameOverContainer");
        GameOverBehavior.gameOverContainerOw.SetActive(false);
        if (GameObject.Find("GameOverContainer")) {
            GameObject.Destroy(GameOverBehavior.gameOverContainerOw);
            GameOverBehavior.gameOverContainerOw = GameObject.Find("GameOverContainer");
            GameOverBehavior.gameOverContainerOw.SetActive(false);
        }
        GameObject.DontDestroyOnLoad(GameOverBehavior.gameOverContainerOw);

        if (LuaScriptBinder.Get(null, "PlayerPosX") == null || LuaScriptBinder.Get(null, "PlayerPosY") == null || LuaScriptBinder.Get(null, "PlayerPosZ") == null) {
            LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(BeginningPosition.x));
            LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(BeginningPosition.y));
            LuaScriptBinder.Set(null, "PlayerPosZ", DynValue.NewNumber(0));
        }
        if (GameObject.Find("Main Camera"))
            GameObject.Destroy(GameObject.Find("Main Camera"));
        //Used only for the 1st scene
        if (LuaScriptBinder.Get(null, "PlayerMap") == null) {
            isStart = true;
            SaveLoad.Start();

            GlobalControls.lastTitle = false;
            string mapName2;
            if (UnitaleUtil.MapCorrespondanceList.ContainsKey(FirstLevelToLoad))  mapName2 = UnitaleUtil.MapCorrespondanceList[FirstLevelToLoad];
            else                                                                  mapName2 = FirstLevelToLoad;
            LuaScriptBinder.Set(null, "PlayerMap", DynValue.NewString(mapName2));

            StaticInits.MODFOLDER = "";
            /*StaticInits.Initialized = false;
            GameObject.Find("Main Camera OW").GetComponent<StaticInits>().initAll();*/
            GlobalControls.realName = PlayerCharacter.instance.Name;
        }
        //Check if there is two Main Camera OW objects
        GameObject temp = GameObject.Find("Main Camera OW");
        temp.SetActive(false);
        if (GameObject.Find("Main Camera OW"))
            GameObject.Destroy(GameObject.Find("Main Camera OW"));
        temp.SetActive(true);

        // After battle tweaks
        ControlPanel.instance.FrameBasedMovement = false;
        if (GlobalControls.realName != null)
            PlayerCharacter.instance.Name = GlobalControls.realName;

        //GameObject.Destroy(gameObject);

        GameObject.DontDestroyOnLoad(GameObject.Find("Canvas OW"));
        GameObject.DontDestroyOnLoad(GameObject.Find("Canvas Two"));
        GameObject.DontDestroyOnLoad(GameObject.Find("Player").transform.parent.gameObject);
        GameObject.DontDestroyOnLoad(GameObject.Find("Main Camera OW"));
        string mapName;
        if (!isStart)
            try {
                if (UnitaleUtil.MapCorrespondanceList.ContainsValue(LuaScriptBinder.Get(null, "PlayerMap").String))
                    mapName = UnitaleUtil.MapCorrespondanceList.FirstOrDefault(x => x.Value == LuaScriptBinder.Get(null, "PlayerMap").String).Key;
                else mapName = LuaScriptBinder.Get(null, "PlayerMap").String;
            } catch { mapName = LuaScriptBinder.Get(null, "PlayerMap").String; }
        else
            mapName = FirstLevelToLoad;

        if (!FileLoader.SceneExists(mapName)) {
            UnitaleUtil.DisplayLuaError("TransitionOverworld", "The map named \"" + mapName + "\" doesn't exist.");
            return;
        }
        if (GlobalControls.nonOWScenes.Contains(mapName)) {
            UnitaleUtil.DisplayLuaError("TransitionOverworld", "Sorry, but \"" + mapName + "\" is not the name of an overworld scene.");
            return;
        }
        SceneManager.LoadScene(mapName);
        GameObject.Find("Don't show it again").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        StartCoroutine(GetIntoDaMap("transitionoverworld", null));
    }

    public static IEnumerator GetIntoDaMap(string call, object[] neededArgs) {
        if (GameObject.Find("Main Camera OW")) {
            GameObject.Find("Main Camera OW").GetComponent<EventManager>().readyToReLaunch = true;
            GameObject.Find("Main Camera OW").tag = "MainCamera";
        }

        //Clear any leftover Sprite and Text objects that are no longer connected to any scripts
        foreach (Transform child in GameObject.Find("Canvas Two").transform)
            if (!child.name.EndsWith("Layer"))
                GameObject.Destroy(child.gameObject);
            else {
                foreach (Transform child2 in child)
                    GameObject.Destroy(child2.gameObject);
            }

        //Reset the player's shader between rooms. The player should realistically be the only sprite object carried between scenes.
        if (PlayerOverworld.instance && PlayerOverworld.instance.sprctrl != null)
            PlayerOverworld.instance.sprctrl.shader.Revert();

        yield return 0;

        Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
        Camera.main.transparencySortAxis = new Vector3(0.0f, 1.0f, 1000000.0f);

        try { PlayerOverworld.instance.backgroundSize = GameObject.Find("Background").GetComponent<RectTransform>().sizeDelta * GameObject.Find("Background").GetComponent<RectTransform>().localScale.x; }
        catch { UnitaleUtil.WriteInLogAndDebugger("RectifyCameraPosition: The 'Background' GameObject is missing."); }

        EventManager.instance.onceReload = false;
        //Permits to reload the current data if needed
        MapInfos mi = GameObject.Find("Background").GetComponent<MapInfos>();
        if (StaticInits.MODFOLDER != mi.modToLoad) {
            StaticInits.MODFOLDER = mi.modToLoad;
            StaticInits.Initialized = false;
            StaticInits.InitAll(true);
            LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString(StaticInits.MODFOLDER));
            if (call == "transitionoverworld") {
                EventManager.instance.ScriptLaunched = false;
                EventManager.instance.script = null;
            }
        }

        AudioSource audio = UnitaleUtil.GetCurrentOverworldAudio();
        if (mi.isMusicKeptBetweenBattles) {
            Camera.main.GetComponent<AudioSource>().Stop();
            Camera.main.GetComponent<AudioSource>().clip = null;
        } else {
            PlayerOverworld.audioKept.Stop();
            PlayerOverworld.audioKept.clip = null;
        }

        //Starts the music if there's no music
        if (audio.clip == null) {
            if (mi.music != "none") {
                audio.clip = AudioClipRegistry.GetMusic(mi.music);
                audio.time = 0;
                audio.Play();
            } else
                audio.Stop();
        } else {
            //Get the file's name with this...thing?
            string test = audio.clip.name.Replace('\\', '/').Split(new string[] { "/Audio/" }, System.StringSplitOptions.RemoveEmptyEntries)[1].Split('.')[0];
            if (test != mi.music) {
                if (mi.music != "none") {
                    audio.clip = AudioClipRegistry.GetMusic(mi.music);
                    audio.time = 0;
                    audio.Play();
                } else
                    audio.Stop();
            }
        }

        Image utHeart = GameObject.Find("utHeart").GetComponent<Image>();
        utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 0);
        PlayerOverworld.instance.cameraShift = Vector2.zero;
        if (call == "tphandler") {
            GameObject.Find("Player").transform.parent.position = (Vector2)neededArgs[0];
            PlayerOverworld.instance.gameObject.GetComponent<CYFAnimator>().movementDirection = ((TPHandler)neededArgs[1]).direction;
            ((TPHandler)neededArgs[1]).activated = false;
            GameObject.Destroy(((TPHandler)neededArgs[1]).gameObject);
        }

        if (GameObject.Find("Don't show it again"))
            GameObject.Destroy(GameObject.Find("Don't show it again"));
        DiscordControls.ShowOWScene(SceneManager.GetActiveScene().name);
        StaticInits.SendLoaded();
    }
}
