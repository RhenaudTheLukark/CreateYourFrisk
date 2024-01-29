using UnityEngine;

public class RotatingEffect : TextEffect {
    private float sinTimer;
    private readonly float intensity;
    private const float rotSpeed = 7.0f;
    private readonly float effectStep;

    public RotatingEffect(TextManager textMan, float intensity = 1.5f, float step = 0f) : base(textMan) {
        this.intensity = intensity != 0 ? intensity : 1.5f;
        effectStep = step;
    }

    protected override void UpdateInternal() {
        for (int i = 0; i < textMan.letters.Count; i++) {
            float iDiv = sinTimer * rotSpeed + i / 3.0f + effectStep * i;

            RectTransform rt = textMan.letters[i].image.GetComponent<RectTransform>();
            Vector2 oldPosition = positions[i];
            positions[i] = new Vector2(intensity * -Mathf.Sin(iDiv), intensity * Mathf.Cos(iDiv));
            rt.localPosition += (Vector3)(positions[i] - oldPosition);
        }

        sinTimer += Time.deltaTime;
    }
}