using UnityEngine;

public abstract class TextEffectLetter {
    protected Letter letter;
    protected RectTransform rt;
    protected float xPos = 0, yPos = 0;

    protected TextEffectLetter(Letter letter) {
        this.letter = letter;
        rt = letter.GetComponent<RectTransform>();
    }

    public void UpdateEffects() { UpdateInternal(); }
    protected abstract void UpdateInternal();

    public void ResetPositions() {
        if (letter)
            rt.localPosition -= new Vector3(xPos, yPos, 0);
    }
}