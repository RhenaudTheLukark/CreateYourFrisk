using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System;

/// <summary>
/// Attached to the disclaimer screen so you can skip it.
/// </summary>
public class DisclaimerScript : MonoBehaviour {
    private void Start() {
        SaveLoad.Start();
        new ControlPanel();
        new PlayerCharacter();
        GlobalControls.misc = new Misc();
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR
            if (GlobalControls.crate)  Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
            else                       Misc.WindowName = ControlPanel.instance.WindowBasisName;
        #endif
        SaveLoad.LoadAlMighty();
        LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString("Title"));
        if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null) 
            if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean) {
                GameObject.Find("Image").GetComponent<Image>().enabled = false;
                GameObject.Find("Image (1)").GetComponent<Image>().enabled = true;
                GameObject.Find("Description").GetComponent<Text>().text = "CRATE YOUR FRISK IS A FERE AND SUPER KEWL EJNINE!!!!1!!\n" +
                                                                           "GO ON WWW.REDIDT.CMO/R/UNITLAE. FOR UPDTAES!!!!!\n" +
                                                                           "NO RELESLING HERE!!! IT'S RFEE!!!\n" +
                                                                           "OR TUBY FEX WILL BE ANGER!!!\n\n" +
                                                                           "U'LL HVAE A BED TMIE!!!";
                GameObject.Find("Description (1)").GetComponent<Text>().text = "NU FEAUTRES IN EXAMPLES MODS!!!!! CHEKC IT OTU!!!!!\n" +
                                                                               "REALLY!!!\n" +
                                                                               "IF U DAD A # IN AN ECNOUNTRE NAME IT'LL NTO BE CHOSE NI\n" +
                                                                               "ENCONUTERS ON THE PAMS!!!! SO COLO!!!";
                GameObject.Find("Description (2)").GetComponent<Text>().text = "SPACE OR KLIK TO\nTSET MODEDV MODE!!!!!";
                GameObject.Find("Description (3)").GetComponent<Text>().text = "PRSES O TO\nOOVERWURL!!!!!";
            }
        UnitaleUtil.AddKeysToMapCorrespondanceList();
    }

    /// <summary>
    /// Checks if you pressed one of the things the disclaimer tells you to. It's pretty straightforward.
    /// </summary>
    private void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            GameObject.DontDestroyOnLoad(GameObject.Find("Main Camera"));
            SceneManager.LoadScene("Intro");
            Destroy(this);
        } else if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) {
            GlobalControls.modDev = true;
            SceneManager.LoadScene("ModSelect");
        }
    }
}