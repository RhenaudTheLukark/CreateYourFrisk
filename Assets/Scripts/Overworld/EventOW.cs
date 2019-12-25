using UnityEngine;
using System.Collections.Generic;

public class EventOW : MonoBehaviour {
    public string scriptToLoad;
    public int actualPage;
    public List<Vector2> eventTriggers = new List<Vector2>();
    public float moveSpeed;
    [HideInInspector] public ScriptWrapper isMovingSource;
    [HideInInspector] public bool isMovingWaitEnd = false;
    [HideInInspector] public ScriptWrapper isRotatingSource;
    [HideInInspector] public bool isRotatingWaitEnd = false;

    public void OnTriggerEnter2D(Collider2D col) {
        //Debug.Log("Frame " + GlobalControls.frame + ": " + (!EventManager.instance.readyToReLaunch) + " && " + (EventManager.instance.script == null) + " && " + (!EventManager.instance.ScriptLaunched) + " && " + (!EventManager.instance.LoadLaunched) + " && " + (!PlayerOverworld.instance.inBattleAnim) + " && " + (!PlayerOverworld.instance.menuRunning[2]));
        if (!EventManager.instance.readyToReLaunch && EventManager.instance.script == null && !EventManager.instance.ScriptLaunched && !EventManager.instance.LoadLaunched && !PlayerOverworld.instance.inBattleAnim && !PlayerOverworld.instance.menuRunning[2])
            if (EventManager.instance.GetTrigger(gameObject, actualPage) == 1 && col == GameObject.Find("Player").GetComponent<BoxCollider2D>())
                EventManager.instance.ExecuteEvent(gameObject);
    }
}
