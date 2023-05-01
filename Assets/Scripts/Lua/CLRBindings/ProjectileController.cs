using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lua binding to set and retrieve information for bullets in the game.
/// </summary>
public class ProjectileController {
    private Projectile p;
    private readonly LuaSpriteController spr;
    private readonly Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();
    private float lastX;
    private float lastY;
    private float lastAbsX;
    private float lastAbsY;
    public static bool globalPixelPerfectCollision;

    public ProjectileController(Projectile p) {
        this.p = p;
        spr = LuaSpriteController.GetOrCreate(p.gameObject, true);
        spr.Reset();
    }

    // The x position of the sprite, relative to the arena position and its anchor.
    public float x {
        get { return p == null ? lastX : p.self.anchoredPosition.x - ArenaManager.arenaCenter.x; }
        set {
            if (p != null)
                p.self.anchoredPosition = new Vector2(value + ArenaManager.arenaCenter.x, p.self.anchoredPosition.y);
        }
    }

    // The y position of the sprite, relative to the arena position and its anchor.
    public float y {
        get { return p == null ? lastY : p.self.anchoredPosition.y - ArenaManager.arenaCenter.y; }
        set {
            if (p != null)
                p.self.anchoredPosition = new Vector2(p.self.anchoredPosition.x, value + ArenaManager.arenaCenter.y);
        }
    }

    // The x position of the sprite, relative to the bottom left corner of the screen.
    public float absx {
        get { return p == null ? lastAbsX : p.self.position.x; }
        set {
            if (p != null)
                p.self.position = new Vector2(value, p.self.position.y);
        }
    }

    // The y position of the sprite, relative to the bottom left corner of the screen.
    public float absy {
        get { return p == null ? lastAbsY : p.self.position.y; }
        set {
            if (p != null)
                p.self.position = new Vector2(p.self.position.x, value);
        }
    }

    //Bullet.Duplicate() has been suspended because of some bugs. Maybe that I'll get on it later.
    /*private DynValue Duplicate(Transform parent, Transform current) {
        Transform[] children = UnitaleUtil.GetFirstChildren(current.gameObject.transform);
        int currentIndex = 1;
        Table table = new Table(null);
        if (current == p.transform)
            p.ctrl = this;
        //Bullet Replication
        GameObject go = GameObject.Instantiate<GameObject>(current.gameObject);
        go.transform.SetParent(parent);
        go.GetComponent<LuaProjectile>().owner = current.GetComponent<LuaProjectile>().owner;
        //Debug.Log(current.GetComponent<LuaProjectile>().ctrl);
        //go.GetComponent<LuaProjectile>().ctrl = Instantiate(current.GetComponent<LuaProjectile>().ctrl);
        //ProjectileController projectileController = new ProjectileController(go.GetComponent<Projectile>());
        //projectileController.active = current.GetComponent<LuaProjectile>().ctrl.active;
        //Debug.Log("isPersistent current " + current.GetComponent<LuaProjectile>().ctrl.isPersistent);
        //projectileController.isPersistent = current.GetComponent<LuaProjectile>().ctrl.isPersistent;
        //Debug.Log("isPersistent new " + projectileController.isPersistent);
        //projectileController.vars = current.GetComponent<LuaProjectile>().ctrl.vars;
        go.self.pivot = current.self.pivot;
        go.self.anchorMin = current.self.anchorMin;
        go.self.anchorMax = current.self.anchorMax;
        go.self.sizeDelta = current.self.sizeDelta;
        go.transform.localPosition = current.localPosition;
        go.transform.localRotation = current.localRotation;
        table.Set(currentIndex++, UserData.Create(go.GetComponent<LuaProjectile>().ctrl));
        //foreach (string key in vars.Keys)
        //    projectileController.SetVar(key, vars[key]);
        foreach (Transform tf in UnitaleUtil.GetFirstChildren(go.transform))
            Destroy(tf.gameObject);

        //Children Replication
        if (children.Length > 0)
            for (int i = 0; i < children.Length; i++) {
                DynValue tabledv = Duplicate(go.transform, children[i]);
                for (int j = 0; j < tabledv.Table.Length; j++)
                    table.Set(currentIndex++, tabledv.Table.Get(j + 1));
            }
        return DynValue.NewTable(table);
    }

    public DynValue Duplicate() {
        return Duplicate(p.transform.parent, p.transform);
    }*/

    public bool ppcollision {
        get {
            if (p == null)
                throw new CYFException("Attempted to get the collision mode of a removed bullet.");
            return p.isPP(); }
        set {
            if (p == null)
                throw new CYFException("Attempted to set the collision mode of a removed bullet.");
            if (!p.isPP() && value)
                p.texture = ((Texture2D)p.GetComponent<Image>().mainTexture).GetPixels32();
            p.ppcollision = value;
            p.ppchanged = true;
        }
    }

    public bool ppchanged {
        get {
            if (p == null)
                throw new CYFException("Attempted to get the value of bullet.ppchanged from a removed bullet.");
            return p.ppchanged;
        }
    }

    public bool isactive {
        get { return p != null; }
    }

    public bool isPersistent = false;

    public string layer {
        get { return spr.img.transform.parent.name == "BulletPool" ? "" : spr.img.transform.parent.name.Substring(0, spr.img.transform.parent.name.Length - 6); }
        set {
            Transform parent = spr.img.transform.parent;
            try {
                spr.img.transform.SetParent(GameObject.Find(value == "" ? "BulletPool" : value + "Bullet").transform);
            } catch { spr.img.transform.SetParent(parent); }
        }
    }

    public LuaSpriteController sprite {
        get { return spr; }
    }

    /*public void UpdatePosition() {
        x = p.self.anchoredPosition.x - ArenaManager.arenaCenter.x;
        y = p.self.anchoredPosition.y - ArenaManager.arenaCenter.y;
        absx = p.self.anchoredPosition.x;
        absy = p.self.anchoredPosition.y;
    }*/

    public void ResetCollisionSystem() {
        if (p == null)
            throw new CYFException("Attempted to reset the personal collision system of a removed bullet.");
        p.ppchanged = false;
        p.ppcollision = globalPixelPerfectCollision;
    }

    public void Remove() {
        if (!isactive) return;
        UnitaleUtil.RemoveChildren(p.gameObject);
        lastX = x;
        lastY = y;
        lastAbsX = absx;
        lastAbsY = absy;
        if (p.gameObject.GetComponent<KeyframeCollection>() != null)
            Object.Destroy(p.gameObject.GetComponent<KeyframeCollection>());
        p.gameObject.GetComponent<Mask>().enabled = false;
        p.gameObject.GetComponent<RectMask2D>().enabled = false;
        spr.StopAnimation();
        BulletPool.instance.Requeue(p);
        p = null;
    }

    public void Move(float newX, float newY) { MoveToAbs(absx + newX, absy + newY); }

    public void MoveTo(float newX, float newY) { MoveToAbs(ArenaManager.arenaCenter.x + newX, ArenaManager.arenaCenter.y + newY); }

    public void MoveToAbs(float newX, float newY) {
        if (p == null) {
            if (GlobalControls.retroMode)
                return;
            throw new CYFException("Attempted to move a removed bullet. You can use a bullet's isactive property to check if it has been removed.");
        }

        if (GlobalControls.retroMode) p.self.anchoredPosition = new Vector2(newX, newY);
        else                          p.self.position = new Vector2(newX, newY);
    }

    public void SendToTop() { p.self.SetAsLastSibling(); }

    public void SendToBottom() { p.self.SetAsFirstSibling(); }

    private DynValue _OnHit = DynValue.Nil;
    public DynValue OnHit {
        get { return _OnHit; }
        set {
            if ((value.Type & (DataType.Nil | DataType.Function | DataType.ClrFunction)) == 0)
                throw new CYFException("bullet.OnHit: This variable has to be a function!");
            if (value.Type == DataType.Function && value.Function.OwnerScript != p.owner)
                throw new CYFException("bullet.OnHit: You can only use a function created in the same script as the projectile!");
            _OnHit = value;
        }
    }


    public void SetVar(string name, DynValue value) {
        if (name == null)
            throw new CYFException("bullet.SetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        vars[name] = value;
    }

    public DynValue GetVar(string name) {
        if (name == null)
            throw new CYFException("bullet.GetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        DynValue retval;
        return vars.TryGetValue(name, out retval) ? retval : null;
    }

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }

    public bool isColliding() {
        if (p == null)
            return false;
        return p.isPP() ? p.HitTestPP() : p.HitTest();
    }

    ////////////////////
    // Children stuff //
    ////////////////////

    public string name {
        get { return p.gameObject.name; }
    }

    public int childIndex {
        get { return p.self.GetSiblingIndex() + 1; }
        set { p.self.SetSiblingIndex(value - 1); }
    }
    public int childCount {
        get { return p.self.childCount; }
    }

    public DynValue GetParent() { return UnitaleUtil.GetObjectParent(p.self); }

    public void SetParent(object parent) {
        UnitaleUtil.SetObjectParent(this, parent);
        LuaSpriteController sParent = parent as LuaSpriteController;
        ProjectileController pParent = parent as ProjectileController;
        if (pParent != null)
            sParent = pParent.sprite;
        if (sParent == null)
            return;
        if (sprite.img.GetComponent<MaskImage>())
            sprite.img.GetComponent<MaskImage>().inverted = sParent._masked == LuaSpriteController.MaskMode.INVERTEDSPRITE || sParent._masked == LuaSpriteController.MaskMode.INVERTEDSTENCIL;
    }

    public DynValue GetChild(int index) {
        if (index > childCount)
            throw new CYFException("This object only has " + childCount + " children yet you try to get its child #" + index);
        return UnitaleUtil.GetObject(sprite.img.transform.GetChild(--index));
    }

    public DynValue[] GetChildren() {
        DynValue[] tab = new DynValue[sprite.img.transform.childCount];
        for (int i = 0; i < sprite.img.transform.childCount; i++)
            tab[i] = GetChild(i + 1);
        return tab;
    }
}