using UnityEngine;

public class TwitchEffectLetter : TextEffectLetter {
    private int updateCount = 0;
    private float intensity;
    private int minWigFrames = 200;
    private int wigFrameVariety = 300;
    private int nextWigInFrames = 100;

    public TwitchEffectLetter(Letter letter, float intensity = 2.0f) : base(letter) {
        this.intensity = intensity != 0 ? intensity : 2.0f;
        nextWigInFrames += (int)(wigFrameVariety * UnityEngine.Random.value);
    }

    protected override void updateInternal() {
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
        rt.position = new Vector2(letter.basisPos.x + xWig, letter.basisPos.y + yWig);
    }
}