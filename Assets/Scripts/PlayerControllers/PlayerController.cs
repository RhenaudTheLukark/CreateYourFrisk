﻿using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public class PlayerController : MonoBehaviour {
    /// <summary>
    /// Singleton of this class: only one PlayerController object can exist at a time in the project.
    /// </summary>
    [HideInInspector]
    public static PlayerController instance;

    /// <summary>
    /// Lua object used by modders to interact or retrieve informations on the Player.
    /// </summary>
    [HideInInspector]
    public static LuaPlayerStatus luaStatus;

    /// <summary>
    /// The RectTransform of the inner box of the battle arena - set using Unity Inspector.
    /// </summary>
    public RectTransform arenaBounds;

    /// <summary>
    /// The absolute position of the Player on screen, used mainly by projectiles for collision detection.
    /// </summary>
    [HideInInspector]
    public Rect playerAbs;

    /// <summary>
    /// The Player's HP.
    /// </summary>
    internal float HP {
        get { return PlayerCharacter.instance.HP; }
        set { PlayerCharacter.instance.HP = value; }
    }

    /// <summary>
    /// Invulnerability timer duing which the Player blinks and is invulnerable as long as this value is greater than 0.
    /// </summary>
    internal float invulTimer;

    /// <summary>
    /// The Player's RectTransform.
    /// </summary>
    internal RectTransform self;

    /// <summary>
    /// How long it takes to do a full blink (appear+disappear), in seconds.
    /// </summary>
    private const float BLINK_CYCLE_SECONDS = 0.18f;

    /// <summary>
    /// Pixels to inset the Player's hitbox, as a temporary replacement for having actually good hitboxes.
    /// </summary>
    private const int HITBOX_INSET = 4;

    /// <summary>
    /// The hurt sound component attached to the Player.
    /// </summary>
    private static AudioSource playerAudio;

    /// <summary>
    /// Intended direction for movement; -1 OR 1 for x, -1 OR 1 for y. Multiplied by speed in Move() function.
    /// </summary>
    private Vector2 intendedShift;

    /// <summary>
    /// True if the Player is moving, false otherwise. Set in the Move function, retrieved through isMoving().
    /// </summary>
    private bool moving;

    /// <summary>
    /// If true, the engine ignores movement inputs. Done when the player should be controlled by something else, like the UI.
    /// </summary>
    public bool overrideControl;

    /// <summary>
    /// Player's image.
    /// </summary>
    public Image selfImg;

    /// <summary>
    /// Player's texture.
    /// </summary>
    public Color32[] texture;

    /// <summary>
    /// Contains a Soul type that affects what player movement does.
    /// Only the Red soul is used for now.
    /// </summary>
    public AbstractSoul soul;

    /// <summary>
    /// True if the Player's defense should be taken in account when computing damage dealt to them, false otherwise.
    /// </summary>
    public static bool allowplayerdef;

    /// <summary>
    /// The last movement of the player.
    /// </summary>
    public Vector2 lastMovement;

    public int lastEnemyChosen = -1;
    public float lastHitMult = -1;

    /// <summary>
    /// Contains directions the player can go in. This is to make abstracting out controls and adding control schemes at a later point easier.
    /// </summary>
    private enum Directions { UP, DOWN, LEFT, RIGHT };

    public void Start() {
        HP = PlayerCharacter.instance.HP;
    }

    public static void PlaySound(string sound) {
        UnitaleUtil.PlaySound("CollisionSoundChannel", sound);
    }

    public string deathMusic;
    public string[] deathText;
    public bool deathEscape = true;
    private int soundDelay;

    /// <summary>
    /// Hurts the player and makes them invulnerable for invulnerabilitySeconds.
    /// </summary>
    /// <param name="damage">Damage to deal to the player.</param>
    /// <param name="invulnerabilitySeconds">Optional invulnerability time for the player, in seconds.</param>
    /// <param name="isDefIgnored">If false, will use Undertale's damage formula.</param>
    /// <param name="playSound">If false, this function will not play any sound clips.</param>
    /// <returns></returns>
    public virtual void Hurt(float damage = 3, float invulnerabilitySeconds = 1.7f, bool isDefIgnored = false, bool playSound = true) {
        if (!isDefIgnored)
            if (allowplayerdef && damage > 0) {
                damage = damage + 2 - Mathf.FloorToInt((PlayerCharacter.instance.DEF + PlayerCharacter.instance.ArmorDEF) / 5f);
                if (damage <= 0)
                    damage = 1;
            }
        // Set timer and play the hurt sound if player was actually hurt

        // Reset the hurt timer if the arguments passed are (0, 0)
        if (damage == 0 && invulnerabilitySeconds == 0) {
            invulTimer = 0;
            selfImg.enabled = true;
            return;
        }

        if (damage >= 0 && (invulTimer <= 0 || invulnerabilitySeconds < 0)) {
            if (soundDelay < 0 && playSound) {
                soundDelay = 2;
                PlaySound("hurtsound");
            }

            if (invulnerabilitySeconds >= 0) invulTimer = invulnerabilitySeconds;
            if (damage != 0)                 SetHP(HP - damage, false);
        } else if (damage < 0) {
            if (playSound)
                PlaySound("healsound");
            SetHP(HP - damage);
        }
    }

    public void SetHP(float newhp, bool actualDamage = true) {
        newhp = Mathf.Round(newhp * Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma)) / Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma);

        // Retromode: Make Player.hp act as an integer
        if (GlobalControls.retroMode)
            newhp = Mathf.Floor(newhp);

        if (newhp <= 0 && !deathEscape)
            return;

        if (newhp <= 0) {
            deathEscape = false;
            if (GlobalControls.isInFight) {
                UnitaleUtil.TryCall(EnemyEncounter.script, "BeforeDeath");
                if (deathEscape)
                    return;

                DynValue dialogues = EnemyEncounter.script.GetVar("deathtext");
                if (dialogues == null)            deathText = null;
                else if (dialogues.Table == null) deathText = dialogues.String != null ? new[] { dialogues.String } : null;
                else {
                    deathText = new string[dialogues.Table.Length];
                    for (int i = 0; i < dialogues.Table.Length; i++)
                        deathText[i] = dialogues.Table.Get(i + 1).String;
                }
                deathMusic = EnemyEncounter.script.GetVar("deathmusic").String;
                if (deathMusic == "")
                    deathMusic = null;
            }
            if (!MusicManager.IsStoppedOrNull(PlayerOverworld.audioKept)) {
                GetComponent<GameOverBehavior>().musicBefore = PlayerOverworld.audioKept;
                GetComponent<GameOverBehavior>().music = GetComponent<GameOverBehavior>().musicBefore.clip;
                GetComponent<GameOverBehavior>().musicBefore.Stop();
            } else if (!MusicManager.IsStoppedOrNull(Camera.main.GetComponent<AudioSource>())) {
                GetComponent<GameOverBehavior>().musicBefore = Camera.main.GetComponent<AudioSource>();
                GetComponent<GameOverBehavior>().music = GetComponent<GameOverBehavior>().musicBefore.clip;
                GetComponent<GameOverBehavior>().musicBefore.Stop();
            } else {
                GetComponent<GameOverBehavior>().musicBefore = null;
                GetComponent<GameOverBehavior>().music = null;
            }
            HP = 0;
            invulTimer = 0;
            selfImg.enabled = true;
            setControlOverride(true);
            RectTransform rt = gameObject.GetComponent<RectTransform>();
            Vector2 pos = rt.position;
            rt.position = new Vector3(pos.x, pos.y, -1000);
            GlobalControls.stopScreenShake = true;
            gameObject.GetComponent<GameOverBehavior>().StartDeath(deathText, deathMusic);
            return;
        }
        if (newhp > PlayerCharacter.instance.MaxHP * 1.5 &&!actualDamage)
            if (newhp > ControlPanel.instance.HPLimit) HP = ControlPanel.instance.HPLimit;
            else                                       HP = (int)(PlayerCharacter.instance.MaxHP * 1.5f);
        // HP greater than Max, heal, already more HP than Max
        else if (newhp > PlayerCharacter.instance.MaxHP && actualDamage && newhp > PlayerCharacter.instance.HP && PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP) { }
        // HP greater than Max, heal
        else if (newhp > PlayerCharacter.instance.MaxHP && actualDamage && newhp > PlayerCharacter.instance.HP)  HP = PlayerCharacter.instance.MaxHP;
        else                                                                                                     HP = newhp;
        if (HP > ControlPanel.instance.HPLimit)
            HP = ControlPanel.instance.HPLimit;
        deathEscape = true;
        if (UIStats.instance)
            UIStats.instance.setHP(HP);
    }

    public void SetMaxHPShift(int shift, float invulnerabilitySeconds = 1.7f, bool set = false, bool canHeal = false, bool sound = true) {
        invulTimer = invulnerabilitySeconds;
        if ((PlayerCharacter.instance.MaxHP + shift <= 0 &&!set) || (shift <= 0 && set)) {
            shift = 0;
            set = true;
        }
        if (set) {
            if (shift == 0) {
                SetHP(0);
                return;
            }

            if (shift > 999)
                shift = 999;
            if (shift == PlayerCharacter.instance.MaxHP)
                return;
            int oldMHP = PlayerCharacter.instance.MaxHP;
            PlayerCharacter.instance.MaxHPShift = shift - PlayerCharacter.instance.BasisMaxHP;
            if (shift < oldMHP) {
                if (sound) {
                    playerAudio.clip = AudioClipRegistry.GetSound("hurtsound");
                    playerAudio.Play();
                }
            } else {
                if (sound) {
                    playerAudio.clip = AudioClipRegistry.GetSound("healsound");
                    playerAudio.Play();
                }
                if (canHeal && oldMHP < shift) {
                    SetHP(PlayerCharacter.instance.HP + (shift - oldMHP));
                }
            }
        } else {
            if (shift + PlayerCharacter.instance.MaxHP > 999)
                shift = 999 - PlayerCharacter.instance.MaxHP;
            if (shift == 0)
                return;
            PlayerCharacter.instance.MaxHPShift += shift;
            if (shift < 0) {
                if (sound) {
                    playerAudio.clip = AudioClipRegistry.GetSound("hurtsound");
                    playerAudio.Play();
                }
            } else {
                if (sound) {
                    playerAudio.clip = AudioClipRegistry.GetSound("healsound");
                    playerAudio.Play();
                }
                if (canHeal)
                    SetHP(PlayerCharacter.instance.HP + shift);
            }
        }
        if (PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP)
            SetHP(PlayerCharacter.instance.MaxHP);
        if (UIStats.instance)
            UIStats.instance.setMaxHP();
    }

    public bool isHurting() { return invulTimer > 0; }

    // Check if player is moving, used in orange/blue projectiles to see if they should hurt or not
    public bool isMoving() { return moving; }

    // Modify absolute player position, accounting for walls
    public void ModifyPosition(float xMove, float yMove, bool ignoreBounds) {
        float xPos = self.anchoredPosition.x + xMove;
        float yPos = self.anchoredPosition.y + yMove;

        SetPosition(xPos, yPos, ignoreBounds);
    }

    public void MoveDirect(Vector2 pos) {
        lastMovement = pos;
        float oldXPos = self.anchoredPosition.x, oldYPos = self.anchoredPosition.y;
        ModifyPosition(pos.x, pos.y, false);
        MovementDelta(oldXPos, oldYPos);
    }

    // move within arena boundaries given 'directional' vector (non-unit: x is -1 OR 1 and y is -1 OR 1)
    public virtual void Move(Vector2 dir) {
        Vector2 soulDir = soul.GetMovement(dir.x, dir.y);
        if (ControlPanel.instance.FrameBasedMovement) soulDir *= 1.0f/60.0f;
        else                                          soulDir *= Time.deltaTime;
        lastMovement = soulDir;

        // reusing the direction Vector2 for position to save ourselves the creation of a new object
        float oldXPos = self.anchoredPosition.x, oldYPos = self.anchoredPosition.y;
        ModifyPosition(soulDir.x, soulDir.y, false);
        MovementDelta(oldXPos, oldYPos);
    }

    // set to ignore regular battle arena controls and updates. Used to forfeit control to UI without disabling player controller.
    public void setControlOverride(bool overrideControls) {
        this.overrideControl = overrideControls;
        soul.setHalfSpeed(false);
    }

    public void SetPosition(float xPos, float yPos, bool ignoreBounds) {
        // check if new position would be out of arena bounds, and modify accordingly if it is
        if (!ignoreBounds) {
            if (xPos < arenaBounds.position.x - arenaBounds.sizeDelta.x / 2 + self.rect.size.x / 2)
                xPos = arenaBounds.position.x - arenaBounds.sizeDelta.x / 2 + self.rect.size.x / 2;
            else if (xPos > arenaBounds.position.x + arenaBounds.sizeDelta.x / 2 - self.rect.size.x / 2)
                xPos = arenaBounds.position.x + arenaBounds.sizeDelta.x / 2 - self.rect.size.x / 2;

            if (yPos < arenaBounds.position.y - arenaBounds.sizeDelta.y / 2 + self.rect.size.y / 2)
                yPos = arenaBounds.position.y - arenaBounds.sizeDelta.y / 2 + self.rect.size.y / 2;
            else if (yPos > arenaBounds.position.y + arenaBounds.sizeDelta.y / 2 - self.rect.size.y / 2)
                yPos = arenaBounds.position.y + arenaBounds.sizeDelta.y / 2 - self.rect.size.y / 2;
        }

        // set player position on screen
        self.anchoredPosition = new Vector2(xPos, yPos);
        // modify the player rectangle position so projectiles know where it is
        playerAbs.x = self.anchoredPosition.x - self.rect.size.x / 2 + HITBOX_INSET;
        playerAbs.y = self.anchoredPosition.y - self.rect.size.y / 2 + HITBOX_INSET;
    }

    public void SetSoul(AbstractSoul s) {
        selfImg.color = s.color;
        soul = s;
        // if still holding X keep the slow applied
        if (InputUtil.Held(GlobalControls.input.Cancel))
            s.setHalfSpeed(true);
    }

    /// <summary>
    /// Built-in Unity function for initialization.
    /// </summary>
    public void Awake() {
        //HP = PlayerCharacter.instance.MaxHP;
        self = GetComponent<RectTransform>();
        selfImg = GetComponent<Image>();
        playerAbs = new Rect(0, 0, selfImg.sprite.texture.width - HITBOX_INSET * 2, selfImg.sprite.texture.height - HITBOX_INSET * 2);
        instance = this;
        playerAudio = GetComponent<AudioSource>();
        SetSoul(new RedSoul());
        luaStatus = new LuaPlayerStatus(this);
    }

    /// <summary>
    /// Modifies the movement direction based on input. Broken up into single ifs so pressing opposing keys prevents you from moving.
    /// </summary>
    private void HandleInput() {
        if (InputUtil.Held(GlobalControls.input.Up))    intendedShift += ModifyMovementDirection(Directions.UP);
        if (InputUtil.Held(GlobalControls.input.Down))  intendedShift += ModifyMovementDirection(Directions.DOWN);
        if (InputUtil.Held(GlobalControls.input.Left))  intendedShift += ModifyMovementDirection(Directions.LEFT);
        if (InputUtil.Held(GlobalControls.input.Right)) intendedShift += ModifyMovementDirection(Directions.RIGHT);

        if (InputUtil.Pressed(GlobalControls.input.Cancel))       soul.setHalfSpeed(true);
        else if (InputUtil.Released(GlobalControls.input.Cancel)) soul.setHalfSpeed(false);
    }

    // given an input direction, let intendedShift carry 'directional' vector (non-unit: x is -1 OR 1 and y is -1 OR 1)
    private Vector2 ModifyMovementDirection(Directions d) {
        switch (d) {
            case Directions.UP:     return Vector2.up;
            case Directions.DOWN:   return Vector2.down;
            case Directions.LEFT:   return Vector2.left;
            case Directions.RIGHT:  return Vector2.right;
            default:                return Vector2.zero;
        }
    }

    private void MovementDelta(float oldX, float oldY) {
        float xDelta = self.anchoredPosition.x - oldX;
        float yDelta = self.anchoredPosition.y - oldY;

        // if the position is the same, the player hasnt moved - by doing it like this we account
        // for things like being moved by external factors like being shoved by boundaries
        // TODO: account for external factors like being moved by other scripts (enemies e.a.)
        if (xDelta == 0.0f && yDelta == 0.0f) moving = false;
        else                                  moving = true;
        soul.PostMovement(xDelta, yDelta);
    }

    /// <summary>
    /// Built-in Unity function called once per frame.
    /// </summary>
    private void Update() {
        // DEBUG CONTROLS
        /*if (Input.GetKeyDown(KeyCode.Alpha1))
            SetSoul(new RedSoul(this));
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetSoul(new BlueSoul(this));*/
        // END DEBUG CONTROLS
        /*
        if (!ArenaManager.instance.firstTurn && (tempQueue.x != -5000 || tempQueue.y != -5000)) {
            SetPosition(tempQueue.x, tempQueue.y, tempQueue2);
            tempQueue = new Vector2(-5000, -5000);
        }
        */

        // prevent player actions from working and the timer from decreasing, if the game is paused
        if (UIController.instance.frozenState != "PAUSE")
            return;

        // handle input and movement, unless control is overridden by the UI controller, for instance
        if (!overrideControl) {
            intendedShift = Vector2.zero; // reset direction we are going in
            HandleInput(); // get direction we want to go in
            Move(intendedShift);
        }

        // if the invulnerability timer has more than 0 seconds (usually when you get hurt), blink to reflect the hurt state
        if (invulTimer > 0.0f) {
            invulTimer -= Time.deltaTime;
            selfImg.enabled = !(invulTimer % BLINK_CYCLE_SECONDS > BLINK_CYCLE_SECONDS / 2.0f) || invulTimer <= 0.0f;
        }

        // constantly update the hitbox to match the position of the sprite itself
        if (!GlobalControls.retroMode) {
            playerAbs.x = luaStatus.sprite.absx - HITBOX_INSET;
            playerAbs.y = luaStatus.sprite.absy - HITBOX_INSET;
        }

        soundDelay--;
    }
}