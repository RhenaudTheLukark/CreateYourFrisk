using UnityEngine;
using System.Collections.Generic;

internal class ShakeEffect : TextEffect {
    private float intensity;
    private bool skipNextFrame = false;

    public ShakeEffect(TextManager textMan, float intensity = 1.0f) : base(textMan) { this.intensity = intensity; }

    protected override void UpdateInternal() {
        if (skipNextFrame) {
            skipNextFrame = false;
            return;
        }
        for (int i = 0; i < textMan.letterReferences.Length; i++) {
            if (textMan.letterReferences[i] == null)
                continue;
            float random = UnityEngine.Random.value * 2.0f * Mathf.PI;
            float xWig = Mathf.Sin(random) * intensity;
            float yWig = Mathf.Cos(random) * intensity;
            RectTransform rt = textMan.letterReferences[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(textMan.letterPositions[i].x + xWig, textMan.letterPositions[i].y + yWig);
        }
        skipNextFrame = true;
    }
}