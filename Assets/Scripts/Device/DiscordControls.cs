using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class DiscordControls {

    static public Discord.Discord discord;
    static string rpName = "";
    static string rpDetails = "";
    static int rpTime = 0;
    
    static public int curr_setting;
    static public string[] settingNames = {"Everything", "Game Only", "Nothing"};
    
    static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    // Use this for initialization
    public static void Start () {

        discord = new Discord.Discord(711497963771527219, (System.UInt64)Discord.CreateFlags.NoRequireDiscord); // Creates the object that manages the Rich Presence Commands. The first argument is the APPID, the second tells the libraries if Discord is a must or not.
        //ResetModName();
        //Clear();
        
        // Gets Discord Visibility Setting
        if (LuaScriptBinder.GetAlMighty(null, "CYFDiscord") == null)
            curr_setting = 0;
        else
            curr_setting = (int) LuaScriptBinder.GetAlMighty(null, "CYFDiscord").Number;
        
        Debug.Log(curr_setting);
    }
    
    public static string ChangeVisibilitySetting(int spd) { // The "speed" at which the options will go. Added so I can write out the setting at init time without changing it.
        curr_setting += spd;
        if (curr_setting >= settingNames.Length)
            curr_setting = 0;
        
        if (spd > 0)
            LuaScriptBinder.SetAlMighty(null, "CYFDiscord", DynValue.NewNumber(curr_setting), true);
        
        SetPresence();
        
        return settingNames[curr_setting];
    }
    
    // Returns the name with "Playing Mod: " attached to it.
    public static string getPlayingName(string name) {
        return "Playing Mod: " + name;
    }
    
    // Sets the status to "Selecting a Mod", erasing details and timer
    public static void StartModSelect() {
        ClearRPVars(true, true);
        SetPresence("Selecting a Mod", "", 0);
    }

    // Sets the status to "Playing a mod: ", erasing details and starting the timer
    public static void StartMod(string name) {
        ClearRPVars(true, true);
        SetPresence(getPlayingName(name), "", 1);
    }

    // The function that sets the Discord Rich presence status. name's and details's default value make them not change, while time's default value removes the timer.
    // The remaining boolean argument tells if Discord should run the timer backwards (as a stopwatch) or not.
    public static void SetPresence(string name = "", string details = "", int time = 0, bool remaining = false) {
        
        if (name != "") rpName = name;

        if (details != "") rpDetails = details;
        
        if (time != 0) 
            rpTime = ((int)(System.DateTime.UtcNow - epochStart).TotalSeconds) + time;
        else 
            rpTime = 0;
        
        if (curr_setting == 2) return;
        
        var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity {
            State = (curr_setting == 0) ? rpDetails : "", // The details (aka second row)
            Details = (curr_setting == 0) ? rpName : "", // The top row
            Timestamps = { // The timer being set up
                Start = (curr_setting == 0) ? (remaining ? 0 : rpTime) : 0,
                End = (curr_setting == 0) ? (remaining ? rpTime : 0) : 0
            },
            Assets = { // The CYF Logo
                LargeImage = (curr_setting <= 1) ? "cyf_logo" : "",
                LargeText = (curr_setting <= 1) ? "Create Your Frisk" : ""
            }
        };

        activityManager.UpdateActivity(activity, (res) => {});
    }
    
    // Since the SetPresence function's first two arguments' default values indicate that they should not be changed, this function resets the vales so they don't appear anymore on the next SetPresence call.
    // (Yes, it's only use is to come right before SetPresence and SetPresence having the default value "")
    public static void ClearRPVars(bool name = false, bool details = false) {
        rpName = name ? "" : rpName;
        rpDetails = details ? "" : rpDetails;
    }

    // Update is called once per frame
    public static void Update () {
        discord.RunCallbacks();
    }
}