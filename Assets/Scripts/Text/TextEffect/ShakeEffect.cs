using UnityEngine;

internal class ShakeEffect : TextEffect {
    private readonly float intensity;
    private bool skipNextFrame;

    public ShakeEffect(TextManager textMan, float intensity = 1.0f) : base(textMan) { this.intensity = intensity; }

    protected override void UpdateInternal() {
        if (skipNextFrame) {
            skipNextFrame = false;
            return;
        }
        for (int i = 0; i < textMan.letters.Count; i++) {
            TextManager.LetterData data = textMan.letters[i];
            RectTransform rt = data.image.GetComponent<RectTransform>();
            float random = Random.value * 2.0f * Mathf.PI;
            float xWig = Mathf.Sin(random) * intensity;
            float yWig = Mathf.Cos(random) * intensity;
            rt.anchoredPosition = new Vector2(data.position.x + xWig, data.position.y + yWig);
        }
        skipNextFrame = true;
    }
}