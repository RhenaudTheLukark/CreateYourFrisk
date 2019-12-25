--From normal to knockout: X + 56, Y + 5, Rot.Z = 90
--From event to image: Pivot = 0.75, ImageX = 168.5, ImageY = -47, Rotation = -90 --> -45
--From knockout to fall: Pivot = 0.5, ImageX = 1060+176, ImageY = 707-89, Rot.Z = -60 --> -15
--Final destination: ImageY = -300

--Camera: 1060, 707
function EventPage0()
    local punderSprite = Event.GetSprite(Event.GetName())
    if GetRealGlobal("CYFInternalCross2") then
        Event.Remove(Event.GetName() .. " (1)")
        Event.Remove(Event.GetName())
    elseif Event.GetAnimHeader("Player") ~= "Chara" then
        Event.SetPage(Event.GetName(), 1)
        if Event.Exists(Event.GetName() .. " (1)") then
            Event.Remove(Event.GetName() .. " (1)")
        end
    else
        Event.SetPage(Event.GetName(), 2)
    end
end

function EventPage2()
    local playerSprite = Event.GetSprite("Player")
    local punderSprite = Event.GetSprite(Event.GetName())
    local maskSprite = Event.GetSprite(Event.GetName() .. " (1)")
    maskSprite.loopmode = "ONESHOT"
    NewAudio.CreateChannel("temp")
    Event.IgnoreCollision(Event.GetName(), true)
    Event.IgnoreCollision(Event.GetName() .. " (1)", true)
    Event.MoveToPoint("Player", 1020, 680, true)
    Event.SetDirection("Player", 6)
    Event.SetDirection(Event.GetName(), 4)
    General.Wait(30)
    General.SetDialog({"[noskip][voice:punderbolt]Hello th[mugshot:Punder/intimidated][waitall:2]ere... [w:25][waitall:1]May I help you?"}, true, {"punder/normal"})
    for i = 1, 30 do
        Audio.Volume((30 - i) / 30)
        General.Wait(1)
    end
    Audio.Stop()
    Audio.Volume(1)
    Event.MoveToPoint("Player", 1060, 680, true, false)
    General.Wait(3)
    Event.MoveToPoint(Event.GetName(), 1120, 680, true, false)
    Event.SetDirection(Event.GetName(), 4)
    General.SetDialog({"[noskip][voice:punderbolt]Hey! [w:25]Back off!", "[noskip][voice:punderbolt]What are you doing?!"}, true, {"Punder/angryIntimidated", "Punder/shocked"})
    playerSpeed = Event.GetSpeed("Player")
    Event.SetSpeed("Player", 6)
    General.Wait(60)

    Event.MoveToPoint("Player", 1120, 680, true, false)
    General.Wait(5)
    NewAudio.PlaySound("temp", "Secret/punch")
    Event.SetAnimHeader(Event.GetName(), "Knockout")
    punderSprite.Set("Overworld/Punder/Secret/knockout")
    punderSprite.rotation = 90
    Event.Teleport(Event.GetName(), 1176, 723)

    for i = 1, 60 do
        punderSprite.rotation = punderSprite.rotation - 3
        General.Wait(1)
    end
    Event.MoveToPoint(Event.GetName(), 1176, 660, true)
    -- Event.SetDirection(Event.GetName(), 4)

    Screen.DispImg("Overworld/Punder/Secret/knockout", 1, -500, -500)
    local imgSprite = Event.GetSprite("Image1")
    imgSprite.rotation = -90
    imgSprite.SetPivot(.5, .75)
    Event.Teleport("Image1", 1230.5, 664)
    Event.SetAnimHeader(Event.GetName(), "NoAnim")
    Event.MoveToPoint("Image1", 1238.5, 616, true, false)

    General.Wait(1)
    for i = 1, 15 do
        imgSprite.rotation = imgSprite.rotation + 3
        General.Wait(1)
    end

    imgSprite.Set("Overworld/Punder/Secret/fall")
    imgSprite.SetPivot(.5, .5)
    imgSprite.rotation = -60
    Event.Teleport("Image1", 1230, 619)
    Event.MoveToPoint("Image1", 1230, 400, true, false)

    for i = 1, 15 do
        imgSprite.rotation = imgSprite.rotation + 3
        General.Wait(1)
    end

    local imgPos
    repeat
        imgPos = Event.GetPosition("Image1")
        General.Wait(1)
    until imgPos[2] == 400

    General.Wait(60)
    Event.SetDirection("Player", 2)

    General.Wait(60)
    local playerPos = Event.GetPosition("Player")
    playerSprite.alpha = 0
    Event.Teleport(Event.GetName() .. " (1)", playerPos[1], playerPos[2])
    Event.SetAnimHeader(Event.GetName() .. " (1)", "Glitch")
    NewAudio.PlaySound("temp", "Secret/noise")

    General.Wait(1)
    while not maskSprite.animcomplete do
        General.Wait(1)
    end

    maskSprite.Set("FriskUT/1")
    General.Wait(120)
    maskSprite.Set("FriskUT/Glitch/gg")
    General.Wait(120)

    Screen.SetTone(true, true, 0, 0, 0, 255)
    Event.Remove("Image1")
    General.Wait(30)

    Event.IgnoreCollision(Event.GetName(), false)
    Event.SetAnimHeader(Event.GetName(), "")
    SetRealGlobal("CYFInternalCross2", true)
    SetRealGlobal("CYFInternalCharacterSelected", false)
    NewAudio.DestroyChannel("temp")
    Event.SetSpeed("Player", playerSpeed)
    Player.Teleport("test2", 320, 200, 2, false)
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
    elseif Event.GetAnimHeader("Player") == "Chara" then   --Impossible to reach
        local tempPunderX = Event.GetPosition(Event.GetName())[1]
        Event.MoveToPoint(Event.GetName(), diff[1] > 0 and eventpos[1] + 60 or eventpos[1] - 60, eventpos[2])
        eventpos = Event.GetPosition(Event.GetName())
        if tempPunderX == eventpos[1] then
            text = "What are you doing? [w:25]\nBack off!"
        else
            text = "Hey...[w:25]you look kinda menacing...[w:25]\nBe good, [w:15]alright?"
        end
        mugshot = "punder/intimidated"
    elseif Event.GetAnimHeader("Player") == "Asriel" then
        text = "Oh hi kid! [w:25]You're cute, [w:15]you know that?"
        mugshot = "punder/veryHappy"
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