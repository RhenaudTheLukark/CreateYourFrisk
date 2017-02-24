function EventPage1()
	SetEventPage("Story1", -1)
	SetBattle("Story1", true, true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then SetEventPage("Story1", -1)
	else                                	    SetEventPage("Story1", 1)
	end
end