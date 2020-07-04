public class LuaDiscord {
    public void SetName(string name) {
        if (name == null) throw new CYFException("Discord.SetName: Argument cannot be nil.");
        DiscordControls.SetName(name);
        DiscordControls.UpdatePresence();
    }

    public void ClearName(bool reset = false) {
        DiscordControls.ClearName(reset);
        DiscordControls.UpdatePresence();
    }

    public void SetDetails(string details) {
        if (details == null) throw new CYFException("Discord.SetDetails: Argument cannot be nil.");
        DiscordControls.SetDetails(details);
        DiscordControls.UpdatePresence();
    }

    public void ClearDetails(bool reset = false) {
        DiscordControls.ClearDetails(reset);
        DiscordControls.UpdatePresence();
    }

    public void SetTime(int time, bool countdown = false) {
        DiscordControls.SetTime(time, countdown);
        DiscordControls.UpdatePresence();
    }

    public void ClearTime(bool reset = false) {
        DiscordControls.ClearTime(reset);
        DiscordControls.UpdatePresence();
    }
}