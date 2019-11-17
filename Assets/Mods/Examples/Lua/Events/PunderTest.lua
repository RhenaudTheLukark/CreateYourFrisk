beforeMovement = math.random(60, 180)
punderSprite = nil
lastPosX = 0
lastPosY = 0

eventName = nil

function EventPage0() -- First event function launched
    eventName = Event.GetName()
    -- Chara player choice has been locked
    if GetRealGlobal("CYFInternalCross2") then
        Event.Remove(eventName)
    else
        Event.SetPage(eventName, 2)
        punderSprite = Event.GetSprite(eventName)
        lastPosX = punderSprite.x
        lastPosY = punderSprite.y
    end
end

function EventPage1()
    -- Turn toward player
    dir = calcDir(Event.GetPosition(eventName), Event.GetPosition("Player"))
    Event.SetDirection(eventName, dir)
    local animHeader = Event.GetAnimHeader(eventName)
    local text = animHeader == "" and "Where am I???" or "I still don't know where I am,[w:10] but I found cool sunglasses!"
    local faceSprite = animHeader == "" and "Punder/intimidated" or "Punder/sun"
    General.SetDialog("[voice:punderbolt]" .. text, true, faceSprite)
end

function EventPage2() -- Coroutine
    if Event.GetPage(eventName) == 2 then Event.SetPage(eventName, 1) end
    if lastPosX == punderSprite.x and lastPosY == punderSprite.y then
        beforeMovement = beforeMovement - 1
        if beforeMovement == 0 then
            beforeMovement = math.random(60, 180)
            x = math.random(-1, 1)
            y = math.random(-1, 1)
            local pos = Event.GetPosition(eventName)
            Event.MoveToPoint(eventName, math.min(math.max(pos[1] + 20 * x, 365), 455), math.min(math.max(pos[2] + 20 * y, 250), 340), false, false)
        end
    end
    lastPosX = punderSprite.x
    lastPosY = punderSprite.y
end

--The name is pretty straightforward
function calcDir(vect1, vect2)
    local diff = { vect1[1] - vect2[1], vect1[2] - vect2[2] }
    local angle = (math.atan2(diff[1], diff[2]) + (math.pi*2)) % (math.pi*2)
    local dir = 2
    if     angle > math.pi/4   and angle <= 3*math.pi/4 then dir = 4
    elseif angle > 3*math.pi/4 and angle <= 5*math.pi/4 then dir = 8
    elseif angle > 5*math.pi/4 and angle <= 7*math.pi/4 then dir = 6
    end
    return dir
end