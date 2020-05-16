using UnityEngine;

public class ShakeEffectLetter : TextEffectLetter {
    private float intensity;
    private bool skipNextFrame = false;

    public ShakeEffectLetter(Letter letter, float intensity = 1.0f) : base(letter) { this.intensity = intensity != 0 ? intensity : 1.0f; }

    protected override void UpdateInternal() {
        if (skipNextFrame) {
            skipNextFrame = false;
            return;
        }

        float random = Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        RectTransform rt = letter.GetComponent<RectTransform>();
        rt.position = new Vector2(letter.basisPos.x + xWig, letter.basisPos.y + yWig);
        skipNextFrame = true;
    }
}