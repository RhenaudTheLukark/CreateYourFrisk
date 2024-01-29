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
        float iDiv = sinTimer * rotSpeed + effectStep;
        sinTimer += Time.deltaTime;

        float oldXPos = xPos;
        float oldYPos = yPos;
        xPos = intensity * -Mathf.Sin(iDiv);
        yPos = intensity * Mathf.Cos(iDiv);
        rt.localPosition += new Vector3(xPos - oldXPos, yPos - oldYPos, 0);
    }
}