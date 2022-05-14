using UnityEngine;

public class LuaButtons
{
    public static LuaSpriteController fightbtn
    {
        get
        {
            return LuaSpriteController.GetOrCreate(UIController.instance.fightButton.gameObject);
        }
    }

    public static LuaSpriteController actbtn
    {
        get
        {
            return LuaSpriteController.GetOrCreate(UIController.instance.actButton.gameObject);
        }
    }

    public static LuaSpriteController itembtn
    {
        get
        {
            return LuaSpriteController.GetOrCreate(UIController.instance.itemButton.gameObject);
        }
    }

    public static LuaSpriteController mercybtn
    {
        get
        {
            return LuaSpriteController.GetOrCreate(UIController.instance.mercyButton.gameObject);
        }
    }

    public string GetCurrentButton()
    {
        switch (UIController.instance.action)
        {
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

    public void BlockButton(string btn)
    {
        UIController.BlockButton(btn);
    }

    public void UnblockButton(string btn)
    {
        UIController.UnblockButton(btn);
    }

    public void MoveButtonAndPlayer(string btn, float newX, float newY)
    {
        UnityEngine.UI.Image image;
        UIController.Actions action;
        switch (btn)
        {
            case "FIGHT":
                image = UIController.instance.fightButton;
                action = UIController.Actions.FIGHT;
                break;
            case "ACT":
                image = UIController.instance.actButton;
                action = UIController.Actions.ACT;
                break;
            case "ITEM":
                image = UIController.instance.itemButton;
                action = UIController.Actions.ITEM;
                break;
            case "MERCY":
                image = UIController.instance.mercyButton;
                action = UIController.Actions.MERCY;
                break;
            default:
                throw new CYFException("MoveButtonWithPlayer() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }

        if (UIController.instance.action != action)
            return;

        image.transform.localPosition = new Vector2(newX, newY);
        PlayerController.instance.SetPosition(UIController.instance.FindPlayerOffsetForAction(action).x, UIController.instance.FindPlayerOffsetForAction(action).y, true);
    }

    public void SetButtonActiveSprite(string btn, string sprite)
    {
        if (sprite == "null")
            throw new CYFException("You can't set a sprite as nil!");

        switch (btn)
        {
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

    public void ResetButtonActiveSprite(string btn)
    {
        switch (btn)
        {
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