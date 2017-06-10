using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using MoonSharp.Interpreter;

public class TransitionOverworld : MonoBehaviour {
    private TransitionOverworld instance;
    public string FirstLevelToLoad;
    public Vector2 BeginningPosition;

    private void Start() {
        if (instance)
            return;
        bool isStart = false;

        GameOverBehavior.gameOverContainerOw = GameObject.Find("GameOverContainer");
        GameOverBehavior.gameOverContainerOw.SetActive(false);
        if (GameObject.Find("GameOverContainer")) {
            GameObject.Destroy(GameOverBehavior.gameOverContainerOw);
            GameOverBehavior.gameOverContainerOw = GameObject.Find("GameOverContainer");
            GameOverBehavior.gameOverContainerOw.SetActive(false);
        }
        GameObject.DontDestroyOnLoad(GameOverBehavior.gameOverContainerOw);

        GlobalControls.beginPosition = BeginningPosition;
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
        GameObject.DontDestroyOnLoad(GameObject.Find("Player"));
        GameObject.DontDestroyOnLoad(GameObject.Find("Main Camera OW"));
        string mapName;
        if (!isStart) {
            try {
                if (UnitaleUtil.MapCorrespondanceList.ContainsValue(LuaScriptBinder.Get(null, "PlayerMap").String))
                    mapName = UnitaleUtil.MapCorrespondanceList.FirstOrDefault(x => x.Value == LuaScriptBinder.Get(null, "PlayerMap").String).Key;
                else mapName = LuaScriptBinder.Get(null, "PlayerMap").String;
            } catch { mapName = LuaScriptBinder.Get(null, "PlayerMap").String; }
        } else
            mapName = FirstLevelToLoad;

        //THERE IS NO WAY TO KNOW IF A SCENE EXISTS WTF
        /*bool loaded = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            if (SceneManager.GetSceneAt(i).name == mapName) {
                loaded = true;*/
                SceneManager.LoadScene(mapName);
        /*break;
            }
        }
        if (!loaded) {
            UnitaleUtil.displayLuaError("TransitionOverworld", "The map named \"" + mapName + "\" doesn't exist.");
            return;
        }*/
        GameObject.Find("Don't show it again").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        instance = this;
        StartCoroutine(GetIntoDaMap("transitionoverworld", null));
    }

    public IEnumerator GetIntoDaMap(string call, object[] neededArgs) {
        //GlobalControls.fadeAuto = true;
        GameObject.Find("Main Camera OW").GetComponent<EventManager>().readyToReLaunch = true;
        GameObject.Find("Main Camera OW").tag = "MainCamera";

        yield return 0;

        Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
        Camera.main.transparencySortAxis = new Vector3(0.0f, 1.0f, 1000000.0f);

        EventManager.instance.onceReload = false;
        //Permits to reload the current data if needed
        MapInfos mi = GameObject.Find("Background").GetComponent<MapInfos>();
        if (StaticInits.MODFOLDER != mi.modToLoad) {
            StaticInits.MODFOLDER = mi.modToLoad;
            StaticInits.Initialized = false;
            StaticInits.initAll();
            LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString(StaticInits.MODFOLDER));
            if (call == "transitionoverworld") {
                EventManager.instance.scriptLaunched = false;
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
                audio.Play();
            } else
                audio.Stop();
        } else {
            //Get the file's name with this...thing?
            string test = audio.clip.name.Replace('\\', '/').Split(new string[] { "/Audio/" }, System.StringSplitOptions.RemoveEmptyEntries)[1].Split('.')[0];

            if (test != mi.music) {
                if (mi.music != "none") {
                    audio.clip = AudioClipRegistry.GetMusic(mi.music);
                    audio.Play();
                } else
                    audio.Stop();
            }
        }

        GameObject.Find("utHeart").GetComponent<Image>().color = new Color(GameObject.Find("utHeart").GetComponent<Image>().color.r, GameObject.Find("utHeart").GetComponent<Image>().color.g,
                                                                           GameObject.Find("utHeart").GetComponent<Image>().color.b, 0);
        if (call == "tphandler") {
            Transform playerPos = GameObject.Find("Player").GetComponent<Transform>();
            playerPos.position = (Vector2)neededArgs[0];
            PlayerOverworld.instance.gameObject.GetComponent<CYFAnimator>().movementDirection = ((TPHandler)neededArgs[1]).direction;
            ((TPHandler)neededArgs[1]).activated = false;
            GameObject.Destroy(((TPHandler)neededArgs[1]).gameObject);
        }

        if (GameObject.Find("Don't show it again"))
            GameObject.Destroy(GameObject.Find("Don't show it again"));
        StaticInits.SendLoaded();
    }
}
