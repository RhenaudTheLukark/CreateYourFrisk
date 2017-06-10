using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TPHandler : MonoBehaviour {
    public string sceneName;            //The name of the scene we'll load
    public Vector2 position;            //The future position of our player
    public int direction = 0;           //The direction of the player
    public bool activated = false;      //Checks if we're already in a TP
    
    private Collider2D playerC2D;       //The player's Collider2D
    private Collider2D objectC2D;       //This object's Collider2D
    private bool uduu;                  //You'll see it

    //Use this for initialization
    void Start () {
        //Same for the object we're testing
        objectC2D = GetComponent<Collider2D>();

        if (!GlobalControls.isInShop) {
            //Finds the player in the GameObject list, and store its Collider2D
            playerC2D = GameObject.Find("Player").GetComponent<Collider2D>();
        }
    }
    
    void OnTriggerEnter2D(Collider2D col) {
        if (!activated && col == playerC2D && !PlayerOverworld.instance.playerNoMove && EventManager.instance.script == null) {
            activated = true;
            objectC2D.enabled = false;
            StaticInits.MODFOLDER = GameObject.Find("Background").GetComponent<MapInfos>().modToLoad;
            gameObject.transform.SetParent(null);
            GameObject.DontDestroyOnLoad(gameObject);
            StartCoroutine(launchTP());
        }
    }

    //launchTP makes our tp, and lots of other nice stuff
    public IEnumerator launchTP() {
        PlayerOverworld.instance.playerNoMove = true; //Launch TP
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
        float fadeTime = GameObject.Find("FadingBlack").GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);

        if (GlobalControls.isInShop) {
            PlayerOverworld.ShowOverworld("Shop");
            GlobalControls.isInShop = false;
        }
        EventManager.SetEventStates();

        if (uduu) {
            uduu = false;
            sceneName = "Secret";
        }
        SceneManager.LoadScene(sceneName);
        StartCoroutine(GameObject.FindObjectOfType<TransitionOverworld>().GetIntoDaMap("tphandler", new object[] { position, this }));
    }
}
