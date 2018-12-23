using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using MoonSharp.Interpreter;

public class OptionsScript : MonoBehaviour {
    // used to prevent the player from erasing real/almighty globals or their save by accident
    private int RealGlobalCooldown = 0;
    private int AlMightyGlobalCooldown = 0;
    private int SaveCooldown = 0;
    
    // used to update the Description periodically
    private int DescriptionTimer = 0;
    
    // Use this for initialization
    private void Start() {
        // add button functions
        
        // reset RealGlobals
        GameObject.Find("ResetRG").GetComponent<Button>().onClick.AddListener(() => {
            if (RealGlobalCooldown > 0) {
                LuaScriptBinder.ClearVariables();
                RealGlobalCooldown = 60 * 2;
                if (!GlobalControls.crate)
                    GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =            "Real Globals Erased!";
                else
                    GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =        "REEL GOLBELZ DELEET!!!!!";
            } else {
                RealGlobalCooldown = 60 * 2;
                if (!GlobalControls.crate)
                    GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =                   "Are you sure?";
                else
                    GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =                      "R U SUR???";
            }
        });
        
        // reset AlMightyGlobals
        GameObject.Find("ResetAG").GetComponent<Button>().onClick.AddListener(() => {
            if (AlMightyGlobalCooldown > 0) {
                LuaScriptBinder.ClearAlMighty();
                AlMightyGlobalCooldown = 60 * 2;
                if (!GlobalControls.crate)
                    GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =        "AlMighty Globals Erased!";
                else
                    GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =          "ALMEIGHTIZ DELEET!!!!!";
            } else {
                AlMightyGlobalCooldown = 60 * 2;
                if (!GlobalControls.crate)
                    GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =                   "Are you sure?";
                else
                    GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =                      "R U SUR???";
            }
        });
        
        // clear Save
        GameObject.Find("ClearSave").GetComponent<Button>().onClick.AddListener(() => {
            if (SaveCooldown > 0) {
                File.Delete(Application.persistentDataPath + "/save.gd");
                SaveCooldown = 60 * 2;
                if (!GlobalControls.crate)
                    GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                   "Save wiped!";
                else
                    GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                           "RIP";
            } else {
                SaveCooldown = 60 * 2;
                if (!GlobalControls.crate)
                    GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                 "Are you sure?";
                else
                    GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                    "R U SUR???";
            }
        });
        
        // toggle safe mode
        GameObject.Find("Safe").GetComponent<Button>().onClick.AddListener(() => {
            ControlPanel.instance.Safe = !ControlPanel.instance.Safe;
            
            // save Safe Mode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFSafeMode", DynValue.NewBoolean(ControlPanel.instance.Safe), true);
            
            if (!GlobalControls.crate) {
                if (ControlPanel.instance.Safe)
                    GameObject.Find("Safe").GetComponentInChildren<Text>().text =                      "Safe mode: On";
                else
                    GameObject.Find("Safe").GetComponentInChildren<Text>().text =                     "Safe mode: Off";
            } else {
                if (ControlPanel.instance.Safe)
                    GameObject.Find("Safe").GetComponentInChildren<Text>().text =                      "SFAE MDOE: ON";
                else
                    GameObject.Find("Safe").GetComponentInChildren<Text>().text =                     "SFAE MDOE: OFF";
            }
        });
        ControlPanel.instance.Safe = !ControlPanel.instance.Safe;
        GameObject.Find("Safe").GetComponent<Button>().onClick.Invoke();
        
        // toggle retrocompatibility mode
        GameObject.Find("Retro").GetComponent<Button>().onClick.AddListener(() => {
            GlobalControls.retroMode =!GlobalControls.retroMode;
            
            // save RetroMode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFRetroMode", DynValue.NewBoolean(GlobalControls.retroMode), true);
            
            if (!GlobalControls.crate) {
                if (GlobalControls.retroMode)
                    GameObject.Find("Retro").GetComponentInChildren<Text>().text =       "Retrocompatibility Mode: On";
                else
                    GameObject.Find("Retro").GetComponentInChildren<Text>().text =      "Retrocompatibility Mode: Off";
            } else {
                if (GlobalControls.retroMode)
                    GameObject.Find("Retro").GetComponentInChildren<Text>().text =        "RETORCMOAPTIILBIYT MOD: ON";
                else
                    GameObject.Find("Retro").GetComponentInChildren<Text>().text =       "RETORCMOAPTIILBIYT MOD: OFF";
            }
        });
        GlobalControls.retroMode =!GlobalControls.retroMode;
        GameObject.Find("Retro").GetComponent<Button>().onClick.Invoke();
        
        // toggle pixel-perfect fullscreen
        GameObject.Find("Fullscreen").GetComponent<Button>().onClick.AddListener(() => {
            GlobalControls.perfectFullscreen =!GlobalControls.perfectFullscreen;
            
            // save RetroMode preferences to AlMighties
            LuaScriptBinder.SetAlMighty(null, "CYFPerfectFullscreen", DynValue.NewBoolean(GlobalControls.perfectFullscreen), true);
            
            if (!GlobalControls.crate) {
                if (GlobalControls.perfectFullscreen)
                    GameObject.Find("Fullscreen").GetComponentInChildren<Text>().text =      "Blurless Fullscreen: On";
                else
                    GameObject.Find("Fullscreen").GetComponentInChildren<Text>().text =     "Blurless Fullscreen: Off";
            } else {
                if (GlobalControls.retroMode)
                    GameObject.Find("Fullscreen").GetComponentInChildren<Text>().text =     "NOT UGLEE FULLSRCEEN: ON";
                else
                    GameObject.Find("Fullscreen").GetComponentInChildren<Text>().text =    "NOT UGLEE FULLSRCEEN: OFF";
            }
        });
        GlobalControls.perfectFullscreen =!GlobalControls.perfectFullscreen;
        GameObject.Find("Fullscreen").GetComponent<Button>().onClick.Invoke();
        
        // exit
        GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(() => {SceneManager.LoadScene("ModSelect");});
        
        // Crate Your Frisk
        if (GlobalControls.crate) {
            // labels
            GameObject.Find("OptionsLabel").GetComponent<Text>().text =                                      "OPSHUNS";
            GameObject.Find("DescriptionLabel").GetComponent<Text>().text =                                "MORE TXET";
            
            // buttons
            GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =                      "RESTE RELA GOLBALZ";
            GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =                   "RESTE ALMIGTY GOLBALZ";
            GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                              "WYPE SAV";
            GameObject.Find("Safe").GetComponentInChildren<Text>().text = "SFAE MODE: " + (ControlPanel.instance.Safe ? "ON" : "OFF");
            GameObject.Find("Retro").GetComponentInChildren<Text>().text = "RETORCMOAPTIILBIYT MOD: " + (ControlPanel.instance.Safe ? "ON" : "OFF");
            GameObject.Find("Fullscreen").GetComponentInChildren<Text>().text = "NOT UGLEE FULLSRCEEN: " + (GlobalControls.perfectFullscreen ? "ON" : "OFF");
            GameObject.Find("Exit").GetComponentInChildren<Text>().text =                         "EXIT TOO MAD SELCT";
        }
    }
    
    // Gets the text the description should use based on what button is currently being hovered over
    private string GetDescription(string buttonName) {
        string response = "";
        switch(buttonName) {
            case "ResetRG":
                response = "Resets all Real Globals.\n\n"
                         + "Real Globals are variables that persist through battles, but are deleted when CYF is closed.";
                if (!GlobalControls.crate)
                    return response;
                else
                    return Temmify.Convert(response);
            case "ResetAG":
                response = "Resets all AlMighty Globals.\n\n"
                         + "AlMighty Globals are variables that are saved to a file, and stay even when you close CYF.\n\n"
                         + "The options on this screen are stored as AlMighties.";
                if (!GlobalControls.crate)
                    return response;
                else
                    return Temmify.Convert(response);
            case "ClearSave":
                response = "Clears your save file.\n\n"
                         + "This is the save file used for CYF's Overworld.\n\n"
                         + "Your save file is located at:\n\n";
                if (!GlobalControls.crate)
                    // return response + Application.persistentDataPath + "/save.gd</size></b>";
                    return response + "<b><size='14'>" + Application.persistentDataPath + "/save.gd</size></b>";
                else
                    return Temmify.Convert(response) + "<b><size='14'>" + Application.persistentDataPath + "/save.gd</size></b>";
            case "Safe":
                response = "Toggles safe mode.\n\n"
                         + "It does nothing specifically, but mod authors can detect if you have this enabled, and use it to filter unsafe content, such as blood, gore, and swear words.";
                if (!GlobalControls.crate)
                    return response;
                else
                    return Temmify.Convert(response);
            case "Retro":
                response = "Toggles retrocompatibility mode.\n\n"
                         + "This mode is designed specifically to make encounters imported from Unitale v0.2.1a act as they did on the old engine.";
                if (!GlobalControls.crate)
                    return response;
                else
                    return Temmify.Convert(response);
            case "Fullscreen":
                response = "Toggles blurless Fullscreen mode.\n\n"
                         + "This controls whether fullscreen mode will appear \"blurry\" or not.\n\n"
                         + "May slow down some computers.";
                if (!GlobalControls.crate)
                    return response;
                else
                    return Temmify.Convert(response);
            case "Exit":
                response = "Returns to the Mod Select screen.";
                if (!GlobalControls.crate)
                    return response;
                else
                    return Temmify.Convert(response);
            default:
                if (!GlobalControls.crate)
                    return "Hover over an option and its description will appear here!";
                else
                    return "HOVR OVR DA TING N GET TEXT HEAR!!";
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
            if ((Input.mousePosition.x / Screen.width) * 640 >= 40 && (Input.mousePosition.x / Screen.width) * 640 <= 300) {
                // ResetRG
                if      ((Input.mousePosition.y / Screen.height) * 480 <= 420 && (Input.mousePosition.y / Screen.height) * 480 > 380)
                    hoverItem = "ResetRG";
                // ResetAG
                else if ((Input.mousePosition.y / Screen.height) * 480 <= 380 && (Input.mousePosition.y / Screen.height) * 480 > 340)
                    hoverItem = "ResetAG";
                // ClearSave
                else if ((Input.mousePosition.y / Screen.height) * 480 <= 340 && (Input.mousePosition.y / Screen.height) * 480 > 300)
                    hoverItem = "ClearSave";
                // Safe
                else if ((Input.mousePosition.y / Screen.height) * 480 <= 300 && (Input.mousePosition.y / Screen.height) * 480 > 260)
                    hoverItem = "Safe";
                // Retro
                else if ((Input.mousePosition.y / Screen.height) * 480 <= 260 && (Input.mousePosition.y / Screen.height) * 480 > 220)
                    hoverItem = "Retro";
                // Fullscreen
                else if ((Input.mousePosition.y / Screen.height) * 480 <= 220 && (Input.mousePosition.y / Screen.height) * 480 > 180)
                    hoverItem = "Fullscreen";
                // Exit
                else if ((Input.mousePosition.y / Screen.height) * 480 <=  60 && (Input.mousePosition.y / Screen.height) * 480 >  20)
                    hoverItem = "Exit";
            }
                
            GameObject.Find("Description").GetComponent<Text>().text = GetDescription(hoverItem);
        }
        
        // make the player click twice to reset RG or AG, or to wipe their save
        if (RealGlobalCooldown > 0)
            RealGlobalCooldown -= 1;
        else if (RealGlobalCooldown == 0) {
            RealGlobalCooldown = -1;
            if (!GlobalControls.crate)
                GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =                  "Reset Real Globals";
            else
                GameObject.Find("ResetRG").GetComponentInChildren<Text>().text =                  "RSETE RAEL GLOBALS";
        }
        
        if (AlMightyGlobalCooldown > 0)
            AlMightyGlobalCooldown -= 1;
        else if (AlMightyGlobalCooldown == 0) {
            AlMightyGlobalCooldown = -1;
            if (!GlobalControls.crate)
                GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =              "Reset AlMighty Globals";
            else
                GameObject.Find("ResetAG").GetComponentInChildren<Text>().text =                      "RESET ALIMGHTY";
        }
        
        if (SaveCooldown > 0)
            SaveCooldown -= 1;
        else if (SaveCooldown == 0) {
            SaveCooldown = -1;
            if (!GlobalControls.crate)
                GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                         "Wipe Save";
            else
                GameObject.Find("ClearSave").GetComponentInChildren<Text>().text =                          "WYPE SAV";
        }
    }
}
