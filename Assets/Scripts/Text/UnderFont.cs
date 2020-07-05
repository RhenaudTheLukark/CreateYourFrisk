using System.Collections.Generic;
using UnityEngine;

public class UnderFont {
    public UnderFont(Dictionary<char, Sprite> letters, string name) {
        Name = name;
        Letters = letters;
        Sound = null;
        try { LineSpacing = Letters[' '].rect.height * 1.5f; }
        catch { throw new CYFException("The font \"" + name + "\" doesn't have a space character, however the font needs one."); }
        CharSpacing = 3;
        DefaultColor = Color.white;
    }

    public string Name { get; private set; }
    public Dictionary<char, Sprite> Letters { get; private set; }
    public AudioClip Sound { get; set; }
    public Color DefaultColor { get; set; }
    public float LineSpacing { get; set; }
    public float CharSpacing { get; set; }
}