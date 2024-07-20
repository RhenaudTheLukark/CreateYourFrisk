using System;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour {
    public GameObject bubbleObject;

    protected UIController ui;

    public enum BubbleSideEnum { LEFT, RIGHT, UP, DOWN, NONE }
    public BubbleSideEnum GetReverseBubbleSide(BubbleSideEnum val) {
        switch (val) {
            case BubbleSideEnum.LEFT:
                return BubbleSideEnum.RIGHT;
            case BubbleSideEnum.RIGHT:
                return BubbleSideEnum.LEFT;
            case BubbleSideEnum.UP:
                return BubbleSideEnum.DOWN;
            case BubbleSideEnum.DOWN:
                return BubbleSideEnum.UP;
            default:
                return val;
        }
    }

    public void Handle(string command) {
        string cmd = command.ToUpper().Trim();
        if (CanCheck && cmd.Equals("CHECK"))  HandleCheck();
        else                                  HandleCustomCommand(cmd);
    }

    public virtual void HandleCheck() { ui.ActionDialogResult(new RegularMessage(Name.ToUpper() + " " + Attack + " ATK " + Defense + " DEF\n" + CheckData)); }

    public void doDamage(int damage) {
        int newHP = HP - damage;
        HP = newHP;
    }

    private int realPresetDmg = FightUIController.DAMAGE_NOT_SET;
    public int presetDmg {
        set { realPresetDmg = value == FightUIController.DAMAGE_NOT_SET ? realPresetDmg : value; }
        get { return realPresetDmg; }
    }

    internal string scriptName;
    internal ScriptWrapper script;
    internal bool inFight = true; // if false, enemy will no longer be considered as an option in menus and such
    private string lastBubbleName;
    public float xFightAnimShift = 0;
    public LuaSpriteController sprite;
    public float bubbleWidth = 0;
    public int index = -1;
    public Vector2[] offsets = { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };
                               //SliceAnimOffset    BubbleOffset       DamageUIOffset

    internal bool spared;
    internal bool killed;

    public string Name {
        get { return script.GetVar("name").String; }
        set { script.SetVar("name", DynValue.NewString(value)); }
    }

    public string[] ActCommands {
        get {
            DynValue actCmds = script.GetVar("commands");
            string[] tempActCmds;
            int add = 0;
            if (!CanCheck)
                tempActCmds = new string[actCmds.Table.Length];
            else {
                tempActCmds = new string[actCmds.Table.Length + 1];
                tempActCmds[0] = "Check"; // HACK: remove hardcoding of Check, but otherwise gets converted to Tuple? idk
                add = 1;
            }
            for (int i = add; i < actCmds.Table.Length + add; i++)
                tempActCmds[i] = actCmds.Table.Get(i - add + 1).String;
            return tempActCmds;
        }
        set {
            DynValue[] values = new DynValue[value.Length];
            for (int i = 0; i < value.Length; i++)
                values[i] = DynValue.NewString(value[i]);
            script.SetVar("commands", DynValue.NewTuple(values));
        }
    }

    public string[] Comments {
        get {
            DynValue comments = script.GetVar("comments");
            string[] tempComments = new string[comments.Table.Length];
            for (int i = 0; i < comments.Table.Length; i++)
                tempComments[i] = comments.Table.Get(i + 1).String;
            return tempComments;
        }
        set {
            DynValue[] values = new DynValue[value.Length];
            for (int i = 0; i < value.Length; i++)
                values[i] = DynValue.NewString(value[i]);
            script.SetVar("comments", DynValue.NewTuple(values));
        }
    }

    public string[] Dialogue {
        get {
            DynValue randDialogue = script.GetVar("randomdialogue");
            if (randDialogue == null || randDialogue.Table == null)
                return null;
            string[] tempDialogue = new string[randDialogue.Table.Length];
            for (int i = 0; i < randDialogue.Table.Length; i++)
                tempDialogue[i] = randDialogue.Table.Get(i + 1).String;
            return tempDialogue;
        }
        set {
            DynValue[] values = new DynValue[value.Length];
            for (int i = 0; i < value.Length; i++)
                values[i] = DynValue.NewString(value[i]);
            script.SetVar("randomdialogue", DynValue.NewTuple(values));
        }
    }

    public string CheckData {
        get { return script.GetVar("check").String; }
        set { script.SetVar("check", DynValue.NewString(value)); }
    }

    public int HP {
        get {
            if (GlobalControls.retroMode && (int)script.GetVar("hp").Number > MaxHP)
                MaxHP = (int)script.GetVar("hp").Number;
            return (int)script.GetVar("hp").Number;
        }
        set { script.SetVar("hp", DynValue.NewNumber(value)); }
    }

    public int MaxHP {
        get { return (int)script.GetVar("maxhp").Number; }
        set { script.SetVar("maxhp", DynValue.NewNumber(value)); }
    }

    public int Attack {
        get { return (int)script.GetVar("atk").Number; }
        set { script.SetVar("atk", DynValue.NewNumber(value)); }
    }

    public int Defense {
        get { return (int)script.GetVar("def").Number; }
        set { script.SetVar("def", DynValue.NewNumber(value)); }
    }

    public int XP {
        get { return (int)script.GetVar("xp").Number; }
        set { script.SetVar("xp", DynValue.NewNumber(value)); }
    }

    public int Gold {
        get { return (int)script.GetVar("gold").Number; }
        set { script.SetVar("gold", DynValue.NewNumber(value)); }
    }

    public bool CanSpare {
        get {
            DynValue spareVal = script.GetVar("canspare");
            if (spareVal == null)
                return false;
            return spareVal.Boolean;
        }
        set { script.SetVar("canspare", DynValue.NewBoolean(value)); }
    }

    public bool CanCheck {
        get {
            DynValue checkVal = script.GetVar("cancheck");
            if (checkVal == null)
                return true;
            return checkVal.Boolean;
        }
        set { script.SetVar("cancheck", DynValue.NewBoolean(value)); }
    }

    public bool Unkillable {
        get {
            DynValue checkVal = script.GetVar("unkillable");
            if (checkVal == null)
                return false;
            return checkVal.Boolean;
        }
        set { script.SetVar("unkillable", DynValue.NewBoolean(value)); }
    }

    public string DialogBubble {
        get {
            if (script.GetVar("dialogbubble") == null)
                return "UI/SpeechBubbles/right";

            return "UI/SpeechBubbles/" + script.GetVar("dialogbubble").String;
        }
    }

    public string DialoguePrefix {
        get {
            DynValue dialoguePrefix = script.GetVar("dialogueprefix");
            if (dialoguePrefix == null || dialoguePrefix.Type == DataType.Nil)
                return "[effect:rotate]";
            return dialoguePrefix.String;
        }
        set { script.SetVar("dialogueprefix", DynValue.NewString(value)); }
    }

    public string Font {
        get {
            DynValue fontVal = script.GetVar("font");
            if (fontVal == null || fontVal.Type == DataType.Nil)
                return SpriteFontRegistry.UI_MONSTERTEXT_NAME;
            return fontVal.String;
        }
        set { script.SetVar("font", DynValue.NewString(value)); }
    }

    public string Voice {
        get {
            DynValue voiceVal = script.GetVar("voice");
            if (voiceVal == null || voiceVal.Type == DataType.Nil)
                return "";
            return voiceVal.String;
        }
        set { script.SetVar("voice", DynValue.NewString(value)); }
    }

    public string DefenseMissText {
        get { return script.GetVar("defensemisstext").String; }
        set { script.SetVar("defensemisstext", DynValue.NewString(value)); }
    }

    public string NoAttackMissText {
        get { return script.GetVar("noattackmisstext").String; }
        set { script.SetVar("noattackmisstext", DynValue.NewString(value)); }
    }

    public Color SpareColor {
        get {
            DynValue spareColor = script.GetVar("sparecolor");
            DynValue spareColor32 = script.GetVar("sparecolor32");
            DynValue val = spareColor.IsNotNil() ? spareColor : spareColor32;
            if (val.IsNil())
                return new Color(1, 1, 0, 1);

            if (val.Type != DataType.Table)
                throw new CYFException("An enemy's spare color must be a table with 3 or 4 numbers: type is " + val.Type.ToString() + ".");

            Table tab = val.Table;
            if (tab.Length < 3 || tab.Length > 4)
                throw new CYFException("An enemy's spare color must be a table with 3 or 4 numbers: the table has " + tab.Length + " elements.");

            foreach (TablePair p in tab.Pairs) {
                if (p.Key.Type != DataType.Number)
                    throw new CYFException("An enemy's spare color must be a table with 3 or 4 numbers: the table's " + p.Key.ToString() + " value doesn't have a numbered key.");
                if (p.Value.Type != DataType.Number)
                    throw new CYFException("An enemy's spare color must be a table with 3 or 4 numbers: the table's " + p.Key.ToString() + " value is of type " + p.Value.Type.ToString() + ".");
            }

            bool is32 = spareColor.IsNil();
            return new Color(
                Mathf.Clamp01((float)tab.Get(1).Number / (is32 ? 255 : 1)),
                Mathf.Clamp01((float)tab.Get(2).Number / (is32 ? 255 : 1)),
                Mathf.Clamp01((float)tab.Get(3).Number / (is32 ? 255 : 1)),
                tab.Get(4).Type == DataType.Nil ? 1 : Mathf.Clamp01((float)tab.Get(4).Number / (is32 ? 255 : 1))
            );
        }
    }

    public float PosX {
        get { return GetComponent<RectTransform>().position.x; }
    }

    public float PosY {
        get { return GetComponent<RectTransform>().position.y; }
    }

    public Vector2 DialogBubblePosition {
        get {
            RectTransform t = GetComponent<RectTransform>();
            Vector2 spr = new Vector2(Mathf.Abs(t.rect.width), t.rect.height);
            Vector2 bubSize;
            if (DialogBubble == "UI/SpeechBubbles/") {
                LuaTextManager text = GetComponentInChildren<LuaTextManager>();
                bubSize = text.GetBubbleSize();
                Vector2 bubbleShift = text.GetBubbleShift();
                Vector2 res;
                switch (BubbleSide) {
                    case BubbleSideEnum.LEFT:  res = new Vector2(-spr.x / 2 - bubSize.x - 25, bubSize.y / 2);              break;
                    case BubbleSideEnum.UP:    res = new Vector2(-bubSize.x / 2,              spr.y / 2 + bubSize.y + 25); break;
                    case BubbleSideEnum.DOWN:  res = new Vector2(-bubSize.x / 2,              -spr.y / 2 - 25);            break;
                    default:                   res = new Vector2(spr.x / 2 + 25,              bubSize.y / 2);              break; // rightside default
                }
                return res - bubbleShift;
            }
            Sprite diagBubbleSpr = SpriteRegistry.Get(DialogBubble);
            bubSize = new Vector2(diagBubbleSpr.rect.width, diagBubbleSpr.rect.height);
            if (diagBubbleSpr.name.StartsWith("left"))   return new Vector2(-spr.x / 2 - bubSize.x - 5, bubSize.y / 2);
            if (diagBubbleSpr.name.StartsWith("top"))    return new Vector2(-bubSize.x / 2,             spr.y / 2 + bubSize.y + 5);
            if (diagBubbleSpr.name.StartsWith("bottom")) return new Vector2(-bubSize.x / 2,             -spr.y / 2 - 5);
                                                         return new Vector2(spr.x / 2 + 5,              bubSize.y / 2); // rightside default
        }
    }

    public BubbleSideEnum BubbleSide {
        get {
            if (script.GetVar("bubbleside").Type == DataType.Nil)
                throw new CYFException("You need to set the value of the variable bubbleside if you want the engine to create a text bubble automatically.\nIts possible values are \"LEFT\", \"RIGHT\", \"UP\", \"DOWN\" or \"NONE\".");
            if (script.GetVar("bubbleside").Type != DataType.String)
                throw new CYFException("The bubbleside value can only be \"LEFT\", \"RIGHT\", \"UP\", \"DOWN\" or \"NONE\", but its value isn't a string.");
            string s = script.GetVar("bubbleside").String.ToUpper();
            try {
                return (BubbleSideEnum)Enum.Parse(typeof(BubbleSideEnum), s);
            } catch { throw new CYFException("The bubbleside value can only be \"LEFT\", \"RIGHT\", \"UP\", \"DOWN\" or \"NONE\", but its value is \"" + s.ToUpper() + "\"."); }
        }
        set {
            script.SetVar("bubbleside", DynValue.NewString(value.ToString()));
        }
    }

    public double BubbleWidth {
        get {
            if (script.GetVar("bubblewidth").Type != DataType.Number)
                throw new CYFException("The bubblewidth value of the monster " + Name + " isn't a number.\nIt must be a number above or equal to 16.");
            double val = script.GetVar("bubblewidth").Number;
            if (val < 16)
                throw new CYFException("The bubblewidth value of the monster " + Name + " is too low (" + val + ").\nIt must be a number above or equal to 16.");
            return val;
        }
        set { script.SetVar("bubblewidth", DynValue.NewNumber(value)); }
    }

    public void InitializeEnemy() {
        try {
            string scriptText = FileLoader.GetScript("Monsters/" + scriptName, StaticInits.ENCOUNTER, "monster");
            script.scriptname = scriptName;
            script.Bind("SetSprite", (Action<string>)SetSprite);
            script.Bind("SetActive", (Action<bool>)SetActive);
            script.Bind("isactive", DynValue.NewBoolean(true));
            script.Bind("Kill", (Action<bool>)DoKill);
            script.Bind("Spare", (Action<bool>)DoSpare);
            script.Bind("Move", (Action<float, float>)Move);
            script.Bind("MoveTo", (Action<float, float>)MoveTo);
            script.Bind("BindToArena", (Action<bool, bool>)BindToArena);
            script.Bind("SetDamage", (Action<int>)SetDamage);
            script.Bind("SetBubbleOffset", (Action<int, int>)SetBubbleOffset);
            script.Bind("SetDamageUIOffset", (Action<int, int>)SetDamageUIOffset);
            script.Bind("SetSliceAnimOffset", (Action<int, int>)SetSliceAnimOffset);
            script.Bind("State", (Action<Script, string>)UIController.SwitchStateOnString);
            script.Bind("Remove", (Action)Remove);
            script.SetVar("canmove", DynValue.NewBoolean(true));
            sprite = LuaSpriteController.GetOrCreate(gameObject);
            script.SetVar("monstersprite", UserData.Create(sprite, LuaSpriteController.data));
            script.SetVar("bubblesprite", UserData.Create(LuaSpriteController.GetOrCreate(bubbleObject)));
            script.SetVar("textobject", UserData.Create(bubbleObject.GetComponentInChildren<LuaTextManager>()));
            script.DoString(scriptText);

            string spriteFile = script.GetVar("sprite").String;
            if (spriteFile != null)
                SetSprite(spriteFile);
            else
                throw new CYFException("The monster script " + scriptName + ".lua's sprite value is not a string.");

            ui = FindObjectOfType<UIController>();
            if (MaxHP == 0)
                MaxHP = HP;

            /*if (script.GetVar("canspare") == null) CanSpare = false;
            if (script.GetVar("cancheck") == null) CanCheck = true;*/
        }
        catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(scriptName, ex.DecoratedMessage != null ? UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message : ex.Message); }
        catch (Exception ex)            { UnitaleUtil.DisplayLuaError(scriptName, "Unknown error. Usually means you're missing a sprite.\nSee documentation for details.\nStacktrace below in case you wanna notify a dev.\n\nError: " + ex.Message + "\n\n" + ex.StackTrace); }
    }

    public void HandleAttack(int hitStatus) { UnitaleUtil.TryCall(script, "HandleAttack", new[] { DynValue.NewNumber(hitStatus) }); }

    public string[] GetDefenseDialog() {
        DynValue dialogues = script.GetVar("currentdialogue");
        if (dialogues.Table == null)
            if (dialogues.String != null)  return new[] { dialogues.String };
            else if (Dialogue == null)     return null;
            else                           return new[] { Dialogue[Random.Range(0, Dialogue.Length)] };

        string[] dialogueStrings = new string[dialogues.Table.Length];
        for (int i = 0; i < dialogues.Table.Length; i++)
            dialogueStrings[i] = dialogues.Table.Get(i + 1).String;
        script.SetVar("currentdialogue", DynValue.NewNil());
        return dialogueStrings;
    }

    protected void HandleCustomCommand(string command) {
        UnitaleUtil.TryCall(script, "HandleCustomCommand", new[] { DynValue.NewString(command) });
    }

    public void CreateBubble() {
        GameObject speechBub = Instantiate(SpriteFontRegistry.BUBBLE_OBJECT);
        speechBub.transform.SetParent(transform);

        LuaTextManager sbTextMan = speechBub.GetComponentInChildren<LuaTextManager>();
        sbTextMan.SetCaller(script);
        sbTextMan.HideBubble();
        sbTextMan.SetText(DynValue.NewString(""));
        sbTextMan.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        sbTextMan.adjustTextDisplay = true;

        bubbleObject = speechBub;
    }

    public void UpdateBubble(int enemyID) {
        LuaTextManager sbTextMan = bubbleObject.GetComponentInChildren<LuaTextManager>();

        bool usingAutoBubble = DialogBubble == "UI/SpeechBubbles/";
        Image speechBubImg = bubbleObject.GetComponent<Image>();
        speechBubImg.enabled = !usingAutoBubble;

        // Bubble management: can be a normal bubble OR an auto bubble from text objects
        if (!usingAutoBubble) {
            try { SpriteUtil.SwapSpriteFromFile(speechBubImg, DialogBubble, enemyID); }
            catch (Exception e) {
                UnitaleUtil.DisplayLuaError(scriptName + ": Creating a dialogue bubble", "An error was encountered. It's highly possible the dialogue bubble " + script.GetVar("dialogbubble") + " doesn't exist.\n\nError: " + e.Message);
                return;
            }
            Sprite speechBubSpr = speechBubImg.sprite;

            float xMov = speechBubSpr.border.x;
            float yMov = -speechBubSpr.border.w - sbTextMan.font.LineSpacing;
            float angle = sbTextMan.rotation * Mathf.Deg2Rad;
            sbTextMan.MoveTo((int)(Mathf.Cos(angle) * xMov - Mathf.Sin(angle) * yMov), (int)(Mathf.Sin(angle) * xMov + Mathf.Cos(angle) * yMov));
            speechBubImg.color = new Color(speechBubImg.color.r, speechBubImg.color.g, speechBubImg.color.b, sbTextMan.letters.Count == 0 ? 0 : 1);

            if (bubbleWidth == 0)
                bubbleWidth = speechBubSpr.textureRect.width - speechBubSpr.border.x - speechBubSpr.border.z;

            sbTextMan.HideBubble();
        } else {
            try { bubbleWidth = (float)BubbleWidth; }
            catch (Exception e) {
                UnitaleUtil.DisplayLuaError(scriptName + ": Creating a dialogue bubble", e.Message);
                return;
            }

            if (sbTextMan.letters.Count > 0) sbTextMan.ShowBubble(GetReverseBubbleSide(BubbleSide).ToString(), DynValue.NewString("50%"));
            else                             sbTextMan.HideBubble();
            sbTextMan.MoveTo(0, 0);
        }

        sbTextMan._textMaxWidth = (int)bubbleWidth;
        speechBubImg.transform.SetAsLastSibling();

        bubbleObject.GetComponent<RectTransform>().anchoredPosition = DialogBubblePosition + offsets[1];
        sbTextMan.Move(0, 0); // Used to even out the text object's position so it's only using integers

        if (Voice != "")
            sbTextMan.fontVoice = Voice;
    }

    public void HideBubble() {
        bubbleObject.GetComponent<Image>().enabled = false;
        bubbleObject.GetComponentInChildren<LuaTextManager>().HideBubble();
    }

    public void Remove() {
        try {
            UIController.instance.encounter.enemies.Remove(this);
            script.Remove();
            Destroy(gameObject);
        } catch (MissingReferenceException) {
            throw new CYFException("Attempt to remove a removed enemy.");
        }
    }

    public void SetSprite(string filename) {
        sprite.Set(filename);
    }

    /// <summary>
    /// Call function to grey out enemy and pop the smoke particles, and mark it as spared.
    /// </summary>
    public void DoSpare(bool playSound = true) {
        if (!inFight)
            return;
        UIController.instance.gold += Gold;
        // We have to code the particles separately because they don't work well in UI screenspace. Great stuff.
        ParticleSystem spareSmoke = Instantiate(Resources.Load<ParticleSystem>("Prefabs/MonsterSpareParticleSys"));
        spareSmoke.Emit(10);
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[10];
        spareSmoke.GetParticles(particles);
        Vector3 particlePos = RTUtil.AbsCenterOf(GetComponent<RectTransform>());
        particlePos.z = 5;
        for (int i = 0; i < particles.Length; i++)
            particles[i].position = particlePos + particles[i].velocity.normalized * 5;
        spareSmoke.SetParticles(particles, particles.Length);
        spareSmoke.gameObject.transform.SetParent(UIController.instance.psContainer.transform);

        // The actually relevant part of sparing code.
        GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
        if (playSound) UIController.PlaySoundSeparate("enemydust");
        SetActive(false);
        spared = true;
        FightUIController.instance.DestroyAllAttackInstances(this);

        UIController.instance.CheckAndTriggerVictory();
    }

    /// <summary>
    /// Call function to turn enemy to dust and mark it as killed.
    /// </summary>
    public void DoKill(bool playSound = true) {
        if (!inFight)
            return;
        UIController.instance.gold += Gold;
        UIController.instance.exp += XP;
        UnitaleUtil.Dust(gameObject, sprite);
        SetActive(false);
        killed = true;
        if (playSound) UIController.PlaySoundSeparate("enemydust");
        FightUIController.instance.DestroyAllAttackInstances(this);

        UIController.instance.CheckAndTriggerVictory();
    }

    /// <summary>
    /// Set if we should consider this monster for menus e.g.
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active) {
        inFight = active;
        script.SetVar("isactive", DynValue.NewBoolean(active));
    }

    public void Move(float x, float y) {
        GetComponent<RectTransform>().position = new Vector2(GetComponent<RectTransform>().position.x + x, GetComponent<RectTransform>().position.y + y);
    }

    public void MoveTo(float x, float y) {
        GetComponent<RectTransform>().position = new Vector2(x, y);
    }

    public void BindToArena(bool bind, bool isUnderArena = false) {
        int count = 0;
        if (bind) {
            count += FindObjectsOfType<EnemyController>().Count(luaec => luaec.transform.parent.name == "LuaEnemyEncounterGO" && luaec.index < index);
            transform.SetParent(GameObject.Find("LuaEnemyEncounterGO").transform, true);
        } else {
            count += FindObjectsOfType<EnemyController>().Count(luaec => luaec.transform.parent.name == "arena_container" && luaec.index <= index &&
                                                                            (isUnderArena && luaec.transform.GetSiblingIndex() < GameObject.Find("arena_border_outer").transform.GetSiblingIndex() || !isUnderArena));
            if (!isUnderArena) count++;
            transform.SetParent(GameObject.Find("arena_container").transform, true);
        }
        transform.SetSiblingIndex(count);
    }

    public void SetDamage(int dmg) { presetDmg = dmg; }

    public void Update() {
        try {
            script.SetVar("posx", DynValue.NewNumber(GetComponent<RectTransform>().position.x));
            script.SetVar("posy", DynValue.NewNumber(GetComponent<RectTransform>().position.y));
        } catch { /* ignored */ }
    }

    public void SetSliceAnimOffset(int x, int y) { offsets[0] = new Vector2(x, y); }

    public void SetBubbleOffset(int x, int y) { offsets[1] = new Vector2(x, y); }

    public void SetDamageUIOffset(int x, int y) { offsets[2] = new Vector2(x, y); }

    public void ResetPresetDamage() { realPresetDmg = FightUIController.DAMAGE_NOT_SET; }
}