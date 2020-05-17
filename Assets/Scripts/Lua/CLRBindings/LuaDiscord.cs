using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaDiscord {

	public void SetName(string name) {
		if (UnitaleUtil.IsOverworld || GlobalControls.isInShop) {
			if (name == "") {
				DiscordControls.ClearRPVars(true);
			}
			DiscordControls.SetPresence(name);
		} else {
			DiscordControls.SetPresence(DiscordControls.getPlayingName(name));
		}
	}
	
	public void SetDetails(string details) {
		if (details == "") {
			DiscordControls.ClearRPVars(false, true);
		}
		DiscordControls.SetPresence("", details, -1);
	}
	
	public void SetTime(int time, bool remaining = false) {
		DiscordControls.SetPresence("", "", time, remaining);
	}
	
}