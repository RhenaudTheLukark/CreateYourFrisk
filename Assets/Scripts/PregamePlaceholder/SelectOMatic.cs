using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectOMatic : MonoBehaviour {
    private static int currentPageID;
    private static List<DirectoryInfo> mods, folders;
    private static List<ModPage> modPages;
    private Dictionary<string, Sprite> bgs = new Dictionary<string, Sprite>();
    private bool animationDone = true;
    private float animationTimer;
    public EventSystem eventSystem;

    private static float modListScroll;         // Used to keep track of the position of the mod list specifically. Resets if you press escape
    private static float encounterListScroll;   // Used to keep track of the position of the encounter list. Resets if you press escape

    private float ExitButtonAlpha = 5f;         // Used to fade the "Exit" button in and out
    private float OptionsButtonAlpha = 5f;      // Used to fade the "Options" button in and out

    private static int selectedItem;            // Used to let users navigate the mod and encounter menus with the arrow keys!

    public GameObject encounterBox, devMod, content, retromodeWarning;
    public GameObject btnList,              btnBack,              btnNext,              btnExit,              btnOptions;
    public Text       ListText, ListShadow, BackText, BackShadow, NextText, NextShadow, ExitText, ExitShadow, OptionsText, OptionsShadow;
    public GameObject ModContainer,  ModBackground,     ModTitle,     ModTitleShadow,     EncounterCount,     EncounterCountShadow,     FolderText,     FolderTextShadow;
    public GameObject AnimContainer, AnimModBackground, AnimModTitle, AnimModTitleShadow, AnimEncounterCount, AnimEncounterCountShadow, AnimFolderText, AnimFolderTextShadow;

    // Use this for initialization
    private void Start() {
        Destroy(GameObject.Find("Player"));
        Destroy(GameObject.Find("Main Camera OW"));
        Destroy(GameObject.Find("Canvas OW"));
        Destroy(GameObject.Find("Canvas Two"));
        UnitaleUtil.firstErrorShown = false;

        // Load directory info
        DirectoryInfo modsFolder = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods"));

        // Deep mod detection in CYF's Mods folder
        List<DirectoryInfo>[] deepSearch = DeepModSearch(modsFolder);
        mods = deepSearch[0];
        folders = deepSearch[1];

        // Add mods and folders together, sort them by name, then by ownership
        List<DirectoryInfo> modsAndFolders = new List<DirectoryInfo>(mods);
        modsAndFolders.AddRange(folders);
        modsAndFolders.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        modPages = modsAndFolders.Select(d => new ModPage(d, mods.Contains(d))).ToList();

        BuildModPagesHierarchy();

        // Sort folders, then mods attached to them
        SortModsAndFolders(modsAndFolders, true);
        SortModsAndFolders(modsAndFolders, false);

        // Close all folders!
        CloseAllFolders();

        // Make sure that there is at least one playable mod present
        if (modPages.Count == 0) {
            GlobalControls.modDev = false;
            UnitaleUtil.DisplayLuaError("loading", "<b>Your mod folder is empty!</b>\nYou need at least 1 playable mod to use the Mod Selector.\n\n"
                + "Remember:\n1. Mods whose names start with \"@\" do not count\n2. Folders without encounter files or with only encounters whose names start with \"@\" do not count");
            return;
        }

        // Bind button functions
        btnBack.GetComponent<Button>().onClick.RemoveAllListeners();
        btnBack.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            if (!animationDone) return;
            modFolderSelection();
            ScrollMods(-1);
        });
        btnNext.GetComponent<Button>().onClick.RemoveAllListeners();
        btnNext.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            if (!animationDone) return;
            modFolderSelection();
            ScrollMods( 1);
        });

        // Give the mod list button a function
        btnList.GetComponent<Button>().onClick.RemoveAllListeners();
        btnList.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            if (animationDone)
                modFolderMiniMenu();
        });
        // Grab the exit button, and give it some functions
        btnExit.GetComponent<Button>().onClick.RemoveAllListeners();
        btnExit.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            SceneManager.LoadScene("Disclaimer");
            DiscordControls.StartTitle();
        });

        // Add devMod button functions
        if (GlobalControls.modDev) {
            btnOptions.GetComponent<Button>().onClick.RemoveAllListeners();
            btnOptions.GetComponent<Button>().onClick.AddListener(() => {
                eventSystem.SetSelectedGameObject(null);
                SceneManager.LoadScene("Options");
            });
        }

        // Crate Your Frisk initializer
        if (GlobalControls.crate) {
            //Exit button
            ExitText.text   = "← BYEE (RATIO'D)";
            ExitShadow.text = ExitText.text;

            //Options button
            OptionsText.text   = "OPSHUNZ (YUMMY) →";
            OptionsShadow.text = OptionsText.text;

            //Back button within scrolling list
            content.transform.Find("Back/Text").GetComponent<Text>().text = "← BCAK";

            //Mod list button
            ListText.gameObject.GetComponent<Text>().text   = "MDO LITS";
            ListShadow.gameObject.GetComponent<Text>().text = "MDO LITS";
        }

        if (retromodeWarning)
            retromodeWarning.SetActive(GlobalControls.retroMode);

        modFolderSelection();

        // Check if the encounter still exists
        ModPage modPage = modPages[currentPageID];
        if (modPage != null) {
            // Open all folders containing the mod
            while (modPage.parent != null) {
                modPage = modPage.parent;
                if (!modPage.isOpen)
                    OpenOrCloseFolder(modPages.FindIndex(p => p == modPage));
            }
        }

        // This check will be true if we just exited out of an encounter
        // If that's the case, we want to open the encounter list so the user only has to click once to re enter
        if (StaticInits.ENCOUNTER != "") {
            // Check to see if there is more than one encounter in the mod just exited from
            DirectoryInfo di2 = new DirectoryInfo(Path.Combine(FileLoader.ModDataPath, "Lua/Encounters"));
            string[] encounters = di2.GetFiles("*.lua").Select(f => Path.GetFileNameWithoutExtension(f.Name)).Where(f => !f.StartsWith("@")).ToArray();

            // Highlight the chosen encounter whenever the user exits the mod menu
            if (encounters.Length > 1) {
                int temp = selectedItem;
                encounterSelection();
                selectedItem = temp;
                content.transform.GetChild(selectedItem).GetComponent<MenuButton>().StartAnimation(1);
            }
            // Move the scrolly bit to where it was when the player entered the encounter
            content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, encounterListScroll);

            // Start the Exit button at half transparency
            ExitButtonAlpha                       = 0.5f;
            ExitText.GetComponent<Text>().color   = new Color(1f, 1f, 1f, 0.5f);
            ExitShadow.GetComponent<Text>().color = new Color(0f, 0f, 0f, 0.5f);

            // Start the Options button at half transparency
            if (GlobalControls.modDev) {
                OptionsButtonAlpha                       = 0.5f;
                OptionsText.GetComponent<Text>().color   = new Color(1f, 1f, 1f, 0.5f);
                OptionsShadow.GetComponent<Text>().color = new Color(0f, 0f, 0f, 0.5f);
            }
        // Player is coming here from the Disclaimer scene
        } else {
            // When the player enters from the Disclaimer screen, reset stored scroll positions
            modListScroll       = 0.0f;
            encounterListScroll = 0.0f;
        }

        // Reset it to let us accurately tell if the player just came here from the Disclaimer scene or the Battle scene
        StaticInits.ENCOUNTER = "";
    }

    /// <summary>
    /// This function performs a deep search for mods in the selected folder.
    /// Note that the function is recursive: it calls itself on subfolders if it finds any.
    /// A mod must satisfy a few conditions to be detected:
    /// - It must contain the folders Sprites and Lua/Encounters.
    /// - Its Lua/Encounters folder must contain at least one sprite.
    /// - Its root folder must not be CYF's Mods folder.
    /// - Its root folder must not be hidden nor start with the character @.
    /// </summary>
    /// <param name="dir">Directory to start the deep search from (usuallt the Mods folder)</param>
    /// <param name="currentDepth">Current depth of the search</param>
    /// <param name="maxDepth">Maximum depth of the search</param>
    /// <returns>The mods and notable folders found during the deep search</returns>
    private List<DirectoryInfo>[] DeepModSearch(DirectoryInfo dir, int currentDepth = 0, int maxDepth = 8) {
        List<DirectoryInfo> mods = new List<DirectoryInfo>();
        List<DirectoryInfo> folders = new List<DirectoryInfo>();
        DirectoryInfo modsDirectory = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods"));

        foreach (DirectoryInfo encountersFolder in dir.GetDirectories()) {
            // Recursive call
            if (currentDepth < maxDepth && encountersFolder.GetDirectories().Length > 0) {
                List<DirectoryInfo>[] childData = DeepModSearch(encountersFolder, currentDepth + 1, maxDepth);
                mods.AddRange(childData[0]);
                folders.AddRange(childData[1]);
            }

            // The current folder should be named Encounters and must contain at least one file.
            if (encountersFolder.Name != "Encounters" || encountersFolder.GetFiles().Length == 0)
                continue;

            DirectoryInfo luaFolder = encountersFolder.Parent;
            // The Encounters folder's parent should be named Lua.
            if (luaFolder == null || luaFolder.Name != "Lua")
                continue;

            DirectoryInfo modRootFolder = luaFolder.Parent;
            // The root of the mod should not be the CYF Mods folder.
            if (modRootFolder == null || modRootFolder.FullName == Path.Combine(FileLoader.DataRoot, "Mods"))
                continue;
            // The root of the mod should not start with the symbol @.
            if (modRootFolder.Name.StartsWith("@"))
                continue;
            // The root of the mod should not be hidden.
            if ((modRootFolder.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                continue;

            // The Lua folder should have a sibling folder named Sprites.
            DirectoryInfo spritesFolder = modRootFolder.GetDirectories().SingleOrDefault(d => d.Name == "Sprites");
            if (spritesFolder == null)
                continue;

            DirectoryInfo modParentFolder = modRootFolder.Parent;
            if (modParentFolder == null)
                continue;

            mods.Add(modRootFolder);
            if (!folders.Where(d => UnitaleUtil.DirectoryPathsEqual(d, modParentFolder)).Any() && !UnitaleUtil.DirectoryPathsEqual(modParentFolder, modsDirectory))
                folders.Add(modParentFolder);
        }

        // Prevent folder duplicates
        folders = folders.Where((d, index) => folders.FindIndex(d2 => UnitaleUtil.DirectoryPathsEqual(d2, d)) == index).ToList();
        return new List<DirectoryInfo>[] { mods, folders };
    }

    // A special function used specifically for error handling
    // It re-generates the mod list, and selects the first mod
    // Used for cases where the player selects a mod or encounter that no longer exists
    private void HandleErrors() {
        Debug.LogWarning("Mod or Encounter not found! Resetting mod list...");
        currentPageID = 0;
        bgs = new Dictionary<string, Sprite>();
        Start();
    }

    private IEnumerator LaunchMod() {
        // First: make sure the mod is still here and can be opened
        if (!new DirectoryInfo(modPages[currentPageID].path.FullName + "/Lua/Encounters/").Exists
         || !File.Exists(modPages[currentPageID].path.FullName + "/Lua/Encounters/" + StaticInits.ENCOUNTER + ".lua")) {
            HandleErrors();
            yield break;
        }

        // Dim the background to indicate loading
        ModBackground.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1875f);

        // Store the current position of the scrolly bit
        encounterListScroll = content.GetComponent<RectTransform>().anchoredPosition.y;

        yield return new WaitForEndOfFrame();
        try {
            StaticInits.InitAll(StaticInits.MODFOLDER, true);
            if (UnitaleUtil.firstErrorShown)
                throw new Exception();
            Debug.Log("Loading " + StaticInits.ENCOUNTER);
            GlobalControls.isInFight = true;
            DiscordControls.StartBattle(modPages[currentPageID].path.Name, StaticInits.ENCOUNTER);
            SceneManager.LoadScene("Battle");
        } catch (Exception e) {
            ModBackground.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            Debug.LogError("An error occured while loading a mod:\n" + e.Message + "\n\n" + e.StackTrace);
        }
    }

    // Shows a mod's "page".
    private void ShowMod(int id) {
        DirectoryInfo modsDirectory = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods"));
        // Error handler
        // If current index is now out of range
        if (id < 0 || id >= modPages.Count) {
            HandleErrors();
            return;
        }

        ModPage modPage = modPages[id];

        // If currently selected mod is not a folder and doesn't exist anymore, throw an error
        if (modPage.isMod
            && (!new DirectoryInfo(modPage.path.FullName + "/Lua/Encounters").Exists
            || new DirectoryInfo(modPage.path.FullName + "/Lua/Encounters").GetFiles("*.lua").Length == 0)) {
            HandleErrors();
            return;
        }

        string relativePath = UnitaleUtil.MakeRelativePath(Path.Combine(FileLoader.DataRoot, "Mods/"), modPage.path.FullName);
        if (modPage.isMod) {
            // Update currently selected mod folder
            StaticInits.MODFOLDER = relativePath;

            // Make clicking the background go to the encounter select screen
            ModBackground.GetComponent<Button>().onClick.RemoveAllListeners();
            ModBackground.GetComponent<Button>().onClick.AddListener(() => {
                eventSystem.SetSelectedGameObject(null);
                if (animationDone) {
                    encounterSelection();
                    content.transform.GetChild(selectedItem).GetComponent<MenuButton>().StartAnimation(1);
                }
            });

            // Update the background
            var ImgComp = ModBackground.GetComponent<Image>();
            FileLoader.absoluteSanitizationDictionary.Clear();
            FileLoader.relativeSanitizationDictionary.Clear();
            // First, check if we already have this mod's background loaded in memory
            if (bgs.ContainsKey(modPage.path.Name)) {
                ImgComp.sprite = bgs[modPage.path.Name];
            } else {
                // if not, find it and store it
                try {
                    Sprite thumbnail = SpriteUtil.FromFile("preview.png");
                    ImgComp.sprite = thumbnail;
                } catch {
                    try {
                        Sprite bg = SpriteUtil.FromFile("bg.png");
                        ImgComp.sprite = bg;
                    } catch { ImgComp.sprite = SpriteUtil.FromFile("black.png"); }
                }
                bgs.Add(modPage.path.Name, ImgComp.sprite);
            }

            // Get all encounters in the mod's Encounters folder
            DirectoryInfo encountersFolder = new DirectoryInfo(Path.Combine(FileLoader.ModDataPath, "Lua/Encounters"));
            string[] encounters = encountersFolder.GetFiles("*.lua").Select(f => Path.GetFileNameWithoutExtension(f.Name)).Where(f => !f.StartsWith("@")).ToArray();

            // List # of encounters, or name of encounter if there is only one
            if (encounters.Length == 1) {
                EncounterCount.GetComponent<Text>().text = encounters[0];
                // Crate Your Frisk version
                if (GlobalControls.crate)
                    EncounterCount.GetComponent<Text>().text = Temmify.Convert(encounters[0],  true);

                // Make clicking the bg directly open the encounter
                ModBackground.GetComponent<Button>().onClick.RemoveAllListeners();
                ModBackground.GetComponent<Button>().onClick.AddListener(() => {
                    eventSystem.SetSelectedGameObject(null);
                    if (!animationDone) return;
                    StaticInits.ENCOUNTER = encounters[0];
                    StartCoroutine(LaunchMod());
                });
            } else {
                EncounterCount.GetComponent<Text>().text = "Has " + encounters.Length + " encounters";
                // Crate Your Frisk version
                if (GlobalControls.crate)
                    EncounterCount.GetComponent<Text>().text = "HSA " + encounters.Length + " ENCUOTNERS";
            }
            EncounterCountShadow.GetComponent<Text>().text = EncounterCount.GetComponent<Text>().text;
        } else {
            // Make clicking the background open or close the folder
            ModBackground.GetComponent<Button>().onClick.RemoveAllListeners();
            ModBackground.GetComponent<Button>().onClick.AddListener(() => {
                eventSystem.SetSelectedGameObject(null);
                if (animationDone) {
                    OpenOrCloseFolder(id);
                    ShowMod(id);
                }
            });

            // Update the background
            var imgComp = ModBackground.GetComponent<Image>();
            imgComp.sprite = Resources.Load<Sprite>("Sprites/Folder" + (modPage.isOpen ? "Open" : "Closed"));

            // List # of mods
            EncounterCount.GetComponent<Text>().text = "Has " + modPage.deepLinkedMods + " mods";
            // Crate Your Frisk version
            if (GlobalControls.crate)
                EncounterCount.GetComponent<Text>().text = "HSA " + modPage.deepLinkedMods + " MDOS";
            EncounterCountShadow.GetComponent<Text>().text = EncounterCount.GetComponent<Text>().text;
        }

        // Update the text
        ModTitle.GetComponent<Text>().text = modPage.path.Name;
        // Crate Your Frisk version
        if (GlobalControls.crate)
            ModTitle.GetComponent<Text>().text = Temmify.Convert(modPage.path.Name, true);
        ModTitleShadow.GetComponent<Text>().text = ModTitle.GetComponent<Text>().text;

        // Give the parent folder if the mod is nested
        if (!UnitaleUtil.DirectoryPathsEqual(modPage.path.Parent, modsDirectory)) {
            List<string> folders = relativePath.Split(Path.DirectorySeparatorChar).ToList();
            folders.RemoveAt(folders.Count - 1);
            FolderText.GetComponent<Text>().text = "Belongs to the folder " + string.Join(Path.DirectorySeparatorChar + "", folders.ToArray());
        } else {
            FolderText.GetComponent<Text>().text = "";
        }
        FolderTextShadow.GetComponent<Text>().text = FolderText.GetComponent<Text>().text;

        // Update the color of the arrows
        if (modPages.Count == 1) {
            BackText.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            NextText.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        } else {
            BackText.color = new Color(1f, 1f, 1f, 1f);
            NextText.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    // Goes to the next or previous mod with a little scrolling animation.
    // -1 for left, 1 for right
    private void ScrollMods(int dir) {
        // First, determine if the next mod should be shown
        bool animate = modPages.Where(p => !p.isHidden).Count() > 1;

        // If the new mod is being shown, start the animation!
        if (!animate) return;
        animationTimer = dir / 10f;
        animationDone  = false;

        // Enable the "ANIM" assets
        AnimContainer.SetActive(true);
        AnimContainer.transform.localPosition                 = new Vector2(0, 0);
        AnimModBackground       .GetComponent<Image>().sprite = ModBackground.GetComponent<Image>().sprite;
        AnimModTitleShadow      .GetComponent<Text>().text    = ModTitleShadow.GetComponent<Text>().text;
        AnimModTitle            .GetComponent<Text>().text    = ModTitle.GetComponent<Text>().text;
        AnimEncounterCountShadow.GetComponent<Text>().text    = EncounterCountShadow.GetComponent<Text>().text;
        AnimEncounterCount      .GetComponent<Text>().text    = EncounterCount.GetComponent<Text>().text;
        AnimFolderTextShadow    .GetComponent<Text>().text    = FolderTextShadow.GetComponent<Text>().text;
        AnimFolderText          .GetComponent<Text>().text    = FolderText.GetComponent<Text>().text;

        // Move all real assets to the side
        ModBackground.transform.Translate(640        * dir, 0, 0);
        ModTitleShadow.transform.Translate(640       * dir, 0, 0);
        ModTitle.transform.Translate(640             * dir, 0, 0);
        EncounterCountShadow.transform.Translate(640 * dir, 0, 0);
        EncounterCount.transform.Translate(640       * dir, 0, 0);
        FolderTextShadow.transform.Translate(640     * dir, 0, 0);
        FolderText.transform.Translate(640           * dir, 0, 0);

        // Actually choose the next visible mod
        do { currentPageID = Math.Mod(currentPageID + dir, modPages.Count); }
        while (modPages[currentPageID].isHidden);

        ShowMod(currentPageID);
    }

    // Used to animate scrolling left or right.
    private void Update() {
        // Animation updating section
        if (AnimContainer.activeSelf) {
            animationTimer = animationTimer > 0 ? Mathf.Floor(animationTimer + 1) : Mathf.Ceil (animationTimer - 1);

            int distance = (int)((20 - Mathf.Abs(animationTimer)) * 3.4 * -Mathf.Sign(animationTimer));

            AnimContainer.transform.Translate(distance, 0, 0);
            ModContainer.transform.Translate(distance, 0, 0);

            if (Mathf.Abs(animationTimer) == 20) {
                AnimContainer.SetActive(false);

                // Manual movement because I can't change the movement multiplier to a precise enough value
                ModContainer.transform.Translate((int)(2 * -Mathf.Sign(animationTimer)), 0, 0);

                animationTimer = 0;
                animationDone = true;
            }
        }

        // Prevent scrolling too far in the encounter box
        if (encounterBox.activeSelf) {
            if (content.GetComponent<RectTransform>().anchoredPosition.y < -200)
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
            else if (content.GetComponent<RectTransform>().anchoredPosition.y > (content.transform.childCount - 1) * 30)
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (content.transform.childCount - 1) * 30);
        }

        // Detect hovering over the Exit button and handle fading
        if (ScreenResolution.mousePosition.x / ScreenResolution.displayedSize.x * 640 < 70 && Input.mousePosition.y / ScreenResolution.displayedSize.y * 480 > 450 && ExitButtonAlpha < 1f) {
            ExitButtonAlpha += 0.05f;
            ExitText.color   = new Color(1f, 1f, 1f, ExitButtonAlpha);
            ExitShadow.color = new Color(0f, 0f, 0f, ExitButtonAlpha);
        } else if (ExitButtonAlpha > 0.5f) {
            ExitButtonAlpha -= 0.05f;
            ExitText.color   = new Color(1f, 1f, 1f, ExitButtonAlpha);
            ExitShadow.color = new Color(0f, 0f, 0f, ExitButtonAlpha);
        }

        // Detect hovering over the Options button and handle fading
        if (GlobalControls.modDev) {
            if (ScreenResolution.mousePosition.x / ScreenResolution.displayedSize.x * 640 > 550 && Input.mousePosition.y / ScreenResolution.displayedSize.y * 480 > 450 && OptionsButtonAlpha < 1f) {
                OptionsButtonAlpha += 0.05f;
                OptionsText.color   = new Color(1f, 1f, 1f, OptionsButtonAlpha);
                OptionsShadow.color = new Color(0f, 0f, 0f, OptionsButtonAlpha);
            } else if (OptionsButtonAlpha > 0.5f) {
                OptionsButtonAlpha -= 0.05f;
                OptionsText.color   = new Color(1f, 1f, 1f, OptionsButtonAlpha);
                OptionsShadow.color = new Color(0f, 0f, 0f, OptionsButtonAlpha);
            }
        }

        // Controls:

        ////////////////// Main: ////////////////////////////////////
        //        Confirm: Start encounter (if mod has only one    //
        //                 encounter), or open encounter list      //
        //         Cancel: Return to Disclaimer screen             //
        //             Up: Open the mod list                       //
        //           Menu: Open the options menu                   //
        //           Left: Scroll left                             //
        //          Right: Scroll right                            //
        ////////////////// Encounter or Mod list: ///////////////////
        //        Confirm: Start an encounter, or select a mod     //
        //         Cancel: Exit                                    //
        //             Up: Move up                                 //
        //           Down: Move down                               //
        //           Menu: Open/Close the folder                   //
        /////////////////////////////////////////////////////////////

        if (!encounterBox.activeSelf) {
            // Main controls
            if (animationDone) {
                // Move left
                if (GlobalControls.input.Left == ButtonState.PRESSED)       ScrollMods(-1);
                // Move right
                else if (GlobalControls.input.Right == ButtonState.PRESSED) ScrollMods(1);
                // Open the mod list
                else if (GlobalControls.input.Up == ButtonState.PRESSED) {
                    modFolderMiniMenu();
                    content.transform.GetChild(selectedItem).GetComponent<MenuButton>().StartAnimation(1);
                // Open the encounter list or start the encounter (if there is only one encounter)
                } else if (GlobalControls.input.Confirm == ButtonState.PRESSED)
                    ModBackground.GetComponent<Button>().onClick.Invoke();
            }

            // Access the Options menu
            if (GlobalControls.input.Menu == ButtonState.PRESSED)
                btnOptions.GetComponent<Button>().onClick.Invoke();
            // Return to the Disclaimer screen
            if (GlobalControls.input.Cancel == ButtonState.PRESSED)
                btnExit.GetComponent<Button>().onClick.Invoke();
        } else {
            // Encounter or Mod List controls
            if (GlobalControls.input.Up == ButtonState.PRESSED || GlobalControls.input.Down == ButtonState.PRESSED) {
                // Store previous value of selectedItem
                int previousSelectedItem = selectedItem;

                // Move up or down the list
                selectedItem += GlobalControls.input.Up == ButtonState.PRESSED ? -1 : 1;

                // Keep the selector in-bounds
                if (selectedItem < 0)                                     selectedItem = content.transform.childCount - 1;
                else if (selectedItem > content.transform.childCount - 1) selectedItem = 0;

                // Animate the old button
                GameObject previousButton = content.transform.GetChild(previousSelectedItem).gameObject;
                previousButton.GetComponent<MenuButton>().StartAnimation(-1);

                // Animate the new button
                GameObject newButton = content.transform.GetChild(selectedItem).gameObject;
                newButton.GetComponent<MenuButton>().StartAnimation(1);

                // Scroll to the newly chosen button if it is hidden!
                float buttonTopEdge    = -newButton.GetComponent<RectTransform>().anchoredPosition.y + 100;
                float buttonBottomEdge = -newButton.GetComponent<RectTransform>().anchoredPosition.y + 100 + 30;

                float topEdge    = content.GetComponent<RectTransform>().anchoredPosition.y;
                float bottomEdge = content.GetComponent<RectTransform>().anchoredPosition.y + 230;

                // Button is above the top of the view
                if (topEdge > buttonTopEdge)
                    content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, buttonTopEdge);
                // Button is below the bottom of the view
                else if (bottomEdge < buttonBottomEdge)
                    content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, buttonBottomEdge - 230);
            }

            // Exit
            if (GlobalControls.input.Cancel == ButtonState.PRESSED)
                ModBackground.GetComponent<Button>().onClick.Invoke();
            // Select the mod or encounter
            else if (GlobalControls.input.Confirm == ButtonState.PRESSED)
                content.transform.GetChild(selectedItem).GetComponent<Button>().onClick.Invoke();

            // Open/Close the current folder
            if (GlobalControls.input.Menu == ButtonState.PRESSED)
                if (content.transform.GetChild(selectedItem).Find("QuickFolderButton").gameObject.activeSelf)
                    content.transform.GetChild(selectedItem).Find("QuickFolderButton").GetComponent<Button>().onClick.Invoke();
        }
    }

    // Shows the "mod page" screen.
    private void modFolderSelection() {
        eventSystem.SetSelectedGameObject(null);
        UnitaleUtil.printDebuggerBeforeInit = "";
        ShowMod(currentPageID);

        // Hide the 4 buttons if needed
        if (!GlobalControls.modDev)
            devMod.SetActive(false);

        // Show the mod list button
        btnList.SetActive(true);

        // If the encounter box is visible, remove all encounter buttons before hiding
        if (encounterBox.activeSelf) {
            foreach (Transform b in content.transform) {
                if (b.gameObject.name != "Back")
                    Destroy(b.gameObject);
                else
                    b.GetComponent<MenuButton>().Reset();
            }
        }
        // Hide the encounter selection box
        encounterBox.SetActive(false);
    }

    // Shows the list of available encounters in a mod.
    private void encounterSelection() {
        // Hide the mod list button
        btnList.SetActive(false);

        // Automatically choose "back"
        selectedItem = 0;

        // Make clicking the background exit the encounter selection screen
        ModBackground.GetComponent<Button>().onClick.RemoveAllListeners();
        ModBackground.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            if (animationDone)
                modFolderSelection();
        });
        // Show the encounter selection box
        encounterBox.SetActive(true);
        // Reset the encounter box's position
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        // Give the back button its function
        GameObject back = content.transform.Find("Back").gameObject;
        back.GetComponent<Button>().onClick.RemoveAllListeners();
        back.GetComponent<Button>().onClick.AddListener(modFolderSelection);

        DirectoryInfo di = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods/" + StaticInits.MODFOLDER + "/Lua/Encounters"));
        if (!di.Exists || di.GetFiles().Length <= 0) return;
        string[] encounters = di.GetFiles("*.lua").Select(f => Path.GetFileNameWithoutExtension(f.Name)).Where(f => !f.StartsWith("@")).ToArray();

        int count = 0;
        foreach (string encounter in encounters) {
            count += 1;

            // Create a button for each encounter file
            GameObject button = Instantiate(back);

            // Set parent and name
            button.transform.SetParent(content.transform);
            button.name = "EncounterButton";

            // Set position
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100 - count * 30);

            // Set color
            button.GetComponent<Image>().color                        = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            button.GetComponent<MenuButton>().NormalColor             = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            button.GetComponent<MenuButton>().HoverColor              = new Color(0.75f, 0.75f, 0.75f, 1f);
            button.transform.Find("Fill").GetComponent<Image>().color = new Color(0.5f,  0.5f,  0.5f,  0.5f);

            // Set text
            button.transform.Find("Text").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(encounter);
            if (GlobalControls.crate)
                button.transform.Find("Text").GetComponent<Text>().text = Temmify.Convert(Path.GetFileNameWithoutExtension(encounter), true);

            // Finally, set function!
            string filename = Path.GetFileNameWithoutExtension(encounter);

            int tempCount = count;

            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => {
                eventSystem.SetSelectedGameObject(null);
                selectedItem          = tempCount;
                StaticInits.ENCOUNTER = filename;
                StartCoroutine(LaunchMod());
            });
        }
    }

    // Opens the scrolling interface and lets the user browse their mods.
    private void modFolderMiniMenu() {
        // Hide the mod list button
        btnList.SetActive(false);

        // Automatically select the current mod when the mod list appears
        selectedItem = FindPageIndex(currentPageID) + 1;

        // Give the back button its function
        GameObject back = content.transform.Find("Back").gameObject;
        back.GetComponent<Button>().onClick.RemoveAllListeners();
        back.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            // Reset the encounter box's position
            modListScroll = 0.0f;
            modFolderSelection();
        });

        // Make clicking the background exit this menu
        ModBackground.GetComponent<Button>().onClick.RemoveAllListeners();
        ModBackground.GetComponent<Button>().onClick.AddListener(() => {
            eventSystem.SetSelectedGameObject(null);
            if (!animationDone) return;
            // Store the encounter box's position so it can be remembered upon exiting a mod
            modListScroll = content.GetComponent<RectTransform>().anchoredPosition.y;
            modFolderSelection();
        });
        // Show the encounter selection box
        encounterBox.SetActive(true);
        // Move the encounter box to the stored position, for easier mod browsing
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, modListScroll);

        int count = -1;
        for (int i = 0; i < modPages.Count; i++) {
            ModPage modPage = modPages[i];
            if (modPage.isHidden)
                continue;

            count++;

            // Create a button for each mod
            GameObject button = Instantiate(back);

            // Set parent and name
            button.transform.SetParent(content.transform);
            button.name = "ModButton";

            // Set position
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100 - (count + 1) * 30);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(430 - 20 * modPage.nestLevel, 30);
            button.transform.Find("Fill").GetComponent<RectTransform>().sizeDelta = new Vector2(420 - 20 * modPage.nestLevel, 20);

            // Set color
            button.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            button.GetComponent<MenuButton>().NormalColor = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            button.GetComponent<MenuButton>().HoverColor  = new Color(0.75f, 0.75f, 0.75f, 1f);
            button.transform.Find("Fill").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            // Set text
            button.transform.Find("Text").GetComponent<Text>().text = modPage.path.Name;
            if (GlobalControls.crate)
                button.transform.Find("Text").GetComponent<Text>().text = Temmify.Convert(modPage.path.Name, true);

            // Set extra nesting elements
            if (modPage.nestLevel > 0)
                button.transform.Find("ChildLink").gameObject.SetActive(true);
            if (modPage.children.Count > 0 && modPage.isNestedOpen) {
                button.transform.Find("ParentLink").gameObject.SetActive(true);
                button.transform.Find("ParentLink").GetComponent<RectTransform>().sizeDelta = new Vector2(2, -15 + 30 * (modPage.shownChildrenAndSelf - 1));
            }

            int tempCount = i;

            // Set the quick folder opening/closing icon for folders
            if (!modPage.isMod) {
                button.transform.Find("QuickFolderButton").gameObject.SetActive(true);
                button.transform.Find("QuickFolderButton").Find("QuickFolderButtonImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/QuickFolder" + (modPage.isOpen ? "Open" : "Closed"));
                button.transform.Find("QuickFolderButton").GetComponent<Button>().onClick.RemoveAllListeners();
                button.transform.Find("QuickFolderButton").GetComponent<Button>().onClick.AddListener(() => {
                    eventSystem.SetSelectedGameObject(null);
                    // Store the encounter box's position so it can be remembered upon exiting a mod
                    modListScroll = content.GetComponent<RectTransform>().anchoredPosition.y;
                    int buttonsInModList = content.transform.childCount;

                    currentPageID = tempCount;
                    OpenOrCloseFolder(currentPageID);
                    modFolderSelection();
                    while (modPages[currentPageID].isHidden)
                        currentPageID--;
                    ShowMod(currentPageID);
                    modFolderMiniMenu();
                    content.transform.GetChild(buttonsInModList + selectedItem - 1).GetComponent<MenuButton>().StartAnimation(1);
                });
            }

            // Finally, set function!
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => {
                eventSystem.SetSelectedGameObject(null);
                // Store the encounter box's position so it can be remembered upon exiting a mod
                modListScroll = content.GetComponent<RectTransform>().anchoredPosition.y;

                currentPageID = tempCount;
                modFolderSelection();
                ShowMod(currentPageID);
            });
        }
    }

    // Links mods and folders together through a parent/child system
    private void BuildModPagesHierarchy() {
        DirectoryInfo modsDirectory = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods"));

        foreach (ModPage modPage in modPages) {
            DirectoryInfo directory = modPage.path;

            do { directory = directory.Parent; }
            while (!UnitaleUtil.DirectoryPathsEqual(directory, modsDirectory) && !folders.Any(d => UnitaleUtil.DirectoryPathsEqual(d, directory)));

            if (!UnitaleUtil.DirectoryPathsEqual(directory, modsDirectory) && folders.Any(d => UnitaleUtil.DirectoryPathsEqual(d, directory))) {
                ModPage parentPage = modPages.Find(p => UnitaleUtil.DirectoryPathsEqual(p.path, directory));
                parentPage.children.Add(modPage);
                modPage.parent = parentPage;
            }
        }
    }

    private int FindPageIndex(int id) {
        ModPage page = modPages[id];
        if (page == null)
            return -1;

        int resultId = 0;
        for (int i = 0; i < id; i++)
            if (!modPages[i].isHidden)
                resultId++;

        return resultId;
    }

    // Closes or opens a given folder
    private void OpenOrCloseFolder(int id) {
        ModPage page = modPages[id];
        if (page == null)
            return;
        page.isOpen = !page.isOpen;
    }

    // Closes all folders
    private void CloseAllFolders() {
        for (int i = 0; i < modPages.Count; i++) {
            ModPage modPage = modPages[i];
            if (modPage.isOpen)
                OpenOrCloseFolder(i);
        }
    }

    // Sorts mods and folders so that anything that belongs to a folder is right under it alphabetically
    // If a folder contains folders and mods, folders will be first alphabetically, then mods will be sorted alphabetically
    private void SortModsAndFolders(List<DirectoryInfo> modsAndFolders, bool sortingFolders) {
        DirectoryInfo modsDirectory = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods"));

        // For each folder, link it to its parent, and put it at the end of its child list
        for (int i = 0; i < modsAndFolders.Count; i++) {
            DirectoryInfo directory = modsAndFolders[i];
            if (folders.Where(d => UnitaleUtil.DirectoryPathsEqual(d, directory)).Any() && !sortingFolders)
                continue;
            if (mods.Where(d => UnitaleUtil.DirectoryPathsEqual(d, directory)).Any() && sortingFolders)
                continue;

            DirectoryInfo currentDirectory = directory;
            while (!UnitaleUtil.DirectoryPathsEqual(currentDirectory.Parent, modsDirectory)) {
                currentDirectory = currentDirectory.Parent;
                // If the parent folder is recognized, move the current directory and its children under it
                if (modsAndFolders.Where(d => UnitaleUtil.DirectoryPathsEqual(d, currentDirectory)).Any()) {
                    ModPage currentPage = modPages.Find(p => UnitaleUtil.DirectoryPathsEqual(p.path, directory));
                    ModPage parentPage = modPages.Find(p => UnitaleUtil.DirectoryPathsEqual(p.path, currentDirectory));

                    // Move the current folder to the right place
                    int oldPageIndex = modPages.FindIndex(p => p == currentPage);
                    int newPageIndex = modPages.FindIndex(p => p == parentPage);
                    newPageIndex += parentPage.deepLinkedChildren + (oldPageIndex > newPageIndex ? 1 : 0);
                    modPages.Remove(currentPage);
                    modPages.Insert(newPageIndex, currentPage);

                    // Move all rightfully sorted children as well
                    for (int j = 0; j < currentPage.deepLinkedChildren; j++) {
                        if (newPageIndex > oldPageIndex) {
                            oldPageIndex--;
                            newPageIndex--;
                        }
                        ModPage childPage = modPages[oldPageIndex];
                        modPages.Remove(childPage);
                        modPages.Insert(newPageIndex, childPage);
                    }

                    if (sortingFolders) parentPage.linkedFolders++;
                    else                parentPage.linkedMods++;

                    break;
                }
            }
        }
    }
}
