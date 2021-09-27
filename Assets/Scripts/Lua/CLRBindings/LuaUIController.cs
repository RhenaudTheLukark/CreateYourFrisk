using UnityEngine;

public class LuaUIController {
    public static LuaSpriteController background
    {
        get
        {
            return LuaSpriteController.GetOrCreate(GameObject.Find("Background"));
        }
    }
    public static LuaSpriteController hpsprite
    {
        get
        {
            return LuaSpriteController.GetOrCreate(GameObject.Find("HPLabel"));
        }
    }
}
