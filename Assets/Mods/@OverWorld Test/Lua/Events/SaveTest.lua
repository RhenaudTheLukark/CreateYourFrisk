function EventPage0()
    local spriteTest = Event.GetSprite(Event.GetName())
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
    if GetRealGlobal("CYFInternalCross2") then
        local count = 5
        for i = 1, 5 do
            if GetRealGlobal("CYFInternalCross" .. i) then
                count = count - 1
            end
        end
        General.SetDialog({"[health:Max][color:ff0000]" .. count .. " left."}, true)
    else
        General.SetDialog({"[health:Max]That weird kangaroo staying in place at your left...", 
                           "Seeing that he won't move by an inch fills you with determination."}, true)
    end
    General.Save()
end