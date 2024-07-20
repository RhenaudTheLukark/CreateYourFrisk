public abstract class TextEffectLetter {
    protected Letter letter;
    protected LuaSpriteController ctrl;
    protected float xPos = 0, yPos = 0;

    protected TextEffectLetter(Letter letter) {
        this.letter = letter;
        ctrl = LuaSpriteController.GetOrCreate(letter.gameObject);
    }

    public void UpdateEffects() { UpdateInternal(); }
    protected abstract void UpdateInternal();

    public void ResetPositions() {
        if (letter && ctrl != null)
            ctrl.Move(-xPos, -yPos);
    }
}