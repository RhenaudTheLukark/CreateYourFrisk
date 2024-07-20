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
    private readonly LuaSpriteController spr;

    /// <summary>
    /// Create a new Lua controller intended for this player.
    /// </summary>
    /// <param name="p">PlayerController this controller is intended for</param>
    public LuaPlayerStatus(PlayerController p) {
        player = p;
        spr = LuaSpriteController.GetOrCreate(p.gameObject);
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
        set { player.SetHP(value); }
    }

    /// <summary>
    /// Player's Max HP.
    /// </summary>
    public int maxhp {
        get { return PlayerCharacter.instance.MaxHP; }
        set { player.SetMaxHPShift(value, 0f, true, false, false); }
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
            if (UIStats.instance)
                UIStats.instance.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
        }
    }

    /// <summary>
    /// Player character's level. Adjusts stats when set.
    /// </summary>
    public int lv {
        get { return PlayerCharacter.instance.LV; }
        set {
            if (PlayerCharacter.instance.LV == value) return;
            PlayerCharacter.instance.SetLevel(value);
            if (UIStats.instance) {
                UIStats.instance.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
                UIStats.instance.setMaxHP();
            }
        }
    }

    public float speed {
        get { return player.soul.realSpeed; }
        set { player.soul.SetSpeed(value); }
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
    /// <param name="playSound">Will sound be played from this action?</param>
    public void Hurt(float damage, float invulTime = 1.7f, bool ignoreDef = false, bool playSound = true) { player.Hurt(damage, invulTime, ignoreDef, playSound); }

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
        if (UIController.instance.GetState() == "DEFENDING") player.setControlOverride(overrideControl);
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
    /// Sets the player's HP above his HP Max.
    /// </summary>
    public void ForceHP(float HP) { player.SetHP(HP, true); }

    /// <summary>
    /// Sets a shift for the player's Max HP. Can be settable and can modify the player's HP.
    /// </summary>
    /// <param name="shift"></param>
    /// <param name="invulSec"></param>
    /// <param name="set"></param>
    /// <param name="canHeal"></param>
    /// <param name="sound"></param>
    public void SetMaxHPShift(int shift, float invulSec = 1.7f, bool set = false, bool canHeal = false, bool sound = true) { player.SetMaxHPShift(shift, invulSec, set, canHeal, sound); }
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
            if (UIStats.instance)
                UIStats.instance.setMaxHP();
        }
        if (resetATK)
            atk = 8 + (2 * lv);
        if (resetDEF)
            def = 10 + (int)UnityEngine.Mathf.Floor((lv - 1) / 4f);
    }

    public void SetAttackAnim(string[] anim, float frequency = 1 / 6f, string prefix = "") {
        if (anim.Length == 0) {
            UIController.instance.fightUI.sliceAnim = new[] { "empty" };
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
        UIController.instance.fightUI.sliceAnim = new[] {
            "UI/Battle/spr_slice_o_0",
            "UI/Battle/spr_slice_o_1",
            "UI/Battle/spr_slice_o_2",
            "UI/Battle/spr_slice_o_3",
            "UI/Battle/spr_slice_o_4",
            "UI/Battle/spr_slice_o_5"
        };
    }

    public void ChangeTarget(int index) {
        if (UIController.instance.state != "ATTACKING")
            return;
        if (index > UIController.instance.encounter.EnabledEnemies.Length || index <= 0)
            throw new CYFException("Player.ChangeTarget(): Enemy number " + index + " doesn't exist.");

        UIController.instance.fightUI.ChangeTarget(UIController.instance.encounter.EnabledEnemies[index -1]);
    }

    public void ForceAttack(int enemyNumber, int damage = FightUIController.DAMAGE_NOT_SET) {
        if (enemyNumber > UIController.instance.encounter.EnabledEnemies.Length || enemyNumber <= 0)
            throw new CYFException("Player.ForceAttack(): Enemy number " + enemyNumber + " doesn't exist.");

        UIController.instance.fightUI.targetNumber = 1;
        UIController.instance.fightUI.targetIDs = new[] { enemyNumber - 1 };
        UIController.instance.fightUI.QuickInit(damage);
    }

    public int[] MultiTarget(int damage = FightUIController.DAMAGE_NOT_SET) { return MultiTarget(null,    new[] { damage }); }
    public int[] MultiTarget(int[] targets, int damage)                     { return MultiTarget(targets, new[] { damage }); }
    public int[] MultiTarget(int[] targets = null, int[] damage = null) {
        UIController.instance.fightUI.multiHit = true;

        // Create a table with all active enemies if none's given
        if (targets == null) {
            targets = new int[UIController.instance.encounter.EnabledEnemies.Length];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = i;
        } else {
            if (targets.Length < 2)
                throw new CYFException("Player.MultiTarget(): You must have at least 2 enemies to trigger a multi attack.");

            // Check for valid attack IDs
            for (int i = 0; i < targets.Length; i++) {
                targets[i]--;
                if (targets[i] >= UIController.instance.encounter.EnabledEnemies.Length || targets[i] < 0)
                    throw new CYFException("Player.MultiTarget(): Enemy number " + targets[i] + " doesn't exist.");
            }
        }

        UIController.instance.fightUI.targetIDs = targets;
        UIController.instance.fightUI.targetNumber = targets.Length;

        // Use a dummy value to not replace the attack values of the enemies themselves
        if (damage == null) damage = new[] { FightUIController.DAMAGE_NOT_SET };

        // Check same amount of targets / damage values if each has their own
        if (damage.Length != 1 && damage.Length != targets.Length)
            throw new CYFException("Player.MultiTarget(): You may have as many numbers of damage values as the number of"
                                 + " enemies if you're using forced damage, or 1 for all enemies at the same time.");

        if (damage.Length != 1) return damage;

        // If only one value, copy it for all targets
        int tempDamage = damage[0];
        damage = new int[targets.Length];
        for (int i = 0; i < damage.Length; i++)
            damage[i] = tempDamage;
        return damage;
    }

    public void ForceMultiAttack(int damage = FightUIController.DAMAGE_NOT_SET) { ForceMultiAttack(null,    new[] { damage }); }
    public void ForceMultiAttack(int[] targets, int damage)                     { ForceMultiAttack(targets, new[] { damage }); }
    public void ForceMultiAttack(int[] targets = null, int[] damage = null) {
        try                    { damage = MultiTarget(targets, damage); }
        catch (CYFException e) { throw new CYFException("Player.ForceMultiAttack() using " + e.Message); }
        UIController.instance.fightUI.QuickInit(damage);
    }

    public void CheckDeath() { UIController.instance.checkDeathCall = true; }
}