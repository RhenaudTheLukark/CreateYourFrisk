function EventPage1()
	SetEventPage("Story5", -1)
	SetBattle("Story5", true, true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then SetEventPage("Story5", -1)
	else                                	    SetEventPage("Story5", 1)
	end
end