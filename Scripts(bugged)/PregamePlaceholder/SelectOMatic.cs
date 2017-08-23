using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectOMatic : MonoBehaviour {
    private static Button b;
    private static Button bs;
    public bool yay = false;
    public SelectionTarget target;

    public enum SelectionTarget { MODFOLDER, ENCOUNTER };

    // Use this for initialization
    private void Start() {
        GameObject.Destroy(GameObject.Find("Player"));
        GameObject.Destroy(GameObject.Find("Main Camera OW"));
        GameObject.Destroy(GameObject.Find("Canvas OW"));
        Button _b = Resources.Load<Button>("Prefabs/ModButtonNew");
        if (_b != null && _b.ToString().ToLower() != "null")
            b = _b;
        Button _bs = Resources.Load<Button>("Prefabs/SpeButton");
        if (_bs != null && _bs.ToString().ToLower() != "null")
            bs = _bs;

        if      (target == SelectionTarget.MODFOLDER)  modFolderSelection();
        else if (target == SelectionTarget.ENCOUNTER)  encounterSelection();
    }

    IEnumerator LaunchMod() {
        yield return new WaitForEndOfFrame();
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        GlobalControls.texBeforeEncounter = tex;
        //byte[] bytes = tex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/ItsAVeryHackyWayToMakeTransitionsIKnowThanksYouCanDeleteThisFileIfYouWantTo.png", bytes);
        StaticInits.Initialized = false;
        StaticInits.InitAll();
        Debug.Log("Loading " + StaticInits.ENCOUNTER);
        GlobalControls.isInFight = true;
        SceneManager.LoadScene("Battle");
    }

    /// <summary>
    /// Makes the buttons for the mod selection screen.
    /// </summary>
    private void modFolderSelection() {
        if (GlobalControls.crate)
            GameObject.Find("Text").GetComponent<Text>().text = "MDO SELECTRO (CILCK + DARG TO SEE OTRHE MODS)";
        DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods"));
        DirectoryInfo[] modDirs = di.GetDirectories();
        int numButton = 0;
        foreach (DirectoryInfo modDir in modDirs) {
            if (modDir.Name == "0.5.0_SEE_CRATE")
                continue;
            Button c = Instantiate(b);
            c.transform.SetParent(GameObject.Find("Content").transform);
            RectTransform crt = c.GetComponent<RectTransform>();
            crt.anchoredPosition = new Vector2(5, 0 - 40 * numButton);
            c.GetComponentInChildren<Text>().text = modDir.Name;
            string mdn = modDir.Name; // create a new object in memory because the reference to moddir in the anonymous function gets fucked
            c.onClick.AddListener(() => { StaticInits.MODFOLDER = mdn; Debug.Log("Selecting directory " + mdn); SceneManager.LoadScene("EncounterSelect"); });
            numButton++;
        }

        if (GlobalControls.modDev) {
            Transform[] tfs = UnitaleUtil.GetFirstChildren(GameObject.Find("Canvas").transform, true);
            foreach (Transform tf in tfs)
                if (tf.gameObject.name == "devMod") {
                    tf.gameObject.SetActive(true);
                    if (GlobalControls.crate)
                        GameObject.Find("Text2").GetComponent<Text>().text = "MODEDV MODE HREE!!!\nIZI MDO TSETING!!!\nGAD LUKC!!!!!1!!";

                    Button but = Instantiate(bs), but2 = Instantiate(bs), but3 = Instantiate(bs), but4 = Instantiate(bs);

                    but.gameObject.name = "ResetRG";
                    but.transform.SetParent(GameObject.Find("Canvas").transform);
                    but.GetComponent<RectTransform>().sizeDelta = new Vector2(150, but.GetComponent<RectTransform>().sizeDelta.y);
                    but.GetComponent<RectTransform>().position = new Vector2(325, 200);
                    but.GetComponentInChildren<Text>().text = "Reset RealGlobals";
                    but.onClick.AddListener(() => {
                        LuaScriptBinder.ClearVariables();
                        if (GlobalControls.crate) GameObject.Find("Text3").GetComponent<Text>().text = "REELGOLBELZ\nDELEET!!!!!";
                        else GameObject.Find("Text3").GetComponent<Text>().text = "RealGlobals\nerased!";
                    });

                    but2.gameObject.name = "ResetAM";
                    but2.transform.SetParent(GameObject.Find("Canvas").transform);
                    but2.GetComponent<RectTransform>().sizeDelta = new Vector2(150, but2.GetComponent<RectTransform>().sizeDelta.y);
                    but2.GetComponent<RectTransform>().position = new Vector2(475, 200);
                    but2.GetComponentInChildren<Text>().text = "Reset AlMighty";
                    but2.onClick.AddListener(() => {
                        LuaScriptBinder.ClearAlMighty();
                        if (GlobalControls.crate) GameObject.Find("Text4").GetComponent<Text>().text = "ALMEIGHTIZ\nDELEET!!!!!";
                        else GameObject.Find("Text4").GetComponent<Text>().text = "AlMighty\nerased!";
                    });

                    but3.gameObject.name = "Retromode";
                    but3.transform.SetParent(GameObject.Find("Canvas").transform);
                    but3.GetComponent<RectTransform>().sizeDelta = new Vector2(300, but3.GetComponent<RectTransform>().sizeDelta.y);
                    but3.GetComponent<RectTransform>().position = new Vector2(325, 440);
                    if (GlobalControls.retroMode) but3.GetComponentInChildren<Text>().text = "0.2.1a retrocompatibility: On";
                    else but3.GetComponentInChildren<Text>().text = "0.2.1a retrocompatibility: Off";
                    but3.onClick.AddListener(() => {
                        GlobalControls.retroMode =!GlobalControls.retroMode;
                        if (GlobalControls.retroMode) but3.GetComponentInChildren<Text>().text = "0.2.1a retrocompatibility: On";
                        else but3.GetComponentInChildren<Text>().text = "0.2.1a retrocompatibility: Off";
                    });

                    but4.gameObject.name = "Safemode";
                    but4.transform.SetParent(GameObject.Find("Canvas").transform);
                    but4.GetComponent<RectTransform>().sizeDelta = new Vector2(300, but3.GetComponent<RectTransform>().sizeDelta.y);
                    but4.GetComponent<RectTransform>().position = new Vector2(325, 400);
                    if (ControlPanel.instance.Safe) but4.GetComponentInChildren<Text>().text = "Safe mode: On";
                    else but4.GetComponentInChildren<Text>().text = "Safe mode: Off";
                    but4.onClick.AddListener(() => {
                        ControlPanel.instance.Safe =!ControlPanel.instance.Safe;
                        if (ControlPanel.instance.Safe) but4.GetComponentInChildren<Text>().text = "Safe mode: On";
                        else but4.GetComponentInChildren<Text>().text = "Safe mode: Off";
                    });
                    break;
                }
        }
    }

    /// <summary>
    /// Makes the buttons for the encounter selection screen. Code duplication? I don't know what you're talking about.
    /// </summary>
    private void encounterSelection() {
        if (GlobalControls.crate)
            GameObject.Find("Text").GetComponent<Text>().text = "ENCNOUTRE SELECTRO (CILCK + DARG TO SEE OTRHE NECOUNETRS)";

        DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods/" + StaticInits.MODFOLDER + "/Lua/Encounters"));
        FileInfo[] encounterFiles = di.GetFiles();
        int numButton = 0;

        Button back = Instantiate(b);
        back.transform.SetParent(GameObject.Find("Canvas").transform);
        back.GetComponent<RectTransform>().anchoredPosition = new Vector2(325, -40);
        if (GlobalControls.crate)
            back.GetComponentInChildren<Text>().text = "BAK";
        else
            back.GetComponentInChildren<Text>().text = "Back";
        back.onClick.AddListener(() => { SceneManager.LoadScene("ModSelect"); });

        foreach (FileInfo encounterFile in encounterFiles) {
            if (!encounterFile.Name.EndsWith(".lua"))
                continue;

            Button c = Instantiate(b);
            c.transform.SetParent(GameObject.Find("Content").transform);
            RectTransform crt = c.GetComponent<RectTransform>();
            crt.anchoredPosition = new Vector2(5, 0 - 40 * numButton);
            c.GetComponentInChildren<Text>().text = encounterFile.Name;
            string efn = Path.GetFileNameWithoutExtension(encounterFile.Name); // create a new object in memory because the reference to moddir in the anonymous function gets fucked
            c.onClick.AddListener(() => {
                StaticInits.ENCOUNTER = efn;
                StartCoroutine(LaunchMod());
            });
            numButton++;
        }
    }
}