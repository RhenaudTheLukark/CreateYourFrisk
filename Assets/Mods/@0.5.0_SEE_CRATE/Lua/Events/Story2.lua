function EventPage1()
	Event.SetPage("Story2", -1)
	General.SetBattle("Story2", "fast", true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("Story2", -1)
	else                                	    Event.SetPage("Story2", 1)
	end
end