function EventPage0()
    local spriteTest = Event.GetSprite(Event.GetName())
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
	General.SetDialog({"[health:Max]Being at the other side of this bridge...\nIt fills you with determination."}, true)
    General.Save()
end