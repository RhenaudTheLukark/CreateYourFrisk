public abstract class TextEffectLetter {
    protected Letter letter;
    protected TextEffectLetter(Letter letter) { this.letter = letter; }
    public void UpdateEffects() { UpdateInternal(); }
    protected abstract void UpdateInternal();
}