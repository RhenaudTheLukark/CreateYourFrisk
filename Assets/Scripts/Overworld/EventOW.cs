using UnityEngine;
using System.Collections.Generic;

public class EventOW : MonoBehaviour {
    public string scriptToLoad;
    public int actualPage;
    public List<Vector2> eventTriggers = new List<Vector2>();
    public int moveSpeed;

    public void OnTriggerEnter2D(Collider2D col) {
        if (!PlayerOverworld.inText)
            if (col == GameObject.Find("Player").GetComponent<BoxCollider2D>())
                EventManager.instance.executeEvent(gameObject);
    }
}
