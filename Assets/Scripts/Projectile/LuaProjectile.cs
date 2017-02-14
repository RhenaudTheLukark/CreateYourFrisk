using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class LuaProjectile : Projectile {    
    public override void OnStart() {
        self.sizeDelta = GetComponent<Image>().sprite.rect.size;
        ctrl.sprite.nativeSizeDelta = self.sizeDelta;
        selfAbs.width = self.rect.width;
        selfAbs.height = self.rect.height;
        GetComponent<Image>().enabled = true;
    }

    public void setSprite(string name) { SpriteUtil.SwapSpriteFromFile(this, name); }

    //public override void OnUpdate() {
        // destroy projectiles outside of the screen
        /*if (!screen.Contains(self.position))
            BulletPool.instance.Requeue(this);*/
    //}

    public override void OnProjectileHit() {
        if (owner.Globals["OnHit"] != null && owner.Globals.Get("OnHit") != null)
            try { owner.Call(owner.Globals["OnHit"], this.ctrl); }
            catch (ScriptRuntimeException ex) { UnitaleUtil.displayLuaError("[wave script filename here]\n(should be a filename, sorry! missing feature)", ex.DecoratedMessage); }
        else
            PlayerController.instance.Hurt(3);
    }

    // Sets the parent of a bullet. Can't be used on an enemy or projectile
    public void SetParent(LuaProjectile parent) {
        transform.SetParent(parent.transform);
    }
}