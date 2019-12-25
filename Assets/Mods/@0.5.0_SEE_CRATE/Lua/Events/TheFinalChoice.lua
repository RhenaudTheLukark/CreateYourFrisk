function EventPage1()
    General.Wait(30)
    Event.CenterOnCamera("TheFinalChoice", 2, true)
    General.Wait(30)
    local sprite = Event.GetSprite("TheFinalChoice")
	sprite.Set("Punderbolt/PunderLeft1")
    General.Wait(30)
	General.SetDialog({"[voice:punderbolt]Oh![w:10] There you are!"}, true, {"Punder/normal"})
	Event.SetPage("TheFinalChoice", -1)
	General.SetBattle("TheFinalChoice", "fast", true)
end

function EventPage2()
    if GetAlMightyGlobal("CrateYourFrisk") then Event.SetPage("TheFinalChoice", -1)
	else                                	    Event.SetPage("TheFinalChoice", 1)
	end
end