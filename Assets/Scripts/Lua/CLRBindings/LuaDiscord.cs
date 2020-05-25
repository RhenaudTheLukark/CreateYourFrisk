using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaDiscord {
    // Sets the top row of the Discord Rich Presence Status. If you are in a battle, it will always put "Playing Mod: " before it, otherwise it will set it with no prefix
    // The default value "" makes the name completely dissapear in the letter case.
    public void SetName(string name = "", bool noPrefix = false) {
        if (name == "") DiscordControls.ClearRPVars(true);

        string realName = name;
        if (noPrefix || UnitaleUtil.IsOverworld || GlobalControls.isInShop) realName = DiscordControls.GetPlayingName(realName);

        DiscordControls.SetPresence(realName);
    }
    
    // Sets the second row of the Discord Rich Presence Status. The default value "" makes it completely dissapear.
    public void SetDetails(string details = "") {
        if (details == "") DiscordControls.ClearRPVars(false, true);
        DiscordControls.SetPresence("", details);
    }
    
    // Sets the current time of the Discord Rich Presence Status based on the time of execution, in seconds. 
    // The default value 0 will make the timer dissapear.
    // Example: typing 1 at the start of battle will make it start from 1 second. Then, 5 minutes later you decide that you want to reset the timer, you can run the same function.
    // The second value tells if the time should be a timer-like (true) or a stopwatch-like (false).
    public void SetTime(int time = 0, bool remaining = false) {
        DiscordControls.SetPresence("", "", time, remaining);
    }
    
}