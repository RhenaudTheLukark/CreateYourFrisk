using UnityEngine;

public class UIStats : MonoBehaviour {
    public static UIStats instance;

    private GameObject nameLevelTextManParent;
    private TextManager nameLevelTextMan;
    private GameObject hpTextManParent;
    private TextManager hpTextMan;
    private LifeBarController lifebar;
    private RectTransform lifebarRt;
    private GameObject hpRect;

    private bool initialized = false;

    void Awake() { instance = this; }

    private void Start() {
        lifebar = gameObject.GetComponentInChildren<LifeBarController>();
        lifebarRt = lifebar.GetComponent<RectTransform>();

        nameLevelTextManParent = GameObject.Find("NameLv");
        nameLevelTextManParent.transform.position = new Vector3(nameLevelTextManParent.transform.position.x, nameLevelTextManParent.transform.position.y - 1, nameLevelTextManParent.transform.position.z);
        hpTextManParent = GameObject.Find("HPTextParent");
        hpTextManParent.transform.position = new Vector3(hpTextManParent.transform.position.x, hpTextManParent.transform.position.y - 1, hpTextManParent.transform.position.z);

        nameLevelTextMan = nameLevelTextManParent.AddComponent<TextManager>();
        hpTextMan = hpTextManParent.AddComponent<TextManager>();
        hpRect = GameObject.Find("HPRect");

        hpTextMan.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        initialized = true;
        setMaxHP();
        setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
    }

    public void setPlayerInfo(string name, int lv) {
        if (initialized) {
            nameLevelTextMan.enabled = true;
            nameLevelTextMan.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
            nameLevelTextMan.SetText(new TextMessage(name.ToUpper() + "  LV " + lv, false, true));
            if (PlayerCharacter.instance.Name.Length > 6)
                hpRect.transform.position = new Vector3(hpRect.transform.parent.position.x + 286.1f, hpRect.transform.position.y, hpRect.transform.position.z);
            else
                hpRect.transform.position = new Vector3(hpRect.transform.parent.position.x + 215.1f, hpRect.transform.position.y, hpRect.transform.position.z);

            nameLevelTextMan.enabled = false;
        }
    }

    public void setHP(float hpCurrent) {
        if (initialized) {
            float hpMax = (float)PlayerCharacter.instance.MaxHP,
                  hpFrac = hpCurrent / hpMax;
            lifebar.setInstant(hpFrac);
            int count = UnitaleUtil.DecimalCount(hpCurrent);
            string sHpCurrent = hpCurrent < 10 ? "0" + hpCurrent.ToString("F" + count) : hpCurrent.ToString("F" + count);
            string sHpMax = hpMax < 10 ? "0" + hpMax : "" + hpMax;
            hpTextMan.SetText(new TextMessage(sHpCurrent + " / " + sHpMax, false, true));
        }
    }

    public void setMaxHP() {
        if (initialized) {
            if (PlayerCharacter.instance.MaxHP >= 100)
                lifebarRt.sizeDelta = new Vector2(120, lifebarRt.sizeDelta.y);
            else
                lifebarRt.sizeDelta = new Vector2(PlayerCharacter.instance.MaxHP * 1.2f, lifebarRt.sizeDelta.y);
            setHP(PlayerCharacter.instance.HP);
        }
    }
}