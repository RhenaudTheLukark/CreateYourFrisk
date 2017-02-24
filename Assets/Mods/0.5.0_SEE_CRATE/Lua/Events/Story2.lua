function EventPage1()
	SetEventPage("Story2", -1)
	SetBattle("Story2", true, true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then SetEventPage("Story2", -1)
	else                                	    SetEventPage("Story2", 1)
	end
end