﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using MoonSharp.Interpreter;

public class OptionsScript : MonoBehaviour {
    // used to prevent the player from erasing real/almighty globals or their save by accident
    private int RealGlobalCooldown;
    private int AlMightyGlobalCooldown;
    private int SaveCooldown;

    // used to update the Description periodically
    private int DescriptionTimer;

    // used to store the state of CreteYourFrisk at the start of the scene
    private bool LocalCrate;
    private bool CrateUnlocked;

    // game objects
    public GameObject ResetRG, ResetAG, ClearSave, Safe, Retro, Fullscreen, Scale, Discord, Crate, Exit;
    public Text Description;

    // Use this for initialization
    private void Start() {
        LocalCrate = GlobalControls.crate;
        CrateUnlocked = LuaScriptBinder.GetAlMighty(null, "CrateYourFrisk") != null;

        // add button functions

        // reset RealGlobals
        ResetRG.GetComponent<Button>().onClick.AddListener(() => {
            if (RealGlobalCooldown > 0) {
                LuaScriptBinder.ClearVariables();
                RealGlobalCooldown = 60 * 2;
                ResetRG.GetComponentInChildren<Text>().text = !LocalCrate ? "Real Globals Erased!" : "REEL GOLBELZ DELEET!!!!!";
            } else {
                RealGlobalCooldown = 60 * 2;
                ResetRG.GetComponentInChildren<Text>().text = !LocalCrate ? "Are you sure?" : "R U SUR???";
            }
        });

        // reset AlMightyGlobals
        ResetAG.GetComponent<Button>().onClick.AddListener(() => {
            if (AlMightyGlobalCooldown > 0) {
                LuaScriptBinder.ClearAlMighty();
                AlMightyGlobalCooldown = 60 * 2;
                ResetAG.GetComponentInChildren<Text>().text = !LocalCrate ? "AlMighty Globals Erased!" : "ALMEIGHTIZ DELEET!!!!!";

                // Add useful almighties
                LuaScriptBinder.SetAlMighty(null, "CYFSafeMode", DynValue.NewBoolean(ControlPanel.instance.Safe));
                LuaScriptBinder.SetAlMighty(null, "CYFRetroMode", DynValue.NewBoolean(GlobalControls.retroMode));
                LuaScriptBinder.SetAlMighty(null, "CYFPerfectFullscreen", DynValue.NewBoolean(ScreenResolution.perfectFullscreen));
                LuaScriptBinder.SetAlMighty(null, "CYFWindowScale", DynValue.NewNumber(ScreenResolution.windowScale));
                if (CrateUnlocked)
                    LuaScriptBinder.SetAlMighty(null, "CrateYourFrisk", DynValue.NewBoolean(GlobalControls.crate));

            } else {
                AlMightyGlobalCooldown = 60 * 2;
                ResetAG.GetComponentInChildren<Text>().text = !LocalCrate ? "Are you sure?" : "R U SUR???";
            }
        });

        // clear Save
        ClearSave.GetComponent<Button>().onClick.AddListener(() => {
            if (SaveCooldown > 0) {
                File.Delete(Application.persistentDataPath + "/save.gd");
                SaveCooldown = 60 * 2;
                ClearSave.GetComponentInChildren<Text>().text = !LocalCrate ? "Save wiped!" : "RIP";
            } else {
                SaveCooldown = 60 * 2;
                ClearSave.GetComponentInChildren<Text>().text = !LocalCrate ? "Are you sure?" : "R U SUR???";
            }
        });

        // toggle safe mode
        Safe.GetComponent<Button>().onClick.AddListener(() => {
            ControlPanel.instance.Safe = !ControlPanel.instance.Safe;

            // save Safe Mode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFSafeMode", DynValue.NewBoolean(ControlPanel.instance.Safe));

            Safe.GetComponentInChildren<Text>().text = !LocalCrate
                ? ("Safe mode: " + (ControlPanel.instance.Safe ? "On" : "Off"))
                : ("SFAE MDOE: " + (ControlPanel.instance.Safe ? "ON" : "OFF"));
        });
        Safe.GetComponentInChildren<Text>().text = !LocalCrate
            ? ("Safe mode: " + (ControlPanel.instance.Safe ? "On" : "Off"))
            : ("SFAE MDOE: " + (ControlPanel.instance.Safe ? "ON" : "OFF"));

        // toggle retrocompatibility mode
        Retro.GetComponent<Button>().onClick.AddListener(() => {
            GlobalControls.retroMode =!GlobalControls.retroMode;

            // save RetroMode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFRetroMode", DynValue.NewBoolean(GlobalControls.retroMode));

            Retro.GetComponentInChildren<Text>().text = !LocalCrate
                ? ("Retrocompatibility Mode: " + (GlobalControls.retroMode ? "On" : "Off"))
                : ( "RETORCMOAPTIILBIYT MOD: " + (GlobalControls.retroMode ? "ON" : "OFF"));
        });
        Retro.GetComponentInChildren<Text>().text = !LocalCrate
            ? ("Retrocompatibility Mode: " + (GlobalControls.retroMode ? "On" : "Off"))
            : ( "RETORCMOAPTIILBIYT MOD: " + (GlobalControls.retroMode ? "ON" : "OFF"));

        // toggle pixel-perfect fullscreen
        Fullscreen.GetComponent<Button>().onClick.AddListener(() => {
            ScreenResolution.perfectFullscreen =!ScreenResolution.perfectFullscreen;

            // save RetroMode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFPerfectFullscreen", DynValue.NewBoolean(ScreenResolution.perfectFullscreen));

            Fullscreen.GetComponentInChildren<Text>().text = !LocalCrate
                ? ( "Blurless Fullscreen: " + (ScreenResolution.perfectFullscreen ? "On" : "Off"))
                : ("NOT UGLEE FULLSCREEN: " + (ScreenResolution.perfectFullscreen ? "ON" : "OFF"));
        });
        Fullscreen.GetComponentInChildren<Text>().text = !LocalCrate
            ? ( "Blurless Fullscreen: " + (ScreenResolution.perfectFullscreen ? "On" : "Off"))
            : ("NOT UGLEE FULLSCREEN: " + (ScreenResolution.perfectFullscreen ? "ON" : "OFF"));

        // change window scale
        Scale.GetComponent<Button>().onClick.AddListener(() => {
            #if UNITY_EDITOR
                int maxScale = 0;
            #else
                int maxScale = Mathf.FloorToInt(System.Math.Min(Screen.currentResolution.width / 640f, Screen.currentResolution.height / 480f));
            #endif
            if (ScreenResolution.windowScale < maxScale)
                ScreenResolution.windowScale += 1;
            else
                ScreenResolution.windowScale = 1;
            ScreenResolution.tempWindowScale = ScreenResolution.windowScale;
            ScreenResolution.SetFullScreen(Screen.fullScreen);

            // save RetroMode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFWindowScale", DynValue.NewNumber(ScreenResolution.windowScale));

            Scale.GetComponentInChildren<Text>().text = !LocalCrate
                ? "Window Scale: "  + ScreenResolution.windowScale + "x"
                : "WEENDO STRECH: " + ScreenResolution.windowScale + "X";
        });
        #if UNITY_EDITOR
            Scale.GetComponent<Button>().onClick.Invoke();
        #else
            Scale.GetComponentInChildren<Text>().text = !LocalCrate
                ? "Window Scale: " + ScreenResolution.windowScale  + "x"
                : "WEENDO STRECH: " + ScreenResolution.windowScale + "X";
        #endif

        // Discord Rich Presence
        // Change Discord Status Visibility
        Discord.GetComponent<Button>().onClick.AddListener(() => {
            Debug.Log("whaDiscord");
            Discord.GetComponentInChildren<Text>().text = (!LocalCrate ? "Discord Display: " : "DEESCORD DESPLAY: ") + DiscordControls.ChangeVisibilitySetting(1);
        });
        Discord.GetComponentInChildren<Text>().text = (!LocalCrate ? "Discord Display: " : "DEESCORD DESPLAY: ") + DiscordControls.ChangeVisibilitySetting(0);

        // Enable / Disable Crate Your Frisk
        Crate.GetComponent<Button>().onClick.AddListener(() => {
            GlobalControls.crate = !GlobalControls.crate;
            LuaScriptBinder.SetAlMighty(null, "CrateYourFrisk", DynValue.NewBoolean(GlobalControls.crate));

            Crate.GetComponentInChildren<Text>().text = !LocalCrate
                ? ("Crate Your Frisk: " + (GlobalControls.crate ? "On" : "Off"))
                : ("BAD SPELING: " + (GlobalControls.crate ? "ON" : "OFF"));
        });
        Crate.GetComponentInChildren<Text>().text = !LocalCrate ? "Crate Your Frisk: Off" : "BAD SPELING: ON";
        // Hide the Crate button if CrateYourFrisk is nil
        Crate.SetActive(CrateUnlocked);

        // exit
        Exit.GetComponent<Button>().onClick.AddListener(() => {
            GlobalControls.ReloadCrate();
            SceneManager.LoadScene("ModSelect");
        });

        // Crate Your Frisk
        if (!LocalCrate) return;
        // labels
        GameObject.Find("OptionsLabel").GetComponent<Text>().text     = "OPSHUNS";
        GameObject.Find("DescriptionLabel").GetComponent<Text>().text = "MORE TXET";

        // buttons
        ResetRG.GetComponentInChildren<Text>().text   = "RESTE RELA GOLBALZ";
        ResetAG.GetComponentInChildren<Text>().text   = "RESTE ALMIGTY GOLBALZ";
        ClearSave.GetComponentInChildren<Text>().text = "WYPE SAV";
        Exit.GetComponentInChildren<Text>().text      = "EXIT TOO MAD SELCT";
    }

    // Gets the text the description should use based on what button is currently being hovered over
    private string GetDescription(string buttonName) {
        string response;
        switch(buttonName) {
            case "ResetRG":
                response = "Resets all Real Globals.\n\n"
                         + "Real Globals are variables that persist through battles, but are deleted when CYF is closed.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "ResetAG":
                response = "Resets all AlMighty Globals.\n\n"
                         + "AlMighty Globals are variables that are saved to a file, and stay even when you close CYF.\n\n"
                         + "The options on this screen are stored as AlMighties.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "ClearSave":
                response = "Clears your save file.\n\n"
                         + "This is the save file used for CYF's Overworld.\n\n"
                         + "Your save file is located at:\n\n";
                if (!LocalCrate)
                    // return response + Application.persistentDataPath + "/save.gd</size></b>";
                    return response + "<b><size='14'>" + Application.persistentDataPath + "/save.gd</size></b>";
                else
                    return Temmify.Convert(response) + "<b><size='14'>" + Application.persistentDataPath + "/save.gd</size></b>";
            case "Safe":
                response = "Toggles safe mode.\n\n"
                         + "This does nothing on its own, but mod authors can detect if you have this enabled, and use it to filter unsafe content, such as blood, gore, and swear words.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "Retro":
                response = "Toggles retrocompatibility mode.\n\n"
                         + "This mode is designed specifically to make encounters imported from Unitale v0.2.1a act as they did on the old engine.\n\n\n\n";
                if (!LocalCrate)
                    return response + "<b>CAUTION!\nDISABLE</b> this for mods made for CYF. This feature should only be used with Mods made for\n<b>Unitale v0.2.1a</b>.";
                else
                    return Temmify.Convert(response) + "<b>" + Temmify.Convert("CAUTION!\nDISABLE") + "</b> " + Temmify.Convert("this for mods made for CYF.");
            case "Fullscreen":
                response = "Toggles blurless Fullscreen mode.\n\n"
                         + "This controls whether fullscreen mode will appear \"blurry\" or not.\n\n\n"
                         + "Press <b>F4</b> or <b>Alt+Enter</b> to toggle Fullscreen.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "Scale":
                response = "Scales the window in Windowed mode.\n\n"
                         + "This is useful for especially large screens (such as 4k monitors).\n\n"
                         + "Has no effect in Fullscreen mode.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "Discord":
                response = "Changes how much Discord Rich Presence should display on your profile regarding you playing Create Your Frisk.\n\n"
                         + "<b>Everything</b>: Everything is displayed: the mod you're playing, a timestamp and a description.\n\n"
                         + "<b>Game Only</b>: Only shows that you're playing Create Your Frisk.\n\n"
                         + "<b>Nothing</b>: Disables Discord Rich Presence entirely.\n\n"
                         + "If CYF's connection to Discord is lost, you will have to restart CYF if you want your rich presence back.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "Crate":
                response = "Enables or disables the Crate Your Frisk mode of the engine.\n\n"
                         + "This mode adds several surprises such as scrambled text or modified Player battle choices.\n\n"
                         + "Unlocked through the completion of CYF v0.5's secret.\n\n"
                         + "The changes are applied when you leave the Options menu.";
                return !LocalCrate ? response : Temmify.Convert(response);
            case "Exit":
                response = "Returns to the Mod Select screen.";
                return !LocalCrate ? response : Temmify.Convert(response);
            default:
                return !LocalCrate ? "Hover over an option and its description will appear here!" : "HOVR OVR DA TING N GET TEXT HEAR!!";
        }
    }

    // Used to animate scrolling left or right.
    private void Update() {
        // update the description every 1/6th of a second
        if (DescriptionTimer > 0)
            DescriptionTimer--;
        else {
            DescriptionTimer = 10;

            // try to find which button the player is hovering over
            string hoverItem = "";
            // if the player is within the range of the buttons
            int mousePosX = (int)((ScreenResolution.mousePosition.x / ScreenResolution.displayedSize.x) * 640);
            int mousePosY = (int)((Input.mousePosition.y / ScreenResolution.displayedSize.y) * 480);
            if (mousePosX >= 40 && mousePosX <= 290) {
                // ResetRG
                if      (mousePosY <= 420 && mousePosY > 380)
                    hoverItem = "ResetRG";
                // ResetAG
                else if (mousePosY <= 380 && mousePosY > 340)
                    hoverItem = "ResetAG";
                // ClearSave
                else if (mousePosY <= 340 && mousePosY > 300)
                    hoverItem = "ClearSave";
                // Safe
                else if (mousePosY <= 300 && mousePosY > 260)
                    hoverItem = "Safe";
                // Retro
                else if (mousePosY <= 260 && mousePosY > 220)
                    hoverItem = "Retro";
                // Fullscreen
                else if (mousePosY <= 220 && mousePosY > 180)
                    hoverItem = "Fullscreen";
                // Scale
                else if (mousePosY <= 180 && mousePosY > 140)
                    hoverItem = "Scale";
                // Discord
                else if (mousePosY <= 140 && mousePosY > 100)
                    hoverItem = "Discord";
                // Crate
                else if (mousePosY <= 100 && mousePosY >  60 && CrateUnlocked)
                    hoverItem = "Crate";
                // Exit
                else if (mousePosY <=  60 && mousePosY >  20)
                    hoverItem = "Exit";
            }

            Description.GetComponent<Text>().text = GetDescription(hoverItem);
        }

        // make the player click twice to reset RG or AG, or to wipe their save
        if (RealGlobalCooldown > 0)
            RealGlobalCooldown -= 1;
        else if (RealGlobalCooldown == 0) {
            RealGlobalCooldown = -1;
            ResetRG.GetComponentInChildren<Text>().text = !LocalCrate ? "Reset Real Globals" : "RSETE RAEL GLOBALS";
        }

        if (AlMightyGlobalCooldown > 0)
            AlMightyGlobalCooldown -= 1;
        else if (AlMightyGlobalCooldown == 0) {
            AlMightyGlobalCooldown = -1;
            ResetAG.GetComponentInChildren<Text>().text = !LocalCrate ? "Reset AlMighty Globals" : "RESET ALIMGHTY";
        }

        if (SaveCooldown > 0)
            SaveCooldown -= 1;
        else if (SaveCooldown == 0) {
            SaveCooldown = -1;
            ClearSave.GetComponentInChildren<Text>().text = !LocalCrate ? "Wipe Save" : "WYPE SAV";
        }
    }
}
