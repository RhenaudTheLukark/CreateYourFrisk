using UnityEngine;
using UnityEngine.SceneManagement;

public class MapInfos : MonoBehaviour {
    public string music;
    public string modToLoad;
    public bool isMusicKeptBetweenBattles;
    public bool noRandomEncounter;

    private void Start() {
        EventManager.GetMapState(this, SceneManager.GetActiveScene().name);
    }
}