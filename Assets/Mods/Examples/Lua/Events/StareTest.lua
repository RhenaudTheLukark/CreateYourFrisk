function EventPage1() end

stareFrame = 0
stareShift = 0
eventFrequency = 4500 -- 1m 15s
currEventDone = false
inputted = false
maxStares = 8

displayTextOneFrameMovement = false
function DisplayText(text, formatted, faceSprites, side)
    local textStr = "{"
    if type(text) == "table" then
        for i = 1, #text do
            textStr = textStr .. "'" .. tostring(text[i]):gsub("\n", "\\\\n"):gsub("'", "\\'") .. "'" .. (next(text, i) and ", " or "")
        end
    else
        textStr = textStr .. "'" .. tostring(text):gsub("\n", "\\\\n"):gsub("'", "\\'") .. "'"
    end
    textStr = textStr .. "}"

    local faceStr
    if type(faceSprites) == "table" then
        faceStr = "{"
        for i = 1, #faceSprites do
            if type(faceSprites[i]) == "table" then
                faceStr = faceStr .. "{"
                for j = 1, #faceSprites[i] do
                    faceStr = faceStr .. (type(faceSprites[i][j]) == "string" and "'" or "") .. faceSprites[i][j] .. (type(faceSprites[i][j]) == "string" and "'" or "") .. (next(faceSprites[i], j) and ", " or "")
                end
                faceStr = faceStr .. "}"
            else
                faceStr = faceStr .. "'" .. faceSprites[i] .. "'" .. (next(faceSprites, i) and ", " or "")
            end
        end
        faceStr = faceStr .. "}"
    else
        faceStr = "'" .. faceSprites .. "'"
    end

    SetGlobal("CYFOWStareSetDialog1", textStr)
    SetGlobal("CYFOWStareSetDialog2", formatted)
    SetGlobal("CYFOWStareSetDialog3", faceStr)
    SetGlobal("CYFOWStareSetDialog4", side)

    Event.SetPage("Event1", 4)
    displayTextOneFrameMovement = true
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
    stare5Count = 0
    stare5Phase = 0
    stare5Velocity = 0
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

function Stare5(frame)
    -- Create Papyrus sprite
    if frame == 0 and not inputted then
        papy = CreateSprite("Overworld/Papyrus/0")
        papy.z = -1
        papy.Scale(2, 2)
        papy.ypivot = 0
        papy.MoveToAbs(-papy.width/2, 230)
        papy.SetAnimation({"0", "1", "0", "2"}, 0.1875, "Overworld/Papyrus")
    end

    if not inputted or (inputted and stare5Phase > 7) then
        -- Walk right #1
        if stare5Phase == 0 and papy.x < 260 then
            papy.x = math.min(papy.x + 2, 260)

            if papy.x == 260 then
                papy.animationspeed = 0.2
                stare5Phase = 1
            end
        -- Walk right #2 (slower)
        elseif stare5Phase == 1 and papy.x < 372 then
            papy.x = math.min(papy.x + 1, 372)
            papy.y = papy.y - 0.5

            if papy.x == 372 then
                papy.StopAnimation()
                papy.Set("Overworld/Papyrus/0")
                stare5Phase = 2
            end
        -- Dialogue #1
        elseif stare5Count ==  310 then
            stare5Phase = 3
            papy.SetAnimation({"6", "0"}, 0.2, "Overworld/Papyrus")
            DisplayText("[noskip][font:papyOW][voice:v_papyrus]HELLO,[w:5] HUMAN![w:10]\nI'VE COME TO SEE WHAT YOU'RE[waitall:5]...[waitall:1][w:20][next]", false, {{"Papyrus/normalT", "Papyrus/normal", 0.2}})
        elseif stare5Count ==  500 then
            papy.StopAnimation()
            papy.Set("Overworld/Papyrus/0")
        -- Dialogue #2
        elseif stare5Count ==  700 then
            stare5Phase = 4
            papy.SetAnimation({"7", "3"}, 0.2, "Overworld/Papyrus")
            DisplayText("[noskip][font:papyOW][voice:v_papyrus]HEY!![w:10] ARE YOU LISTENING???[w:20][next]", false, {{"Papyrus/madT", "Papyrus/mad", 0.2}})
        elseif stare5Count ==  820 then
            papy.StopAnimation()
            papy.Set("Overworld/Papyrus/3")
        -- Dialogue #3
        elseif stare5Count == 1000 then
            stare5Phase = 5
            papy.SetAnimation({"8", "9", "10"}, 0.15, "Overworld/Papyrus")
            DisplayText("[noskip][font:papyOW][voice:v_papyrus]AAARRGH!!![w:20]\nSTOP IGNORING MEEE!!!!![w:20][next]", false, {{"Papyrus/madT", "Papyrus/mad", 0.2}})
        elseif stare5Count == 1200 then
            papy.StopAnimation()
            papy.Set("Overworld/Papyrus/11")
        -- Dialogue #4
        elseif stare5Count == 1400 then
            stare5Phase = 6
            papy.SetAnimation({"11", "14"}, 0.2, "Overworld/Papyrus")
            DisplayText("[noskip][font:papyOW][voice:v_papyrus]Music kept = false\nErm[waitall:3]...[waitall:1][w:10] Why did you go here with your " .. Player.GetHP() .. " HP?[w:30][next]", false, {{"Papyrus/suspiciousT", "Papyrus/suspicious", 0.2}})
        elseif stare5Count == 1660 then
            papy.StopAnimation()
            papy.Set("Overworld/Papyrus/11")
        -- Walk left
        elseif stare5Count == 1950 then
            stare5Phase = 7
            papy.SetAnimation({"11", "12", "11", "13"}, 0.1875, "Overworld/Papyrus")
        elseif stare5Phase == 7 and papy.x > 170 then
            papy.x = math.max(papy.x - 1.5, 170)

            if papy.x == 170 then
                papy.StopAnimation()
                papy.Set("Overworld/Papyrus/11")
                stare5Phase = 8
            end
        -- Jump off
        elseif stare5Count == 2160 then
            papy.Set("Overworld/Papyrus/18")
            stare5Phase = 9
            stare5Velocity = 4
            -- TODO: play sound?
        elseif stare5Phase == 9 and stare5Count <= 2360 then
            papy.x = papy.x - 0.25
            papy.y = papy.y + stare5Velocity
            stare5Velocity = stare5Velocity - 0.2
            papy.rotation = papy.rotation + 0.25
            
            -- screm
            if stare5Count == 2210 then
                DisplayText("[noskip][font:papyOW][voice:v_papyrus][effect:shake,5]AAAAAAAA[w:10][next]", false, "Papyrus/papy he do a jump")
            elseif stare5Count == 2360 then
                papy.Remove()
            end
        elseif stare5Count > 2360 then
            return true
        end
    else
        -- Initial walks right - left half
        if stare5Phase == 0 or (stare5Phase == 1 and papy.x <= 320) then
            -- Walk to x=320, then straight up, then remove and end
            if papy.x < 320 then
                papy.x = math.min(papy.x + 2, 320)

                -- Begin walking up
                if papy.x == 320 then
                    papy.SetAnimation({"15", "16", "15", "17"}, 0.1875, "Overworld/Papyrus")
                end
            elseif papy.y < 480 then
                papy.y = math.min(papy.y + 2, 480)

                -- Remove and end event
                if papy.y == 480 then
                    papy.Remove()
                    return true
                end
            end
        -- Initial walks right - right half
        elseif stare5Phase == 1 and papy.x > 320 then
            -- Run once
            if not papy["walkLeftBool"] then
                papy["walkLeftBool"] = true
                papy.SetAnimation({"11", "12", "11", "13"}, 0.1875, "Overworld/Papyrus")
            end

            papy.x = math.max(papy.x - 2, 320)

            -- As soon as he gets to x=320, the prevoius conditional block will take over and make him walk up
            if papy.x == 320 then
                papy.SetAnimation({"15", "16", "15", "17"}, 0.1875, "Overworld/Papyrus")
            end
        -- Interrupt speech and flee going South
        elseif stare5Phase > 1 and stare5Phase < 7 then
            -- Frame #1
            if not papy["interrupt"] then
                papy["interrupt"] = true
                -- Stop dialogue if any is active
                if GetGlobal("CYFOWStareSetDialogActive") then
                    General.EndDialog()
                end
            -- Frame #2 (2-frame wait is necessary just for starting the next dialogue)
            elseif not papy["startflee"] then
                papy["startflee"] = true
                papy.SetAnimation({"11", "12", "11", "13"}, 0.1, "Overworld/Papyrus")

                Audio.PlaySound("hitsound", 0.75)
                Misc.ShakeScreen(8, 8)
                DisplayText("[noskip][font:papyOW][voice:v_papyrus][speed:3][lettereffect:shake,3]I JUST REMEMBERED[lettereffect:none] I HAVE TO OF IN COLD BONE OF OUT DOG BONE EAT THE BONE[w:10][next]", false, "Papyrus/papy he do a jump")
            -- Frame #3 onwards
            else
                -- Run left
                if papy.x > 320 then
                    papy.x = math.max(papy.x - 6, 320)

                    if papy.x == 320 then
                        papy.SetAnimation({"19", "20", "19", "21"}, 0.1, "Overworld/Papyrus")
                    end
                -- Run down
                elseif (papy.y + (papy.yscale * papy.height)) > 0 or GetGlobal("CYFOWStareSetDialogActive") then
                    papy.y = papy.y - 6
                -- Remove and end event
                else
                    Audio.PlaySound("runaway", 1)
                    papy.Remove()
                    return true
                end
            end
        -- Walking left
        elseif stare5Phase == 7 then
            -- Keep walking towards x=170
            if papy.x > 170 then
                papy.x = math.max(papy.x - 1.5, 170)
            -- Walk off screen towards initial spawning position
            elseif papy.x ~= -papy.width or papy.y ~= 230 then
                papy.x = math.max(papy.x - 1.5, -papy.width)
                papy.y = math.min(papy.y + 1.5, 230)
            -- Remove and end event
            else
                papy.Remove()
                return true
            end
        -- Jumping off - at this point it's too late to stop him. So no condition here. RIP papy ;w;
        end
    end

    stare5Count = papy and stare5Count + 1 or stare5Count
end

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

    if not displayTextOneFrameMovement then
        Player.CanMove(false)
    else
        Player.CanMove(true)
        displayTextOneFrameMovement = false
    end
    if not inputted then
        stareFrame = stareFrame + 1
    end
end