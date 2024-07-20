using UnityEngine;

public class Letter : MonoBehaviour {
    public Vector2 basisPos;
    public TextEffectLetter effect = null;
    public int characterNumber;
    public bool started;

    private void Start() {
        basisPos = transform.position;
        started = true;
    }

    private void Update() {
        if (effect == null || !started) return;
        effect.UpdateEffects();
    }
}