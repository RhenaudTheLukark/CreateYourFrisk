using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class FightUIController : MonoBehaviour {
    public static FightUIController instance;
    public List<FightUI> boundFightUiInstances = new List<FightUI> { };
    public List<FightUI> allFightUiInstances = new List<FightUI> { };

    public RectTransform targetRt;
    public int presetDmg = 0;
    public LuaSpriteController line;
    private float borderX;
    private float xSpeed = -450.0f;
    public int[] shakeX; //Modify it in the Editor if needed
    //private int[] shakeX = new int[] { 24, 0, 0, 0, 0, -48, 0, 0, 0, 0, 38, 0, 0, 0, 0, -28, 0, 0, 0, 0, 20, 0, 0, 0, 0, -12, 0, 0, 0, 0, 8, 0, 0, 0, 0, -2, 0, 0, 0, 0};
    private string[] lineAnim = new string[] { "UI/Battle/spr_targetchoice_0", "UI/Battle/spr_targetchoice_1" };
    public string[] sliceAnim; //Modify it in the Editor if needed
    public float sliceAnimFrequency = 1 / 6f;
    public int targetNumber = 1;
    public int[] targetIDs = new int[] { };
    public bool multiHit = false;
    public bool stopped = false;
    private bool finishingFade = false;

    private void LaunchInstance(bool bind = false) {
        GameObject go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/FightInstance"));
        go.GetComponent<FightUI>().transform.SetParent(transform);
        go.GetComponent<FightUI>().transform.SetAsLastSibling();
        if (bind)
            boundFightUiInstances.Add(go.GetComponent<FightUI>());
        allFightUiInstances.Add(go.GetComponent<FightUI>());
    }

    private void Awake() {
        foreach (Transform child in gameObject.transform)
            if (child.name == "FightUILine") {
                line = new LuaSpriteController(child.GetComponent<Image>());
                Start();
                return;
            }
    }

    private void Start() {
        line.Set("UI/Battle/spr_targetchoice_0");
        instance = this;
    }

    public void ChangeTarget(LuaEnemyController target) {
        while (boundFightUiInstances.Count > 1) {
            allFightUiInstances.Remove(boundFightUiInstances[1]);
            Destroy(boundFightUiInstances[1].lifeBar.gameObject);
            Destroy(boundFightUiInstances[1].damageText.gameObject);
            Destroy(boundFightUiInstances[1].gameObject);
            boundFightUiInstances.RemoveAt(1);
        }
        boundFightUiInstances[0].ChangeTarget(target);
    }

    public void setAlpha(float a) {
        Color c = Color.white;
        c.a = a;
        GetComponent<Image>().color = c;
    }

    public void Init() {
        commonInit();
        finishingFade = false;
        stopped = false;
        targetRt.anchoredPosition = new Vector2(GetComponent<RectTransform>().rect.width / 2, 0);
        for (int i = 0; i < targetNumber; i++) {
            LaunchInstance(true);
            boundFightUiInstances[boundFightUiInstances.Count - 1].Init(targetIDs[i]);
        }
        // damageTextRt.position = target.GetComponent<RectTransform>().position;
        setAlpha(1.0f);
    }

    public void commonInit() {
        gameObject.SetActive(true);
        gameObject.GetComponent<Image>().enabled = true;
        line.StopAnimation();
        line.img.gameObject.SetActive(true);
        line.img.GetComponent<Image>().enabled = true;
        borderX = -GetComponent<RectTransform>().rect.width / 2;
    }
    public void commonQuickInit() {
        if (UIController.instance.state != UIController.UIState.ATTACKING) {
            gameObject.GetComponent<Image>().enabled = false;
            targetRt.gameObject.SetActive(false);
        }
    }

    public void quickInit(LuaEnemyController target, int damage = -478294) {
        commonInit();
        commonQuickInit();
        LaunchInstance();
        allFightUiInstances[allFightUiInstances.Count - 1].quickInit(targetIDs[0], target, damage);
        allFightUiInstances[allFightUiInstances.Count - 1].isCoroutine = true;
        StopAction(-2, true);
        allFightUiInstances[allFightUiInstances.Count - 1].StopAction(-2);
        // damageTextRt.position = target.GetComponent<RectTransform>().position;
    }

    public void quickMultiInit(float atkMult, int[] damage) {
        commonInit();
        commonQuickInit();
        for (int i = 0; i < targetIDs.Length; i++) {
            LaunchInstance();
            allFightUiInstances[allFightUiInstances.Count - 1].quickInit(targetIDs[i], UIController.instance.encounter.EnabledEnemies[targetIDs[i]], damage[i]);
            allFightUiInstances[allFightUiInstances.Count - 1].isCoroutine = true;
        }
        StopAction(atkMult, true);
        for (int i = 0; i < targetIDs.Length; i++)
            allFightUiInstances[allFightUiInstances.Count - 1 - (targetIDs.Length - 1 - i)].StopAction(atkMult);
    }

    public void StopAction(float atkMult = -2, bool stopCoroutine = false) {
        if (!stopCoroutine) {
            if (stopped)
                return;
            stopped = true;
            foreach (FightUI fight in boundFightUiInstances)
                fight.StopAction(atkMult);
        }
        line.SetAnimation(lineAnim, 1 / 12f);
        UIController.PlaySoundSeparate(AudioClipRegistry.GetSound("slice"));
    }

    public int getDamage(LuaEnemyController enemy, float atkMult) {
        if (enemy.presetDmg != -1826643) {
            int dmg = enemy.presetDmg;
            enemy.presetDmg = -1826643;
            return dmg;
        }
        if (atkMult == -2)
            atkMult = getAtkMult();
        if (atkMult < 0)
            return -1;
        int damage = (int)Mathf.Round(((PlayerCharacter.instance.WeaponATK + PlayerCharacter.instance.ATK - enemy.Defense) + UnityEngine.Random.value * 2) * atkMult);
        if (damage < 0)
            return 0;
        if (GlobalControls.crate)
            damage = -damage;
        return damage;
    }

    public float getAtkMult() {
        if (stopped)
            if (Mathf.Abs(targetRt.anchoredPosition.x) <= 12)
                return 2.2f;
            else {
                float mult = 2.0f - 2.0f * Mathf.Abs(targetRt.anchoredPosition.x * 2.0f / GetComponent<RectTransform>().rect.width);
                if (mult < 0)
                    mult = 0;
                return mult;
            } else
                return -1.0f;
    }

    public bool Finished() {
        if (!stopped)
            return targetRt.anchoredPosition.x < borderX;
        foreach (FightUI fight in boundFightUiInstances)
            if (!fight.Finished())
                return false;
        return true;
    }

    public void initFade() {
        if (!finishingFade) {
            finishingFade = true;
            multiHit = false;
            line.img.GetComponent<Image>().enabled = false;
            line.StopAnimation();
            //Damage = new int[] { };
            // Arena resizes to a small default size in most regular battles before entering actual defense state
        }
    }

    public void disableImmediate() { gameObject.SetActive(false); }

    // Update is called once per frame
    private void Update() {
        // do not update the attack UI if the ATTACKING state is frozen
        if (UIController.instance.frozenState != UIController.UIState.PAUSE)
            return;

        if (!ArenaManager.instance.firstTurn) {
            for (int i = 0; i < allFightUiInstances.Count; i++)
                if (!boundFightUiInstances.Contains(allFightUiInstances[i]))
                    if (allFightUiInstances[i].Finished()) {
                        allFightUiInstances[i].slice.Remove();
                        if (allFightUiInstances[i].lifeBar) {
                            Destroy(allFightUiInstances[i].lifeBar.gameObject);
                            Destroy(allFightUiInstances[i].damageText.gameObject);
                            Destroy(allFightUiInstances[i].gameObject);
                        }
                        allFightUiInstances.RemoveAt(i);
                        i--;
                    }

            if (finishingFade) {
                float resizeProg = 1.0f - ArenaManager.instance.getProgress();
                setAlpha(resizeProg);
                if (resizeProg == 0.0f) {
                    while (boundFightUiInstances.Count != 0) {
                        allFightUiInstances.Remove(boundFightUiInstances[boundFightUiInstances.Count - 1]);
                        Destroy(boundFightUiInstances[boundFightUiInstances.Count - 1].lifeBar.gameObject);
                        Destroy(boundFightUiInstances[boundFightUiInstances.Count - 1].damageText.gameObject);
                        Destroy(boundFightUiInstances[boundFightUiInstances.Count - 1].gameObject);
                        boundFightUiInstances.RemoveAt(boundFightUiInstances.Count - 1);
                    }
                    targetRt.gameObject.SetActive(true);
                    gameObject.GetComponent<Image>().enabled = true;
                    finishingFade = false;
                    if (allFightUiInstances.Count == 0)
                        gameObject.SetActive(false);
                }
                return;
            } else if (boundFightUiInstances.Count != 0) {
                bool pass = true;
                for (int i = 0; i < boundFightUiInstances.Count; i++)
                    if (!(boundFightUiInstances[i].slice.animcomplete &&!boundFightUiInstances[i].slice.keyframes.enabled && stopped && boundFightUiInstances[i].waitingToFade)) {
                        pass = false;
                        break;
                    }
                if (pass) {
                    int number = 0;
                    for (int i = 0; i < boundFightUiInstances.Count; i++) {
                        if (!boundFightUiInstances[i].shakeInProgress)
                            number++;
                        else break;
                    }
                    if (number == boundFightUiInstances.Count)
                        initFade();
                }
            }

            if (stopped)
                return;

            float mv = xSpeed * Time.deltaTime;
            targetRt.anchoredPosition = new Vector2(targetRt.anchoredPosition.x + mv, 0);
            if (Finished() && /*UIController.instance.inited &&*/ boundFightUiInstances.Count != 0) {
                stopped = true;
                StationaryMissScript smc = Resources.Load<StationaryMissScript>("Prefabs/StationaryMiss");
                for (int i = 0; i < boundFightUiInstances.Count; i++) {
                    boundFightUiInstances[i].enemy.HandleAttack(-1);
                    StationaryMissScript smc2 = Instantiate(smc);
                    if (boundFightUiInstances[i].enemy.NoAttackMissText != null)
                        smc2.SetText(boundFightUiInstances[i].enemy.NoAttackMissText);
                    smc2.transform.SetParent(GameObject.Find("Canvas").transform);
                    if (boundFightUiInstances[i].enemy.NoAttackMissText != null)
                        smc2.setXPosition(boundFightUiInstances[i].enePos.x - 10 * boundFightUiInstances[i].enemy.NoAttackMissText.Length + 20);
                    else smc2.setXPosition(boundFightUiInstances[i].enePos.x);
                }
                initFade();
            }
        }
    }
}