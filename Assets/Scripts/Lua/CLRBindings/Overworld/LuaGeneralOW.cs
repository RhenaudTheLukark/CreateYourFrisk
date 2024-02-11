using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;

public class LuaGeneralOW {
    private readonly TextManager textmgr;
    public ScriptWrapper appliedScript;

    public delegate void LoadedAction(string coroName, object args, string evName);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    [MoonSharpHidden] public LuaGeneralOW(TextManager textmgr) { this.textmgr = textmgr; }

    [CYFEventFunction] public void HiddenReloadAppliedScript() { EventManager.instance.CheckCurrentEvent(); }

    /// <summary>
    /// Displays a text.
    /// </summary>
    /// <param name="texts"></param>
    /// <param name="formatted"></param>
    /// <param name="mugshots"></param>
    /// <param name="forcePosition"></param>
    [CYFEventFunction] public void SetDialog(DynValue texts, bool formatted = true, DynValue mugshots = null, DynValue forcePosition = null) {
        // Unfortunately, either C# or MoonSharp (don't know which) has a ridiculous limit in place
        // Calling `SetDialog({""}, true, nil, true)` fails to pass the final argument
        if (mugshots != null && mugshots.Type == DataType.Table && forcePosition != null) PlayerOverworld.instance.UIPos = forcePosition.Type == DataType.Boolean ? (forcePosition.Boolean ? 2 : 1) : 0;
        else                                                                              PlayerOverworld.instance.UIPos = 0;

        if (EventManager.instance.coroutines.ContainsKey(appliedScript) && EventManager.instance.script != appliedScript) {
            UnitaleUtil.DisplayLuaError(appliedScript.scriptname, "General.SetDialog: This function cannot be used in a coroutine.");
            return;
        }
        if (EventManager.instance.eventsLoading) {
            UnitaleUtil.DisplayLuaError(appliedScript.scriptname, "General.SetDialog: This function cannot be used in EventPage0.");
            return;
        }
        TextMessage[] textmsgs;
        if (texts.Type == DataType.String && texts.String.Length > 0)
            textmsgs = new[]{new TextMessage(texts.String, formatted, false, mugshots != null ? mugshots.Type == DataType.Table ? mugshots.Table.Get(0) : mugshots : null)};
        else if (texts.Type == DataType.Table && texts.Table.Length > 0) {
            textmsgs = new TextMessage[texts.Table.Length];
            for (int i = 0; i < texts.Table.Length; i++)
                textmsgs[i] = new TextMessage(texts.Table.Get(i + 1).String, formatted, false, mugshots != null ? mugshots.Type == DataType.Table ? mugshots.Table.Get(i+1) : mugshots : null);
        } else {
            UnitaleUtil.DisplayLuaError(EventManager.instance.eventScripts[EventManager.instance.events[EventManager.instance.actualEventIndex]].scriptname, "General.SetDialog: You need to input a non-empty array or string here.");
            return;
        }
        textmgr.SetEffect(null);
        textmgr.SetTextQueue(textmsgs);
        textmgr.transform.parent.parent.SetAsLastSibling();
    }

    /// <summary>
    /// Makes a choice, like when you have to choose between cinnamon and butterscotch
    /// </summary>
    /// <param name="choices"></param>
    /// <param name="question"></param>
    /// <param name="forcePosition"></param>
    [CYFEventFunction] public void SetChoice(DynValue choices, string question = "", DynValue forcePosition = null) {
        // Unfortunately, something weird is happening here
        // Calling `SetChoice({"Yes", "No"}, nil, true)` fails to pass the final argument
        if (question != null && forcePosition != null) PlayerOverworld.instance.UIPos = forcePosition.Type == DataType.Boolean ? (forcePosition.Boolean ? 2 : 1) : 0;
        else                                           PlayerOverworld.instance.UIPos = 0;

        TextMessage textMsgChoice = new TextMessage("", false, false);
        textMsgChoice.AddToText("[mugshot:null]");
        List<string> finalText = new List<string>();
        bool[] oneLiners = new bool[2];
        List<string[]> preTexts = new List<string[]>();
        bool threeLiner = false;

        //Do not put more than 3 lines and 2 choices
        //If the 3rd parameter is a string, it has to be a question
        if (question != "") {
            textMsgChoice.AddToText("* " + question + "\n");
        }
        for (int i = 0; i < 2; i++) {
            //If there's no text, just don't print it
            if (choices.Table.Get(i + 1).String == null)
                continue;

            preTexts.Add(choices.Table.Get(i + 1).String.Split('\n'));
            string[] preText = preTexts[preTexts.Count - 1];
            oneLiners[i] = preText.Length == 1 && question != "";
            if (preText.Length == 3)
                threeLiner = true;
            if (!oneLiners[i]) continue;
            string line = preText[0];
            preTexts[preTexts.Count - 1] = new[] { "", line };
        }

        for (int i = 0; i < 2; i++) {
            string[] preText = preTexts[i];
            for (int j = 0; j < (threeLiner ? 3 : 2); j++) {
                if (j == finalText.Count)
                    finalText.Add("");
                finalText[j] += "[charspacing:8] [charspacing:2]" + (j >= preText.Length ? "" : preText[j]) + (i == 0 ? "\t" : "") + "[charspacing:default]";
            }
        }

        //Add the text to the text to print then the SetChoice function with its parameters
        for (int i = 0; i < finalText.Count; i++) {
            if (finalText[i] != "\t")
                textMsgChoice.AddToText(finalText[i] + ((i == finalText.Count - 1) ? "" : "\n"));
        }
        textmgr.SetEffect(null);
        textmgr.SetText(textMsgChoice);
        textmgr.transform.parent.parent.SetAsLastSibling();

        if (StCoroutine != null) StCoroutine("ISetChoice", new object[] { question != "", oneLiners }, appliedScript.GetVar("_internalScriptName").String);
    }

    [CYFEventFunction] public void EndDialog() {
        if (EventManager.instance.eventsLoading) {
            UnitaleUtil.DisplayLuaError(appliedScript.scriptname, "General.EndDialog: This function cannot be used in EventPage0.");
            return;
        }
        if (EventManager.instance.script == appliedScript) {
            UnitaleUtil.DisplayLuaError(appliedScript.scriptname, "General.EndDialog: This function can only be used in a coroutine.");
            return;
        }

        if (textmgr != null && textmgr.GetComponent<UnityEngine.UI.Image>().color.a != 0) {
            // Clean up text manager
            textmgr.SetTextFrameAlpha(0);
            textmgr.SetTextQueue(new TextMessage[] { });
            textmgr.HideTextObject();

            // Clean up SetChoice if applicable
            if (EventManager.instance.script != null && EventManager.instance.script == textmgr.caller) {
                string key = EventManager.instance.script.GetVar("_internalScriptName").String + ".ISetChoice";
                if (EventManager.instance.cSharpCoroutines.ContainsKey(key)) {
                    // Stop the ISetChoice coroutine
                    EventManager.instance.ForceEndCoroutine(key);

                    // Remove the "tempHeart" GameObject if it already exists
                    if (GameObject.Find("Canvas OW/tempHeart"))
                        Object.Destroy(GameObject.Find("Canvas OW/tempHeart"));
                }
            }

            // End event
            appliedScript.Call("CYFEventNextCommand");
            if (EventManager.instance.script != null && EventManager.instance.script == textmgr.caller) // End text from event
                textmgr.caller.Call("CYFEventNextCommand");
            else
                PlayerOverworld.instance.PlayerNoMove = false;  // End text no event
        } else
            appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public void Wait(int frames) { if (StCoroutine != null) StCoroutine("IWait", frames, appliedScript.GetVar("_internalScriptName").String); }

    /// <summary>
    /// Function that ends when the player press the button "Confirm"
    /// </summary>
    [CYFEventFunction] public void WaitForInput() { if (StCoroutine != null) StCoroutine("IWaitForInput", null, appliedScript.GetVar("_internalScriptName").String); }

    /// <summary>
    /// Launch the GameOver screen
    /// </summary>
    [CYFEventFunction] public void GameOver(DynValue deathText = null, string deathMusic = null) {
        PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        /*Transform rt = GameObject.Find("Player").GetComponent<Transform>();
        rt.position = new Vector3(rt.position.x, rt.position.y, -1000);*/
        string[] deathTable = null;

        if (deathText != null && deathText.Type != DataType.Void) {
            switch (deathText.Type) {
                case DataType.Table: {
                    deathTable = new string[deathText.Table.Length];
                    for (int i = 0; i < deathText.Table.Length; i++)
                        deathTable[i] = deathText.Table[i + 1].ToString();
                    break;
                }
                case DataType.String: deathTable = new[] { deathText.String }; break;
                default:              throw new CYFException("General.GameOver: deathText needs to be a table or a string.");
            }
        }

        PlayerOverworld.instance.enabled = false;

        // Stop the "kept audio" if it is playing
        if (PlayerOverworld.audioKept == UnitaleUtil.GetCurrentOverworldAudio()) {
            PlayerOverworld.audioKept.Stop();
            PlayerOverworld.audioKept.clip = null;
            PlayerOverworld.audioKept.time = 0;
        }

        //Saves our most recent map and position to control where the player respawns
        string mapName = UnitaleUtil.MapCorrespondanceList.ContainsKey(SceneManager.GetActiveScene().name) ? UnitaleUtil.MapCorrespondanceList[SceneManager.GetActiveScene().name] : SceneManager.GetActiveScene().name;
        LuaScriptBinder.Set(null, "PlayerMap", DynValue.NewString(mapName));

        Transform tf = GameObject.Find("Player").transform;
        LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(tf.position.x));
        LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(tf.position.y));
        LuaScriptBinder.Set(null, "PlayerPosZ", DynValue.NewNumber(tf.position.z));

        Object.FindObjectOfType<GameOverBehavior>().StartDeath(deathTable, deathMusic);

        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Plays and adjust the volume of a chosen bgm.
    /// </summary>
    /// <param name="bgm">The name of the chosen BGM to play.</param>
    /// <param name="volume">The volume of the BGM. Clamped from 0 to 1.</param>
    [CYFEventFunction] public void PlayBGM(string bgm, float volume) {
        volume = Mathf.Clamp01(volume);
        if (AudioClipRegistry.GetMusic(bgm) == null)
            throw new CYFException("General.PlayBGM: The given BGM doesn't exist. Please check if you've spelled it correctly.");
        AudioSource audio = UnitaleUtil.GetCurrentOverworldAudio();
        audio.clip = AudioClipRegistry.GetMusic(bgm);
        audio.volume = volume;
        audio.Play();
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Stops the current BGM.
    /// </summary>
    /// <param name="fadeFrames"></param>
    /// <param name="waitEnd"></param>
    [CYFEventFunction] public void StopBGM(int fadeFrames = 0, bool waitEnd = false) {
        if (EventManager.instance.bgmCoroutine)                                       throw new CYFException("General.StopBGM: The music is already fading.");
        if (!GameObject.Find("Main Camera OW").GetComponent<AudioSource>().isPlaying) throw new CYFException("General.StopBGM: There is no current BGM.");
        if (fadeFrames < 0)                                                           throw new CYFException("General.StopBGM: The fade time has to be positive or equal to 0.");
        if (StCoroutine != null) StCoroutine("IFadeBGM", new object[] { fadeFrames, waitEnd }, appliedScript.GetVar("_internalScriptName").String);
        if (!waitEnd)
            appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Plays a selected sound at a given volume.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="volume"></param>
    [CYFEventFunction] public void PlaySound(string sound, float volume = 0.65f) {
        volume = Mathf.Clamp01(volume);
        if (AudioClipRegistry.GetSound(sound) == null)
            throw new CYFException("General.PlaySound: The given BGM doesn't exist. Please check if you've spelled it correctly.");
        UnitaleUtil.PlaySound("PlaySound", sound, volume);
        //GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound(sound), volume);
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Saves the game. Pretty obvious, heh.
    /// </summary>
    [CYFEventFunction] public void Save(bool forced = false) { if (StCoroutine != null) StCoroutine("ISave", new object[] { forced }, appliedScript.GetVar("_internalScriptName").String); }

    /// <summary>
    /// Sends the player back to the title screen, making him lose his progression
    /// </summary>
    [CYFEventFunction] public void TitleScreen() {
        UnitaleUtil.ExitOverworld(false);
        SceneManager.LoadScene("TitleScreen");
        Object.Destroy(GameObject.Find("SpritePivot"));
    }

    /// <summary>
    /// Sets an encounter of the current mod folder, with a given encounter name
    /// The boolean is used to tell if the encounter anim will be short
    /// </summary>
    /// <param name="encounterName"></param>
    /// <param name="anim"></param>
    /// <param name="ForceNoFlee"></param>
    [CYFEventFunction] public void SetBattle(string encounterName = "", string anim = "normal", bool ForceNoFlee = false) {
        anim = anim.ToLower();
        if (anim != "normal" && anim != "fast" && anim != "instant")
            throw new CYFException("General.SetBattle: Invalid animation \"" + anim + "\".\nIt should be\"normal\", \"fast\" or \"instant\".");

        PlayerOverworld.instance.SetEncounterAnim(encounterName, anim, ForceNoFlee);
    }

    [CYFEventFunction] public void EnterShop(string scriptName, bool instant = false) {
        ShopScript.scriptName = scriptName;
        if (StCoroutine != null) StCoroutine("IEnterShop", instant, appliedScript.GetVar("_internalScriptName").String);
    }
}
