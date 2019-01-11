using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System;
using System.Collections;

/// <summary>
/// Attached to the disclaimer screen so you can skip it.
/// </summary>
public class DisclaimerScript : MonoBehaviour {
    private static bool initial = false;
    
    private void Start() {
        if (!initial) {
            StaticInits.Start();
            SaveLoad.Start();
            new ControlPanel();
            new PlayerCharacter();
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR
                if (GlobalControls.crate)  Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
                else                       Misc.WindowName = ControlPanel.instance.WindowBasisName;
            #endif
            SaveLoad.LoadAlMighty();
            LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString("@Title"));
            
            UnitaleUtil.AddKeysToMapCorrespondanceList();
            
            initial = true;
        }
        
        if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null) 
            if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean) {
                GameObject.Find("Image").GetComponent<Image>().enabled = false;
                GameObject.Find("Image (1)").GetComponent<Image>().enabled = true;
                /*GameObject.Find("Description").GetComponent<Text>().text = "CRATE YOUR FRISK IS A FERE AND SUPER KEWL EJNINE!!!!1!!\n" +
                                                                           "GO ON WWW.REDIDT.CMO/R/UNITLAE. FOR UPDTAES!!!!!\n" +
                                                                           "NO RELESLING HERE!!! IT'S RFEE!!!\n" +
                                                                           "OR TUBY FEX WILL BE ANGER!!!\n\n" +
                                                                           "U'LL HVAE A BED TMIE!!!";
                GameObject.Find("Description (1)").GetComponent<Text>().text = "NU FEAUTRES IN EXAMPLES MODS!!!!! CHEKC IT OTU!!!!!\n" +
                                                                               "REALLY!!!\n" +
                                                                               "IF U DAD A # IN AN ECNOUNTRE NAME IT'LL NTO BE CHOSE NI\n" +
                                                                               "ENCONUTERS ON THE PAMS!!!! SO COLO!!!";*/
                GameObject.Find("Description").GetComponent<Text>().text = "GO TO /R/UNITLAE. FOR UPDTAES!!!!!";
                GameObject.Find("Description (1)").GetComponent<Text>().text = "NO RELESLING HERE!!! IT'S RFEE!!! " +
                                                                               "OR TUBY FEX WILL BE ANGER!!! " +
                                                                               "U'LL HVAE A BED TMIE!!!";
                GameObject.Find("Description (2)").GetComponent<Text>().text = "SPACE OR KLIK TO\n<color='#ff0000'>PALY MODS!!!!!</color>";
                GameObject.Find("Description (3)").GetComponent<Text>().text = "PRSES O TO\n<color='#ffff00'>OOVERWURL!!!!!</color>";
                GameObject.Find("Description (4)").GetComponent<Text>().text = "<b><color='red'>KNOW YUOR CODE</color> R U'LL HVAE A BED TMIE!!!</b>";
            }
    }

    /// <summary>
    /// Checks if you pressed one of the things the disclaimer tells you to. It's pretty straightforward.
    /// </summary>
    private void Update() {
        // try to hook on to the game window when the user interacts
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.O)
              || (Input.GetKeyDown(KeyCode.F4)        // F4
              || (Input.GetKeyDown(KeyCode.Return)
              &&(Input.GetKey(KeyCode.LeftAlt)        // LAlt  + Enter
              || Input.GetKey(KeyCode.RightAlt)))))   // RAlt  + Enter
                Misc.RetargetWindow();
        #endif
        
        if (Input.GetKeyDown(KeyCode.O)) {
            StaticInits.MODFOLDER = StaticInits.EDITOR_MODFOLDER;
            StaticInits.Initialized = false;
            StaticInits.InitAll();
            GlobalControls.modDev = false;
            SceneManager.LoadScene("Intro");
            Destroy(this);
        } else if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)) {
            /*GlobalControls.modDev = true;
            SceneManager.LoadScene("ModSelect");*/
            StartCoroutine(ModSelect());
        }
    }
    
    // The mod select screen can take some extra time to load,
    // because it now searches for encounter files on top of mods.
    // To compensate, this function will add "Loading" text to the Disclaimer screen
    // whenever it's time to go to the mod select menu.
    IEnumerator ModSelect() {
        // if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null && LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean)
        if (GlobalControls.crate)
            GameObject.Find("Description (4)").GetComponent<Text>().text = "LAODING MODS!!!!!";
        else
            GameObject.Find("Description (4)").GetComponent<Text>().text = "Loading mods...";
        yield return new WaitForEndOfFrame();
        GlobalControls.modDev = true;
        SceneManager.LoadScene("ModSelect");
    }
}