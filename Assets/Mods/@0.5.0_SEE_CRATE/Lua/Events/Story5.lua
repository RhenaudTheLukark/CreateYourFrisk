function EventPage1()
	Event.SetPage("Story5", -1)
	General.SetBattle("Story5", "fast", true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("Story5", -1)
	else                                	    Event.SetPage("Story5", 1)
	end
end