public abstract class TextEffect {
    //private bool doUpdate = true;
    protected TextManager textMan;
    protected TextEffect(TextManager textMan) { this.textMan = textMan; }
    public void UpdateEffects() { UpdateInternal(); }
    protected abstract void UpdateInternal();
}