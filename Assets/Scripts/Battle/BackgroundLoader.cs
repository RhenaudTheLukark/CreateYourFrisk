using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extremely lazy background loader which is only slightly better than not having a background.
/// Currently attempts to load the 'bg' file from the Sprites folder, otherwise does nothing.
/// Attached to the Background object in the Battle scene.
/// </summary>
public class BackgroundLoader : MonoBehaviour {
    Image bgImage;
    // Use this for initialization
    private void Start() {
        bgImage = GetComponent<Image>();
        try {
            Sprite bg = SpriteUtil.fromFile(FileLoader.pathToModFile("Sprites/bg.png"));
            if (bg != null) {
                bg.texture.filterMode = FilterMode.Point;
                bgImage.sprite = bg;
                bgImage.color = Color.white;
            }
        } catch {
            // background failed loading, no need to do anything
            UnitaleUtil.writeInLogAndDebugger("[WARN]No background file found. Using empty background.");
        }
    }
}