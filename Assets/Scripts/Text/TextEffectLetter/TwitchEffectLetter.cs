using UnityEngine;

public class TwitchEffectLetter : TextEffectLetter {
    private int updateCount;
    private readonly float intensity;
    private readonly int avgWigFrames = 500;
    private readonly int wigFrameVariety = 400;
    private int nextWigInFrames;

    public TwitchEffectLetter(Letter letter, float intensity = 2.0f, int step = 0) : base(letter) {
        this.intensity = intensity > 0 ? intensity : 2.0f;

        if (step > 0) {
            avgWigFrames = step;
            wigFrameVariety = step * 4 / 5;
        }
        nextWigInFrames = GetNextWigTime();
    }

    protected override void UpdateInternal() {
        if (updateCount == 0) {
            rt.localPosition -= new Vector3(xPos, yPos);
            xPos = yPos = 0;
        }

        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;
        nextWigInFrames = GetNextWigTime();

        float random = Random.value * 2.0f * Mathf.PI;
        xPos = Mathf.Sin(random) * intensity;
        yPos = Mathf.Cos(random) * intensity;
        rt.localPosition += new Vector3(xPos, yPos);
    }

    private int GetNextWigTime() { return avgWigFrames + Mathf.RoundToInt(wigFrameVariety * (Random.value * 2 - 1)); }
}