using System;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class LuaEnemyController : EnemyController {
    internal string scriptName;
    internal ScriptWrapper script;
    internal bool inFight = true; // if false, enemy will no longer be considered as an option in menus and such
    private string lastBubbleName;
    private bool error = false;
    public int presetDmg = -1826643; // You'll not be able to deal exactly -1 826 643 dmg with this technique.
    public float xFightAnimShift = 0;
    public LuaSpriteController sprite;
    public float bubbleWidth = 0;
    public int index = -1;
    public Vector2[] offsets = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };
                                             //SliceAnimOffset    BubbleOffset       DamageUIOffset

    internal bool spared = false;
    internal bool killed = false;

    public override string Name {
        get { return script.GetVar("name").String; }
        set { script.SetVar("name", DynValue.NewString(value)); }
    }

    public override string[] ActCommands {
        get {
            if (error)
                return new string[] { "LUA error" };
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

    public override string[] Comments {
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

    public override string[] Dialogue {
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

    public override string CheckData {
        get { return script.GetVar("check").String; }
        set { script.SetVar("check", DynValue.NewString(value)); }
    }

    public override int HP {
        get {
            if (GlobalControls.retroMode && (int)script.GetVar("hp").Number > MaxHP)
                MaxHP = (int)script.GetVar("hp").Number;
            return (int)script.GetVar("hp").Number;
        }
        set { script.SetVar("hp", DynValue.NewNumber(value)); }
    }

    public override int MaxHP {
        get { return (int)script.GetVar("maxhp").Number; }
        set { script.SetVar("maxhp", DynValue.NewNumber(value)); }
    }

    public override int Attack {
        get { return (int)script.GetVar("atk").Number; }
        set { script.SetVar("atk", DynValue.NewNumber(value)); }
    }

    public override int Defense {
        get { return (int)script.GetVar("def").Number; }
        set { script.SetVar("def", DynValue.NewNumber(value)); }
    }

    public override int XP {
        get { return (int)script.GetVar("xp").Number; }
        set { script.SetVar("xp", DynValue.NewNumber(value)); }
    }

    public override int Gold {
        get { return (int)script.GetVar("gold").Number; }
        set { script.SetVar("gold", DynValue.NewNumber(value)); }
    }

    public string DefenseMissText {
        get { return script.GetVar("defensemisstext").String; }
        set { script.SetVar("defensemisstext", DynValue.NewString(value)); }
    }

    public string NoAttackMissText {
        get { return script.GetVar("noattackmisstext").String; }
        set { script.SetVar("noattackmisstext", DynValue.NewString(value)); }
    }

    public override string DialogBubble {
        get {
            if (script.GetVar("dialogbubble") == null)
                return "UI/SpeechBubbles/right";

            string bubbleToGet = script.GetVar("dialogbubble").String;
            return "UI/SpeechBubbles/" + bubbleToGet;
        }
    }

    public override bool CanSpare {
        get {
            DynValue spareVal = script.GetVar("canspare");
            if (spareVal == null)
                return false;
            return spareVal.Boolean;
        }

        set { script.SetVar("canspare", DynValue.NewBoolean(value)); }
    }

    public override bool CanCheck {
        get {
            DynValue checkVal = script.GetVar("cancheck");
            if (checkVal == null)
                return true;
            return checkVal.Boolean;
        }

        set { script.SetVar("cancheck", DynValue.NewBoolean(value)); }
    }

    public override string DialoguePrefix {
        get {
            DynValue dialoguePrefix = script.GetVar("dialogueprefix");
            if (dialoguePrefix == null || dialoguePrefix.Type == DataType.Nil)
                return "[effect:rotate]";
            return dialoguePrefix.String;
        }

        set { script.SetVar("dialogueprefix", DynValue.NewString(value)); }
    }

    public override bool Unkillable {
        get {
            DynValue checkVal = script.GetVar("unkillable");
            if (checkVal == null)
                return false;
            return checkVal.Boolean;
        }

        set { script.SetVar("unkillable", DynValue.NewBoolean(value)); }
    }

    public override string Font {
        get {
            DynValue fontVal = script.GetVar("font");
            if (fontVal == null || fontVal.Type == DataType.Nil)
                return SpriteFontRegistry.UI_MONSTERTEXT_NAME;
            return fontVal.String;
        }

        set { script.SetVar("font", DynValue.NewString(value)); }
    }

    public override string Voice {
        get {
            DynValue voiceVal = script.GetVar("voice");
            if (voiceVal == null || voiceVal.Type == DataType.Nil)
                return "";
            return voiceVal.String;
        }

        set { script.SetVar("voice", DynValue.NewString(value)); }
    }

    public float PosX {
        get { return GetComponent<RectTransform>().position.x; }
    }

    public float PosY {
        get { return GetComponent<RectTransform>().position.y; }
    }

    public bool canMove = false;

    private void Start() {
        try {
            string scriptText = ScriptRegistry.Get(ScriptRegistry.MONSTER_PREFIX + scriptName);
            if (scriptText == null) {
                UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "Tried to load monster script " + scriptName + ".lua but it didn't exist. Is it misspelled?");
                return;
            }
            script.scriptname = scriptName;
            script.Bind("SetSprite", (Action<string>)SetSprite);
            script.Bind("SetActive", (Action<bool>)SetActive);
            script.Bind("isactive", DynValue.NewBoolean(true));
            script.Bind("Kill", (Action)DoKill);
            script.Bind("Spare", (Action)DoSpare);
            script.Bind("Move", (Action<float, float>)Move);
            script.Bind("MoveTo", (Action<float, float>)MoveTo);
            script.Bind("BindToArena", (Action<bool, bool>)BindToArena);
            script.Bind("SetDamage", (Action<int>)SetDamage);
            script.Bind("SetBubbleOffset", (Action<int, int>)SetBubbleOffset);
            script.Bind("SetDamageUIOffset", (Action<int, int>)SetDamageUIOffset);
            script.Bind("SetSliceAnimOffset", (Action<int, int>)SetSliceAnimOffset);
            script.Bind("State", (Action<Script, string>)UIController.SwitchStateOnString);
            script.SetVar("canmove", DynValue.NewBoolean(false));
            sprite = new LuaSpriteController(GetComponent<Image>());
            script.SetVar("monstersprite", UserData.Create(sprite, LuaSpriteController.data));
            script.DoString(scriptText);

            string spriteFile = script.GetVar("sprite").String;
            if (spriteFile != null)
                SetSprite(spriteFile);
            else
                throw new CYFException("missing sprite");

            ui = FindObjectOfType<UIController>();
            if (MaxHP == 0)
                MaxHP = HP;

            textBubbleSprite = Resources.Load<Sprite>("Sprites/UI/SpeechBubbles/right");

            /*if (script.GetVar("canspare") == null) CanSpare = false;
            if (script.GetVar("cancheck") == null) CanCheck = true;*/
        }
        catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(scriptName, ex.DecoratedMessage != null ? UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message : ex.Message); }
        catch (Exception ex)            { UnitaleUtil.DisplayLuaError(scriptName, "Unknown error. Usually means you're missing a sprite.\nSee documentation for details.\nStacktrace below in case you wanna notify a dev.\n\nError: " + ex.Message + "\n\n" + ex.StackTrace); }
    }

    public override void HandleAttack(int hitStatus) { TryCall("HandleAttack", new DynValue[] { DynValue.NewNumber(hitStatus) }); }

    /*public override string GetRegularScreenDialog() {
        if (!error)
            return script.Call(script.Globals["GetRegularScreenDialog"]).String;
        else {
            UIController.instance.textmgr.setFont(SpriteFontRegistry.F_UI_MONSTERDIALOG);
            luaErrorMsg = luaErrorMsg.Replace("\\n", "").Replace("\\r", "").Replace("\\t", "");
            for (int i = 0; i < luaErrorMsg.Length; i++)
                if (i > 0 && i % 40 == 0)
                    luaErrorMsg = luaErrorMsg.Insert(i, "\\r");
            return "[starcolor:ffffff][color:ffffff]LUA error.\n" + luaErrorMsg;
        }
    }*/

    public override string[] GetDefenseDialog() {
        if (!error) {
            DynValue dialogues = script.GetVar("currentdialogue");
            if (dialogues == null || dialogues.Table == null)
                if (dialogues.String != null)  return new string[] { dialogues.String };
                else if (Dialogue == null)     return null;
                else                           return new string[] { Dialogue[UnityEngine.Random.Range(0, Dialogue.Length)] };

            string[] dialogueStrings = new string[dialogues.Table.Length];
            for (int i = 0; i < dialogues.Table.Length; i++)
                dialogueStrings[i] = dialogues.Table.Get(i + 1).String;
            script.SetVar("currentdialogue", DynValue.NewNil());
            return dialogueStrings;
        } else
            return new string[] { "LUA\nerror." };
    }

    public bool TryCall(string func, DynValue[] param = null) {
        try {
            DynValue sval = script.GetVar(func);
            if (sval == null || sval.Type == DataType.Nil) return false;
            if (param != null)                             script.Call(func, param);
            else                                           script.Call(func);
            return true;
        }
        catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(scriptName, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message); }
        return true;
    }

    protected override void HandleCustomCommand(string command) {
        TryCall("HandleCustomCommand", new DynValue[] { DynValue.NewString(command) });
    }

    public void SetSprite(string filename) {
        if (filename == null)
            throw new CYFException("The enemy's sprite can't be nil!");
        SpriteUtil.SwapSpriteFromFile(this, filename);
    }

    /// <summary>
    /// Call function to grey out enemy and pop the smoke particles, and mark it as spared.
    /// </summary>
    public void DoSpare() {
        if (!inFight)
            return;
        UIController.instance.gold += Gold;
        // We have to code the particles separately because they don't work well in UI screenspace. Great stuff.
        ParticleSystem spareSmoke = Instantiate<ParticleSystem>(Resources.Load<ParticleSystem>("Prefabs/MonsterSpareParticleSys"));
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
        UIController.PlaySoundSeparate(AudioClipRegistry.GetSound("enemydust"));
        SetActive(false);
        spared = true;

        UIController.instance.CheckAndTriggerVictory();
    }

    /// <summary>
    /// Call function to turn enemy to dust and mark it as killed.
    /// </summary>
    public void DoKill() {
        if (!inFight)
            return;
        UIController.instance.gold += Gold;
        UIController.instance.exp += XP;
        GameObject go = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/MonsterDuster"));
        go.transform.SetParent(UIController.instance.psContainer.transform);
        GetComponent<ParticleDuplicator>().Activate(sprite);
        SetActive(false);
        killed = true;
        UIController.PlaySoundSeparate(AudioClipRegistry.GetSound("enemydust"));

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
        if (!canMove)
            return;
        GetComponent<RectTransform>().position = new Vector2(GetComponent<RectTransform>().position.x + x, GetComponent<RectTransform>().position.y + y);
    }

    public void MoveTo(float x, float y) {
        if (!canMove)
            return;
        GetComponent<RectTransform>().position = new Vector2(x, y);
    }

    public void BindToArena(bool bind, bool isUnderArena = false) {
        int count = 0;
        if (bind) { //If bind :ahde:
            foreach (LuaEnemyController luaec in GameObject.FindObjectsOfType<LuaEnemyController>()) //for each enemies...
                if (luaec.transform.parent.name == "LuaEnemyEncounterGO" && luaec.index < index) //If the enemy's index is greater than the current enemy's index, let's put it below.
                    count++;
            transform.SetParent(GameObject.Find("LuaEnemyEncounterGO").transform, true);
        } else {
            foreach (LuaEnemyController luaec in GameObject.FindObjectsOfType<LuaEnemyController>())
                if (luaec.transform.parent.name == "arena_container" && luaec.index <= index &&
                    ((isUnderArena && luaec.transform.GetSiblingIndex() < GameObject.Find("arena_border_outer").transform.GetSiblingIndex()) ||!isUnderArena))  count++;
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
        } catch { }
        if (!ArenaManager.instance.firstTurn &&!canMove) {
            canMove = true;
            script.SetVar("canmove", DynValue.NewBoolean(true));
        }
    }

    public void SetSliceAnimOffset(int x, int y) { offsets[0] = new Vector2(x, y); }

    public void SetBubbleOffset(int x, int y) { offsets[1] = new Vector2(x, y); }

    public void SetDamageUIOffset(int x, int y) { offsets[2] = new Vector2(x, y); }

    public bool InFight() { return inFight; }
}