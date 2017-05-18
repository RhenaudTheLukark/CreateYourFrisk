using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TPHandler : MonoBehaviour {
    public string sceneName;            //The name of the scene we'll load
    public Vector2 position;            //The future position of our player
    public int direction = 0;           //The direction of the player
    public bool activated = false;      //Checks if we're already in a TP
    
    private Collider2D playerC2D;       //The player's Collider2D
    private Collider2D objectC2D;       //This object's Collider2D
    private Fading blackFont;           //The fading animation
    private bool playOnce = false;      //Used to play the music only once
    private bool uduu;                  //You'll see it
    private GameObject blackPic;        //A black picture

    //Use this for initialization
    void Start () {
        //Same for the object we're testing
        objectC2D = GetComponent<Collider2D>();

        //Finds the player in the GameObject list, and store its Collider2D
        playerC2D = GameObject.Find("Player").GetComponent<Collider2D>();
        
        blackPic = GameObject.Find("FadingBlack");
        blackFont = blackPic.GetComponent<Fading>();
    }
    
    void OnTriggerEnter2D(Collider2D col) {
        if (!activated && col == playerC2D && !PlayerOverworld.playerNoMove && EventManager.instance.script == null) {
            activated = true;
            objectC2D.enabled = false;
            StaticInits.MODFOLDER = GameObject.Find("Background").GetComponent<MapInfos>().modToLoad;
            gameObject.transform.SetParent(null);
            GameObject.DontDestroyOnLoad(gameObject);
            StartCoroutine(launchTP());
        }
    }

    //launchTP makes our tp, and lots of other nice stuff
    IEnumerator launchTP() {
        PlayerOverworld.playerNoMove = true; //Launch TP
        switch (GlobalControls.uduu) {
            case 0:
                if (gameObject.name == "TP Up Right") GlobalControls.uduu++;
                break;
            case 1:
                if (gameObject.name == "TP Down Right") GlobalControls.uduu++;
                else GlobalControls.uduu = 0;
                break;
            case 2:
                if (gameObject.name == "TP Up Right") GlobalControls.uduu++;
                else GlobalControls.uduu = 0;
                break;
            case 3:
                if (gameObject.name == "TP Up Right") GlobalControls.uduu++;
                else GlobalControls.uduu = 0;
                break;
            default:
                if (gameObject.name == "TP Up Left")
                    uduu = true;
                GlobalControls.uduu = 0;
                break;
        }
        float fadeTime = blackFont.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);

        PlayerOverworld.instance.gameObject.GetComponent<CYFAnimator>().movementDirection = direction;

        EventManager.SetEventStates();

        if (uduu) {
            uduu = false;
            sceneName = "Secret";
        }
        SceneManager.LoadScene(sceneName);
        StartCoroutine(GameObject.FindObjectOfType<TransitionOverworld>().GetIntoDaMap("tphandler", new object[] { position, this }));
    }

    /*public IEnumerator GetIntoDaMap() {        
        Transform playerPos = GameObject.Find("Player").GetComponent<Transform>();
        playerPos.position = new Vector2 (position.x, position.y);

        yield return 0;
        yield return Application.isLoadingLevel;

        GameObject.Find("Main Camera OW").GetComponent<EventManager>().ResetEvents();

        //Permits to reload the current data if needed
        MapInfos mi = GameObject.Find("Background").GetComponent<MapInfos>();
        if (StaticInits.MODFOLDER != mi.modToLoad) {
            StaticInits.MODFOLDER = mi.modToLoad;
            StaticInits.Initialized = false;
            StaticInits.initAll();
            LuaScriptBinder.Set(null, "ModFolder", MoonSharp.Interpreter.DynValue.NewString(StaticInits.MODFOLDER));
        }

        if (!playOnce) {
            playOnce = true;
            AudioSource audio = UnitaleUtil.GetCurrentOverworldAudio();
           
            //Starts the music if there's no music
            if (audio.clip == null) {
                if (mi.music != "none") {
                    audio.clip = AudioClipRegistry.GetMusic(mi.music);
                    audio.Play();
                } else
                    audio.Stop();
            } else {
                //Get the file's name with this...thing?
                string test = audio.clip.name;

                while (test.Contains("/") || test.Contains("\\")) 
                    test = test.Substring(1);

                for (int i = 0; i < test.Length; i++)
                    if (test.Substring(i, 1) == ".")
                        test = test.Substring(0, i);
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
        }
        float fadeTime2 = blackFont.BeginFade(-1);

        yield return new WaitForSeconds(fadeTime2);

        playOnce = activated = false; //GlobalControls.fadeAuto = false;

        GameObject.Destroy(gameObject);
    }*/
}
