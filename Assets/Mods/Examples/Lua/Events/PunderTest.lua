local beforeMovement = math.random(60, 180)
local spriteTest
local lastPosX
local lastPosY

function EventPage0() --First event function launched
    Event.SetPage(Event.GetName(), 2)
    spriteTest = Event.GetSprite(Event.GetName())
    lastPosX = spriteTest.x
    lastPosY = spriteTest.y
end

function EventPage1()
    --Turn toward player
    dir, diff = calcDirAndDiff(Event.GetPosition(Event.GetName()), Event.GetPosition("Player"))
    Event.SetDirection(Event.GetName(), dir)
    General.SetDialog({"[voice:punderbolt]Where am I???"}, true, {"punderIntimidated"})
end

function EventPage2() --Coroutine
    if Event.GetPage(Event.GetName()) == 2 then Event.SetPage(Event.GetName(), 1) end
    if lastPosX == spriteTest.x and lastPosY == spriteTest.y then
        beforeMovement = beforeMovement - 1
    end
    lastPosX = spriteTest.x 
    lastPosY = spriteTest.y
    if beforeMovement == 0 then
        beforeMovement = math.random(60, 180)
        x = math.random(-1, 1)
        y = math.random(-1, 1)
        local pos = Event.GetPosition(Event.GetName())
        Event.MoveToPoint(Event.GetName(), pos[1] + 20 * x, pos[2] + 20 * y, false, false)
    end
end

--The name is pretty straightforward
function calcDirAndDiff(vect1, vect2)
    local diff = { vect1[1] - vect2[1], vect1[2] - vect2[2] }
    local angle = (math.atan2(diff[1], diff[2]) + (math.pi*2)) % (math.pi*2)
    local dir = 2
    if     angle > math.pi/4   and angle <= 3*math.pi/4 then dir = 4
    elseif angle > 3*math.pi/4 and angle <= 5*math.pi/4 then dir = 8
    elseif angle > 5*math.pi/4 and angle <= 7*math.pi/4 then dir = 6
    end
    return dir, diff
end