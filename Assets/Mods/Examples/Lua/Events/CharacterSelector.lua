--Oh hi! Welcome to CYF's 0.6's secret, part 1 and 2! It's a good thing you can't discover the 3rd part like this, though.
local currentChar = 1
local phase = 0
local background = nil
local chars = {}

local animationKeys = { "FriskUT", "CharaOW", "MonsterKidOW", "BoosterOW", "AsrielOW" }
local AN1M4710NK3Y5 = { "FriskUT", "CharaOW", "MonsterKidOW", "BoosterOW", "AsrielOW", "Overworld/CharacterSelector/Mystery/mysteryman" }
local positions = { {160, 160}, {320, 160}, {480, 160}, {240, 320}, {400, 320} }
local P051710N5 = { {150, 200}, {320, 100}, {490, 200}, {240, 380}, {400, 380}, {320, 240} }
local disabled = {}
local animationCount = 0
local friskWait = 1
local lastInput = ""
local b0015P3C141 = false

local fadeCount = 1
local nextPhase = false
local inProgress = false
local count = 0
local vely = 2
local show = false

local limit = 10

function EventPage0()
    if GetRealGlobal("CYFInternalCharacterSelected") then
        Event.Remove(Event.GetName())
    else
        SetRealGlobal("CYFInternalCharacterSelected", true)
        Screen.SetTone(false, false, 0, 0, 0, 0)
        disabled = { GetRealGlobal("CYFInternalCross1"), GetRealGlobal("CYFInternalCross2"), GetRealGlobal("CYFInternalCross3"), GetRealGlobal("CYFInternalCross4"), GetRealGlobal("CYFInternalCross5") }
        disabled[6] = not (disabled[1] and disabled[2] and disabled[3] and disabled[4] and disabled[5])
        if not disabled[6] then
            Audio.Stop()
            animationKeys = AN1M4710NK3Y5
            positions = P051710N5
        end
        while disabled[currentChar] do
            currentChar = currentChar + 1
        end
        lastEnabled = currentChar
        NewAudio.CreateChannel("CharSelect")
        Screen.DispImg("px", 1, 320, 240, 0, 0, 0, 255)
        background = Event.GetSprite("Image1")
        background.Scale(640, 480)
        Screen.DispImg("px", 4 * #animationKeys + 2, 320, 240, 0, 0, 0, 255)
        foreground = Event.GetSprite("Image" .. (4 * #animationKeys + 2))
        foreground.Scale(640, 480)
        local playerSprite = Event.GetSprite("Player")
        playerSprite.alpha = 1
    end
end

function EventPage1()
    Player.CanMove(false)
    if phase == 0 then
        phase = 1
        for i = 1, #animationKeys do
            Screen.DispImg("Overworld/CharacterSelector/window_border", i * 2, positions[i][1], positions[i][2], 255, 255, 255, 255)
            Screen.DispImg("Overworld/CharacterSelector/window_back", i * 2 + 1, positions[i][1], positions[i][2], 255, 255, 255, 255)
            Screen.DispImg(animationKeys[i] .. "/1", 2 * #animationKeys + i + 1, positions[i][1], positions[i][2], 255, 255, 255, 255)
            Screen.DispImg("Overworld/CharacterSelector/Mystery/cross" .. i, 3 * #animationKeys + i + 1, positions[i][1], positions[i][2], 255, 255, 255, 255)
            chars[i] = {}
            chars[i]["border"] = Event.GetSprite("Image" .. (i * 2))
            chars[i]["back"] = Event.GetSprite("Image" .. (i * 2 + 1))
            chars[i]["back"].SetParent(chars[i]["border"])
            chars[i]["sprite"] = Event.GetSprite("Image" .. (#animationKeys * 2 + i + 1))
            chars[i]["sprite"].SetParent(chars[i]["back"])
            chars[i]["sprite"].loopmode = "ONESHOT"
            chars[i]["cross"] = Event.GetSprite("Image" .. (#animationKeys * 3 + i + 1))
            chars[i]["cross"].alpha = disabled[i] and 1 or 0
            chars[i]["cross"].SetParent(chars[i]["back"])
        end
        foreground.MoveAbove(chars[#chars]["border"])
        ChangeTarget(currentChar, false, true)
    elseif phase == 1 then
        foreground.alpha = fadeCount
        if fadeCount <= 0 then phase = 2 end
    elseif phase == 2 then
        local beginCurrentChar = currentChar
        HandleInput() -- Triggers the formula
        if beginCurrentChar ~= currentChar then
            limit = 10
            while disabled[currentChar] do
                if limit == 0 then
                    break
                end
                if lastInput == "Left" or lastInput == "Right" then
                    HandleInput(lastInput) -- Triggers the formula with the same direction as before
                else
                    HandleUpDownFail()
                    break
                end
                limit = limit - 1
            end
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
            chars[currentChar]["sprite"].x = -10
            chars[currentChar]["sprite"].SetAnimation({"MonsterKidOW/f0", "MonsterKidOW/f1", "MonsterKidOW/f2",  "MonsterKidOW/f2",  "MonsterKidOW/f2",
                                                       "MonsterKidOW/f3", "MonsterKidOW/f4", "MonsterKidOW/f5",  "MonsterKidOW/f5",  "MonsterKidOW/f5",
                                                       "MonsterKidOW/f5", "MonsterKidOW/f5", "MonsterKidOW/f5",  "MonsterKidOW/f6",  "MonsterKidOW/f7",
                                                       "MonsterKidOW/f8", "MonsterKidOW/f9", "MonsterKidOW/f10", "MonsterKidOW/f11"                     }, 0.1)
        elseif currentChar == 4 then
            if count == 0 then
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
        elseif currentChar == 3 or currentChar == 5 then
            if chars[currentChar]["sprite"].animcomplete then  phase = 5 end
        else
            if not b0015P3C141 then
                SetRealGlobal("CYFInternalCharacterSelected", false)
                SetRealGlobal("CYFInternalCross1", false)
                SetRealGlobal("CYFInternalCross2", false)
                SetRealGlobal("CYFInternalCross3", false)
                SetRealGlobal("CYFInternalCross4", false)
                SetRealGlobal("CYFInternalCross5", false)
                lastAlpha = 0
                b0015P3C141 = true
                for i = 4, #animationKeys * 4 + 2 do
                    Screen.SupprImg(i)
                end
                Screen.DispImg("Overworld/CharacterSelector/Mystery/mysteryman/1", 2, 320, 320, 255, 255, 255, 255)
                Screen.DispImg("Overworld/CharacterSelector/Mystery/mysteryman/2", 3, 320, 100, 255, 255, 255, 255)
                mysSpr = Event.GetSprite("Image2")
                mysTextSpr = Event.GetSprite("Image3")
                mysSpr.alpha = 0
                mysSpr.Scale(2, 2)
                mysTextSpr.alpha = 0
                fadeCount = 8
                SetRealGlobal("1a6377e26b5119334e651552be9f17f8d92e83c9", true)
                General.Save(true)
            end
            if lastAlpha <= 0 and (-fadeCount + 4) / 4 > 0 then
                NewAudio.PlaySound("CharSelect", "Secret/sound")
            end
            mysSpr.alpha = (-fadeCount + 4) / 4
            mysTextSpr.alpha = (-fadeCount + 4) / 4
            lastAlpha = (-fadeCount + 4) / 4
        end
    elseif phase == 5 then
        fadeCount = 3
        phase = 6
    elseif phase == 6 then
        for i = 1, #chars do
            if chars[i]["cross"].alpha != 0 then
                chars[i]["cross"].alpha = fadeCount - 2
            end
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

function HandleUpDownFail()
    local leftBound = false
    local rightBound = false
    local left = false
    local index = 1
    local curr = currentChar
    local y = positions[curr][2]
    repeat
        if not (rightBound and not left) and not (leftBound and left) then
            curr = left and currentChar - index or currentChar + index
            if curr > #positions then
                rightBound = true
            elseif curr < 1 then
                leftBound = true
            elseif positions[curr][2] ~= y then
                if curr < currentChar then
                    leftBound = true
                else
                    rightBound = true
                end
            end
        end
        if leftBound and rightBound then
            ChangeTarget(lastEnabled)
            return
        end
        if left then
            index = index + 1
        end
        left = not left
    until not disabled[curr] and not (rightBound and left) and not (leftBound and not left)
    ChangeTarget(curr)
end

function HandleInput(forcedInput)
    if currentChar ~= 6 then
        if Input.Right == 1 or forcedInput == "Right" then
            lastInput = "Right"
            if currentChar < 4 then ChangeTarget(currentChar % 3 + 1)
            else                    ChangeTarget((currentChar - 3) % 2 + 1 + 3)
            end
        elseif Input.Left == 1 or forcedInput == "Left" then
            lastInput = "Left"
            local temp = (currentChar + 2) % 3
            if currentChar < 4 then ChangeTarget(temp != 0 and temp or 3)
            else                    ChangeTarget((currentChar - 3) % 2 + 1 + 3)
            end
        elseif Input.Down == 1 or Input.Up == 1 or forcedInput == "Down" or forcedInput == "Up" then
            lastInput = Input.Down == 1 or forcedInput == "Down" and "Down" or "Up"
            local temp = (currentChar + (currentChar > 3 and 2 or 3)) % 5
            ChangeTarget(currentChar != 3 and (temp != 0 and temp or 5) or 5)
        end
    end
    if Input.Confirm == 1 then
        chars[currentChar]["back"].color = {1, 1, 0}
        Audio.PlaySound("menuconfirm")
        phase = 3
    end
end

function ChangeTarget(number, sound, forced)
    currentChar = number
    if not disabled[number] and currentChar ~= lastEnabled or forced then
        chars[lastEnabled]["sprite"].StopAnimation()
        chars[lastEnabled]["sprite"].Set(animationKeys[lastEnabled] .. "/1")
        chars[lastEnabled]["border"].color = {1, 1, 1}
        animationCount = 0
        if sound then Audio.PlaySound("menumove") end

        lastEnabled = currentChar
        if currentChar ~= 6 then
            chars[currentChar]["sprite"].SetAnimation({ animationKeys[currentChar].."/"..math.floor(animationCount/2)*4,   animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+1,
                                                        animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+2, animationKeys[currentChar].."/"..math.floor(animationCount/2)*4+3 }, 0.25)
        end
        chars[currentChar]["border"].color = {1, 0, 0}
    end
end

function Exit()
    for i = 1, #animationKeys * 4 + 2 do Screen.SupprImg(i) end
    NewAudio.DestroyChannel("CharSelect")
    Player.CanMove(true)
    Event.Remove(Event.GetName())
end