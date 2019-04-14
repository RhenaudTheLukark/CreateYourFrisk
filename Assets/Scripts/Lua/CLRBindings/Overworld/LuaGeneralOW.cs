using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;

public class LuaGeneralOW {
    private TextManager textmgr;
    public ScriptWrapper appliedScript;

    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    [MoonSharpHidden] public LuaGeneralOW(TextManager textmgr) { this.textmgr = textmgr; }

    [CYFEventFunction] public void HiddenReloadAppliedScript() { EventManager.instance.CheckCurrentEvent(); }

    /// <summary>
    /// Displays a text.
    /// </summary>
    /// <param name="texts"></param>
    /// <param name="formatted"></param>
    /// <param name="mugshots"></param>
    [CYFEventFunction] public void SetDialog(DynValue texts, bool formatted = true, DynValue mugshots = null, DynValue forcePosition = null) {
        // Unfortunately, either C# or MoonSharp (don't know which) has a ridiculous limit in place
        // Calling `SetDialog({""}, true, nil, true)` fails to pass the final argument
        if (mugshots != null && mugshots.Type == DataType.Table && forcePosition != null)
            PlayerOverworld.instance.UIPos = forcePosition.Type == DataType.Boolean ? (forcePosition.Boolean == true ? 2 : 1) : 0;
        else
            PlayerOverworld.instance.UIPos = 0;
        
        if (EventManager.instance.coroutines.ContainsKey(appliedScript) && EventManager.instance.script != appliedScript) {
            UnitaleUtil.DisplayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "General.SetDialog: You can't use that function in a coroutine.");
            return;
        } else if (EventManager.instance.LoadLaunched) {
            UnitaleUtil.DisplayLuaError(EventManager.instance.events[EventManager.instance.actualEventIndex].name, "General.SetDialog: You can't use that function in a page 0 function.");
            return;
        }
        TextMessage[] textmsgs = new TextMessage[texts.Table.Length];
        for (int i = 0; i < texts.Table.Length; i++)
            textmsgs[i] = new TextMessage(texts.Table.Get(i + 1).String, formatted, false, mugshots != null ? mugshots.Type == DataType.Table ? mugshots.Table.Get(i+1) : mugshots : null);
        textmgr.SetTextQueue(textmsgs);
        textmgr.transform.parent.parent.SetAsLastSibling();
    }
    
    /// <summary>
    /// Makes a choice, like when you have to choose between cinnamon and butterscotch
    /// </summary>
    /// <param name="question"></param>
    /// <param name="varIndex"></param>
    [CYFEventFunction] public void SetChoice(DynValue choices, string question = "", DynValue forcePosition = null) {
        // Unfortunately, something weird is happening here
        // Calling `SetChoice({"Yes", "No"}, nil, true)` fails to pass the final argument
        if (question != null && forcePosition != null)
            PlayerOverworld.instance.UIPos = forcePosition.Type == DataType.Boolean ? (forcePosition.Boolean == true ? 2 : 1) : 0;
        else
            PlayerOverworld.instance.UIPos = 0;
        
        TextMessage textMsgChoice = new TextMessage("", false, false, true);
        textMsgChoice.AddToText("[mugshot:null]");
        List<string> finalText = new List<string>();
        bool[] oneLiners = new bool[2];

        //Do not put more than 3 lines and 2 choices
        //If the 3rd parameter is a string, it has to be a question
        if (question != "") {
            textMsgChoice.AddToText("* " + question + "\n");
        }
        for (int i = 0; i < 2; i++) {
            //If there's no text, just don't print it
            if (choices.Table.Get(i + 1).String == null)
                continue;

            string[] preText = choices.Table.Get(i + 1).String.Split('\n');
            oneLiners[i] = preText.Length == 1 && question != "";
            if (oneLiners[i]) {
                string line = preText[0];
                preText = new string[] { "", line };
            }

            for (int j = 0; j < 3; j++) {
                if (j == finalText.Count)
                    finalText.Add("");
                finalText[j] += "[charspacing:8] [charspacing:2]" + (i == 0 ? "     " : "") + (j >= preText.Length ? "" : preText[j]) + (i == 0 ? "\t" : "");
                if (j == 2)
                    break;
            }
        }

        //Add the text to the text to print then the SetChoice function with its parameters
        for (int i = 0; i < finalText.Count; i++) {
            if (finalText[i] != "\t")
                textMsgChoice.AddToText(finalText[i] + ((i == finalText.Count - 1) ? "" : "\n"));
        }
        textmgr.SetText(textMsgChoice);
        textmgr.transform.parent.parent.SetAsLastSibling();

        StCoroutine("ISetChoice", new object[] { question != "", oneLiners });
    }

    [CYFEventFunction] public void Wait(int frames) { StCoroutine("IWait", frames); }

    /// <summary>
    /// Function that ends when the player press the button "Confirm"
    /// </summary>
    [CYFEventFunction] public void WaitForInput() { StCoroutine("IWaitForInput", null); }

    /// <summary>
    /// Launch the GameOver screen
    /// </summary>
    [CYFEventFunction] public void GameOver(DynValue deathText = null, string deathMusic = null) {
        PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        /*Transform rt = GameObject.Find("Player").GetComponent<Transform>();
        rt.position = new Vector3(rt.position.x, rt.position.y, -1000);*/
        string[] deathTable = null;

        if (deathText != null) {
            if (deathText.Type == DataType.Table) {
                deathTable = new string[deathText.Table.Length];
                for (int i = 0; i < deathText.Table.Length; i++)
                    deathTable[i] = deathText.Table[i + 1].ToString();
            } else if (deathText.Type == DataType.String)
                deathTable = new string[] { deathText.String };
            else
                throw new CYFException("General.GameOver: deathText needs to be a table or a string.");
        }

        GlobalControls.Music = UnitaleUtil.GetCurrentOverworldAudio().clip;
        PlayerOverworld.instance.enabled = false;
        
        // Stop the "kept audio" if it is playing
        if (PlayerOverworld.audioKept == UnitaleUtil.GetCurrentOverworldAudio()) {
            PlayerOverworld.audioKept.Stop();
            PlayerOverworld.audioKept.clip = null;
            PlayerOverworld.audioKept.time = 0;
        }
        
        //Saves our most recent map and position to control where the player respawns
        string mapName;
        if (UnitaleUtil.MapCorrespondanceList.ContainsKey(SceneManager.GetActiveScene().name)) mapName = UnitaleUtil.MapCorrespondanceList[SceneManager.GetActiveScene().name];
        else mapName = SceneManager.GetActiveScene().name;
        LuaScriptBinder.Set(null, "PlayerMap", DynValue.NewString(mapName));
        
        Transform tf = GameObject.Find("Player").transform;
        LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(tf.position.x));
        LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(tf.position.y));
        LuaScriptBinder.Set(null, "PlayerPosZ", DynValue.NewNumber(tf.position.z));
        
        GameObject.FindObjectOfType<GameOverBehavior>().StartDeath(deathTable, deathMusic);
        
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
    [CYFEventFunction] public void StopBGM(int fadeFrames = 0, bool waitEnd = false) {
        if (EventManager.instance.bgmCoroutine)
            throw new CYFException("General.StopBGM: The music is already fading.");
        else if (!GameObject.Find("Main Camera OW").GetComponent<AudioSource>().isPlaying)
            throw new CYFException("General.StopBGM: There is no current BGM.");
        else if (fadeFrames < 0)
            throw new CYFException("General.StopBGM: The fade time has to be positive or equal to 0.");
        StCoroutine("IFadeBGM", new object[] { fadeFrames, waitEnd });
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
        UnitaleUtil.PlaySound("PlaySound", AudioClipRegistry.GetSound(sound), volume);
        //GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound(sound), volume);
        appliedScript.Call("CYFEventNextCommand");
    }

    /// <summary>
    /// Saves the game. Pretty obvious, heh.
    /// </summary>
    [CYFEventFunction] public void Save(bool forced = false) { StCoroutine("ISave", new object[] { forced }); }

    /// <summary>
    /// Sends the player back to the title screen, making him lose his progression
    /// </summary>
    [CYFEventFunction] public void TitleScreen() {
        NewMusicManager.DestroyChannel("StaticKeptAudio");
        GameObject.Destroy(GameObject.Find("Player"));
        GameObject.Destroy(GameObject.Find("Canvas OW"));
        StaticInits.MODFOLDER = "@Title";
        StaticInits.Initialized = false;
        StaticInits.InitAll();
        GameObject.Destroy(GameObject.Find("Main Camera OW"));
        SceneManager.LoadScene("TitleScreen");
    }

    /// <summary>
    /// Sets an encounter of the current mod folder, with a given encounter name
    /// The boolean is used to tell if the encounter anim will be short
    /// </summary>
    /// <param name="encounterName"></param>
    /// <param name="quickAnim"></param>
    [CYFEventFunction] public void SetBattle(string encounterName, bool quickAnim = false, bool ForceNoFlee = false) { PlayerOverworld.instance.SetEncounterAnim(encounterName, quickAnim, ForceNoFlee); }


    [CYFEventFunction] public void EnterShop(string scriptName) {
        ShopScript.scriptName = scriptName;
        StCoroutine("IEnterShop", null);
    }
}
