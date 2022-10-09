using MoonSharp.Interpreter;
using UnityEngine.UI;

public class LuaProjectile : Projectile {
    public override void OnStart() {
        self.sizeDelta = GetComponent<Image>().sprite.rect.size;
        ctrl.sprite.nativeSizeDelta = self.sizeDelta;
        if (!this.isPP() || GlobalControls.retroMode) {
            selfAbs.width = self.rect.width;
            selfAbs.height = self.rect.height;
        }
        GetComponent<Image>().enabled = true;
    }

    public void setSprite(string newSprite) {
        if (newSprite == null)
            throw new CYFException("You can't set a projectile's sprite to nil!");
        SpriteUtil.SwapSpriteFromFile(this, newSprite);
        if (!UnitaleUtil.IsOverworld) name = newSprite;
    }

    //public override void OnUpdate() {
        // destroy projectiles outside of the screen
        /*if (!screen.Contains(self.position))
            BulletPool.instance.Requeue(this);*/
    //}

    public override void OnProjectileHit() {
        if (ctrl.OnHit.Type != DataType.Nil)
            try { owner.Call(ctrl.OnHit, ctrl); }
            catch (ScriptRuntimeException ex) {
                UnitaleUtil.DisplayLuaError((owner.Globals["wavename"] != null) ? (string) owner.Globals["wavename"] : "[wave script filename here]\n(should be a filename, sorry! missing feature)", UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message, ex.DoNotDecorateMessage);
            }
        else if (owner.Globals["OnHit"] != null && owner.Globals.Get("OnHit") != null)
            try { owner.Call(owner.Globals["OnHit"], ctrl); }
            catch (ScriptRuntimeException ex) {
                UnitaleUtil.DisplayLuaError((owner.Globals["wavename"] != null) ? (string)owner.Globals["wavename"] : "[wave script filename here]\n(should be a filename, sorry! missing feature)", UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message, ex.DoNotDecorateMessage);
            }
        else
            PlayerController.instance.Hurt();
    }
}