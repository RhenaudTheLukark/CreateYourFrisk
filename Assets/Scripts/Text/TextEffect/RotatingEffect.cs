using UnityEngine;

public class RotatingEffect : TextEffect {
    private float sinTimer;
    private readonly float intensity;
    private const float rotSpeed = 7.0f;
    private readonly float effectStep;

    public RotatingEffect(TextManager textMan, float intensity = 1.5f, float step = 0f) : base(textMan) {
        this.intensity = intensity;
        effectStep = step;
    }

    protected override void UpdateInternal() {
        for (int i = 0; i < textMan.letterReferences.Count; i++) {
            if (textMan.letterReferences[i] == null) continue;
            RectTransform rt = textMan.letterReferences[i].GetComponent<RectTransform>();
            float iDiv = sinTimer * rotSpeed + i / 3.0f + effectStep * i;
            rt.anchoredPosition = new Vector2(textMan.letterPositions[i].x + intensity * -Mathf.Sin(iDiv), textMan.letterPositions[i].y + intensity * Mathf.Cos(iDiv));
        }

        sinTimer += Time.deltaTime;
    }
}