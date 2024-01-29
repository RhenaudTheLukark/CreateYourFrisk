using UnityEngine;

public class ShakeEffectLetter : TextEffectLetter {
    private readonly float intensity;
    private bool skipNextFrame;

    public ShakeEffectLetter(Letter letter, float intensity = 1.0f) : base(letter) {
        this.intensity = intensity > 0 ? intensity : 1.0f;
    }

    protected override void UpdateInternal() {
        if (skipNextFrame) {
            skipNextFrame = false;
            return;
        }
        skipNextFrame = true;

        float random = Random.value * 2.0f * Mathf.PI;
        float oldXPos = xPos;
        float oldYPos = yPos;
        xPos = Mathf.Sin(random) * intensity;
        yPos = Mathf.Cos(random) * intensity;
        rt.localPosition += new Vector3(xPos - oldXPos, yPos - oldYPos, 0);
    }
}