using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour {
    public static UIStats instance;

    private LuaTextManager nameLevelTextMan;
    private LuaTextManager hpTextMan;
    private LifeBarController lifebar;
    private RectTransform lifebarRt;
    private GameObject hpRect;

    private bool initialized;

    private void Awake() { instance = this; }

    private void Start() {
        lifebar = gameObject.GetComponentInChildren<LifeBarController>();
        lifebarRt = lifebar.GetComponent<RectTransform>();

        nameLevelTextMan = GameObject.Find("NameLv").GetComponent<LuaTextManager>();
        nameLevelTextMan.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        nameLevelTextMan.progressmode = "NONE";
        nameLevelTextMan.HideBubble();
        nameLevelTextMan.SetCaller(EnemyEncounter.script);

        hpRect = GameObject.Find("HPRect");

        hpTextMan = GameObject.Find("HPText").GetComponent<LuaTextManager>();
        hpTextMan.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME), false);
        hpTextMan.progressmode = "NONE";
        hpTextMan.HideBubble();
        hpTextMan.SetCaller(EnemyEncounter.script);
        initialized = true;
        setMaxHP();
        setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
    }

    public void setPlayerInfo(string newName, int newLv) {
        if (!initialized) return;
        nameLevelTextMan.enabled = true;
        nameLevelTextMan.SetText(new TextMessage(newName.ToUpper() + "  LV " + newLv, false, true));
        setNamePosition();

        nameLevelTextMan.enabled = false;
    }

    public void setNamePosition() {
        int textLength = 0;
        foreach (Image reference in nameLevelTextMan.letterReferences) {
            if (reference != null)
                textLength++;
        }

        hpRect.transform.position = new Vector3(hpRect.transform.parent.position.x + (((textLength > 13) || (PlayerCharacter.instance.Name.Length > 6) ) ? 286.1f : 215.1f), hpRect.transform.position.y, hpRect.transform.position.z);
    }

    public void setHP(float hpCurrent) {
        if (!initialized) return;
        float hpMax  = PlayerCharacter.instance.MaxHP,
              hpFrac = hpCurrent / hpMax;
        lifebar.setInstant(hpFrac);
        int    count      = UnitaleUtil.DecimalCount(hpCurrent);
        string sHpCurrent = hpCurrent < 10 ? "0" + hpCurrent.ToString("F" + count) : hpCurrent.ToString("F" + count);
        string sHpMax     = hpMax     < 10 ? "0" + hpMax : "" + hpMax;
        hpTextMan.SetText(new TextMessage(sHpCurrent + " / " + sHpMax, false, true));
    }

    public void setMaxHP() {
        if (!initialized) return;
        lifebarRt.sizeDelta = new Vector2(Mathf.Min(120, PlayerCharacter.instance.MaxHP * 1.2f), lifebarRt.sizeDelta.y);
        setHP(PlayerCharacter.instance.HP);
    }
}