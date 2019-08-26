using UnityEngine;

public class EnemyController : MonoBehaviour {
    internal Sprite textBubbleSprite;

    internal Vector2 textBubblePos;

    protected UIController ui;

    public virtual string Name { get; set; }
    public virtual string[] ActCommands { get; set; }
    public virtual string[] Comments { get; set; }
    public virtual string[] Dialogue { get; set; }
    public virtual string CheckData { get; set; }
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
    public virtual string DialoguePrefix { get; set; }
    public virtual string Font { get; set; }
    public virtual string Voice { get; set; }

    public Vector2 DialogBubblePosition {
        get {
            Sprite diagBubbleSpr = SpriteRegistry.Get(DialogBubble);
            RectTransform t = GetComponent<RectTransform>();
            if (diagBubbleSpr.name.StartsWith("right"))        textBubblePos = new Vector2(t.rect.width + 5, (-t.rect.height + diagBubbleSpr.rect.height) / 2);
            else if (diagBubbleSpr.name.StartsWith("left"))    textBubblePos = new Vector2(-diagBubbleSpr.rect.width - 5, (-t.rect.height + diagBubbleSpr.rect.height) / 2);
            else if (diagBubbleSpr.name.StartsWith("top"))     textBubblePos = new Vector2((t.rect.width - diagBubbleSpr.rect.width) / 2, diagBubbleSpr.rect.height + 5);
            else if (diagBubbleSpr.name.StartsWith("bottom"))  textBubblePos = new Vector2((t.rect.width - diagBubbleSpr.rect.width) / 2, -t.rect.height - 5);
            else                                               textBubblePos = new Vector2(t.rect.width + 5, (t.rect.height - diagBubbleSpr.rect.height) / 2); // rightside default
            return textBubblePos;
        }
    }

    public void Handle(string command) {
        string cmd = command.ToUpper().Trim();
        if (CanCheck && cmd.Equals("CHECK"))  HandleCheck();
        else                                  HandleCustomCommand(cmd);
    }

    // hitstatus -1: you didn't press anything while attacking
    // hitstatus  0: you dealt no damage
    // hitstatus  1: you dealt any amount of damage
    public virtual void HandleAttack(int hitStatus) { ui.ActionDialogResult(new RegularMessage("Your attack handler\ris missing."), UIController.UIState.ENEMYDIALOGUE); }

    public virtual void AttackStarting() {}

    protected virtual void HandleCustomCommand(string command) { ui.ActionDialogResult(new RegularMessage("Command handler missing.\nGood job."), UIController.UIState.DEFENDING); }

    public virtual void HandleCheck() { ui.ActionDialogResult(new RegularMessage(Name.ToUpper() + " " + Attack + " ATK " + Defense + " DEF\n" + CheckData), UIController.UIState.ENEMYDIALOGUE); }

    public void doDamage(int damage) {
        int newHP = HP - damage;
        HP = newHP;
    }

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