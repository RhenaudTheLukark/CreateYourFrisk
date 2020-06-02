using UnityEngine;
using MoonSharp.Interpreter;

public class LuaPlayerOW {
    public ScriptWrapper appliedScript;

    public delegate void LoadedAction(string coroName, object args, string evName);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    [MoonSharpHidden] public LuaPlayerOW() { }

    [CYFEventFunction] public int GetLevel() { try { return PlayerCharacter.instance.LV; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetLevel(int value) { PlayerCharacter.instance.SetLevel(value); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public float GetHP() { try { return PlayerCharacter.instance.HP; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetHP(float value) { setHP(value); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public int GetMaxHP() { try { return PlayerCharacter.instance.BasisMaxHP + PlayerCharacter.instance.MaxHP; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetMaxHP(int value) { setMaxHP(value - PlayerCharacter.instance.BasisMaxHP); appliedScript.Call("CYFEventNextCommand"); }
    [CYFEventFunction] public void ResetMaxHP() { setMaxHP(PlayerCharacter.instance.BasisMaxHP); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public string GetName() { try { return PlayerCharacter.instance.Name; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetName(string value) { PlayerCharacter.instance.Name = value; appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public int GetWeaponATK() { try { return PlayerCharacter.instance.WeaponATK; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public int GetArmorDEF() { try { return PlayerCharacter.instance.ArmorDEF; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public int GetATK() { try { return PlayerCharacter.instance.ATK; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public int GetDEF() { try { return PlayerCharacter.instance.DEF; } finally { appliedScript.Call("CYFEventNextCommand"); } }

    [CYFEventFunction] public int GetGold() { try { return PlayerCharacter.instance.Gold; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetGold(int value) { PlayerCharacter.instance.SetGold(value); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public string GetWeapon() { try { return PlayerCharacter.instance.Weapon; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetWeapon(string value) { EventManager.instance.luainvow.SetEquip(value);}

    [CYFEventFunction] public string GetArmor() { try { return PlayerCharacter.instance.Armor; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetArmor(string value) { EventManager.instance.luainvow.SetEquip(value); }

    [CYFEventFunction] public int GetEXP() { try { return PlayerCharacter.instance.EXP; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetEXP(int value) { PlayerCharacter.instance.SetEXP(value, true); appliedScript.Call("CYFEventNextCommand"); }

    /*public int Level {
        get { return PlayerCharacter.instance.LV; }
        set { PlayerCharacter.instance.SetLevel(value); }
    }

    public float HP {
        get { return PlayerCharacter.instance.HP; }
        set { setHP(value); }
    }

    public int MaxHP {
        get { return PlayerCharacter.instance.MaxHP; }
        set { setMaxHP(value - PlayerCharacter.instance.BasisMaxHP); }
    }

    public string Name {
        get { return PlayerCharacter.instance.Name; }
        set { PlayerCharacter.instance.Name = value; }
    }

    public int Gold {
        get { return PlayerCharacter.instance.Gold; }
        set { PlayerCharacter.instance.SetGold(value); }
    }

    public string Weapon {
        get { return PlayerCharacter.instance.Weapon; }
        set { EventManager.instance.luainvow.SetWeapon(value); }
    }

    public string Armor {
        get { return PlayerCharacter.instance.Armor; }
        set { EventManager.instance.luainvow.SetArmor(value); }
    }

    public int EXP {
        get { return PlayerCharacter.instance.EXP; }
        set { PlayerCharacter.instance.SetEXP(value, true); }
    }*/

    [CYFEventFunction] public void ForceHP(float newhp) { setHP(newhp, true); appliedScript.Call("CYFEventNextCommand"); }

    /// <summary>
    /// Hurts the PlayerCharacter.instance with the given amount of damage. Heal()'s opposite.
    /// </summary>
    /// <param name="damage">This one seems obvious</param>
    [CYFEventFunction]
    public void Hurt(int damage) {
        if (damage >= 0) UnitaleUtil.PlaySound("HurtSound", AudioClipRegistry.GetSound("hurtsound"), 0.65f);
        else             UnitaleUtil.PlaySound("HurtSound", AudioClipRegistry.GetSound("healsound"), 0.65f);

        if (-damage + PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP) PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        else if (-damage + PlayerCharacter.instance.HP <= 0)                        PlayerCharacter.instance.HP = 1;
        else                                                                        PlayerCharacter.instance.HP -= damage;
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Heals the PlayerCharacter.instance with the given amount of heal. Hurt()'s opposite.
    /// </summary>
    /// <param name="heal">This one seems obvious too</param>
    [CYFEventFunction]
    public void Heal(int heal) { Hurt(-heal); appliedScript.Call("CYFEventNextCommand"); }

    /// <summary>
    /// Enables or disables the PlayerCharacter.instance's movement
    /// </summary>
    /// <param name="canMove">Can the PlayerCharacter.instance move?</param>
    [CYFEventFunction]
    public void CanMove(bool canMove) {
        PlayerOverworld.instance.forceNoAction = !canMove;
        appliedScript.Call("CYFEventNextCommand");
    }

    [MoonSharpHidden]
    public void setHP(float newhp, bool forced = false) {
        if (newhp <= 0) {
            EventManager.instance.luagenow.GameOver();
            return;
        }
        float CheckedHP = PlayerCharacter.instance.HP;
        if (CheckedHP - newhp >= 0) UnitaleUtil.PlaySound("CollisionSoundChannel", AudioClipRegistry.GetSound("hurtsound").name);
        else                        UnitaleUtil.PlaySound("CollisionSoundChannel", AudioClipRegistry.GetSound("healsound").name);

        newhp = Mathf.Round(newhp * Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma)) / Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma);

        if (forced) CheckedHP = newhp > PlayerCharacter.instance.MaxHP * 1.5 ? (int)(PlayerCharacter.instance.MaxHP * 1.5) : newhp;
        else        CheckedHP = newhp > PlayerCharacter.instance.MaxHP       ? PlayerCharacter.instance.MaxHP              : newhp;

        if (CheckedHP > ControlPanel.instance.HPLimit)
            CheckedHP = ControlPanel.instance.HPLimit;

        PlayerCharacter.instance.HP = CheckedHP;
    }

    [MoonSharpHidden]
    public void setMaxHP(int value) {
        if (value == PlayerCharacter.instance.MaxHP)
            return;
        if (value <= 0) {
            setHP(0);
            return;
        }
        if (value > ControlPanel.instance.HPLimit)
            value = ControlPanel.instance.HPLimit;
        else if (PlayerCharacter.instance.HP > value)
            PlayerCharacter.instance.HP = value;
        else
            UnitaleUtil.PlaySound("CollisionSoundChannel", AudioClipRegistry.GetSound("healsound").name);
        PlayerCharacter.instance.MaxHPShift = value - PlayerCharacter.instance.BasisMaxHP;
    }

    [CYFEventFunction]
    public void Teleport(string mapName, float posX, float posY, int direction = 0, bool NoFadeIn = false, bool NoFadeOut = false) {
        TPHandler tp = GameObject.Instantiate<TPHandler>(Resources.Load<TPHandler>("Prefabs/TP On-the-fly"));
        tp.sceneName = mapName;
        tp.position = new Vector2(posX, posY);
        tp.direction = direction;
        tp.noFadeIn = NoFadeIn;
        tp.noFadeOut = NoFadeOut;
        tp.transform.position = PlayerOverworld.instance.gameObject.transform.position;
        EventManager.instance.EndEvent();
    }
}
