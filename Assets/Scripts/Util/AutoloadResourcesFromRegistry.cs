#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public string SoundPath;
    [Header("Animatior Resource")]
    public string AnimatorPath;

    public bool done = false;
    private bool doneFromLoadedScene = false;
    private bool handleDictErrors = false;

    /*void Awake() {
        //if (StaticInits.Initialized)
        LateStart();
        //LateUpdater.lateInit.Add(LateStart);
    }*/

    void OnEnable() {
        if (UnitaleUtil.isOverworld())
            Fading.StartFade += LateStart;
        else
            StaticInits.Loaded += LateStart;
        foreach (AutoloadResourcesFromRegistry a in FindObjectsOfType<AutoloadResourcesFromRegistry>())
            if (a.done || a.doneFromLoadedScene) {
                LateStart();
                return;
            }
    }

    void OnDisable() {
        if (UnitaleUtil.isOverworld())
            StaticInits.Loaded -= LateStart;
        else
            Fading.StartFade -= LateStart;
    }

    /*void LateStart() {
        if (!done) {
            done = true;
            if (!string.IsNullOrEmpty(SpritePath)) {
                Image img = GetComponent<Image>();
                if (img != null) {
                    //img.sprite = SpriteRegistry.Get(SpritePath);
                    //if (SetNativeSize)
                    //    img.SetNativeSize();
                    //img.sprite = SpriteRegistry.Get(SpritePath);
                    if (img.sprite == null && handleDictErrors) {
                        UnitaleUtil.displayLuaError("AutoloadResourcesFromRegistry", "You tried to load the sprite \"" + SpritePath + "\", but it doesn't exist.");
                        return;
                    }
                    if (img.sprite != false) {
                        //img.sprite.name = SpritePath.ToLower(); TODO: Find a way to store the sprite's path
                        if (SetNativeSize) {
                            img.SetNativeSize();
                            if (!GameObject.FindObjectOfType<TextManager>().overworld) {
                                img.rectTransform.sizeDelta = new Vector2(img.sprite.texture.width, img.sprite.texture.height);
                                img.rectTransform.localScale = new Vector3(1, 1, 1);
                            } else {
                                img.rectTransform.sizeDelta = new Vector2(img.sprite.texture.width / 100f, img.sprite.texture.height / 100f);
                                img.rectTransform.localScale = new Vector3(100, 100, 1);
                            }
                        }
                    }
                } else {
                    SpriteRenderer img2 = GetComponent<SpriteRenderer>();
                    if (img2 != null)
                        img2.sprite = SpriteRegistry.Get(SpritePath);
                    else
                        throw new CYFException("The GameObject " + gameObject.name + " doesn't have an Image or SpriteRenderer component.");
                    if (SetNativeSize)
                        if (!GameObject.FindObjectOfType<TextManager>().overworld) {
                            img2.GetComponent<RectTransform>().sizeDelta = new Vector2(img2.sprite.texture.width, img2.sprite.texture.height);
                            img2.transform.localScale = new Vector3(1, 1, 1);
                        } else {
                            img2.GetComponent<RectTransform>().sizeDelta = new Vector2(img2.sprite.texture.width / 100f, img2.sprite.texture.height / 100f);
                            img2.transform.localScale = new Vector3(100, 100, 1);
                        }
                }

                ParticleSystem psys = GetComponent<ParticleSystem>();
                if (psys != null) {
                    ParticleSystemRenderer prender = GetComponent<ParticleSystemRenderer>();
                    prender.material.mainTexture = SpriteRegistry.Get(SpritePath).texture;
                }
            }

            if (!string.IsNullOrEmpty(SoundPath)) {
                AudioSource aSrc = GetComponent<AudioSource>();
                aSrc.clip = AudioClipRegistry.Get(SoundPath);
                aSrc.loop = Loop;
            }
        }
    }*/

    void LateStart() {
        if (this == null)
            return;
        bool hasError = false;
        if ((!done && this.handleDictErrors) || (!doneFromLoadedScene && !this.handleDictErrors)) {
            if (!done && this.handleDictErrors)
                done = true;
            else
                doneFromLoadedScene = true;
            bool handleDictErrors = this.handleDictErrors;
            if (!string.IsNullOrEmpty(SpritePath)) {
                Image img = GetComponent<Image>();
                SpriteRenderer img2 = GetComponent<SpriteRenderer>();
                ParticleSystemRenderer img3 = GetComponent<ParticleSystemRenderer>();
                if (img != null) {
                    img.sprite = SpriteRegistry.Get(SpritePath);
                    if (img.sprite == null && handleDictErrors) {
                        UnitaleUtil.displayLuaError("AutoloadResourcesFromRegistry", "You tried to load the sprite \"" + SpritePath + "\", but it doesn't exist.");
                        return;
                    } else if (img.sprite == null)
                        hasError = true;
                    else {
                        //img.sprite.name = SpritePath.ToLower(); TODO: Find a way to store the sprite's path
                        if (SetNativeSize) {
                            img.SetNativeSize();
                            if (!UnitaleUtil.isOverworld()) {
                                img.rectTransform.sizeDelta = new Vector2(img.sprite.texture.width, img.sprite.texture.height);
                                img.rectTransform.localScale = new Vector3(1, 1, 1);
                            } else {
                                img.rectTransform.sizeDelta = new Vector2(img.sprite.texture.width / 100f, img.sprite.texture.height / 100f);
                                img.rectTransform.localScale = new Vector3(100, 100, 1);
                            }
                        }
                    }
                } else if (img2 != null) {
                    img2.sprite = SpriteRegistry.Get(SpritePath);
                    if (img2.sprite == null && handleDictErrors) {
                        UnitaleUtil.displayLuaError("AutoloadResourcesFromRegistry", "You tried to load the sprite \"" + SpritePath + "\", but it doesn't exist.");
                        return;
                    } else if (img2.sprite == null)
                        hasError = true;
                    else {
                        //img2.sprite.name = SpritePath.ToLower();
                        if (SetNativeSize) {
                            if (!UnitaleUtil.isOverworld()) {
                                img2.GetComponent<RectTransform>().sizeDelta = new Vector2(img2.sprite.texture.width, img2.sprite.texture.height);
                                img2.transform.localScale = new Vector3(1, 1, 1);
                            } else {
                                img2.GetComponent<RectTransform>().sizeDelta = new Vector2(img2.sprite.texture.width / 100f, img2.sprite.texture.height / 100f);
                                img2.transform.localScale = new Vector3(100, 100, 1);
                            }
                        }
                    }
                } else if (img3 != null)
                    img3.material.mainTexture = SpriteRegistry.Get(SpritePath).texture;
                else
                    throw new CYFException("The GameObject " + gameObject.name + " doesn't have an Image, Sprite Renderer or Particle System component.");
            }

            if (!string.IsNullOrEmpty(SoundPath) && !hasError) {
                AudioSource aSrc = GetComponent<AudioSource>();
                aSrc.clip = AudioClipRegistry.Get(SoundPath);
                /*if (aSrc.clip == null && handleDictErrors) {
                    UnitaleUtil.displayLuaError("AutoloadResourcesFromRegistry", "You tried to load the music \"" + SoundPath + "\", but it doesn't exist.");
                    return;
                }*/
                aSrc.loop = Loop;
            }
            this.handleDictErrors = true;

            /* TODO: Make so AnimatorControllers can be loaded from a file
            if (gameObject.GetComponent<Animator>() &&!string.IsNullOrEmpty(AnimatorPath))
                LoadAnimatorController();*/
        }
    }
}
