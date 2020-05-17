using UnityEngine;

public class TwitchEffectLetter : TextEffectLetter {
    private int updateCount = 0;
    private float intensity;
    private int minWigFrames = 300;
    private int wigFrameVariety = 750;
    private int nextWigInFrames = 300;

    public TwitchEffectLetter(Letter letter, float intensity = 2.0f) : base(letter) {
        this.intensity = intensity != 0 ? intensity : 2.0f;
        nextWigInFrames = (int)(wigFrameVariety * UnityEngine.Random.value);
    }

    protected override void UpdateInternal() {
        if (updateCount == 0)
            letter.GetComponent<RectTransform>().position = letter.basisPos;
        // don't make it happen too often
        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        float random = UnityEngine.Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = minWigFrames + (int)(wigFrameVariety * UnityEngine.Random.value);
        RectTransform rt = letter.GetComponent<RectTransform>();
        rt.position = new Vector2(rt.position.x + xWig, rt.position.y + yWig);
    }
}