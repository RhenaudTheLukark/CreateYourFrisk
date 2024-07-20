function EventPage1()
	Audio.PlaySound("glitch")
	Event.SetPage("MusicWarper", 2)
end

function EventPage2()
	local vBegin = 2040
	local vEnd = 3320
	local volume = 1 - math.max(0, math.min(1, (Event.GetPosition("Player")[1] - vBegin) / (vEnd - vBegin)))
	NewAudio.SetVolume("StaticKeptAudio", volume)
end

function EventPage3()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("MusicWarper", -1)
	else                                	    Event.SetPage("MusicWarper", 1)
	end
end