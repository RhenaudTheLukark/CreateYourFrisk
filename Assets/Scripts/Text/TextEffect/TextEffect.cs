using System.Collections.Generic;
using UnityEngine;

public abstract class TextEffect {
    protected TextManager textMan;
    protected List<Vector2> positions;

    protected TextEffect(TextManager textMan) {
        this.textMan = textMan;
        positions = new List<Vector2>();
        foreach (TextManager.LetterData l in textMan.letters)
            positions.Add(new Vector2());
    }

    public void UpdateEffects() {
        while (textMan.letters.Count > positions.Count) positions.Add(new Vector2());
        while (textMan.letters.Count < positions.Count) positions.RemoveAt(positions.Count - 1);
        UpdateInternal();
    }
    protected abstract void UpdateInternal();

    ~TextEffect() {
        for (int i = 0; i < positions.Count; i++)
            if (textMan != null && textMan.letters[i].image != null)
                textMan.letters[i].image.transform.position -= (Vector3)positions[i];
    }
}