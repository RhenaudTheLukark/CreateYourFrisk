using UnityEngine;

/// <summary>
/// Class used to mark sprite objects. Holds its controller and such.
/// </summary>
public class CYFSprite : MonoBehaviour {
    public LuaSpriteController ctrl;

    public void LateUpdate() {
        if (ctrl == null || !ctrl.limbo || ctrl.removed) return;
        UnitaleUtil.RemoveChildren(gameObject, true);
        ctrl.removed = true;
        ctrl.limbo = false;
        Destroy(gameObject);
    }
}
