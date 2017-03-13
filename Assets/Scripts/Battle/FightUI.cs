using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FightUI : MonoBehaviour {
    public LuaSpriteController slice;
    private LifeBarController lifeBar;
    public TextManager damageText;
    private RectTransform damageTextRt;
    
    public bool shakeInProgress = false;
    private int[] shakeX = new int[] { 12, -12, 7, -7, 3, -3, 1, -1, 0 };
    //private int[] shakeX = new int[] { 24, 0, 0, 0, 0, -48, 0, 0, 0, 0, 38, 0, 0, 0, 0, -28, 0, 0, 0, 0, 20, 0, 0, 0, 0, -12, 0, 0, 0, 0, 8, 0, 0, 0, 0, -2, 0, 0, 0, 0};
    private int shakeIndex = -1;
    private int Damage = -478294;
    private float shakeTimer = 0.0f;
    private float totalShakeTime = 1.5f;
    public float sliceAnimFrequency = 1 / 6f;
    public LuaEnemyController enemy;
    public Vector2 enePos, eneSize;
    private string[] sliceAnim = new string[] {
        "UI/Battle/spr_slice_o_0",
        "UI/Battle/spr_slice_o_1",
        "UI/Battle/spr_slice_o_2",
        "UI/Battle/spr_slice_o_3",
        "UI/Battle/spr_slice_o_4",
        "UI/Battle/spr_slice_o_5"
    };
    private bool wait1frame = false; //Hacky way to wait one frame before launching the lifebars anim
    private bool needAgain = false;
    private bool showedup = false;
    public bool stopped = false;
    public bool isCoroutine = false;
    public bool waitingToFade = false;

    public void Start() {
        foreach(RectTransform child in transform) {
            if (child.name == "SliceAnim")
                slice = new LuaSpriteController(child.GetComponent<Image>(), "empty");
            else if (child.name == "HPBar")
                lifeBar = child.GetComponent<LifeBarController>();
            else if (child.name == "DamageNumber") {
                damageText = child.GetComponent<TextManager>();
                damageTextRt = child.GetComponent<RectTransform>();
            }
        }
        sliceAnim = UIController.instance.fightUI.sliceAnim;
        sliceAnimFrequency = UIController.instance.fightUI.sliceAnimFrequency;
        shakeX = UIController.instance.fightUI.shakeX;
        damageText.setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        damageText.setMute(true);
    }
    
    public void Init(int enemyIndex) {
        Start();
        Damage = -478294;
        lifeBar.setVisible(false);
        lifeBar.whenDamage = true;
        enemy = UIController.instance.encounter.enabledEnemies[enemyIndex];
        enePos = enemy.GetComponent<RectTransform>().position;
        eneSize = enemy.GetComponent<RectTransform>().sizeDelta;
        shakeTimer = 0;
        Vector3 slicePos = new Vector3(enemy.GetComponent<RectTransform>().position.x + enemy.offsets[0].x,
                                       enemy.GetComponent<RectTransform>().position.y + eneSize.y / 2 + enemy.offsets[0].y - 55, enemy.GetComponent<RectTransform>().position.z);
        slice.img.GetComponent<RectTransform>().position = slicePos;
    }

    public void quickInit(int enemyIndex, LuaEnemyController target, int damage = -478294) {
        Init(enemyIndex);
        enemy = target;
        if (damage != -478294)
            Damage = damage;
        shakeInProgress = false;
    }

    public bool Finished() {
        if (shakeTimer > 0)
            return shakeTimer >= totalShakeTime;
        return false;
    }
	
    public void ChangeTarget(LuaEnemyController target) {
        enemy = target;
        if (Damage != -478294)
            Damage = 0;
        Damage = FightUIController.instance.getDamage(enemy, PlayerController.instance.lastHitMult);
        enePos = enemy.GetComponent<RectTransform>().position;
        eneSize = enemy.GetComponent<RectTransform>().sizeDelta;
        Vector3 slicePos = new Vector3(enemy.GetComponent<RectTransform>().position.x + enemy.offsets[0].x,
        enemy.GetComponent<RectTransform>().position.y + eneSize.y / 2 + enemy.offsets[0].y - 55, enemy.GetComponent<RectTransform>().position.z);
        slice.img.GetComponent<RectTransform>().position = slicePos;
    }

    public void StopAction(float atkMult) {
        PlayerController.instance.lastHitMult = FightUIController.instance.getAtkMult();
        bool damagePredefined = false;
        if (Damage != -478294)
            damagePredefined = true;
        stopped = true;
        enemy.TryCall("BeforeDamageCalculation");
        if (!damagePredefined)
            Damage = FightUIController.instance.getDamage(enemy, atkMult);
        //slice.StopAnimation();
        slice.SetAnimation(sliceAnim, sliceAnimFrequency);
        slice.loopmode = "ONESHOT";
    }

    // Update is called once per frame
    void Update() {
        if (isCoroutine &&!shakeInProgress && showedup) {
            for (int i = 0; i < FightUIController.instance.allFightUiInstances.Count; i ++)
                if (FightUIController.instance.allFightUiInstances[i] == this)
                    FightUIController.instance.allFightUiInstances.RemoveAt(i);
            slice.Remove();
            Destroy(gameObject);
        } else if (shakeInProgress) {
            int shakeidx = (int)Mathf.Floor(shakeTimer * shakeX.Length / totalShakeTime);
            bool wentIn = false;
            if (Damage > 0 && shakeIndex != shakeidx || wentIn) {
                if (shakeIndex != shakeidx && shakeIndex >= shakeX.Length)
                    shakeIndex = shakeX.Length - 1;
                shakeIndex = shakeidx;
                wentIn = true;
                Vector2 localEnePos = enemy.GetComponent<RectTransform>().anchoredPosition; // get local position to do the shake
                enemy.GetComponent<RectTransform>().anchoredPosition = new Vector2(localEnePos.x + shakeX[shakeIndex], localEnePos.y);

                /*#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                    if (StaticInits.ENCOUNTER == "01 - Two monsters" && StaticInits.MODFOLDER == "Examples")
                        Misc.MoveWindow(shakeX[shakeIndex] * 2, 0);
                #endif*/

            }
            if (shakeTimer < 1.5f)
                damageTextRt.position = new Vector2(damageTextRt.position.x, enemy.offsets[2].y + enePos.y + 40 * (2 + Mathf.Sin(shakeTimer * Mathf.PI * 0.75f)));
            shakeTimer += Time.deltaTime;
            if (shakeTimer >= totalShakeTime)
                shakeInProgress = false;
        } else if (!shakeInProgress && (slice.animcomplete &&!slice.img.GetComponent<KeyframeCollection>().enabled && stopped &&!showedup) || needAgain) {
            needAgain = true;
            if (!wait1frame) {
                slice.StopAnimation();
                slice.Set("empty");
                enemy.TryCall("BeforeDamageValues");
                if (Damage > 0) {
                    AudioSource aSrc = GetComponent<AudioSource>();
                    aSrc.clip = AudioClipRegistry.GetSound("hitsound");
                    aSrc.Play();
                }
                // set damage numbers and positioning
                string damageTextStr = "";
                if (Damage == 0) damageTextStr = "[color:c0c0c0]MISS";
                else if (Damage > 0) damageTextStr = "[color:ff0000]" + Damage;
                else damageTextStr = "[color:00ff00]" + Damage;
                // the -14 is to compensate for the 14 characters that [color:rrggbb] is worth until commands no longer count for text length. soon
                int damageTextWidth = (damageTextStr.Length - 14) * 29 + (damageTextStr.Length - 1 - 14) * 3; // lol hardcoded offsets
                foreach (char c in damageTextStr)
                    if (c == '1')
                        damageTextWidth -= 12; // lol hardcoded offsets
                damageTextRt.position = new Vector2(enePos.x - 0.5f * damageTextWidth + enemy.offsets[2].x, enePos.y + 40 + enemy.offsets[2].y);
                damageText.setText(new TextMessage(damageTextStr, false, true));

                // initiate lifebar and set lerp to its new health value
                if (Damage != 0) {
                    int newHP = enemy.HP - Damage;
                    try {
                        lifeBar.GetComponent<RectTransform>().position = new Vector2(enePos.x + enemy.offsets[2].x, enePos.y + 20 + enemy.offsets[2].y);
                        lifeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(enemy.GetComponent<RectTransform>().rect.width, 13);
                        lifeBar.whenDamageValue = enemy.GetComponent<RectTransform>().rect.width;
                        lifeBar.setInstant(enemy.HP < 0 ? 0 : (float)enemy.HP / (float)enemy.getMaxHP());
                        lifeBar.setLerp((float)enemy.HP / (float)enemy.getMaxHP(), (float)newHP / (float)enemy.getMaxHP());
                        lifeBar.setVisible(true);
                        enemy.doDamage(Damage);
                    } catch { return; }
                }
                enemy.HandleAttack(Damage);
                wait1frame = true;
            } else {
                // finally, damage enemy and call its attack handler in case you wanna stop music on death or something
                shakeInProgress = true;
                waitingToFade = true;
                needAgain = false;
                totalShakeTime = shakeX.Length * (1.5f / 8.0f);
                wait1frame = false;
                showedup = true;
            }
        } else if (!slice.animcomplete)
            slice.img.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(slice.img.GetComponent<Image>().sprite.rect.width, slice.img.GetComponent<Image>().sprite.rect.height);
    }
}
