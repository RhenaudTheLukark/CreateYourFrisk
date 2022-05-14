using UnityEngine;

public class LuaPlayerUI {
    private readonly UIStats ui = UIStats.instance;
    private LuaSpriteController bg;

    public LuaSpriteController background {
        get {
            if (bg != null)
                return bg;
            bg = LuaSpriteController.GetOrCreate(Object.FindObjectOfType<BackgroundLoader>().gameObject);
            return bg;
        }
    }


    public LuaTextManager namelv {
        get { return UIStats.instance.nameLevelTextMan; }
    }


    public LuaSpriteController hplabel {
        get { return UIStats.instance.hpLabel; }
    }

    public LifeBarController hpbar {
        get { return UIStats.instance.lifebar; }
    }

    public LuaTextManager hptext {
        get { return UIStats.instance.hpTextMan; }
    }


    public void StopUpdate(bool toggle) {
        ui.stopUIUpdate = toggle;
    }

    public void Hide(bool hide) {
        ui.Hide(hide);
    }

    public void RepositionHPElements() {
        hpbar.transform.position = new Vector3(ui.hpLabel.absx + ui.hpLabel.spr.GetComponent<RectTransform>().sizeDelta.x + 8, hpbar.transform.position.y, hpbar.transform.position.z);
        ui.hpTextMan.transform.position = new Vector3(hpbar.background.absx + hpbar.backgroundRt.sizeDelta.x + 14, ui.hpTextMan.transform.position.y, ui.hpTextMan.transform.position.z);
    }

    public void Reset() {
        try {
            background.Set("bg");
            background.color = new float[] { 1, 1, 1, 1 };
        } catch (CYFException) {
            // Background failed loading, no need to do anything.
            UnitaleUtil.Warn("No background file found. Using empty background.");
        }
        background.Scale(640 / background.width, 480 / background.height);

        namelv.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        namelv.progressmode = "NONE";
        namelv.HideBubble();
        namelv.color = new float[] { 1, 1, 1, 1 };

        hplabel.Set(GlobalControls.crate ? "UI/spr_phname_0" : "UI/spr_hpname_0");
        hplabel.SetPivot(0, 0);
        hplabel.SetAnchor(0, 0);
        hplabel.Scale(1, 1);

        hpbar.RemoveOutline();
        hpbar.background.SetPivot(0, 0);
        hpbar.background.SetAnchor(0, 0);
        hpbar.background.color = new float[] { 1, 0, 0, 1 };
        hpbar.fill.color = new float[] { 1, 1, 0, 1 };
        hpbar.SetSprites("bar-px");
        hpbar.Resize(1, 20);

        hptext.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        hptext.progressmode = "NONE";
        hptext.HideBubble();
        hptext.color = new float[] { 1, 1, 1, 1 };

        ui.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
        ui.setNamePosition();
        ui.setMaxHP();
    }
}
