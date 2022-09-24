using UnityEngine;

public class RotatingEffectLetter : TextEffectLetter {
    private float sinTimer;
    private readonly float intensity;
    private const float rotSpeed = 7.0f;
    private readonly float effectStep;

    public RotatingEffectLetter(Letter letter, float intensity = 1.5f, float step = 0f) : base(letter) {
        this.intensity = intensity != 0 ? intensity : 1.5f;
        effectStep = step;
    }

    protected override void UpdateInternal() {
        RectTransform rt = letter.GetComponent<RectTransform>();
        float iDiv = sinTimer * rotSpeed + effectStep;
        rt.position = new Vector2(rt.position.x + intensity * -Mathf.Sin(iDiv), rt.position.y + intensity * Mathf.Cos(iDiv));
        sinTimer += Time.deltaTime;
    }
}