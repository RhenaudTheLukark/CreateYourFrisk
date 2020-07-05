using UnityEngine;
using UnityEngine.UI;

public class Letter : MonoBehaviour {
    public Vector2 basisPos;
    public Image img;
    public TextEffectLetter effect = null;
    public bool started;
    public Color colorFromText;
    private bool goodInit;


    private void Start() {
        img = GetComponent<Image>();
        if (GlobalControls.isInFight)
            if (ArenaManager.instance.firstTurn) LateUpdater.lateActions.Add(LateStart);
            else                                 LateStart();
        else                                     LateStart();
    }

    private void LateStart() {
        started = true;
        try {
            basisPos = transform.position;
            goodInit = true;
        } catch { /* ignored */ }
    }

    private void Update() {
        if (effect == null || !started) return;
        if (!goodInit) {
            basisPos = transform.position;
            goodInit = true;
        }
        effect.UpdateEffects();
    }
}