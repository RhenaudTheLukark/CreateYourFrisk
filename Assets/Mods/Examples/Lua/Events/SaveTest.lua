function EventPage0() -- First event function launched
    local spriteTest = Event.GetSprite(Event.GetName())
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
    -- Chara player choice has been locked
    if GetRealGlobal("CYFInternalCross2") then
        local count = 5
        for i = 1, 5 do
            if GetRealGlobal("CYFInternalCross" .. i) then
                count = count - 1
            end
        end
        General.SetDialog({"[health:Max][color:ff0000]" .. count .. " left."}, true)
    else
        General.SetDialog({"[health:Max]Testing such a magnificent engine fills you with [color:ff0000]determination.",
                           "HP restored."}, true)
    end
    General.Save()
end