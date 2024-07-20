using UnityEngine;

internal class ShakeEffect : TextEffect {
    private readonly float intensity;
    private bool skipNextFrame;

    public ShakeEffect(TextManager textMan, float intensity = 1.0f) : base(textMan) {
        this.intensity = intensity > 0 ? intensity : 1.0f;
    }

    protected override void UpdateInternal() {
        if (skipNextFrame) {
            skipNextFrame = false;
            return;
        }
        skipNextFrame = true;

        for (int i = 0; i < textMan.letters.Count; i++) {
            LuaSpriteController ctrl = LuaSpriteController.GetOrCreate(textMan.letters[i].image.gameObject);
            float random = Random.value * 2.0f * Mathf.PI;
            Vector2 oldPosition = positions[i];
            positions[i] = new Vector2(Mathf.Sin(random) * intensity, Mathf.Cos(random) * intensity);
            ctrl.Move(positions[i].x - oldPosition.x, positions[i].y - oldPosition.y);
        }
    }
}