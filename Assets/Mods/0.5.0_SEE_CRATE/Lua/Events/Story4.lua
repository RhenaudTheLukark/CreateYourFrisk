function EventPage1()
	SetEventPage("Story4", -1)
	SetBattle("Story4", true, true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then SetEventPage("Story4", -1)
	else                                	    SetEventPage("Story4", 1)
	end
end