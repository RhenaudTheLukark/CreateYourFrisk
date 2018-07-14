using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the attack animation related to an enemy.
/// </summary>
public class FightUI : MonoBehaviour {
    public LuaSpriteController slice;       // Slice animation.
    public LifeBarController lifeBar;       // Life bar object.
    public TextManager damageText;          // Text displaying the damage aplied to the enemy.
    private RectTransform damageTextRt;     // Hitbox of the damage text.
    
    public bool shakeInProgress = false;    // Is the enemy still shaking?
    private int[] shakeX;                   // Static shake position of the enemy at each frame.
    //private int[] shakeX = new int[] { 12, -12, 7, -7, 3, -3, 1, -1, 0 };
    private int shakeIndex = -1;            // Current frame of the shaking animation displayed.
    private int Damage = -478294;           // Damage displayed by the damage text.
    private float shakeTimer = 0.0f;        // Current timer of the shaking animation.
    private float totalShakeTime = 1.5f;    // Total time of the shaking animation.
    public LuaEnemyController enemy;        // Enemy this attack animation aims at. 
    public Vector2 enePos, eneSize;         // Size and position of the enemy's sprite.
    private string[] sliceAnim;             // Animation of the sliceAnim.
    public float sliceAnimFrequency;        // Time for each frame of the slice animation to appear.

    private bool triggersOnce = false;      // Hacky way to only launch a code block on the main animation only once.
    private bool stayInAnimation = false;   // Used to stay in the main animation loop until the attack handler has been launched.
    public bool attackHandlerLaunched = false; // Used to know when the attack handler has been launched.
    public bool attackCursorStopped = false; // Determines if the attack cursor before the animation has been stopped or not.

    /// <summary>
    /// First function launched by the object as it is created.
    /// </summary>
    public void Start() {
        // Registers the components the animation uses for later use.
        foreach(RectTransform child in transform) {
            if (child.name == "SliceAnim")
                slice = new LuaSpriteController(child.GetComponent<Image>());
            else if (child.name == "HPBar")
                lifeBar = child.GetComponent<LifeBarController>();
            else if (child.name == "DamageNumber") {
                damageText = child.GetComponent<TextManager>();
                damageTextRt = child.GetComponent<RectTransform>();
            }
        }
        // Sets all the parameters the attack anim take from the encounter's values.
        sliceAnim = UIController.instance.fightUI.sliceAnim;
        sliceAnimFrequency = UIController.instance.fightUI.sliceAnimFrequency;
        shakeX = UIController.instance.fightUI.shakeX;
        damageText.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        damageText.SetMute(true);
    }
    
    /// <summary>
    /// Initializes the attack animation.
    /// </summary>
    /// <param name="enemyIndex"></param>
    public void Init(int enemyIndex) {
        Start();
        // HACK: The static damage value used here is supposed to never be used.
        Damage = -478294;
        // Set the lifebar up and append the animation's sprites on the target enemy.
        lifeBar.setVisible(false);
        lifeBar.whenDamage = true;
        enemy = UIController.instance.encounter.EnabledEnemies[enemyIndex];
        lifeBar.transform.SetParent(enemy.transform);
        damageText.transform.SetParent(enemy.transform);
        slice.img.transform.SetParent(enemy.transform);
        // Store the enemy's position for the shaking animation and the enemy's size for the slice animation.
        enePos = enemy.GetComponent<RectTransform>().position;
        eneSize = enemy.GetComponent<RectTransform>().sizeDelta;
        shakeInProgress = false;
        shakeTimer = 0;
        // Sets the slice animation's position.
        Vector3 slicePos = new Vector3(enemy.offsets[0].x, eneSize.y / 2 + enemy.offsets[0].y - 55, 0);
        slice.img.GetComponent<RectTransform>().localPosition = slicePos;
    }

    /// <summary>
    /// Initializes the attack animation with a forced damage value.
    /// </summary>
    /// <param name="enemyIndex">Index of the targeted enemy.</param>
    /// <param name="damage">Damage dealt, set it to a value to force the amount of damage.</param>
    public void ForcedDamageInit(int enemyIndex, int damage = -478294) {
        Init(enemyIndex);
        if (damage != -478294)
            Damage = damage;
    }

    /// <summary>
    /// Checks if the animation is finished.
    /// </summary>
    /// <returns></returns>
    public bool Finished() {
        if (shakeTimer > 0)
            return shakeTimer >= totalShakeTime;
        return false;
    }
	
    /// <summary>
    /// Changes the target of this attack animation.
    /// </summary>
    /// <param name="target">New animation's target.</param>
    public void ChangeTarget(LuaEnemyController target) {
        enemy = target;
        Damage = FightUIController.instance.getDamage(enemy, PlayerController.instance.lastHitMult);
        // Append the animation's sprites on the new target enemy.
        enePos = enemy.GetComponent<RectTransform>().position;
        eneSize = enemy.GetComponent<RectTransform>().sizeDelta;
        lifeBar.transform.SetParent(enemy.transform);
        damageText.transform.SetParent(enemy.transform);
        slice.img.transform.SetParent(enemy.transform);
        // Sets the slice animation's position.
        Vector3 slicePos = new Vector3(enemy.offsets[0].x, eneSize.y / 2 + enemy.offsets[0].y - 55, 0);
        slice.img.GetComponent<RectTransform>().localPosition = slicePos;
    }

    /// <summary>
    /// Function launched when the attack cursor has been stopped.
    /// </summary>
    /// <param name="atkMult"></param>
    public void StopAction(float atkMult) {
        // Get attack's accuracy.
        PlayerController.instance.lastHitMult = FightUIController.instance.getAtkMult();
        // Force the damage dealt 
        attackCursorStopped = true;
        // Calls the "BeforeDamageCalculation" function in the enemy's Lua script.
        enemy.TryCall("BeforeDamageCalculation");
        // If the Damage value hasn't been changed, compute the damage.
        if (Damage != -478294)
            Damage = FightUIController.instance.getDamage(enemy, atkMult);
        
        // Starts the slice animation.
        slice.SetAnimation(sliceAnim, sliceAnimFrequency);
        slice.loopmode = "ONESHOT";
    }

    /// <summary>
    /// Updates the object. Called once per frame.
    /// </summary>
    void Update() {
        // Enemy is shaking
        if (shakeInProgress) {
            // Compute what shake position should the player be at.
            int shakeidx = (int)Mathf.Floor(shakeTimer * shakeX.Length / totalShakeTime);
            // If the damage value is greater than 0 and if the animation must display the next frame...
            if (Damage > 0 && shakeIndex != shakeidx) {
                shakeIndex = shakeidx;
                // Move the enemy to the desired shaking position.
                enemy.GetComponent<RectTransform>().anchoredPosition = new Vector2(enePos.x + shakeX[shakeIndex], enePos.y);
                
                // TEST: Shakes the game window alongside the enemy if on the Windows Standalone or on Unity.
                /*#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                    if (StaticInits.ENCOUNTER == "01 - Two monsters" && StaticInits.MODFOLDER == "Examples")
                        Misc.MoveWindow(shakeX[shakeIndex] * 2, 0);
                #endif*/
            }
            if (shakeTimer < 1.5f) // The text only moves for the first 1.5 seconds.
                damageTextRt.localPosition = new Vector2(damageTextRt.localPosition.x, enemy.offsets[2].y + 40 * (2 + Mathf.Sin(shakeTimer * Mathf.PI * 0.75f)));
            shakeTimer += Time.deltaTime;
            // End of the shaking animation.
            if (shakeTimer >= totalShakeTime)
                shakeInProgress = false;
        // Slice anim completed, 
        } else if ((slice.animcomplete && !slice.img.GetComponent<KeyframeCollection>().enabled && attackCursorStopped && !attackHandlerLaunched) || stayInAnimation) {
            stayInAnimation = true;
            // Code block triggered once, used to wait a frame before reaching the next block.
            if (!triggersOnce) {
                triggersOnce = true;
                // Ends the slice animation.
                slice.StopAnimation();
                slice.Set("empty");
                // Triggers the "BeforeDamageValues" function in the enemy's Lua script.
                enemy.TryCall("BeforeDamageValues");
                // Plays the hit sound if the damage dealt is positive.
                if (Damage > 0) {
                    AudioSource aSrc = GetComponent<AudioSource>();
                    aSrc.clip = AudioClipRegistry.GetSound("hitsound");
                    aSrc.Play();
                }
                // Set damage numbers and moves the enemy's sprite.
                string damageTextStr = "";
                if (Damage == 0)     damageTextStr = "[color:c0c0c0]MISS";       // MISS, no damage dealt
                else if (Damage > 0) damageTextStr = "[color:ff0000]" + Damage;  // Positive damage dealt, HP loss, red text
                else                 damageTextStr = "[color:00ff00]" + Damage;  // Negative damage dealt, HP gain, green text

                // The -14 is to compensate for the 14 characters that [color:rrggbb] is worth until commands no longer count for text length.
                // Might change that some day.
                // This block is lvk's code, I don't take responsibility in any number of hacks involved.
                int damageTextWidth = (damageTextStr.Length - 14) * 29 + (damageTextStr.Length - 1 - 14) * 3; // lol hardcoded offsets
                foreach (char c in damageTextStr)
                    if (c == '1')
                        damageTextWidth -= 12; // lol hardcoded offsets

                // Moves the damage text up, making it bounce up before falling back down.
                damageTextRt.localPosition = new Vector2(- 0.5f * damageTextWidth + enemy.offsets[2].x, 40 + enemy.offsets[2].y);
                damageText.SetText(new TextMessage(damageTextStr, false, true));

                // Initiate lifebar and set lerp to its new health value.
                if (Damage != 0) {
                    try {
                        // Moves the life bar and sets up some values.
                        lifeBar.GetComponent<RectTransform>().localPosition = new Vector2(enemy.offsets[2].x, 20 + enemy.offsets[2].y);
                        lifeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(enemy.GetComponent<RectTransform>().rect.width, 13);
                        lifeBar.whenDamageValue = enemy.GetComponent<RectTransform>().rect.width;
                        // THE FOLLOWING CAST IS NOT REDUNDANT, IT'S USED TO GET FLOAT VALUES (NOT INTEGERS).
                        lifeBar.setInstant(enemy.HP < 0 ? 0 : (float)enemy.HP / (float)enemy.MaxHP);
                        lifeBar.setLerp((float)enemy.HP / (float)enemy.MaxHP, (float)(enemy.HP - Damage) / (float)enemy.MaxHP);
                        // Displays the life bar.
                        lifeBar.setVisible(true);
                        // Applies the amount of damage to the enemy's data.
                        enemy.doDamage(Damage);
                    } catch { return; }
                }
                // Launches the enemy's "HandleAttack" Lua function.
                enemy.HandleAttack(Damage);
            } else {
                // Sets some variables to advance the animation.
                shakeInProgress = true;
                stayInAnimation = false;
                totalShakeTime = shakeX.Length * (1.5f / 8.0f);
                attackHandlerLaunched = true;
            }
        }
        // Might actually be useless, so I'm commenting it out for now. TODO: More tests needed.
        //else if (!slice.animcomplete)
        //    slice.img.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(slice.img.GetComponent<Image>().sprite.rect.width, slice.img.GetComponent<Image>().sprite.rect.height);
    }
}
