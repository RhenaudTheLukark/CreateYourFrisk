using UnityEngine;
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.UI;

public class LuaSpriteController {
    [MoonSharpHidden] public GameObject _img;  // The real image
    internal GameObject img { // A image that returns the real image. We use this to be able to detect if the real image is null, and if it is, throw an exception
        get {
            if (_img == null)
                throw new CYFException("Attempted to perform action on removed sprite.");
            if (!_img.activeInHierarchy && !firstFrame)
                throw new CYFException("Attempted to perform action on removed sprite.");
            return _img;
        }
        set { _img = value; }
    }
    public LuaSpriteShader shader;
    private bool firstFrame = true;
    private Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();
    [MoonSharpHidden] public Vector2 nativeSizeDelta;                   // The native size of the image
    private Vector3 internalRotation = Vector3.zero;  // The rotation of the sprite
    private float xScale = 1;                         // The X scale of the sprite
    private float yScale = 1;                         // The Y scale of the sprite
    private Sprite originalSprite;                    // The original sprite
    [MoonSharpHidden] public KeyframeCollection keyframes;              // This variable is used to store an animation
    [MoonSharpHidden] public string tag;                                // The tag of the sprite : "projectile", "enemy", "bubble", "letter" or "other"
    private KeyframeCollection.LoopMode loop = KeyframeCollection.LoopMode.LOOP;
    [MoonSharpHidden] public static MoonSharp.Interpreter.Interop.IUserDataDescriptor data = UserData.GetDescriptorForType<LuaSpriteController>(true);

    //The name of the sprite
    public string spritename {
        get {
            if (img.GetComponent<Image>())
                return img.GetComponent<Image>().sprite.name;
            else
                return img.GetComponent<SpriteRenderer>().sprite.name;
        }
    }

    // The x position of the sprite, relative to the arena position and its anchor.
    public float x {
        get {
            float val = img.GetComponent<RectTransform>().anchoredPosition.x;
            if (img.transform.parent != null)
                if (img.transform.parent.name == "SpritePivot")
                    val += img.transform.parent.localPosition.x;
            return val;
        }
        set {
            if (img.transform.parent.name == "SpritePivot")
                img.transform.parent.localPosition = new Vector3(value, img.transform.parent.localPosition.y, img.transform.parent.localPosition.z) - (Vector3)img.GetComponent<RectTransform>().anchoredPosition;
            else
                img.GetComponent<RectTransform>().anchoredPosition = new Vector2(value, img.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    // The y position of the sprite, relative to the arena position and its anchor.
    public float y {
        get {
            float val = img.GetComponent<RectTransform>().anchoredPosition.y;
            if (img.transform.parent != null)
                if (img.transform.parent.name == "SpritePivot")
                    val += img.transform.parent.localPosition.y;
            return val;
        }
        set {
            if (img.transform.parent.name == "SpritePivot")
                img.transform.parent.localPosition = new Vector3(img.transform.parent.localPosition.x, value, img.transform.parent.localPosition.z) - (Vector3)img.GetComponent<RectTransform>().anchoredPosition;
            else
                img.GetComponent<RectTransform>().anchoredPosition = new Vector2(img.GetComponent<RectTransform>().anchoredPosition.x, value);
        }
    }

    // The z position of the sprite, relative to the arena position and its anchor. (Only useful in the overworld)
    public float z {
        get { return GetTarget().localPosition.z; }
        set {
            Transform target = GetTarget();
            target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, value);
        }
    }

    // The x position of the sprite, relative to the bottom left corner of the screen.
    public float absx {
        get { return GetTarget().position.x; }
        set {
            Transform target = GetTarget();
            target.position = new Vector3(value, target.position.y, target.position.z);
        }
    }

    // The y position of the sprite, relative to the bottom left corner of the screen.
    public float absy {
        get { return GetTarget().position.y; }
        set {
            Transform target = GetTarget();
            target.position = new Vector3(target.position.x, value, target.position.z);
        }
    }

    // The z position of the sprite, relative to the bottom left corner of the screen. (Only useful in the overworld)
    public float absz {
        get { return GetTarget().position.z; }
        set {
            Transform target = GetTarget();
            target.position = new Vector3(target.position.x, target.position.y, value);
        }
    }

    // The x scale of the sprite. This variable is used for the same purpose as img, to be able to do other things when setting the variable
    public float xscale {
        get { return xScale; }
        set {
            xScale = value;
            Scale(xScale, yScale);
        }
    }

    // The y scale of the sprite.
    public float yscale {
        get { return yScale; }
        set {
            yScale = value;
            Scale(xScale, yScale);
        }
    }

    // Is the sprite active? True if the image of the sprite isn't null, false otherwise
    public bool isactive {
        get { return GlobalControls.retroMode ? _img == null : _img != null; }
    }

    // The original width of the sprite
    public float width {
        get {
            if (tag == "letter")                return img.GetComponent<Image>().sprite.rect.width;
            else if (img.GetComponent<Image>()) return img.GetComponent<Image>().mainTexture.width;
            else                                return img.GetComponent<SpriteRenderer>().sprite.texture.width;
        }
    }

    // The original height of the sprite
    public float height {
        get {
            if (tag == "letter")                return img.GetComponent<Image>().sprite.rect.height;
            else if (img.GetComponent<Image>()) return img.GetComponent<Image>().mainTexture.height;
            else                                return img.GetComponent<SpriteRenderer>().sprite.texture.height;
        }
    }

    // The x pivot of the sprite.
    public float xpivot {
        get { return img.GetComponent<RectTransform>().pivot.x; }
        set { SetPivot(value, img.GetComponent<RectTransform>().pivot.y); }
    }

    // The y pivot of the sprite.
    public float ypivot {
        get { return img.GetComponent<RectTransform>().pivot.y; }
        set { SetPivot(img.GetComponent<RectTransform>().pivot.x, value); }
    }

    // Is the current animation finished? True if there is a finished animation, false otherwise
    public bool animcomplete {
        get {
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
        get { return loop.ToString(); }
        set {
            try {
                loop = (KeyframeCollection.LoopMode)Enum.Parse(typeof(KeyframeCollection.LoopMode), value.ToUpper(), true);
                if (keyframes != null)
                    keyframes.SetLoop((KeyframeCollection.LoopMode)Enum.Parse(typeof(KeyframeCollection.LoopMode), value.ToUpper(), true));
            } catch { throw new CYFException("sprite.loopmode can only have either \"ONESHOT\", \"ONESHOTEMPTY\" or \"LOOP\", but you entered \"" + value.ToUpper() + "\"."); }
        }
    }

    // The color of the sprite. It uses an array of three floats between 0 and 1
    public float[] color {
        get {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return new float[] { imgtemp.color.r, imgtemp.color.g, imgtemp.color.b };
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return new float[] { imgtemp.color.r, imgtemp.color.g, imgtemp.color.b };
            }
        }
        set {
            if (value == null)
                throw new CYFException("sprite.color can't be nil.");
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                // If we don't have three floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color(value[0], value[1], value[2], alpha);
                else if (value.Length == 4) imgtemp.color = new Color(value[0], value[1], value[2], value[3]);
                else                        throw new CYFException("You need 3 or 4 numeric values when setting a sprite's color.");
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                // If we don't have three floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color(value[0], value[1], value[2], alpha);
                else if (value.Length == 4) imgtemp.color = new Color(value[0], value[1], value[2], value[3]);
                else                        throw new CYFException("You need 3 or 4 numeric values when setting a sprite's color.");
            }
        }
    }

    // The color of the sprite on a 32 bits format. It uses an array of three floats between 0 and 255
    public float[] color32 {
        // We need first to convert the Color into a Color32, and then get the values.
        get {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return new float[] { ((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b };
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return new float[] { ((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b };
            }
        }
        set {
            if (value == null)
                throw new CYFException("sprite.color can't be nil.");
            for (int i = 0; i < value.Length; i++)
                if (value[i] < 0)        value[i] = 0;
                else if (value[i] > 255) value[i] = 255;
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                // If we don't have three/four floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)alpha32);
                else if (value.Length == 4) imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)value[3]);
                else                        throw new CYFException("You need 3 or 4 numeric values when setting a sprite's color.");
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                // If we don't have three/four floats, we throw an error
                if (value.Length == 3)      imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)alpha32);
                else if (value.Length == 4) imgtemp.color = new Color32((byte)value[0], (byte)value[1], (byte)value[2], (byte)value[3]);
                else                        throw new CYFException("You need 3 or 4 numeric values when setting a sprite's color.");
            }
        }
    }

    // The alpha of the sprite. It is clamped between 0 and 1
    public float alpha {
        get {
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
        get { return internalRotation.z; }
        set {
            // We mod the value from 0 to 360 because angles are between 0 and 360 normally
            internalRotation.z = Math.Mod(value, 360);
            img.GetComponent<RectTransform>().eulerAngles = internalRotation;
            if (img.GetComponent<Projectile>() && img.GetComponent<Projectile>().isPP())
                img.GetComponent<Projectile>().needSizeRefresh = true;
        }
    }

    // The layer of the sprite
    public string layer {
        // You can't get or set the layer on an enemy sprite
        get {
            Transform target = GetTarget();
            if (tag == "bubble" || tag == "event" || tag == "letter")         return "none";
            if (tag == "projectile" && !target.parent.name.Contains("Layer")) return "BulletPool";
            if (tag == "enemy" && !target.parent.name.Contains("Layer"))      return "specialEnemyLayer";
            return target.parent.name.Substring(0, target.parent.name.Length - 5);
        } set {
            if      (tag == "event")  throw new CYFException("sprite.layer: Overworld events' layers can't be changed.");
            else if (tag == "bubble") throw new CYFException("sprite.layer: Bubbles' layers can't be changed.");
            else if (tag == "letter") throw new CYFException("sprite.layer: Letters' layers can't be changed.");
            Transform target = GetTarget();
            Transform parent = target.parent;
            try {
                target.SetParent(GameObject.Find(value + "Layer").transform);
                img.GetComponent<MaskImage>().inverted = false;
            } catch { target.SetParent(parent); }
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
    public LuaSpriteController(Image i) {
        img = i.gameObject;
        originalSprite = i.sprite;
        nativeSizeDelta = new Vector2(100, 100);
        if (img.GetComponent<Projectile>())                            tag = "projectile";
        else if (img.GetComponent<LuaEnemyController>())               tag = "enemy";
        else if (i.transform.parent != null)
            if (i.transform.parent.GetComponent<LuaEnemyController>()) tag = "bubble";
            else                                                       tag = "other";
        shader = new LuaSpriteShader("sprite", img);
    }

    public LuaSpriteController(SpriteRenderer i) {
        img = i.gameObject;
        originalSprite = i.sprite;
        nativeSizeDelta = new Vector2(100, 100);
        tag = "event";
        shader = new LuaSpriteShader("event", img);
    }

    // Changes the sprite of this instance
    public void Set(string name) {
        // Change the sprite
        if (name == null)
            throw new CYFException("You can't set a sprite as nil!");
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
    }

    // Sets the parent of a sprite.
    public void SetParent(LuaSpriteController parent) {
        if      (tag == "bubble")                                              throw new CYFException("sprite.SetParent() can not be used with bubbles.");
        else if (tag == "event" || (parent != null && parent.tag == "event"))  throw new CYFException("sprite.SetParent() can not be used with an Overworld Event's sprite.");
        else if (tag == "letter" ^ (parent != null && parent.tag == "letter")) throw new CYFException("sprite.SetParent() can not be used between letter sprites and other sprites.");
        try {
            GetTarget().SetParent(parent.img.transform);
            if (img.GetComponent<MaskImage>())
                img.GetComponent<MaskImage>().inverted = parent._masked > 3;
        } catch { throw new CYFException("sprite.SetParent(): You tried to set a removed sprite/nil sprite as this sprite's parent."); }
    }

    // Sets the pivot of a sprite (its rotation point)
    public void SetPivot(float x, float y) {
        img.GetComponent<RectTransform>().pivot = new Vector2(x, y);
        if (img.transform.parent.name == "SpritePivot")
            img.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }

    // Sets the anchor of a sprite
    public void SetAnchor(float x, float y) {
        img.GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
        img.GetComponent<RectTransform>().anchorMax = new Vector2(x, y);
    }

    public void Move(float x, float y) {
        if (img.transform.parent.name == "SpritePivot")
            img.transform.parent.localPosition = new Vector3(x + this.x, y + this.y, img.transform.parent.localPosition.z) - (Vector3)img.GetComponent<RectTransform>().anchoredPosition;
        else
            img.GetComponent<RectTransform>().anchoredPosition = new Vector2(x + this.x, y + this.y);
    }

    public void MoveTo(float x, float y) {
        if (img.transform.parent.name == "SpritePivot")
            img.transform.parent.localPosition = new Vector3(x, y, img.transform.parent.localPosition.z) - (Vector3)img.GetComponent<RectTransform>().anchoredPosition;
        else
            img.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    public void MoveToAbs(float x, float y) {
        GetTarget().position = new Vector3(x, y, GetTarget().position.z);
    }

    // Sets both xScale and yScale of a sprite
    public void Scale(float xs, float ys) {
        if (img.GetComponent<Projectile>())
            img.GetComponent<Projectile>().needSizeRefresh = true;
        xScale = xs;
        yScale = ys;
        if (tag == "letter") {
            nativeSizeDelta = new Vector2(img.GetComponent<Image>().sprite.rect.width, img.GetComponent<Image>().sprite.rect.height);
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(nativeSizeDelta.x * Mathf.Abs(xScale), nativeSizeDelta.y * Mathf.Abs(yScale));
        } else if (img.GetComponent<Image>()) { // In battle
            nativeSizeDelta = new Vector2(img.GetComponent<Image>().sprite.texture.width, img.GetComponent<Image>().sprite.texture.height);
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(nativeSizeDelta.x * Mathf.Abs(xScale), nativeSizeDelta.y * Mathf.Abs(yScale));
            // img.GetComponent<RectTransform>().localScale = new Vector3(xs < 0 ? -1 : 1, ys < 0 ? -1 : 1, 1);
        } else { // In overworld
            nativeSizeDelta = new Vector2(img.GetComponent<SpriteRenderer>().sprite.texture.width, img.GetComponent<SpriteRenderer>().sprite.texture.height);
            img.GetComponent<RectTransform>().localScale = new Vector3(100 * Mathf.Abs(xScale), 100 * Mathf.Abs(yScale), 1);
        }
        internalRotation = new Vector3(ys < 0 ? 180 : 0, xs < 0 ? 180 : 0, internalRotation.z);
        img.GetComponent<RectTransform>().eulerAngles = internalRotation;
    }

    // Sets an animation for this instance
    public void SetAnimation(string[] frames) { SetAnimation(frames, 1 / 30f); }

    // Sets an animation for this instance with a frame timer
    public void SetAnimation(string[] spriteNames, float frametime, string prefix = "") {
        if (spriteNames == null)
            throw new CYFException("sprite.SetAnimation: The first argument (list of images) is nil.\n\nSee the documentation for proper usage.");
        else if (spriteNames.Length == 0)
            throw new CYFException("sprite.SetAnimation: No sequence of animations was provided (animation table is empty).");
        if (frametime < 0)
            throw new CYFException("sprite.SetAnimation: An animation can not have negative speed!");
        else if (frametime == 0)
            throw new CYFException("sprite.SetAnimation: An animation can not play at 0 frames per second!");

        if (prefix != "") {
            while (prefix.StartsWith("/"))
                prefix = prefix.Substring(1);

            if (!prefix.EndsWith("/"))
                prefix += "/";

            for (int i = 0; i < spriteNames.Length; i++)
                spriteNames[i] = prefix + spriteNames[i];
        }

        Vector2 pivot = img.GetComponent<RectTransform>().pivot;
        Keyframe[] kfArray = new Keyframe[spriteNames.Length];
        for (int i = 0; i < spriteNames.Length; i++) {
            // at least one sprite in the sequence was unable to be loaded
            if (SpriteRegistry.Get(spriteNames[i]) == null)
                throw new CYFException("sprite.SetAnimation: Failed to load sprite with the name \"" + spriteNames[i] + "\". Are you sure it is spelled correctly?");

            kfArray[i] = new Keyframe(SpriteRegistry.Get(spriteNames[i]), spriteNames[i].ToLower());
        }
        if (keyframes == null) {
            if (img.GetComponent<KeyframeCollection>()) {
                keyframes = img.GetComponent<KeyframeCollection>();
                keyframes.enabled = true;
            } else {
                keyframes = img.AddComponent<KeyframeCollection>();
                keyframes.spr = this;
            }
        } else
            keyframes.enabled = true;
        keyframes.loop = loop;
        keyframes.Set(kfArray, frametime);
        UpdateAnimation();
        img.GetComponent<RectTransform>().pivot = pivot;
    }

    public void StopAnimation() {
        if (keyframes != null) {
            Vector2 pivot = img.GetComponent<RectTransform>().pivot;
            keyframes.enabled = false;
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                imgtemp.sprite = originalSprite;
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                imgtemp.sprite = originalSprite;
            }
            img.GetComponent<RectTransform>().pivot = pivot;
        }
    }

    // Gets or sets the paused state of a sprite's animation.
    public DynValue animationpaused {
        get {
            if (img && keyframes != null)
                return DynValue.NewBoolean(keyframes.paused);
            return DynValue.NewNil();
        }
        set {
            if (img) {
                if (value.Type != DataType.Boolean)
                    throw new CYFException("sprite.paused can only be set to a boolean value.");

                if (keyframes != null)
                    keyframes.paused = value.Boolean;
                else
                    throw new CYFException("Unable to pause/resume a sprite without an active animation.");
            }
        }
    }

    // Gets or sets the current frame of an animated sprite's animation.
    // Example: If a sprite's animation table is      {"sans_head_1", "sans_head_2", "sans_head_3", "sans_head"2},
    // then for each sprite in the table, this will be: ^ 1            ^ 2            ^ 3            ^ 4
    public int currentframe {
        set {
            if (img) {
                if (keyframes != null && keyframes.enabled) {
                    if (value < 1 || value > keyframes.keyframes.Length)
                        throw new CYFException("sprite.currentframe: New value " + value + " is out of bounds.");
                    else {
                        // Store the previous "progress" of the frame
                        float progress = (keyframes.currTime / keyframes.timePerFrame) % 1;
                        // Calls keyframes.currTime %= keyframes.totalTime
                        keyframes.SetLoop(keyframes.loop);
                        keyframes.currTime = ((value - 1) * keyframes.timePerFrame) + (progress * keyframes.timePerFrame);
                    }
                } else
                    throw new CYFException("sprite.currentframe: You can not set the current frame of a sprite without an active animation.");
            }
        }
        get {
            if (img && keyframes != null)
                return keyframes.getIndex();
            return 0;
        }
    }

    // Gets or sets the current "play position" of a sprite's animation, in seconds.
    public float currenttime {
        set {
            if (img) {
                if (keyframes != null && keyframes.enabled) {
                    if (value < 0 || value > keyframes.totalTime)
                        throw new CYFException("sprite.currenttime: New value " + value + " is out of bounds.");
                    else
                        keyframes.currTime = value % keyframes.totalTime;
                } else
                    throw new CYFException("sprite.currenttime: You can not set the current time of a sprite without an active animation.");
            }
        }
        get {
            if (img && keyframes != null) {
                if (keyframes.enabled) {
                    if (!keyframes.animationComplete())
                        return keyframes.currTime % keyframes.totalTime;
                }
                return keyframes.totalTime;
            }
            return 0;
        }
    }

    // Gets (read-only) the total time an animation will run for, in seconds.
    public float totaltime {
        get {
            if (img && keyframes != null)
                return keyframes.totalTime;
            return 0;
        }
    }

    // Gets or sets the speed of an animated sprite's animation.
    public float animationspeed {
        set {
            if (img) {
                if (keyframes != null) {
                    if (value < 0)
                        throw new CYFException("sprite.animationspeed: An animation can not have negative speed!");
                    else if (value == 0)
                        throw new CYFException("sprite.animationspeed: An animation can not play at 0 frames per second!");

                    float percentCompletion = keyframes.currTime / (keyframes.keyframes.Length * keyframes.timePerFrame);
                    // Calls keyframes.totalTime = keyframes.timePerFrame * keyframes.Length;
                    keyframes.Set(keyframes.keyframes, value);
                    keyframes.currTime = percentCompletion * (keyframes.keyframes.Length * keyframes.timePerFrame);
                    // Calls keyframes.currTime %= keyframes.totalTime
                    keyframes.SetLoop(keyframes.loop);
                } else
                    throw new CYFException("sprite.animationspeed: You can not change the speed of a sprite without an active animation.");
            }
        }
        get {
            if (img && keyframes != null)
                return keyframes.timePerFrame;
            return 0;
        }
    }

    public void SendToTop() {
        GetTarget().SetAsLastSibling();
    }

    public void SendToBottom() {
        GetTarget().SetAsFirstSibling();
    }

    public void MoveBelow(LuaSpriteController sprite) {
        if (sprite == null)                                       throw new CYFException("sprite.MoveBelow: The sprite passed as an argument is nil.");
        else if (sprite.GetTarget().parent != GetTarget().parent) UnitaleUtil.Warn("You can't change the order of two sprites without the same parent.");
        else                                                      GetTarget().SetSiblingIndex(sprite.GetTarget().GetSiblingIndex());
    }

    public void MoveAbove(LuaSpriteController sprite) {
        if (sprite == null)                                       throw new CYFException("sprite.MoveAbove: The sprite passed as an argument is nil.");
        else if (sprite.GetTarget().parent != GetTarget().parent) UnitaleUtil.Warn("You can't change the order of two sprites without the same parent.");
        else                                                      GetTarget().SetSiblingIndex(sprite.GetTarget().GetSiblingIndex() + 1);
    }

    private Dictionary<string, int> maskTypes = new Dictionary<string, int>() {
        {"off",             0},
        {"box",             1},
        {"sprite",          2},
        {"stencil",         3},
        {"invertedsprite",  4},
        {"invertedstencil", 5}
    };
    [MoonSharpHidden] public int _masked = 0;
    public void Mask(string mode) {
        if (tag == "event")
            throw new CYFException("sprite.Mask: Can not be applied to Overworld Event sprites.");
        else if (tag == "letter")
            throw new CYFException("sprite.Mask: Can not be applied to Letter sprites.");
        else if (mode == null)
            throw new CYFException("sprite.Mask: No argument provided.");

        mode = mode.ToLower();
        int masked = -1;
        if (!maskTypes.TryGetValue(mode, out masked))
            throw new CYFException("sprite.Mask: Invalid mask mode \"" + mode.ToString() + "\".");

        if (masked != _masked) {
            //If children need to have their "inverted" property updated, then do so
            if ((_masked < 4 && masked > 3) || (_masked > 3 && masked < 4))
                foreach (Transform child in GetTarget()) {
                    MaskImage childmask = child.gameObject.GetComponent<MaskImage>();
                    if (childmask != null)
                        childmask.inverted = masked > 3;
                }
            RectMask2D box = img.GetComponent<RectMask2D>();
            Mask spr = img.GetComponent<Mask>();

            //Box mask mode
            if (masked == 1) {
                //Remove sprite mask if applicable
                spr.enabled = false;
                box.enabled = true;
            } else if (masked > 1) {
                //The mask mode now can't possibly be box, so remove box mask if applicable
                spr.enabled = true;
                box.enabled = false;
                // Used to differentiate between "sprite" and "stencil"-like display modes
                spr.showMaskGraphic = masked == 2 || masked == 4;
            //Mask has been disabled
            } else if (masked == 0) {
                spr.enabled = false;
                box.enabled = false;
            }
        }

        _masked = masked;
    }

    public void Remove() {
        if (_img == null)
            return;
        else if (!GlobalControls.retroMode && tag == "projectile") {
            img.GetComponent<Projectile>().ctrl.Remove();
            return;
        }

        bool throwError = false;
        if ((!GlobalControls.retroMode && img.gameObject.name == "player") || (!GlobalControls.retroMode && tag == "projectile") || tag == "enemy" || tag == "bubble") {
            if (img.gameObject.name == "player")
                throw new CYFException("sprite.Remove(): You can't remove the Player's sprite!");
            else if (tag == "projectile") {
                if (img.GetComponent<Projectile>().ctrl != null)
                    if (img.GetComponent<Projectile>().ctrl.isactive) throwError = true;
            } else                                                    throwError = true;
        }
        if (throwError)
            throw new CYFException("sprite.Remove(): You can't remove a " + tag + "'s sprite!");

        if (tag == "projectile") {
            Projectile[] pcs = img.GetComponentsInChildren<Projectile>();
            for (int i = 1; i < pcs.Length; i++)
                pcs[i].ctrl.Remove();
        }
        StopAnimation();
        GameObject.Destroy(GetTarget().gameObject);
        _img = null;
    }

    public void Dust(bool playDust = true, bool removeObject = false) {
        if (tag == "enemy" || tag == "bubble")
            throw new CYFException("sprite.Dust(): You can't dust a " + tag + "'s sprite!");

        GameObject go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/MonsterDuster"));
        go.transform.SetParent(UIController.instance.psContainer.transform);
        if (playDust)
            UnitaleUtil.PlaySound("DustSound", AudioClipRegistry.GetSound("enemydust"));
        img.GetComponent<ParticleDuplicator>().Activate(this);
        if (img.gameObject.name != "player") {
            img.SetActive(false);
            if (removeObject)
                Remove();
        }
    }

    internal void UpdateAnimation() {
        if (!img)
            return;
        if (keyframes == null || keyframes.paused)
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
            Quaternion rot = img.transform.rotation;
            Vector2 pivot = img.GetComponent<RectTransform>().pivot;
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                if (imgtemp.sprite != s) {
                    imgtemp.sprite = s;
                    originalSprite = imgtemp.sprite;
                    nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
                    shader.UpdateTexture(imgtemp.sprite.texture);
                    Scale(xScale, yScale);
                    if (tag == "projectile")
                        img.GetComponent<Projectile>().needUpdateTex = true;
                    img.transform.rotation = rot;
                }
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                if (imgtemp.sprite != s) {
                    imgtemp.sprite = s;
                    originalSprite = imgtemp.sprite;
                    nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
                    shader.UpdateTexture(imgtemp.sprite.texture);
                    Scale(xScale, yScale);
                    img.transform.rotation = rot;
                }
            }
            img.GetComponent<RectTransform>().pivot = pivot;
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

    public void SetVar(string name, DynValue value) {
        if (name == null)
            throw new CYFException("sprite.SetVar: The first argument (the index) is null.\n\nSee the documentation for proper usage.");
        vars[name] = value;
    }

    public DynValue GetVar(string name) {
        if (name == null)
            throw new CYFException("sprite.GetVar: The first argument (the index) is null.\n\nSee the documentation for proper usage.");
        DynValue retval;
        if (vars.TryGetValue(name, out retval)) return retval;
        else return DynValue.NewNil();
    }

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }

    private Transform GetTarget() {
        Transform target = img.transform;
        if (img.transform.parent.name == "SpritePivot")
            target = target.parent;
        return target;
    }
}
