using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Attached to the disclaimer screen so you can skip it.
/// </summary>
public class DisclaimerScript : MonoBehaviour {
    public GameObject Logo, LogoCrate, RedditPlug, LegalStuff, ModSelection, Overworld, LuaKnowledgeDisclaimer, Version;

    private void Start() {
        if (GlobalControls.crate) {
            Logo.GetComponent<Image>().enabled = false;
            LogoCrate.GetComponent<Image>().enabled = true;
            RedditPlug.GetComponent<Text>().text = "GO TO /R/UNITLAE. FOR UPDTAES!!!!!";
            LegalStuff.GetComponent<Text>().text = "NO RELESLING HERE!!! IT'S RFEE!!! OR TUBY FEX WILL BE ANGER!!! U'LL HVAE A BED TMIE!!!";
            ModSelection.GetComponent<Text>().text = "SPACE OR KLIK TO\n<color='#ff0000'>PALY MODS!!!!!</color>";
            Overworld.GetComponent<Text>().text = "PRSES O TO\n<color='#ffff00'>OOVERWURL!!!!!</color>";
            LuaKnowledgeDisclaimer.GetComponent<Text>().text = "<b><color='red'>KNOW YUOR CODE</color> R U'LL HVAE A BED TMIE!!!</b>";
            Version.GetComponent<Text>().text = "v" + Random.Range(0,9) + "." + Random.Range(0,9) + "." + Random.Range(0,9);
        } else if (Random.Range(0, 1000) == 021) {
            Logo.GetComponent<Image>().enabled              = false;
            Version.GetComponent<Transform>().localPosition = new Vector3(0f, 160f, 0f);
            Version.GetComponent<Text>().color              = new Color(1f, 1f, 1f, 1f);
            Version.GetComponent<Text>().text               = "Not Unitale v0.2.1a";
        } else if (GlobalControls.BetaVersion > 0)
            Version.GetComponent<Text>().text = "v" + GlobalControls.CYFversion + "\nLTS " + (GlobalControls.LTSversion + 1) + "\n<color=\"#00ff00\">b" + GlobalControls.BetaVersion + "</color>";
        else
            Version.GetComponent<Text>().text = "v" + GlobalControls.CYFversion + "\nLTS " + GlobalControls.LTSversion;
    }

    /// <summary>
    /// Checks if you pressed one of the things the disclaimer tells you to. It's pretty straightforward.
    /// </summary>
    private void Update() {
        if (!ScreenResolution.hasInitialized) return;
        if (GlobalControls.input.Menu == ButtonState.PRESSED) {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Misc.RetargetWindow();
            #endif
            StaticInits.InitAll(StaticInits.EDITOR_MODFOLDER);
            GlobalControls.modDev = false;
            SceneManager.LoadScene("Intro");
            Destroy(this);
        } else if (GlobalControls.input.Confirm == ButtonState.PRESSED || Input.GetMouseButtonDown(0)) {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Misc.RetargetWindow();
            #endif
            StartCoroutine(ModSelect());
        }
    }

    // The mod select screen can take some extra time to load,
    // because it now searches for encounter files on top of mods.
    // To compensate, this function will add "Loading" text to the Disclaimer screen
    // whenever it's time to go to the mod select menu.
    private IEnumerator ModSelect() {
        LuaKnowledgeDisclaimer.GetComponent<Text>().text = GlobalControls.crate ? "LAODING MODS!!!!!" : "Loading mods...";
        yield return new WaitForEndOfFrame();
        GlobalControls.modDev = true;
        DiscordControls.StartModSelect(false);
        SceneManager.LoadScene("ModSelect");
    }
}