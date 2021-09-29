﻿using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class FightUI : MonoBehaviour {
    public LuaSpriteController slice;
    public LifeBarController lifeBar;
    public TextManager damageText;
    private RectTransform damageTextRt;

    public bool shakeInProgress;
    private int[] shakeX = { 12, -12, 7, -7, 3, -3, 1, -1, 0 };
    //private int[] shakeX = new int[] { 24, 0, 0, 0, 0, -48, 0, 0, 0, 0, 38, 0, 0, 0, 0, -28, 0, 0, 0, 0, 20, 0, 0, 0, 0, -12, 0, 0, 0, 0, 8, 0, 0, 0, 0, -2, 0, 0, 0, 0};
    private int shakeIndex = -1;
    private int Damage = FightUIController.DAMAGE_NOT_SET;
    private float shakeTimer;
    private float totalShakeTime = 1.5f;
    public float sliceAnimFrequency = 1 / 6f;
    public EnemyController enemy;
    public Vector2 enePos, eneSize;
    private string[] sliceAnim = {
        "UI/Battle/spr_slice_o_0",
        "UI/Battle/spr_slice_o_1",
        "UI/Battle/spr_slice_o_2",
        "UI/Battle/spr_slice_o_3",
        "UI/Battle/spr_slice_o_4",
        "UI/Battle/spr_slice_o_5"
    };
    private bool wait1frame; //Hacky way to wait one frame before launching the lifebars anim
    private bool needAgain;
    private bool showedup;
    public bool stopped;
    public bool isCoroutine;
    public bool waitingToFade;

    public void Start() {
        foreach(RectTransform child in transform) {
            switch (child.name) {
                case "SliceAnim": slice   = LuaSpriteController.GetOrCreate(child.gameObject); break;
                case "HPBar":     lifeBar = child.GetComponent<LifeBarController>();      break;
                case "DamageNumber":
                    damageText = child.GetComponent<TextManager>();
                    damageTextRt = child.GetComponent<RectTransform>();
                    break;
            }
        }
        sliceAnim = UIController.instance.fightUI.sliceAnim;
        sliceAnimFrequency = UIController.instance.fightUI.sliceAnimFrequency;
        shakeX = UIController.instance.fightUI.shakeX;
        damageText.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        damageText.SetMute(true);
    }

    public void Init(int enemyIndex) {
        Start();
        Damage = FightUIController.DAMAGE_NOT_SET;
        lifeBar.setVisible(false);
        lifeBar.whenDamage = true;
        enemy = UIController.instance.encounter.EnabledEnemies[enemyIndex];
        lifeBar.transform.SetParent(enemy.transform);
        damageText.transform.SetParent(enemy.transform);
        slice.img.transform.SetParent(enemy.transform);
        shakeTimer = 0;
    }

    public void quickInit(int enemyIndex, EnemyController target, int damage) {
        Init(enemyIndex);
        enemy = target;

        if (damage != FightUIController.DAMAGE_NOT_SET)
            Damage = damage;
        shakeInProgress = false;
    }

    public bool Finished() {
        if (shakeTimer > 0)
            return shakeTimer >= totalShakeTime;
        return false;
    }

    public void ChangeTarget(EnemyController target) {
        enemy = target;
        if (Damage != FightUIController.DAMAGE_NOT_SET)
            Damage = 0;
        Damage = FightUIController.instance.getDamage(enemy, PlayerController.instance.lastHitMult);
        lifeBar.transform.SetParent(enemy.transform);
        damageText.transform.SetParent(enemy.transform);
        slice.img.transform.SetParent(enemy.transform);
    }

    public void StopAction(float atkMult) {
        PlayerController.instance.lastHitMult = FightUIController.instance.getAtkMult();
        bool damagePredefined = Damage != FightUIController.DAMAGE_NOT_SET;
        stopped = true;
        UnitaleUtil.TryCall(enemy.script, "BeforeDamageCalculation");
        if (!damagePredefined)
            Damage = FightUIController.instance.getDamage(enemy, atkMult);
        //slice.StopAnimation();
        slice.SetAnimation(sliceAnim, sliceAnimFrequency);
        slice.loopmode = "ONESHOT";
    }

    // Update is called once per frame
    private void Update() {
        // do not update the attack UI if the ATTACKING state is frozen
        if (UIController.instance.frozenState != "PAUSE")
            return;

        eneSize = enemy.GetComponent<RectTransform>().sizeDelta;
        enePos = new Vector2(enemy.GetComponent<RectTransform>().position.x - eneSize.x * (Mathf.Abs(enemy.sprite.xpivot) - 0.5f) * Mathf.Sign(enemy.sprite.xscale),
                             enemy.GetComponent<RectTransform>().position.y - eneSize.y * (Mathf.Abs(enemy.sprite.ypivot) - 0.5f) * Mathf.Sign(enemy.sprite.yscale));

        if (shakeInProgress) {
            int shakeidx = (int)Mathf.Floor(shakeTimer * shakeX.Length / totalShakeTime);
            if (Damage > 0 && shakeIndex != shakeidx) {
                if (shakeIndex != shakeidx && shakeIndex >= shakeX.Length)
                    shakeIndex = shakeX.Length - 1;
                shakeIndex = shakeidx;
                Vector2 localEnePos = enemy.GetComponent<RectTransform>().anchoredPosition; // get local position to do the shake
                enemy.GetComponent<RectTransform>().anchoredPosition = new Vector2(localEnePos.x + shakeX[shakeIndex], localEnePos.y);

                /*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    if (StaticInits.ENCOUNTER == "01 - Two monsters" && StaticInits.MODFOLDER == "Examples")
                        Misc.MoveWindow(shakeX[shakeIndex] * 2, 0);
                #endif*/
            }
            if (shakeTimer < 1.5f)
                damageTextRt.position = new Vector2(damageTextRt.position.x, enePos.y - eneSize.y / 2 + enemy.offsets[2].y + 40 * (2 + Mathf.Sin(shakeTimer * Mathf.PI * 0.75f)));
            shakeTimer += Time.deltaTime;
            if (shakeTimer >= totalShakeTime)
                shakeInProgress = false;
        } else if (((!slice.isactive || slice.animcomplete && !slice.img.GetComponent<KeyframeCollection>().enabled) && stopped &&!showedup) || needAgain) {
            needAgain = true;
            if (!wait1frame) {
                wait1frame = true;
                slice.StopAnimation();
                slice.Set("empty");
                UnitaleUtil.TryCall(enemy.script, "BeforeDamageValues", new[] { DynValue.NewNumber(Damage) });
                if (Damage > 0) {
                    AudioSource aSrc = GetComponent<AudioSource>();
                    aSrc.clip = AudioClipRegistry.GetSound("hitsound");
                    aSrc.Play();
                }
                // set damage numbers and positioning
                string damageTextStr;
                if (Damage == 0) {
                    if (enemy.DefenseMissText == null) damageTextStr = "[color:c0c0c0]MISS";
                    else                               damageTextStr = "[color:c0c0c0]" + enemy.DefenseMissText;
                } else if (Damage > 0)                 damageTextStr = "[color:ff0000]" + Damage;
                else                                   damageTextStr = "[color:00ff00]" + Damage;
                damageTextRt.localPosition = new Vector3(0, 0, 0);
                damageText.SetText(new TextMessage(damageTextStr, false, true));
                damageTextRt.position = new Vector3(enePos.x - UnitaleUtil.CalcTextWidth(damageText) / 2 + enemy.offsets[2].x, enePos.y - eneSize.y / 2 + 40 + enemy.offsets[2].y);

                // initiate lifebar and set lerp to its new health value
                if (Damage != 0) {
                    int newHP = enemy.HP - Damage;
                    try {
                        lifeBar.GetComponent<RectTransform>().position = new Vector2(enePos.x + enemy.offsets[2].x, enePos.y - eneSize.y / 2 + 20 + enemy.offsets[2].y);
                        lifeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(enemy.GetComponent<RectTransform>().rect.width, 13);
                        lifeBar.whenDamageValue = enemy.GetComponent<RectTransform>().rect.width;
                        lifeBar.setInstant(enemy.HP < 0 ? 0 : enemy.HP / (float)enemy.MaxHP);
                        lifeBar.setLerp(enemy.HP / (float)enemy.MaxHP, newHP / (float)enemy.MaxHP);
                        lifeBar.setVisible(true);
                        enemy.doDamage(Damage);
                    } catch { return; }
                }
                enemy.HandleAttack(Damage);
            } else {
                // finally, damage enemy and call its attack handler in case you wanna stop music on death or something
                shakeInProgress = true;
                waitingToFade = true;
                needAgain = false;
                totalShakeTime = shakeX.Length * (1.5f / 8.0f);
                showedup = true;
            }
        } else if (slice.isactive && !slice.animcomplete) {
            slice.img.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(slice.img.GetComponent<Image>().sprite.rect.width, slice.img.GetComponent<Image>().sprite.rect.height);
            slice.img.GetComponent<RectTransform>().position = new Vector2(enePos.x + enemy.offsets[0].x, enePos.y + enemy.offsets[0].y);
        }
    }
}
