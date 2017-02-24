function EventPage1()
	SetEventPage("Story3", -1)
	SetBattle("Story3", true, true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then SetEventPage("Story3", -1)
	else                                	    SetEventPage("Story3", 1)
	end
end