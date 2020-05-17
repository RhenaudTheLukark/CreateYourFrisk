using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordControls {

    static public Discord.Discord discord;
    static string rpName = "";
    static string rpDetails = "";
    static int rpTime = 0;
	
	static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    // Use this for initialization
    public static void Start () {

        discord = new Discord.Discord(711497963771527219, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        //ResetModName();
        //Clear();
    }

	public static string getPlayingName(string name) {
		return "Playing Mod: " + name;
	}
	
    public static void StartModSelect() {
		ClearRPVars(true, true);
		SetPresence("Selecting a Mod", "", 0);
	}

    public static void StartMod(string name) {
		ClearRPVars(true, true);
        SetPresence(getPlayingName(name), "", 1);
    }

    public static void SetPresence(string name = "", string details = "", int time = 0, bool remaining = false) {

        if (name != "") {
            rpName = name;
        }

        if (details != "") {
            rpDetails = details;
        }
		
		if (time != 0) {
			rpTime = ((int)(System.DateTime.UtcNow - epochStart).TotalSeconds) + time;
		} else {
			rpTime = 0;
		}
		
		
		var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity
        {
            State = rpDetails,
            Details = rpName,
            Timestamps = {
                Start = (remaining ? 0 : rpTime),
				End = (remaining ? rpTime : 0)
            },
			Assets = {
				LargeImage = "cyf_logo",
				LargeText = "Create Your Frisk"
			}
        };

        activityManager.UpdateActivity(activity, (res) => {
            if (res == Discord.Result.Ok)
            {
                Debug.Log("Everything is fine!" + " || " + name + " || " + details);
            }
            else
            {
                Debug.Log("Failed");
            }
        });
    }
	
	public static void ClearRPVars(bool name = false, bool details = false) {
		rpName = name ? "" : rpName;
		rpDetails = details ? "" : rpDetails;
	}

    public static void ClearPresence() {
        var activityManager = discord.GetActivityManager();
        activityManager.ClearActivity((result) =>
        {
            if (result == Discord.Result.Ok)
            {
                Debug.Log("Clear Success!");
            }
            else
            {
                Debug.Log("Clear Failed");
            }
        });
    }

    // Update is called once per frame
    public static void Update () {
        discord.RunCallbacks();
    }
}