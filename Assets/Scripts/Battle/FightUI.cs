using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class FightUI : MonoBehaviour {
    public LuaSpriteController slice;
    public LifeBarController lifeBar;
    public TextManager damageText;

    public bool shakeInProgress;
    private int[] shakeX = { 12, -12, 7, -7, 3, -3, 1, -1, 0 };
    //private int[] shakeX = new int[] { 24, 0, 0, 0, 0, -48, 0, 0, 0, 0, 38, 0, 0, 0, 0, -28, 0, 0, 0, 0, 20, 0, 0, 0, 0, -12, 0, 0, 0, 0, 8, 0, 0, 0, 0, -2, 0, 0, 0, 0};
    private int shakeIndex = -1;
    private int Damage = FightUIController.DAMAGE_NOT_SET;
    private int enemyIndex = 1;
    private float shakeTimer;
    private float totalShakeTime = 1.5f;
    public float sliceAnimFrequency = 1 / 6f;
    public EnemyController enemy;
    public Vector2 eneSize;
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
    public bool waitingToFade;

    public bool isInit;

    public void Setup() {
        if (isInit) return;
        lifeBar = transform.GetComponentInChildren<LifeBarController>();
        lifeBar.Start();
        lifeBar.AddOutline(1);
        lifeBar.outline.SetPivot(0.5f, 0.5f);
        lifeBar.outline.SetAnchor(0.5f, 1);
        lifeBar.SetVisible(false);

        damageText = transform.GetComponentInChildren<TextManager>();
        damageText.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        damageText.SetMute(true);

        slice = LuaSpriteController.GetOrCreate(transform.GetComponentInChildren<Image>().gameObject);
        sliceAnim = UIController.instance.fightUI.sliceAnim;
        sliceAnimFrequency = UIController.instance.fightUI.sliceAnimFrequency;
        shakeX = UIController.instance.fightUI.shakeX;

        if (enemy == null)
            enemy = UIController.instance.encounter.EnabledEnemies[enemyIndex];
        transform.SetParent(enemy.transform);
        transform.localEulerAngles = Vector3.zero;
        GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        isInit = true;
    }

    public void Init(int eIndex) {
        enemyIndex = eIndex;
        Setup();
    }

    public void QuickInit(int enemyIndex, EnemyController target, int damage) {
        Init(enemyIndex);
        enemy = target;

        if (damage != FightUIController.DAMAGE_NOT_SET)
            Damage = damage;
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
        Damage = FightUIController.instance.GetDamage(enemy, PlayerController.instance.lastHitMult);
        transform.SetParent(enemy.transform);
    }

    public void StopAction(float atkMult) {
        PlayerController.instance.lastHitMult = FightUIController.instance.GetAtkMult();
        bool damagePredefined = Damage != FightUIController.DAMAGE_NOT_SET;
        stopped = true;
        UnitaleUtil.TryCall(enemy.script, "BeforeDamageCalculation");
        if (!damagePredefined)
            Damage = FightUIController.instance.GetDamage(enemy, atkMult);
        //slice.StopAnimation();
        slice.SetAnimation(sliceAnim, sliceAnimFrequency);
        slice.loopmode = "ONESHOT";
    }

    // Update is called once per frame
    private void Update() {
        // do not update the attack UI if the ATTACKING state is frozen
        if (UIController.instance.frozenState != "PAUSE" || !isInit)
            return;

        eneSize = enemy.GetComponent<RectTransform>().sizeDelta;

        if (shakeInProgress) {
            int shakeidx = (int)Mathf.Floor(shakeTimer * shakeX.Length / totalShakeTime);
            if (Damage > 0 && shakeIndex != shakeidx) {
                if (shakeIndex != shakeidx && shakeIndex >= shakeX.Length)
                    shakeIndex = shakeX.Length - 1;
                shakeIndex = shakeidx;
                Vector2 localEnePos = enemy.GetComponent<RectTransform>().anchoredPosition; // get local position to do the shake
                enemy.GetComponent<RectTransform>().anchoredPosition = new Vector2(localEnePos.x + shakeX[shakeIndex], localEnePos.y);
            }
            if (shakeTimer < 1.5f)
                damageText.transform.localPosition = new Vector2(-UnitaleUtil.CalcTextWidth(damageText) / 2 + enemy.offsets[2].x,
                                                                 -eneSize.y / 2 + enemy.offsets[2].y + 40 * (2 + Mathf.Sin(shakeTimer * Mathf.PI * 0.75f)));
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
                damageText.SetText(new TextMessage(damageTextStr, false, true));
                damageText.transform.localPosition = new Vector2(-UnitaleUtil.CalcTextWidth(damageText) / 2 + enemy.offsets[2].x, -eneSize.y / 2 + enemy.offsets[2].y + 40);

                // initiate lifebar and set lerp to its new health value
                if (Damage != 0) {
                    int newHP = enemy.HP - Damage;
                    lifeBar.outline.img.transform.localPosition = new Vector2(enemy.offsets[2].x - 1, -eneSize.y / 2 + 20 + enemy.offsets[2].y - 1);
                    lifeBar.Resize(enemy.GetComponent<RectTransform>().rect.width, 13);
                    lifeBar.SetLerp(enemy.HP / (float)enemy.MaxHP, newHP / (float)enemy.MaxHP);
                    lifeBar.SetVisible(true);
                    enemy.doDamage(Damage);
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
            slice.img.transform.localPosition = new Vector2(enemy.offsets[0].x, enemy.offsets[0].y);
        }
    }
}
