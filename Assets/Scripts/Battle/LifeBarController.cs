using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for all the lifebars in the game. To be used with the HPBar prefab.
/// Contains a background sprite, a mask sprite, and a fill sprite, parented in that order.
/// Sprites can be used instead of the standard "px" sprite, however the sprites for the
/// background and fill must have the same size.
/// </summary>
public class LifeBarController : MonoBehaviour {
    public LuaSpriteController background, mask, fill, outline;
    [MoonSharpHidden] public RectTransform backgroundRt, maskRt, fillRt, outlineRt;

    public float currentFill { get; private set; }
    private float oldFill = 1.0f;
    private float desiredFill = 1.0f;
    private float fillLinearTime = 1.0f; // how many seconds does it take to go from current healthbar position to new healthbar position
    private float fillTimer;
    private bool init;
    private bool needInstant;

    public bool inLerp { get; private set; }

    public bool hasOutline {
        get { return outline != null; }
    }

    private int _outlineThickness;
    public int outlineThickness {
        get { return _outlineThickness; }
        set {
            if (outline == null)
                return;
            _outlineThickness = value;
            Resize(background.xscale, background.yscale);
        }
    }

    public bool isactive {
        get { return background.isactive; }
    }

    /// <summary>
    /// Creates a bar object using its position and size.
    /// </summary>
    /// <param name="x">Absolute X position of the bottom left corner of the bar.</param>
    /// <param name="y">Absolute Y position of the bottom left corner of the bar.</param>
    /// <param name="width">Width of the bar object, in pixels.</param>
    /// <param name="height">Height of the bar object, in pixels.</param>
    /// <returns></returns>
    public static LifeBarController Create(float x, float y, float width, float height = 20) {
        LifeBarController lifebar = Instantiate(Resources.Load<LifeBarController>("Prefabs/HPBar"));
        lifebar.Start();
        lifebar.background.MoveToAbs(x, y);
        lifebar.Resize(width, height);
        return lifebar;
    }
    /// <summary>
    /// Creates a bar object using its position and either one sprite applied to both sprite objects, or one sprite for each sprite object.
    /// </summary>
    /// <param name="x">Absolute X position of the bottom left corner of the bar.</param>
    /// <param name="y">Absolute Y position of the bottom left corner of the bar.</param>
    /// <param name="backgroundSprite">Path to the sprite used for the bar object's background.</param>
    /// <param name="fillSprite">Path to the sprite used for the bar object's fill. Will use backgroundsprite if not given.</param>
    /// <returns></returns>
    public static LifeBarController Create(float x, float y, string backgroundSprite, string fillSprite = null) {
        LifeBarController lifebar = Instantiate(Resources.Load<LifeBarController>("Prefabs/HPBar"));
        lifebar.Start();
        lifebar.background.MoveToAbs(x, y);
        lifebar.SetSprites(backgroundSprite, fillSprite);
        return lifebar;
    }

    /// <summary>
    /// Initializes needed variables as well as the bar object's size.
    /// </summary>
    [MoonSharpHidden] public void Start() {
        if (init) return;
        backgroundRt = GetComponent<RectTransform>();
        maskRt = backgroundRt.GetChild(0).GetComponent<RectTransform>();
        fillRt = maskRt.GetChild(0).GetComponent<RectTransform>();
        background = LuaSpriteController.GetOrCreate(gameObject);
        mask = LuaSpriteController.GetOrCreate(maskRt.gameObject);
        fill = LuaSpriteController.GetOrCreate(fillRt.gameObject);

        if (!backgroundRt.parent)
            background.layer = "BelowArena";

        float width = backgroundRt.sizeDelta.x, height = backgroundRt.sizeDelta.y;
        background.Set("bar-px");
        mask.Set("bar-px");
        fill.Set("bar-px");
        Resize(width, height);

        mask.Mask("stencil");

        currentFill = 1;

        init = true;
    }

    /// <summary>
    /// Set the healthbar's fill to this value on the same frame.
    /// </summary>
    /// <param name="fillValue">Healthbar fill in range of [0.0, 1.0].</param>
    /// <param name="allowNonClamped">True if the value can go outside of bounds.</param>
    public void SetInstant(float fillValue, bool allowNonClamped = false) {
        if (!allowNonClamped) fillValue = Mathf.Clamp01(fillValue);
        desiredFill = fillValue;
        fillTimer = fillLinearTime;
        inLerp = false;
        needInstant = true;
    }

    /// <summary>
    /// Start a linear-time transition from first value to second value.
    /// </summary>
    /// <param name="originalValue">Value to start the healthbar at, in range of [0.0, 1.0].</param>
    /// <param name="fillValue">Value the healthbar should be at when finished, in range of [0.0, 1.0].</param>
    /// <param name="time">Time for the healthbar to reach its destination in frames.</param>
    /// <param name="allowNonClamped">True if values outside of the range [0.0, 1.0] should be kept.</param>
    public void SetLerpFull(float originalValue, float fillValue, int time = 60, bool allowNonClamped = false) {
        fillLinearTime = time / 60f;

        if (!allowNonClamped) {
            fillValue = Mathf.Clamp01(fillValue);
            originalValue = Mathf.Clamp01(originalValue);
        }
        SetInstant(originalValue);
        currentFill = originalValue;
        oldFill = currentFill;
        desiredFill = fillValue;
        fillTimer = 0.0f;

        inLerp = true;
    }

    /// <summary>
    /// Start a linear-time transition from the current value to a given value.
    /// </summary>
    /// <param name="fillValue">Value the healthbar should be at when finished, in range of [0.0, 1.0].</param>
    /// <param name="time">Time for the healthbar to reach its destination in frames.</param>
    /// <param name="allowNonClamped">True if values outside of the range [0.0, 1.0] should be kept.</param>
    public void SetLerp(float fillValue, int time = 60, bool allowNonClamped = false) {
        SetLerpFull(desiredFill, fillValue, time, allowNonClamped);
    }

    /// <summary>
    /// Adds an outline with a given color to the bar object.
    /// Don't forget to move the outline from now on, not the background!
    /// </summary>
    /// <param name="thickness">Thickness of the outline, in pixels.</param>
    /// <param name="r">Red color of the outline.</param>
    /// <param name="g">Green color of the outline.</param>
    /// <param name="b">Blue color of the outline.</param>
    public void AddOutline(int thickness, float r = 0, float g = 0, float b = 0) {
        if (!isactive) return;
        if (outlineRt) RemoveOutline();

        outline = (LuaSpriteController)SpriteUtil.MakeIngameSprite("bar-px", -1).UserData.Object;
        outlineRt = outline.spr.GetComponent<RectTransform>();
        outlineRt.gameObject.name = "HPBarOutline";

        outline.Scale(backgroundRt.sizeDelta.x + 2 * thickness, backgroundRt.sizeDelta.y + 2 * thickness);
        outline.SetPivot(0, 0);
        outline.SetAnchor(0, 0);
        outline.MoveToAbs(background.absx - thickness, background.absy - thickness);
        outline.color = new[] { r, g, b };

        outlineRt.SetParent(backgroundRt.parent);
        outlineRt.SetSiblingIndex(outlineRt.GetSiblingIndex());
        background.SetParent(outline);

        outlineThickness = thickness;
    }

    /// <summary>
    /// Removes the bar object's outline if it has one.
    /// </summary>
    public void RemoveOutline() {
        if (!isactive) return;
        if (!outlineRt) return;
        backgroundRt.SetParent(outlineRt.parent);
        backgroundRt.SetSiblingIndex(outlineRt.GetSiblingIndex());
        Destroy(outlineRt.gameObject);
        outline = null;
        outlineRt = null;
        outlineThickness = 0;
    }

    /// <summary>
    /// Scales all elements of the bar using the given x and y scale.
    /// </summary>
    /// <param name="width">New x scale of the bar object.</param>
    /// <param name="height">New y scale of the bar object.</param>
    /// <param name="updateOutline">True of the outline should be resized as well.</param>
    public void Resize(float width, float height, bool updateOutline = true) {
        if (!isactive) return;
        // Update the position and size of the outline
        if (outlineRt && updateOutline) {
            outline.Scale((width + outlineThickness * 2) * background.width / outline.width, (height + outlineThickness * 2) * background.height / outline.height);
            Vector2 oldPos = new Vector2(background.absx, background.absy);
            background.MoveTo(outlineThickness, outlineThickness);
            outline.Move(oldPos.x - background.absx, oldPos.y - background.absy);
        }
        background.Scale(width, height);
        mask.Scale(currentFill * width * background.width / mask.width, height * background.height / mask.height);
        fill.Scale(width, height);
        if (!inLerp)
            SetInstant(currentFill, true);
    }

    /// <summary>
    /// Sets the sprites for all elements of the bar object, and resizes all elemetns accordingly.
    /// </summary>
    /// <param name="bgSprite">New sprite for the bar object's background.</param>
    /// <param name="fSprite">New sprite for the bar object's fill. Will copy bgSprite if null or empty.</param>
    /// <param name="mSprite">New sprite for the bar object's mask. Will do nothing if null or empty.</param>
    /// <param name="oSprite">New sprite for the bar object's background. Will do nothing if there's no outline, or if null or empty.</param>
    public void SetSprites(string bgSprite, string fSprite = null, string mSprite = null, string oSprite = null) {
        if (!isactive) return;
        background.Set(bgSprite);
        fill.Set(string.IsNullOrEmpty(fSprite) ? bgSprite : fSprite);
        if (!string.IsNullOrEmpty(mSprite)) mask.Set(mSprite);
        if (hasOutline && !string.IsNullOrEmpty(oSprite)) outline.Set(oSprite);
        Resize(1, 1);
    }

    /// <summary>
    /// Set the fill color of this healthbar.
    /// </summary>
    /// <param name="c">Color for present health.</param>
    [MoonSharpHidden] public void SetFillColor(Color c) {
        if (!isactive) return;
        fill.color = new[] { c.r, c.g, c.b, c.a };
    }

    /// <summary>
    /// Set the background color of this healthbar.
    /// </summary>
    /// <param name="c">Color for missing health.</param>
    [MoonSharpHidden] public void SetBackgroundColor(Color c) {
        if (!isactive) return;
        background.color = new[] { c.r, c.g, c.b, c.a };
    }

    /// <summary>
    /// Sets visibility for the image components of the healthbar.
    /// </summary>
    /// <param name="visible">True for visible, false for hidden.</param>
    public void SetVisible(bool visible) {
        if (!isactive) return;
        foreach (Image img in (outlineRt ?? backgroundRt).GetComponentsInChildren<Image>())
            img.enabled = visible;
    }

    /// <summary>
    /// Destroys this bar object's instance.
    /// </summary>
    public void Remove() {
        if (!isactive) return;
        if (this == UIStats.instance.lifebar) throw new CYFException("You can't remove the player's lifebar!");
        if (hasOutline) outline.Remove();
        background.Remove();
    }

    /// <summary>
    /// Takes care of moving the healthbar to its intended position.
    /// </summary>
    private void Update() {
        if (!isactive) return;
        if (!needInstant) {
            if (!inLerp)
                return;
            if (Mathf.Abs(currentFill - desiredFill) < 0.0001f || UIController.instance.frozenState != "PAUSE") {
                inLerp = false;
                return;
            }
        }

        currentFill = Mathf.Lerp(oldFill, desiredFill, fillTimer / fillLinearTime);
        mask.Scale(currentFill * backgroundRt.sizeDelta.x, backgroundRt.sizeDelta.y);
        if (background.spritename == "bar-px" && mask.spritename == "bar-px" && fill.spritename == "bar-px") {
            if (currentFill < 0 || currentFill > 1) fill.Scale(currentFill * backgroundRt.sizeDelta.x, backgroundRt.sizeDelta.y);
            else                                    fill.Scale(backgroundRt.sizeDelta.x, backgroundRt.sizeDelta.y);
        }

        fillTimer += Time.deltaTime;

        needInstant = false;
    }
}
