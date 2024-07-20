using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FightUIController : MonoBehaviour {
    public static FightUIController instance;
    public List<FightUI> boundFightUiInstances = new List<FightUI>();
    public List<FightUI> allFightUiInstances = new List<FightUI>();

    public const int DAMAGE_NOT_SET = -478294;

    public RectTransform targetRt;
    public LuaSpriteController line;
    private float borderX;
    private const float xSpeed = -450.0f;
    public int[] shakeX; //Modify it in the Editor if needed
    //private int[] shakeX = new int[] { 24, 0, 0, 0, 0, -48, 0, 0, 0, 0, 38, 0, 0, 0, 0, -28, 0, 0, 0, 0, 20, 0, 0, 0, 0, -12, 0, 0, 0, 0, 8, 0, 0, 0, 0, -2, 0, 0, 0, 0};
    private readonly string[] lineAnim = { "UI/Battle/spr_targetchoice_0", "UI/Battle/spr_targetchoice_1" };
    public string[] sliceAnim; //Modify it in the Editor if needed
    public float sliceAnimFrequency = 1 / 6f;
    public int targetNumber = 1;
    public int[] targetIDs = { };
    public bool multiHit;
    public bool stopped;
    private bool finishingFade;

    private void LaunchInstance(bool bind = false) {
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/FightInstance"));
        go.transform.SetParent(transform);
        if (bind)
            boundFightUiInstances.Add(go.GetComponent<FightUI>());
        allFightUiInstances.Add(go.GetComponent<FightUI>());
    }

    private void Awake() {
        line = LuaSpriteController.GetOrCreate(targetRt.gameObject);
        instance = this;
    }

    public void ChangeTarget(EnemyController target) {
        while (boundFightUiInstances.Count > 1) {
            allFightUiInstances.Remove(boundFightUiInstances[1]);
            Destroy(boundFightUiInstances[1].gameObject);
            boundFightUiInstances.RemoveAt(1);
        }
        boundFightUiInstances[0].ChangeTarget(target);
    }

    public void SetAlpha(float a) {
        Color c = Color.white;
        c.a = a;
        GetComponent<Image>().color = c;
    }

    public void Init() {
        CommonInit();
        gameObject.GetComponent<Image>().enabled = true;
        borderX = -GetComponent<RectTransform>().rect.width / 2;
        finishingFade = false;
        stopped = false;
        targetRt.anchoredPosition = new Vector2(GetComponent<RectTransform>().rect.width / 2, 0);
        targetRt.GetComponent<Image>().enabled = true;
        line.StopAnimation();
        line.Set("UI/Battle/spr_targetchoice_0");
        for (int i = 0; i < targetNumber; i++) {
            LaunchInstance(true);
            boundFightUiInstances[boundFightUiInstances.Count - 1].Init(targetIDs[i]);
        }
        SetAlpha(1.0f);
    }

    public void CommonInit() {
        gameObject.SetActive(true);
    }

    public void QuickInit(int damage) { QuickInit(new[] { damage }); }
    public void QuickInit(int[] damage) {
        CommonInit();
        if (UIController.instance.state == "ATTACKING") return;

        for (int i = 0; i < targetNumber; i++) {
            LaunchInstance();
            allFightUiInstances[allFightUiInstances.Count - 1].QuickInit(targetIDs[i], UIController.instance.encounter.EnabledEnemies[targetIDs[i]], damage[i]);
        }

        if (boundFightUiInstances.Count == 0)
            HideAttackingUI();

        UIController.PlaySoundSeparate("slice");
        for (int i = 0; i < targetIDs.Length; i++)
            allFightUiInstances[allFightUiInstances.Count - 1 - (targetIDs.Length - 1 - i)].StopAction(2.2f);
    }

    public void StopAction(float atkMult = -2) {
        if (stopped)
            return;
        stopped = true;
        foreach (FightUI fight in boundFightUiInstances)
            fight.StopAction(atkMult);
        line.SetAnimation(lineAnim, 1 / 12f);
        UIController.PlaySoundSeparate("slice");
    }

    public int GetDamage(EnemyController enemy, float atkMult) {
        if (enemy.presetDmg != DAMAGE_NOT_SET) {
            int dmg = enemy.presetDmg;
            enemy.ResetPresetDamage();
            return dmg;
        }
        if (atkMult == -2)
            atkMult = GetAtkMult();
        if (atkMult < 0)
            return -1;
        int damage = (int)Mathf.Round(((PlayerCharacter.instance.WeaponATK + PlayerCharacter.instance.ATK - enemy.Defense) + Random.value * 2) * atkMult);
        if (damage < 0)
            return 0;
        if (GlobalControls.crate)
            damage = -damage;
        return damage;
    }

    public float GetAtkMult() {
        if (!stopped) return -1.0f;
        if (Mathf.Abs(targetRt.anchoredPosition.x) <= 12)
            return 2.2f;
        float mult = 2.0f - 2.0f * Mathf.Abs(targetRt.anchoredPosition.x * 2.0f / GetComponent<RectTransform>().rect.width);
        if (mult < 0)
            mult = 0;
        return mult;
    }

    public bool Finished() {
        if (!stopped)
            return targetRt.anchoredPosition.x < borderX;
        return boundFightUiInstances.Count == 0 || boundFightUiInstances.All(fight => fight.Finished());
    }

    public void InitFade() {
        if (finishingFade) return;
        finishingFade = true;
        multiHit = false;
        targetRt.GetComponent<Image>().enabled = false;
    }

    public void HideAttackingUI() {
        if (allFightUiInstances.Count == 0)
            gameObject.SetActive(false);
        else {
            gameObject.GetComponent<Image>().enabled = false;
            targetRt.GetComponent<Image>().enabled = false;
        }
    }

    public void DestroyAllAttackInstances(EnemyController enemy) {
        for (int i = allFightUiInstances.Count - 1; i >= 0; i--) {
            FightUI f = allFightUiInstances[i];
            if (f.enemy != enemy) continue;
            int boundID = boundFightUiInstances.IndexOf(f);
            if (boundID != -1) boundFightUiInstances.Remove(f);
            allFightUiInstances.Remove(f);
            Destroy(f.gameObject);
        }

        if (boundFightUiInstances.Count == 0)
            HideAttackingUI();
    }

    // Update is called once per frame
    private void Update() {
        // do not update the attack UI if the ATTACKING state is frozen
        if (UIController.instance.frozenState != "PAUSE")
            return;

        for (int i = 0; i < allFightUiInstances.Count; i++)
            if (!boundFightUiInstances.Contains(allFightUiInstances[i]))
                if (allFightUiInstances[i].Finished()) {
                    Destroy(allFightUiInstances[i].gameObject);
                    allFightUiInstances.RemoveAt(i);
                    i--;

                    if (boundFightUiInstances.Count == 0)
                        HideAttackingUI();
                }

        if (finishingFade) {
            float resizeProg = 1.0f - ArenaManager.instance.getProgress();
            SetAlpha(resizeProg);
            if (resizeProg != 0.0f) return;
            while (boundFightUiInstances.Count != 0) {
                allFightUiInstances.Remove(boundFightUiInstances[boundFightUiInstances.Count - 1]);
                Destroy(boundFightUiInstances[boundFightUiInstances.Count - 1].gameObject);
                boundFightUiInstances.RemoveAt(boundFightUiInstances.Count - 1);
            }
            gameObject.GetComponent<Image>().enabled = true;
            finishingFade                            = false;
            gameObject.SetActive(false);
            return;
        }
        if (boundFightUiInstances.Count != 0) {
            bool pass = boundFightUiInstances.All(t => t.slice.animcomplete && !t.slice.keyframes.enabled && stopped && t.waitingToFade);
            if (pass && boundFightUiInstances.All(fightUi => !fightUi.shakeInProgress))
                InitFade();
        }

        if (stopped || UIController.instance.state != "ATTACKING")
            return;

        float mv = xSpeed * Time.deltaTime;
        targetRt.anchoredPosition = new Vector2(targetRt.anchoredPosition.x + mv, 0);
        if (!Finished() || boundFightUiInstances.Count == 0) return;
        stopped = true;
        StationaryMissScript smc = Resources.Load<StationaryMissScript>("Prefabs/StationaryMiss");
        foreach (FightUI fightUi in boundFightUiInstances) {
            fightUi.enemy.HandleAttack(-1);
            StationaryMissScript smc2 = Instantiate(smc);
            if (fightUi.enemy.NoAttackMissText != null)
                smc2.SetText(fightUi.enemy.NoAttackMissText);
            smc2.transform.SetParent(GameObject.Find("Canvas").transform);
            smc2.setPosition(fightUi.transform.position.x, fightUi.transform.position.y + fightUi.eneSize.y / 2 + 40);
        }
        InitFade();
    }
}