using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lua binding to set and retrieve information for bullets in the game.
/// </summary>
public class ProjectileController {
    private Projectile p;
    private LuaSpriteController spr;
    private Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();
    private float lastX = 0;
    private float lastY = 0;
    private float lastAbsX = 0;
    private float lastAbsY = 0;

    public ProjectileController(Projectile p) {
        this.p = p;
        spr = new LuaSpriteController(p.GetComponent<Image>());
    }

    // The x position of the sprite, relative to the arena position and its anchor.
    public float x {
        get {
            if (p == null)
                return lastX;
            else
                return p.self.anchoredPosition.x - ArenaManager.arenaCenter.x;
        }
        set {
            if (p != null)
                p.self.anchoredPosition = new Vector2(value + ArenaManager.arenaCenter.x, p.self.anchoredPosition.y);
        }
    }

    // The y position of the sprite, relative to the arena position and its anchor.
    public float y {
        get {
            if (p == null)
                return lastY;
            else
                return p.self.anchoredPosition.y - ArenaManager.arenaCenter.y;
        }
        set {
            if (p != null)
                p.self.anchoredPosition = new Vector2(p.self.anchoredPosition.x, value + ArenaManager.arenaCenter.y);
        }
    }

    // The x position of the sprite, relative to the bottom left corner of the screen.
    public float absx {
        get {
            if (p == null)
                return lastAbsX;
            else
                return p.self.position.x;
        }
        set {
            if (p != null)
                p.self.position = new Vector2(value, p.self.position.y);
        }
    }

    // The y position of the sprite, relative to the bottom left corner of the screen.
    public float absy {
        get {
            if (p == null)
                return lastAbsY;
            else
                return p.self.position.y;
        }
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
        get {
            if (spr.img.transform.parent.name == "BulletPool")
                return "";
            else
                return spr.img.transform.parent.name.Substring(0, spr.img.transform.parent.name.Length - 6);
        }
        set {
            Transform parent = spr.img.transform.parent;
            try {
                if (value == "")
                    spr.img.transform.SetParent(GameObject.Find("BulletPool").transform);
                else
                    spr.img.transform.SetParent(GameObject.Find(value + "Bullet").transform);
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
        p.ppcollision = GlobalControls.ppcollision;
    }

    public void Remove() {
        if (isactive) {
            Transform[] pcs = UnitaleUtil.GetFirstChildren(p.transform);
            for (int i = 1; i < pcs.Length; i++)
                try { pcs[i].GetComponent<Projectile>().ctrl.Remove(); }
                catch { new LuaSpriteController(pcs[i].GetComponent<Image>()).Remove(); }
            lastX = x;
            lastY = y;
            lastAbsX = absx;
            lastAbsY = absy;
            if (p.gameObject.GetComponent<KeyframeCollection>() != null)
                GameObject.Destroy(p.gameObject.GetComponent<KeyframeCollection>());
            p.gameObject.GetComponent<Mask>().enabled = false;
            p.gameObject.GetComponent<RectMask2D>().enabled = false;
            spr.StopAnimation();
            BulletPool.instance.Requeue(p);
            p = null;
        }
    }

    public void Move(float x, float y) { MoveToAbs(this.absx + x, this.absy + y); }

    public void MoveTo(float x, float y) { MoveToAbs(ArenaManager.arenaCenter.x + x, ArenaManager.arenaCenter.y + y); }

    public void MoveToAbs(float x, float y) {
        if (p == null) {
            if (GlobalControls.retroMode)
                return;
            else
                throw new CYFException("Attempted to move a removed bullet. You can use a bullet's isactive property to check if it has been removed.");
        }

        if (GlobalControls.retroMode)
            p.self.anchoredPosition = new Vector2(x, y);
        else
            p.self.position = new Vector2(x, y);
    }

    public void SendToTop() { p.self.SetAsLastSibling(); }

    public void SendToBottom() { p.self.SetAsFirstSibling(); }

    public void SetVar(string name, DynValue value) {
        if (name == null)
            throw new CYFException("bullet.SetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        vars[name] = value;
    }

    public DynValue GetVar(string name) {
        if (name == null)
            throw new CYFException("bullet.GetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        DynValue retval;
        if (vars.TryGetValue(name, out retval)) return retval;
        else                                    return null;
    }

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }

    public bool isColliding() {
        if (p == null)
            return false;
        if (p.isPP())  return p.HitTestPP();
        else           return p.HitTest();
    }
}