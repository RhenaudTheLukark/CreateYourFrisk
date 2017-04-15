using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using MoonSharp.Interpreter;

public class TransitionOverworld : MonoBehaviour {
    public string FirstLevelToLoad;
    public Vector2 BeginningPosition;
    private string call;
    private object[] neededArgs;

    private void OnEnable()  { StaticInits.Loaded += LaunchFade; }
    private void OnDisable() { StaticInits.Loaded -= LaunchFade; }

    private void Start() {
        bool isStart = false;

        GameOverBehavior.gameOverContainer = GameObject.Find("GameOverContainer");
        GameOverBehavior.gameOverContainer.SetActive(false);
        if (GameObject.Find("GameOverContainer")) {
            GameObject.Destroy(GameOverBehavior.gameOverContainer);
            GameOverBehavior.gameOverContainer = GameObject.Find("GameOverContainer");
            GameOverBehavior.gameOverContainer.SetActive(false);
        }
        GameObject.DontDestroyOnLoad(GameOverBehavior.gameOverContainer);

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
        if (GameObject.Find("Main Camera OW")) { 
            GameObject.Destroy(GameObject.Find("Main Camera OW"));
            temp.GetComponent<EventManager>().readyToReLaunch = true;
        }        
        temp.SetActive(true);

        //MIONNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN
        /*GameObject goodluck = Resources.Load<GameObject>("Prefabs/MIONNNNNNNNNNNN");
        GameObject gl = Instantiate(goodluck);
        gl.name = "MIONNNNNNNNNNNN";
        gl.transform.SetParent(GameObject.Find("Canvas OW").transform);*/
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
        StartCoroutine(GetIntoDaMap("transitionoverworld", null));
    }

    public IEnumerator GetIntoDaMap(string call, object[] neededArgs) {
        this.call = call;
        this.neededArgs = neededArgs;
        //GlobalControls.fadeAuto = true;
        GameObject.Find("Main Camera OW").GetComponent<EventManager>().readyToReLaunch = true;
        GameObject.Find("Main Camera OW").tag = "MainCamera";

        yield return 0;

        bool neededReload = false;
        //Permits to reload the current data if needed
        MapInfos mi = GameObject.Find("Background").GetComponent<MapInfos>();
        if (StaticInits.MODFOLDER != mi.modToLoad) {
            StaticInits si = GameObject.Find("Main Camera OW").GetComponent<StaticInits>();
            StaticInits.MODFOLDER = mi.modToLoad;
            StaticInits.Initialized = false;
            si.initAll();
            LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString(StaticInits.MODFOLDER));
            if (call == "transitionoverworld")
                EventManager.instance.scriptLaunched = false;
            neededReload = true;
        }

        AudioSource audio;
        if (mi.isMusicKeptBetweenBattles) audio = PlayerOverworld.audioKept;
        else audio = Camera.main.GetComponent<AudioSource>();

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
        if (audio == PlayerOverworld.audioKept) {
            MusicManager.src.Stop();
            MusicManager.src.clip = null;
        } else {
            PlayerOverworld.audioKept.Stop();
            PlayerOverworld.audioKept.clip = null;
        }

        EventManager.instance.ResetEvents();

        GameObject.Find("utHeart").GetComponent<Image>().color = new Color(GameObject.Find("utHeart").GetComponent<Image>().color.r, 
                                                                           GameObject.Find("utHeart").GetComponent<Image>().color.g,
                                                                           GameObject.Find("utHeart").GetComponent<Image>().color.b, 0);
        if (call == "tphandler") {
            Transform playerPos = GameObject.Find("Player").GetComponent<Transform>();
            playerPos.position = (Vector2)neededArgs[0];
        }
        //GlobalControls.fadeAuto = false;
        //GameObject.Destroy(gameObject);
        if (!neededReload)
            LaunchFade();
    }

    private void LaunchFade() { StartCoroutine(LaunchFade2()); }

    private IEnumerator LaunchFade2() {
        float fadeTime2 = GameObject.Find("FadingBlack").GetComponent<Fading>().BeginFade(-1);

        yield return new WaitForSeconds(fadeTime2);

        //yield return new WaitForSeconds(GameObject.Find("FadingBlack").GetComponent<Fading>().fadeSpeed);
        if (call == "tphandler") {
            ((TPHandler)neededArgs[1]).activated = false;
            GameObject.Destroy(((TPHandler)neededArgs[1]).gameObject);
        }

        if (GameObject.Find("Don't show it again"))
            GameObject.Destroy(GameObject.Find("Don't show it again"));
    }
}
