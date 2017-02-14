using System.Collections.Generic;
using UnityEngine;

public class UnderFont {
    public UnderFont(Dictionary<char, Sprite> letters) {
        Letters = letters;
        Sound = null;
        LineSpacing = Letters[' '].rect.height * 1.5f;
        CharSpacing = 3;
        DefaultColor = Color.white;
    }

    public Dictionary<char, Sprite> Letters { get; private set; }
    public AudioClip Sound { get; set; }
    public Color DefaultColor { get; set; }
    public float LineSpacing { get; set; }
    public float CharSpacing { get; set; }
}