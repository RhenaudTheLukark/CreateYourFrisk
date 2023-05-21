using Discord;
using System;
using MoonSharp.Interpreter;
using UnityEngine;

public static class DiscordControls {
    public static Discord.Discord discord;
    private static Activity activity;
    private static ActivityManager activityManager;
    /// <summary>
    /// 0 = Everything
    /// 1 = Game Only
    /// 2 = Nothing
    /// </summary>
    public static int curr_setting;

    private static readonly string[] settingNames = { "Everything", "Game Only", "Nothing" };
    private static readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static string oldDetails = activity.Details;
    private static string oldState = activity.State;
    private static long oldTime;
    private static bool updateQueued;
    public static bool isActive;

    // Use this for initialization
    public static void Start() {
        // Creates the object that manages the Rich Presence Commands. The first argument is the APPID, the second tells the libraries if Discord must be started or not.
        try {
            discord = new Discord.Discord(711497963771527219, (ulong)CreateFlags.NoRequireDiscord);
            activityManager = discord.GetActivityManager();
            isActive = true;
            Debug.Log("Discord Status: Success");
        } catch (Exception e) {
            isActive = false;
            Debug.Log("Discord Status: Failed - " + e.Message);
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

        if (isActive)
            switch (curr_setting) {
                case 0:
                    StartModSelect(false);
                    break;
                case 1:
                    activity.Details          = "";
                    activity.State            = "";
                    activity.Timestamps.Start = 0;
                    activity.Timestamps.End   = 0;
                    UpdatePresence(true);
                    break;
                default:
                    Clear();
                    break;
            }

        return GlobalControls.crate ? Temmify.Convert(settingNames[curr_setting]) : settingNames[curr_setting];
    }

    /// <summary>
    /// Sets the status when you're on the title screen, erasing details and timer
    /// </summary>
    public static void StartTitle() {
        activity.Details = "Title Screen";
        activity.State = "";

        UpdatePresence();
    }

    /// <summary>
    /// Sets the status when you're entering the Overworld, erasing details and timer
    /// </summary>
    public static void StartOW() {
        activity.Details = "In the Overworld";
        activity.State = "";
        ClearTime(false);

        UpdatePresence();
    }

    /// <summary>
    /// This function runs whenever showing a scene
    /// </summary>
    public static void ShowOWScene(string mapName) {
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
        if (!isActive || (!force && curr_setting > 0) || updateQueued) return;

        updateQueued = true;
    }

    /// <summary>
    /// This function will be called one time, on the frame after applying settings, to prevent abuse of the activity manager.
    /// </summary>
    private static void UpdateActivity() {
        if (isActive)
            activityManager.UpdateActivity(activity, (res) => {});
        updateQueued = false;
    }

    /// <summary>
    /// Sets the text in the top row of the discord rich presence status
    /// </summary>
    /// <param name="name">New text to display.</param>
    public static void SetName(string name) {
        // Work around a very strange bug in the Discord SDK
        if (name.Length == 1)
            name += " ";

        activity.Details = (curr_setting == 0) ? name : "";
    }

    /// <summary>
    /// Resets the text in the top row of the discord rich presence status to blank (or what it would be originally)
    /// </summary>
    /// <param name="reset">If true, text will be reset to its initial value. Otherwise, it will be cleared from the status.</param>
    public static void ClearName(bool reset) {
        activity.Details = reset ? oldDetails : "";
    }

    /// <summary>
    /// Sets the text in the second row of the discord rich presence status
    /// </summary>
    /// <param name="details">New text to display.</param>
    public static void SetDetails(string details) {
        // Work around a very strange bug in the Discord SDK
        if (details.Length == 1)
            details += " ";

        activity.State = (curr_setting == 0) ? details : "";
    }

    /// <summary>
    /// Resets the text in the second row of the discord rich presence status to blank (or what it would be originally)
    /// </summary>
    /// <param name="reset">If true, text will be reset to its initial value. Otherwise, it will be cleared from the status.</param>
    public static void ClearDetails(bool reset) {
        activity.State = reset ? oldState : "";
    }

    /// <summary>
    /// Sets the timer value in the discord rich presence status, to either elapsed time or a countdown timer
    /// </summary>
    /// <param name="seconds">Number of seconds to display in the timer.</param>
    /// <param name="countdown">If true, the timer will count down from this value instead of counting up.</param>
    public static void SetTime(int seconds, bool countdown) {
        if (!countdown) {
            activity.Timestamps.Start = GetCurrentTime() - seconds;
            activity.Timestamps.End = 0;
        } else {
            activity.Timestamps.Start = 0;
            activity.Timestamps.End = GetCurrentTime() + seconds;
        }
    }

    /// <summary>
    /// Resets the timer value in the discord rich presence status to blank (or what it would be originally)
    /// </summary>
    /// <param name="reset">If true, text will be reset to its initial value. Otherwise, it will be cleared from the status.</param>
    public static void ClearTime(bool reset) {
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
    private static int GetCurrentTime() { return (int)(DateTime.UtcNow - epochStart).TotalSeconds; }

    /// <summary>
    /// Internal use function that clears the discord rich presence status.
    /// </summary>
    public static void Clear() {
        if (isActive)
            activityManager.ClearActivity((result) => {});
    }

    // Update is called once per frame
    public static void Update() {
        if (!isActive) return;
        if (updateQueued) UpdateActivity();
        try { discord.RunCallbacks(); }
        catch { isActive = false; }
    }
}