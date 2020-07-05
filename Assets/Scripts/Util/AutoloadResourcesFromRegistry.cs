using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Behaviour that retrieves resources from the built-in registry rather than setting them on the components in the Unity Editor.
/// </summary>
class AutoloadResourcesFromRegistry : MonoBehaviour {
    [Header("Image Resource")]
    public bool SetNativeSize;
    public string SpritePath;
    [Header("Audio Resource")]
    public bool Loop;
    //public string SoundPath;

    public bool done;
    private bool doneFromLoadedScene;
    private bool handleDictErrors;

    private void OnEnable() {
        if (UnitaleUtil.IsOverworld) Fading.StartFade += LateStart;
        else                         StaticInits.Loaded += LateStart;
        if (FindObjectsOfType<AutoloadResourcesFromRegistry>().Any(a => a.done || a.doneFromLoadedScene))
            LateStart();
    }

    private void OnDisable() {
        if (UnitaleUtil.IsOverworld) StaticInits.Loaded -= LateStart;
        else                         Fading.StartFade -= LateStart;
    }

    private void LateStart() {
        if (this == null) return;
        //bool hasError = false;
        if ((done || !handleDictErrors) && (doneFromLoadedScene || handleDictErrors)) return;
        if (!done && handleDictErrors)
            done = true;
        else
            doneFromLoadedScene = true;
        bool currHandleDictErrors = handleDictErrors;
        if (!string.IsNullOrEmpty(SpritePath)) {
            Image                  img  = GetComponent<Image>();
            SpriteRenderer         img2 = GetComponent<SpriteRenderer>();
            ParticleSystemRenderer img3 = GetComponent<ParticleSystemRenderer>();
            if (img != null) {
                img.sprite = SpriteRegistry.Get(SpritePath);
                if (img.sprite == null && currHandleDictErrors) {
                    UnitaleUtil.DisplayLuaError("AutoloadResourcesFromRegistry", "You tried to load the sprite \"" + SpritePath + "\", but it doesn't exist.");
                    return;
                }
                //if (img.sprite == null)
                    //hasError = true;
                if (img.sprite != null) {
                    //img.sprite.name = SpritePath.ToLower(); TODO: Find a way to store the sprite's path
                    if (SetNativeSize) {
                        img.SetNativeSize();
                        if (!UnitaleUtil.IsOverworld) {
                            img.rectTransform.localScale = new Vector3(1, 1, 1);
                            img.rectTransform.sizeDelta  = new Vector2(img.sprite.texture.width, img.sprite.texture.height);
                        } else {
                            img.rectTransform.localScale = new Vector3(100, 100, 1);
                            img.rectTransform.sizeDelta  = new Vector2(img.sprite.texture.width / 100f, img.sprite.texture.height / 100f);
                        }
                    }
                }
            } else if (img2 != null) {
                img2.sprite = SpriteRegistry.Get(SpritePath);
                if (img2.sprite == null && currHandleDictErrors) {
                    UnitaleUtil.DisplayLuaError("AutoloadResourcesFromRegistry", "You tried to load the sprite \"" + SpritePath + "\", but it doesn't exist.");
                    return;
                }
                //if (img2.sprite == null)
                    //hasError = true;
                if (img2.sprite != null) {
                    //img2.sprite.name = SpritePath.ToLower();
                    if (SetNativeSize) {
                        if (!UnitaleUtil.IsOverworld) {
                            img2.transform.localScale                    = new Vector3(1, 1, 1);
                            img2.GetComponent<RectTransform>().sizeDelta = new Vector2(img2.sprite.texture.width, img2.sprite.texture.height);
                        } else {
                            img2.transform.localScale                    = new Vector3(100, 100, 1);
                            img2.GetComponent<RectTransform>().sizeDelta = new Vector2(img2.sprite.texture.width / 100f, img2.sprite.texture.height / 100f);
                        }
                    }
                }
            } else if (img3 != null)
                img3.material.mainTexture = SpriteRegistry.Get(SpritePath).texture;
            else
                throw new CYFException("The GameObject " + gameObject.name + " doesn't have an Image, Sprite Renderer or Particle System component.");
        }

        /*
        if (!string.IsNullOrEmpty(SoundPath) && !hasError) {
            AudioSource aSrc = GetComponent<AudioSource>();
            aSrc.clip = AudioClipRegistry.Get(SoundPath);
            aSrc.loop = Loop;
        }
        */
        handleDictErrors = true;
    }
}
