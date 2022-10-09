using MoonSharp.Interpreter;
using UnityEngine;

public class LuaCYFObject {
    public Transform transform;

    public string name {
        get { return transform.gameObject.name; }
    }

    public int childIndex {
        get { return transform.GetSiblingIndex() + 1; }
        set { transform.SetSiblingIndex(value - 1); }
    }
    public int childCount {
        get { return transform.childCount; }
    }

    public LuaCYFObject(Transform t) {
        transform = t;
    }

    public DynValue GetParent() {
        return UnitaleUtil.GetObjectParent(transform);
    }

    public void SetParent(object p) {
        UnitaleUtil.SetObjectParent(transform, p);
    }

    public DynValue GetChild(int index) {
        if (index > childCount)
            throw new CYFException("This object only has " + childCount + " children yet you try to get its child #" + index);
        return UnitaleUtil.GetObject(transform.GetChild(--index));
    }

    public DynValue[] GetChildren() {
        DynValue[] tab = new DynValue[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            tab[i] = GetChild(i + 1);
        return tab;
    }
}
