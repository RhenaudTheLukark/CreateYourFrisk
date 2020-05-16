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
    public GameObject Logo, LogoCrate, Desc1, Desc2, Desc3, Desc4, Desc5, Version;

    private void Start() {
        if (GlobalControls.crate) {
            Logo.GetComponent<Image>().enabled = false;
            LogoCrate.GetComponent<Image>().enabled = true;
            Desc1.GetComponent<Text>().text = "GO TO /R/UNITLAE. FOR UPDTAES!!!!!";
            Desc2.GetComponent<Text>().text = "NO RELESLING HERE!!! IT'S RFEE!!! OR TUBY FEX WILL BE ANGER!!! U'LL HVAE A BED TMIE!!!";
            Desc3.GetComponent<Text>().text = "SPACE OR KLIK TO\n<color='#ff0000'>PALY MODS!!!!!</color>";
            Desc4.GetComponent<Text>().text = "PRSES O TO\n<color='#ffff00'>OOVERWURL!!!!!</color>";
            Desc5.GetComponent<Text>().text = "<b><color='red'>KNOW YUOR CODE</color> R U'LL HVAE A BED TMIE!!!</b>";
            Version.GetComponent<Text>().text = "v" + UnityEngine.Random.Range(0,9) + "." + UnityEngine.Random.Range(0,9) + "." + UnityEngine.Random.Range(0,9);
        } else
            if (UnityEngine.Random.Range(0, 1000) == 021) {
                Logo.GetComponent<Image>().enabled = false;
                Version.GetComponent<Transform>().localPosition = new Vector3(0f, 160f, 0f);
                Version.GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
                Version.GetComponent<Text>().text = "Not Unitale v0.2.1a";
            } else
                Version.GetComponent<Text>().text = "v" + GlobalControls.CYFversion;
    }

    /// <summary>
    /// Checks if you pressed one of the things the disclaimer tells you to. It's pretty straightforward.
    /// </summary>
    private void Update() {
        // Try to hook on to the game window when the user interacts
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.O)
              || (Input.GetKeyDown(KeyCode.F4)        // F4
              || (Input.GetKeyDown(KeyCode.Return)
              &&(Input.GetKey(KeyCode.LeftAlt)        // LAlt  + Enter
              || Input.GetKey(KeyCode.RightAlt)))))   // RAlt  + Enter
                Misc.RetargetWindow();
        #endif

        if (ScreenResolution.hasInitialized)
            if (Input.GetKeyDown(KeyCode.O)) {
                StaticInits.MODFOLDER = StaticInits.EDITOR_MODFOLDER;
                StaticInits.Initialized = false;
                StaticInits.InitAll();
                GlobalControls.modDev = false;
                SceneManager.LoadScene("Intro");
                Destroy(this);
            } else if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                StartCoroutine(ModSelect());
    }

    // The mod select screen can take some extra time to load,
    // because it now searches for encounter files on top of mods.
    // To compensate, this function will add "Loading" text to the Disclaimer screen
    // whenever it's time to go to the mod select menu.
    IEnumerator ModSelect() {
        // if (LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null && LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk").Boolean)
        if (GlobalControls.crate)
            Desc5.GetComponent<Text>().text = "LAODING MODS!!!!!";
        else
            Desc5.GetComponent<Text>().text = "Loading mods...";
        yield return new WaitForEndOfFrame();
        GlobalControls.modDev = true;
        SceneManager.LoadScene("ModSelect");
    }
}