using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaPlayerOverworld {
    public int Level {
        get { return PlayerCharacter.instance.LV; }
        set { PlayerCharacter.instance.SetLevel(value); }
    }
}
