using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class TPHandler : MonoBehaviour {
    public string sceneName;            //The name of the scene we'll load
    public Vector2 position;            //The future position of our player
    public int direction = 0;           //The direction of the player
    public bool activated;              //Checks if we're already in a TP
    public bool noFadeIn;
    public bool noFadeOut;

    private Collider2D playerC2D;       //The player's Collider2D
    private Collider2D objectC2D;       //This object's Collider2D

    //Use this for initialization
    private void Start() {
        //Same for the object we're testing
        objectC2D = GetComponent<Collider2D>();

        if (!GlobalControls.isInShop) {
            //Finds the player in the GameObject list, and store its Collider2D
            playerC2D = GameObject.Find("Player").GetComponent<Collider2D>();
        }
    }

    private void OnTriggerEnter2D(Object col) {
        if (activated || col != playerC2D || PlayerOverworld.instance.PlayerNoMove || EventManager.instance.script != null) return;
        activated = true;
        objectC2D.enabled = false;
        StaticInits.MODFOLDER = GameObject.Find("Background").GetComponent<MapInfos>().modToLoad;
        gameObject.transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LaunchTP());
    }

    //launchTP makes our tp, and lots of other nice stuff
    public IEnumerator LaunchTP() {
        PlayerOverworld.instance.PlayerNoMove = true; //Launch TP
        if (GameObject.Find("FadingBlack"))
            if (!noFadeIn) {
                float fadeTime = GameObject.Find("FadingBlack").GetComponent<Fading>().BeginFade(1);
                yield return new WaitForSeconds(fadeTime);
            } else
                GameObject.Find("FadingBlack").GetComponent<Fading>().FadeInstant(1);
        EventManager.instance.fadeOutToMap = !noFadeOut;

        if (GlobalControls.isInShop) {
            PlayerOverworld.ShowOverworld("Shop");
            GlobalControls.isInShop = false;
        }
        EventManager.instance.SetEventStates();
        GlobalControls.EventData.Clear();

        if (!FileLoader.SceneExists(sceneName)) {
            UnitaleUtil.DisplayLuaError("Teleportation script", "The map named \"" + sceneName + "\" doesn't exist.");
            yield break;
        }
        if (GlobalControls.nonOWScenes.Contains(sceneName)) {
            UnitaleUtil.DisplayLuaError("Teleportation script", "Sorry, but \"" + sceneName + "\" is not the name of an overworld scene.");
            yield break;
        }
        SceneManager.LoadScene(sceneName);
        StartCoroutine(TransitionOverworld.GetIntoDaMap("tphandler", new object[] { position, this }));
    }

    public void LaunchTPInternal() {
        StartCoroutine(LaunchTP());
    }
}
