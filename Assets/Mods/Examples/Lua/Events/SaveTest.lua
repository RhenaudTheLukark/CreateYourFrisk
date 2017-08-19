function EventPage0() --First event function launched
    spriteTest = Event.GetSprite(Event.GetName())
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
	General.SetDialog({"[health:Max]Testing such a magnificent engine fills you with [color:ff0000]determination.", 
                       "HP restored."}, true)
    General.Save()
end