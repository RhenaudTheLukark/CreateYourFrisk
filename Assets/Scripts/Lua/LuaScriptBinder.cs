using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

/// <summary>
/// Takes care of creating <see cref="Script"/> objects with globally bound functions.
/// Doubles as a dictionary for the SetGlobal/GetGlobal functions attached to these scripts.
/// Is also used to store global variables from the game, to be accessed from Lua scripts.
/// </summary>
public static class LuaScriptBinder {
    private static Dictionary<string, DynValue> dict = new Dictionary<string, DynValue>(), battleDict = new Dictionary<string, DynValue>(), alMightyDict = new Dictionary<string, DynValue>();
    private static MusicManager mgr = new MusicManager();
    private static NewMusicManager newmgr = new NewMusicManager();
    public static List<Script> scriptlist = new List<Script>();

    /// <summary>
    /// Registers C# types with MoonSharp so we can bind them to Lua scripts later.
    /// </summary>
    static LuaScriptBinder() {
        // Battle bindings
        UserData.RegisterType<MusicManager>();
        UserData.RegisterType<NewMusicManager>();
        UserData.RegisterType<ProjectileController>();
        UserData.RegisterType<LuaArenaStatus>();
        UserData.RegisterType<LuaPlayerStatus>();
        UserData.RegisterType<LuaEnemyStatus>();
        UserData.RegisterType<LuaInputBinding>();
        UserData.RegisterType<LuaUnityTime>();
        UserData.RegisterType<ScriptWrapper>();
        UserData.RegisterType<LuaSpriteController>();
        UserData.RegisterType<LuaInventory>();
        UserData.RegisterType<Misc>();
        UserData.RegisterType<LuaTextManager>();
        UserData.RegisterType<LuaFile>();
        UserData.RegisterType<LuaSpriteShader>();
        UserData.RegisterType<LuaSpriteShader.MatrixFourByFour>();
        UserData.RegisterType<LuaDiscord>();

        // Overworld bindings
        UserData.RegisterType<LuaEventOW>();
        UserData.RegisterType<LuaPlayerOW>();
        UserData.RegisterType<LuaGeneralOW>();
        UserData.RegisterType<LuaInventoryOW>();
        UserData.RegisterType<LuaScreenOW>();
        UserData.RegisterType<LuaMapOW>();
    }

    /// <summary>
    /// Generates Script object with globally defined functions and objects bound, and the os/io/file modules taken out.
    /// </summary>
    /// <returns>Script object for use within Unitale</returns>
    public static Script BoundScript(/*bool overworld = false*/) {
        Script script = new Script(CoreModules.Preset_Complete ^ CoreModules.IO ^ CoreModules.OS_System);
        // library support
        script.Options.ScriptLoader = new FileSystemScriptLoader();
        ((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths = new string[] { FileLoader.pathToModFile("Lua/?.lua"), FileLoader.pathToDefaultFile("Lua/?.lua"), FileLoader.pathToModFile("Lua/Libraries/?.lua"), FileLoader.pathToDefaultFile("Lua/Libraries/?.lua") };
        // separate function bindings
        script.Globals["SetGlobal"] = (Action<Script, string, DynValue>)SetBattle;
        script.Globals["GetGlobal"] = (Func<Script, string, DynValue>)GetBattle;
        script.Globals["SetRealGlobal"] = (Action<Script, string, DynValue>)Set;
        script.Globals["GetRealGlobal"] = (Func<Script, string, DynValue>)Get;
        script.Globals["SetAlMightyGlobal"] = (Action<Script, string, DynValue>)SetAlMighty;
        script.Globals["GetAlMightyGlobal"] = (Func<Script, string, DynValue>)GetAlMighty;

        script.Globals["isCYF"] = true;
        script.Globals["safe"] = ControlPanel.instance.Safe;
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            script.Globals["windows"] = true;
        #else
            script.Globals["windows"] = false;
        #endif
        script.Globals["CYFversion"] = GlobalControls.CYFversion;
        if (!UnitaleUtil.IsOverworld) {
            script.Globals["CreateSprite"] = (Func<string, string, int, DynValue>)SpriteUtil.MakeIngameSprite;
            script.Globals["CreateLayer"] = (Func<string, string, bool, bool>)SpriteUtil.CreateLayer;
            script.Globals["CreateProjectileLayer"] = (Action<string, string, bool>)SpriteUtil.CreateProjectileLayer;
            script.Globals["SetFrameBasedMovement"] = (Action<bool>)SetFrameBasedMovement;
            script.Globals["SetAction"] = (Action<string>)SetAction;
            script.Globals["SetPPCollision"] = (Action<bool>)SetPPCollision;
            script.Globals["AllowPlayerDef"] = (Action<bool>)AllowPlayerDef;
            script.Globals["CreateText"] = (Func<Script, DynValue, DynValue, int, string, int, LuaTextManager>)CreateText;
            script.Globals["GetCurrentState"] = (Func<string>)GetState;
            script.Globals["BattleDialog"] = (Action<DynValue>)LuaEnemyEncounter.BattleDialog;
            script.Globals["BattleDialogue"] = (Action<DynValue>)LuaEnemyEncounter.BattleDialog;

            if (LuaEnemyEncounter.doNotGivePreviousEncounterToSelf)
                LuaEnemyEncounter.doNotGivePreviousEncounterToSelf = false;
            else
                script.Globals["Encounter"] = LuaEnemyEncounter.script_ref;

            DynValue PlayerStatus = UserData.Create(PlayerController.luaStatus);
            script.Globals.Set("Player", PlayerStatus);
        } else if (!GlobalControls.isInShop) {
            try {
                DynValue PlayerOW = UserData.Create(EventManager.instance.luaplow);
                script.Globals.Set("FPlayer", PlayerOW);
                DynValue EventOW = UserData.Create(EventManager.instance.luaevow);
                script.Globals.Set("FEvent", EventOW);
                DynValue GeneralOW = UserData.Create(EventManager.instance.luagenow);
                script.Globals.Set("FGeneral", GeneralOW);
                DynValue InventoryOW = UserData.Create(EventManager.instance.luainvow);
                script.Globals.Set("FInventory", InventoryOW);
                DynValue ScreenOW = UserData.Create(EventManager.instance.luascrow);
                script.Globals.Set("FScreen", ScreenOW);
                DynValue MapOW = UserData.Create(EventManager.instance.luamapow);
                script.Globals.Set("FMap", MapOW);
            } catch { }
        }
        script.Globals["DEBUG"] = (Action<string>)UnitaleUtil.WriteInLogAndDebugger;
        script.Globals["EnableDebugger"] = (Action<bool>)EnableDebugger;
        // clr bindings
        DynValue MusicMgr = UserData.Create(mgr);
        script.Globals.Set("Audio", MusicMgr);
        DynValue NewMusicMgr = UserData.Create(newmgr);
        script.Globals.Set("NewAudio", NewMusicMgr);
        // What? Why?
        bool emptyInventory = false;
        if (Inventory.inventory.Count == 0) {
            Inventory.inventory.Add(new UnderItem("Testing Dog"));
            emptyInventory = true;
        }
        DynValue inv = UserData.Create(Inventory.luaInventory);
        script.Globals.Set("Inventory", inv);
        if (emptyInventory)
            Inventory.inventory.Clear();
        DynValue InputMgr = UserData.Create(GlobalControls.luaInput);
        script.Globals.Set("Input", InputMgr);
        DynValue Win = UserData.Create(new Misc());
        script.Globals.Set("Misc", Win);
        DynValue TimeInfo = UserData.Create(new LuaUnityTime());
        script.Globals.Set("Time", TimeInfo);
        DynValue DiscordMgr = UserData.Create(new LuaDiscord());
        script.Globals.Set("Discord", DiscordMgr);
        scriptlist.Add(script);
        return script;
    }

    private delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    private delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg, T6 arg6);

    public static string GetState() {
        try {
            return (UIController.instance.frozenState != UIController.UIState.PAUSE) ? UIController.instance.frozenState.ToString() : UIController.instance.state.ToString();
        } catch {
            return "NONE (error)";
        }
    }

    public static DynValue Get(Script script, string key) {
        if (key == null)
            throw new CYFException("GetRealGlobal: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (dict.ContainsKey(key)) {
            // Due to how MoonSharp tables require an owner, we have to create an entirely new table if we want to work with it in other scripts.
            if (dict[key].Type == DataType.Table) {
                DynValue t = DynValue.NewTable(script);
                foreach (TablePair pair in dict[key].Table.Pairs)
                    t.Table.Set(pair.Key, pair.Value);
                return t;
            } else
                return dict[key];
        }
        return null;
    }

    public static void Set(Script script, string key, DynValue value) {
        if (key == null)
            throw new CYFException("SetRealGlobal: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);
    }

    public static DynValue GetBattle(Script script, string key) {
        if (key == null)
            throw new CYFException("GetGlobal: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (battleDict.ContainsKey(key)) {
            // Due to how MoonSharp tables require an owner, we have to create an entirely new table if we want to work with it in other scripts.
            if (battleDict[key].Type == DataType.Table) {
                DynValue t = DynValue.NewTable(script);
                foreach (TablePair pair in battleDict[key].Table.Pairs)
                    t.Table.Set(pair.Key, pair.Value);
                return t;
            } else
            return battleDict[key];
        }
        return null;
    }

    public static void SetBattle(Script script, string key, DynValue value) {
        if (key == null)
            throw new CYFException("SetGlobal: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (battleDict.ContainsKey(key))
            battleDict.Remove(key);
        battleDict.Add(key, value);
    }

    public static DynValue GetAlMighty(Script script, string key) {
        if (key == null)
            throw new CYFException("GetAlMightyGlobal: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (alMightyDict.ContainsKey(key)) {
            // Due to how MoonSharp tables require an owner, we have to create an entirely new table if we want to work with it in other scripts.
            if (alMightyDict[key].Type == DataType.Table) {
                DynValue t = DynValue.NewTable(script);
                foreach (TablePair pair in alMightyDict[key].Table.Pairs)
                    t.Table.Set(pair.Key, pair.Value);
                return t;
            } else
                return alMightyDict[key];
        }
        return null;
    }

    public static void SetAlMighty(Script script, string key, DynValue value) { SetAlMighty(script, key, value, true); }
    public static void SetAlMighty(Script script, string key, DynValue value, bool reload) {
        if (key == null)
            throw new CYFException("SetAlMightyGlobal: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        if (alMightyDict.ContainsKey(key))
            alMightyDict.Remove(key);
        alMightyDict.Add(key, value);
        if (reload)
            SaveLoad.SaveAlMighty();
    }

    /// <summary>
    /// Clears the global dictionary. Used in the reset functionality, as everything is reinitialized.
    /// </summary>
    public static void Clear() { dict.Clear(); }

    public static void ClearBattleVar() {
        Dictionary<string, DynValue> a = dict;
        battleDict.Clear();
        dict = a;
    }

    public static void ClearAlMighty() {
        alMightyDict.Clear();
        SaveLoad.SaveAlMighty();
    }

    public static void ClearVariables() {
        dict.Clear();
        UserData.RegisterType<MusicManager>();
        UserData.RegisterType<NewMusicManager>();
        UserData.RegisterType<ProjectileController>();
        UserData.RegisterType<LuaArenaStatus>();
        UserData.RegisterType<LuaPlayerStatus>();
        UserData.RegisterType<LuaEnemyStatus>();
        UserData.RegisterType<LuaInputBinding>();
        UserData.RegisterType<LuaUnityTime>();
        UserData.RegisterType<ScriptWrapper>();
        UserData.RegisterType<LuaSpriteController>();
        UserData.RegisterType<LuaInventory>();
        UserData.RegisterType<Misc>();
        UserData.RegisterType<LuaTextManager>();
        UserData.RegisterType<LuaFile>();
        //UserData.RegisterType<Windows>();
        //Overworld
        UserData.RegisterType<LuaEventOW>();
        UserData.RegisterType<LuaPlayerOW>();
        UserData.RegisterType<LuaGeneralOW>();
        UserData.RegisterType<LuaInventoryOW>();
        UserData.RegisterType<LuaScreenOW>();
        UserData.RegisterType<LuaMapOW>();
    }

    /// <summary>
    /// Returns this script's dictionaries
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, DynValue> GetSavedDictionary()     { return dict; }
    public static Dictionary<string, DynValue> GetBattleDictionary()    { return battleDict; }
    public static Dictionary<string, DynValue> GetAlMightyDictionary()  { return alMightyDict; }

    /// <summary>
    /// Replaces the current dictionary with the given one.
    /// /!\ THIS ERASES THE CURRENT DICTIONARY /!\
    /// </summary>
    /// <param name="newDict"></param>
    public static void SetSavedDictionary(Dictionary<string, DynValue> newDict)          { dict = newDict; }
    public static void SetBattleDictionary(Dictionary<string, DynValue> newDict)    { battleDict = newDict; }
    public static void SetAlMightyDictionary(Dictionary<string, DynValue> newDict)  { alMightyDict = newDict; }

    /// <summary>
    /// Removes one or several keys from the dictionaries.
    /// </summary>
    /// <param name="str"></param>
    public static void Remove(string str)                { dict.Remove(str); }
    public static void Remove(List<string> list)         { foreach (string str in list) dict.Remove(str); }
    public static void Remove(string[] strs)             { foreach (string str in strs) dict.Remove(str); }
    public static void RemoveBattle(string str)          { battleDict.Remove(str); }
    public static void RemoveBattle(List<string> list)   { foreach (string str in list) battleDict.Remove(str); }
    public static void RemoveBattle(string[] strs)       { foreach (string str in strs) battleDict.Remove(str); }
    public static void RemoveAlMighty(string str)        { alMightyDict.Remove(str); }
    public static void RemoveAlMighty(List<string> list) { foreach (string str in list) alMightyDict.Remove(str); }
    public static void RemoveAlMighty(string[] strs)     { foreach (string str in strs) alMightyDict.Remove(str); }

    /// <summary>
    /// Returns a list that contains all keys that contains the string given in argument.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static List<string> GetKeysWithString(string str) {
        List<string> list = new List<string>();
        foreach (string key in dict.Keys)
            if (key.Contains(str))
                list.Add(key);
        return list;
    }

    public static void CopyToBattleVar() {
        dict["CYFSwitch"] = DynValue.NewBoolean(true);
        foreach (string key in dict.Keys) {
            DynValue temp;
            dict.TryGetValue(key, out temp);
            SetBattle(null, key, temp);
        }
    }

    public static void SetFrameBasedMovement(bool b) { ControlPanel.instance.FrameBasedMovement = b; }

    public static void SetAction(string action) {
        try {
            UIController.instance.forcedaction = (UIController.Actions)Enum.Parse(typeof(UIController.Actions), action, true);
            if (((GetState() == "ACTIONSELECT" && UIController.instance.frozenState == UIController.UIState.PAUSE) || !UIController.instance.stated) && UIController.instance.forcedaction != UIController.Actions.NONE)
                UIController.instance.MovePlayerToAction(UIController.instance.forcedaction);
        } catch { throw new CYFException("SetAction() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + action + "\"."); }
    }

    public static void SetPPCollision(bool b) {
        GlobalControls.ppcollision = b;
        foreach (LuaProjectile p in GameObject.Find("Canvas").GetComponentsInChildren<LuaProjectile>(true))
            if (!p.ppchanged)
                p.ppcollision = b;
    }

    public static void AllowPlayerDef(bool b) { GlobalControls.allowplayerdef = b; }

    public static void SetPPAlphaLimit(float f) {
        if (f < 0 || f > 1)  UnitaleUtil.DisplayLuaError("Pixel-Perfect alpha limit", "The alpha limit should be between 0 and 1.");
        else                 ControlPanel.instance.MinimumAlpha = f;
    }

    public static LuaTextManager CreateText(Script scr, DynValue text, DynValue position, int textWidth, string layer = "BelowPlayer", int bubbleHeight = -1) {
        // Check if the arguments are what they should be
        if (text == null || (text.Type != DataType.Table && text.Type != DataType.String))
            throw new CYFException("CreateText: The text argument must be a non-empty table of strings or a simple string.");
        if (position == null || position.Type != DataType.Table || position.Table.Get(1).Type != DataType.Number || position.Table.Get(2).Type != DataType.Number)
            throw new CYFException("CreateText: The position argument must be a non-empty table of two numbers.");

        GameObject go = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CstmTxtContainer"));
        LuaTextManager luatm = go.GetComponentInChildren<LuaTextManager>();
        go.GetComponent<RectTransform>().position = new Vector2((float)position.Table.Get(1).Number, (float)position.Table.Get(2).Number);

        UnitaleUtil.GetChildPerName(go.transform, "BubbleContainer").GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        UnitaleUtil.GetChildPerName(go.transform, "BubbleContainer").GetComponent<RectTransform>().localPosition = new Vector2(-12, 8);
        UnitaleUtil.GetChildPerName(go.transform, "BubbleContainer").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 20, 100);     //Used to set the borders
        UnitaleUtil.GetChildPerName(go.transform, "BackHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 20, 100 - 20 * 2);   //BackHorz
        UnitaleUtil.GetChildPerName(go.transform, "BackVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth - 20, 100);            //BackVert
        UnitaleUtil.GetChildPerName(go.transform, "CenterHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 16, 96 - 16 * 2);  //CenterHorz
        UnitaleUtil.GetChildPerName(go.transform, "CenterVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth - 16, 96);           //CenterVert
        foreach (ScriptWrapper scrWrap in ScriptWrapper.instances) {
            if (scrWrap.script == scr) {
                luatm.SetCaller(scrWrap);
                break;
            }
        }
        // Layers don't exist in the overworld, so we don't set it
        if (!UnitaleUtil.IsOverworld || GlobalControls.isInShop)
            luatm.layer = layer;
        else
            luatm.layer = (layer == "BelowPlayer" ? "Default" : layer);
        luatm.textMaxWidth = textWidth;
        luatm.bubbleHeight = bubbleHeight;
        luatm.ShowBubble();

        // Converts the text argument into a table if it's a simple string
        text = text.Type == DataType.String ? DynValue.NewTable(scr, new DynValue[1] { text }) : text;

        if (text.Table.Length == 0)
            text = null;

        //////////////////////////////////////////
        ///////////  LATE START SETTER  //////////
        //////////////////////////////////////////

        // Text objects' Late Start will be disabled if the first line of text contains [instant] before any regular characters
        bool enableLateStart = true;

        // if we've made it this far, then the text is valid.

        // so, let's scan the first line of text for [instant]
        string firstLine = text.Table.Get(1).String;

        // if [instant] or [instant:allowcommand] is found, check for the earliest match, and whether it is at the beginning
        if (firstLine.IndexOf("[instant]") > -1 || firstLine.IndexOf("[instant:allowcommand]") > -1) {
            // determine whether [instant] or [instant:allowcommand] is first
            string testFor = "[instant]";
            if (firstLine.IndexOf("[instant:allowcommand]") > -1 &&
                ((firstLine.IndexOf("[instant]") > -1 && firstLine.IndexOf("[instant:allowcommand]") < firstLine.IndexOf("[instant]"))
                || firstLine.IndexOf("[instant]") == -1)) {
                testFor = "[instant:allowcommand]";
            }

            // grab all of the text that comes before the matched command
            string precedingText = firstLine.Substring(0, firstLine.IndexOf(testFor));

            // remove all commands other than the matched command from this variable
            while (precedingText.IndexOf('[') > -1) {
                int i = 0;
                if (UnitaleUtil.ParseCommandInline(precedingText, ref i) == null) break;
                else                                                              precedingText = precedingText.Replace(precedingText.Substring(0, i + 1), "");
            }

            // if the length of the remaining string is 0, then disable late start!
            if (precedingText.Length == 0)
                enableLateStart = false;
        }

        //////////////////////////////////////////
        /////////// INITIAL FONT SETTER //////////
        //////////////////////////////////////////

        // If the first line of text has [font] at the beginning, use it initially!
        if (firstLine.IndexOf("[font:") > -1 && firstLine.Substring(firstLine.IndexOf("[font:")).IndexOf(']') > -1) {
            // grab all of the text that comes before the matched command
            string precedingText = firstLine.Substring(0, firstLine.IndexOf("[font:"));

            // remove all commands other than the matched command from this variable
            while (precedingText.IndexOf('[') > -1) {
                int i = 0;
                if (UnitaleUtil.ParseCommandInline(precedingText, ref i) == null) break;
                else                                                              precedingText = precedingText.Replace(precedingText.Substring(0, i + 1), "");
            }

            // if the length of the remaining string is 0, then set the font!
            if (precedingText.Length == 0) {
                int startCommand = firstLine.IndexOf("[font:");
                string command = UnitaleUtil.ParseCommandInline(precedingText, ref startCommand);
                if (command != null) {
                    string fontPartOne = command.Substring(6);
                    string fontPartTwo = fontPartOne.Substring(0, fontPartOne.IndexOf("]"));
                    UnderFont font = SpriteFontRegistry.Get(fontPartTwo);
                    if (font == null)
                        throw new CYFException("The font \"" + fontPartTwo + "\" doesn't exist.\nYou should check if you made a typo, or if the font really is in your mod.");
                    luatm.SetFont(font, true);
                    luatm.UpdateBubble();
                } else luatm.ResetFont();
            } else     luatm.ResetFont();
        } else         luatm.ResetFont();

        if (enableLateStart)
            luatm.lateStartWaiting = true;
        luatm.SetText(text);
        if (enableLateStart) {
            luatm.DestroyChars();
            luatm.LateStart();
        }
        return luatm;
    }

    public static void SetButtonLayer(string layer) {
        GameObject obj1 = GameObject.Find("Stats");
        GameObject obj2 = GameObject.Find("UIRect");
        Transform parent1 = obj1.transform.parent;
        Transform parent2 = obj2.transform.parent;
        try {
            if (layer == "default") {
                obj1.transform.SetParent(GameObject.Find("Canvas").transform);
                obj1.transform.SetSiblingIndex(obj1.transform.parent.GetComponentInChildren<UIController>().transform.GetSiblingIndex() + 1);
                obj2.transform.SetParent(GameObject.Find("Canvas").transform);
                obj2.transform.SetSiblingIndex(obj2.transform.parent.GetComponentInChildren<UIController>().transform.GetSiblingIndex() + 1);
            } else {
                obj1.transform.SetParent(GameObject.Find(layer + "Layer").transform);
                obj2.transform.SetParent(GameObject.Find(layer + "Layer").transform);
            }
        }
        catch {
            obj1.transform.SetParent(parent1);
            obj2.transform.SetParent(parent2);
        }
    }

    public static void EnableDebugger(bool state) {
        if (UserDebugger.instance == null)
            return;

        UserDebugger.instance.canShow = state;
        if (!state && UserDebugger.instance.gameObject.activeSelf) {
            UserDebugger.instance.gameObject.SetActive(false);
            Camera.main.GetComponent<FPSDisplay>().enabled = false;
        }
    }
}
