local beforeMovement = math.random(30, 120)
local spriteTest
local lastPosX; local lastPosY

function EventPage0()
    Event.SetPage(Event.GetName(), 2)
    spriteTest = Event.GetSprite(Event.GetName())
    lastPosX = spriteTest.x
    lastPosY = spriteTest.y
    spriteTest.SetAnimation({"SavePoint/0", "SavePoint/1"}, 0.2)
end

function EventPage1()
	General.SetDialog({"[health:Max]Testing such a magnificent project fills you with [color:ff0000]determination.", 
                       "HP restored."}, true)
    General.Save()
end

function EventPage2()
    --DEBUG(tostring(eventName))
    if Event.GetPage(Event.GetName()) == 2 then Event.SetPage(Event.GetName(), 1) end
    if lastPosX == spriteTest.x and lastPosY == spriteTest.y then
        beforeMovement = beforeMovement - 1
    end
    --DEBUG("beforeMovement = " .. tostring(beforeMovement))
    lastPosX = spriteTest.x 
    lastPosY = spriteTest.y
    if beforeMovement == 0 then
        beforeMovement = math.random(30, 120)
        x = math.random(-1, 1)
        y = math.random(-1, 1)
        local pos = Event.GetPosition(Event.GetName())
        Event.MoveToPoint(Event.GetName(), pos[1] + 20 * x, pos[2] + 20 * y)
    end
end