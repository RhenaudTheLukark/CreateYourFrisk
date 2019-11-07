function EventPage1()
	Event.SetPage("Story4", -1)
	General.SetBattle("Story4", "fast", true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("Story4", -1)
	else                                	    Event.SetPage("Story4", 1)
	end
end