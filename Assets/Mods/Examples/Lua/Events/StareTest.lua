function EventPage1() end

stareFrame = 0
stareShift = 0
eventFrequency = 4500 -- 1m 15s
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
    stare3DogOpen = false
    stare3InputtedFrame = 0
    stare3DogStartingY = 0
    stare3DogLegsYScale = 0
    stare3InputtedFirst = false
    stare3Count = 0
    stare3DogSpeed = 0
    stare4Count = 0
end

punderSprite = nil
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
            Event.SetAnimHeader("Punder", "")
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

dogSprite = nil
dogPawsSprite = nil
dogLegsSprite = nil
-- Handles the dog's barking animation
function Stare3Bark(frame, maxFrame)
    if frame % 15 == 0 and frame < maxFrame then
        dogSprite.Set("Overworld/Dog" .. (stare3DogOpen and "" or "Bark"))
        stare3DogOpen = not stare3DogOpen
        if stare3DogOpen then
            Audio.PlaySound("Bark")
        end
    end
end

-- Handles the dog's (and his legs if handled) bouncing animation
function Stare3Bounce(frame, handleLegs)
    if frame % 30 <= 15 then
        local scale = 1 + math.sin(frame * math.pi * 2 / 15) * .1
        dogSprite.Scale(scale, 1 / scale)
        if handleLegs then
            dogSprite.absy = stare3DogStartingY - 3 * (1 / stare3DogLegsYScale - 1 / scale * stare3DogLegsYScale)
            dogLegsSprite.Scale(scale, 1 / scale * stare3DogLegsYScale)
            dogPawsSprite.Scale(scale, 1 / scale)
        end
    end
end

function Stare3(frame)
    -- Init: Get the dog's sprite and set some useful variables
    if frame == 0 and not inputted then
        dogSprite = Event.GetSprite("Event1")
        Event.SetSpeed("Event1", 1)
        stare3DogStartingY = dogSprite.absy
    end

    -- Part 1: Dog barks and bounces twice
    if frame <= 60 then
        -- Stops the stare event instantly if the player presses a key during this part
        if inputted then
            dogSprite.Set("Overworld/Dog")
            dogSprite.Scale(1, 1)
            return true
        end
        -- Barking animation, sound and bouncing
        Stare3Bark(frame, 60)
        Stare3Bounce(frame, false)
    -- Part 2: L E G S
    -- If the Player hasn't pressed any key yet
    elseif not inputted then
        if frame >= 90 and frame < 170 then
            -- Init: Creates the legs and paws sprites and move the dog up
            if frame == 90 then
                dogPawsSprite = CreateSprite("Overworld/DogPaws")
                dogPawsSprite.SetPivot(.5, 0)
                dogPawsSprite.MoveToAbs(dogSprite.absx, dogSprite.absy)

                dogLegsSprite = CreateSprite("Overworld/DogLegs")
                dogLegsSprite.SetPivot(.5, 0)
                dogLegsSprite.MoveToAbs(dogSprite.absx, dogSprite.absy + 6)
                Event.MoveToPoint("Event1", dogSprite.absx, dogSprite.absy + 80, true, false)
            end
            -- Scale the legs so that they seem attached to the dog
            dogLegsSprite.yscale = dogLegsSprite.yscale + 1/3
        elseif frame == 170 then
            -- D O G   S U C C E S F U L L Y   R A I S E D
            Audio.PlaySound("success")
        -- Wait for several seconds...
        elseif frame >= 450 and frame < 510 then
            -- Dog barks twice again and bounces, but this time the legs bounce too!
            -- Init: We store the dog's legs' scale
            if frame == 450 then
                stare3DogLegsYScale = dogLegsSprite.yscale
            end
            -- Barking animation, sound and bouncing
            Stare3Bark(frame, 60)
            Stare3Bounce(frame, true)
        -- Lowers the dog back to normal
        elseif frame >= 510 and frame < 590 then
            if frame == 510 then
                Event.MoveToPoint("Event1", dogSprite.absx, stare3DogStartingY, true, false)
            end
            dogLegsSprite.yscale = dogLegsSprite.yscale - 1/3
        -- Remove the paw sprites and call it a day
        elseif frame == 590 then
            dogLegsSprite.Remove()
            dogPawsSprite.Remove()
            return true
        elseif frame > 590 then
            return true
        end
    -- If the Player pressed a key
    else
        -- If the dog was bouncing with his long legs, reset it back as if he wasn't bouncing
        if frame >= 450 and frame <= 510 and stare3DogLegsYScale ~= 0 then
            dogSprite.absy = stare3DogStartingY + 80
            dogSprite.Scale(1, 1)
            dogLegsSprite.Scale(1, stare3DogLegsYScale)
            dogPawsSprite.Scale(1, 1)
            stare3DogLegsYScale = 0
        end
        -- As long as the leg sprites exist, shorten the legs and keep the dog in midair
        if dogLegsSprite.isactive then
            -- Stop the dog's movement, barking and prepare the legs to be scaled down
            if not stare3InputtedFirst then
                Event.MoveToPoint("Event1", dogSprite.absx, dogSprite.absy, true, false)
                dogSprite.Set("Overworld/Dog")
                stare3DogOpen = false
                dogLegsSprite.SetPivot(.5, 1)
                dogLegsSprite.MoveToAbs(dogSprite.absx, dogSprite.absy + 6)
                stare3InputtedFirst = true
            end
            -- Scale the legs down and raise the paws
            dogLegsSprite.yscale = dogLegsSprite.yscale - 2
            dogPawsSprite.Move(0, 6)
            -- End condition: when the legs are no more, delete the sprites and prepare the dog to fall
            if dogLegsSprite.yscale <= 0 then
                dogLegsSprite.Remove()
                dogPawsSprite.Remove()
                Event.SetSpeed("Event1", 0)
                Event.MoveToPoint("Event1", dogSprite.absx, stare3DogStartingY, true, false)
            end
        -- While the dog is falling...
        elseif dogSprite.absy > stare3DogStartingY then
            -- ...increase his falling speed and rotate him t the side a little
            stare3DogSpeed = stare3DogSpeed + 0.05
            Event.SetSpeed("Event1", stare3DogSpeed)
            dogSprite.rotation = dogSprite.rotation - (dogSprite.rotation < 10 and .5 or dogSprite.rotation < 15 and .25 or .1)
        -- When the dog is on the ground and still rotated, barking or bouncing
        elseif (stare3Count <= 15 or dogSprite.rotation ~= 0 or dogSprite.xscale ~= 1 or stare3DogSpeed > 0) then
            -- Reset the dog's rotation value to 0 over some frames
            if dogSprite.rotation ~= 0 then
                dogSprite.rotation = dogSprite.rotation - math.max(dogSprite.rotation, -2)
            end
            -- Make the dog bounce depending on his downward speed
            local scale = dogSprite.xscale + stare3DogSpeed / 50
            dogSprite.Scale(scale, 1 / scale)
            stare3DogSpeed = (stare3DogSpeed < 0.25 and stare3DogSpeed > 0) and -stare3DogSpeed or (stare3DogSpeed - 0.25)
            if stare3DogSpeed < 0 and dogSprite.xscale < 1 then
                dogSprite.Scale(1, 1)
            end
            -- Make him bark one last time
            Stare3Bark(stare3Count, 16)
            stare3Count = stare3Count + 1
        else
            return true
        end
    end
    return false
end

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