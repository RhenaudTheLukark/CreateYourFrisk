using UnityEngine;

public class UIStats : MonoBehaviour {
    public static UIStats instance;

    public LuaTextManager nameLevelTextMan;
    public LuaTextManager hpTextMan;
    public LifeBarController lifebar;
    public LuaSpriteController hpLabel;
    public GameObject hpRect;
    public bool stopUIUpdate = false;
    public bool hiddenUI;

    private bool initialized;

    private void Awake() { instance = this; }

    private void Start() {
        lifebar = gameObject.GetComponentInChildren<LifeBarController>();

        nameLevelTextMan = GameObject.Find("NameLv").GetComponent<LuaTextManager>();
        nameLevelTextMan.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        nameLevelTextMan.progressmode = "NONE";
        nameLevelTextMan.HideBubble();
        nameLevelTextMan.SetCaller(EnemyEncounter.script);

        hpRect = GameObject.Find("HPRect");
        hpLabel = LuaSpriteController.GetOrCreate(GameObject.Find("HPLabel"));

        hpTextMan = GameObject.Find("HPText").GetComponent<LuaTextManager>();
        hpTextMan.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        hpTextMan.progressmode = "NONE";
        hpTextMan.HideBubble();
        hpTextMan.SetCaller(EnemyEncounter.script);
        initialized = true;
        setMaxHP();
        setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
    }

    public void setPlayerInfo(string newName, int newLv) {
        if (!initialized || stopUIUpdate) return;
        nameLevelTextMan.enabled = true;
        nameLevelTextMan.SetText(new TextMessage(newName.ToUpper() + "  LV " + newLv, false, true));
        setNamePosition();

        nameLevelTextMan.enabled = false;
    }

    public void setNamePosition() {
        if (stopUIUpdate) return;
        nameLevelTextMan.MoveTo(0, -11);
        hpLabel.MoveTo(0, -9);
        lifebar.background.MoveTo(31, -14);
        hpTextMan.MoveTo(0, -11);
        setMaxHP();
        hpRect.transform.localPosition = new Vector3(PlayerCharacter.instance.Name.Length > 6 ? 286 : 215, 0, 0);
    }

    public void setHP(float hpCurrent) {
        if (!initialized || stopUIUpdate) return;
        float hpMax  = PlayerCharacter.instance.MaxHP,
              hpFrac = hpCurrent / hpMax;
        lifebar.SetInstant(hpFrac);
        int    count      = UnitaleUtil.DecimalCount(hpCurrent);
        string sHpCurrent = hpCurrent < 10 ? "0" + hpCurrent.ToString("F" + count) : hpCurrent.ToString("F" + count);
        string sHpMax     = hpMax     < 10 ? "0" + hpMax : "" + hpMax;
        hpTextMan.SetText(new TextMessage(sHpCurrent + " / " + sHpMax, false, true));
    }

    public void setMaxHP() {
        if (!initialized || stopUIUpdate) return;
        if (lifebar.background.spritename == "bar-px")
            lifebar.Resize(Mathf.Min(120, PlayerCharacter.instance.MaxHP * 1.2f), 20);
        hpTextMan.MoveToAbs(lifebar.background.absx + lifebar.backgroundRt.sizeDelta.x + 14, hpTextMan.transform.position.y);
        setHP(PlayerCharacter.instance.HP);
    }

    public void Hide(bool hide) {
        int alpha = hide ? 0 : 1;

        nameLevelTextMan.alpha = alpha;
        hpTextMan.alpha = alpha;
        lifebar.fill.alpha = alpha;
        lifebar.background.alpha = alpha;
        hpLabel.alpha = alpha;

        UIController.instance.fightButton.color = new Color(1, 1, 1, alpha);
        UIController.instance.actButton.color = new Color(1, 1, 1, alpha);
        UIController.instance.itemButton.color = new Color(1, 1, 1, alpha);
        UIController.instance.mercyButton.color = new Color(1, 1, 1, alpha);

        hiddenUI = hide;
    }
}