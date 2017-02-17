using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

// The base projectile class. All projectiles, including new/combined types, should inherit from this.
public abstract class Projectile : MonoBehaviour {
    /*
     * Commented out because Z indices don't really work yet because the Unity 5 UI likes to work differently, despite operating in world space.
     * 
    public const float Z_INDEX_INITIAL = 30.0f; //Z index the projectiles start spawning at, reset after every wave
    public static float Z_INDEX_NEXT { //Used to set the initial Z position for projectiles when they're created.
        get {
            zIndexCurrent -= 0.001f;
            return zIndexCurrent;
        } 
        set { zIndexCurrent = value; }
    }
    private static float zIndexCurrent = Z_INDEX_INITIAL;
     */

    internal Script owner; //TODO convert to ScriptWrapper, observe performance influence
    protected internal RectTransform self; // RectTransform of this projectile
    protected internal ProjectileController ctrl;
    protected internal Color32[] texture;
    private Image img;
    public bool needUpdateTex = true;
    public Rect selfAbs; // Rectangle containing position and size of this projectile

    private bool currentlyVisible = true; // Used to keep track of whether this object is currently visible to potentially save time in SetRenderingActive().
    //private bool Collision = false;
    
    public bool ppcollision = false;
    public bool ppchanged = false;

    /// <summary>
    /// Built-in Unity function run for initialization
    /// </summary>
    private void Awake() {
        self = GetComponent<RectTransform>();
        ctrl = new ProjectileController(this);
    }

    /// <summary>
    /// Built-in Unity function run on enabling this object
    /// </summary>
    private void OnEnable() {
        self = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        img.color = Color.white;
        img.rectTransform.eulerAngles = Vector3.zero;
        Vector2 half = new Vector2(0.5f, 0.5f);
        img.rectTransform.anchorMax = half;
        img.rectTransform.anchorMin = half;
        img.rectTransform.pivot = half;
        self.sizeDelta = img.sprite.rect.size;
        //selfAbs = new Rect(self.anchoredPosition.x - self.rect.width / 2, self.anchoredPosition.y - self.rect.height / 2, self.sizeDelta.x, self.sizeDelta.y);
        ppchanged = false;
        ppcollision = GlobalControls.ppcollision;
        OnStart();
        if (PlayerController.instance.texture == null)
            PlayerController.instance.texture = ((Texture2D)PlayerController.instance.selfImg.mainTexture).GetPixels32();
    }

    /// <summary>
    /// Renew the attached Projectile Controller. This is done whenever this projectile is dequeued from the bullet pool.
    /// </summary>
    public void renewController() { ctrl = new ProjectileController(this); }

    public bool isPP() { return (ppcollision && ppchanged) || (GlobalControls.ppcollision && !ppchanged); }

    /// <summary>
    /// Built-in Unity function run on every frame
    /// </summary>
    private void Update() {
        //selfAbs = new Rect(self.anchoredPosition.x - self.rect.width / 2, self.anchoredPosition.y - self.rect.height / 2, self.sizeDelta.x, self.sizeDelta.y);
        ctrl.UpdatePosition();
        //OnUpdate();
        UpdateHitRect();
        if (HitTest())
            if (isPP()) {
                if (HitTestPP())
                    OnProjectileHit();
            } else
                OnProjectileHit();
    }

    /// <summary>
    /// Overrideable start function to set projectile-specific settings.
    /// </summary>
    public virtual void OnStart() { }

    /// <summary>
    /// Overrideable update function to execute on every frame.
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Overrideable method that executes when player is hit. Usually, this calls Hurt() on the player in some way.
    /// </summary>
    public virtual void OnProjectileHit() { PlayerController.instance.Hurt(); }

    /// <summary>
    /// Updates the projectile's hitbox.
    /// </summary>
    public virtual void UpdateHitRect() {
        if (ppcollision && ppchanged || GlobalControls.ppcollision && !ppchanged) {
            float cst = ctrl.sprite.rotation * Mathf.Deg2Rad,
                  realX = self.sizeDelta.x * Mathf.Abs(Mathf.Cos(cst)) + self.sizeDelta.y * Mathf.Abs(Mathf.Sin(cst)),
                  realY = self.sizeDelta.y * Mathf.Abs(Mathf.Cos(cst)) + self.sizeDelta.x * Mathf.Abs(Mathf.Sin(cst));
            selfAbs.width = Mathf.FloorToInt(realX) + 2;
            selfAbs.height = Mathf.FloorToInt(realY) + 2;
        } else {
            selfAbs.width = self.sizeDelta.x;
            selfAbs.height = self.sizeDelta.y;
        }
        float rot = -(self.eulerAngles.z + 90) * Mathf.Deg2Rad,
              Px = (0.5f - self.pivot.x) * selfAbs.width, 
              Py = (0.5f - self.pivot.y) * selfAbs.height,
              Centerx = Px * Mathf.Sin(rot) + Py * Mathf.Cos(rot), 
              Centery = Px * Mathf.Cos(rot) - Py * Mathf.Sin(rot);
        selfAbs.x = self.position.x + Centerx - selfAbs.width  / 2;
        selfAbs.y = self.position.y + Centery - selfAbs.height / 2;
        //selfAbs.width = maxDistance.x*2;
        //selfAbs.height = maxDistance.y*2;
    }

    /// <summary>
    /// Return the rectangle surrounding this projectile.
    /// </summary>
    /// <returns>The rectangle surrounding this projectile.</returns>
    public Rect getRekt() { return selfAbs; }

    /// <summary>
    /// Enable or disable rendering of this projectile and its children.
    /// </summary>
    /// <param name="active">true to enable rendering, false to disable</param>
    protected void SetRenderingActive(bool active) {
        // dont cycle through children if they arent changing state anyway
        if (currentlyVisible == active)
            return;
        Image selfImg = GetComponent<Image>();
        if (selfImg != null)
            selfImg.enabled = active;
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
            image.enabled = active;
        currentlyVisible = active;
    }

    /// <summary>
    /// Overrideable method run on every frame that should update the hitbox and return true if this projectile is hitting the player.
    /// </summary>
    /// <returns>true if there's a collision, otherwise false</returns>
    public virtual bool HitTest() {
        return selfAbs.Overlaps(PlayerController.instance.playerAbs);
    }

    /// <summary>
    /// Function that replaces the old Sprite Collision system by a Pixel-Perfect Collision system.
    /// </summary>
    /// <returns>true if there's a collision, otherwise false</returns>
    public virtual bool HitTestPP() {
        if (selfAbs.Overlaps(PlayerController.instance.playerAbs)) {
            if (needUpdateTex) {
                texture = ((Texture2D)GetComponent<Image>().mainTexture).GetPixels32();
                needUpdateTex = false;
            }

            /*Rect rectProjectile = new Rect(new Vector2(selfAbs.x + selfAbs.width * (self.anchorMax.x - 0.5f), selfAbs.y + selfAbs.height * (self.anchorMax.y - 0.5f)),
                                             new Vector2(selfAbs.width, selfAbs.height));*/

            Color32[] tempPlayerHitbox = new Color32[Mathf.RoundToInt(PlayerController.instance.playerAbs.width) * Mathf.RoundToInt(PlayerController.instance.playerAbs.height)];
            for (int i = 0; i < tempPlayerHitbox.Length; i++)
                tempPlayerHitbox[i].a = 255;

            Vector2 positionPlayerFromProjectile = (Vector2)PlayerController.instance.self.position - selfAbs.position - (selfAbs.size + PlayerController.instance.playerAbs.size) / 2;
            return UnitaleUtil.TestPP(tempPlayerHitbox, texture, ctrl.sprite.rotation, Mathf.RoundToInt(PlayerController.instance.playerAbs.height), 
                                      GetComponent<Image>().mainTexture.height, new Vector2(ctrl.sprite.xscale, ctrl.sprite.yscale), positionPlayerFromProjectile);
            //Color32[] colors = UnitaleUtil.RotateMatrixOld(texture, ctrl.sprite.rotation, (int)GetComponent<Image>().sprite.rect.height, self.localScale, out sizeDelta);
            /*Texture2D tex = new Texture2D((int)sizeDelta.x, (int)sizeDelta.y);
            tex.SetPixels32(colors);
            tex.Apply(false);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/SavedScreen" + count++ +".png", bytes);*/
        }
        return false;
    }
    
    IEnumerator IOldCheckCollision() {
        Rect rectPlayer = PlayerController.instance.playerAbs;
        Texture2D tex = new Texture2D(Mathf.FloorToInt(rectPlayer.width) - 2, Mathf.FloorToInt(rectPlayer.height) - 2, TextureFormat.RGB24, false);
        Color colorPlayer = GameObject.Find("player").GetComponent<Image>().color;
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(new Rect(rectPlayer.x + rectPlayer.width / 2 + 2, rectPlayer.y + rectPlayer.height / 2 - 1, tex.width, tex.height), 0, 0);
        Color[] pixs = tex.GetPixels();
        for (int i = 0; i < pixs.Length; i++)
            if (pixs[i] != colorPlayer && i != 0) 
                yield break;
    }

    Rect Intersect(Rect r1, Rect r2) {
        Vector2 bottomLeft = new Vector2(Mathf.Max(r1.xMin, r2.xMin), Mathf.Max(r1.yMin, r2.yMin)),
                topRight   = new Vector2(Mathf.Min(r1.xMax, r2.xMax), Mathf.Min(r1.yMax, r2.yMax));
        return new Rect(bottomLeft.x, bottomLeft.y, Mathf.Ceil(topRight.x - bottomLeft.x), Mathf.Ceil(topRight.y - bottomLeft.y));
    }
}