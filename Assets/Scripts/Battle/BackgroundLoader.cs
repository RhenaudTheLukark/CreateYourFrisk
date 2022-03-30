using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extremely lazy background loader which is only slightly better than not having a background.
/// Currently attempts to load the 'bg' file from the Sprites folder, otherwise does nothing.
/// Attached to the Background object in the Battle scene.
/// </summary>
public class BackgroundLoader : MonoBehaviour {
    // Use this for initialization
    private void Start() {
        LuaSpriteController sprite = LuaSpriteController.GetOrCreate(gameObject);
        try {
            sprite.Set("bg");
            sprite.color = new float[4] { 1, 1, 1, 1 };
        } catch (CYFException) {
            // Background failed loading, no need to do anything.
            UnitaleUtil.Warn("No background file found. Using empty background.");
        }
        sprite.Scale(640 / sprite.width, 480 / sprite.height);
    }
}
