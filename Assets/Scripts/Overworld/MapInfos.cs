using UnityEngine;
using UnityEngine.SceneManagement;

public class MapInfos : MonoBehaviour {
    public int id;
    public string music;
    public string modToLoad;
    public bool isMusicKeptBetweenBattles;
    public bool noRandomEncounter;

    private void Start() {
        EventManager.GetEventStates(SceneManager.GetActiveScene().buildIndex);
    }
}
