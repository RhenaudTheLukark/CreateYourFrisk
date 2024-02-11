using MoonSharp.Interpreter;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

    public LuaTextManager maintext {
        get { return UIController.instance.mainTextManager; }
    }

    public LuaSpriteController mugshot {
        get { return UIController.instance.mainTextManager.mugshot; }
    }

    public LuaSpriteController mugshotmask {
        get { return UIController.instance.mainTextManager.mugshotMask; }
    }

    public DynValue enemylifebarlist {
        get { return DynValue.NewTable(null, UIController.instance.arenaParent.GetComponentsInChildren<LifeBarController>().Select(p => UserData.Create(p)).ToArray()); }
    }


    public static LuaSpriteController fightbtn {
        get { return LuaSpriteController.GetOrCreate(UIController.instance.fightButton.gameObject); }
    }

    public static LuaSpriteController actbtn {
        get { return LuaSpriteController.GetOrCreate(UIController.instance.actButton.gameObject); }
    }

    public static LuaSpriteController itembtn {
        get { return LuaSpriteController.GetOrCreate(UIController.instance.itemButton.gameObject); }
    }

    public static LuaSpriteController mercybtn {
        get { return LuaSpriteController.GetOrCreate(UIController.instance.mercyButton.gameObject); }
    }


    public static LuaCYFObject root {
        get { return new LuaCYFObject(Object.FindObjectOfType<Canvas>().transform); }
    }


    public void StopUpdate(bool toggle) {
        ui.stopUIUpdate = toggle;
    }

    public void Hide(bool hide) {
        ui.Hide(hide);
    }

    public void RepositionHPElements() {
        hpbar.transform.position = new Vector3(ui.hpLabel.absx + ui.hpLabel.spr.GetComponent<RectTransform>().sizeDelta.x + 8, hpbar.transform.position.y, hpbar.transform.position.z);
        ui.hpTextMan.MoveToAbs(hpbar.background.absx + hpbar.backgroundRt.sizeDelta.x + 14, ui.hpTextMan.transform.position.y);
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

        EnableButton("FIGHT");
        ResetButtonPosition("FIGHT");
        ResetPlayerPosOnButton("FIGHT");
        ResetButtonActiveSprite("FIGHT");

        EnableButton("ACT");
        ResetButtonPosition("ACT");
        ResetPlayerPosOnButton("ACT");
        ResetButtonActiveSprite("ACT");

        EnableButton("ITEM");
        ResetButtonPosition("ITEM");
        ResetPlayerPosOnButton("ITEM");
        ResetButtonActiveSprite("ITEM");

        EnableButton("MERCY");
        ResetButtonPosition("MERCY");
        ResetPlayerPosOnButton("MERCY");
        ResetButtonActiveSprite("MERCY");
    }


    public string GetCurrentButton() {
        return UIController.instance.action.ToString();
    }

    public void EnableButton(string btn) {
        UIController.EnableButton(btn);
    }

    public void DisableButton(string btn) {
        UIController.DisableButton(btn);
        UpdateButtons();
    }

    public void ResetButtonPosition(string btn, bool resetX = true, bool resetY = true) {
        Image image;
        Vector2 basePos;
        if (!UIController.instance.buttonDictionary.TryGetValue(btn, out image))
            throw new CYFException("ResetButtonPosition() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        UIController.instance.buttonBasePositions.TryGetValue(btn, out basePos);
        image.rectTransform.anchoredPosition = new Vector3(resetX ? basePos.x : image.transform.position.x, resetY ? basePos.y : image.transform.position.y);
        UpdateButtons();
    }

    public float GetPlayerXPosOnButton(string btn) {
        UIController.Actions action;
        try {
            action = (UIController.Actions)Enum.Parse(typeof(UIController.Actions), btn);
            if (action == UIController.Actions.NONE)
                throw new CYFException("GetPlayerXPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        } catch {
            throw new CYFException("GetPlayerXPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }

        return UIController.instance.playerOffsets[(int)action].x;
    }

    public float GetPlayerYPosOnButton(string btn) {
        UIController.Actions action;
        try {
            action = (UIController.Actions)Enum.Parse(typeof(UIController.Actions), btn);
            if (action == UIController.Actions.NONE)
                throw new CYFException("GetPlayerYPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        } catch {
            throw new CYFException("GetPlayerYPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }

        return UIController.instance.playerOffsets[(int)action].y;
    }

    public void SetPlayerXPosOnButton(string btn, float newX) {
        UIController.Actions action;
        try {
            action = (UIController.Actions)Enum.Parse(typeof(UIController.Actions), btn);
            if (action == UIController.Actions.NONE)
                throw new CYFException("SetPlayerXPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        } catch {
            throw new CYFException("SetPlayerXPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }

        UIController.instance.playerOffsets[(int)action].x = newX;

        UpdateButtons();
    }

    public void SetPlayerYPosOnButton(string btn, float newY) {
        UIController.Actions action;
        try {
            action = (UIController.Actions)Enum.Parse(typeof(UIController.Actions), btn);
            if (action == UIController.Actions.NONE)
                throw new CYFException("SetPlayerYPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        } catch {
            throw new CYFException("SetPlayerYPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }

        UIController.instance.playerOffsets[(int)action].y = newY;

        UpdateButtons();
    }

    public void ResetPlayerPosOnButton(string btn, bool resetX = true, bool resetY = true) {
        UIController.Actions action;
        try {
            action = (UIController.Actions)Enum.Parse(typeof(UIController.Actions), btn);
            if (action == UIController.Actions.NONE)
                throw new CYFException("ResetPlayerPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        } catch {
            throw new CYFException("ResetPlayerPosOnButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }

        Vector2 basePlayerPos;
        UIController.instance.buttonBasePlayerPositions.TryGetValue(btn, out basePlayerPos);

        if (resetX) UIController.instance.playerOffsets[(int)action].x = basePlayerPos.x;
        if (resetY) UIController.instance.playerOffsets[(int)action].y = basePlayerPos.y;

        UpdateButtons();
    }

    public void SetButtonActiveSprite(string btn, string sprite) {
        if (sprite == "null")
            throw new CYFException("You can't set a sprite as nil!");

        switch (btn) {
            case "FIGHT": UIController.fightButtonSprite = SpriteRegistry.Get(sprite); break;
            case "ACT":   UIController.actButtonSprite   = SpriteRegistry.Get(sprite); break;
            case "ITEM":  UIController.itemButtonSprite  = SpriteRegistry.Get(sprite); break;
            case "MERCY": UIController.mercyButtonSprite = SpriteRegistry.Get(sprite); break;
            default:      throw new CYFException("SetButtonActiveSprite() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
        UpdateButtons();
    }

    public void ResetButtonActiveSprite(string btn) {
        switch (btn) {
            case "FIGHT": UIController.fightButtonSprite = SpriteRegistry.Get(GlobalControls.crate ? "UI/Buttons/gifhtbt_1" : "UI/Buttons/fightbt_1"); break;
            case "ACT":   UIController.actButtonSprite   = SpriteRegistry.Get(GlobalControls.crate ? "UI/Buttons/catbt_1"   : "UI/Buttons/actbt_1");   break;
            case "ITEM":  UIController.itemButtonSprite  = SpriteRegistry.Get(GlobalControls.crate ? "UI/Buttons/tembt_1"   : "UI/Buttons/itembt_1");  break;
            case "MERCY": UIController.mercyButtonSprite = SpriteRegistry.Get(GlobalControls.crate ? "UI/Buttons/mecrybt_1" : "UI/Buttons/mercybt_1"); break;
            default:      throw new CYFException("ResetButtonActiveSprite() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
        UpdateButtons();
    }

    public void UpdateButtons() {
        LuaScriptBinder.SetAction(UIController.instance.action.ToString());
    }
}
