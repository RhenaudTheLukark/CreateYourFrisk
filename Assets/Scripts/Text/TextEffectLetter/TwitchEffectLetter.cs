using UnityEngine;

public class TwitchEffectLetter : TextEffectLetter {
    private int updateCount;
    private readonly float intensity;
    private readonly int avgWigFrames = 500;
    private readonly int wigFrameVariety = 400;
    private int nextWigInFrames;

    public TwitchEffectLetter(Letter letter, float intensity = 2.0f, int step = 0) : base(letter) {
        this.intensity = intensity != 0 ? intensity : 2.0f;
        if (step > 0) {
            avgWigFrames = step;
            wigFrameVariety = step * 4 / 5;
        }

        nextWigInFrames = (int)(wigFrameVariety * Random.value);
    }

    protected override void UpdateInternal() {
        if (updateCount == 0)
            letter.GetComponent<RectTransform>().position = letter.basisPos;
        // Don't make it happen too often
        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        float random = Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = GetNextWigTime();
        RectTransform rt = letter.GetComponent<RectTransform>();
        rt.position = new Vector2(rt.position.x + xWig, rt.position.y + yWig);
    }
    private int GetNextWigTime() {
        return avgWigFrames + (int)(wigFrameVariety * (Random.value * 2 - 1));
    }
}