using UnityEngine;
using UnityEngine.UI;

public class Letter : MonoBehaviour {
    public Vector2 basisPos;
    public Image img;
    public TextEffectLetter effect = null;
    public int characterNumber;
    public bool started;
    public Color colorFromText;

    private void Start() {
        img = GetComponent<Image>();
        basisPos = transform.position;
        started = true;
    }

    private void Update() {
        if (effect == null || !started) return;
        effect.UpdateEffects();
    }
}