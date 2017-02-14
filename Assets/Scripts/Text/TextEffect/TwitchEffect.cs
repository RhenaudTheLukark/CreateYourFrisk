using UnityEngine;
using System.Collections.Generic;

public class TwitchEffect : TextEffect {
    private int letterCount = 0;
    private int letRefCount = 0;
    private int prevChar = 0;
    private int updateCount = 0;
    private float intensity;
    private int minWigFrames = 34;
    private int wigFrameVariety = 30;
    private int nextWigInFrames = 24;
    private List<int> availableIndexes = new List<int>();

    public TwitchEffect(TextManager textMan, float intensity = 2.0f) : base(textMan) { this.intensity = intensity; }

    protected override void updateInternal() {
        List<Letter> letters = textMan.letters;
        if (letters.Count != letterCount && textMan.isFinished() || letRefCount != textMan.letterReferences.Length) {
            letRefCount = textMan.letterReferences.Length;
            letterCount = letters.Count;
            availableIndexes.Clear();
            for (int i = 0; i < letters.Count; i++)
                if (i < letters.Count)
                    if (letters[i].effect == null)
                        availableIndexes.Add(i);
        }
        if (availableIndexes.Count == 0)
            return;

        // move back last character
        if (prevChar >= 0 && textMan.letterReferences.Length > prevChar && textMan.letterReferences[prevChar] != null)
            textMan.letterReferences[prevChar].GetComponent<RectTransform>().anchoredPosition = textMan.letterPositions[prevChar];
        prevChar = -1;

        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        int selectedChar = UnityEngine.Random.Range(0, availableIndexes.Count);
        if (textMan.letterReferences[availableIndexes[selectedChar]] == null)
            return;
        float random = UnityEngine.Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = minWigFrames + (int)(wigFrameVariety * UnityEngine.Random.value);
        RectTransform rt = textMan.letterReferences[availableIndexes[selectedChar]].GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(textMan.letterPositions[availableIndexes[selectedChar]].x + xWig, textMan.letterPositions[availableIndexes[selectedChar]].y + yWig);
        prevChar = availableIndexes[selectedChar];
    }
}