using UnityEngine;

public abstract class TextEffectLetter {
    protected Letter letter;
    public TextEffectLetter(Letter letter) { this.letter = letter; }
    public void UpdateEffects() { UpdateInternal(); }
    protected abstract void UpdateInternal();
}