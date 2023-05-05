using UnityEngine;
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.UI;

public class LuaSpriteController {
    [MoonSharpHidden] public CYFSprite spr;
    [MoonSharpHidden] public bool removed;
    [MoonSharpHidden] public bool limbo;
    internal GameObject img { // A image that returns the real image. We use this to be able to detect if the real image is null, and if it is, throw an exception
        get {
            if (removed) throw new CYFException("Attempted to perform action on removed sprite.");
            return spr.gameObject;
        }
    }
    public LuaSpriteShader shader;
    private readonly Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();
    [MoonSharpHidden] public Vector2 nativeSizeDelta;      // The native size of the image
    private Vector3 internalRotation = Vector3.zero;       // The rotation of the sprite
    private float xScale = 1;                              // The X scale of the sprite
    private float yScale = 1;                              // The Y scale of the sprite
    [MoonSharpHidden] public KeyframeCollection keyframes; // This variable is used to store an animation
    [MoonSharpHidden] public string tag;                   // The tag of the sprite : "projectile", "enemy", "letter" or "other"
    private KeyframeCollection.LoopMode loop = KeyframeCollection.LoopMode.LOOP;
    [MoonSharpHidden] public static MoonSharp.Interpreter.Interop.IUserDataDescriptor data = UserData.GetDescriptorForType<LuaSpriteController>(true);

    public void Reset() {
        removed = false;
        limbo = false;

        internalRotation = Vector3.zero;
        Scale(1, 1);
        SetPivot(0.5f, 0.5f);
        SetAnchor(0.5f, 0.5f);
        img.GetComponent<Image>().color = new Color(1, 1, 1, 1);

        Mask("OFF");
        shader.Revert();

        StopAnimation();
        keyframes = null;
        loop = KeyframeCollection.LoopMode.LOOP;

        vars.Clear();
    }

    // The name of the sprite
    [MoonSharpHidden] public string _spritename = "empty";
    public string spritename {
        // TODO: Restore in 0.7
        //get { return img.GetComponent<Image>() ? img.GetComponent<Image>().sprite.name : img.GetComponent<SpriteRenderer>().sprite.name; }
        get { return _spritename; }
        [MoonSharpHidden] set { _spritename = value; }
    }

    // The x position of the sprite, relative to the arena position and its anchor.
    public float x {
        get { return GetTarget().anchoredPosition.x + (GetTarget().gameObject != img ? img.transform.localPosition.x : 0); }
        set { MoveTo(value, y); }
    }

    // The y position of the sprite, relative to the arena position and its anchor.
    public float y {
        get { return GetTarget().anchoredPosition.y + (GetTarget().gameObject != img ? img.transform.localPosition.y : 0); }
        set { MoveTo(x, value); }
    }

    // The z position of the sprite, relative to the arena position and its anchor. (Only useful in the overworld)
    public float z {
        get { return GetTarget().localPosition.z; }
        set { MoveTo(x, y, value); }
    }

    // The x position of the sprite, relative to the bottom left corner of the screen.
    public float absx {
        get { return GetTarget().position.x; }
        set { MoveToAbs(value, absy); }
    }

    // The y position of the sprite, relative to the bottom left corner of the screen.
    public float absy {
        get { return GetTarget().position.y; }
        set { MoveToAbs(absx, value); }
    }

    // The z position of the sprite, relative to the bottom left corner of the screen. (Only useful in the overworld)
    public float absz {
        get { return GetTarget().position.z; }
        set { MoveToAbs(absx, absy, value); }
    }

    // The x scale of the sprite. This variable is used for the same purpose as img, to be able to do other things when setting the variable
    public float xscale {
        get { return xScale; }
        set { Scale(value, yScale); }
    }

    // The y scale of the sprite.
    public float yscale {
        get { return yScale; }
        set { Scale(xScale, value); }
    }

    // Is the sprite active? True if the image of the sprite isn't null, false otherwise
    public bool isactive {
        get { return !GlobalControls.retroMode ^ (removed || limbo); }
    }

    // The original width of the sprite
    public float width {
        get {
            if (tag == "letter")           return img.GetComponent<Image>().sprite.rect.width;
            if (img.GetComponent<Image>()) return img.GetComponent<Image>().mainTexture.width;
            return img.GetComponent<SpriteRenderer>().sprite.texture.width;
        }
    }

    // The original height of the sprite
    public float height {
        get {
            if (tag == "letter")           return img.GetComponent<Image>().sprite.rect.height;
            if (img.GetComponent<Image>()) return img.GetComponent<Image>().mainTexture.height;
            return img.GetComponent<SpriteRenderer>().sprite.texture.height;
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
            if (keyframes == null && img.GetComponent<KeyframeCollection>())
                keyframes = img.GetComponent<KeyframeCollection>();
            if (keyframes == null)                        return false;
            if (keyframes.enabled == false)               return true;
            if (loop == KeyframeCollection.LoopMode.LOOP) return false;
            return keyframes.enabled && keyframes.animationComplete();
        }
    }

    // The loop mode of the animation
    public string loopmode {
        get { return loop.ToString(); }
        set {
            try { loop = (KeyframeCollection.LoopMode)Enum.Parse(typeof(KeyframeCollection.LoopMode), value.ToUpper(), true); }
            catch { throw new CYFException("sprite.loopmode can only be either \"ONESHOT\", \"ONESHOTEMPTY\" or \"LOOP\", but you entered \"" + value.ToUpper() + "\"."); }
            if (keyframes != null)
                keyframes.SetLoop(loop);
        }
    }

    // The color of the sprite. It uses an array of three floats between 0 and 1
    public float[] color {
        get {
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                return new[] { imgtemp.color.r, imgtemp.color.g, imgtemp.color.b };
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                return new[] { imgtemp.color.r, imgtemp.color.g, imgtemp.color.b };
            }
        }
        set {
            if (value == null)
                throw new CYFException("sprite.color can't be nil.");
            if (img.GetComponent<Image>()) {
                Image imgtemp = img.GetComponent<Image>();
                switch (value.Length) {
                    // If we don't have three floats, we throw an error
                    case 3:  imgtemp.color = new Color(value[0], value[1], value[2], alpha);    break;
                    case 4 : imgtemp.color = new Color(value[0], value[1], value[2], value[3]); break;
                    default: throw new CYFException("You need 3 or 4 numeric values when setting a sprite's color.");
                }
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                switch (value.Length) {
                    // If we don't have three floats, we throw an error
                    case 3:  imgtemp.color = new Color(value[0], value[1], value[2], alpha);    break;
                    case 4:  imgtemp.color = new Color(value[0], value[1], value[2], value[3]); break;
                    default: throw new CYFException("You need 3 or 4 numeric values when setting a sprite's color.");
                }
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
                value[i] = Mathf.Clamp(value[0], 0, 255);
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
                imgtemp.color = new Color32(((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b, (byte)Mathf.Clamp(value, 0, 255));
            } else {
                SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
                imgtemp.color = new Color32(((Color32)imgtemp.color).r, ((Color32)imgtemp.color).g, ((Color32)imgtemp.color).b, (byte)Mathf.Clamp(value, 0, 255));
            }
        }
    }

    // The rotation of the sprite
    public float rotation {
        get { return Math.Mod(GetParentRot() + img.GetComponent<RectTransform>().localEulerAngles.z + (yScale < 0 ? 180 : 0), 360); }
        set {
            // We mod the value from 0 to 360 because angles are between 0 and 360 normally
            internalRotation.z = Math.Mod(value, 360);
            if (GlobalControls.isInFight && EnemyEncounter.script.GetVar("noscalerotationbug").Boolean) {
                internalRotation.z = Math.Mod(internalRotation.z - GetParentRot(), 360);
                img.GetComponent<RectTransform>().localEulerAngles = internalRotation;
            } else
                img.GetComponent<RectTransform>().eulerAngles = internalRotation;

            if (img.GetComponent<Projectile>() && img.GetComponent<Projectile>().isPP())
                img.GetComponent<Projectile>().needSizeRefresh = true;
        }
    }

    private float GetParentRot() {
        Transform t = spr.transform.parent;
        while (t != null) {
            CYFSprite sprite = t.GetComponent<CYFSprite>();
            if (sprite != null)
                return sprite.ctrl.rotation;
            t = t.parent;
        }
        return 0;
    }

    // The layer of the sprite
    public string layer {
        // You can't get or set the layer on an enemy sprite
        get {
            Transform target = GetTarget();
            if (tag == "event" || tag == "letter")                            return "none";
            if (tag == "projectile" && !target.parent.name.Contains("Layer")) return "BulletPool";
            if (tag == "enemy" && !target.parent.name.Contains("Layer"))      return "specialEnemyLayer";
            return target.parent.name.Substring(0, target.parent.name.Length - 5);
        } set {
            switch (tag) {
                case "event":  throw new CYFException("sprite.layer: Overworld events' layers can't be changed.");
                case "letter": throw new CYFException("sprite.layer: Letters' layers can't be changed.");
            }

            Transform target = GetTarget();
            Transform parent = target.parent;
            try {
                target.SetParent(GameObject.Find(value + "Layer").transform);
                img.GetComponent<MaskImage>().inverted = false;
            } catch { target.SetParent(parent); }
        }
    }

    public int characterNumber {
        get {
            if (tag != "letter")
                throw new CYFException("You can only use characterNumber on letters!");
            return img.GetComponent<Letter>().characterNumber;
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

    /// <summary>
    /// Creates the instance of LuaSpriteController for the given GameObject or returns one if it already exists.
    /// </summary>
    /// <param name="go">GameObject to create a controller for.</param>
    /// <param name="forceReset">Force the reset of the sprite object.</param>
    /// <returns>An instance of LuaSpriteController manipulating the Gameobject go.</returns>
    public static LuaSpriteController GetOrCreate(GameObject go, bool forceReset = false) {
        // Fetch or add the GameObject's CYFSprite component, then retrieve its controller if it exists
        CYFSprite newSpr = go.GetComponent<CYFSprite>() ?? go.AddComponent<CYFSprite>();
        LuaSpriteController ctrl = newSpr.ctrl ?? new LuaSpriteController { spr = newSpr };
        if (newSpr.ctrl != null && !forceReset) return ctrl;
        newSpr.ctrl = ctrl;

        // Images are used for most of CYF's sprites
        ctrl.nativeSizeDelta = new Vector2(100, 100);
        Image image = newSpr.GetComponent<Image>();
        if (image != null) {
            // A controller's tag gives us more info on what the sprite actually is used for
            if (ctrl.img.GetComponent<Projectile>())           ctrl.tag = "projectile";
            else if (ctrl.img.GetComponent<EnemyController>()) ctrl.tag = "enemy";
            else                                               ctrl.tag = "other";
            ctrl.shader = new LuaSpriteShader("sprite", ctrl.img);
        } else {
            // SpriteRenderers are used for overworld events
            ctrl.tag = "event";
            ctrl.shader = new LuaSpriteShader("event", ctrl.img);
        }

        return ctrl;
    }

    public static bool HasSpriteController(GameObject go) {
        CYFSprite newSpr = go.GetComponent<CYFSprite>();
        return newSpr != null && newSpr.ctrl != null;
    }

    // Changes the sprite of this instance
    public void Set(string name) {
        // Change the sprite
        if (name == null)
            throw new CYFException("You can't set a sprite as nil!");
        if (img.GetComponent<Image>()) {
            Image imgtemp = img.GetComponent<Image>();
            SpriteUtil.SwapSpriteFromFile(imgtemp, name);
            nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
            shader.UpdateTexture(imgtemp.sprite.texture);
        } else {
            SpriteRenderer imgtemp = img.GetComponent<SpriteRenderer>();
            SpriteUtil.SwapSpriteFromFile(imgtemp, name);
            nativeSizeDelta = new Vector2(imgtemp.sprite.texture.width, imgtemp.sprite.texture.height);
            shader.UpdateTexture(imgtemp.sprite.texture);
        }
        // TODO: Restore in 0.7
        //imgtemp.name = name;
        spritename = name;
        Scale(xScale, yScale);
        if (tag == "projectile")
            img.GetComponent<Projectile>().needUpdateTex = true;
    }

    // Sets the pivot of a sprite (its rotation point)
    public void SetPivot(float x, float y) {
        img.GetComponent<RectTransform>().pivot = new Vector2(x, y);
        if (img.transform.parent != null && img.transform.parent.name == "SpritePivot")
            img.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }

    // Sets the anchor of a sprite
    public void SetAnchor(float x, float y) {
        img.GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
        img.GetComponent<RectTransform>().anchorMax = new Vector2(x, y);
    }

    public void Move(float x, float y) { MoveTo(this.x + x, this.y + y); }
    public void Move(float x, float y, float z) { MoveTo(this.x, this.y, this.z); }

    public void MoveTo(float x, float y) {
        if (img.transform.parent != null && img.transform.parent.name == "SpritePivot")
            img.transform.parent.localPosition = new Vector3(x, y, img.transform.parent.localPosition.z) - (Vector3)img.GetComponent<RectTransform>().anchoredPosition;
        else if (tag == "letter" && (GetParent().UserData.Object as LuaTextManager).adjustTextDisplay)
            img.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Round(x) - 0.01f, Mathf.Round(y) - 0.01f);
        else
            img.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }
    public void MoveTo(float x, float y, float z) {
        if (img.transform.parent != null && img.transform.parent.name == "SpritePivot")
            img.transform.parent.localPosition = new Vector3(x, y, z) - (Vector3)img.GetComponent<RectTransform>().anchoredPosition;
        else
            MoveTo(x, y);
    }

    public void MoveToAbs(float x, float y) {
        if (tag == "letter" && (GetParent().UserData.Object as LuaTextManager).adjustTextDisplay)
            GetTarget().position = new Vector3(Mathf.Round(x) - 0.01f, Mathf.Round(y) - 0.01f, GetTarget().position.z);
        else
            GetTarget().position = new Vector3(x, y, GetTarget().position.z);
    }
    public void MoveToAbs(float x, float y, float z) {
        GetTarget().position = new Vector3(x, y, z);
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

        // Flip the sprite horizontally and/or vertically if its scale is negative
        // The noscalerotationbug variable handles internalRotation as local rotation instead of global
        float zValue = internalRotation.z;
        internalRotation = new Vector3(ys < 0 ? 180 : 0, xs < 0 ? 180 : 0, zValue);
        if (GlobalControls.isInFight && EnemyEncounter.script.GetVar("noscalerotationbug").Boolean)
            img.GetComponent<RectTransform>().localEulerAngles = internalRotation;
        else
            img.GetComponent<RectTransform>().eulerAngles = internalRotation;
    }

    // Sets an animation for this instance
    public void SetAnimation(string[] frames) { SetAnimation(frames, 1 / 30f); }

    // Sets an animation for this instance with a frame timer
    public void SetAnimation(string[] spriteNames, float frametime, string prefix = "") {
        if (spriteNames == null)     throw new CYFException("sprite.SetAnimation: The first argument (list of images) is nil.\n\nSee the documentation for proper usage.");
        if (spriteNames.Length == 0) throw new CYFException("sprite.SetAnimation: No sequence of animations was provided (animation table is empty).");
        if (frametime < 0)           throw new CYFException("sprite.SetAnimation: An animation can not have negative speed!");
        if (frametime == 0)          throw new CYFException("sprite.SetAnimation: An animation can not play at 0 frames per second!");

        if (prefix != "") {
            while (prefix.StartsWith("/"))
                prefix = prefix.Substring(1);

            if (!prefix.EndsWith("/"))
                prefix += "/";

            for (int i = 0; i < spriteNames.Length; i++)
                spriteNames[i] = prefix + spriteNames[i];
        }

        Keyframe[] kfArray = new Keyframe[spriteNames.Length];
        for (int i = spriteNames.Length - 1; i >= 0; i--) {
            Set(spriteNames[i]);
            kfArray[i] = new Keyframe(SpriteRegistry.Get(spriteNames[i]), spriteNames[i]);
        }
        if (keyframes == null) {
            if (img.GetComponent<KeyframeCollection>())
                keyframes = img.GetComponent<KeyframeCollection>();
            else {
                keyframes = img.AddComponent<KeyframeCollection>();
                keyframes.spr = this;
            }
        }
        keyframes.enabled = true;
        keyframes.loop = loop;
        keyframes.Set(kfArray, frametime);
        UpdateAnimation();
    }

    public void StopAnimation() {
        if (keyframes == null) return;
        keyframes.enabled = false;
    }

    // Gets or sets the paused state of a sprite's animation.
    public DynValue animationpaused {
        get {
            if (img && keyframes != null)
                return DynValue.NewBoolean(keyframes.paused);
            return DynValue.NewNil();
        }
        set {
            if (!img) return;
            if (value.Type != DataType.Boolean)
                throw new CYFException("sprite.paused can only be set to a boolean value.");

            if (keyframes != null)
                keyframes.paused = value.Boolean;
            else
                throw new CYFException("Unable to pause/resume a sprite without an active animation.");
        }
    }

    // Gets or sets the current frame of an animated sprite's animation.
    // Example: If a sprite's animation table is      {"sans_head_1", "sans_head_2", "sans_head_3", "sans_head"2},
    // then for each sprite in the table, this will be: ^ 1            ^ 2            ^ 3            ^ 4
    public int currentframe {
        set {
            if (!img) return;
            if (keyframes != null && keyframes.enabled) {
                if (value < 1 || value > keyframes.keyframes.Length)
                    throw new CYFException("sprite.currentframe: New value " + value + " is out of bounds.");
                // Store the previous "progress" of the frame
                float progress = (keyframes.currTime / keyframes.timePerFrame) % 1;
                // Calls keyframes.currTime %= keyframes.totalTime
                keyframes.SetLoop(keyframes.loop);
                keyframes.currTime = ((value - 1) * keyframes.timePerFrame) + (progress * keyframes.timePerFrame);
            } else
                throw new CYFException("sprite.currentframe: You can not set the current frame of a sprite without an active animation.");
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
            if (!img) return;
            if (keyframes != null && keyframes.enabled) {
                if (value < 0 || value > keyframes.totalTime)
                    throw new CYFException("sprite.currenttime: New value " + value + " is out of bounds.");
                keyframes.currTime = value % keyframes.totalTime;
            } else
                throw new CYFException("sprite.currenttime: You can not set the current time of a sprite without an active animation.");
        }
        get {
            if (!img || keyframes == null) return 0;
            if (!keyframes.enabled) return keyframes.totalTime;
            if (!keyframes.animationComplete())
                return keyframes.currTime % keyframes.totalTime;
            return keyframes.totalTime;
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
            if (!img) return;
            if (keyframes != null) {
                if (value < 0)  throw new CYFException("sprite.animationspeed: An animation can not have negative speed!");
                if (value == 0) throw new CYFException("sprite.animationspeed: An animation can not play at 0 frames per second!");

                float percentCompletion = keyframes.currTime / (keyframes.keyframes.Length * keyframes.timePerFrame);
                // Calls keyframes.totalTime = keyframes.timePerFrame * keyframes.Length;
                keyframes.Set(keyframes.keyframes, value);
                keyframes.currTime = percentCompletion * (keyframes.keyframes.Length * keyframes.timePerFrame);
                // Calls keyframes.currTime %= keyframes.totalTime
                keyframes.SetLoop(keyframes.loop);
            } else
                throw new CYFException("sprite.animationspeed: You can not change the speed of a sprite without an active animation.");
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
        if (sprite == null)                                  throw new CYFException("sprite.MoveBelow: The sprite passed as an argument is nil.");
        if (sprite.GetTarget().parent != GetTarget().parent) UnitaleUtil.Warn("You can't change the order of two sprites without the same parent.");
        else                                                 GetTarget().SetSiblingIndex(sprite.GetTarget().GetSiblingIndex());
    }

    public void MoveAbove(LuaSpriteController sprite) {
        if (sprite == null)                                  throw new CYFException("sprite.MoveAbove: The sprite passed as an argument is nil.");
        if (sprite.GetTarget().parent != GetTarget().parent) UnitaleUtil.Warn("You can't change the order of two sprites without the same parent.");
        else                                                 GetTarget().SetSiblingIndex(sprite.GetTarget().GetSiblingIndex() + 1);
    }

    public enum MaskMode {
        OFF,
        BOX,
        SPRITE,
        STENCIL,
        INVERTEDSPRITE,
        INVERTEDSTENCIL
    }
    [MoonSharpHidden] public MaskMode _masked;
    public void Mask(string mode) {
        switch (tag) {
            case "event":  throw new CYFException("sprite.Mask: Can not be applied to Overworld Event sprites.");
            case "letter": throw new CYFException("sprite.Mask: Can not be applied to Letter sprites.");
            default:       if (mode == null) throw new CYFException("sprite.Mask: No argument provided."); break;
        }

        MaskMode masked;
        try { masked = (MaskMode)Enum.Parse(typeof(MaskMode), mode, true); }
        catch { throw new CYFException("sprite.Mask: Invalid mask mode \"" + mode + "\"."); }

        if (masked != _masked) {
            //If children need to have their "inverted" property updated, then do so
            if ((int)_masked < 4 && (int)masked > 3 || (int)_masked > 3 && (int)masked < 4)
                foreach (Transform child in GetTarget()) {
                    MaskImage childmask = child.gameObject.GetComponent<MaskImage>();
                    if (childmask != null)
                        childmask.inverted = (int)masked > 3;
                }
            RectMask2D box = img.GetComponent<RectMask2D>();
            Mask mask = img.GetComponent<Mask>();

            switch (masked) {
                case MaskMode.BOX:
                    //Remove sprite mask if applicable
                    mask.enabled = false;
                    box.enabled = true;
                    break;
                case MaskMode.OFF:
                    //Mask has been disabled
                    mask.enabled = false;
                    box.enabled = false;
                    break;
                default:
                    //The mask mode now can't possibly be box, so remove box mask if applicable
                    mask.enabled = true;
                    box.enabled = false;
                    // Used to differentiate between "sprite" and "stencil"-like display modes
                    mask.showMaskGraphic = masked == MaskMode.SPRITE || masked == MaskMode.INVERTEDSPRITE;
                    break;
            }
        }

        _masked = masked;
    }

    public void Remove() {
        if (removed)
            return;

        if (!GlobalControls.retroMode) {
            if (tag == "projectile") {
                img.GetComponent<Projectile>().ctrl.Remove();
                return;
            }

            if (img.gameObject.name == "player")
                throw new CYFException("sprite.Remove(): You can't remove the Player's sprite!");
        }
        if (tag == "enemy")
            throw new CYFException("sprite.Remove(): You can't remove an enemy's sprite!");

        UnitaleUtil.RemoveChildren(img);

        if (limbo)
            return;

        StopAnimation();
        limbo = true;
    }

    public void Dust(bool playDust = true, bool removeObject = false) {
        if (tag == "enemy")
            throw new CYFException("sprite.Dust(): You can't dust an enemy's sprite!");
        if (removed)
            return;

        UnitaleUtil.Dust(img, this);
        if (playDust)
            UnitaleUtil.PlaySound("DustSound", "enemydust");
        if (removeObject && !img.GetComponent<PlayerController>())
            Remove();
    }

    internal void UpdateAnimation() {
        if (!img)
            return;
        if (keyframes == null || keyframes.paused)
            return;
        Keyframe k = keyframes.getCurrent();
        if (k != null) {
            // TODO: Restore in 0.7
            //img.name = k.name;
            spritename = k.name;
        } else {
            StopAnimation();
            return;
        }

        if (k.sprite == null) return;
        Set(spritename);
        // TODO: Remove in 0.7
        if (k == KeyframeCollection.EMPTY_KEYFRAME)
            spritename = "blank";
    }

    public void SetVar(string name, DynValue value) {
        if (name == null)
            throw new CYFException("sprite.SetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        vars[name] = value;
    }

    public DynValue GetVar(string name) {
        if (name == null)
            throw new CYFException("sprite.GetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        DynValue retval;
        return vars.TryGetValue(name, out retval) ? retval : DynValue.NewNil();
    }

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }

    private RectTransform GetTarget() {
        RectTransform target = img.GetComponent<RectTransform>();
        if (target.parent != null && target.parent.name == "SpritePivot")
            return target.parent.GetComponent<RectTransform>();
        return target;
    }

    ////////////////////
    // Children stuff //
    ////////////////////

    public string name {
        get { return img.name; }
    }

    public int childIndex {
        get { return img.transform.GetSiblingIndex() + 1; }
        set { img.transform.SetSiblingIndex(value - 1); }
    }
    public int childCount {
        get { return img.transform.childCount; }
    }

    public DynValue GetParent() {
        return UnitaleUtil.GetObjectParent(img.transform);
    }

    public void SetParent(object parent) {
        UnitaleUtil.SetObjectParent(this, parent);
        LuaSpriteController sParent = parent as LuaSpriteController;
        ProjectileController pParent = parent as ProjectileController;
        if (pParent != null)
            sParent = pParent.sprite;
        if (sParent == null)
            return;
        if (img.GetComponent<MaskImage>())
            img.GetComponent<MaskImage>().inverted = sParent._masked == MaskMode.INVERTEDSPRITE || sParent._masked == MaskMode.INVERTEDSTENCIL;
    }

    public DynValue GetChild(int index) {
        if (index > childCount)
            throw new CYFException("This object only has " + childCount + " children yet you try to get its child #" + index);
        return UnitaleUtil.GetObject(img.transform.GetChild(--index));
    }

    public DynValue[] GetChildren() {
        DynValue[] tab = new DynValue[img.transform.childCount];
        for (int i = 0; i < img.transform.childCount; i++)
            tab[i] = GetChild(i + 1);
        return tab;
    }
}
