using System;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Letter : MonoBehaviour {
    public Vector2 basisPos;
    public Image img;
    //public Color currentColor = Color.white;
    public TextEffectLetter effect = null;
    public bool started = false;

    private void Start() {
        img = GetComponent<Image>();
        if (SceneManager.GetActiveScene().name == "Battle")
            if (ArenaManager.instance.firstTurn)
                LateUpdater.lateActions.Add(LateStart);
            else
                LateStart();
        else
            LateStart();
    }

    private void LateStart() {
        started = true;
        basisPos = transform.position;
    }
    
    private void Update() {
        if (effect != null && started)
            effect.updateEffects();
    }
}