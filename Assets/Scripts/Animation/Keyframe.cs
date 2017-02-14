using UnityEngine;
using System.Collections;

public class Keyframe {
    public Sprite sprite;
    public string name;

    public Keyframe(Sprite sprite, string name = "empty") {
        this.sprite = sprite;
        this.name = name;
    }
}
