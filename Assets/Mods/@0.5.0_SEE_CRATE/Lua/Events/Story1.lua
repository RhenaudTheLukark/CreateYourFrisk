function EventPage1()
    SetRealGlobal("ow", true)
	Event.SetPage("Story1", -1)
	General.SetBattle("Story1", "fast", true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("Story1", -1)
	else                                	    Event.SetPage("Story1", 1)
	end
end