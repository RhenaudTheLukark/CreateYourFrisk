using UnityEngine.UI;
/// <summary>
/// Lua binding to set and retrieve information for the on-screen player.
/// </summary>
public class LuaPlayerStatus {
    /// <summary>
    /// This Lua controller's attached PlayerController.
    /// </summary>
    protected PlayerController player;

    /// <summary>
    /// The sprite controller for the player.
    /// </summary>
    private LuaSpriteController spr;

    /// <summary>
    /// Create a new Lua controller intended for this player.
    /// </summary>
    /// <param name="p">PlayerController this controller is intended for</param>
    public LuaPlayerStatus(PlayerController p) {
        player = p;
        spr = new LuaSpriteController(p.GetComponent<Image>());
    }

    /// <summary>
    /// Get player's X position relative to the arena's center.
    /// </summary>
    public float x {
        get { return player.self.anchoredPosition.x - ArenaManager.arenaCenter.x; }
    }

    /// <summary>
    /// Get player's Y position relative to the arena's center.
    /// </summary>
    public float y {
        get { return player.self.anchoredPosition.y - ArenaManager.arenaCenter.y; }
    }

    /// <summary>
    /// Get player's X position relative to the bottom left of the screen.
    /// </summary>
    public float absx {
        get { return player.self.anchoredPosition.x; }
    }

    /// <summary>
    /// Get player's Y position relative to the bottom left of the screen.
    /// </summary>
    public float absy {
        get { return player.self.anchoredPosition.y; }
    }

    /// <summary>
    /// Sprite controller for the player soul.
    /// </summary>
    public LuaSpriteController sprite {
        get { return spr; }
    }

    /// <summary>
    /// Get player's current HP.
    /// </summary>
    public float hp {
        get { return player.HP; }
        set { player.setHP(value); }
    }

    /// <summary>
    /// Player's Max HP.
    /// </summary>
    public int maxhp {
        get { return PlayerCharacter.instance.MaxHP; }
        set { player.setMaxHPShift(value, 0f, true, false, false); }
    }

    /// <summary>
    /// Player's Max HP shift.
    /// </summary>
    public int MaxHPShift {
        get { return PlayerCharacter.instance.MaxHPShift; }
    }
    public int maxhpshift {
        get { return MaxHPShift; }
    }

    /// <summary>
    /// Get player's current ATK.
    /// </summary>
    public int atk {
        set { PlayerCharacter.instance.ATK = value; }
        get { return PlayerCharacter.instance.ATK; }
    }

    /// <summary>
    /// Get player's current weapon.
    /// </summary>
    public string weapon {
        get { return PlayerCharacter.instance.Weapon; }
    }

    /// <summary>
    /// Get player's current weapon's atk.
    /// </summary>
    public int weaponatk {
        get { return PlayerCharacter.instance.WeaponATK; }
    }

    /// <summary>
    /// Get player's current DEF.
    /// </summary>
    public int def {
        set { PlayerCharacter.instance.DEF = value; }
        get { return PlayerCharacter.instance.DEF; }
    }

    /// <summary>
    /// Get player's current weapon.
    /// </summary>
    public string armor {
        get { return PlayerCharacter.instance.Armor; }
    }

    /// <summary>
    /// Get player's current armor's def.
    /// </summary>
    public int armordef {
        get { return PlayerCharacter.instance.ArmorDEF; }
    }

    /// <summary>
    /// Player character's name.
    /// </summary>
    public string name {
        get { return PlayerCharacter.instance.Name; }
        set {
            if (value == null)
                throw new CYFException("Player.name: Attempt to set the player's name to a nil value.\n\nPlease double-check your code.");

            PlayerCharacter.instance.Name = value;
            UIStats.instance.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
        }
    }

    /// <summary>
    /// Player character's level. Adjusts stats when set.
    /// </summary>
    public int lv {
        get { return PlayerCharacter.instance.LV; }
        set {
            if (PlayerCharacter.instance.LV != value) {
                if (PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP * 1.5 && PlayerCharacter.instance.LV > value)
                    player.setHP((int)(PlayerCharacter.instance.MaxHP * 1.5));
                PlayerCharacter.instance.SetLevel(value);
                UIStats.instance.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
                UIStats.instance.setMaxHP();
            }
        }
    }

    public int lastenemychosen {
        get { return player.lastEnemyChosen; }
    }

    public float lasthitmultiplier {
        get { return player.lastHitMult; }
    }

    /// <summary>
    /// True if player is currently blinking and invincible, false otherwise.
    /// </summary>
    public bool isHurting {
        get { return player.isHurting(); }
    }
    public bool ishurting {
        get { return isHurting; }
    }

    /// <summary>
    /// True if player is currently moving, false otherwise. Being pushed by the edges of the arena counts as moving.
    /// </summary>
    public bool isMoving {
        get { return player.isMoving(); }
    }
    public bool ismoving {
        get { return isMoving; }
    }

    /// <summary>
    /// Hurts the player with the given damage and invulnerabilty time. If this gets the player to 0 (or less) HP, you get the game over screen.
    /// </summary>
    /// <param name="damage">Damage to deal to the player</param>
    /// <param name="invulTime">Invulnerability time in seconds</param>
    /// <param name="ignoreDef">Will the damage ignore the player's defense?</param>
    public void Hurt(float damage, float invulTime = 1.7f, bool ignoreDef = false) { player.Hurt(damage, invulTime, ignoreDef); }

    /// <summary>
    /// Heals the player. Convenience method which is the same as hurting the player for -damage and no invulnerability time.
    /// </summary>
    /// <param name="heal">Value to heal the player for</param>
    public void Heal(float heal) { player.Hurt(-heal, 0.0f); }

    /// <summary>
    /// Override player control. Note: this will disable all movement checking on the player, making it ignore the arena walls.
    /// </summary>
    /// <param name="overrideControl"></param>
    public void SetControlOverride(bool overrideControl) {
        if (UIController.instance.GetState() == UIController.UIState.DEFENDING) player.setControlOverride(overrideControl);
    }

    /// <summary>
    /// Move the player relative to his current position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="ignoreWalls"></param>
    public void Move(float x, float y, bool ignoreWalls = false) { player.SetPosition(absx + x, absy + y, ignoreWalls); }

    /// <summary>
    /// Move the player relative to the arena center.
    /// </summary>
    /// <param name="x">X position of player relative to arena center.</param>
    /// <param name="y">Y position of player relative to arena center.</param>
    /// <param name="ignoreWalls">If false, it will place you at the edge of the arena instead of over it.</param>
    public void MoveTo(float x, float y, bool ignoreWalls = false) { MoveToAbs(ArenaManager.arenaCenter.x + x, ArenaManager.arenaCenter.y + y, ignoreWalls); }

    /// <summary>
    /// Move the player relative to the lower left corner of the screen.
    /// </summary>
    /// <param name="x">X position of player relative to the lower left of the screen.</param>
    /// <param name="y">Y position of player relative to the lower left of the screen.</param>
    /// <param name="ignoreWalls">If false, it will place you at the edge of the arena instead of over it.</param>
    public void MoveToAbs(float x, float y, bool ignoreWalls = false) { player.SetPosition(x, y, ignoreWalls); }

    /// <summary>
    /// Sets the player's HP above his HP Max. Maximum : 150% HP Max.
    /// </summary>
    public void ForceHP(float HP) { player.setHP(HP, false); }

    /// <summary>
    /// Sets a shift for the player's Max HP. Can be settable and can modify the player's HP.
    /// </summary>
    /// <param name="shift"></param>
    /// <param name="set"></param>
    /// <param name="canHeal"></param>
    public void SetMaxHPShift(int shift, float invulSec = 1.7f, bool set = false, bool canHeal = false, bool sound = true) { player.setMaxHPShift(shift, invulSec, set, canHeal, sound); }
    public void setMaxHPShift(int shift, float invulSec = 1.7f, bool set = false, bool canHeal = false, bool sound = true) { SetMaxHPShift(shift, invulSec, set, canHeal, sound); }

    /// <summary>
    /// Resets any of the player's Max HP, ATK and DEF to their default values, based on LV.
    /// </summary>
    /// <param name="resetMHP">If true, will reset Max HP.</param>
    /// <param name="resetATK">If true, will reset ATK.</param>
    /// <param name="resetDEF">If true, will reset DEF.</param>
    public void ResetStats(bool resetMHP = true, bool resetATK = true, bool resetDEF = true) {
        if (resetMHP) {
            PlayerCharacter.instance.MaxHPShift = 0;
            UIStats.instance.setMaxHP();
        }
        if (resetATK)
            atk = 8 + (2 * lv);
        if (resetDEF)
            def = 10 + (int)UnityEngine.Mathf.Floor((lv - 1) / 4);
    }

    public void SetAttackAnim(string[] anim, float frequency = 1 / 6f, string prefix = "") {
        if (anim.Length == 0) {
            UIController.instance.fightUI.sliceAnim = new string[] { "empty" };
            UIController.instance.fightUI.sliceAnimFrequency = 1 / 30f;
        } else {
            if (prefix != "") {
                while (prefix.StartsWith("/"))
                    prefix = prefix.Substring(1);

                if (!prefix.EndsWith("/"))
                    prefix += "/";

                for (int i = 0; i < anim.Length; i++)
                    anim[i] = prefix + anim[i];
            }

            UIController.instance.fightUI.sliceAnim = anim;
            UIController.instance.fightUI.sliceAnimFrequency = frequency;
        }
    }

    public void ResetAttackAnim() {
        UIController.instance.fightUI.sliceAnimFrequency = 1 / 6f;
        UIController.instance.fightUI.sliceAnim = new string[] {
            "UI/Battle/spr_slice_o_0",
            "UI/Battle/spr_slice_o_1",
            "UI/Battle/spr_slice_o_2",
            "UI/Battle/spr_slice_o_3",
            "UI/Battle/spr_slice_o_4",
            "UI/Battle/spr_slice_o_5"
        };
    }

    public void ChangeTarget(int index) {
        if (UIController.instance.state == UIController.UIState.ATTACKING)
            if (index <= UIController.instance.encounter.EnabledEnemies.Length && index > 0)
                UIController.instance.fightUI.ChangeTarget(UIController.instance.encounter.EnabledEnemies[index-1]);
            else
                UnitaleUtil.DisplayLuaError("Changing the target", "Enemy number " + index + " doesn't exist.");
    }

    public void ForceAttack(int enemyNumber, int damage = -478294) {
        if (enemyNumber <= UIController.instance.encounter.EnabledEnemies.Length && enemyNumber > 0) {
            //UIController.instance.SwitchState(UIController.UIState.ATTACKING);
            UIController.instance.fightUI.targetNumber = 1;
            UIController.instance.fightUI.targetIDs = new int[] { enemyNumber - 1 };
            UIController.instance.fightUI.quickInit(UIController.instance.encounter.EnabledEnemies[enemyNumber - 1], damage);
        } else
            UnitaleUtil.DisplayLuaError("Force Attack", "Enemy number " + enemyNumber + " doesn't exist.");
    }

    public int[] MultiTarget(int damage) { return MultiTarget(null, new int[] { damage }); }
    public int[] MultiTarget(int[] damage, bool thisIsTheDamageForm) { return MultiTarget(null, damage); }
    public int[] MultiTarget(int[] targets = null, int[] damage = null) {
        if (targets != null) {
            if (targets.Length < 2) {
                UnitaleUtil.DisplayLuaError("Multi Target", "You must have at least 2 enemies to trigger a multi attack.");
                return null;
            }
            for (int i = 0; i < targets.Length; i++) {
                targets[i]--;
                if (targets[i] >= UIController.instance.encounter.EnabledEnemies.Length || targets[i] < 0) {
                    UnitaleUtil.DisplayLuaError("Multi Target", "Enemy number " + targets[i] + " doesn't exist.");
                    return null;
                }
            }
        }
        UIController.instance.fightUI.multiHit = true;
        if (targets == null) {
            targets = new int[UIController.instance.encounter.EnabledEnemies.Length];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = i;
        }
        if (damage != null)
            if (damage.Length != 1 && damage.Length != targets.Length)
                UnitaleUtil.DisplayLuaError("Multi Target", "You may have as many numbers of damage values as the number of enemies if you're using forced damage,"
                                                          + " or 1 for all enemies at the same time.");

        UIController.instance.fightUI.targetIDs = targets;
        UIController.instance.fightUI.targetNumber = targets.Length;
        if (damage != null) {
            if (damage.Length == 1) {
                int tempDamage = damage[0];
                damage = new int[UIController.instance.fightUI.targetNumber];
                for (int i = 0; i < damage.Length; i++)
                    damage[i] = tempDamage;
            }
            for (int i = 0; i < damage.Length; i++)
                UIController.instance.encounter.EnabledEnemies[targets[i]].presetDmg = damage[i];
            /*for (int i = 0; i < targets.Length; i++) {
                Debug.Log((UIController.instance.fightUI.allFightUiInstances.Count - 1 - (targets.Length - 1 - i)) + " / " + (UIController.instance.fightUI.allFightUiInstances.Count - 1));
                UIController.instance.fightUI.allFightUiInstances[UIController.instance.fightUI.allFightUiInstances.Count - 1 - (targets.Length - 1 - i)].Damage = damage[i];
            }*/
        }
        return damage;
    }

    public void ForceMultiAttack(int damage) { ForceMultiAttack(null, new int[] { damage }); }
    public void ForceMultiAttack(int[] damage, bool thisIsTheDamageForm) { ForceMultiAttack(null, damage); }
    public void ForceMultiAttack(int[] targets = null, int[] damage = null) {
        int[] damage2 = MultiTarget(targets, damage);
        if (targets == null) {
            targets = new int[UIController.instance.encounter.EnabledEnemies.Length];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = i;
        }
        if (damage != null)
            if (damage.Length == 1) {
                int tempDamage = damage[0];
                damage = new int[targets.Length];
                for (int i = 0; i < damage.Length; i++)
                    damage[i] = tempDamage;
            } else
                damage = damage2;
        UIController.instance.fightUI.quickMultiInit(2.2f, damage);
    }

    public void CheckDeath() {
        UIController.instance.needOnDeath = true;
    }
}