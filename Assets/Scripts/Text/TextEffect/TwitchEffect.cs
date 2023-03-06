using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwitchEffect : TextEffect {
    private int prevChar;
    private int updateCount;
    private readonly float intensity;
    private readonly int avgWigFrames = 48;
    private readonly int wigFrameVariety = 16;
    private int nextWigInFrames;

    public TwitchEffect(TextManager textMan, float intensity = 2.0f, int step = 0) : base(textMan) {
        this.intensity = intensity;
        if (step > 0) {
            avgWigFrames = step;
            wigFrameVariety = step / 3;
        }
        nextWigInFrames = GetNextWigTime();
    }

    protected override void UpdateInternal() {
        if (textMan.letters.Count == 0)
            return;

        // move back last character
        if (prevChar >= 0 && textMan.letters.Count > prevChar)
            textMan.letters[prevChar].image.GetComponent<RectTransform>().anchoredPosition = textMan.letters[prevChar].position;
        prevChar = -1;

        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        int selectedChar = Random.Range(0, textMan.letters.Count);
        float random = Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = GetNextWigTime();
        TextManager.LetterData data = textMan.letters[selectedChar];
        RectTransform rt = data.image.GetComponent<RectTransform>();
        rt.position = new Vector2(rt.position.x + xWig, rt.position.y + yWig);
        prevChar = selectedChar;
    }

    private int GetNextWigTime() {
        return avgWigFrames + (int)(wigFrameVariety * (Random.value * 2 - 1));
    }
}