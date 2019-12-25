function EventPage1()
	Event.SetPage("Story3", -1)
	General.SetBattle("Story3", "fast", true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("Story3", -1)
	else                                	    Event.SetPage("Story3", 1)
	end
end