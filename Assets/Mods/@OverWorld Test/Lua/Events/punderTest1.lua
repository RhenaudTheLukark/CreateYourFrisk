function EventPage0() 
    if GetRealGlobal("CYFInternalCross2") then
        Event.Remove(Event.GetName())
    end
end

function EventPage1()
    --General.TitleScreen()
    local spriteTest = Event.GetSprite(Event.GetName())
    local playerpos = Event.GetPosition("Player") 
    local eventpos = Event.GetPosition(Event.GetName())
    local dir
    local diff
    dir, diff = calcDirAndDiff(eventpos, playerpos)
    local text = ""
    local mugshot = "Punder/normal"
    if Event.GetAnimHeader("Player") == "MK" then            
        text = "Hello there little buddy!"
        mugshot = "Punder/veryHappy"
    elseif Event.GetAnimHeader("Player") == "Chara" then    
        local tempPunderX = Event.GetPosition(Event.GetName())[1]
        Event.MoveToPoint(Event.GetName(), diff[1] > 0 and eventpos[1] + 60 or eventpos[1] - 60, eventpos[2])
        eventpos = Event.GetPosition(Event.GetName())
        if tempPunderX == eventpos[1] then 
            text = "What are you doing? [w:25]\nBack off!" 
        else 
            text = "Hey...[w:25]you look kinda menacing...[w:25]\nBe good, [w:15]alright?"
        end
        mugshot = "Punder/intimidated"
    elseif Event.GetAnimHeader("Player") == "Asriel" then    
        text = "Oh hi kid! [w:25]You're cute, [w:15]you know that?"
        mugshot = "Punder/veryHappy"
    else                                                     
        text = "Hey, [w:15]how's it going?"
    end
    dir, diff = calcDirAndDiff(Event.GetPosition(Event.GetName()), Event.GetPosition("Player"))
    Event.SetDirection(Event.GetName(), dir)
    General.SetDialog({"[voice:punderbolt]" .. text}, true, {mugshot})
end

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