using System;
using MoonSharp.Interpreter;

public class DiscordControls {
    static public Discord.Discord discord;
    static string rpName = "";
    static string rpDetails = "";
    static int rpTime = 0;

    /// <summary>
    /// 0 = Everything
    /// 1 = Game Only
    /// 2 = Nothing
    /// </summary>
    static public int curr_setting;
    static public string[] settingNames = { "Everything", "Game Only", "Nothing" };
    
    static readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    static bool isactive = true;

    // Use this for initialization
    public static void Start () {
        // Creates the object that manages the Rich Presence Commands. The first argument is the APPID, the second tells the libraries if Discord must be started or not.
        try {
            discord = new Discord.Discord(711497963771527219, (ulong)Discord.CreateFlags.NoRequireDiscord);
        } catch (Exception) {
            isactive = false;
        }
        
        // Gets Discord Visibility Setting
        if (LuaScriptBinder.GetAlMighty(null, "CYFDiscord") == null) curr_setting = 0;
        else                                                         curr_setting = (int) LuaScriptBinder.GetAlMighty(null, "CYFDiscord").Number;
    }

    /// <summary>
    /// Changes the Discord Rich Presence visibility setting.
    /// </summary>
    /// <param name="spd">Added so the setting can be written out at init time without changing it.</param>
    /// <returns>The name of the current setting.</returns>
    public static string ChangeVisibilitySetting(int spd) {
        curr_setting += spd;
        if (curr_setting >= settingNames.Length)
            curr_setting = 0;
        
        if (spd > 0)
            LuaScriptBinder.SetAlMighty(null, "CYFDiscord", DynValue.NewNumber(curr_setting), true);
        
        SetPresence();
        
        return settingNames[curr_setting];
    }

    /// <summary></summary>
    /// <param name="name">The string we want to add a prefix to.</param>
    /// <returns>The string given with "Playing Mod: " added at the beginning of the string.</returns>
    public static string GetPlayingName(string name) { return "Playing Mod: " + name; }
    
    /// <summary>
    /// Sets the status when you're choosing a mod, erasing details and timer
    /// </summary>
    public static void StartModSelect() {
        if (!isactive) return;
        ClearRPVars(true, true);
        SetPresence("Selecting a Mod", "", 0);
    }
    
    /// <summary>
    /// Sets the initial status when you play a mod, erasing details and starting the timer
    /// </summary>
    /// <param name="name">The name of the mod.</param>
    public static void StartMod(string name) {
        if (!isactive) return;
        ClearRPVars(true, true);
        SetPresence(GetPlayingName(name), "", 1);
    }
    
    /// <summary>
    /// Updates the Discord Rich Presence status bar.
    /// </summary>
    /// <param name="name">The first line of the Discord Rich Presence status bar.</param>
    /// <param name="details">The second line of the Discord Rich Presence status bar.</param>
    /// <param name="time">The time currently displayed on the Discord Rich Presence status bar.</param>
    /// <param name="remaining">True if the timer must be counted down, false otherwise.</param>
    public static void SetPresence(string name = "", string details = "", int time = 0, bool remaining = false) {
        if (!isactive) return;
        
        if (name != "")    rpName = name;
        if (details != "") rpDetails = details;
        
        if (time != 0) rpTime = ((int)(System.DateTime.UtcNow - epochStart).TotalSeconds) + time;
        else           rpTime = 0;
        
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
                LargeText = (curr_setting <= 1) ? ("Create Your Frisk v" + GlobalControls.CYFversion) : ""
            }
        };

        activityManager.UpdateActivity(activity, (res) => {});
    }
    
    /// <summary>
    /// Clears some Discord Rich Presence related variables.
    /// </summary>
    /// <param name="name">True if you want to clear the first line of the Discord Rich Presence status bar.</param>
    /// <param name="details">True if you want to clear the second line of the Discord Rich Presence status bar.</param>
    public static void ClearRPVars(bool name = false, bool details = false) {
        rpName = name ? "" : rpName;
        rpDetails = details ? "" : rpDetails;
    }

    // Update is called once per frame
    public static void Update() {
        if (isactive)
            discord.RunCallbacks();
    }
}