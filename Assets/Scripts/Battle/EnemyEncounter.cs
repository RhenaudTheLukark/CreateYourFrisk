using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyEncounter : MonoBehaviour {
    public List<EnemyController> enemies;
    public Vector2[] enemyPositions;
    internal float waveTimer;
    public int turnCount;

    public string EncounterText { get; set; }
    public bool CanRun = true;

    public static ScriptWrapper script;
    public float waveBeginTime;
    private ScriptWrapper[] waves;
    private string[] waveNames;
    public bool gameOverStance;
    public static bool doNotGivePreviousEncounterToSelf;

    private delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public void Awake() {
        InitScript();
    }

    public void OnDestroy() {
        ScriptWrapper.instances.Clear();
    }

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    public void InitScript() {
        doNotGivePreviousEncounterToSelf = true;
        script = new ScriptWrapper { scriptname = StaticInits.ENCOUNTER };

        script.DoString(FileLoader.GetScript("Encounters/" + StaticInits.ENCOUNTER, StaticInits.ENCOUNTER, "encounter"));
        script.Bind("State", (Action<Script, string>)UIController.SwitchStateOnString);
        script.Bind("RandomEncounterText", (Func<string>)RandomEncounterText);
        script.Bind("CreateProjectile", (Func<Script, string, float, float, string, DynValue>)CreateProjectile);
        script.Bind("CreateProjectileAbs", (Func<Script, string, float, float, string, DynValue>)CreateProjectileAbs);
        script.Bind("SetButtonLayer", (Action<string>)LuaScriptBinder.SetButtonLayer);
        script.Bind("CreateEnemy", (Func<string, float, float, DynValue>)CreateEnemy);
        script.Bind("Flee", (Action)Flee);

        LoadEnemiesAndPositions();
    }

    public DynValue CreateEnemy(string enemyScript, float x, float y) {
        GameObject enemyObject = Instantiate(Resources.Load<GameObject>("Prefabs/LUAEnemy"));

        enemyObject.transform.SetParent(gameObject.transform);
        enemyObject.transform.localScale = new Vector3(1, 1, 1); // apparently this was suddenly required or the scale would be (0,0,0)

        EnemyController enemyController = enemyObject.GetComponent<EnemyController>();
        enemyController.scriptName = enemyScript;
        enemyController.index = enemies.Count - 1;
        enemyController.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        enemyController.script = new ScriptWrapper();
        enemies.Add(enemyController);
        enemyController.CreateBubble();
        enemyController.InitializeEnemy();

        return UserData.Create(enemyController.script);
    }

    public void Flee() {
        StartCoroutine(UIController.instance.ISuperFlee());
    }

    public bool CallOnSelfOrChildren(string func, DynValue[] param = null) {
        // TODO: Don't stop execution if the function has been run in the Encounter script
        if (UnitaleUtil.TryCall(script, func, param)) return true;

        bool calledOne = false;
        foreach (EnemyController enemy in enemies)
            if (UnitaleUtil.TryCall(enemy.script, func, param))
                calledOne = true;
        return calledOne;
    }

    public EnemyController[] EnabledEnemies {
        get { return enemies.Where(x => x.inFight).ToArray(); }
    }

    public Vector2 ArenaSize {
        get {
            if (script.GetVar("arenasize") == null) return new Vector2(155, 130);
            Table size = script.GetVar("arenasize").Table;
            if (size == null)
                return new Vector2(155, 130);
            if (size.Get(1).Number < 16 || size.Get(2).Number < 16) // TODO remove hardcoding (but player never changes size so nobody cares)
                return new Vector2(size.Get(1).Number > 16 ? (int)size.Get(1).Number : 16, size.Get(2).Number > 16 ? (int)size.Get(2).Number : 16);
            return new Vector2((int)size.Get(1).Number, (int)size.Get(2).Number);
        }
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

    protected void LoadEnemiesAndPositions() {
        AudioSource musicSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        EncounterText = script.GetVar("encountertext").String;
        DynValue enemyScriptsLua = script.GetVar("enemies");
        DynValue enemyPositionsLua = script.GetVar("enemypositions");
        string musicFile = script.GetVar("music").String;

        if (enemyScriptsLua.Table == null) {
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "There has to be an enemies table in your encounter's file.");
            return;
        }

        int enemyCount = enemyScriptsLua.Table.Length;

        if (enemyPositionsLua != null && enemyPositionsLua.Table != null) {
            enemyPositions = new Vector2[enemyPositionsLua.Table.Length];
            for (int i = 0; i < enemyPositionsLua.Table.Length; i++) {
                Table posTable = enemyPositionsLua.Table.Get(i + 1).Table;
                if (i >= enemyCount)
                    break;

                enemyPositions[i] = new Vector2((float)posTable.Get(1).Number, (float)posTable.Get(2).Number);
            }
        }

        if (MusicManager.IsStoppedOrNull(PlayerOverworld.audioKept)) {
            if (musicFile != null) {
                try {
                    AudioClip music = AudioClipRegistry.GetMusic(musicFile);
                    musicSource.clip = music;
                    MusicManager.filename = "music:" + musicFile.ToLower();
                } catch (Exception) { UnitaleUtil.Warn("Loading custom music failed."); }
            } else {
                musicSource.clip = AudioClipRegistry.GetMusic("mus_battle1");
                musicSource.volume = .6f;
                MusicManager.filename = "music:mus_battle1";
            }
            NewMusicManager.audioname["src"] = MusicManager.filename;
        }
        // Instantiate all the enemy objects
        if (enemyCount > enemyPositions.Length) {
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "All enemies in an encounter must have a screen position defined. Either your enemypositions table is missing, "
                + "or there are more enemies than available positions. Refer to the documentation's Basic Setup section on how to do this.");
            return;
        }

        Table luaEnemyTable = script.GetVar("enemies").Table;

        enemies = new List<EnemyController>();
        for (int i = 0; i < enemyCount; i++)
            luaEnemyTable.Set(i + 1, CreateEnemy(enemyScriptsLua.Table.Get(i + 1).String, enemyPositions[i].x, enemyPositions[i].y));

        script.SetVar("enemies", DynValue.NewTable(luaEnemyTable));
        Table luaWaveTable = new Table(null);
        script.SetVar("Wave", DynValue.NewTable(luaWaveTable));

        //if (MusicManager.isStoppedOrNull(PlayerOverworld.audioKept))
        //    musicSource.Play(); // play that funky music
    }

    protected string RandomEncounterText() {
        if (EnabledEnemies.Length <= 0)
            return "";
        int randomEnemy = Random.Range(0, EnabledEnemies.Length);
        string[] comments;
        try { comments = EnabledEnemies[randomEnemy].Comments; }
        catch { throw new CYFException("RandomEncounterText: Can not read the \"comments\" table of enemy #" + (randomEnemy + 1) + ".\nAre you sure it's set?"); }
        if (comments.Length <= 0)
            return "";
        int randomComment = Random.Range(0, comments.Length);
        return comments[randomComment];
    }

    public static void BattleDialog(Script scr, DynValue arg) {
        if (UIController.instance == null)
            UnitaleUtil.Warn("BattleDialog can only be used as early as EncounterStarting.");
        else {
            UIController.instance.battleDialogueStarted = true;
            EnemyController enemy = UIController.instance.encounter.enemies.Find(e => e.script.script == scr);
            if (enemy)
                UIController.instance.mainTextManager.SetCaller(enemy.script);
            TextMessage[] msgs = null;
            if (arg.Type == DataType.String)
                msgs = new TextMessage[]{new RegularMessage(arg.String)};
            else if (arg.Type == DataType.Table && (GlobalControls.retroMode || arg.Table.Length > 0)) {
                // Check if the data is right
                for (int i = 0; i < arg.Table.Length; i++)
                    if (arg.Table.Get(i + 1).Type != DataType.String)
                        throw new CYFException("BattleDialog: You need to input a non-empty array or a string here." +
                                               "\n\nIf you're sure that you've entered what's needed, you may contact the dev.");

                msgs = new TextMessage[arg.Table.Length];
                for (int i = 0; i < arg.Table.Length; i++)
                    msgs[i] = new RegularMessage(arg.Table.Get(i + 1).String);
            } else if (!GlobalControls.retroMode)
                throw new CYFException("BattleDialog: You need to input a non-empty array or a string here." +
                                       "\n\nIf you're sure that you've entered what's needed, you may contact the dev.");
            if (!GlobalControls.retroMode)
                UIController.instance.mainTextManager.SetEffect(new TwitchEffect(UIController.instance.mainTextManager));

            UIController.instance.ActionDialogResult(msgs);
        }
    }

    public void HandleItem(int ID) { Inventory.UseItem(ID); }

    // <summary>
    // Overrideable item handler on a per-encounter basis. Should return true if a custom action is executed for the given item.
    // </summary>
    // <param name="item">Item to be checked for custom action</param>
    // <returns>true if a custom action should be executed for given item, false if the default action should happen</returns>
    /*public virtual bool CustomItemHandler(UnderItem item) {
        UIController.instance.encounter.CallOnSelfOrChildren("HandleItem", new MoonSharp.Interpreter.DynValue[] { MoonSharp.Interpreter.DynValue.NewString(item.Name) });
        return false;*/
        // the following was test code that allowed you to activate dogs in order 2-3-1 to replace all bullets with dogs
        /*if (dogTest[0] && dogTest[1] && dogTest[2])
        {
            UIController.instance.ActionDialogResult(new RegularMessage[]{
                new RegularMessage("After unlocking the\r[color:ffff00]Secret of Dog[color:ffffff],\ryou don't feel like using dog " + item.ID[7] + "."),
                new RegularMessage("So you released it.\nFarewell, dog!")
            }, UIController.UIState.ENEMYDIALOGUE);
            Inventory.container.Remove(item);
            return true;
        }

        if (item.ID == "DOGTEST2")
        {
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog 2.\nMight be part of a pattern."), UIController.UIState.ENEMYDIALOGUE);
            dogTest[0] = true;
            return true;
        }

        if (item.ID == "DOGTEST3" && dogTest[0])
        {
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog 3.\nThis seems about right..."), UIController.UIState.ENEMYDIALOGUE);
            dogTest[1] = true;
            return true;
        }

        if (item.ID == "DOGTEST1" && dogTest[1])
        {
            AudioClip yay = Resources.Load<AudioClip>("Sounds/dogsecret");
            AudioSource.PlayClipAtPoint(yay, Camera.main.transform.position);
            UIController.instance.ActionDialogResult(new RegularMessage[]{
                new RegularMessage("You have unlocked the\r[color:ffff00]Secret of Dog[color:ffffff].\nYou are overcome with happiness."),
                new RegularMessage("And spiders, too.")
            }, UIController.UIState.ENEMYDIALOGUE);
            dogTest[2] = true;
            return true;
        }

        if (dogTest[0] || dogTest[1] || dogTest[2])
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog " + item.ID[7] + ".\nNo... that's not it."), UIController.UIState.ENEMYDIALOGUE);
        else
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog " + item.ID[7] + ".\nIt feels off."), UIController.UIState.ENEMYDIALOGUE);

        dogTest[0] = false;
        dogTest[1] = false;
        dogTest[2] = false;
        return true;*/
    //}

    public void HandleSpare() {
        //bool sparedAny = false;
        foreach (EnemyController enemy in enemies)
            if (enemy.CanSpare)
                enemy.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
    }

    [HideInInspector] public DynValue CreateProjectileAbs(Script s, string sprite, float xpos, float ypos, string layerName = "") {
        LuaProjectile projectile = (LuaProjectile)BulletPool.instance.Retrieve();
        if (sprite == null)
            throw new CYFException("You can't create a projectile with a nil sprite!");
        SpriteUtil.SwapSpriteFromFile(projectile, sprite);
        // TODO: Restore in 0.7
        //projectile.name = sprite;
        projectile.GetComponent<CYFSprite>().ctrl.spritename = sprite;
        projectile.owner = s;
        projectile.gameObject.SetActive(true);
        projectile.ctrl.MoveToAbs(xpos, ypos);
        //projectile.ctrl.z = Projectile.Z_INDEX_NEXT; //doesn't work yet, thanks unity UI
        projectile.transform.SetAsLastSibling();
        //projectile.ctrl.UpdatePosition();
        projectile.ctrl.sprite.Set(sprite);
        if (layerName != "")
            try { projectile.transform.SetParent(GameObject.Find(layerName + "Bullet").transform); }
            catch {
                try { projectile.transform.SetParent(GameObject.Find(layerName + "Layer").transform); }
                catch { /* ignored */ }
            }
        DynValue projectileController = UserData.Create(projectile.ctrl);
        //Texture2D tex = (Texture2D)projectile.GetComponent<Image>().mainTexture;
        //projectile.selfAbs = UnitaleUtil.GetFurthestCoordinates(tex.GetPixels32(), tex.height, projectile.self);

        return projectileController;
    }

    private DynValue CreateProjectile(Script s, string sprite, float xpos, float ypos, string layerName = "") {
        return CreateProjectileAbs(s, sprite, ArenaManager.arenaCenter.x + xpos, ArenaManager.arenaCenter.y + ypos, layerName);
    }

    public void UpdateWave() {
        string currentScript = "";
        try {
            for (int i = 0; i < waves.Length; i++) {
                currentScript = waveNames[i];
                try { waves[i].Call("Update"); }
                catch (InterpreterException ex) {
                    UnitaleUtil.DisplayLuaError(currentScript, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
                    return;
                } catch (Exception ex) {
                    if (GlobalControls.retroMode) return;
                    if (waves[i].script.Globals["Update"] == null) UnitaleUtil.DisplayLuaError(currentScript, "All the wave scripts need an Update() function!");
                    else                                           UnitaleUtil.DisplayLuaError(currentScript, "This error is a " + ex.GetType() + " error.\nPlease send this error to the main dev.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
                    return;
                }
            }
        } catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(currentScript, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
        }
    }

    private void PrepareWave() {
        DynValue nextWaves = script.GetVar("nextwaves");
        if (nextWaves.Type != DataType.Table) {
            if (nextWaves.Type == DataType.Nil)
                UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "nextwaves is not defined in your script.");
            else
                UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "nextwaves is a " + nextWaves.Type + ", but should be a table.");
            return;
        }

        if (waves != null)
            foreach (ScriptWrapper wrap in waves)
                wrap.Remove();

        waves = new ScriptWrapper[nextWaves.Table.Length];
        waveNames = new string[waves.Length];
        int currentWaveScript = 0;
        try {
            List<int> indexes = new List<int>();
            for (int i = 0; i < waves.Length; i++) {
                currentWaveScript = i;
                waves[i] = new ScriptWrapper { scriptname = nextWaves.Table.Get(i + 1).String };
                waves[i].script.Globals["EndWave"] = (Action)EndWaveTimer;
                waves[i].script.Globals["State"] = (Action<Script, string>)UIController.SwitchStateOnString;
                waves[i].script.Globals["CreateProjectile"] = (Func<Script, string, float, float, string, DynValue>)CreateProjectile;
                waves[i].script.Globals["CreateProjectileAbs"] = (Func<Script, string, float, float, string, DynValue>)CreateProjectileAbs;
                if (nextWaves.Table.Get(i + 1).Type != DataType.String){
                    UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "Non-string value encountered in nextwaves table");
                    return;
                }
                waveNames[i] = nextWaves.Table.Get(i + 1).String;
                waves[i].script.Globals["wavename"] = nextWaves.Table.Get(i + 1).String;
                try {
                    waves[i].DoString(FileLoader.GetScript("Waves/" + nextWaves.Table.Get(i + 1).String, nextWaves.Table.Get(i + 1).String + ".lua", "wave"));
                    indexes.Add(i);
                } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(nextWaves.Table.Get(i + 1).String + ".lua", UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
                } catch (Exception ex) { UnitaleUtil.DisplayLuaError("<UNKNOWN LOCATION>", ex.Message + "\n\n" + ex.StackTrace); }
            }
            Table luaWaveTable = new Table(null);
            for (int i = 0; i < indexes.Count; i++)
                luaWaveTable.Set(i + 1, UserData.Create(waves[indexes[i]]));
            script.SetVar("Wave", DynValue.NewTable(luaWaveTable));
        } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(nextWaves.Table.Get(currentWaveScript + 1).String + ".lua", UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message); }
    }

    public void NextWave() {
        waveBeginTime = Time.time;
        turnCount++;
        PrepareWave();
        if (script.GetVar("wavetimer") != null) waveTimer = Time.time + (float)script.GetVar("wavetimer").Number;
        else                                       waveTimer = Time.time + 4.0f;
    }

    public void EndWave(bool death = false) {
        Table t = script["Wave"].Table;
        if (!death)
            foreach (DynValue obj in t.Keys) {
                try {
                    (t[obj] as ScriptWrapper).Call("EndingWave");
                    ScriptWrapper.instances.Remove((ScriptWrapper)t[obj]);
                } catch { UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "You shouldn't override Wave, now you get an error :P"); }
            }
        if (!GlobalControls.retroMode)
            foreach (LuaProjectile p in FindObjectsOfType<LuaProjectile>())
                if (!p.ctrl.isPersistent)
                    p.ctrl.Remove();
        if (ArenaManager.instance.showWhenWaveEnds)
            ArenaManager.instance.Show();
        if (!death)
            CallOnSelfOrChildren("DefenseEnding");
        if (GlobalControls.retroMode)
            EncounterText = script.GetVar("encountertext").String;
        script.SetVar("Wave", DynValue.NewTable(new Table(null)));
        // Projectile.Z_INDEX_NEXT = Projectile.Z_INDEX_INITIAL; // doesn't work yet
    }

    public void EndWaveTimer() { waveTimer = Time.time; }

    public bool WaveInProgress() { return Time.time < waveTimer; }
}