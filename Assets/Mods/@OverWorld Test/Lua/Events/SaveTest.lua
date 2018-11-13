function EventPage0()
    local spriteTest = Event.GetSprite(Event.GetName())
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
    General.SetDialog({"[health:Max]That weird kangaroo staying in place at your left...", 
                       "Seeing that he won't move by an inch fills you with determination."}, true)
    General.Save()
end