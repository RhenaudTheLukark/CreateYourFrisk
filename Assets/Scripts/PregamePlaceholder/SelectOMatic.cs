using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectOMatic : MonoBehaviour {
    private static int CurrentSelectedMod = 0;
    private static List<DirectoryInfo> modDirs;
    private static Dictionary<string, Sprite> bgs = new Dictionary<string, Sprite>();
    private GameObject encounterBox;
    private GameObject devMod;
    private GameObject btnList;
    private bool animationDone = true;
    private float animationTimer = 0;
    
    private static float modListScroll = 0.0f; // used to keep track of the position of the mod list specifically. resets if you press escape
    private static float encounterListScroll = 0.0f; // used to keep track of the position of the encounter list. resets if you press escape
    
    // used to fade the "Exit" button in and out
    private float ExitButtonAlpha = 5f; 
    
    // used to prevent the player from erasing real/almighty globals by accident
    private int RealGlobalCooldown = 0;
    private int AlMightyGlobalCooldown = 0;
    
    // Use this for initialization
    private void Start() {
        GameObject.Destroy(GameObject.Find("Player"));
        GameObject.Destroy(GameObject.Find("Main Camera OW"));
        GameObject.Destroy(GameObject.Find("Canvas OW"));

        // load directory info
        DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods"));
        var modDirsTemp = di.GetDirectories();
        
        // remove mods with 0 encounters and hidden mods from the list
        List<DirectoryInfo> purged = new List<DirectoryInfo>();
        foreach (DirectoryInfo modDir in modDirsTemp) {
            
            // make sure the Encounters folder exists
            if (!(new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods/" + modDir.Name + "/Lua/Encounters"))).Exists)
                continue;
            
            // count encounters
            bool hasEncounters = false;
            foreach(FileInfo file in new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods/" + modDir.Name + "/Lua/Encounters")).GetFiles("*.lua")) {
                hasEncounters = true;
                break;
            }
            
            if (/*modDir.Name != "0.5.0_SEE_CRATE" && modDir.Name != "Title" && */hasEncounters && (modDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && !modDir.Name.StartsWith("@"))
                purged.Add(modDir);
        }
        modDirs = purged;
        
        // make sure that there is at least one playable mod present
        if (purged.Count == 0) {
            GlobalControls.modDev = false;
            UnitaleUtil.DisplayLuaError("loading", "<b>Your mod folder is empty!</b>\nYou need at least 1 playable mod to use the Mod Selector.\n\n"
                + "Remember:\n1. Mods whose names start with \"@\" do not count\n2. Folders without encounter files do not count");
            return;
        }
        
        modDirs.Sort(delegate(DirectoryInfo a, DirectoryInfo b) {
            return a.Name.CompareTo(b.Name);
        });
        
        // bind button functions
        GameObject.Find("BtnBack").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("BtnBack").GetComponent<Button>().onClick.AddListener(() => {
            if (animationDone) {
                modFolderSelection();
                ScrollMods(-1);
            }
            });
        GameObject.Find("BtnNext").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("BtnNext").GetComponent<Button>().onClick.AddListener(() => {
            if (animationDone) {
                modFolderSelection();
                ScrollMods( 1);
            }
            });
        
        // grab the encounter selection box
        if (encounterBox == null)
            encounterBox = GameObject.Find("ScrollWin");
        // grab the devMod box
        if (devMod == null)
            devMod = GameObject.Find("devMod");
        // grab the mod list button, and give it a function
        if (btnList == null)
            btnList = GameObject.Find("BtnList");
        btnList.GetComponent<Button>().onClick.RemoveAllListeners();
        btnList.GetComponent<Button>().onClick.AddListener(() => {
            if (animationDone)
                modFolderMiniMenu();
            });
        // grab the exit button, and give it some functions
        GameObject.Find("BtnExit").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("BtnExit").GetComponent<Button>().onClick.AddListener(() => {SceneManager.LoadScene("Disclaimer");});
        
        // add devMod button functions
        if (GlobalControls.modDev) {
            // clear prior button functions just in case
            devMod.transform.Find("ResetRG").GetComponent<Button>().onClick.RemoveAllListeners();
            devMod.transform.Find("ResetAG").GetComponent<Button>().onClick.RemoveAllListeners();
            devMod.transform.Find("Safe").GetComponent<Button>().onClick.RemoveAllListeners();
            devMod.transform.Find("Retro").GetComponent<Button>().onClick.RemoveAllListeners();
            
            // reset RealGlobals
            devMod.transform.Find("ResetRG").GetComponent<Button>().onClick.AddListener(() => {
                if (RealGlobalCooldown > 0) {
                    LuaScriptBinder.ClearVariables();
                    if (!GlobalControls.crate)
                        devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text =     "RealGlobals Erased!";
                    else
                        devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text ="REELGOLBELZ\nDELEET!!!!!";
                } else {
                    RealGlobalCooldown = 60 * 2;
                    if (!GlobalControls.crate)
                        devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text = "Are you sure?";
                    else
                        devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text = "R U SUR???";
                }
            });
            
            // reset AlMighties
            devMod.transform.Find("ResetAG").GetComponent<Button>().onClick.AddListener(() => {
                if (AlMightyGlobalCooldown > 0) {
                    LuaScriptBinder.ClearVariables();
                    if (!GlobalControls.crate)
                        devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text =        "AlMighty Erased!";
                    else
                        devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text =  "ALMEIGHTIZ DELEET!!!!!";
                } else {
                    AlMightyGlobalCooldown = 60 * 2;
                    if (!GlobalControls.crate)
                        devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text = "Are you sure?";
                    else
                        devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text = "R U SUR???";
                }
            });
            
            // toggle safe mode
            devMod.transform.Find("Safe").GetComponent<Button>().onClick.AddListener(() => {
                ControlPanel.instance.Safe = !ControlPanel.instance.Safe;
                if (!GlobalControls.crate) {
                    if (ControlPanel.instance.Safe)
                        devMod.transform.Find("Safe").GetComponentInChildren<Text>().text = "Safe mode: On";
                    else
                        devMod.transform.Find("Safe").GetComponentInChildren<Text>().text = "Safe mode: Off";
                } else {
                    if (ControlPanel.instance.Safe)
                        devMod.transform.Find("Safe").GetComponentInChildren<Text>().text = "SFAE MDOE: ON";
                    else
                        devMod.transform.Find("Safe").GetComponentInChildren<Text>().text = "SFAE MDOE: OFF";
                }
            });
            ControlPanel.instance.Safe = !ControlPanel.instance.Safe;
            devMod.transform.Find("Safe").GetComponent<Button>().onClick.Invoke();
            
            // toggle retrocompatibility mode
            devMod.transform.Find("Retro").GetComponent<Button>().onClick.AddListener(() => {
                GlobalControls.retroMode =!GlobalControls.retroMode;
                if (!GlobalControls.crate) {
                    if (GlobalControls.retroMode)
                        devMod.transform.Find("Retro").GetComponentInChildren<Text>().text = "Compatibility: On";
                    else
                        devMod.transform.Find("Retro").GetComponentInChildren<Text>().text = "Compatibility: Off";
                } else {
                    if (GlobalControls.retroMode)
                        devMod.transform.Find("Retro").GetComponentInChildren<Text>().text = "CMOAPTIILBIYT: ON";
                    else
                        devMod.transform.Find("Retro").GetComponentInChildren<Text>().text = "CMOAPTIILBIYT: OFF";
                }
            });
            GlobalControls.retroMode =!GlobalControls.retroMode;
            devMod.transform.Find("Retro").GetComponent<Button>().onClick.Invoke();
        }
        
        // just for debugging, remove later.
        // GlobalControls.crate = true;
        
        // Crate Your Frisk initializer
        if (GlobalControls.crate) {
            // devMod buttons
            if (GlobalControls.modDev) {
                devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text =     "RSETE RAELGLOBALS";
                devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text =     "RESET ALIMGHTY";
                devMod.transform.Find("Safe").GetComponentInChildren<Text>().text =        "SFAE MDOE: OFF";
                devMod.transform.Find("Retro").GetComponentInChildren<Text>().text =       "CMOAPTIILBIYT: OFF";
            }
            
            // back button
            GameObject back = encounterBox.transform.Find("ScrollCutoff/Content/Back").gameObject;
            back.transform.Find("Text").GetComponent<Text>().text = "BCAK";
            
            // mod list button
            btnList.transform.Find("Label").gameObject.GetComponent<Text>().text = "MDO LITS";
            btnList.transform.Find("LabelShadow").gameObject.GetComponent<Text>().text = "MDO LITS";
        }
        
        // this check will be true if we just exited out of an encounter
        // if that's the case, we want to open the encounter list so the user only has to click once to re enter
        modFolderSelection();
        if (StaticInits.ENCOUNTER != "") {
            // check to see if there is more than one encounter in the mod just exited from
            List<string> encounters = new List<string>();
            DirectoryInfo di2 = new DirectoryInfo(System.IO.Path.Combine(FileLoader.ModDataPath, "Lua/Encounters"));
            foreach (FileInfo f in di2.GetFiles("*.lua")) {
                if (encounters.Count < 2)
                    encounters.Add(Path.GetFileNameWithoutExtension(f.Name));
            }
            
            if (encounters.Count > 1)
                encounterSelection();
            
            // move the scrolly bit to where it was when the player entered the encounter
            encounterBox.transform.Find("ScrollCutoff/Content").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, encounterListScroll);
            
            // start the Exit button at half transparency
            ExitButtonAlpha = 0.5f;
            GameObject.Find("BtnExit/Text").gameObject.GetComponent<Text>().color       = new Color(1f, 1f, 1f, 0.5f);
            GameObject.Find("BtnExit/TextShadow").gameObject.GetComponent<Text>().color = new Color(0f, 0f, 0f, 0.5f);
            
            // reset it to let us accurately tell if the player just came here from the Disclaimer scene or the Battle scene
            StaticInits.ENCOUNTER = "";
        // player is coming here from the Disclaimer scene
        } else {
            // when the player enters from the Disclaimer screen, reset stored scroll positions
            modListScroll = 0.0f;
            encounterListScroll = 0.0f;
        }
    }
    
    // A special function used specifically for error handling
    // It re-generates the mod list, and selects the first mod
    // Used for cases where the player selects a mod that no longer exists
    private void HandleErrors() {
        Debug.Log("Error detected! Mod not found! Resetting mod list...");
        CurrentSelectedMod = 0;
        bgs = new Dictionary<string, Sprite>();
        Start();
    }
    
    IEnumerator LaunchMod() {
        // first: make sure the mod is still here and can be opened
        if (!(new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods/" + modDirs[CurrentSelectedMod].Name + "/Lua/Encounters/"))).Exists) {
            HandleErrors();
            yield break;
        }
        
        // dim the background to indicate loading
        GameObject.Find("ModBackground").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1875f);
        
        // store the current position of the scrolly bit
        encounterListScroll = encounterBox.transform.Find("ScrollCutoff/Content").gameObject.GetComponent<RectTransform>().anchoredPosition.y;
        
        yield return new WaitForEndOfFrame();
        /*int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        GlobalControls.texBeforeEncounter = tex;*/
        //byte[] bytes = tex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/ItsAVeryHackyWayToMakeTransitionsIKnowThanksYouCanDeleteThisFileIfYouWantTo.png", bytes);
        StaticInits.Initialized = false;
        StaticInits.InitAll();
        Debug.Log("Loading " + StaticInits.ENCOUNTER);
        GlobalControls.isInFight = true;
        SceneManager.LoadScene("Battle");
    }
    
    // Shows a mod's "page".
    private void ShowMod(int id) {
        // error handler
        // if current index is now out of range OR currently selected mod is not present:
        if (CurrentSelectedMod < 0 || CurrentSelectedMod > modDirs.Count - 1
            || !(new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods/" + modDirs[CurrentSelectedMod].Name + "/Lua/Encounters"))).Exists
            ||  (new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods/" + modDirs[CurrentSelectedMod].Name + "/Lua/Encounters"))).GetFiles("*.lua").Length == 0) {
            HandleErrors();
            return;
        }
        
        // Update currently selected mod folder
        StaticInits.MODFOLDER = modDirs[id].Name;
        
        // make clicking the background go to the encounter select screen
        GameObject.Find("ModBackground").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ModBackground").GetComponent<Button>().onClick.AddListener(() => {
            if (animationDone)
                    encounterSelection();
            });
        
        // Update the background
        var ImgComp = GameObject.Find("ModBackground").GetComponent<Image>();
        // first check if we already have this mod's background loaded in memory
        if (bgs.ContainsKey(modDirs[CurrentSelectedMod].Name)) {
            ImgComp.sprite = bgs[modDirs[CurrentSelectedMod].Name];
        } else {
            // if not, find it and store it
            try {
                Sprite thumbnail = SpriteUtil.FromFile(FileLoader.pathToModFile("Sprites/preview.png"));
                ImgComp.sprite = thumbnail;
            } catch {
                try {
                    Sprite bg = SpriteUtil.FromFile(FileLoader.pathToModFile("Sprites/bg.png"));
                    ImgComp.sprite = bg;
                } catch {
                    ImgComp.sprite = SpriteUtil.FromFile("Sprites/black.png");
                }
            }
            bgs.Add(modDirs[CurrentSelectedMod].Name, ImgComp.sprite);
        }
        
        // Get all encounters in the mod's Encounters folder
        List<string> encounters = new List<string>();
        DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(FileLoader.ModDataPath, "Lua/Encounters"));
        foreach (FileInfo f in di.GetFiles("*.lua")) {
            encounters.Add(Path.GetFileNameWithoutExtension(f.Name));
        }
        
        // Update the text
        GameObject.Find("ModTitle").GetComponent<Text>().text = modDirs[id].Name;
        // crate your frisk version
        if (GlobalControls.crate)
            GameObject.Find("ModTitle").GetComponent<Text>().text = Temmify.Convert(modDirs[id].Name, true);
        GameObject.Find("ModTitleShadow").GetComponent<Text>().text = GameObject.Find("ModTitle").GetComponent<Text>().text;
        
        // list # encounters, or name of encounter if only one
        if (encounters.Count == 1) {
            GameObject.Find("EncounterCount").GetComponent<Text>().text = encounters[0];
            // crate your frisk version
            if (GlobalControls.crate)
                GameObject.Find("EncounterCount").GetComponent<Text>().text = Temmify.Convert(encounters[0], true);
            
            // make clicking the bg directly open the encounter
            GameObject.Find("ModBackground").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("ModBackground").GetComponent<Button>().onClick.AddListener(() => {
                if (animationDone) {
                    StaticInits.ENCOUNTER = encounters[0];
                    StartCoroutine(LaunchMod());
                }
                });
        } else {
            GameObject.Find("EncounterCount").GetComponent<Text>().text = "Has " + encounters.Count + " encounters";
            // crate your frisk version
            if (GlobalControls.crate)
                GameObject.Find("EncounterCount").GetComponent<Text>().text = "HSA " + encounters.Count + " ENCUOTNERS";
        }
        GameObject.Find("EncounterCountShadow").GetComponent<Text>().text = GameObject.Find("EncounterCount").GetComponent<Text>().text;
        
        // Update the color of the arrows
        if (CurrentSelectedMod == 0)
            GameObject.Find("BtnBack").transform.Find("Text").gameObject.GetComponent<Text>().color = new Color(0.25f, 0.25f, 0.25f, 1f);
        else
            GameObject.Find("BtnBack").transform.Find("Text").gameObject.GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
        if (CurrentSelectedMod == modDirs.Count - 1)
            GameObject.Find("BtnNext").transform.Find("Text").gameObject.GetComponent<Text>().color = new Color(0.25f, 0.25f, 0.25f, 1f);
        else
            GameObject.Find("BtnNext").transform.Find("Text").gameObject.GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
    }
    
    // Goes to the next or previous mod with a little scrolling animation.
    // -1 for left, 1 for right
    private void ScrollMods(int dir) {
        // first, determine if the next mod should be shown
        bool animate = false;
        if ((dir == -1 && CurrentSelectedMod > 0) || (dir == 1 && CurrentSelectedMod < modDirs.Count - 1)) {
            // show the new mod
            animate = true;
        }
        
        // if the new mod is being shown, start the animation!
        if (animate) {
            animationTimer = dir / 10f;
            animationDone = false;
            
            // create a BG for the previous mod
            GameObject OldBG = Instantiate(GameObject.Find("ModBackground"), GameObject.Find("Canvas").transform);
            OldBG.name = "ANIM ModBackground";
            OldBG.GetComponent<Button>().onClick.RemoveAllListeners();
            
            // create a new mod title
            GameObject OldTitleShadow = Instantiate(GameObject.Find("ModTitleShadow"), GameObject.Find("Canvas").transform);
            OldTitleShadow.name = "ANIM ModTitleShadow";
            GameObject OldTitle = Instantiate(GameObject.Find("ModTitle"), GameObject.Find("Canvas").transform);
            OldTitle.name = "ANIM ModTitle";
            
            // create a new encounter count label
            GameObject OldCountShadow = Instantiate(GameObject.Find("EncounterCountShadow"), GameObject.Find("Canvas").transform);
            OldCountShadow.name = "ANIM EncounterCountShadow";
            GameObject OldCount = Instantiate(GameObject.Find("EncounterCount"), GameObject.Find("Canvas").transform);
            OldCount.name = "ANIM EncounterCount";
            
            // properly layer all "fake" assets
            OldCount.transform.SetAsFirstSibling();
            OldCountShadow.transform.SetAsFirstSibling();
            OldTitle.transform.SetAsFirstSibling();
            OldTitleShadow.transform.SetAsFirstSibling();
            OldBG.transform.SetAsFirstSibling();
            
            // move all real assets to the side
            GameObject.Find("ModBackground").transform.Translate(640 * dir, 0, 0);
            GameObject.Find("ModTitleShadow").transform.Translate(640 * dir, 0, 0);
            GameObject.Find("ModTitle").transform.Translate(640 * dir, 0, 0);
            GameObject.Find("EncounterCountShadow").transform.Translate(640 * dir, 0, 0);
            GameObject.Find("EncounterCount").transform.Translate(640 * dir, 0, 0);
            
            // actually choose the new mod
            CurrentSelectedMod += dir;
            ShowMod(CurrentSelectedMod);
        }
    }
    
    // Used to animate scrolling left or right.
    private void Update() {
        // Animation updating section
        if (GameObject.Find("ANIM ModBackground") != null) {
            if (animationTimer > 0)
                animationTimer = Mathf.Floor((animationTimer + 1));
            else
                animationTimer = Mathf.Ceil ((animationTimer - 1));
            
            int distance = (int)(((20 - Mathf.Abs(animationTimer)) * 3.4) * -Mathf.Sign(animationTimer));
            
            GameObject.Find("ANIM ModBackground")       .transform.Translate(distance, 0, 0);
            GameObject.Find("ANIM ModTitleShadow")      .transform.Translate(distance, 0, 0);
            GameObject.Find("ANIM ModTitle")            .transform.Translate(distance, 0, 0);
            GameObject.Find("ANIM EncounterCountShadow").transform.Translate(distance, 0, 0);
            GameObject.Find("ANIM EncounterCount")      .transform.Translate(distance, 0, 0);
            
            GameObject.Find("ModBackground")            .transform.Translate(distance, 0, 0);
            GameObject.Find("ModTitleShadow")           .transform.Translate(distance, 0, 0);
            GameObject.Find("ModTitle")                 .transform.Translate(distance, 0, 0);
            GameObject.Find("EncounterCountShadow")     .transform.Translate(distance, 0, 0);
            GameObject.Find("EncounterCount")           .transform.Translate(distance, 0, 0);
            
            if (Mathf.Abs(animationTimer) == 20) {
                Destroy(GameObject.Find("ANIM ModBackground"));
                Destroy(GameObject.Find("ANIM ModTitleShadow"));
                Destroy(GameObject.Find("ANIM ModTitle"));
                Destroy(GameObject.Find("ANIM EncounterCountShadow"));
                Destroy(GameObject.Find("ANIM EncounterCount"));
                
                // manual movement because I can't change the movement multiplier to a precise enough value
                GameObject.Find("ModBackground")            .transform.Translate((int)(2 * -Mathf.Sign(animationTimer)), 0, 0);
                GameObject.Find("ModTitleShadow")           .transform.Translate((int)(2 * -Mathf.Sign(animationTimer)), 0, 0);
                GameObject.Find("ModTitle")                 .transform.Translate((int)(2 * -Mathf.Sign(animationTimer)), 0, 0);
                GameObject.Find("EncounterCountShadow")     .transform.Translate((int)(2 * -Mathf.Sign(animationTimer)), 0, 0);
                GameObject.Find("EncounterCount")           .transform.Translate((int)(2 * -Mathf.Sign(animationTimer)), 0, 0);
                
                animationTimer = 0;
                animationDone = true;
            }
        }
        
        // prevent scrolling too far in the encounter box
        if (GameObject.Find("ScrollWin")) {
            GameObject content = encounterBox.transform.Find("ScrollCutoff/Content").gameObject;
            if (content.GetComponent<RectTransform>().anchoredPosition.y < -200)
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
            else if (content.GetComponent<RectTransform>().anchoredPosition.y > (content.transform.childCount - 1) * 30)
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (content.transform.childCount - 1) * 30);
        }
        
        // detect hovering over the Exit button and handle fading
        if (Input.mousePosition.x < 75 && Input.mousePosition.y > 450 && ExitButtonAlpha < 1f) {
            ExitButtonAlpha += 0.05f;
            GameObject.Find("BtnExit/Text").GetComponent<Text>().color =        new Color(1f, 1f, 1f, ExitButtonAlpha);
            GameObject.Find("BtnExit/TextShadow").GetComponent<Text>().color =  new Color(0f, 0f, 0f, ExitButtonAlpha);
        } else if (ExitButtonAlpha > 0.5f) {
            ExitButtonAlpha -= 0.05f;
            GameObject.Find("BtnExit/Text").GetComponent<Text>().color =        new Color(1f, 1f, 1f, ExitButtonAlpha);
            GameObject.Find("BtnExit/TextShadow").GetComponent<Text>().color =  new Color(0f, 0f, 0f, ExitButtonAlpha);
        }
        
        // make the player click twice to reset RG or AG
        if (RealGlobalCooldown > 0)
            RealGlobalCooldown -= 1;
        else if (RealGlobalCooldown == 0) {
            RealGlobalCooldown = -1;
            if (!GlobalControls.crate)
                devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text =    "Reset RealGlobals";
            else
                devMod.transform.Find("ResetRG").GetComponentInChildren<Text>().text =     "RSETE RAELGLOBALS";
        }
        
        if (AlMightyGlobalCooldown > 0) {
            AlMightyGlobalCooldown -= 1;
        } else if (AlMightyGlobalCooldown == 0) {
            AlMightyGlobalCooldown = -1;
            if (!GlobalControls.crate)
                devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text =     "Reset AlMighty";
            else
                devMod.transform.Find("ResetAG").GetComponentInChildren<Text>().text =     "RESET ALIMGHTY";
        }
    }
    
    // Shows the "mod page" screen.
    private void modFolderSelection() {
        UnitaleUtil.printDebuggerBeforeInit = "";
        ShowMod(CurrentSelectedMod);
        
        // hide the 4 buttons if needed
        if (!GlobalControls.modDev) {
            devMod.SetActive(false);
        }
        
        // show the mod list button
        btnList.SetActive(true);
        
        // if the encounter box is visible, remove all encounter buttons before hiding
        // use try because I can't check if a gameobject is active for some reason
        try {
            Transform content = encounterBox.transform.Find("ScrollCutoff/Content");
            foreach (Transform b in content) {
                if (b.gameObject.name != "Back") {
                    Destroy(b.gameObject);
                }
            }
        } catch {
            // do nothing
        }
        // hide the encounter selection box
        encounterBox.SetActive(false);
    }

    private void encounterSelection() {
        // hide the mod list button
        btnList.SetActive(false);
        
        // make clicking the background exit the encounter selection screen
        GameObject.Find("ModBackground").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ModBackground").GetComponent<Button>().onClick.AddListener(() => {
            if (animationDone)
                modFolderSelection();
            });
        // show the encounter selection box
        encounterBox.SetActive(true);
        // reset the encounter box's position
        encounterBox.transform.Find("ScrollCutoff/Content").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        
        // grab pre-existing objects
        GameObject content = encounterBox.transform.Find("ScrollCutoff/Content").gameObject;
        // give the back button its function
        GameObject back = content.transform.Find("Back").gameObject;
        back.GetComponent<Button>().onClick.RemoveAllListeners();
        back.GetComponent<Button>().onClick.AddListener(() => {modFolderSelection();});
        
        DirectoryInfo di = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods/" + StaticInits.MODFOLDER + "/Lua/Encounters"));
        if (di.Exists && di.GetFiles().Length > 0) {
            FileInfo[] encounterFiles = di.GetFiles("*.lua");
            
            int count = 0;
            foreach (FileInfo encounter in encounterFiles) {
                count += 1;
                
                // create a button for each encounter file
                GameObject button = Instantiate(back);
                // set parent and name
                button.transform.SetParent(content.transform);
                button.name = "EncounterButton";
                // set position
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100 - (count * 30));
                // set color
                button.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
                button.transform.Find("Fill").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                // set text
                button.transform.Find("Text").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(encounter.Name);
                if (GlobalControls.crate)
                    button.transform.Find("Text").GetComponent<Text>().text = Temmify.Convert(Path.GetFileNameWithoutExtension(encounter.Name), true);
                // finally, set function!
                string filename = Path.GetFileNameWithoutExtension(encounter.Name);
                
                button.GetComponent<Button>().onClick.RemoveAllListeners();
                button.GetComponent<Button>().onClick.AddListener(() => {
                    StaticInits.ENCOUNTER = filename;
                    StartCoroutine(LaunchMod());
                });
            }
        }
    }
    
    // opens the scrolling interface and lets the user browse their mods
    private void modFolderMiniMenu() {
        // hide the mod list button
        btnList.SetActive(false);
        
        // grab pre-existing objects
        GameObject content = encounterBox.transform.Find("ScrollCutoff/Content").gameObject;
        // give the back button its function
        GameObject back = content.transform.Find("Back").gameObject;
        back.GetComponent<Button>().onClick.RemoveAllListeners();
        back.GetComponent<Button>().onClick.AddListener(() => {
            // reset the encounter box's position
            modListScroll = 0.0f;
            
            modFolderSelection();
            });
        
        // make clicking the background exit this menu
        GameObject.Find("ModBackground").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ModBackground").GetComponent<Button>().onClick.AddListener(() => {
            if (animationDone) {
                // store the encounter box's position so it can be remembered upon exiting a mod
                modListScroll = content.GetComponent<RectTransform>().anchoredPosition.y;
                
                modFolderSelection();
            }
            });
        // show the encounter selection box
        encounterBox.SetActive(true);
        // move the encounter box to the stored position, for easier mod browsing
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, modListScroll);
        
        int count = -1;
        foreach (DirectoryInfo mod in modDirs) {
            count += 1;
            
            // create a button for each mod
            GameObject button = Instantiate(back);
            // set parent and name
            button.transform.SetParent(content.transform);
            button.name = "ModButton";
            // set position
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100 - ((count + 1) * 30));
            // set color
            button.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            button.transform.Find("Fill").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            // set text
            button.transform.Find("Text").GetComponent<Text>().text = mod.Name;
            if (GlobalControls.crate)
                button.transform.Find("Text").GetComponent<Text>().text = Temmify.Convert(mod.Name, true);
            // finally, set function!
            int tempCount = count;
            
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => {
                // store the encounter box's position so it can be remembered upon exiting a mod
                modListScroll = content.GetComponent<RectTransform>().anchoredPosition.y;
                
                CurrentSelectedMod = tempCount;
                modFolderSelection();
                ShowMod(CurrentSelectedMod);
            });
        }
    }
}
