﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The class responsible for making some people lose faith in the project. In very dire need of refactoring,
/// but hard to do until functionality can be split into battle and overworld functions.
///
/// As it stands this class is a messy finite state machine that takes care of controlling not only the battle,
/// but also a lot of things it shouldn't (text manager, enemy dialogue, keyboard controls etc.)
/// If you're familiar with the term cyclomatic complexity, you probably wouldn't want to hire me at this point.
///
/// The eventual redesign of the UI controller will try to change over as much of the functionality to Lua.
/// As we're missing some key functionality to accomplish this, refactoring has been put off for now.
/// </summary>
public class UIController : MonoBehaviour {
    public static UIController instance;    // The instance of this class, only one UIController should exist at all times
    public TextManager mainTextManager;     // Main text manager in the arena

    public static Sprite fightButtonSprite, actButtonSprite, itemButtonSprite, mercyButtonSprite;  // UI button sprites when the soul is selecting them
    public Image fightButton, actButton, itemButton, mercyButton;                                   // UI button objects in the scene

    public Actions action = Actions.FIGHT;     // Current action chosen when entering the state ENEMYSELECT
    public Actions forcedAction = Actions.NONE; // Action forced by the user previously for the next time we enter the state ENEMYSELECT

    private GameObject arenaParent; // Arena's parent, which will be used to manipulate it
    public GameObject psContainer;  // Container for any particle effect used when using sprite.Dust() and when sparing or killing an enemy
    private AudioSource uiAudio;    // AudioSource only used to play the sound menumove when the Player moves in menus

    internal EnemyEncounter encounter;               // Main encounter script
    [HideInInspector] public FightUIController fightUI; // Main Player attack handler

    private readonly Vector2 initialHealthPos = new Vector2(250, -10); // Initial health bar position for target selection

    public TextManager[] monsterDialogues;  // Enemies' dialogue bubbles' text objects appearing in the state ENEMYDIALOGUE
    public int[] monsterDialogueEnemyID;    // Stores the ID of the associated enemy

    private bool musicPausedFromRunning;    // Used to pause the BGM when trying to flee in retromode for a comedic effect
    private int runAwayAttempts;            // Amount of times the Player tried to flee unsuccessfully in this encounter

    private int selectedAction; // Act option chosen by the Player
    private int selectedEnemy;  // Enemy chosen by the Player
    private int selectedItem;   // Item chosen by the Player
    private int selectedMercy;  // Mercy option chosen by the Player

    private bool[] disabledActions = { false, false, false, false }; // Actions disabled by the player
    public Vector2[] playerOffsets = { new Vector2(16, 19), new Vector2(16, 19), new Vector2(16, 19), new Vector2(16, 19) }; // Player can customize its position on the button.

    private int meCry;  // Used to display which dialogue should be displayed if the MECRY button has been selected, in CrateYourFrisk mode

    public int exp = 0;     // Amount of EXP earned by the Player at the end of the encounter
    public int gold = 0;    // Amount of Gold earned by the Player at the end of the encounter

    internal string state = "ACTIONSELECT";              // Current state of the battle
    private string stateAfterDialogs = "DEFENDING";      // State to enter after the current arena dialogue is done. Only used after a proper call to BattleDialog()
    private string lastNewState = "UNUSED";              // Allows the detection of state changes during an OnDeath() call so the engine can switch to it properly

    private readonly Vector2 upperLeft = new Vector2(-255, 100);    // Coordinates of the first choice in a choice text
    private bool encounterHasUpdate;                                // True if the encounter has an Update function, false otherwise
    private bool parentStateCall = true;                            // Used to stop the execution of a previous State() call if a new call has been done and to prevent infinite EnteringState() loops
    private bool childStateCalled;                                  // Used to stop the execution of a previous State() call if a new call has been done and to prevent infinite EnteringState() loops
    private bool fleeSwitch;                                        // True if the Player fled, and the encounter can be ended
    public Dictionary<int, string[]> messages;                      // Stores the messages enemies will say in the state ENEMYDIALOGUE
    public bool[] readyToNextLine;                                  // Used to know which enemy bubbles are done displaying their text
    public bool needOnDeath;                                        // Used to force the check on whether the enemies are dead or not
    private bool onDeathSwitch;                                     // Allows to switch to a given state if State() was used in OnDeath()
    public bool stateSwitched;                                      // True if the state has been changed this frame, false otherwise
    public bool battleDialogueStarted;                              // True if the battle dialog is being displayed, false otherwise. Only used for the state ITEMMENU, and not updated outside of it

    public enum Actions { FIGHT, ACT, ITEM, MERCY, NONE }   // Action enumeration used to know which main UI button we're selecting or we chose
    // Dictionaries used to link values gracefully
    public Dictionary<string, Image> buttonDictionary = new Dictionary<string, Image>();
    public Dictionary<string, Sprite> buttonSpriteDictionary = new Dictionary<string, Sprite>();
    public Dictionary<string, Vector2> buttonBasePositions = new Dictionary<string, Vector2>();
    public Dictionary<string, Vector2> buttonBasePlayerPositions = new Dictionary<string, Vector2>();

    /*
    public enum UIState {
        NONE,           // Initial state. Used to see if a modder has changed the state before the UI controller wants to
        ACTIONSELECT,   // Selecting an action (FIGHT/ACT/ITEM/MERCY)
        ATTACKING,      // Player attack screen
        DEFENDING,      // Enemy attack phase, bullet waves appear here
        ENEMYSELECT,    // Selecting an enemy target for FIGHT or ACT
        ACTMENU,        // Open up the act menu
        ITEMMENU,       // Open up the item menu
        MERCYMENU,      // Open up the mercy menu
        ENEMYDIALOGUE,  // The Player is visible and the arena is resizing, but the enemy still has own dialogue
        DIALOGRESULT,   // Transition state leading to either UIState.ENEMYDIALOGUE or UIState.DEFENDING
        DONE,           // Finished state of battle. Returns the Player to the mod selection screen or the overworld
        UNUSED,         // Used for OnDeath(). Keep this state secret, please
        PAUSE           // Used exclusively for State("PAUSE"). Not a real state, but it needs to be listed to allow users to call State("PAUSE")
    }*/

    public List<string> UIStates = new List<string>() {"NONE", "ACTIONSELECT", "ATTACKING", "DEFENDING", "ENEMYSELECT", "ACTMENU", "ITEMMENU", "MERCYMENU", "ENEMYDIALOGUE", "DIALOGRESULT", "DONE", "UNUSED", "PAUSE"};

    // Variables for PAUSE's "encounter freezing" behavior
    public string frozenState = "PAUSE"; // Used to keep track of what state was frozen
    public float frozenTimestamp;               // Used for DEFENDING's wavetimer
    private bool frozenControlOverride = true;  // Used for the Player's control override
    private bool frozenPlayerVisibility = true; // Used for the Player's invincibility timer when hurt

    public delegate void Message();
    public static event Message SendToStaticInit;

    public void ActionDialogResult(TextMessage msg, string afterDialogState = "ENEMYDIALOGUE") {
        ActionDialogResult(new[] { msg }, afterDialogState);
    }

    public void ActionDialogResult(TextMessage[] msg, string afterDialogState = "ENEMYDIALOGUE") {
        stateAfterDialogs = afterDialogState;
        SwitchState("DIALOGRESULT");
        mainTextManager.SetTextQueue(msg);
    }

    public static void EndBattle(bool fromGameOver = false) {
        MusicManager.SetSoundDictionary("RESETDICTIONARY", "");
        LuaSpriteController spr = (LuaSpriteController)SpriteUtil.MakeIngameSprite("black", -1).UserData.Object;
        if (GameObject.Find("TopLayer"))
            spr.layer = "Top";
        spr.Scale(640, 480);
        if (GlobalControls.modDev) //Empty the inventory if not in the overworld
            Inventory.inventory.Clear();
        Inventory.RemoveAddedItems();
        if (GlobalControls.modDev)
            PlayerCharacter.instance.MaxHPShift = 0;
        PlayerCharacter.instance.ATK = 8 + 2 * PlayerCharacter.instance.LV;
        PlayerCharacter.instance.DEF = 10 + (int)Mathf.Floor((PlayerCharacter.instance.LV - 1) / 4f);
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Misc.WindowName = GlobalControls.crate ? ControlPanel.instance.WinodwBsaisNmae : ControlPanel.instance.WindowBasisName;
        #endif
        if (instance && instance.psContainer != null)
            instance.psContainer.SetActive(false);

        //Stop encounter storage for good!
        if (GlobalControls.modDev) {
            ScriptWrapper.instances.Clear();
        } else {
            foreach (EnemyController enemy in instance.encounter.enemies)
                ScriptWrapper.instances.Remove(enemy.script);
            Table t = EnemyEncounter.script["Wave"].Table;
            foreach (DynValue obj in t.Keys) {
                try {
                    ScriptWrapper.instances.Remove(((ScriptWrapper)t[obj]));
                } catch { /* ignored */ }
            }
            ScriptWrapper.instances.Remove(EnemyEncounter.script);
        }

        //Properly set "isInFight" to false, as it shouldn't be true anymore
        GlobalControls.isInFight = false;

        LuaScriptBinder.ClearBattleVar();
        GlobalControls.stopScreenShake = true;
        MusicManager.hiddenDictionary.Clear();
        if (GlobalControls.modDev) {
            List<string> toDelete = NewMusicManager.audioname.Keys.Where(str => str != "src").ToList();
            foreach (string str in toDelete)
                NewMusicManager.DestroyChannel(str);
            PlayerCharacter.instance.Reset();
            // Discord Rich Presence
            DiscordControls.StartModSelect();
            SceneManager.LoadScene("ModSelect");
        } else {
            foreach (string str in NewMusicManager.audioname.Keys.Where(str => str != "StaticKeptAudio"))
                NewMusicManager.Stop(str);
            SceneManager.UnloadSceneAsync("Battle");
            PlayerOverworld.ShowOverworld("Battle");
        }

        //Reset to 4:3
        ScreenResolution.ResetAfterBattle();
        Time.timeScale = 1;
    }

    public void SwitchState(string newState, bool first = false) {
        stateSwitched = true;
        if (onDeathSwitch) {
            lastNewState = newState;
            return;
        }
        //Pre-state
        if (fleeSwitch && newState != "DONE")
            return;
        if (parentStateCall) {
            parentStateCall = false;
            try {
                EnemyEncounter.script.Call("EnteringState", new[] { DynValue.NewString(newState), DynValue.NewString(state) });
            } catch (InterpreterException ex) {
                UnitaleUtil.DisplayLuaError(EnemyEncounter.script.scriptname, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message);
            }
            parentStateCall = true;

            if (childStateCalled) {
                childStateCalled = false;
                return;
            }
        } else {
            childStateCalled = true;
        }

        // Quick and dirty addition to add some humor to the Run away command.
        if (musicPausedFromRunning) {
            Camera.main.GetComponent<AudioSource>().UnPause();
            musicPausedFromRunning = false;
        }
        // END DEBUG
        // below: actions based on ending a previous state, or actions that affect multiple states

        // PAUSE can freeze states
        if (newState == "PAUSE" && frozenState != "PAUSE") return;
        if (newState == "PAUSE" && frozenState == "PAUSE") {
            frozenState = state;

            // execute extra code based on the state that is being frozen
            switch(frozenState) {
                case "ACTIONSELECT":
                case "DIALOGRESULT":
                    mainTextManager.SetPause(true);
                    break;
                case "DEFENDING":
                    frozenControlOverride = PlayerController.instance.overrideControl;
                    PlayerController.instance.setControlOverride(true);

                    frozenTimestamp = Time.time;
                    break;
                case "ENEMYDIALOGUE":
                    TextManager[] textManagers = FindObjectsOfType<TextManager>();
                    foreach (TextManager textManager in textManagers)
                        if (textManager.gameObject.name.StartsWith("DialogBubble")) // game object name is hardcoded as it won't change
                            textManager.SetPause(true);
                    break;
                case "ATTACKING":
                    FightUI fui = fightUI.boundFightUiInstances[0];
                    if (fui.slice != null && fui.slice.keyframes != null)
                        fui.slice.keyframes.paused = true;

                    if (fightUI.line != null && fightUI.line.keyframes != null)
                        fightUI.line.keyframes.paused = true;
                    break;
            }

            frozenPlayerVisibility                    = PlayerController.instance.selfImg.enabled;
            PlayerController.instance.selfImg.enabled = true;

            return;
        }
        if (newState == frozenState && frozenState != "PAUSE") {
            // execute extra code based on the state that is being un-frozen
            switch(frozenState) {
                case "ACTIONSELECT":
                case "DIALOGRESULT":
                    mainTextManager.SetPause(true);
                    break;
                case "DEFENDING":
                    PlayerController.instance.setControlOverride(frozenControlOverride);

                    frozenTimestamp     =  Time.time - frozenTimestamp;
                    encounter.waveTimer += frozenTimestamp;
                    break;
                case "ENEMYDIALOGUE":
                    TextManager[] textManagers = FindObjectsOfType<TextManager>();
                    foreach (TextManager textManager in textManagers)
                        if (textManager.gameObject.name.StartsWith("DialogBubble")) // game object name is hardcoded as it won't change
                            textManager.SetPause(false);
                    break;
                case "ATTACKING":
                    FightUI fui = fightUI.boundFightUiInstances[0];
                    if (fui.slice != null && fui.slice.keyframes != null)
                        fui.slice.keyframes.paused = false;

                    if (fightUI.line != null && fightUI.line.keyframes != null)
                        fightUI.line.keyframes.paused = false;
                    break;
            }

            PlayerController.instance.selfImg.enabled = frozenPlayerVisibility;

            frozenState = "PAUSE";

            return;
        }
        frozenState = "PAUSE";

        frozenTimestamp  = 0.0f;

        if (newState == "DEFENDING" || newState == "ENEMYDIALOGUE") {
            PlayerController.instance.setControlOverride(newState != "DEFENDING");
            mainTextManager.DestroyChars();
            PlayerController.instance.SetPosition(ArenaManager.instance.currentX, ArenaManager.instance.currentY + 70, true);
            PlayerController.instance.GetComponent<Image>().enabled = true;
            fightButton.overrideSprite = null;
            actButton.overrideSprite = null;
            itemButton.overrideSprite = null;
            mercyButton.overrideSprite = null;
            mainTextManager.SetPause(true);
        } else if ((state == "DEFENDING" || state == "ENEMYDIALOGUE") && newState != "DEFENDING" && newState != "ENEMYDIALOGUE") {
            ArenaManager.instance.ResetArena();
            PlayerController.instance.invulTimer = 0.0f;
            PlayerController.instance.setControlOverride(true);
        }

        if (state == "ENEMYSELECT" && forcedAction == Actions.FIGHT)
            foreach (LifeBarController lbc in arenaParent.GetComponentsInChildren<LifeBarController>())
                Destroy(lbc.gameObject);
        else if (state == "ENEMYDIALOGUE") {
            TextManager[] textManagers = FindObjectsOfType<TextManager>();
            foreach (TextManager textManager in textManagers)
                if (textManager.gameObject.name.StartsWith("DialogBubble")) // game object name is hardcoded as it won't change
                    Destroy(textManager.gameObject);
        } else if (state == "DIALOGRESULT")
            mainTextManager.SetCaller(EnemyEncounter.script);
        else if (state == "ATTACKING")
            FightUIController.instance.HideAttackingUI();

        string oldState = state;
        state = newState;
        //encounter.CallOnSelfOrChildren("Entered" + Enum.GetName(typeof(UIState), state).Substring(0, 1)
        //                                         + Enum.GetName(typeof(UIState), state).Substring(1, Enum.GetName(typeof(UIState), state).Length - 1).ToLower());
        if (oldState == "DEFENDING" && state != "DEFENDING") {
            string current = state;
            encounter.EndWave();
            if (state != current && !GlobalControls.retroMode)
                return;
        }

        switch (state) {
            case "ATTACKING":
                // Error for no active enemies
                if (encounter.EnabledEnemies.Length == 0)
                    throw new CYFException("Cannot enter state ATTACKING with no active enemies.");

                // Disable all current attack instances otherwise they break
                // TODO: Find the exact reason why they break
                foreach (EnemyController enemy in encounter.EnabledEnemies)
                    FightUIController.instance.DestroyAllAttackInstances(enemy);

                mainTextManager.DestroyChars();
                PlayerController.instance.GetComponent<Image>().enabled = false;
                if (!fightUI.multiHit) {
                    fightUI.targetIDs = new[] { selectedEnemy };
                    fightUI.targetNumber = 1;
                }

                fightUI.Init();
                break;

            case "ACTIONSELECT":
                forcedAction = Actions.NONE;
                PlayerController.instance.setControlOverride(true);
                PlayerController.instance.GetComponent<Image>().enabled = true;
                SetPlayerOnAction(action);
                mainTextManager.SetPause(ArenaManager.instance.isResizeInProgress());
                if (!GlobalControls.retroMode) {
                    mainTextManager.SetEffect(new TwitchEffect(mainTextManager));
                    encounter.EncounterText = EnemyEncounter.script.GetVar ("encountertext").String;
                }
                if (encounter.EncounterText == null) {
                    encounter.EncounterText = "";
                    UnitaleUtil.Warn("There is no encounter text!");
                }
                mainTextManager.SetText(new RegularMessage(encounter.EncounterText));
                break;

            case "ACTMENU":
                string[] actions = new string[encounter.EnabledEnemies[selectedEnemy].ActCommands.Length];
                if (actions.Length == 0)
                    throw new CYFException("Cannot enter state ACTMENU without commands.");
                for (int i = 0; i < actions.Length; i++)
                    actions[i] = encounter.EnabledEnemies[selectedEnemy].ActCommands[i];

                selectedAction = 0;
                SetPlayerOnSelection(selectedAction);
                if (!GlobalControls.retroMode)
                    mainTextManager.SetEffect(new TwitchEffect(mainTextManager));
                mainTextManager.SetText(new SelectMessage(actions, false));
                break;

            case "ITEMMENU":
                battleDialogueStarted = false;
                // Error for empty inventory
                if (Inventory.inventory.Count == 0)
                    throw new CYFException("Cannot enter state ITEMMENU with empty inventory.");
                else {
                    string[] items = GetInventoryPage(0);
                    selectedItem = 0;
                    if (!GlobalControls.retroMode)
                        mainTextManager.SetEffect(new TwitchEffect(mainTextManager));
                    mainTextManager.SetText(new SelectMessage(items, false));
                    SetPlayerOnSelection(0);
                    /*ActionDialogResult(new TextMessage[] {
                        new TextMessage("Can't open inventory.\nClogged with pasta residue.", true, false),
                        new TextMessage("Might also be a dog.\nIt's ambiguous.",true,false)
                    }, UIState.ENEMYDIALOGUE);*/
                }
                break;

            case "MERCYMENU":
                if (LuaScriptBinder.Get(null, "ForceNoFlee") != null) {
                    EnemyEncounter.script.SetVar("flee", DynValue.NewBoolean(false));
                    LuaScriptBinder.Remove("ForceNoFlee");
                }
                if (!EnemyEncounter.script.GetVar("flee").Boolean && EnemyEncounter.script.GetVar("flee").Type != DataType.Nil)
                    encounter.CanRun = false;
                else
                    encounter.CanRun = true;
                selectedMercy = 0;
                string[] mercyOptions = new string[1 + (encounter.CanRun ? 1 : 0)];
                mercyOptions[0] = "Spare";
                if (encounter.EnabledEnemies.Any(enemy => enemy.CanSpare))
                    mercyOptions[0] = "[starcolor:ffff00][color:ffff00]" + mercyOptions[0] + "[color:ffffff]";
                if (encounter.CanRun)
                    mercyOptions[1] = "Flee";
                SetPlayerOnSelection(0);
                if (!GlobalControls.retroMode)
                    mainTextManager.SetEffect(new TwitchEffect(mainTextManager));
                mainTextManager.SetText(new SelectMessage(mercyOptions, true));
                break;

            case "ENEMYSELECT":
                // Error for no active enemies
                if (encounter.EnabledEnemies.Length == 0)
                    throw new CYFException("Cannot enter state ENEMYSELECT with no active enemies.");

                string[] names = new string[encounter.EnabledEnemies.Length];
                string[] colorPrefixes = new string[names.Length];
                for (int i = 0; i < encounter.EnabledEnemies.Length; i++) {
                    names[i] = encounter.EnabledEnemies[i].Name;
                    if (encounter.EnabledEnemies[i].CanSpare)
                        colorPrefixes[i] = "[color:ffff00]";
                }
                if (encounter.EnabledEnemies.Length > 3) {
                    selectedEnemy = 0;
                    string[] newNames = new string[3];
                    newNames[0] = names[0];
                    newNames[1] = names[1];
                    colorPrefixes[2] = "";
                    newNames[2] = "\tPAGE 1";
                    names = newNames;
                }
                for (int i = 0; i < names.Length; i++)
                    names[i] += "[color:ffffff]";
                if (!GlobalControls.retroMode)
                    mainTextManager.SetEffect(new TwitchEffect(mainTextManager));
                mainTextManager.SetText(new SelectMessage(names, true, colorPrefixes));
                if (forcedAction != Actions.FIGHT && forcedAction != Actions.ACT)
                    forcedAction = action;
                if (forcedAction == Actions.FIGHT) {
                    int maxWidth = (int)initialHealthPos.x, count = 0;

                    for (int i = 0; i < encounter.EnabledEnemies.Length; i++) {
                        if (encounter.EnabledEnemies.Length > 3)
                            if (i > 1)
                                break;
                        //int mNameWidth = UnitaleUtil.fontStringWidth(mainTextManager.Charset, "* " + encounter.enabledEnemies[i].Name) + 50;
                        for (int j = count; j < mainTextManager.textQueue[mainTextManager.currentLine].Text.Length; j++)
                            if (mainTextManager.textQueue[mainTextManager.currentLine].Text[j] == '\n' || mainTextManager.textQueue[mainTextManager.currentLine].Text[j] == '\r')
                                break;
                        count++;
                        //int mNameWidth = (int)UnitaleUtil.calcTotalLength(mainTextManager, lastCount, count);
                        for (int j = 0; j <= 1 && j < encounter.EnabledEnemies.Length; j++) {
                            int mNameWidth = (int)UnitaleUtil.CalcTextWidth(mainTextManager) + 50;
                            if (mNameWidth > maxWidth)
                                maxWidth = mNameWidth;
                        }
                    }
                    for (int i = 0; i < encounter.EnabledEnemies.Length; i++) {
                        if (encounter.EnabledEnemies.Length > 3)
                            if (i > 1)
                                break;
                        LifeBarController lifeBar = LifeBarController.Create(0, 0, 90);
                        lifeBar.transform.SetParent(mainTextManager.transform);
                        lifeBar.transform.SetAsFirstSibling();
                        lifeBar.background.MoveTo(maxWidth, initialHealthPos.y - i * mainTextManager.Charset.LineSpacing);
                        lifeBar.SetFillColor(Color.green);
                        float hpDivide = encounter.EnabledEnemies[i].HP / (float)encounter.EnabledEnemies[i].MaxHP;
                        lifeBar.SetInstant(hpDivide, encounter.EnabledEnemies[i].HP < 0);
                    }
                }

                if (selectedEnemy >= encounter.EnabledEnemies.Length)
                    selectedEnemy = 0;
                SetPlayerOnSelection(selectedEnemy * 2); // single list so skip right row by multiplying x2
                break;

            case "DEFENDING":
                ArenaManager.instance.Resize((int)encounter.ArenaSize.x, (int)encounter.ArenaSize.y);
                PlayerController.instance.setControlOverride(false);
                encounter.NextWave();
                // ActionDialogResult(new TextMessage("This is where you'd\rdefend yourself.\nBut the code was spaghetti.", true, false), UIState.ACTIONSELECT);
                break;

            case "DIALOGRESULT":
                PlayerController.instance.GetComponent<Image>().enabled = false;
                break;

            case "ENEMYDIALOGUE":
                PlayerController.instance.GetComponent<Image>().enabled = true;
                if (!GlobalControls.retroMode)
                    ArenaManager.instance.Resize((int)encounter.ArenaSize.x, (int)encounter.ArenaSize.y);
                else
                    ArenaManager.instance.Resize(155, 130);
                encounter.CallOnSelfOrChildren("EnemyDialogueStarting");
                if (state != "ENEMYDIALOGUE")
                    return;
                monsterDialogues = new TextManager[encounter.EnabledEnemies.Length];
                monsterDialogueEnemyID = new int[encounter.EnabledEnemies.Length];
                readyToNextLine = new bool[encounter.enemies.Count];
                // TODO: Try to merge this and the other bubble-related code block
                for (int i = 0; i < encounter.EnabledEnemies.Length; i++) {
                    messages.Remove(i);
                    messages.Add(i, encounter.EnabledEnemies[i].GetDefenseDialog());
                    string[] message = messages[i];
                    if (message == null) {
                        UnitaleUtil.Warn("Entered ENEMYDIALOGUE, but no current/random dialogue was set for " + encounter.EnabledEnemies[i].Name);
                        SwitchState("DEFENDING");
                        break;
                    }
                    GameObject speechBub = Instantiate(SpriteFontRegistry.BUBBLE_OBJECT);
                    //RectTransform enemyRt = encounter.enabledEnemies[i].GetComponent<RectTransform>();
                    TextManager sbTextMan = speechBub.GetComponent<TextManager>();
                    monsterDialogues[i] = sbTextMan;
                    monsterDialogueEnemyID[i] = encounter.enemies.IndexOf(encounter.EnabledEnemies[i]);
                    sbTextMan.SetCaller(encounter.EnabledEnemies[i].script);
                    Image speechBubImg = speechBub.GetComponent<Image>();

                    // error catcher
                    try { SpriteUtil.SwapSpriteFromFile(speechBubImg, encounter.EnabledEnemies[i].DialogBubble, i); }
                    catch {
                        if (encounter.EnabledEnemies[i].DialogBubble != "UI/SpeechBubbles/")
                            UnitaleUtil.DisplayLuaError(encounter.EnabledEnemies[i].scriptName + ": Creating a dialogue bubble",
                                                        "The dialogue bubble \"" + encounter.EnabledEnemies[i].script.GetVar("dialogbubble") + "\" doesn't exist.");
                        else
                            UnitaleUtil.DisplayLuaError(encounter.EnabledEnemies[i].scriptName + ": Creating a dialogue bubble",
                                                        "This monster has no set dialogue bubble.");
                        return;
                    }

                    sbTextMan._textMaxWidth = (int)encounter.EnabledEnemies[i].bubbleWidth;
                    Sprite speechBubSpr = speechBubImg.sprite;
                    // TODO improve position setting/remove hardcoding of position setting
                    bool reversedX = encounter.EnabledEnemies[i].sprite.xscale < 0,
                         reversedY = encounter.EnabledEnemies[i].sprite.yscale < 0;

                    speechBub.transform.SetParent(encounter.EnabledEnemies[i].transform);
                    speechBub.GetComponent<RectTransform>().anchorMin = speechBub.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                    speechBub.GetComponent<RectTransform>().anchoredPosition = new Vector3((encounter.EnabledEnemies[i].DialogBubblePosition.x + encounter.EnabledEnemies[i].offsets[1].x) * (reversedX ? -1 : 1),
                                                                                           (encounter.EnabledEnemies[i].DialogBubblePosition.y + encounter.EnabledEnemies[i].offsets[1].y) * (reversedY ? -1 : 1));
                    sbTextMan.SetOffset(speechBubSpr.border.x, -speechBubSpr.border.w);

                    UnderFont enemyFont = SpriteFontRegistry.Get(encounter.EnabledEnemies[i].Font ?? string.Empty) ?? SpriteFontRegistry.Get(SpriteFontRegistry.UI_MONSTERTEXT_NAME);
                    sbTextMan.SetFont(enemyFont);

                    TextMessage[] monsterMessages = new TextMessage[message.Length];
                    for (int j = 0; j < monsterMessages.Length; j++)
                        monsterMessages[j] = new MonsterMessage(encounter.EnabledEnemies[i].DialoguePrefix + message[j]);

                    sbTextMan.SetTextQueue(monsterMessages);
                    speechBubImg.color = new Color(speechBubImg.color.r, speechBubImg.color.g, speechBubImg.color.b, sbTextMan.letterReferences.Count(ltr => ltr != null) == 0 ? 0 : 1);

                    speechBub.GetComponent<Image>().enabled = true;
                    if (encounter.EnabledEnemies[i].Voice != "")
                        sbTextMan.letterSound = encounter.EnabledEnemies[i].Voice;
                    encounter.EnabledEnemies[i].bubbleObject = speechBub;
                }
                break;

            case "DONE":
                //StaticInits.Reset();
                //LuaEnemyEncounter.script.SetVar("unescape", DynValue.NewBoolean(false));
                EndBattle();
                break;
        }
    }

    public static void SwitchStateOnString(Script scr, string state) {
        if (state == null)
            throw new CYFException("State: Argument cannot be nil.");
        state = state.ToUpper();
        if (instance.encounter.gameOverStance) return;
        if (!instance.UIStates.Contains(state))
            throw new CYFException("The state \"" + state + "\" is not a valid state. Are you sure it exists?\n\nPlease double-check in the Misc. Functions section of the docs for a list of every default valid state.");

        try {
            instance.SwitchState(state);
        } catch (Exception ex) {
            // a different error has occurred
            throw new CYFException("An error occurred while trying to enter the state \"" + state + "\":\n\n" + ex.Message + "\n\nTraceback (for devs):\n" + ex);
        }
    }

    public static void CreateNewUIState(string name) {
        if (instance.UIStates.Contains(name))
            throw new CYFException("The state \"" + name + "\" already exists.");

        instance.UIStates.Add(name);
    }

    private void Awake() {
        if (GlobalControls.crate) {
            fightButtonSprite = SpriteRegistry.Get("UI/Buttons/gifhtbt_1");
            actButtonSprite = SpriteRegistry.Get("UI/Buttons/catbt_1");
            itemButtonSprite = SpriteRegistry.Get("UI/Buttons/tembt_1");
            mercyButtonSprite = SpriteRegistry.Get("UI/Buttons/mecrybt_1");
        } else {
            fightButtonSprite = SpriteRegistry.Get("UI/Buttons/fightbt_1");
            actButtonSprite = SpriteRegistry.Get("UI/Buttons/actbt_1");
            itemButtonSprite = SpriteRegistry.Get("UI/Buttons/itembt_1");
            mercyButtonSprite = SpriteRegistry.Get("UI/Buttons/mercybt_1");
        }

        arenaParent = GameObject.Find("arena_border_outer");
        //canvasParent = GameObject.Find("Canvas");
        uiAudio = GetComponent<AudioSource>();
        uiAudio.clip = AudioClipRegistry.GetSound("menumove");

        instance = this;
    }

    public void UpdateBubble() {
        for (int i = 0; i < encounter.enemies.Count; i++) {
            try {
                EnemyController enemy = encounter.enemies[i];
                if (!enemy.bubbleObject)
                    continue;
                int monsterDialogueID = Array.IndexOf(monsterDialogueEnemyID, i);
                if (monsterDialogues[monsterDialogueID] == null) {
                    readyToNextLine[i] = true;
                    continue;
                }

                if (monsterDialogues[monsterDialogueID].currentLine >= monsterDialogues[monsterDialogueID].LineCount()) {
                    readyToNextLine[i] = true;
                    continue;
                }

                GameObject speechBub = enemy.bubbleObject;
                TextManager sbTextMan = speechBub.GetComponent<TextManager>();
                sbTextMan._textMaxWidth = (int)enemy.bubbleWidth;
                readyToNextLine[i] = false;
                Image speechBubImg = speechBub.GetComponent<Image>();
                speechBubImg.color = new Color(speechBubImg.color.r, speechBubImg.color.g, speechBubImg.color.b, sbTextMan.letterReferences.Count(ltr => ltr != null) == 0 ? 0 : 1);

                SpriteUtil.SwapSpriteFromFile(speechBubImg, enemy.DialogBubble, monsterDialogueID);
                Sprite speechBubSpr = speechBubImg.sprite;
                sbTextMan.SetOffset(speechBubSpr.border.x, -speechBubSpr.border.w);

                bool reversedX = enemy.sprite.xscale < 0,
                     reversedY = enemy.sprite.yscale < 0;
                speechBub.GetComponent<RectTransform>().anchorMin = speechBub.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                speechBub.GetComponent<RectTransform>().anchoredPosition = new Vector3((enemy.DialogBubblePosition.x + enemy.offsets[1].x) * (reversedX ? -1 : 1),
                                                                                       (enemy.DialogBubblePosition.y + enemy.offsets[1].y) * (reversedY ? -1 : 1));
                if (enemy.Voice != "")
                    sbTextMan.letterSound = enemy.Voice;
            } catch (Exception e) {
                throw new CYFException("Error while updating monster #" + i + ": \n" + e.Message + "\n\n" + e.StackTrace);
            }
        }
    }

    private void UpdateMonsterDialogue() {
        bool allGood = true;
        for (int i = 0; i < monsterDialogues.Length; i++) {
            if (readyToNextLine[monsterDialogueEnemyID[i]]) continue;
            if (monsterDialogues[i] == null) {
                readyToNextLine[monsterDialogueEnemyID[i]] = true;
                continue;
            }
            if (monsterDialogues[i].CanAutoSkip()) {
                readyToNextLine[monsterDialogueEnemyID[i]] = true;
                DoNextMonsterDialogue(false, i);
            }
            if (monsterDialogues[i].CanAutoSkipAll()) {
                for (int j = 0; j < monsterDialogues.Length; j++)
                    readyToNextLine[monsterDialogueEnemyID[j]] = true;
                DoNextMonsterDialogue();
                return;
            }

            if ((!monsterDialogues[i].AllLinesComplete() || monsterDialogues[i].LineCount() == 0) && !monsterDialogues[i].CanAutoSkipThis() && (monsterDialogues[i].AllLinesComplete() || !monsterDialogues[i].LineComplete())) {
                allGood = false;
                continue;
            }


            readyToNextLine[monsterDialogueEnemyID[i]] = true;
        }

        if (!allGood) return;
        for (int i = 0; i < readyToNextLine.Length; i++)
            readyToNextLine[i] = true;
    }

    public void DoNextMonsterDialogue(bool singleLineAll = false, int index = -1) {
        bool complete = true, foiled = false;
        if (index != -1) {
            if (monsterDialogues[index] == null)
                return;

            if (monsterDialogues[index].HasNext()) {
                FileInfo fi = new FileInfo(FileLoader.PathToDefaultFile("Sprites/" + encounter.EnabledEnemies[index].DialogBubble + ".png"));
                if (!fi.Exists)
                    fi = new FileInfo(FileLoader.PathToModFile("Sprites/" + encounter.EnabledEnemies[index].DialogBubble + ".png"));
                if (!fi.Exists)
                    UnitaleUtil.Warn("The bubble " + encounter.EnabledEnemies[index].DialogBubble + ".png doesn't exist.");
                else {
                    Sprite speechBubSpr = SpriteUtil.FromFile(encounter.EnabledEnemies[index].DialogBubble + ".png");
                    monsterDialogues[index].SetOffset(speechBubSpr.border.x, -speechBubSpr.border.w);
                }
                monsterDialogues[index].NextLineText();
                complete = false;
            } else {
                monsterDialogues[index].DestroyChars();
                Destroy(monsterDialogues[index].gameObject);
                foreach (TextManager textManager in monsterDialogues)
                    if (textManager != null)
                        complete = false;
            }
        } else if (!singleLineAll)
            for (int i = 0; i < monsterDialogues.Length; i++) {
                if (monsterDialogues[i] == null)
                    continue;

                if (monsterDialogues[i].AllLinesComplete() && monsterDialogues[i].LineCount() != 0 || (!monsterDialogues[i].HasNext() && readyToNextLine[i])) {
                    monsterDialogues[i].DestroyChars();
                    Destroy(monsterDialogues[i].gameObject); // this text manager's game object is a dialog bubble and should be destroyed at this point
                    continue;
                }
                complete = false;

                // part that autoskips text if [nextthisnow] or [finished] is introduced
                if (monsterDialogues[i].CanAutoSkipThis() || monsterDialogues[i].CanAutoSkip()) {
                    if (monsterDialogues[i].HasNext()) {
                        FileInfo fi = new FileInfo(FileLoader.PathToDefaultFile("Sprites/" + encounter.EnabledEnemies[i].DialogBubble + ".png"));
                        if (!fi.Exists)
                            fi = new FileInfo(FileLoader.PathToModFile("Sprites/" + encounter.EnabledEnemies[i].DialogBubble + ".png"));
                        if (!fi.Exists) {
                            Debug.LogError("The bubble " + encounter.EnabledEnemies[i].DialogBubble + ".png doesn't exist.");
                        } else {
                            Sprite speechBubSpr = SpriteUtil.FromFile(encounter.EnabledEnemies[i].DialogBubble + ".png");
                            monsterDialogues[i].SetOffset(speechBubSpr.border.x, -speechBubSpr.border.w);
                        }
                        monsterDialogues[i].NextLineText();
                    } else {
                        monsterDialogues[i].DestroyChars();
                        Destroy(monsterDialogues[i].gameObject); // code duplication? in my source? it's more likely than you think
                        if (!foiled)
                            complete = true;
                        continue;
                    }
                } else if (readyToNextLine[monsterDialogueEnemyID[i]]) {
                    FileInfo fi = new FileInfo(FileLoader.PathToDefaultFile("Sprites/" + encounter.EnabledEnemies[i].DialogBubble + ".png"));
                    if (!fi.Exists)
                        fi = new FileInfo(FileLoader.PathToModFile("Sprites/" + encounter.EnabledEnemies[i].DialogBubble + ".png"));
                    if (!fi.Exists) {
                        Debug.LogError("The bubble " + encounter.EnabledEnemies[i].DialogBubble + ".png doesn't exist.");
                    } else {
                        Sprite speechBubSpr = SpriteUtil.FromFile(encounter.EnabledEnemies[i].DialogBubble + ".png");
                        monsterDialogues[i].SetOffset(speechBubSpr.border.x, -speechBubSpr.border.w);
                    }
                    monsterDialogues[i].NextLineText();
                }
                foiled = true;
            }
        if (!complete)
            UpdateBubble();
        // looping through the same list three times? there's a reason this class is the most in need of redoing
        // either way, after doing everything required, check which text manager has the longest text now and mute all others
        int longestTextLen = 0;
        int longestTextMgrIndex = -1;
        for (int i = 0; i < monsterDialogues.Length; i++) {
            if (monsterDialogues[i] == null) continue;
            monsterDialogues[i].SetMute(true);
            if (monsterDialogues[i].AllLinesComplete() || monsterDialogues[i].letterReferences.Length <= longestTextLen) continue;
            longestTextLen      = monsterDialogues[i].letterReferences.Length - monsterDialogues[i].currentReferenceCharacter;
            longestTextMgrIndex = i;
        }

        if (longestTextMgrIndex > -1)
            monsterDialogues[longestTextMgrIndex].SetMute(false);

        if (!complete) return; // break if we're not done with all text
        if (encounter.EnabledEnemies.Length <= 0) return;
        encounter.CallOnSelfOrChildren("EnemyDialogueEnding");
        if (state == "ENEMYDIALOGUE")
            SwitchState("DEFENDING");
    }

    private static string[] GetInventoryPage(int page) {
        int invCount = 0;
        for (int i = page * 4; i < page * 4 + 4; i++) {
            if (Inventory.inventory.Count <= i) break;
            invCount++;
        }

        if (invCount == 0) return null;

        string[] items = new string[6];
        for (int i = 0; i < invCount; i++)
            items[i] = Inventory.inventory[i + page * 4].ShortName;
        items[5] = "PAGE " + (page + 1);
        return items;
    }

    private string[] GetEnemyPage(int page, out string[] colors) {
        int enemyCount = 0;
        for (int i = page * 2; i < page * 2 + 2; i++) {
            if (encounter.EnabledEnemies.Length <= i)
                break;
            enemyCount++;
        }
        colors = new string[6];
        string[] enemies = new string[6];
        enemies[0] = encounter.EnabledEnemies[page * 2].Name;
        if (enemyCount == 2)
            enemies[2] = "* " + encounter.EnabledEnemies[page * 2 + 1].Name;
        for (int i = page * 2; i < encounter.EnabledEnemies.Length && enemyCount > 0; i++) {
            if (encounter.EnabledEnemies[i].CanSpare)
                colors[(i - page * 2) * 2] = "[color:ffff00]";
            enemyCount--;
        }
        enemies[5] = "PAGE " + (page + 1);
        return enemies;
    }

    private void RenewLifeBars(int page) {
        int maxWidth = (int)initialHealthPos.x;
        foreach (LifeBarController lbc in arenaParent.GetComponentsInChildren<LifeBarController>())
            Destroy(lbc.gameObject);
        for (int i = page * 2; i <= page * 2 + 1 && i < encounter.EnabledEnemies.Length; i++) {
            int mNameWidth = (int)UnitaleUtil.CalcTextWidth(mainTextManager) + 50;
            if (mNameWidth > maxWidth)
                maxWidth = mNameWidth;
        }
        for (int i = page * 2; i <= page * 2 + 1 && i < encounter.EnabledEnemies.Length; i++) {
            LifeBarController lifeBar = LifeBarController.Create(0, 0, 90);
            lifeBar.transform.SetParent(mainTextManager.transform);
            lifeBar.transform.SetAsFirstSibling();
            lifeBar.background.MoveTo(maxWidth, initialHealthPos.y - (i - page * 2) * mainTextManager.Charset.LineSpacing);
            lifeBar.SetFillColor(Color.green);
            float hpDivide = encounter.EnabledEnemies[i].HP / (float)encounter.EnabledEnemies[i].MaxHP;
            lifeBar.SetInstant(hpDivide, encounter.EnabledEnemies[i].HP < 0);
        }
    }

    public string GetState() { return state; }

    private void HandleAction() {
        if (!stateSwitched || state == "ATTACKING")
            switch (state) {
                case "ATTACKING":
                    fightUI.StopAction();
                    break;

                case "DIALOGRESULT":
                    if (!mainTextManager.LineComplete())
                        break;

                    if (!mainTextManager.AllLinesComplete() && mainTextManager.LineComplete())
                        mainTextManager.NextLineText();
                    else if (mainTextManager.AllLinesComplete() && mainTextManager.LineCount() != 0) {
                        mainTextManager.DestroyChars();
                        SwitchState(stateAfterDialogs);
                    }
                    break;

                case "ACTIONSELECT":
                    switch (action) {
                        case Actions.FIGHT:
                            if (encounter.EnabledEnemies.Length > 0)
                                SwitchState("ENEMYSELECT");
                            else
                                mainTextManager.DoSkipFromPlayer();
                            break;

                        case Actions.ACT:
                            if (GlobalControls.crate)
                                if (ControlPanel.instance.Safe) UnitaleUtil.PlaySound("MEOW", "sounds/meow" + Math.RandomRange(1, 8));
                                else UnitaleUtil.PlaySound("MEOW", "sounds/meow" + Math.RandomRange(1, 9));
                            else if (encounter.EnabledEnemies.Length > 0)
                                SwitchState("ENEMYSELECT");
                            else
                                mainTextManager.DoSkipFromPlayer();
                            break;

                        case Actions.ITEM:
                            if (GlobalControls.crate) {
                                const string strBasis = "TEM WANT FLAKES!!!1!1";
                                string strModified = strBasis;
                                for (int i = strBasis.Length - 2; i >= 0; i--)
                                    strModified = strModified.Substring(0, i) + "[voice:tem" + Math.RandomRange(1, 7) + "]" + strModified.Substring(i, strModified.Length - i);
                                ActionDialogResult(new TextMessage(strModified, true, false));

                            } else {
                                if (Inventory.inventory.Count == 0) {
                                    //ActionDialogResult(new TextMessage("Your Inventory is empty.", true, false), UIState.ACTIONSELECT);
                                    PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
                                    mainTextManager.DoSkipFromPlayer();
                                    return;
                                }
                                SwitchState("ITEMMENU");
                            }
                            break;

                        case Actions.MERCY:
                            if (GlobalControls.crate) {
                                string[] texts = {
                                    "You know...\rSeeing the engine like this...\rIt makes me want to cry.",
                                    "All these typos...\rCrate Your Frisk is bad.\nWe must destroy it.",
                                    "We have two solutions here:\nDestroy the engine's data...",
                                    "...Or another way. Though, I'll\rneed some time to find out\rhow to do this...",
                                    "*sniffles* I can barely stand\rthe view... This is so\rdisgusting...",
                                    "I feel like I'm getting there,\rkeep up the good work!",
                                    "Here, just a bit more...",
                                    "...No, I don't have it.\nStupid dog!\nPlease give me more time!",
                                    "I want to puke...\nEven the engine is a\rplace of shitposts and memes.",
                                    "Will there one day be a place\rwhere shitposts and memes\rwill not appear?",
                                    "I hope so...\rMy eyes are bleeding.",
                                    "Hm? Oh! Look! I have it!",
                                    "Let me read:",
                                    "\"To remove the big engine\rtypo bug...\""
                                };

                                if (meCry < 14)
                                    ActionDialogResult(new TextMessage(texts[meCry], true, false));
                                else if (meCry == 14)
                                    ActionDialogResult(new TextMessage[] {
                                        new RegularMessage("\"...click the BAD SPELING button\rin CYF's options menu.\""),
                                        new RegularMessage("Is that all? Come on, all\rthis time lost for such\ran easy response..."),
                                        new RegularMessage("...Sorry for the wait.\nDo whatever you want now! :D"),
                                        new RegularMessage("But please..."),
                                        new RegularMessage("For the love of all that\ris good..."),
                                        new RegularMessage("Remove Crate Your Frisk."),
                                        new RegularMessage("Now I'll wash my eyes with\rsome bleach."),
                                        new RegularMessage("Cya!")
                                    });
                                else
                                    ActionDialogResult(new TextMessage("But the dev is long gone\r(and blind).", true, false));
                                meCry++;
                            } else
                                SwitchState("MERCYMENU");
                            break;
                    }
                    PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
                    break;

                case "ENEMYSELECT":
                    switch (forcedAction) {
                        case Actions.FIGHT:
                            // encounter.enemies[selectedEnemy].HandleAttack(-1);
                            PlayerController.instance.lastEnemyChosen = selectedEnemy + 1;
                            SwitchState("ATTACKING");
                            break;

                        case Actions.ACT:
                            if (encounter.EnabledEnemies[selectedEnemy].ActCommands.Length != 0)
                                SwitchState("ACTMENU");
                            break;
                    }
                    PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
                    break;

                case "ACTMENU":
                    PlayerController.instance.lastEnemyChosen = selectedEnemy + 1;
                    encounter.EnabledEnemies[selectedEnemy].Handle(encounter.EnabledEnemies[selectedEnemy].ActCommands[selectedAction]);
                    PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
                    break;

                case "ITEMMENU":
                    //encounter.HandleItem(Inventory.container[selectedItem]);
                    encounter.HandleItem(selectedItem);
                    PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
                    break;

                case "MERCYMENU":
                    switch (selectedMercy) {
                        case 0: {
                            bool[] canSpare = new bool[encounter.enemies.Count];
                            int    count    = encounter.enemies.Count;
                            for (int i = 0; i < count; i++)
                                canSpare[i] = encounter.enemies[i].CanSpare;
                            EnemyController[] enabledEnTemp = encounter.EnabledEnemies;
                            bool playSound = true;
                            for (int i = 0; i < count; i++) {
                                if (!enabledEnTemp.Contains(encounter.enemies[i])) continue;
                                if (!canSpare[i]) continue;
                                if (UnitaleUtil.TryCall(encounter.enemies[i].script, "OnSpare")) continue;
                                encounter.enemies[i].DoSpare(playSound);
                                playSound = false;
                            }
                            if (encounter.EnabledEnemies.Length > 0)
                                encounter.CallOnSelfOrChildren("HandleSpare");
                            break;
                        }
                        case 1: {
                            if (!GlobalControls.retroMode) {
                                bool fleeSuccess = EnemyEncounter.script.GetVar("fleesuccess").Boolean || EnemyEncounter.script.GetVar("fleesuccess").Type != DataType.Boolean && Math.RandomRange(0, 9) + encounter.turnCount > 4;

                                if (encounter.CallOnSelfOrChildren("HandleFlee", new[] { DynValue.NewBoolean(fleeSuccess) }))
                                    break;

                                if (fleeSuccess) StartCoroutine(ISuperFlee());
                                else             SwitchState("ENEMYDIALOGUE");
                            } else {
                                PlayerController.instance.GetComponent<Image>().enabled = false;
                                AudioClip yay = AudioClipRegistry.GetSound("runaway");
                                AudioSource.PlayClipAtPoint(yay, Camera.main.transform.position);
                                string fittingLine;
                                switch (runAwayAttempts) {
                                    case 0:  fittingLine = "...[w:15]But you realized\rthe overworld was missing.";                               break;
                                    case 1:  fittingLine = "...[w:15]But the overworld was\rstill missing.";                                      break;
                                    case 2:  fittingLine = "You walked off as if there\rwere an overworld, but you\rran into an invisible wall."; break;
                                    case 3:  fittingLine = "...[w:15]On second thought, the\rembarrassment just now\rwas too much.";              break;
                                    case 4:  fittingLine = "But you became aware\rof the skeleton inside your\rbody, and forgot to run.";         break;
                                    case 5:  fittingLine = "But you needed a moment\rto forget about your\rscary skeleton.";                      break;
                                    case 6:  fittingLine = "...[w:15]You feel as if you\rtried this before.";                                     break;
                                    case 7:  fittingLine = "...[w:15]Maybe if you keep\rsaying that, the\roverworld will appear.";                break;
                                    case 8:  fittingLine = "...[w:15]Or not.";                                                                    break;
                                    default: fittingLine = "...[w:15]But you decided to\rstay anyway.";                                           break;
                                }

                                ActionDialogResult(new TextMessage[] { new RegularMessage("I'm outta here."), new RegularMessage(fittingLine) });
                                Camera.main.GetComponent<AudioSource>().Pause();
                                musicPausedFromRunning = true;
                                runAwayAttempts++;
                            }

                            break;
                        }
                    }
                    PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
                    break;

                case "ENEMYDIALOGUE":
                    bool singleLineAll = monsterDialogues.Where(mgr => mgr != null).All(mgr => mgr.LineCount() <= 1 && mgr.CanSkip());
                    if (singleLineAll) {
                        foreach (TextManager mgr in monsterDialogues)
                            mgr.DoSkipFromPlayer();
                        mainTextManager.nextMonsterDialogueOnce = true;
                    } else if (!ArenaManager.instance.isResizeInProgress()) {
                        bool readyToSkip = readyToNextLine.All(b => b);
                        if (readyToSkip)
                            DoNextMonsterDialogue();
                    }
                    break;
            }
        else
            PlaySound(AudioClipRegistry.GetSound("menuconfirm"));
    }

    public static void DisableButton(string btn) {
        Actions act;
        try {
            act = (Actions)Enum.Parse(typeof(Actions), btn);
            if (act == Actions.NONE)
                throw new CYFException("DisableButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
        catch { throw new CYFException("DisableButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\"."); }

        instance.disabledActions[(int)act] = true;
    }

    public static void EnableButton(string btn) {
        Actions act;
        try {
            act = (Actions)Enum.Parse(typeof(Actions), btn);
            if (act == Actions.NONE)
                throw new CYFException("DisableButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\".");
        }
        catch { throw new CYFException("DisableButton() can only take \"FIGHT\", \"ACT\", \"ITEM\" or \"MERCY\", but you entered \"" + btn + "\"."); }

        instance.disabledActions[(int)act] = false;
    }

    public Actions FindAvailableAction(int change) {
        // All buttons are disabled: nothing is done
        if (disabledActions.Count(x => !x) == 0)
            return action;

        int actionIndex = Math.Mod((int)action + change, 4);
        if (!disabledActions[actionIndex]) return (Actions) actionIndex;

        int nextChange = change >= 0 ? 1 : -1;
        do { actionIndex = Math.Mod(actionIndex + nextChange, 4); }
        while (disabledActions[actionIndex]);

        return (Actions)actionIndex;
    }

    private void HandleArrows() {
        bool left = InputUtil.Pressed(GlobalControls.input.Left);
        bool right = InputUtil.Pressed(GlobalControls.input.Right);
        bool up = InputUtil.Pressed(GlobalControls.input.Up);
        bool down = InputUtil.Pressed(GlobalControls.input.Down);

        switch (state) {
            case "ACTIONSELECT":
                if (!left &&!right)
                    break;

                int oldActionIndex = (int)action;
                action = FindAvailableAction(left ? -1 : 1);
                int actionIndex = (int)action;

                if (oldActionIndex != actionIndex) {
                    fightButton.overrideSprite = null;
                    actButton.overrideSprite = null;
                    itemButton.overrideSprite = null;
                    mercyButton.overrideSprite = null;

                    SetPlayerOnAction(action);
                }

                PlaySound(AudioClipRegistry.GetSound("menumove"));
                break;

            case "ENEMYSELECT":
                bool odd = false;
                if (encounter.EnabledEnemies.Length > 3) {
                    if (!up &&!down &&!right &&!left) break;
                    if (right) {
                        if (selectedEnemy % 2 == 1)
                            odd = true;
                        selectedEnemy = (selectedEnemy + 2) % encounter.EnabledEnemies.Length;
                        if (encounter.EnabledEnemies.Length % 2 == 1 && selectedEnemy < 2) selectedEnemy = odd ? 1 : 0;
                    } else if (left) {
                        if (selectedEnemy % 2 == 1)
                            odd = true;
                        selectedEnemy = (selectedEnemy - 2 + encounter.EnabledEnemies.Length) % encounter.EnabledEnemies.Length;
                        if (encounter.EnabledEnemies.Length % 2 == 1 && selectedEnemy > encounter.EnabledEnemies.Length - 3)
                            if (odd && encounter.EnabledEnemies.Length % 2 == 0)      selectedEnemy = encounter.EnabledEnemies.Length - 1;
                            else if (odd && encounter.EnabledEnemies.Length % 2 == 1) selectedEnemy = encounter.EnabledEnemies.Length - 2;
                            else if (!odd && encounter.EnabledEnemies.Length % 2 == 0) selectedEnemy = encounter.EnabledEnemies.Length - 2;
                            else if (!odd && encounter.EnabledEnemies.Length % 2 == 1) selectedEnemy = encounter.EnabledEnemies.Length - 1;
                    } else if (selectedEnemy / 2 * 2 + (selectedEnemy % 2 + 1) % 2 < encounter.EnabledEnemies.Length)
                        selectedEnemy = selectedEnemy / 2 * 2 + (selectedEnemy % 2 + 1) % 2;
                    if (right || left) {
                        string[] colors;
                        string[] textTemp = GetEnemyPage(selectedEnemy / 2, out colors);
                        mainTextManager.SetText(new SelectMessage(textTemp, false, colors));
                        if (forcedAction == Actions.FIGHT)
                            RenewLifeBars(selectedEnemy / 2);
                    }
                    SetPlayerOnSelection(selectedEnemy % 2 * 2);
                } else {
                    if (!up && !down) break;
                    if (up)           selectedEnemy--;
                    else              selectedEnemy++;
                    selectedEnemy = (selectedEnemy + encounter.EnabledEnemies.Length) % encounter.EnabledEnemies.Length;
                    SetPlayerOnSelection(selectedEnemy * 2);
                }
                break;

            case "ACTMENU":
                if (!up && !down && !left && !right)
                    return;

                int xCol = selectedAction % 2; // can just use remainder here, xCol will never be negative at this part
                int yCol = selectedAction / 2;

                if (left)       xCol--;
                else if (right) xCol++;
                else if (up)    yCol--;
                else            yCol++;

                int actionCount = encounter.EnabledEnemies[selectedEnemy].ActCommands.Length;
                int leftColSize = (actionCount + 1) / 2;
                int rightColSize = actionCount / 2;

                if (left || right) xCol = Math.Mod(xCol, 2);
                if (up || down)    yCol = xCol == 0 ? Math.Mod(yCol, leftColSize) : Math.Mod(yCol, rightColSize);
                int desiredAction = yCol * 2 + xCol;
                if (desiredAction >= 0 && desiredAction < actionCount) {
                    selectedAction = desiredAction;
                    SetPlayerOnSelection(selectedAction);
                }
                break;

            case "ITEMMENU":
                if (!up &&!down &&!left &&!right)
                    return;

                int xColI = Math.Mod(selectedItem, 2);
                int yColI = Math.Mod(selectedItem, 4) / 2;

                if (left)       xColI--;
                else if (right) xColI++;
                else if (up)    yColI--;
                else            yColI++;

                // UnitaleUtil.writeInLog("xCol after controls " + xColI);
                // UnitaleUtil.writeInLog("yCol after controls " + yColI);

                const int itemCount = 4; // HACK: should do item count based on page number...
                const int leftColSizeI = (itemCount + 1) / 2;
                const int rightColSizeI = itemCount / 2;
                int desiredItem = (selectedItem / 4) * 4;
                switch (xColI) {
                    case -1:
                        xColI       =  1;
                        desiredItem -= 4;
                        break;
                    case 2:
                        xColI       =  0;
                        desiredItem += 4;
                        break;
                }

                if (up || down)
                    yColI = xColI == 0 ? Math.Mod(yColI, leftColSizeI) : Math.Mod(yColI, rightColSizeI);
                desiredItem += (yColI * 2 + xColI);

                // UnitaleUtil.writeInLog("xCol after evaluation " + xColI);
                // UnitaleUtil.writeInLog("yCol after evaluation " + yColI);

                // UnitaleUtil.writeInLog("Unchecked desired item " + desiredItem);

                if (desiredItem < 0)                              desiredItem = Math.Mod(desiredItem, 4) + (Inventory.inventory.Count / 4) * 4;
                else if (desiredItem > Inventory.inventory.Count) desiredItem = Math.Mod(desiredItem, 4);

                if (desiredItem != selectedItem && desiredItem < Inventory.inventory.Count) { // 0 check not needed, done before
                    selectedItem = desiredItem;
                    SetPlayerOnSelection(Math.Mod(selectedItem, 4));
                    int page = selectedItem / 4;
                    mainTextManager.SetText(new SelectMessage(GetInventoryPage(page), false));
                }

                // UnitaleUtil.writeInLog("Desired item index after evaluation " + desiredItem);
                break;

            case "MERCYMENU":
                if (!up && !down) break;
                if (up)           selectedMercy--;
                else              selectedMercy++;
                selectedMercy = encounter.CanRun ? Math.Mod(selectedMercy, 2) : 0;

                SetPlayerOnSelection(selectedMercy * 2);
                break;
        }
    }

    private void HandleCancel() {
        switch (state) {
            case "ACTIONSELECT":
            case "DIALOGRESULT":
                if (mainTextManager.CanSkip() &&!mainTextManager.LineComplete())
                    mainTextManager.DoSkipFromPlayer();
                break;

            case "ENEMYDIALOGUE":
                bool singleLineAll = true;
                bool cannotSkip = false;
                // why two booleans for the same result? 'cause they're different conditions
                foreach (TextManager mgr in monsterDialogues) {
                    if (!mgr.CanSkip())
                        cannotSkip = true;

                    if (mgr.LineCount() > 1)
                        singleLineAll = false;
                }

                if (cannotSkip || singleLineAll)
                    break;

                foreach (TextManager mgr in monsterDialogues)
                    mgr.DoSkipFromPlayer();
                break;

            case "ACTMENU":
                SwitchState("ENEMYSELECT");
                break;

            case "ENEMYSELECT":
            case "ITEMMENU":
            case "MERCYMENU":
                SwitchState("ACTIONSELECT");
                break;
        }
    }

    private void PlaySound(AudioClip clip) {
        if (!uiAudio.clip.Equals(clip))
            uiAudio.clip = clip;
        uiAudio.Play();
    }

    public static void PlaySoundSeparate(string sound) { UnitaleUtil.PlaySound("SeparateSound", sound, 0.95f); }

    public Vector2 FindPlayerOffsetForAction(Actions action) {
        string str = action.ToString();
        Image image;
        buttonDictionary.TryGetValue(str, out image);
        return action != Actions.NONE ? new Vector2(image.transform.position.x + playerOffsets[(int)action].x, image.transform.position.y + playerOffsets[(int)action].y) : Vector2.zero;
    }

    private void SetPlayerOnAction(Actions newAction) {
        switch (newAction) {
            case Actions.FIGHT: fightButton.overrideSprite = fightButtonSprite; break;
            case Actions.ACT:   actButton.overrideSprite   = actButtonSprite;   break;
            case Actions.ITEM:  itemButton.overrideSprite  = itemButtonSprite;  break;
            case Actions.MERCY: mercyButton.overrideSprite = mercyButtonSprite; break;
            default:            return;
        }

        if (state == "ACTIONSELECT")
            PlayerController.instance.SetPosition(FindPlayerOffsetForAction(newAction).x, FindPlayerOffsetForAction(newAction).y, true);
    }

    public void MovePlayerToAction(Actions act) {
        fightButton.overrideSprite = null;
        actButton.overrideSprite = null;
        itemButton.overrideSprite = null;
        mercyButton.overrideSprite = null;

        action = act;
        action = FindAvailableAction(0);
        SetPlayerOnAction(action);
    }

    // visualization:
    // 0    1
    // 2    3
    // 4    5
    private void SetPlayerOnSelection(int selection) {
        int xMv = selection % 2; // remainder safe again, selection is never negative
        int yMv = selection / 2;
        PlayerController.instance.SetPosition(upperLeft.x + ArenaManager.instance.currentX + xMv * 256,
                                              upperLeft.y + ArenaManager.instance.currentY - yMv * mainTextManager.Charset.LineSpacing, true);
    }

    private void Start() {
        messages = new Dictionary<int, string[]>();
        // reset GlobalControls' frame timer
        GlobalControls.frame = 0;

        mainTextManager = GameObject.Find("TextManager").GetComponent<TextManager>();
        mainTextManager.SetEffect(new TwitchEffect(mainTextManager));
        mainTextManager.ResetFont();
        mainTextManager.SetCaller(EnemyEncounter.script);
        encounter = FindObjectOfType<EnemyEncounter>();

        fightButton = GameObject.Find("FightBt").GetComponent<Image>();
        actButton = GameObject.Find("ActBt").GetComponent<Image>();
        itemButton = GameObject.Find("ItemBt").GetComponent<Image>();
        mercyButton = GameObject.Find("MercyBt").GetComponent<Image>();
        if (GlobalControls.crate) {
            fightButton.sprite = SpriteRegistry.Get("UI/Buttons/gifhtbt_0");
            fightButton.GetComponent<AutoloadResourcesFromRegistry>().SpritePath = "UI/Buttons/gifhtbt_0";
            actButton.sprite = SpriteRegistry.Get("UI/Buttons/catbt_0");
            actButton.GetComponent<AutoloadResourcesFromRegistry>().SpritePath = "UI/Buttons/catbt_0";
            itemButton.sprite = SpriteRegistry.Get("UI/Buttons/tembt_0");
            itemButton.GetComponent<AutoloadResourcesFromRegistry>().SpritePath = "UI/Buttons/tembt_0";
            mercyButton.sprite = SpriteRegistry.Get("UI/Buttons/mecrybt_0");
            mercyButton.GetComponent<AutoloadResourcesFromRegistry>().SpritePath = "UI/Buttons/mecrybt_0";
        }
        // Add dictionaries to easily access buttons and their data through strings
        buttonDictionary.Add("FIGHT", fightButton);
        buttonDictionary.Add("ACT", actButton);
        buttonDictionary.Add("ITEM", itemButton);
        buttonDictionary.Add("MERCY", mercyButton);
        buttonSpriteDictionary.Add("FIGHT", fightButtonSprite);
        buttonSpriteDictionary.Add("ACT", actButtonSprite);
        buttonSpriteDictionary.Add("ITEM", itemButtonSprite);
        buttonSpriteDictionary.Add("MERCY", mercyButtonSprite);
        buttonBasePositions.Add("FIGHT", new Vector2(32, 6));
        buttonBasePositions.Add("ACT", new Vector2(185, 6));
        buttonBasePositions.Add("ITEM", new Vector2(355, 6));
        buttonBasePositions.Add("MERCY", new Vector2(500, 6));
        buttonBasePlayerPositions.Add("FIGHT", new Vector2(16, 19));
        buttonBasePlayerPositions.Add("ACT", new Vector2(16, 19));
        buttonBasePlayerPositions.Add("ITEM", new Vector2(16, 19));
        buttonBasePlayerPositions.Add("MERCY", new Vector2(16, 19));

        ArenaManager.instance.ResizeImmediate(ArenaManager.UIWidth, ArenaManager.UIHeight);
        //ArenaManager.instance.MoveToImmediate(0, -160, false);

        /*GameObject.Find("HideEncounter").GetComponent<Image>().sprite = Sprite.Create(GlobalControls.texBeforeEncounter,
                                                                                      new Rect(0, 0, GlobalControls.texBeforeEncounter.width, GlobalControls.texBeforeEncounter.height),
                                                                                      new Vector2(0.5f, 0.5f));*/
        //if (GameOverBehavior.gameOverContainer)
        //    GameObject.Destroy(GameOverBehavior.gameOverContainer);
        GameOverBehavior.gameOverContainer = GameObject.Find("GameOverContainer");
        GameOverBehavior.gameOverContainer.SetActive(false);
        MusicManager.src = Camera.main.GetComponent<AudioSource>();
        //NewMusicManager.OnLevelWasLoaded();

        if (NewMusicManager.audiolist.ContainsKey("src"))
            NewMusicManager.audiolist.Remove("src");
        if (NewMusicManager.audiolist.ContainsKey("StaticKeptAudio"))
            NewMusicManager.audiolist.Remove("StaticKeptAudio");

        MusicManager.src = Camera.main.GetComponent<AudioSource>();
        NewMusicManager.audiolist.Add("src", MusicManager.src);
        if (PlayerOverworld.audioKept)
            NewMusicManager.audiolist.Add("StaticKeptAudio", PlayerOverworld.audioKept);

        ProjectileController.globalPixelPerfectCollision = false;
        ControlPanel.instance.FrameBasedMovement = false;

        LuaScriptBinder.CopyToBattleVar();
        if (EnemyEncounter.script.GetVar("Update") != null)
            encounterHasUpdate = true;
        GameObject.Find("Main Camera").GetComponent<ProjectileHitboxRenderer>().enabled = !GameObject.Find("Main Camera").GetComponent<ProjectileHitboxRenderer>().enabled;
        //There are scene init bugs, let's fix them!
        /*if (GameObject.Find("TopLayer").transform.parent != GameObject.Find("Canvas").transform) {
            RectTransform[] rts = GameObject.Find("Canvas").GetComponentsInChildren<RectTransform>(true);
            rts[rts.Length - 1].SetParent(rts[rts.Length - 2]);
            GameObject.Find("TopLayer").transform.SetParent(GameObject.Find("Canvas").transform);
            rts[rts.Length - 2].SetAsLastSibling();
        } else {*/
        /*bool toAdd = false; int indexDeb = 0, indexText = 0, j = 0;
        Transform[] rts = UnitaleUtil.GetFirstChildren(GameObject.Find("Canvas").transform, true);
        foreach (Transform rt in rts) {
            if (rt.gameObject.name == "Text") {
                rt.SetParent(GameObject.Find("Debugger").transform);
                indexText = j;
                toAdd = true;
                break;
            } else if (rt.gameObject.name == "Debugger") {
                rt.SetAsLastSibling();
                indexDeb = j;
                break;
            }
            j++;
        }
        if (toAdd)
            rts[indexText].SetParent(rts[indexDeb]);*/
        //}

        // If retromode is enabled, set the inventory to the one with TESTDOGs (can be overridden)
        if (GlobalControls.retroMode && GlobalControls.modDev) {
            // Set the in-game names of these items to TestDogN instead of DOGTESTN
            for (int i = 1; i <= 7; i++)
                Inventory.NametoShortName.Add("DOGTEST" + i, "TestDog" + i);

            Inventory.luaInventory.AddCustomItems(new[] {"DOGTEST1", "DOGTEST2", "DOGTEST3", "DOGTEST4", "DOGTEST5", "DOGTEST6", "DOGTEST7"},
                                           new[] {3, 3, 3, 3, 3, 3, 3});
            Inventory.luaInventory.SetInventory(new[] {"DOGTEST1", "DOGTEST2", "DOGTEST3", "DOGTEST4", "DOGTEST5", "DOGTEST6", "DOGTEST7"});

            // Undo our changes to this table!
            for (int i = 1; i <= 7; i++)
                Inventory.NametoShortName.Remove("DOGTEST" + i);
        }

        StaticInits.SendLoaded();
        psContainer = new GameObject("psContainer");
        // The following is a trick to make psContainer spawn within the battle scene, rather than the overworld scene, if in the overworld
        psContainer.transform.SetParent(mainTextManager.transform);
        psContainer.transform.SetParent(null);
        psContainer.transform.SetAsFirstSibling();

        //Play that funky music
        if (MusicManager.IsStoppedOrNull(PlayerOverworld.audioKept))
            GameObject.Find("Main Camera").GetComponent<AudioSource>().Play();

        if (SendToStaticInit != null)
            SendToStaticInit();

        if (GlobalControls.crate) {
            UserDebugger.instance.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "DEGUBBER (F9 OT TOGLGE, DEBUG(STIRNG) TO PRNIT)";
            LuaSpriteController.GetOrCreate(GameObject.Find("HPLabel")).Set("UI/spr_phname_0");
        }

        // PlayerController.instance.Awake();
        PlayerController.instance.playerAbs = new Rect(0, 0,
                                                        PlayerController.instance.selfImg.sprite.texture.width  - 8,
                                                        PlayerController.instance.selfImg.sprite.texture.height - 8);
        PlayerController.instance.setControlOverride(true);
        PlayerController.instance.SetPosition(48, 25, true);
        fightUI = GameObject.Find("FightUI").GetComponent<FightUIController>();
        fightUI.gameObject.SetActive(false);

        if (UnitaleUtil.firstErrorShown) return;
        encounter.CallOnSelfOrChildren("EncounterStarting");

        if (!stateSwitched)
            SwitchState("ACTIONSELECT", true);
    }

    public void CheckAndTriggerVictory() {
        if (encounter.EnabledEnemies.Length > 0)
            return;
        Camera.main.GetComponent<AudioSource>().Stop();
        bool levelUp = PlayerCharacter.instance.AddBattleResults(exp, gold);
        Inventory.RemoveAddedItems();
        if (levelUp && exp != 0) {
            UIStats.instance.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
            UIStats.instance.setMaxHP();
            UIStats.instance.setHP(PlayerCharacter.instance.HP);
            ActionDialogResult(new RegularMessage("[sound:levelup]YOU WON!\nYou earned "+ exp +" XP and "+ gold +" gold.\nYour LOVE increased."), "DONE");
        } else
            ActionDialogResult(new RegularMessage("YOU WON!\nYou earned " + exp + " XP and " + gold + " gold."), "DONE");
    }

    public IEnumerator ISuperFlee() {
        PlayerController.instance.GetComponent<Image>().enabled = false;
        UnitaleUtil.PlaySound("Mercy", "runaway");

        List<string> fleeTexts = new List<string>();
        DynValue tempFleeTexts = EnemyEncounter.script.GetVar("fleetexts");
        if (tempFleeTexts.Type == DataType.Table)
            for (int i = 0; i < tempFleeTexts.Table.Length; i++)
                fleeTexts.Add(tempFleeTexts.Table.Get(i + 1).String);
        else {
            /*fleeTexts = new List<string> { "I'm outta here.",  "I've got better things to do.", "Don't waste my time.",
                                           "Nah, I don't like you.", "I just wanted to walk\ra bit. Leave me alone.", "You're cute, I won't kill you :3",
                                           "Better safe than sorry.", "Do as if you never saw\rthem and walk away.", "I'll kill you last.",
                                           "Nope. [w:5]Nope. Nope. Nope. Nope.", "Wait for me, Rhenaud!", "Flee like sissy!" };
            if (!ControlPanel.instance.Safe) {
                fleeTexts.Add("I've got shit to do.");
                fleeTexts.Add("Fuck this shit I'm out.");
            }*/
            if (exp > 0 || gold > 0) {
                string fleeString = "Ran away with " + exp + " EXP\rand " + gold + " GOLD.";
                bool levelUp = PlayerCharacter.instance.AddBattleResults(exp, gold);
                if (levelUp && exp > 0) {
                    UIStats.instance.setPlayerInfo(PlayerCharacter.instance.Name, PlayerCharacter.instance.LV);
                    UIStats.instance.setMaxHP();
                    UIStats.instance.setHP(PlayerCharacter.instance.HP);
                    fleeString = "[sound:levelup]" + fleeString + "\nYour LOVE increased.";
                }
                fleeTexts = new List<string> { fleeString };
            } else
                fleeTexts = new List<string> {
                    "Escaped...",
                    "Don't slow me down.",
                    "I've got better to do.",
                    "I'm outta here."
                };
        }

        ActionDialogResult(new TextMessage[] { new RegularMessage(fleeTexts[Math.RandomRange(0, fleeTexts.Count)]) });
        fleeSwitch = true;

        Camera.main.GetComponent<AudioSource>().Pause();
        LuaSpriteController spr = (LuaSpriteController)SpriteUtil.MakeIngameSprite("spr_heartgtfo_0", "Top").UserData.Object;
        spr.absx = PlayerController.instance.transform.position.x;
        spr.absy = PlayerController.instance.transform.position.y;
        spr.SetAnimation(new[] { "spr_heartgtfo_0", "spr_heartgtfo_1" }, 1 / 10f);
        spr.color = new[] { PlayerController.instance.GetComponent<Image>().color.r, PlayerController.instance.GetComponent<Image>().color.g, PlayerController.instance.GetComponent<Image>().color.b };
        while (spr.absx > -20) {
            spr.absx--;
            yield return 0;
        }
    }

    // Update is called once per frame
    private void Update() {
        //frameDebug++;
        stateSwitched = false;
        if (encounter.gameOverStance)
            return;
        if (encounterHasUpdate)
            UnitaleUtil.TryCall(EnemyEncounter.script, "Update");

        if (frozenState != "PAUSE")
            return;

        if (mainTextManager.IsPaused() &&!ArenaManager.instance.isResizeInProgress())
            mainTextManager.SetPause(false);

        if (state == "DIALOGRESULT")
            if (mainTextManager.CanAutoSkipAll() || (mainTextManager.CanAutoSkipThis() && mainTextManager.LineComplete()))
                if (mainTextManager.HasNext())
                    mainTextManager.NextLineText();
                else {
                    mainTextManager.DestroyChars();
                    SwitchState(stateAfterDialogs);
                }

        if (state == "ENEMYDIALOGUE") {
            if (monsterDialogues.All(mgr => mgr.CanAutoSkipThis())) DoNextMonsterDialogue();
            else                                                             UpdateMonsterDialogue();
        }

        if (state == "DEFENDING") {
            if (!encounter.WaveInProgress()) {
                if (GlobalControls.retroMode)
                    foreach (LuaProjectile p in FindObjectsOfType<LuaProjectile>())
                            BulletPool.instance.Requeue(p);
                SwitchState("ACTIONSELECT");
            } else if (!encounter.gameOverStance && frozenState == "PAUSE")
                encounter.UpdateWave();
            return;
        }

        if (!fleeSwitch)
            if (InputUtil.Pressed(GlobalControls.input.Confirm)) {
                if (state == "ACTIONSELECT" && !ArenaManager.instance.isMoveInProgress() && !ArenaManager.instance.isResizeInProgress() || state != "ACTIONSELECT")
                    HandleAction();
            } else if (InputUtil.Pressed(GlobalControls.input.Cancel)) HandleCancel();
            else HandleArrows();
        else if (InputUtil.Pressed(GlobalControls.input.Confirm))
            SwitchState("DONE");

        if ((state == "ATTACKING" || needOnDeath) && fightUI.Finished()) {
            bool noOnDeath = true;
            onDeathSwitch = true;
            bool playSound = true;
            foreach (EnemyController enemyController in encounter.EnabledEnemies) {
                if (enemyController.HP > 0 || enemyController.Unkillable) continue;
                // fightUI.disableImmediate();
                if (UnitaleUtil.TryCall(enemyController.script, "OnDeath")) continue;
                noOnDeath = false;
                enemyController.DoKill(playSound);
                playSound = false;

                if (encounter.EnabledEnemies.Length > 0)
                    SwitchState("ENEMYDIALOGUE");
                //else
                //    checkAndTriggerVictory();
            }

            onDeathSwitch = false;
            if (state == "ATTACKING" && fightUI.Finished()) {
                if (lastNewState != "UNUSED") {
                    SwitchState(lastNewState);
                    lastNewState = "UNUSED";
                } else if (noOnDeath)
                    SwitchState("ENEMYDIALOGUE");
            }
            needOnDeath = false;
        }
        //if (state == UIState.ENEMYDIALOGUE)
        //    if ((Vector2)arenaParent.transform.position == new Vector2(320, 90))
        //        PlayerController.instance.setControlOverride(false);
    }
}
