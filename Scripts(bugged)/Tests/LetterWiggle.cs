using UnityEngine;

public class LetterWiggle : MonoBehaviour {
    private float wiggleX;
    private float wiggleY;
    private float wiggleTime;
    private float timer;
    private RectTransform self;

    private void Start() {
        wiggleTime = Random.Range(4.0f, 15.0f);
        self = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    private void Update() {
        if (timer > wiggleTime) {
            timer = 0.0f;
            wiggleTime = Random.Range(4.0f, 15.0f);
            self.position = new Vector2(self.position.x - wiggleX, self.position.y - wiggleY);
        }

        timer += Time.deltaTime;

        if (timer > wiggleTime) {
            wiggleX = Random.Range(-2.0f, 2.0f);
            wiggleY = Random.Range(-2.0f, 2.0f);
            self.position = new Vector2(self.position.x + wiggleX, self.position.y + wiggleY);
        }
    }
}