using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

internal class LuaEnemyEncounter : EnemyEncounter {
    public static ScriptWrapper script;
    internal static ScriptWrapper script_ref;
    public float waveBeginTime = 0f;
    private ScriptWrapper[] waves;
    private string[] waveNames;
    public bool gameOverStance = false;
    public static bool doNotGivePreviousEncounterToSelf = false;

    public override Vector2 ArenaSize {
        get {
            if (script.GetVar("arenasize") != null) {
                Table size = script.GetVar("arenasize").Table;
                if (size == null)
                    return base.ArenaSize;
                if (size.Get(1).Number < 16 || size.Get(2).Number < 16) // TODO remove hardcoding (but player never changes size so nobody cares
                    return new Vector2(size.Get(1).Number > 16 ? (int)size.Get(1).Number : 16, size.Get(2).Number > 16 ? (int)size.Get(2).Number : 16);
                return new Vector2((int)size.Get(1).Number, (int)size.Get(2).Number);
            }
            return base.ArenaSize;
        }
    }

    private delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    private bool InitScript {
        get {
            doNotGivePreviousEncounterToSelf = true;
            script = new ScriptWrapper() { scriptname = StaticInits.ENCOUNTER };
            string scriptText = ScriptRegistry.Get(ScriptRegistry.ENCOUNTER_PREFIX + StaticInits.ENCOUNTER);
            try { script.DoString(scriptText); }
            catch (InterpreterException ex) {
                UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message, ex.DoNotDecorateMessage);
                return false;
            }
            script.Bind("State", (Action<Script, string>)UIController.SwitchStateOnString);
            script.Bind("RandomEncounterText", (Func<string>)RandomEncounterText);
            script.Bind("CreateProjectile", (Func<Script, string, float, float, string, DynValue>)CreateProjectile);
            script.Bind("CreateProjectileAbs", (Func<Script, string, float, float, string, DynValue>)CreateProjectileAbs);
            script.Bind("SetButtonLayer", (Action<string>)LuaScriptBinder.SetButtonLayer);
            script_ref = script;
            return true;
        }
    }

    [HideInInspector] public DynValue CreateProjectileAbs(Script s, string sprite, float xpos, float ypos, string layerName = "") {
        LuaProjectile projectile = (LuaProjectile)BulletPool.instance.Retrieve();
        if (sprite == null)
            throw new CYFException("You can't create a projectile with a nil sprite!");
        SpriteUtil.SwapSpriteFromFile(projectile, sprite);
        projectile.name = sprite;
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
                catch { }
            }
        DynValue projectileController = UserData.Create(projectile.ctrl);
        //Texture2D tex = (Texture2D)projectile.GetComponent<Image>().mainTexture;
        //projectile.selfAbs = UnitaleUtil.GetFurthestCoordinates(tex.GetPixels32(), tex.height, projectile.self);

        return projectileController;
    }

    private DynValue CreateProjectile(Script s, string sprite, float xpos, float ypos, string layerName = "") {
        return CreateProjectileAbs(s, sprite, ArenaManager.arenaCenter.x + xpos, ArenaManager.arenaCenter.y + ypos, layerName);
    }

    private void PrepareWave() {
        DynValue nextWaves = script.GetVar("nextwaves");
        if (nextWaves.Type != DataType.Table) {
            if (nextWaves.Type == DataType.Nil)
                UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "nextwaves is not defined in your script.");
            else
                UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "nextwaves is a " + nextWaves.Type.ToString() + ", but should be a table.");
            return;
        }
        waves = new ScriptWrapper[nextWaves.Table.Length];
        waveNames = new string[waves.Length];
        int currentWaveScript = 0;
        try {
            List<int> indexes = new List<int>();
            for (int i = 0; i < waves.Length; i++) {
                currentWaveScript = i;
                DynValue ArenaStatus = UserData.Create(ArenaManager.luaStatus);
                waves[i] = new ScriptWrapper() { scriptname = nextWaves.Table.Get(i + 1).String };
                waves[i].script.Globals.Set("Arena", ArenaStatus);
                waves[i].script.Globals["EndWave"] = (Action)EndWaveTimer;
                waves[i].script.Globals["State"] = (Action<Script, string>)UIController.SwitchStateOnString;
                waves[i].script.Globals["CreateProjectile"] = (Func<Script, string, float, float, string, DynValue>)CreateProjectile;
                waves[i].script.Globals["CreateProjectileAbs"] = (Func<Script, string, float, float, string, DynValue>)CreateProjectileAbs;
                if (nextWaves.Table.Get(i + 1).Type != DataType.String){
                    UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "Non-string value encountered in nextwaves table");
                    return;
                } else
                    waveNames[i] = nextWaves.Table.Get(i + 1).String;
                waves[i].script.Globals["wavename"] = nextWaves.Table.Get(i + 1).String;
                try {
                    waves[i].DoString(ScriptRegistry.Get(ScriptRegistry.WAVE_PREFIX + nextWaves.Table.Get(i + 1).String));
                    indexes.Add(i);
                } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(nextWaves.Table.Get(i + 1).String + ".lua", UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
                } catch (Exception ex) {
                    if (!GlobalControls.retroMode &&!ScriptRegistry.dict.ContainsKey(ScriptRegistry.WAVE_PREFIX + nextWaves.Table.Get(i + 1).String))
                        UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "The wave \"" + nextWaves.Table.Get(i + 1).String + "\" doesn't exist.");
                    else
                        UnitaleUtil.DisplayLuaError("<UNKNOWN LOCATION>", ex.Message + "\n\n" + ex.StackTrace);
                }
            }
            Table luaWaveTable = new Table(null);
            for (int i = 0; i < indexes.Count; i++)
                luaWaveTable.Set(i + 1, UserData.Create(waves[indexes[i]]));
            script.SetVar("Wave", DynValue.NewTable(luaWaveTable));
        } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(nextWaves.Table.Get(currentWaveScript + 1).String + ".lua", UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message); }
    }

    public void Awake() {
        if (InitScript)
            LoadEnemiesAndPositions();
        CanRun = true;
    }

    protected override void LoadEnemiesAndPositions() {
        AudioSource musicSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        EncounterText = script.GetVar("encountertext").String;
        DynValue enemyScriptsLua = script.GetVar("enemies");
        DynValue enemyPositionsLua = script.GetVar("enemypositions");
        string musicFile = script.GetVar("music").String;

        try { enemies = new LuaEnemyController[enemyScriptsLua.Table.Length]; /*dangerously assumes enemies is defined*/ }
        catch (Exception) {
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "There's no enemies table in your encounter. Is this a pre-0.1.2 encounter? It's easy to fix!\n\n"
                + "1. Create a Monsters folder in the mod's Lua folder\n"
                + "2. Add the monster script (custom.lua) to this new folder\n"
                + "3. Add the following line to the beginning of this encounter script, located in the mod folder/Lua/Encounters:\nenemies = {\"custom\"}\n"
                + "4. You're done! Starting from 0.1.2, you can name your monster and encounter scripts anything.");
            return;
        }
        if (enemyPositionsLua != null && enemyPositionsLua.Table != null) {
            enemyPositions = new Vector2[enemyPositionsLua.Table.Length];
            for (int i = 0; i < enemyPositionsLua.Table.Length; i++) {
                Table posTable = enemyPositionsLua.Table.Get(i + 1).Table;
                if (i >= enemies.Length)
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
        if (enemies.Length > enemyPositions.Length) {
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "All enemies in an encounter must have a screen position defined. Either your enemypositions table is missing, "
                + "or there are more enemies than available positions. Refer to the documentation's Basic Setup section on how to do this.");
        }
        enemyInstances = new GameObject[enemies.Length];
        for (int i = 0; i < enemies.Length; i++) {
            enemyInstances[i] = Instantiate(Resources.Load<GameObject>("Prefabs/LUAEnemy 1"));
            enemyInstances[i].transform.SetParent(gameObject.transform);
            enemyInstances[i].transform.localScale = new Vector3(1, 1, 1); // apparently this was suddenly required or the scale would be (0,0,0)
            enemies[i] = enemyInstances[i].GetComponent<LuaEnemyController>();
            enemies[i].scriptName = enemyScriptsLua.Table.Get(i + 1).String;
            enemies[i].index = i;
            if (i < enemyPositions.Length)
                enemies[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(enemyPositions[i].x, enemyPositions[i].y);
            else
                enemies[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 1);
        }

        // Attach the controllers to the encounter's enemies table
        DynValue[] enemyStatusCtrl = new DynValue[enemies.Length];
        Table luaEnemyTable = script.GetVar("enemies").Table;
        for (int i = 0; i < enemyStatusCtrl.Length; i++) {
            //enemies[i].luaStatus = new LuaEnemyStatus(enemies[i]);
            enemies[i].script = new ScriptWrapper();
            luaEnemyTable.Set(i + 1, UserData.Create(enemies[i].script));
        }
        script.SetVar("enemies", DynValue.NewTable(luaEnemyTable));
        Table luaWaveTable = new Table(null);
        script.SetVar("Wave", DynValue.NewTable(luaWaveTable));

        //if (MusicManager.isStoppedOrNull(PlayerOverworld.audioKept))
        //    musicSource.Play(); // play that funky music
    }

    /*public override void HandleItem(UnderItem item) {
        //if (!CustomItemHandler(item))
        //item.inCombatUse();
        Inventory.UseItem(item.Name);
    }*/

    public void HandleItem(int ID) {
        //item.inCombatUse();
        Inventory.UseItem(ID);
    }

    public bool CallOnSelfOrChildren(string func, DynValue[] param = null) {
        bool result;
        if (param != null)
            result = TryCall(func, param);
        else
            result = TryCall(func);

        if (!result) {
            bool calledOne = false;
            foreach (LuaEnemyController enemy in enemies) {
                if (param != null) {
                    if (enemy.TryCall(func, param))
                        calledOne = true;
                } else if (enemy.TryCall(func))
                        calledOne = true;
            }
            return calledOne;
        }
        return true;
    }

    public bool TryCall(string func, DynValue[] param = null) {
        try {
            if (script.GetVar(func) == null) return false;
            if (param != null)               script.Call(func, param);
            else                             script.Call(func);
            return true;
        } catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            return true;
        }
    }

    public override void HandleSpare() {
        /*
        if (script.GetVar("HandleSpare") == null)
            base.HandleSpare();
        else
            if (!script.Call(script.Globals["HandleSpare"]).Boolean)
                base.HandleSpare();
         */
        base.HandleSpare();
    }

    ///<summary>
    ///Overrideable item handler on a per-encounter basis. Should return true if a custom action is executed for the given item.
    ///</summary>
    ///<param name="item">Item to be checked for custom action</param>
    ///<returns>true if a custom action should be executed for given item, false if the default action should happen</returns>
    //public override bool CustomItemHandler(UnderItem item) { return CallOnSelfOrChildren("HandleItem", new DynValue[] { DynValue.NewString(item.Name) }); }

    public override void UpdateWave() {
        string currentScript = "";
        try {
            for (int i = 0; i < waves.Length; i++) {
                currentScript = waveNames[i];
                try { waves[i].script.Call(waves[i].script.Globals["Update"]); }
                catch (InterpreterException ex) {
                    UnitaleUtil.DisplayLuaError(currentScript, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
                    return;
                } catch (Exception ex) {
                    if (!GlobalControls.retroMode) {
                        if (waves[i].script.Globals["Update"] == null)
                            UnitaleUtil.DisplayLuaError(currentScript, "All the wave scripts need an Update() function!");
                        else
                            UnitaleUtil.DisplayLuaError(currentScript, "This error is a " + ex.GetType().ToString() + " error.\nPlease send this error to the main dev.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
                    }
                    return;
                }
            }
        } catch (InterpreterException ex) {
            UnitaleUtil.DisplayLuaError(currentScript, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            return;
        }
    }

    public override void NextWave() {
        waveBeginTime = Time.time;
        turnCount++;
        PrepareWave();
        if (script.GetVar("wavetimer") != null)
            waveTimer = Time.time + (float)script.GetVar("wavetimer").Number;
        else
            waveTimer = Time.time + 4.0f;
    }

    public void EndWaveTimer() { waveTimer = Time.time; }

    public override void EndWave(bool death = false) {
        ArenaManager.instance.resetArena();
        Table t = script["Wave"].Table;
        if (!death)
            foreach (object obj in t.Keys) {
                try {
                    ((ScriptWrapper)t[obj]).Call("EndingWave");
                    ScriptWrapper.instances.Remove(((ScriptWrapper)t[obj]));
                    LuaScriptBinder.scriptlist.Remove(((ScriptWrapper)t[obj]).script);
                } catch { UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "You shouldn't override Wave, now you get an error :P"); }
            }
        if (!GlobalControls.retroMode)
            foreach (LuaProjectile p in FindObjectsOfType<LuaProjectile>())
                if (!p.ctrl.isPersistent)
                    p.ctrl.Remove();
        if (!death)
            CallOnSelfOrChildren("DefenseEnding");
        if (GlobalControls.retroMode) {
            EncounterText = script.GetVar("encountertext").String;
        }
        script.SetVar("Wave", DynValue.NewTable(new Table(null)));
        // Projectile.Z_INDEX_NEXT = Projectile.Z_INDEX_INITIAL; // doesn't work yet
    }

    public new bool WaveInProgress() { return Time.time < waveTimer; }

    public static void BattleDialog(DynValue arg) {
        if (UIController.instance == null)
            UnitaleUtil.Warn("BattleDialog can only be used as early as EncounterStarting.");
        else {
            UIController.instance.battleDialogued = true;
            TextMessage[] msgs = null;
            if (arg.Type == DataType.String)
                msgs = new TextMessage[]{new RegularMessage(arg.String)};
            else if (arg.Type == DataType.Table && (GlobalControls.retroMode || arg.Table.Length > 0)) {
                msgs = new TextMessage[arg.Table.Length];
                for (int i = 0; i < arg.Table.Length; i++)
                    msgs[i] = new RegularMessage(arg.Table.Get(i + 1).String);
            } else if (!GlobalControls.retroMode)
                UnitaleUtil.DisplayLuaError("BattleDialog", "You need to input a non-empty array or a string here." +
                                                            "\n\nIf you're sure that you've entered what's needed, you may contact the dev.");
            if (!GlobalControls.retroMode)
                UIController.instance.textmgr.SetEffect(new TwitchEffect(UIController.instance.textmgr));
            UIController.instance.ActionDialogResult(msgs, UIController.UIState.ENEMYDIALOGUE);
        }
    }

    /*public static void BattleDialog(List<string> lines) {
        TextMessage[] msgs = new TextMessage[lines.Count];
        for (int i = 0; i < lines.Count; i++)
            msgs[i] = new RegularMessage(lines[i]);
        UIController.instance.ActionDialogResult(msgs, UIController.UIState.ENEMYDIALOGUE);
    }*/
}