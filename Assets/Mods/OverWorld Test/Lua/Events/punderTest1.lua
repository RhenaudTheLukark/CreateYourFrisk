function EventPage1()
    local spriteTest = Event.GetSprite("punder1")
    local playerpos = Event.GetPosition("Player")
    local eventpos = Event.GetPosition("Punder1")
    local diff = { eventpos[1] - playerpos[1], eventpos[2] - playerpos[2] }
    local angle = (math.atan2(diff[1], diff[2]) + (math.pi*2)) % (math.pi*2)
    --DEBUG(tostring(angle/math.pi) .. "π")
    local dirword = "Down"
    if     angle > math.pi/4   and angle <= 3*math.pi/4 then dirword = "Left"
    elseif angle > 3*math.pi/4 and angle <= 5*math.pi/4 then dirword = "Up"
    elseif angle > 5*math.pi/4 and angle <= 7*math.pi/4 then dirword = "Right"
    end
	spriteTest.Set("Overworld/Punder" .. dirword .. "1")
	General.SetDialog({"[voice:punderbolt]Hey, [w:20]how's going?"}, true, {"pundermug"})
	spriteTest.Set("Overworld/PunderDown1")
end