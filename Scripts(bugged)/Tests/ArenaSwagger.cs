using UnityEngine;

// continually modifies arena size
public class ArenaSwagger : MonoBehaviour {
    public RectTransform outer;
    public RectTransform inner;

    private void Update() {
        inner.sizeDelta = new Vector2(200 - 100 * Mathf.Sin(Time.time), 125 - 75 * Mathf.Cos(0.4f + Time.time));
        outer.sizeDelta = new Vector2(inner.sizeDelta.x + 10, inner.sizeDelta.y + 10);
    }
}