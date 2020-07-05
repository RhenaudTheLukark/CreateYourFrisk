using UnityEngine;
using UnityEngine.UI;

public class TwitchEffect : TextEffect {
    private int prevChar;
    private int updateCount;
    private readonly float intensity;
    private const int minWigFrames = 34;
    private const int wigFrameVariety = 30;
    private int nextWigInFrames = 24;

    public TwitchEffect(TextManager textMan, float intensity = 2.0f) : base(textMan) { this.intensity = intensity; }

    protected override void UpdateInternal() {
        Image[] letters = textMan.letterReferences;
        if (letters.Length == 0)
            return;

        // move back last character
        if (prevChar >= 0 && textMan.letterReferences.Length > prevChar && textMan.letterReferences[prevChar] != null)
            textMan.letterReferences[prevChar].GetComponent<RectTransform>().anchoredPosition = textMan.letterPositions[prevChar];
        prevChar = -1;

        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        int selectedChar = Random.Range(0, letters.Length);
        if (letters[selectedChar] == null)
            return;
        float random = Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = minWigFrames + (int)(wigFrameVariety * Random.value);
        RectTransform rt = letters[selectedChar].GetComponent<RectTransform>();
        rt.position = new Vector2(letters[selectedChar].transform.position.x + xWig, letters[selectedChar].transform.position.y + yWig);
        prevChar = selectedChar;
    }
}