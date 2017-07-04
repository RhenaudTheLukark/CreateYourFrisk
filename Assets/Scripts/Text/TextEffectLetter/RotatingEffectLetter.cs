using UnityEngine;

public class RotatingEffectLetter : TextEffectLetter {
    private float sinTimer;
    private float intensity;
    private float rotSpeed = 7.0f;

    public RotatingEffectLetter(Letter letter, float intensity = 1.5f) : base(letter) { this.intensity = intensity != 0 ? intensity : 1.5f; }

    protected override void UpdateInternal() {
        RectTransform rt = letter.GetComponent<RectTransform>();
        float iDiv = sinTimer * rotSpeed;
        rt.position = new Vector2(rt.position.x + intensity * -Mathf.Sin(iDiv), rt.position.y + intensity * Mathf.Cos(iDiv));
        sinTimer += Time.deltaTime;
    }
}