function EventPage0()
    local spriteTest = Event.GetSprite("Save")
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
	General.SetDialog({"[health:Max]That weird kangaroo being staying at your left...", 
                       "Seeing that he won't move by an inch feels you with determination."}, true)
    General.Save()
end