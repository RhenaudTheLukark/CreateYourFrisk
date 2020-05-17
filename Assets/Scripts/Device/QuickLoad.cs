using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System;
using System.IO;

// Are you tired of waiting around? Do you want your mod to feel like a game on it's own?
// Well . . . I can only help with the second one, sadly . . .

public class QuickLoad : MonoBehaviour {

#if UNITY_EDITOR_WIN
    private string modtoload = "Encounter Skeleton";
#else
    private string modtoload = "Just-Monika";
#endif

    //private string encountertoload = "encounter";

    // Use this for initialization
    void Start () {
        StaticInits.Start();
        SaveLoad.Start();
        new ControlPanel();
        new PlayerCharacter();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (GlobalControls.crate)  Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
                else                       Misc.WindowName = ControlPanel.instance.WindowBasisName;
#endif
        SaveLoad.LoadAlMighty();
        LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString("@Title"));

        //UnitaleUtil.AddKeysToMapCorrespondanceList();

        GlobalControls.modDev = true;
        //SceneManager.LoadScene("ModSelect");

        StaticInits.MODFOLDER = modtoload;

        List<string> encounters = new List<string>();
        DirectoryInfo di = new DirectoryInfo(Path.Combine(FileLoader.ModDataPath, "Lua/Encounters"));
        foreach (FileInfo f in di.GetFiles("*.lua")) {
            encounters.Add(Path.GetFileNameWithoutExtension(f.Name));
        }

        StaticInits.ENCOUNTER = encounters[0];

        StaticInits.Initialized = false;
        StaticInits.InitAll();
        Debug.Log("Loading " + StaticInits.ENCOUNTER);
        GlobalControls.isInFight = true;
		// Update Discord Rich Presence
		DiscordControls.StartMod(modtoload);
        SceneManager.LoadScene("Battle");

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
