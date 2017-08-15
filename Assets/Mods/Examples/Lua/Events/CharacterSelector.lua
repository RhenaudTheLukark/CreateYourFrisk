local currentChar = 1
local phase = 0
local background = nil
local player = nil
local chars = {}

local animationKeys = { "FriskUT", "CharaOW", "MonsterKidOW", "BoosterOW", "AsrielOW" }
local animationCount = 0
local friskWait = 1

local fadeCount = 1

function EventPage0()
    NewAudio.CreateChannel("CharSelect")
    Screen.DispImg("px", 1, 320, 240, 255, 255, 255, 255)
    background = Event.GetSprite("Image1")
    background.Scale(640, 480)
    Screen.DispImg("px", #animationKeys * 3 + 2, 320, 240, 255, 255, 255, 255)
    foreground = Event.GetSprite("Image17")
    foreground.Scale(640, 480)
    player = Event.GetSprite("Player")
end

function EventPage1()
    Player.CanMove(false)
    if phase == 0 then
        phase = 1
        Screen.DispImg("window_border", 2, 160, 160, 255, 255, 255, 255)
        Screen.DispImg("window_back", 3, 160, 160, 255, 255, 255, 255)
        Screen.DispImg("window_border", 4, 320, 160, 255, 255, 255, 255)
        Screen.DispImg("window_back", 5, 320, 160, 255, 255, 255, 255)
        Screen.DispImg("window_border", 6, 480, 160, 255, 255, 255, 255)
        Screen.DispImg("window_back", 7, 480, 160, 255, 255, 255, 255)
        Screen.DispImg("window_border", 8, 240, 320, 255, 255, 255, 255)
        Screen.DispImg("window_back", 9, 240, 320, 255, 255, 255, 255)
        Screen.DispImg("window_border", 10, 400, 320, 255, 255, 255, 255)
        Screen.DispImg("window_back", 11, 400, 320, 255, 255, 255, 255)
        Screen.DispImg(animationKeys[1] .. "/1", 12, 160, 160, 255, 255, 255, 255)
        Screen.DispImg(animationKeys[2] .. "/1", 13, 320, 160, 255, 255, 255, 255)
        Screen.DispImg(animationKeys[3] .. "/1", 14, 480, 160, 255, 255, 255, 255)
        Screen.DispImg(animationKeys[4] .. "/1", 15, 240, 320, 255, 255, 255, 255)
        Screen.DispImg(animationKeys[5] .. "/1", 16, 400, 320, 255, 255, 255, 255)
        for i = 1, #animationKeys do 
            chars[i] = {}
            chars[i]["border"] = Event.GetSprite("Image" .. (i * 2))
            chars[i]["back"] = Event.GetSprite("Image" .. (i * 2 + 1))
            chars[i]["sprite"] = Event.GetSprite("Image" .. (#animationKeys * 2 + 1 + i))
            chars[i]["sprite"].loopmode = "ONESHOT"
        end
        foreground.MoveAbove(chars[#chars]["sprite"])
        ChangeTarget(1, false)
    elseif phase == 1 then
        foreground.alpha = fadeCount
        if fadeCount <= 0 then phase = 2 end
    elseif phase == 2 then
        if Input.Right == 1 then
            if currentChar < 4 then ChangeTarget(currentChar % 3 + 1)
            else                    ChangeTarget((currentChar - 3) % 2 + 1 + 3) 
            end
        elseif Input.Left == 1 then
            local temp = (currentChar + 2) % 3
            if currentChar < 4 then ChangeTarget(temp != 0 and temp or 3)
            else                    ChangeTarget((currentChar - 3) % 2 + 1 + 3) 
            end
        elseif Input.Down == 1 or Input.Up == 1 then
            local temp = (currentChar + (currentChar > 3 and 2 or 3)) % 5
            ChangeTarget(currentChar != 3 and (temp != 0 and temp or 5) or 5)
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
        elseif currentChar == 4 then
            if count == nil then
				count = 0
                inProgress = true
                Event.SetAnimHeader("Player", "Booster")
                chars[currentChar]["sprite"].StopAnimation()
                chars[currentChar]["sprite"].Set(animationKeys[currentChar] .. "/1")
                chars[currentChar]["cross"].Set("ut-heart")
                chars[currentChar]["cross"].y = chars[currentChar]["cross"].y - 8
                chars[currentChar]["cross"].x = chars[currentChar]["cross"].x + 31
                chars[currentChar]["cross"].color32 = {0, 60, 255}
            elseif (count - 30) % 12 == 0 and count < 60 and count >= 30 then
                if show then
                    chars[currentChar]["cross"].alpha = 0
                else
                    chars[currentChar]["cross"].alpha = 1
                    NewAudio.PlaySound("CharSelect", "BeginBattle2")
                end
                show = not show
            elseif count >= 90 and count <= 150 then
                if count == 90 then
                    chars[currentChar]["sprite"].Set(animationKeys[currentChar] .. "/j1")
                    chars[currentChar]["sprite"].x = chars[currentChar]["sprite"].x + .5
                    --chars[currentChar]["cross"].x = chars[currentChar]["cross"].x + .5
                end
                if count == 120 then
                    chars[currentChar]["sprite"].Set(animationKeys[currentChar] .. "/j0")
                end
                chars[currentChar]["sprite"].y = chars[currentChar]["sprite"].y + vely
                --chars[currentChar]["cross"].y = chars[currentChar]["cross"].y + vely
                vely = vely - 1/15
                if count == 150 then
                    chars[currentChar]["sprite"].Set(animationKeys[currentChar] .. "/1")
                end
            elseif count == 165 then
                phase = 5
            end
            count = count + 1
            return
        elseif currentChar == 5 then
            Event.SetAnimHeader("Player", "Asriel")
            chars[currentChar]["sprite"].SetPivot(.5, 0)
            chars[currentChar]["sprite"].absy = chars[currentChar]["sprite"].absy - 28
            chars[currentChar]["sprite"].SetAnimation({"AsrielOW/s0", "AsrielOW/s1", "AsrielOW/s2", "AsrielOW/s3", "AsrielOW/s4", "AsrielOW/s4", "AsrielOW/s5", "AsrielOW/s6", 
                                                       "AsrielOW/s7", "AsrielOW/s8", "AsrielOW/s9", "AsrielOW/s6", "AsrielOW/s7", "AsrielOW/s8", "AsrielOW/s9", "AsrielOW/s10-1", 
                                                       "AsrielOW/s10-2", "AsrielOW/s11", "AsrielOW/s12", "AsrielOW/s12", "AsrielOW/s13", "AsrielOW/s14", "AsrielOW/s15", 
                                                       "AsrielOW/s15", "AsrielOW/s15", "AsrielOW/s16", "AsrielOW/s17", "AsrielOW/s18" }, 0.2)
        end
        phase = 4
    elseif phase == 4 then
        if currentChar == 1 then
            friskWait = friskWait - Time.dt
            if friskWait <= 0 then                             phase = 5 end
        elseif currentChar == 2 then
            if NewAudio.isStopped("CharSelect")  then          phase = 5 end
        elseif currentChar >= 3 and currentChar <= 5 then
            if chars[currentChar]["sprite"].animcomplete then  phase = 5 end
        end
    elseif phase == 5 then
        fadeCount = 3
        phase = 6
    elseif phase == 6 then
        for i = 1, #chars do
            chars[i]["sprite"].alpha = fadeCount - 2
            chars[i]["border"].alpha = fadeCount - 1
            chars[i]["back"].alpha = fadeCount - 1
        end
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
    if fadeCount > 0 then fadeCount = fadeCount - 4 * Time.dt end
end

function ChangeTarget(number, sound)
    chars[currentChar]["sprite"].StopAnimation()
    chars[currentChar]["sprite"].Set(animationKeys[currentChar] .. "/1")
    chars[currentChar]["border"].color = {1, 1, 1}
    animationCount = 0
    if sound then Audio.PlaySound("menumove") end
    currentChar = number
    chars[currentChar]["sprite"].SetAnimation({ animationKeys[currentChar].."/"..math.floor(animationCount/2)*4,   animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+1, 
                                                animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+2, animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+3 }, 0.25)
    chars[currentChar]["border"].color = {1, 0, 0}
end

function Exit()
    for i = 1, #animationKeys * 3 + 2 do Screen.SupprImg(i) end
    Event.SetPage(Event.GetName(), -1)
    NewAudio.DestroyChannel("CharSelect")
    Player.CanMove(true)
end