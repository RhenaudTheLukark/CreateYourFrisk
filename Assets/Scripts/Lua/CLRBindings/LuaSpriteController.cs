using UnityEngine;
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.UI;

public class LuaSpriteController {
    private GameObject _img;  // The real image
    internal GameObject img { // A image that returns the real image. We use this to be able to detect if the real image is null, and if it is, throw an exception
        get {
            if (_img == null)
                throw new ScriptRuntimeException("Attempted to perform action on removed sprite.");
            if (!_img.activeInHierarchy && !firstFrame)
                throw new ScriptRuntimeException("Attempted to perform action on removed sprite.");
            return _img;
        }
        set { _img = value; }
    }
    private bool firstFrame = true;
    private Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();
    public Vector2 nativeSizeDelta;                   // The native size of the image
    private Vector3 internalRotation = Vector3.zero;  // The rotation of the sprite
    private float xScale = 1.0f;                      // The X scale of the sprite
    private float yScale = 1.0f;                      // The Y scale of the sprite
    private Sprite originalSprite;                    // The original sprite
    public KeyframeCollection keyframes;              // This variable is used to store an animation
    public string tag;                                // The tag of the sprite : "projectile", "enemy", "bubble" or "other"
    public string spritename = "empty";
    private KeyframeCollection.LoopMode loop = KeyframeCollection.LoopMode.LOOP;
    public static MoonSharp.Interpreter.Interop.IUserDataDescriptor data = UserData.GetDescriptorForType<LuaSpriteController>(true);

    // The x position of the sprite, relative to the arena position and its anchor.
    public float x {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the x position of a removed sprite.");
            return img.GetComponent<RectTransform>().anchoredPosition.x;
        }
        set { img.GetComponent<RectTransform>().anchoredPosition = new Vector2(value, img.GetComponent<RectTransform>().anchoredPosition.y); }
    }

    // The y position of the sprite, relative to the arena position and its anchor.
    public float y {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the y position of a removed sprite.");
            return img.GetComponent<RectTransform>().anchoredPosition.y;
        }
        set { img.GetComponent<RectTransform>().anchoredPosition = new Vector2(img.GetComponent<RectTransform>().anchoredPosition.x, value); }
    }

    // The x position of the sprite, relative to the bottom left corner of the screen.
    public float absx {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the absx position of a removed sprite.");
            return img.GetComponent<RectTransform>().position.x; }
        set { img.GetComponent<RectTransform>().position = new Vector2(value, img.GetComponent<RectTransform>().position.y); }
    }

    // The y position of the sprite, relative to the bottom left corner of the screen.
    public float absy {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the absy position of a removed sprite.");
            return img.GetComponent<RectTransform>().position.y;
        }
        set { img.GetComponent<RectTransform>().position = new Vector2(img.GetComponent<RectTransform>().position.x, value); }
    }

    // The x scale of the sprite. This variable is used for the same purpose as img, to be able to do other things when setting the variable
    public float xscale {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the scale of a removed sprite.");
            return xScale;
        }
        set {
            xScale = value;
            Scale(xScale, yScale);
        }
    }

    // The y scale of the sprite. 
    public float yscale {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the scale of a removed sprite.");
            return yScale;
        }
        set {
            yScale = value;
            Scale(xScale, yScale);
        }
    }

    // Is the sprite active? True if the image of the sprite isn't null, false otherwise
    public bool isactive { 
        get {
            return GlobalControls.retroMode ? _img == null : _img != null;
        } 
    }

    // The original width of the sprite
    public float width {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the width of a removed sprite.");
            if (img.GetComponent<Image>())  return img.GetComponent<Image>().mainTexture.width;
            else                            return img.GetComponent<SpriteRenderer>().sprite.texture.width;
        }
    }

    // The original height of the sprite
    public float height {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the height of a removed sprite.");
            if (img.GetComponent<Image>())  return img.GetComponent<Image>().mainTexture.height;
            else                            return img.GetComponent<SpriteRenderer>().sprite.texture.height;
        }
    }

    // Is the current animation finished? True if there is a finished animation, false otherwise
    public bool animcomplete {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the animcomplete value of a removed sprite.");
            if (keyframes == null)
                if (img.GetComponent<KeyframeCollection>())
                    keyframes = img.GetComponent<KeyframeCollection>();
            if (keyframes != null)
                if (keyframes.enabled == false)                    return true;
                else if (loop == KeyframeCollection.LoopMode.LOOP) return false;
                else                                               return keyframes.enabled && keyframes.animationComplete();
            return false;
        }
    }

    // The loop mode of the animation
    public string loopmode {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the loopmode of a removed sprite.");
            return loop.ToString();
        }
        set {
            try {
                loop = (KeyframeCollection.LoopMode)Enum.Parse(typeof(KeyframeCollection.LoopMode), value.ToUpper(), true);
                if (keyframes != null)
                    keyframes.loop = (KeyframeCollection.LoopMode)Enum.Parse(typeof(KeyframeCollection.LoopMode), value.ToUpper(), true);
            } catch { throw new ScriptRuntimeException("sprite.loopmode can only have either \"ONESHOT\", \"ONESHOTEMPTY\" or \"LOOP\", but you entered \"" + value.ToUpper() + "\"."); }
        }
    }

    // The color of the sprite. It uses an array of three floats between 0 and 1
    public float[] color {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the color of a removed sprite.");
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return new float[] { imgtemp.color.r, imgtemp.color.g, imgtemp.color.b };
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return new float[] { imgtemp.color.r, imgtemp.color.g, imgtemp.color.b };
            }
        }
        set {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                // If we don't have three floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color(value[0], value[1], value[2], alpha);
                else if (value.Length == 4) imgtemp.color = new Color(value[0], value[1], value[2], value[3]);
                else                        throw new ScriptRuntimeException("You need 3 or 4 numeric values when setting a sprite's color.");
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                // If we don't have three floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color(value[0], value[1], value[2], alpha);
                else if (value.Length == 4) imgtemp.color = new Color(value[0], value[1], value[2], value[3]);
                else                        throw new ScriptRuntimeException("You need 3 or 4 numeric values when setting a sprite's color.");
            }
        }
    }

    // The color of the sprite on a 32 bits format. It uses an array of three floats between 0 and 255
    public float[] color32 {
        // We need first to convert the Color into a Color32, and then get the values.
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the color of a removed sprite.");
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return new float[] { ((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b };
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return new float[] { ((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b };
            }
        }
        set {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                // If we don't have three/four floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)alpha32);
                else if (value.Length == 4) imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)value[3]);
                else                        throw new ScriptRuntimeException("You need 3 or 4 numeric values when setting a sprite's color.");
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                // If we don't have three/four floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)alpha32);
                else if (value.Length == 4) imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)value[3]);
                else                        throw new ScriptRuntimeException("You need 3 or 4 numeric values when setting a sprite's color.");
            }
        }
    }

    // The alpha of the sprite. It is clamped between 0 and 1
    public float alpha {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the alpha of a removed sprite.");
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return imgtemp.color.a;
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return imgtemp.color.a;
            }
        }
        set {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                imgtemp.color = new Color(imgtemp.color.r, imgtemp.color.g, imgtemp.color.b, Mathf.Clamp01(value));
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                imgtemp.color = new Color(imgtemp.color.r, imgtemp.color.g, imgtemp.color.b, Mathf.Clamp01(value));
            }
        }
    }

    // The alpha of the sprite in a 32 bits format. It is clamped between 0 and 255
    public float alpha32 {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the alpha of a removed sprite.");
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return ((Color32)imgtemp.color).a;
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return ((Color32)imgtemp.color).a;
            }
        }
        // We need first to convert the Color into a Color32, and then get the values.
        // Color32s needs bytes, not ints, so we cast them
        set {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                imgtemp.color = new Color32(((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b, (byte)value);
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                imgtemp.color = new Color32(((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b, (byte)value);
            }
        }
    }

    // The rotation of the sprite
    public float rotation {
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the rotation of a removed sprite.");
            return internalRotation.z;
        }
        set {
            // We mod the value from 0 to 360 because angles are between 0 and 360 normally
            internalRotation.z = Math.mod(value, 360);
            img.GetComponent<RectTransform>().eulerAngles = internalRotation;
        }
    }

    // The layer of the sprite
    public string layer {
        // You can't get or set the layer on an enemy sprite
        get {
            if (!img) throw new ScriptRuntimeException("You can't get the layer of a removed sprite.");
            if (tag == "enemy" || tag == "bubble")
                return "none";
            if (tag == "projectile" && !img.transform.parent.name.Contains("Layer"))
                return "BulletPool";
            return img.transform.parent.name.Substring(0, img.transform.parent.name.Length - 5);
        } set {
            if (tag == "enemy" || tag == "bubble")
                return;
            Transform parent = img.transform.parent;
            try { img.transform.SetParent(GameObject.Find(value + "Layer").transform); } 
            catch { img.transform.SetParent(parent); }
        }
    }

    /*
    public bool filter {
        get { return img.sprite.texture.filterMode != FilterMode.Point; }
        set {
            if (value)  img.sprite.texture.filterMode = FilterMode.Trilinear;
            else        img.sprite.texture.filterMode = FilterMode.Point;
        }
    }
    */

    // The function that creates a sprite.
    public LuaSpriteController(Image i, string name = "empty") {
        img = i.gameObject;
        originalSprite = i.sprite;
        nativeSizeDelta = new Vector2(100, 100);
        if (i.gameObject.GetComponent<Projectile>())                   tag = "projectile";
        else if (i.gameObject.GetComponent<LuaEnemyController>())      tag = "enemy";
        else if (i.transform.parent != null)
            if (i.transform.parent.GetComponent<LuaEnemyController>()) tag = "bubble";
        else                                                           tag = "other";
        spritename = name.ToLower();
    }
    
    public LuaSpriteController(SpriteRenderer i, string name = "empty") {
        img = i.gameObject;
        originalSprite = i.sprite;
        nativeSizeDelta = new Vector2(100, 100);
        if (i.gameObject.GetComponent<Projectile>())                   tag = "projectile";
        else if (i.gameObject.GetComponent<LuaEnemyController>())      tag = "enemy";
        else if (i.transform.parent != null)
            if (i.transform.parent.GetComponent<LuaEnemyController>()) tag = "bubble";
        else                                                           tag = "other";
        spritename = name.ToLower();
    }

    // Changes the sprite of this instance
    public void Set(string name) {
        if (!img) throw new ScriptRuntimeException("You can't set the sprite of a removed sprite.");
        // Change the sprite
        if (img.GetComponent<Image>()) {
            Image imgtemp = img.GetComponent<Image>();
            SpriteUtil.SwapSpriteFromFile(imgtemp, name);
            originalSprite = imgtemp.sprite;
            nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
        } else {
            SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
            SpriteUtil.SwapSpriteFromFile(imgtemp, name);
            originalSprite = imgtemp.sprite;
            nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
        }
        Scale(xScale, yScale);
        if (tag == "projectile")
            img.GetComponent<Projectile>().needUpdateTex = true;
        spritename = name.ToLower();
    }

    // Sets the parent of a sprite. Can't be used on an enemy
    public void SetParent(LuaSpriteController parent) {
        if (!img) throw new ScriptRuntimeException("You can't set the parent of a removed sprite.");
        if (tag == "enemy" || tag == "bubble")
            return;
        img.transform.SetParent(parent.img.transform);
    }
    
    // Sets the pivot of a sprite (its rotation point)
    public void SetPivot(float x, float y) {
        if (!img) throw new ScriptRuntimeException("You can't set the pivot of a removed sprite.");
        img.GetComponent<RectTransform>().pivot = new Vector2(x, y);
    }

    // Sets the anchor of a sprite
    public void SetAnchor(float x, float y) {
        if (!img) throw new ScriptRuntimeException("You can't set the anchor of a removed sprite.");
        img.GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
        img.GetComponent<RectTransform>().anchorMax = new Vector2(x, y);
    }

    // Sets both xScale and yScale of a sprite 
    public void Scale(float xs, float ys) {
        if (!img) throw new ScriptRuntimeException("You can't set the scale of a removed sprite.");
        xScale = xs;
        yScale = ys;
        if (img.GetComponent<Image>())
            nativeSizeDelta = new Vector2(img.GetComponent<Image>().sprite.texture.width, img.GetComponent<Image>().sprite.texture.height);
        else
            nativeSizeDelta = new Vector2(img.GetComponent<SpriteRenderer>().sprite.texture.width, img.GetComponent<SpriteRenderer>().sprite.texture.height);
        img.GetComponent<RectTransform>().sizeDelta = new Vector2(nativeSizeDelta.x * Mathf.Abs(xScale), nativeSizeDelta.y * Mathf.Abs(yScale));
        internalRotation = new Vector3(ys < 0 ? 180 : 0, xs < 0 ? 180 : 0, internalRotation.z);
        img.GetComponent<RectTransform>().eulerAngles = internalRotation;
        /*Transform buffer = img.GetComponent<RectTransform>().parent; int bufferIndex = img.GetComponent<RectTransform>().GetSiblingIndex();
        img.GetComponent<RectTransform>().SetParent(null);
        Vector2 oldScale = new Vector3(img.GetComponent<RectTransform>().lossyScale.x, img.GetComponent<RectTransform>().lossyScale.y, 1);
        img.transform.localScale = new Vector3(xs, ys, 1);
        foreach (RectTransform rt in img.GetComponentsInChildren<RectTransform>())
            if (rt.gameObject.name != img.name)
                rt.localScale = new Vector3(rt.localScale.x / (img.GetComponent<RectTransform>().lossyScale.x / oldScale.x), rt.localScale.y / (img.GetComponent<RectTransform>().lossyScale.y / oldScale.y), 1);
        img.GetComponent<RectTransform>().SetParent(buffer);
        img.GetComponent<RectTransform>().SetSiblingIndex(bufferIndex);*/
    }

    // Sets an animation for this instance
    public void SetAnimation(string[] frames) { SetAnimation(frames, 1 / 30f); }

    // Sets an animation for this instance with a frame timer
    public void SetAnimation(string[] spriteNames, float frametime) {
        if (!img) throw new ScriptRuntimeException("You can't set the animation of a removed sprite.");
        Keyframe[] kfArray = new Keyframe[spriteNames.Length];
        for (int i = 0; i < spriteNames.Length; i++)
            kfArray[i] = new Keyframe(SpriteRegistry.Get(spriteNames[i]), spriteNames[i].ToLower());
        if (keyframes == null) {
            keyframes = img.AddComponent<KeyframeCollection>();
            keyframes.spr = this;
        } else
            keyframes.enabled = true;
        keyframes.loop = loop;
        keyframes.Set(kfArray, frametime);
        UpdateAnimation();
    }

    public void StopAnimation() {
        if (!img) throw new ScriptRuntimeException("You can't stop the animation of a removed sprite.");
        if (keyframes != null) {
            keyframes.enabled = false;

            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                imgtemp.sprite = originalSprite;
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                imgtemp.sprite = originalSprite;
            }
        }
    }

    public void MoveTo(float x, float y) {
        if (!img) throw new ScriptRuntimeException("You can't move a removed sprite.");
        img.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    public void MoveToAbs(float x, float y) {
        if (!img) throw new ScriptRuntimeException("You can't move a removed sprite.");
        img.GetComponent<RectTransform>().position = new Vector2(x, y);
    }

    public void SendToTop() {
        if (!img) throw new ScriptRuntimeException("You can't send a removed sprite to the top of the sprite list.");
        if (tag == "enemy" || tag == "bubble")
            return;
        img.GetComponent<RectTransform>().SetAsLastSibling();
    }

    public void SendToBottom() {
        if (!img) throw new ScriptRuntimeException("You can't send a removed sprite to the bottom of the sprite list.");
        if (tag == "enemy" || tag == "bubble")
            return;
        img.GetComponent<RectTransform>().SetAsFirstSibling();
    }

    public void Remove() {
        if (!img) throw new ScriptRuntimeException("You can't remove a removed sprite.");
        if (tag == "enemy" || tag == "bubble")
            return;

        if (tag == "projectile") {
            Projectile[] pcs = img.GetComponentsInChildren<Projectile>();
            for (int i = 1; i < pcs.Length; i++)
                pcs[i].ctrl.Remove();
        }
        StopAnimation();
        GameObject.Destroy(img.gameObject);
        img = null;
    }

    public void Dust(bool playDust = true, bool removeObject = false) {
        if (!img) throw new ScriptRuntimeException("You can't dust a removed sprite to the top of the sprite list.");
        if (tag == "enemy" || tag == "bubble")
            return;
        GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/MonsterDuster"));
        go.transform.SetParent(UIController.instance.psContainer.transform);
        if (playDust)
            UnitaleUtil.PlaySound("DustSound", AudioClipRegistry.GetSound("enemydust"));
        img.GetComponent<ParticleDuplicator>().Activate(this);
        if (img.gameObject.name != "player") {
            if (removeObject) {
                if (tag == "projectile") img.GetComponent<Projectile>().ctrl.Remove();
                else if (tag == "other") Remove();
            } else img.SetActive(false);
        }
    }

    internal void UpdateAnimation() {
        if (!img)
            return;
        if (keyframes == null)
            return;
        Keyframe k = keyframes.getCurrent();
        Sprite s = SpriteRegistry.GENERIC_SPRITE_PREFAB.sprite;
        if (k != null)
            s = k.sprite;
        else {
            StopAnimation();
            return;
        }
        if (k.sprite != null) {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                if (imgtemp.sprite != s) {
                    imgtemp.sprite = s;
                    originalSprite = imgtemp.sprite;
                    nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
                }
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                if (imgtemp.sprite != s) {
                    imgtemp.sprite = s;
                    originalSprite = imgtemp.sprite;
                    nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
                }
            }
            spritename = k.name.ToLower();
            Scale(xScale, yScale);
            if (tag == "projectile")
                img.GetComponent<Projectile>().needUpdateTex = true;
        }
    }

    /*
    internal void UpdateAnimation() {
        if (keyframes == null)
            return;
        Keyframe k = keyframes.getCurrent();
        Sprite s = SpriteRegistry.GENERIC_SPRITE_PREFAB.sprite;
        
        if (k != null)
            s = k.sprite;

        if (img.sprite != s)
            img.sprite = s;
    }*/

    void Update() {
        UpdateAnimation();
        firstFrame = false;
    }

    public void SetVar(string name, DynValue value) { vars[name] = value; }

    public DynValue GetVar(string name) {
        DynValue retval;
        if (vars.TryGetValue(name, out retval)) return retval;
        else return DynValue.NewNil();
    }

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }
}
