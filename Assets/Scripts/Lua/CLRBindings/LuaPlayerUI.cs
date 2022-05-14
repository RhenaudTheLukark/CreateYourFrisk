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

    public static LuaSpriteController fightbtn {
        get {
            return LuaSpriteController.GetOrCreate(UIController.instance.fightButton.gameObject);
        }
    }

    public static LuaSpriteController actbtn {
        get {
            return LuaSpriteController.GetOrCreate(UIController.instance.actButton.gameObject);
        }
    }

    public static LuaSpriteController itembtn {
        get {
            return LuaSpriteController.GetOrCreate(UIController.instance.itemButton.gameObject);
        }
    }

    public static LuaSpriteController mercybtn {
        get {
            return LuaSpriteController.GetOrCreate(UIController.instance.mercyButton.gameObject);
        }
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
        hpbar.SetSprites("px");
        hpbar.Resize(1, 20);

        hptext.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        hptext.progressmode = "NONE";
        hptext.HideBubble();
        hptext.color = new float[] { 1, 1, 1, 1 };

        ui.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
        ui.setNamePosition();
        ui.setMaxHP();
    }

    public string GetCurrentButton() {
        switch (UIController.instance.action) {
            case UIController.Actions.FIGHT:
                return "FIGHT";
            case UIController.Actions.ACT:
                return "ACT";
            case UIController.Actions.ITEM:
                return "ITEM";
            case UIController.Actions.MERCY:
                return "MERCY";
            default:
                return "UNKNOWN";
        }
    }

    public void EnableButton(string btn) {
        UIController.EnableButton(btn);
    }

    public void DisableButton(string btn) {
        UIController.DisableButton(btn);
    }

    public void ResetButtonX(string btn) {
        switch (btn) {
            case "FIGHT":
                UIController.instance.fightButton.transform.localPosition = new Vector3(-289, UIController.instance.fightButton.transform.localPosition.y);
                break;
            case "ACT":
                UIController.instance.actButton.transform.localPosition = new Vector3(-136, UIController.instance.transform.localPosition.y);
                break;
            case "ITEM":
                UIController.instance.itemButton.transform.localPosition = new Vector3(134, UIController.instance.transform.localPosition.y);
                break;
            case "MERCY":
                UIController.instance.mercyButton.transform.localPosition = new Vector3(289, UIController.instance.mercyButton.transform.localPosition.y);
                break;
            default:
                throw new CYFException("ResetButtonX() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void ResetButtonY(string btn) {
        switch (btn) {
            case "FIGHT":
                UIController.instance.fightButton.transform.localPosition = new Vector3(UIController.instance.fightButton.transform.localPosition.x, 0);
                break;
            case "ACT":
                UIController.instance.actButton.transform.localPosition = new Vector3(UIController.instance.transform.localPosition.x, 0);
                break;
            case "ITEM":
                UIController.instance.itemButton.transform.localPosition = new Vector3(UIController.instance.transform.localPosition.x, 0);
                break;
            case "MERCY":
                UIController.instance.mercyButton.transform.localPosition = new Vector3(UIController.instance.mercyButton.transform.localPosition.x, 0);
                break;
            default:
                throw new CYFException("ResetButtonY() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public float GetPlayerX(string btn) {
        switch (btn)
        {
            case "FIGHT":
                return UIController.instance.playerOffsets[0].x;
            case "ACT":
                return UIController.instance.playerOffsets[1].x;
            case "ITEM":
                return UIController.instance.playerOffsets[2].x;
            case "MERCY":
                return UIController.instance.playerOffsets[3].x;
            default:
                throw new CYFException("GetPlayerX() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public float GetPlayerY(string btn) {
        switch (btn) {
            case "FIGHT":
                return UIController.instance.playerOffsets[0].y;
            case "ACT":
                return UIController.instance.playerOffsets[1].y;
            case "ITEM":
                return UIController.instance.playerOffsets[2].y;
            case "MERCY":
                return UIController.instance.playerOffsets[3].y;
            default:
                throw new CYFException("GetPlayerY() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void SetPlayerX(string btn, float newX) {
        switch (btn) {
            case "FIGHT":
                UIController.instance.playerOffsets[0].x = newX;
                break;
            case "ACT":
                UIController.instance.playerOffsets[1].x = newX;
                break;
            case "ITEM":
                UIController.instance.playerOffsets[2].x = newX;
                break;
            case "MERCY":
                UIController.instance.playerOffsets[3].x = newX;
                break;
            default:
                throw new CYFException("SetPlayerX() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void SetPlayerY(string btn, float newY) {
        switch (btn) {
            case "FIGHT":
                UIController.instance.playerOffsets[0].y = newY;
                break;
            case "ACT":
                UIController.instance.playerOffsets[1].y = newY;
                break;
            case "ITEM":
                UIController.instance.playerOffsets[2].y = newY;
                break;
            case "MERCY":
                UIController.instance.playerOffsets[3].y = newY;
                break;
            default:
                throw new CYFException("SetPlayerY() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void ResetPlayerX(string btn) {
        switch (btn) {
            case "FIGHT":
                UIController.instance.playerOffsets[0].x = 337;
                break;
            case "ACT":
                UIController.instance.playerOffsets[1].x = 338;
                break;
            case "ITEM":
                UIController.instance.playerOffsets[2].x = 227;
                break;
            case "MERCY":
                UIController.instance.playerOffsets[3].x = 226;
                break;
            default:
                throw new CYFException("ResetPlayerX() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void ResetPlayerY(string btn) {
        switch (btn) {
            case "FIGHT":
                UIController.instance.playerOffsets[0].y = 25;
                break;
            case "ACT":
                UIController.instance.playerOffsets[1].y = 25;
                break;
            case "ITEM":
                UIController.instance.playerOffsets[2].y = 25;
                break;
            case "MERCY":
                UIController.instance.playerOffsets[3].y = 25;
                break;
            default:
                throw new CYFException("ResetPlayerY() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void SetButtonActiveSprite(string btn, string sprite) {
        if (sprite == "null")
            throw new CYFException("You can't set a sprite as nil!");

        switch (btn) {
            case "FIGHT":
                UIController.fightButtonSprite = SpriteRegistry.Get(sprite);
                break;
            case "ACT":
                UIController.actButtonSprite = SpriteRegistry.Get(sprite);
                break;
            case "ITEM":
                UIController.itemButtonSprite = SpriteRegistry.Get(sprite);
                break;
            case "MERCY":
                UIController.mercyButtonSprite = SpriteRegistry.Get(sprite);
                break;
            default:
                throw new CYFException("SetButtonActiveSprite() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }

    public void ResetButtonActiveSprite(string btn) {
        switch (btn) {
            case "FIGHT":
                if (GlobalControls.crate)
                    UIController.fightButtonSprite = SpriteRegistry.Get("UI/Buttons/gifhtbt_1");
                else
                    UIController.fightButtonSprite = SpriteRegistry.Get("UI/Buttons/fightbt_1");
                break;
            case "ACT":
                if (GlobalControls.crate)
                    UIController.actButtonSprite = SpriteRegistry.Get("UI/Buttons/catbt_1");
                else
                    UIController.actButtonSprite = SpriteRegistry.Get("UI/Buttons/actbt_1");
                break;
            case "ITEM":
                if (GlobalControls.crate)
                    UIController.itemButtonSprite = SpriteRegistry.Get("UI/Buttons/tembt_1");
                else
                    UIController.itemButtonSprite = SpriteRegistry.Get("UI/Buttons/itembt_1");
                break;
            case "MERCY":
                if (GlobalControls.crate)
                    UIController.mercyButtonSprite = SpriteRegistry.Get("UI/Buttons/mecrybt_1");
                else
                    UIController.mercyButtonSprite = SpriteRegistry.Get("UI/Buttons/mercybt_1");
                break;
            default:
                throw new CYFException("ResetButtonActiveSprite() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
    }
}
