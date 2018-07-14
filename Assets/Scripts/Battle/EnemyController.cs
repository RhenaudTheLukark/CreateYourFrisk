using UnityEngine;

/// <summary>
/// Class used to hold enemies' important data and some useful core functions needed for the enemy script to be handled.
/// </summary>
public class EnemyController : MonoBehaviour {
    internal Sprite textBubbleSprite; // Bubble sprite
    internal Vector2 textBubblePos;   // Position of the text bubble.

    public virtual string Name { get; set; }
    public virtual string[] ActCommands { get; set; }
    public virtual string[] Comments { get; set; }
    public virtual string[] Dialogue { get; set; }
    public virtual string CheckData { get; set; }     // Description text used for the "CHECK" option.
    public virtual int HP { get; set; }
    public virtual int MaxHP { get; set; }
    public virtual int Attack { get; set; }
    public virtual int Defense { get; set; }
    public virtual int XP { get; set; }
    public virtual int Gold { get; set; }
    public virtual bool CanSpare { get; set; }
    public virtual bool CanCheck { get; set; }
    public virtual bool Unkillable { get; set; }
    public virtual string DialogBubble { get; set; }
    public virtual string Font { get; set; }
    public virtual string Voice { get; set; }

    // Computes where the bubble must be placed when the enemy talks.
    public Vector2 DialogBubblePosition {
        get {
            // Fetch the bubble's sprite.
            Sprite diagBubbleSpr = SpriteRegistry.Get(DialogBubble);
            RectTransform t = GetComponent<RectTransform>();
            // Most bubbles have a side where a bubble should be: this block 
            // places the bubble where it should be depending on this parameter.
            if (diagBubbleSpr.name.StartsWith("right"))        textBubblePos = new Vector2(t.rect.width + 5, (-t.rect.height + diagBubbleSpr.rect.height) / 2);
            else if (diagBubbleSpr.name.StartsWith("left"))    textBubblePos = new Vector2(-diagBubbleSpr.rect.width - 5, (-t.rect.height + diagBubbleSpr.rect.height) / 2);
            else if (diagBubbleSpr.name.StartsWith("top"))     textBubblePos = new Vector2((t.rect.width - diagBubbleSpr.rect.width) / 2, diagBubbleSpr.rect.height + 5);
            else if (diagBubbleSpr.name.StartsWith("bottom"))  textBubblePos = new Vector2((t.rect.width - diagBubbleSpr.rect.width) / 2, -t.rect.height - 5);
            else                                               textBubblePos = new Vector2(t.rect.width + 5, (t.rect.height - diagBubbleSpr.rect.height) / 2); // rightside default
            return textBubblePos;
        }
    }

    // Launches functions related to the enemy's ACT choices.
    public void Handle(string command) {
        string cmd = command.ToUpper().Trim();
        if (CanCheck && cmd.Equals("CHECK"))  HandleCheck();
        else                                  HandleCustomCommand(cmd);
    }

    // Handles what happens after the player attacked an enemy.
    // This part is never launched as this function is overridden by the class LuaEnemyController.
    // hitstatus -1: You didn't press anything while attacking.
    // hitstatus  0: You dealt no damage.
    // hitstatus  1: You dealt any amount of damage.
    public virtual void HandleAttack(int hitStatus) {
        UIController.instance.ActionDialogResult(new RegularMessage("Your attack handler\ris missing."), UIController.UIState.ENEMYDIALOGUE);
    }

    // Handles what happens before the player attacks an enemy.
    // This part is never launched as this function is overridden by the class LuaEnemyController.
    public virtual void AttackStarting() {}

    // Handles what happens after the player launched an ACT command on this enemy.
    // This part is never launched as this function is overridden by the class LuaEnemyController.
    protected virtual void HandleCustomCommand(string command) {
        UIController.instance.ActionDialogResult(new RegularMessage("Command handler missing.\nGood job."), UIController.UIState.DEFENDING);
    }

    // Handles what happens after the player used the "CHECK" action of an enemy.
    public virtual void HandleCheck() {
        UIController.instance.ActionDialogResult(new RegularMessage(Name.ToUpper() + " " + Attack + " ATK " + Defense + " DEF\n" + CheckData), UIController.UIState.ENEMYDIALOGUE);
    }

    // Applies a given amount of damage to the enemy.
    public void doDamage(int damage) {
        int newHP = HP - damage;
        HP = newHP;
    }

    // Returns one of the enemy's random defense dialog.
    // This part is never launched as this function is overridden by the class LuaEnemyController.
    public virtual string[] GetDefenseDialog() {
        string[] randoms = new string[] {
            "Check\nit out.",
            "That's\nsome\nsolid\ntext.",
            "More\ntext,\nplease.",
            "We're\ngetting\ncloser.",
            "You\nguys\nSUCK\nat this."
        };
        return new string[] { randoms[Random.Range(0, randoms.Length)] };
    }
}