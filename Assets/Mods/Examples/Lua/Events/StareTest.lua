function EventPage1() end

stareFrame = 0
stareShift = 0
eventFrequency = 300 -- 4500 -- 1m 15s
currEventDone = false
inputted = false
maxStares = 8

function DisplayText(text, faceSprite)
    if type(text) == "table" then
        for i = 1, #text do
            SetGlobal("CYFOWStareText" .. i, text[i])
        end
    else
        SetGlobal("CYFOWStareText1", text)
    end

    if type(faceSprite) == "table" then
        for i = 1, #faceSprite do
            SetGlobal("CYFOWStareFace" .. i, faceSprite[i])
        end
    else
        SetGlobal("CYFOWStareFace1", faceSprite)
    end

    Event.SetPage("Event1", 4)
end

function resetStareVars()
    stare1MovementUpDone = false
    stare2MovementDownDone = false
    stare4Count = 0
end

function Stare1(frame)
    -- run once
    if frame == 0 and not inputted then
        Event.StopCoroutine("Punder")
        if punderSprite == nil then
            punderSprite = Event.GetSprite("Punder")
        end
        -- walk up-left to 320, 320
        Event.MoveToPoint("Punder", 320, 320, true, false)
    end
    
    -- normal event behavior
    if not inputted then
        -- walk up to 320, 480
        if not stare1MovementUpDone and punderSprite.absx == 320 and punderSprite.absy == 320 then
            stare1MovementUpDone = true
            Event.MoveToPoint("Punder", 320, 480, true, false)
        end
    else
        -- punder actually finished
        if punderSprite.absx == 320 and punderSprite.absy == 480 then
            stareFrame = eventFrequency * 2
            inputted = false
            Stare2(0)
            inputted = true
        -- the player pressed a key early
        else
            -- punder has moved through the upwards portion already
            if stare1MovementUpDone then
                -- move down to 320, 320 first
                if punderSprite.absy > 320 then
                    Event.MoveToPoint("Punder", 320, 320, true, false)
                -- then move down-right to 400, 260
                elseif punderSprite.absx ~= 400 and punderSprite.absy ~= 260 then
                    Event.MoveToPoint("Punder", 400, 260, true, false)
                -- end the event
                else
                    Event.SetPage("Punder", 2)
                    return true
                end
            -- punder has not reached 320, 320 by the time the player pressed a key
            else
                -- move straight to 400, 260
                if punderSprite.absx ~= 400 and punderSprite.absy ~= 260 then
                    Event.MoveToPoint("Punder", 400, 260, true, false)
                -- end the event
                else
                    Event.SetPage("Punder", 2)
                    return true
                end
            end
        end
    end
    return false
end

function Stare2(frame)
    if frame == 0 and not inputted then
        Event.Teleport("Punder", 320, 480)
        Event.SetAnimHeader("Punder", "Sun")
        Event.MoveToPoint("Punder", 320, 320, true, false)
    elseif not stare2MovementDownDone and punderSprite.absx == 320 and punderSprite.absy == 320 then
        stare2MovementDownDone = true
        Event.MoveToPoint("Punder", 400, 260, true, false)
    elseif punderSprite.absx == 400 and punderSprite.absy == 260 then
        Event.SetPage("Punder", 2)
        return true
    end
    return false
end

function Stare3(frame) DEBUG("Stare3: " .. frame) return true end

function Stare4(frame) -- requires at least 574 frames
    -- Create Monster Kid sprite
    if frame == 0 and not inputted then
        mk = CreateSprite("MonsterKidOW/13")
        mk.ypivot = 0
        mk.MoveToAbs(320, -mk.height)
        mk.SetAnimation({"12", "13", "14", "15"}, 0.1875, "MonsterKidOW")
    end

    -- Event is over
    if stare4Count > 0 and not mk then
        return true
    -- Walk up
    elseif stare4Count < 126 then
        mk.absy = mk.absy + 2
    -- Stop walking
    elseif stare4Count == 126 and not inputted then
        mk.StopAnimation()
        mk.Set("MonsterKidOW/13")
    -- Look at Player
    elseif stare4Count == 180 and not inputted then
        mk.Set("MonsterKidOW/9")
    -- Look up
    elseif stare4Count == 360 and not inputted then
        mk.Set("MonsterKidOW/13")
    -- Set animation before walking
    elseif stare4Count == 430 or (stare4Count < 430 and inputted) then
        stare4Count = 430
        mk.SetAnimation({"12", "13", "14", "15"}, 0.1875, "MonsterKidOW")
    -- Walk up
    elseif stare4Count > 430 and mk.absy < 480 then
        mk.absy = mk.absy + 2
    -- Offscreen, remove MK
    elseif stare4Count > 430 and mk.absy >= 480 then
        mk.Remove()
        mk = nil
    end

    stare4Count = mk and stare4Count + 1 or stare4Count
end

function Stare5(frame) DEBUG("Stare5: " .. frame) return true end
function Stare6(frame) DEBUG("Stare6: " .. frame) return true end
function Stare7(frame) DEBUG("Stare7: " .. frame) return true end
function Stare8(frame) DEBUG("Stare8: " .. frame) return true end

-- Auto
function EventPage2()
    stareShift = Event.Exists("Punder") and 0 or 2
    stareFrame = 0
    inputted = false
    currEventDone = true
    resetStareVars()
    Event.SetPage(Event.GetName(), 3)
end

-- Parallel process
function EventPage3()
    local stareID = math.floor(stareFrame / eventFrequency)
    local realStareID = stareID + stareShift
    if stareID > 0 and realStareID <= maxStares then
        currEventDone = _G["Stare" .. realStareID](stareFrame % eventFrequency)
    end

    if not inputted then
        if Input.Left == 1 or Input.Right == 1 or Input.Up == 1 or Input.Down == 1 or Input.Confirm == 1 or Input.Cancel == 1 or Input.Menu == 1 then
            inputted = true
        end
    end

    if inputted and currEventDone then
        SetGlobal("CYFOWStare", math.min(realStareID, maxStares) + 1)
        Player.CanMove(true)
        Event.SetPage(Event.GetName(), 1)
        Event.SetPage("Event1", 5)
        Event.StopCoroutine()
        return
    end

    Player.CanMove(false)
    if not inputted then
        stareFrame = stareFrame + 1
    end
end