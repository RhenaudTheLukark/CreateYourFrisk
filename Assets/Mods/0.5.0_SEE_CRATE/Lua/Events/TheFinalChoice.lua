function EventPage1()
    Wait(30)
    CenterEventOnCamera("TheFinalChoice", 2, true)
    Wait(30)
    GetSpriteOfEvent("TheFinalChoice")
	if GetRealGlobal("GetSpriteOfEventTheFinalChoice") != null then
	    GetRealGlobal("GetSpriteOfEventTheFinalChoice").Set("Punderbolt/PunderLeft1")
	end
    Wait(30)
	SetDialog({"[voice:punderbolt]Oh! There you are!"}, true, {"pundermug"})
	SetEventPage("TheFinalChoice", -1)
	SetBattle("TheFinalChoice", true, true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then SetEventPage("TheFinalChoice", -1)
	else                                	    SetEventPage("TheFinalChoice", 1)
	end
end