local currentChar = 1
local phase = 0
local background = nil
local player = nil
local chars = { {}, {}, {} }

local animationKeys = { "FriskUT", "CharaOW", "MonsterKidOW" }
local animationCount = 0
local friskWait = 1

local fadeCount = 1
local posX
local posY

function EventPage0()
    NewAudio.CreateChannel("CharSelect")
    Screen.DispImg("px", 1, 320, 240, 255, 255, 255, 255)
    background = Event.GetSprite("Image1")
    background.Scale(640, 480)
    Screen.DispImg("px", 11, 320, 240, 255, 255, 255, 255)
    foreground = Event.GetSprite("Image11")
    foreground.Scale(640, 480)
    player = Event.GetSprite("Player")
    posX = player.absx
    posY = player.absy
    player.absx = 92218
    player.absy = -34937
end

function EventPage1()
    if phase == 0 then
        phase = 1
        Screen.DispImg("window_border", 2, 160, 240, 255, 255, 255, 255)
        Screen.DispImg("window_back", 3, 160, 240, 255, 255, 255, 255)
        Screen.DispImg("window_border", 4, 320, 240, 255, 255, 255, 255)
        Screen.DispImg("window_back", 5, 320, 240, 255, 255, 255, 255)
        Screen.DispImg("window_border", 6, 480, 240, 255, 255, 255, 255)
        Screen.DispImg("window_back", 7, 480, 240, 255, 255, 255, 255)
        chars[1]["border"] = Event.GetSprite("Image2")
        chars[2]["border"] = Event.GetSprite("Image4")
        chars[3]["border"] = Event.GetSprite("Image6")
        chars[1]["back"] = Event.GetSprite("Image3")
        chars[2]["back"] = Event.GetSprite("Image5")
        chars[3]["back"] = Event.GetSprite("Image7")
        Screen.DispImg("FriskUT/1", 8, 160, 240, 255, 255, 255, 255)
        Screen.DispImg("CharaOW/1", 9, 320, 240, 255, 255, 255, 255)
        Screen.DispImg("MonsterKidOW/1", 10, 480, 240, 255, 255, 255, 255)
        chars[1]["sprite"] = Event.GetSprite("Image8")
        chars[2]["sprite"] = Event.GetSprite("Image9")
        chars[3]["sprite"] = Event.GetSprite("Image10")
        chars[1]["sprite"].loopmode = "ONESHOT"
        chars[2]["sprite"].loopmode = "ONESHOT"
        chars[3]["sprite"].loopmode = "ONESHOT"
        foreground.MoveAbove(chars[3]["sprite"])
        ChangeTarget(1, false)
    elseif phase == 1 then
        foreground.alpha = fadeCount
        if fadeCount <= 0 then phase = 2 end
    elseif phase == 2 then
        if Input.Right == 1 then
            ChangeTarget(currentChar % 3 + 1)
        elseif Input.Left == 1 then
            local temp = (currentChar + 2) % 3
            ChangeTarget(temp != 0 and temp or 3)
        elseif Input.Confirm == 1 then
            chars[currentChar]["back"].color = {1, 1, 0}
            Audio.PlaySound("menuconfirm")
            phase = 3
        end
    elseif phase == 3 then
        if currentChar == 1 then
            Event.SetAnimHeader("Player", "")
            chars[currentChar]["sprite"].StopAnimation()
            chars[currentChar]["sprite"].Set(animationKeys[currentChar] .. "/1")
        elseif currentChar == 2 then
            Event.SetAnimHeader("Player", "Chara")
            chars[currentChar]["sprite"].loopmode = "LOOP"
            chars[currentChar]["sprite"].SetAnimation({"CharaOW/l1", "CharaOW/l2", "CharaOW/l3" }, 1/8)
            NewAudio.PlaySound("CharSelect", "Laugh")
        elseif currentChar == 3 then
            Event.SetAnimHeader("Player", "MK")
            chars[currentChar]["sprite"].SetAnimation({"MonsterKidOW/f0", "MonsterKidOW/f1", "MonsterKidOW/f2",  "MonsterKidOW/f2",  "MonsterKidOW/f2",
                                                       "MonsterKidOW/f3", "MonsterKidOW/f4", "MonsterKidOW/f5",  "MonsterKidOW/f5",  "MonsterKidOW/f5",
                                                       "MonsterKidOW/f5", "MonsterKidOW/f5", "MonsterKidOW/f5",  "MonsterKidOW/f6",  "MonsterKidOW/f7",
                                                       "MonsterKidOW/f8", "MonsterKidOW/f9", "MonsterKidOW/f10", "MonsterKidOW/f11"                     }, 0.1)
        end
        phase = 4
    elseif phase == 4 then
        if currentChar == 1 then
            friskWait = friskWait - Time.dt
            if friskWait <= 0 then                            phase = 5 end
        elseif currentChar == 2 then
            if NewAudio.isStopped("CharSelect") then          phase = 5 end
        elseif currentChar == 3 then
            if chars[currentChar]["sprite"].animcomplete then phase = 5 end
        end
    elseif phase == 5 then
        fadeCount = 3
        phase = 6
    elseif phase == 6 then
        player.absx = posX
        player.absy = posY
        chars[1]["sprite"].alpha = fadeCount - 2
        chars[2]["sprite"].alpha = fadeCount - 2
        chars[3]["sprite"].alpha = fadeCount - 2
        chars[1]["border"].alpha = fadeCount - 1
        chars[2]["border"].alpha = fadeCount - 1
        chars[3]["border"].alpha = fadeCount - 1
        chars[1]["back"].alpha = fadeCount - 1
        chars[2]["back"].alpha = fadeCount - 1
        chars[3]["back"].alpha = fadeCount - 1
        background.alpha = fadeCount
        if fadeCount <= 0 then Exit() end
    end
    
    if phase > 0 and phase < 3 then
        if chars[currentChar]["sprite"].animcomplete then
            animationCount = (animationCount + 1) % 8
            chars[currentChar]["sprite"].SetAnimation({ animationKeys[currentChar].."/"..math.floor(animationCount/2)*4,   animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+1, 
                                                        animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+2, animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+3 }, 0.25)
        end
    end
    if fadeCount > 0 then
        fadeCount = fadeCount - 4 * Time.dt
    end
end

function ChangeTarget(number, sound)
    chars[currentChar]["sprite"].StopAnimation()
    chars[currentChar]["border"].color = {1, 1, 1}
    animationCount = 0
    if sound then Audio.PlaySound("menumove") end
    currentChar = number
    chars[currentChar]["sprite"].SetAnimation({ animationKeys[currentChar].."/"..math.floor(animationCount/2)*4,   animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+1, 
                                                animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+2, animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+3 }, 0.25)
    chars[currentChar]["border"].color = {1, 0, 0}
end

function Exit()
    Screen.SupprImg(1)
    Screen.SupprImg(2)
    Screen.SupprImg(3)
    Screen.SupprImg(4)
    Screen.SupprImg(5)
    Screen.SupprImg(6)
    Screen.SupprImg(7)
    Screen.SupprImg(8)
    Screen.SupprImg(9)
    Screen.SupprImg(10)
    Screen.SupprImg(11)
    Event.SetPage(Event.GetName(), -1)
end