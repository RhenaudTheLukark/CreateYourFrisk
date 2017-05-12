function EventPage1()
    General.TitleScreen()
    --[[
    local spriteTest = Event.GetSprite("Punder1")
    local playerpos = Event.GetPosition("Player") 
    local eventpos = Event.GetPosition("Punder1")
    local dir
    local diff
    dir, diff = calcDirAndDiff(eventpos, playerpos)
    local text = ""
    if Event.GetAnimHeader("Player") == "MK" then            
        text = "Hello there little buddy!"
    elseif Event.GetAnimHeader("Player") == "Chara" then    
        local tempPunderX = Event.GetPosition("Punder1")[1]
        Event.MoveToPoint(Event.GetName(), diff[1] > 0 and eventpos[1] + 60 or eventpos[1] - 60, eventpos[2])
        eventpos = Event.GetPosition("Punder1")
        if tempPunderX == eventpos[1] then 
            text = "What are you doing? [w:20]\nBack off!" 
        else 
            text = "Hey[waitall:5]...[waitall:1]you look kinda menacing[waitall:5]...[waitall:1][w:30]\nBe good, [w:20]alright?"
        end
    elseif Event.GetAnimHeader("Player") == "CharaTad" then  
        text = "Are you alright? [w:20]\nYou seem lost[waitall:5]..."
    elseif Event.GetAnimHeader("Player") == "Asriel" then    
        text = "Oh hi kid! [w:20]You're cute, [w:20]you know that?"
    else                                                     
        text = "Hey, [w:20]how's going?"
    end
    dir, diff = calcDirAndDiff(Event.GetPosition("Punder1"), Event.GetPosition("Player"))
    Event.SetDirection(Event.GetName(), dir)
    General.SetDialog({"[voice:punderbolt]" .. text}, true, {"pundermug"})]]
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