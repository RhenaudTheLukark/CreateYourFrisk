using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class LuaPlayerOW {
    private PlayerCharacter player = PlayerCharacter.instance;
    public ScriptWrapper appliedScript;

    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden]
    public static event LoadedAction StCoroutine;

    [MoonSharpHidden] public LuaPlayerOW() { }

    [CYFEventFunction] public int GetLevel() { try { return player.LV; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetLevel(int value) { player.SetLevel(value); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public float GetHP() { try { return player.HP; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetHP(float value) { setHP(value); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public int GetMaxHP() { try { return player.MaxHP; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetMaxHP(int value) { setMaxHP(value - player.BasisMaxHP); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public string GetName() { try { return player.Name; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetName(string value) { player.Name = value; appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public int GetWeaponATK() { try { return player.WeaponATK; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public int GetArmorDEF() { try { return player.ArmorDEF; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public int GetATK() { try { return player.ATK; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public int GetDEF() { try { return player.DEF; } finally { appliedScript.Call("CYFEventNextCommand"); } }

    [CYFEventFunction] public int GetGold() { try { return player.Gold; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetGold(int value) { player.SetGold(value); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public string GetWeapon() { try { return player.Weapon; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetWeapon(string value) { EventManager.instance.luainvow.setEquip(value);}

    [CYFEventFunction] public string GetArmor() { try { return player.Armor; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetArmor(string value) { EventManager.instance.luainvow.setEquip(value); }

    [CYFEventFunction] public int GetEXP() { try { return player.EXP; } finally { appliedScript.Call("CYFEventNextCommand"); } }
    [CYFEventFunction] public void SetEXP(int value) { player.SetEXP(value, true); appliedScript.Call("CYFEventNextCommand"); }

    /*public int Level {
        get { return player.LV; }
        set { player.SetLevel(value); }
    }

    public float HP {
        get { return player.HP; }
        set { setHP(value); }
    }

    public int MaxHP {
        get { return player.MaxHP; }
        set { setMaxHP(value - player.BasisMaxHP); }
    }

    public string Name {
        get { return player.Name; }
        set { player.Name = value; }
    }

    public int Gold {
        get { return player.Gold; }
        set { player.SetGold(value); }
    }

    public string Weapon {
        get { return player.Weapon; }
        set { EventManager.instance.luainvow.SetWeapon(value); }
    }

    public string Armor {
        get { return player.Armor; }
        set { EventManager.instance.luainvow.SetArmor(value); }
    }

    public int EXP {
        get { return player.EXP; }
        set { player.SetEXP(value, true); }
    }*/

    [CYFEventFunction] public void ForceHP(float newhp) { setHP(newhp, true); appliedScript.Call("CYFEventNextCommand"); }

    /// <summary>
    /// Hurts the player with the given amount of damage. Heal()'s opposite.
    /// </summary>
    /// <param name="damage">This one seems obvious</param>
    [CYFEventFunction]
    public void Hurt(int damage) {
        if (damage >= 0) UnitaleUtil.PlaySound("HurtSound", AudioClipRegistry.GetSound("hurtsound"), 0.65f);
        else             UnitaleUtil.PlaySound("HurtSound", AudioClipRegistry.GetSound("healsound"), 0.65f);

        if (-damage + player.HP > player.MaxHP) player.HP = player.MaxHP;
        else if (-damage + player.HP <= 0) player.HP = 1;
        else player.HP -= damage;
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Heals the player with the given amount of heal. Hurt()'s opposite.
    /// </summary>
    /// <param name="heal">This one seems obvious too</param>
    [CYFEventFunction]
    public void Heal(int heal) { Hurt(-heal); appliedScript.Call("CYFEventNextCommand"); }

    /// <summary>
    /// Enables or disables the player's movement
    /// </summary>
    /// <param name="heal">This one seems obvious too</param>
    [CYFEventFunction]
    public void CanMove(bool canMove) {
        PlayerOverworld.instance.forceNoAction = !canMove;
        appliedScript.Call("CYFEventNextCommand");
    }

    [MoonSharpHidden]
    public void setHP(float newhp, bool forced = false) {
        if (newhp <= 0) {
            GameOverBehavior gob = GameObject.FindObjectOfType<GameOverBehavior>();
            if (!MusicManager.isStoppedOrNull(PlayerOverworld.audioKept)) {
                gob.musicBefore = PlayerOverworld.audioKept;
                gob.music = gob.musicBefore.clip;
                gob.musicBefore.Stop();
            } else if (!MusicManager.isStoppedOrNull(Camera.main.GetComponent<AudioSource>())) {
                gob.musicBefore = Camera.main.GetComponent<AudioSource>();
                gob.music = gob.musicBefore.clip;
                gob.musicBefore.Stop();
            } else {
                gob.musicBefore = null;
                gob.music = null;
            }
            player.HP = 0;
            gob.gameObject.transform.SetParent(null);
            GameObject.DontDestroyOnLoad(gob.gameObject);
            RectTransform rt = gob.gameObject.GetComponent<RectTransform>();
            rt.position = new Vector3(rt.position.x, rt.position.y, -1000);
            gob.gameObject.GetComponent<GameOverBehavior>().StartDeath();
            return;
        }
        float CheckedHP = player.HP;
        if (CheckedHP - newhp >= 0) UnitaleUtil.PlaySound("CollisionSoundChannel", AudioClipRegistry.GetSound("hurtsound").name);
        else                 UnitaleUtil.PlaySound("CollisionSoundChannel", AudioClipRegistry.GetSound("healsound").name);

        newhp = Mathf.Round(newhp * Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma)) / Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma);

        if (forced) CheckedHP = newhp > player.MaxHP * 1.5 ? (int)(player.MaxHP * 1.5) : newhp;
        else        CheckedHP = newhp > player.MaxHP       ? player.MaxHP              : newhp;

        if (CheckedHP > ControlPanel.instance.HPLimit)
            CheckedHP = ControlPanel.instance.HPLimit;

        player.HP = CheckedHP;
    }

    [MoonSharpHidden]
    public void setMaxHP(int value) {
        if (value == player.MaxHP)
            return;
        if (value <= 0) {
            setHP(0);
            return;
        }
        if (value > ControlPanel.instance.HPLimit)
            value = ControlPanel.instance.HPLimit;
        else if (value < player.MaxHP) 
            player.HP -= (player.MaxHP - value);
        else
            UnitaleUtil.PlaySound("CollisionSoundChannel", AudioClipRegistry.GetSound("healsound").name);
        player.MaxHPShift = value - player.BasisMaxHP;
    }

    [CYFEventFunction]
    public void Teleport(string mapName, float posX, float posY, int direction = 0, bool NoFadeIn = false, bool NoFadeOut = false) {
        TPHandler tp = GameObject.Instantiate(Resources.Load<TPHandler>("Prefabs/TP On-the-fly"));
        tp.sceneName = mapName;
        tp.position = new Vector2(posX, posY);
        tp.direction = direction;
        tp.noFadeIn = NoFadeIn;
        tp.noFadeOut = NoFadeOut;
        tp.transform.position = PlayerOverworld.instance.gameObject.transform.position;
        EventManager.instance.EndEvent();
    }
}
