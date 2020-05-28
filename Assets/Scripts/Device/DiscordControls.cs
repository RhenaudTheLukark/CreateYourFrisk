using Discord;
using System;
using MoonSharp.Interpreter;

public static class DiscordControls {
    public static Discord.Discord discord;
    static Activity activity;
    static ActivityManager activityManager;
    /// <summary>
    /// 0 = Everything
    /// 1 = Game Only
    /// 2 = Nothing
    /// </summary>
    public static int curr_setting;

    static string[] settingNames = { "Everything", "Game Only", "Nothing" };
    static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    static string oldDetails = activity.Details;
    static string oldState = activity.State;
    static long oldTime;
    static bool isactive = true;

    // Use this for initialization
    public static void Start() {
        // Creates the object that manages the Rich Presence Commands. The first argument is the APPID, the second tells the libraries if Discord must be started or not.
        try {
            discord = new Discord.Discord(711497963771527219, (ulong)Discord.CreateFlags.NoRequireDiscord);
        } catch (Exception) {
            isactive = false;
        }

        // Gets Discord Visibility Setting
        if (LuaScriptBinder.GetAlMighty(null, "CYFDiscord") == null) curr_setting = 0;
        else                                                         curr_setting = (int)LuaScriptBinder.GetAlMighty(null, "CYFDiscord").Number;

        // Creates the activity objects that will be modified and used as needed
        activity = new Activity {
            Name = GlobalControls.crate ? ControlPanel.instance.WinodwBsaisNmae : ControlPanel.instance.WindowBasisName,
            Details = "", // The top row
            State = "", // The second row
            Timestamps = { // The timer
                Start = 0,
                End = 0
            },
            Assets = { // The CYF Logo
                LargeImage = "cyf_logo",
                LargeText = ControlPanel.instance.WindowBasisName
            }
        };
        activityManager = discord.GetActivityManager();

        // Set initial activity properties and status
        ChangeVisibilitySetting(0);
        oldTime = GetCurrentTime();
        ClearTime(true);
        StartTitle();
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

        if (curr_setting == 0)
            StartModSelect(false);
        else if (curr_setting == 1) {
            activity.Details = "";
            activity.State = "";
            activity.Timestamps.Start = 0;
            activity.Timestamps.End = 0;
            UpdatePresence(true);
        } else
            Clear();

        return GlobalControls.crate ? Temmify.Convert(settingNames[curr_setting]) : settingNames[curr_setting];
    }

    /// <summary>
    /// Sets the status when you're on the title screen, erasing details and timer
    /// </summary>
    public static void StartTitle() {
        if (!isactive) return;

        activity.Details = "Title Screen";
        activity.State = "";

        UpdatePresence();
    }

    /// <summary>
    /// Sets the status when you're entering the Overworld, erasing details and timer
    /// </summary>
    public static void StartOW() {
        if (!isactive) return;

        activity.Details = "In the Overworld";
        activity.State = "";
        ClearTime(false);

        UpdatePresence();
    }

    /// <summary>
    /// This function runs whenever showing a scene 
    /// </summary>
    public static void ShowOWScene(string mapName) {
        if (!isactive) return;

        activity.Details = "In the Overworld";
        activity.State = mapName;
        UnitaleUtil.MapCorrespondanceList.TryGetValue(mapName, out activity.State);
        ClearTime(false);

        oldDetails = activity.Details;
        oldState = activity.State;
        oldTime = activity.Timestamps.Start;

        UpdatePresence();
    }

    /// <summary>
    /// Sets the status when you're choosing a mod, erasing details and timer
    /// </summary>
    /// <param name="reset">Whether to reset the timer when loading the mod select scene.</param>
    public static void StartModSelect(bool reset = true) {
        if (!isactive) return;

        activity.Details = "Selecting a Mod";
        activity.State = "";
        if (reset)
            oldTime = GetCurrentTime();
        ClearTime(true);

        UpdatePresence();
    }

    /// <summary>
    /// Sets the initial status when you play a mod, erasing details and starting the timer
    /// </summary>
    /// <param name="modName">The name of the mod.</param>
    /// <param name="encounterName">The name of the encounter.</param>
    public static void StartBattle(string modName, string encounterName) {
        if (!isactive) return;

        activity.Details = "Playing Mod: " + modName;
        activity.State = encounterName;
        oldTime = GetCurrentTime();
        ClearTime(true);

        oldDetails = activity.Details;
        oldState = activity.State;
        oldTime = activity.Timestamps.Start;

        UpdatePresence();
    }

    /// <summary>
    /// The function to actually update the discord rich presence status
    /// </summary>
    /// <param name="force">Forcefully updates presence even when setting is set to "game only" or "nothing".</param>
    public static void UpdatePresence(bool force = false) {
        if (!isactive || (!force && curr_setting > 0)) return;

        activityManager.UpdateActivity(activity, (res) => {});
    }

    /// <summary>
    /// Sets the text in the top row of the discord rich presence status
    /// </summary>
    /// <param name="name">New text to display.</param>
    public static void SetName(string name) {
        if (!isactive) return;

        // Work around a very strange bug in the Discord SDK
        if (name.Length == 1)
            name = name + " ";

        activity.Details = (curr_setting == 0) ? name : "";
    }

    /// <summary>
    /// Resets the text in the top row of the discord rich presence status to blank (or what it would be originally)
    /// </summary>
    /// <param name="reset">If true, text will be reset to its initial value. Otherwise, it will be cleared from the status.</param>
    public static void ClearName(bool reset) {
        if (!isactive) return;

        if (reset)
            activity.Details = oldDetails;
        else
            activity.Details = "";
    }

    /// <summary>
    /// Sets the text in the second row of the discord rich presence status
    /// </summary>
    /// <param name="details">New text to display.</param>
    public static void SetDetails(string details) {
        if (!isactive) return;

        // Work around a very strange bug in the Discord SDK
        if (details.Length == 1)
            details = details + " ";

        activity.State = (curr_setting == 0) ? details : "";
    }

    /// <summary>
    /// Resets the text in the second row of the discord rich presence status to blank (or what it would be originally)
    /// </summary>
    /// <param name="reset">If true, text will be reset to its initial value. Otherwise, it will be cleared from the status.</param>
    public static void ClearDetails(bool reset) {
        if (!isactive) return;

        if (reset)
            activity.State = oldState;
        else
            activity.State = "";
    }

    /// <summary>
    /// Sets the timer value in the discord rich presence status, to either elapsed time or a countdown timer
    /// </summary>
    /// <param name="seconds">Number of seconds to display in the timer.</param>
    /// <param name="countdown">If true, the timer will count down from this value instead of counting up.</param>
    public static void SetTime(int seconds, bool countdown) {
        if (!isactive) return;

        if (!countdown) {
            activity.Timestamps.Start = GetCurrentTime() - seconds;
            activity.Timestamps.End = 0;
        } else {
            activity.Timestamps.Start = 0;
            activity.Timestamps.End = GetCurrentTime() + seconds;
        }
    }

    /// <summary>
    /// Resets the imer value in the discord rich presence status to blank (or what it would be originally)
    /// </summary>
    /// <param name="reset">If true, text will be reset to its initial value. Otherwise, it will be cleared from the status.</param>
    public static void ClearTime(bool reset) {
        if (!isactive) return;

        if (reset) {
            activity.Timestamps.Start = oldTime;
            activity.Timestamps.End = 0;
        } else {
            activity.Timestamps.Start = 0;
            activity.Timestamps.End = 0;
        }
    }

    /// <summary>
    /// Internal use function that gets a timestamp for the current moment in time.
    /// </summary>
    private static int GetCurrentTime() { return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds; }

    /// <summary>
    /// Internal use function that clears the discord rich presence status.
    /// </summary>
    public static void Clear() { activityManager.ClearActivity((result) => {}); }

    // Update is called once per frame
    public static void Update() {
        if (isactive)
            discord.RunCallbacks();
    }
}