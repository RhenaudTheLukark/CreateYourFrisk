using UnityEngine;

public class LuaUIController {
    public static LuaSpriteController background {
        get {
            return LuaSpriteController.GetOrCreate(GameObject.Find("Background"));
        }
    }

    public static LuaSpriteController hpsprite {
        get {
            return LuaSpriteController.GetOrCreate(GameObject.Find("HPLabel"));
        }
    }

    public static LuaTextManager hptext {
        get {
            return GameObject.Find("HPText").GetComponent<LuaTextManager>();
        }
    }

    public static LuaTextManager namelv {
        get {
            return GameObject.Find("NameLv").GetComponent<LuaTextManager>();
        }
    }

    public void UpdateInfo() {
        UIStats.instance.setNamePosition();
    }

}
