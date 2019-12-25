function EventPage0()
    local spriteTest = Event.GetSprite(Event.GetName())
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, .2)
end

function EventPage1()
    General.SetDialog({"[health:Max]This large stretch of snow is so beautiful to the eye...", 
                       "[waitall:5]...[waitall:1][w:40]Seeing that the background is finally here fills you with determination."}, true)
    General.Save()
end