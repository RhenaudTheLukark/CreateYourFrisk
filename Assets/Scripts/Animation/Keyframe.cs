using UnityEngine;

/// <summary>
/// This class is used for sprite animations.
/// A Keyframe instance must include a sprite and a name, which is the path associated to it
/// </summary>
public class Keyframe {
    /// <summary>
    /// Sprite used for the Keyframe.
    /// </summary>
    public Sprite sprite;
    /// <summary>
    /// Path to the sprite. Can be "empty" if unknown.
    /// </summary>
    public string name;

    /// <summary>
    /// Constructor of the class.
    /// </summary>
    /// <param name="sprite">Sprite used for the Keyframe.</param>
    /// <param name="name">Path to the sprite. Can be "empty" if unknown.</param>
    public Keyframe(Sprite sprite, string name = "empty") {
        this.sprite = sprite;
        this.name = name;
    }
}
