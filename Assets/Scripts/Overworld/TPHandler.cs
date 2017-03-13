using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TPHandler : MonoBehaviour {
    public string sceneName;            //The name of the scene we'll load
    public Vector2 position;            //The future position of our player
    public int uduu2 = GlobalControls.uduu;
    public int direction = 0;           //The direction of the player
    
    private Collider2D playerC2D;       //The player's Collider2D
    private Collider2D objectC2D;       //This object's Collider2D
    private Fading blackFont;           //The fading animation
    private bool playOnce = false;      //Used to play the music only once
    public bool activated = false;     //Checks if we're already in a TP
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
        if (!activated && col == playerC2D) {
            PlayerOverworld.inText = true;  //UnitaleUtil.writeInLogAndDebugger("TPDetected true");
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
        uduu2 = GlobalControls.uduu;
        float fadeTime = blackFont.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);

        Animator anim = GameObject.Find("Player").GetComponent<Animator>();
        if (anim) {
            GameObject.Find("Player").GetComponent<PlayerOverworld>().forcedAnim = true;
            switch (direction) {
                case 2:
                    anim.SetTrigger("MovingDown");
                    anim.SetTrigger("StopLeft");
                    anim.SetTrigger("StopRight");
                    anim.SetTrigger("StopUp");
                    anim.ResetTrigger("StopDown");
                    anim.ResetTrigger("MovingLeft");
                    anim.ResetTrigger("MovingRight");
                    anim.ResetTrigger("MovingUp");
                    break;
                case 4:
                    anim.SetTrigger("StopDown");
                    anim.SetTrigger("MovingLeft");
                    anim.SetTrigger("StopRight");
                    anim.SetTrigger("StopUp");
                    anim.ResetTrigger("MovingDown");
                    anim.ResetTrigger("StopLeft");
                    anim.ResetTrigger("MovingRight");
                    anim.ResetTrigger("MovingUp");
                    break;
                case 6:
                    anim.SetTrigger("StopDown");
                    anim.SetTrigger("StopLeft");
                    anim.SetTrigger("MovingRight");
                    anim.SetTrigger("StopUp");
                    anim.ResetTrigger("MovingDown");
                    anim.ResetTrigger("MovingLeft");
                    anim.ResetTrigger("StopRight");
                    anim.ResetTrigger("MovingUp");
                    break;
                case 8:
                    anim.SetTrigger("StopDown");
                    anim.SetTrigger("StopLeft");
                    anim.SetTrigger("StopRight");
                    anim.SetTrigger("MovingUp");
                    anim.ResetTrigger("MovingDown");
                    anim.ResetTrigger("MovingLeft");
                    anim.ResetTrigger("MovingRight");
                    anim.ResetTrigger("StopUp");
                    break;
            }
        }

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
            StaticInits si = GameObject.Find("Main Camera OW").GetComponent<StaticInits>();
            StaticInits.MODFOLDER = mi.modToLoad;
            StaticInits.Initialized = false;
            si.initAll();
            LuaScriptBinder.Set(null, "ModFolder", MoonSharp.Interpreter.DynValue.NewString(StaticInits.MODFOLDER));
        }

        if (!playOnce) {
            playOnce = true;
            AudioSource audio;
            if (mi.isMusicKeptBetweenBattles)  audio = PlayerOverworld.audioKept;
            else                               audio = MusicManager.src;
           
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
