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
        List<Image> letters = textMan.letterReferences;
        if (letters.Count == 0)
            return;

        // move back last character
        if (prevChar >= 0 && textMan.letterReferences.Count > prevChar && textMan.letterReferences[prevChar] != null)
            textMan.letterReferences[prevChar].GetComponent<RectTransform>().anchoredPosition = textMan.letterPositions[prevChar];
        prevChar = -1;

        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        int selectedChar = Random.Range(0, letters.Count);
        if (letters[selectedChar] == null)
            return;
        float random = Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = GetNextWigTime();
        RectTransform rt = letters[selectedChar].GetComponent<RectTransform>();
        rt.position = new Vector2(letters[selectedChar].transform.position.x + xWig, letters[selectedChar].transform.position.y + yWig);
        prevChar = selectedChar;
    }

    private int GetNextWigTime() {
        return avgWigFrames + (int)(wigFrameVariety * (Random.value * 2 - 1));
    }
}