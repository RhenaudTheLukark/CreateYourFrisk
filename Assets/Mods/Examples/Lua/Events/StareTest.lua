function EventPage1() end

stareFrame = 1800
stareShift = 0
eventFrequency = 3600 -- 1m
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
    stare2Ended = false
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
    stare6Count = 0
    stare6Sprites = nil
    stare6Speeds = nil
    Stare7 = Event.Exists("Punder") and Stare7Alive or Stare7Dead
    stare7Count = 0
    stare7Phase = 0
    stare8Count = 0
    stare8Phase = 0
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
    if stare2Ended then
        return true
    elseif frame == 0 and not inputted then
        Event.Teleport("Punder", 320, 480)
        Event.SetAnimHeader("Punder", "Sun")
        Event.MoveToPoint("Punder", 320, 320, true, false)
    elseif not stare2MovementDownDone and punderSprite.absx == 320 and punderSprite.absy == 320 then
        stare2MovementDownDone = true
        Event.MoveToPoint("Punder", 400, 260, true, false)
    elseif punderSprite.absx == 400 and punderSprite.absy == 260 then
        Event.SetPage("Punder", 2)
        stare2Ended = true
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
                dogPawsSprite.z = -1
                dogPawsSprite.SetPivot(.5, 0)
                dogPawsSprite.MoveToAbs(dogSprite.absx, dogSprite.absy)

                dogLegsSprite = CreateSprite("Overworld/DogLegs")
                dogLegsSprite.z = -1
                dogLegsSprite.SetPivot(.5, 0)
                dogLegsSprite.MoveToAbs(dogSprite.absx, dogSprite.absy + 6)
                Event.MoveToPoint("Event1", dogSprite.absx, dogSprite.absy + 80, true, false)
            end
            -- Scale the legs so that they seem attached to the dog
            dogLegsSprite.yscale = dogLegsSprite.yscale + 1/3
        elseif frame == 170 then
            -- D O G   S U C C E S S F U L L Y   R A I S E D
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
            -- ...increase his falling speed and rotate him to the side a little
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
            Audio.PlaySound("Jump")
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

boosterSprite = nil
charaSprite = nil
boosterTimestamps = {
    -- Common
    [0] =   { speed = 6,  isHorz = true  }, -- From left to top right loop
    [70] =  { speed = -6, isHorz = false }, -- From top right loop to bottom right loop
    [85] =  { speed = -6, isHorz = true  }, -- From bottom right loop to bottom left loop
    [110] = { speed = 6,  isHorz = false }, -- From bottom left loop to top left loop
    [125] = { speed = 6,  isHorz = true  }, -- From top left loop to top right loop
    [150] = { speed = -6, isHorz = false }, -- From top right loop to bottom right loop
    [165] = { speed = -6, isHorz = true  }, -- From bottom right loop to bottom left loop
    [190] = { speed = 6,  isHorz = false }, -- From bottom left loop to top left loop
    [205] = { speed = 6,  isHorz = true  }, -- From top left loop to top loop
    [220] = { speed = 6,  isHorz = false }, -- From top loop to top
    [260] = { speed = 0 },                  -- Waiting...
    -- Booster-only
    [350] = { speed = 6,  isHorz = true,  instaTP = { x = -40, y = 240 } }, -- From bottom to center
    [410] = { speed = -6, isHorz = false },                                 -- From center to left
    [460] = { speed = 0 },                                                  -- Waiting...
    [520] = { speed = -6, isHorz = false, instaTP = { x = 320, y = 480 } }, -- From top to bottom
    [610] = { speed = 0 },                                                  -- Waiting...
    [700] = { speed = 6,  isHorz = false, instaTP = { x = 320, y = -60 } }, -- From bottom to top behind Chara
    [790] = { speed = 0 },                                                  -- Waiting...
    [830] = { speed = -6, isHorz = false, instaTP = { x = 320, y = 480 } }, -- From top to center
    [860] = { speed = 0,  noSound = true, surprise = true },                -- Encounter bubble and wait
    [895] = { speed = 6,  isHorz = false },                                 -- From center to top
    [935] = { speed = 0 },                                                  -- Already dead
}
charaTimestamps = {
    -- Chara-only
    [420] =  { speed = -6, isHorz = false, instaTP = { x = 320, y = 480 } }, -- From top to bottom
    [510] =  { speed = 0 },                                                  -- Waiting...
    [550] =  { speed = 6,  isHorz = true,  instaTP = { x = -40, y = 240 } }, -- From left to center
    [610] =  { speed = 6,  isHorz = false },                                 -- From center to top
    [650] =  { speed = 0 },                                                  -- Waiting...
    [690] =  { speed = 6,  isHorz = false, instaTP = { x = 320, y = -60 } }, -- From bottom to top
    [780] =  { speed = 0 },                                                  -- Waiting...
    [820] =  { speed = 6,  isHorz = false, instaTP = { x = 320, y = -60 } }, -- From bottom to center
    [860] =  { speed = 0,  noSound = true, surprise = true },                -- Encounter bubble and wait
    [895] =  { speed = 6,  isHorz = false },                                 -- From center to top
    [945] =  { speed = 0 },                                                  -- Waiting...
    [1060] = { speed = -3, isHorz = false, instaTP = { x = 320, y = 480 } }, -- From top to center
    [1140] = { speed = 0,  laugh = true   },                                 -- Laughing animation
    [1310] = { speed = -3, isHorz = true  },                                 -- From center to left
    [1440] = { speed = 0,  noSound = true },                                 -- Never used
}
function Stare6(frame)
    -- Create sprites
    if frame == 0 and not inputted then
        boosterSprite = CreateSprite("BoosterOW/8")
        boosterSprite.ypivot = 0
        boosterSprite.MoveToAbs(-40, 240)
        boosterSprite["path"] = "BoosterOW"
        boosterSprite["speed"] = 0
        boosterSprite["isHorz"] = false
        boosterSprite.z = -1
        charaSprite = CreateSprite("CharaOW/8")
        charaSprite.ypivot = 0
        charaSprite.MoveToAbs(-40, 240)
        charaSprite["path"] = "CharaOW"
        charaSprite["speed"] = 0
        charaSprite["isHorz"] = false
        charaSprite.z = -1
        stare6Sprites = { boosterSprite, charaSprite }
        stare6Speeds = { boosterTimestamps, boosterTimestamps }
    end

    -- The sprites are only deleted when the animation is done
    if not charaSprite then
        return true
    end

    -- Handle movement for both sprites using the current count and their timestamp table
    for k, v in pairs({ stare6Count, stare6Count <= 270 and stare6Count - 10 or stare6Count }) do
        local sprite = stare6Sprites[k]
        local speedObject = stare6Speeds[k][v]
        -- If a timestamp has been found, then the sprite's behavior will change
        if speedObject then
            -- If the encounter bubble exists, delete it
            if sprite["surprise"] then
                sprite["surprise"].Remove()
                sprite["surprise"] = nil
            end
            -- Replace the sprite's speed and isHorz values with the new ones
            sprite["speed"] = speedObject.speed
            sprite["isHorz"] = speedObject.isHorz
            -- If there's a TP, teleport the sprite at the given coords
            if speedObject.instaTP then
                sprite.MoveToAbs(speedObject.instaTP.x, speedObject.instaTP.y)
            end
            -- Create encounter bubbles if needed
            if speedObject.surprise then
                -- Play the encounter bubble sound once
                if k == 2 then
                    Audio.PlaySound("BeginBattle1")
                end
                local spritename = sprite.spritename:sub(sprite.spritename:find("/[^/]*$") + 1)
                sprite.StopAnimation()
                sprite.Set(sprite["path"] .. "/" .. (math.floor(tonumber(spritename) / 4) * 4 + 1))
                local surprise = CreateSprite("Overworld/EncounterBubble" .. (k == 2 and "Geno" or ""))
                surprise.SetParent(sprite)
                surprise.SetPivot(.5, 0)
                surprise.SetAnchor(.5, 1)
                surprise.MoveTo(0, 0)
                sprite["surprise"] = surprise
            end
            -- If the sprite doesn't move in this behavior
            if sprite["speed"] == 0 then
                sprite.StopAnimation()
                -- If the sprite needs a laughing animation, use it
                if speedObject.laugh then
                    sprite.SetAnimation({ "l1", "l2", "l3" }, 1 / 8, sprite["path"])
                    NewAudio.CreateChannel("Stare")
                    NewAudio.PlaySound("Stare", "Laugh")
                -- If the sprite doesn't move, it plays the runaway sound: if noSound is true, it doesn't play it
                elseif not speedObject.noSound then
                    Audio.PlaySound("runaway")
                end
            else
                -- Delete the audio channel Stare if it exists
                if NewAudio.Exists("Stare") then
                    NewAudio.DestroyChannel("Stare")
                end
                local start = sprite["speed"] > 0 and (sprite["isHorz"] and 8 or 12) or (sprite["isHorz"] and 4 or 0)
                local tab = { }
                for i = 0, 3 do table.insert(tab, start + i) end
                sprite.SetAnimation(tab, 3 / 5 / math.abs(sprite["speed"]), sprite["path"])
            end
        end
        -- Move the sprite using its speed and isHorz values
        sprite.Move(sprite["isHorz"] and sprite["speed"] or 0, sprite["isHorz"] and 0 or sprite["speed"])
    end

    local canInput = true
    for _, v in pairs(stare6Sprites) do
        if v["speed"] ~= 0 or NewAudio.Exists("Stare") or boosterSprite["surprise"] then
            canInput = false
            break
        end
    end

    -- If no sprite is moving, the audio channel Stare doesn't exist, no encounter bubble exists and player has pressed a key, stop the event
    -- Deletes all the sprites and clean the animation up when it's done
    if (inputted and canInput) or stare6Count == 1439 then
        boosterSprite.Remove(); boosterSprite = nil
        charaSprite.Remove();   charaSprite = nil
        stare6Sprites = nil; stare6Speeds = nil
        if NewAudio.Exists("Stare") then
            NewAudio.DestroyChannel("Stare")
        end
    -- If a key is pressed while the laughing animation is ongoing, stop it
    elseif NewAudio.Exists("Stare") and (inputted or NewAudio.IsStopped("Stare")) then
        stare6Count = 1309
    -- Replace the chara sprite's timestamp table with his own when the common part is finished
    elseif stare6Count == 270 then
        stare6Speeds[2] = charaTimestamps
    -- Killing in progress
    elseif stare6Count == 1000 then
        Audio.PlaySound("hitSound")
        Misc.ShakeScreen(3)
    end

    stare6Count = stare6Count + 1

    return false
end

punderSprite = nil
function Stare7Alive(frame)
    -- Create Asriel sprite and move Punder
    if frame == 0 and not inputted then
        asriel = CreateSprite("AsrielOW/13")
        asriel.ypivot = 0
        asriel.MoveToAbs(320, -56)
        asriel.SetAnimation({12, 13, 14, 15}, 0.15, "AsrielOW")
        asriel.z = -1 -- in front of Punder

        -- move Punder into place
        Event.StopCoroutine("Punder")
        punderSprite = Event.GetSprite("Punder")
        -- walk to start point
        Event.MoveToPoint("Punder", 400, 260, true, false)
    end

    if not inputted then
        -- Phase 0: Walk up
        if stare7Count < 120 then
            asriel.absy = math.min(asriel.absy + 3, 260)
        -- Phase 1: Look right and talk to Punder friend!
        elseif stare7Count == 120 then
            stare7Phase = 1
            asriel.StopAnimation()
            asriel.Set("Overworld/Asriel/16")
        elseif stare7Count >= 154 and stare7Count%14 == 0 and stare7Count < 308 then
            asriel.Set("Overworld/Asriel/1" .. (stare7Count%28 == 14 and 6 or 7))
            if stare7Count == 238 then
                Event.SetDirection("Punder", 4)
            end
        elseif stare7Count == 308 then
            asriel.Set("AsrielOW/9")
        elseif stare7Count == 340 then
            Event.SetAnimHeader("Punder", "SunMovingLeft")
        elseif stare7Count == 460 then
            Event.SetAnimHeader("Punder", "Sun")
            Event.SetDirection("Punder", 4)
        elseif stare7Count == 530 then
            asriel.Set("AsrielOW/5")
        -- Phase 2: Play tag!
        elseif stare7Count == 600 then
            stare7Phase = 2
            Audio.PlaySound("runaway")
            asriel.SetAnimation({4, 5, 6, 5}, 0.15, "AsrielOW")
            punderSpeed = Event.GetSpeed("Punder")
            Event.SetSpeed("Punder", 3)
            Event.MoveToPoint("Punder", 215, 260, true, false)
        elseif stare7Count > 600 and stare7Count < 1220 then -- tag loop
            -- asriel
            do
                -- run left
                if     asriel.absy == 260 and asriel.absx > 215 then
                    asriel.absx = math.max(asriel.absx - 3, 215)

                    -- go down next
                    if asriel.absx == 215 then
                        asriel.SetAnimation({0, 1, 2, 1}, 0.15, "AsrielOW")
                        asriel.z = -1
                    end
                -- run down
                elseif asriel.absx == 215 and asriel.absy > 140 then
                    asriel.absy = math.max(asriel.absy - 3, 140)

                    -- go right next
                    if asriel.absy == 140 then
                        asriel.SetAnimation({8, 9, 10, 9}, 0.15, "AsrielOW")
                    end
                -- run right
                elseif asriel.absy == 140 and asriel.absx < 400 then
                    asriel.absx = math.min(asriel.absx + 3, 400)

                    -- go up next
                    if asriel.absx == 400 then
                        asriel.SetAnimation({12, 13, 14, 13}, 0.15, "AsrielOW")
                        asriel.z = 0
                    end
                -- run up
                elseif asriel.absx == 400 and asriel.absy < 260 then
                    asriel.absy = math.min(asriel.absy + 3, 260)

                    -- go left next
                    if asriel.absy == 260 then
                        asriel.SetAnimation({4, 5, 6, 5}, 0.15, "AsrielOW")
                    end
                end
            end

            -- punder
            do
                -- run down next
                if     punderSprite.absy == 260 and punderSprite.absx == 215 then
                    Event.MoveToPoint("Punder", 215, 140, true, false)
                -- run right next
                elseif punderSprite.absx == 215 and punderSprite.absy == 140 then
                    Event.MoveToPoint("Punder", 400, 140, true, false)
                -- run up next
                elseif punderSprite.absy == 140 and punderSprite.absx == 400 then
                    Event.MoveToPoint("Punder", 400, 260, true, false)
                -- run left next
                elseif punderSprite.absx == 400 and punderSprite.absy == 260 then
                    Event.MoveToPoint("Punder", 215, 260, true, false)
                end
            end
        elseif stare7Count == 1220 then
            asriel.Set("AsrielOW/5")
            asriel.StopAnimation()
            Event.MoveToPoint("Punder", asriel.absx + punderSprite.width/2, asriel.absy, true, false)
        -- Phase 3: Asriel got tagged!
        elseif stare7Count == 1238 then
            stare7Phase = 3
            Audio.PlaySound("Bump") -- BeginBattle1
            asriel.Set("Overworld/Asriel/16")
            asriel.xscale = -1
        elseif stare7Count > 1238 and stare7Count <= 1238 + 15 then
            local i = stare7Count - 1238
            local scale = 1 + math.sin(i * math.pi * 2 / 15) * 0.05
            asriel.xscale = -1 / scale
            asriel.yscale = scale
        elseif stare7Count == 1254 then
            asriel.Scale(-1, 1)
        elseif stare7Count == 1320 then
            asriel.Set("AsrielOW/9")
            asriel.xscale = 1
        elseif stare7Count == 1335 then
            Event.MoveToPoint("Punder", asriel.absx + 80, 260, true, false)
        -- Phase 4: Asriel's turn to chase!
        elseif stare7Count == 1380 then
            stare7Phase = 4
            Audio.PlaySound("runaway")
            asriel.SetAnimation({8, 9, 10, 9}, 0.15, "AsrielOW")
            asriel.z = 0
            Event.MoveToPoint("Punder", 400, 260, true, false)
        elseif stare7Count > 1380 and stare7Count < 1980 then -- tag loop
            -- asriel
            do
                -- run right
                if     asriel.absy == 260 and asriel.absx < 400 then
                    asriel.absx = math.min(asriel.absx + 3, 400)

                    -- go down next
                    if asriel.absx == 400 then
                        asriel.SetAnimation({0, 1, 2, 1}, 0.15, "AsrielOW")
                    end
                -- run down
                elseif asriel.absx == 400 and asriel.absy > 140 then
                    asriel.absy = math.max(asriel.absy - 3, 140)

                    -- go left next
                    if asriel.absy == 140 then
                        asriel.SetAnimation({4, 5, 6, 5}, 0.15, "AsrielOW")
                        asriel.z = -1
                    end
                -- run left
                elseif asriel.absy == 140 and asriel.absx > 215 then
                    asriel.absx = math.max(asriel.absx - 3, 215)

                    -- go up next
                    if asriel.absx == 215 then
                        asriel.SetAnimation({12, 13, 14, 13}, 0.15, "AsrielOW")
                    end
                -- run up
                elseif asriel.absx == 215 and asriel.absy < 260 then
                    asriel.absy = math.min(asriel.absy + 3, 260)

                    -- go right next
                    if asriel.absy == 260 then
                        asriel.SetAnimation({8, 9, 10, 9}, 0.15, "AsrielOW")
                        asriel.z = 0
                    end
                end
            end

            -- punder
            do
                -- run down next
                if     punderSprite.absx == 400 and punderSprite.absy == 260 then
                    Event.MoveToPoint("Punder", 400, 140, true, false)
                -- run left next
                elseif punderSprite.absx == 400 and punderSprite.absy == 140 then
                    Event.MoveToPoint("Punder", 215, 140, true, false)
                -- run up next
                elseif punderSprite.absx == 215 and punderSprite.absy == 140 then
                    Event.MoveToPoint("Punder", 215, 260, true, false)
                -- run right next
                elseif punderSprite.absx == 215 and punderSprite.absy == 260 then
                    Event.MoveToPoint("Punder", 400, 260, true, false)
                end
            end
        -- Phase 5: Punder got tagged!
        elseif stare7Count == 1980 then
            Event.MoveToPoint("Punder", punderSprite.absx, punderSprite.absy, true, false)
            stare7Phase = 5
        elseif stare7Count > 1980 and stare7Count < 1998 then
            asriel.absx = asriel.absx + 3
        elseif stare7Count == 1998 then
            Audio.PlaySound("Bump")
            asriel.StopAnimation()
            asriel.Set("Overworld/Asriel/16")
        elseif stare7Count > 1998 and stare7Count <= 1998 + 15 then
            local i = stare7Count - 1998
            local scale = 1 + math.sin(i * math.pi * 2 / 15) * 0.05
            punderSprite.xscale = 1 / scale
            punderSprite.yscale = scale
        elseif stare7Count == 2013 then
            punderSprite.Scale(1, 1)
        elseif stare7Count == 2060 then
            Event.SetDirection("Punder", 4)
        elseif stare7Count == 2120 then
            Event.SetAnimHeader("Punder", "SunMovingLeft")
        elseif stare7Count == 2220 then
            Event.SetAnimHeader("Punder", "Sun")
            Event.SetDirection("Punder", 4)
        elseif stare7Count >= 2300 and stare7Count%15 == 0 and stare7Count <= 2415 then
            asriel.Set("Overworld/Asriel/1" .. (stare7Count%30 == 0 and 7 or 6))
        elseif stare7Count == 2500 then
            Event.SetSpeed("Punder", punderSpeed)
            Event.MoveToPoint("Punder", 400, 260, true, false)
        elseif stare7Count == 2540 then
            asriel.Set("AsrielOW/13")
        -- Phase 6: Bye-bye!
        elseif stare7Count == 2590 then
            stare7Phase = 6
            asriel.SetAnimation({12, 13, 14, 15}, 0.1875, "AsrielOW")
        elseif stare7Count > 2590 and stare7Count < 2701 then
            asriel.absy = asriel.absy + 2
        -- The end!!
        elseif stare7Count >= 2701 then
            if asriel then
                asriel.Remove()
                asriel = nil
                Event.SetSpeed("Punder", punderSpeed)
                Event.SetPage("Punder", 2)
            end
            return true
        end
    -- Player pressed a key
    else
        -- Asriel hasn't walked all the way up yet
        if stare7Phase == 0 then
            -- run once
            if stare7Count < 120 then
                stare7Count = 120
                asriel.SetAnimation({0, 1, 2, 1}, 0.1875, "AsrielOW")
            elseif stare7Count > 120 and asriel.absy > -56 then
                asriel.absy = asriel.absy - 2
            -- end of event
            elseif stare7Count > 120 and asriel.absy <= -56 then
                asriel.Remove()
                asriel = nil
                Event.SetPage("Punder", 2)
                return true
            end
        -- Talking to Punder
        elseif stare7Phase == 1 then
            -- run once
            if stare7Count < 600 then
                stare7Count = 600
                asriel.SetAnimation({12, 13, 14, 13}, 0.1875, "AsrielOW")
                Event.SetAnimHeader("Punder", "Sun")
                Event.SetPage("Punder", 2)
            elseif stare7Count > 600 and asriel.absy < 480 then
                asriel.absy = asriel.absy + 2
            -- end of event
            elseif stare7Count > 600 and asriel.absy >= 480 then
                asriel.Remove()
                asriel = nil
                return true
            end
        -- Tag game CCW
        elseif stare7Phase == 2 then
            -- asriel
            do
                -- run left
                if     asriel.absy == 260 and asriel.absx > 215 then
                    if asriel.absx >= 298 then
                        asriel.absx = math.max(asriel.absx - 3, 298)

                        -- run north off-screen
                        if asriel.absx == 298 then
                            asriel.absy = asriel.absy + 1
                            asriel.SetAnimation({12, 13, 14, 13}, 0.1875, "AsrielOW")
                            asriel.z = 0
                            Event.MoveToPoint("Punder", punderSprite.absx, punderSprite.absy, true, false)
                            Event.SetSpeed("Punder", punderSpeed)
                            Event.SetPage("Punder", 2)
                        end
                    else
                        asriel.absx = math.max(asriel.absx - 3, 215)

                        -- go down next
                        if asriel.absx == 215 then
                            asriel.SetAnimation({0, 1, 2, 1}, 0.15, "AsrielOW")
                            asriel.z = -1
                        end
                    end
                -- run down
                elseif asriel.absx == 215 and asriel.absy > 140 then
                    asriel.absy = math.max(asriel.absy - 3, 140)

                    -- go right next
                    if asriel.absy == 140 then
                        asriel.SetAnimation({8, 9, 10, 9}, 0.15, "AsrielOW")
                    end
                -- run right
                elseif asriel.absy == 140 and asriel.absx < 400 then
                    asriel.absx = math.min(asriel.absx + 3, 400)

                    -- go up next
                    if asriel.absx == 400 then
                        asriel.SetAnimation({12, 13, 14, 13}, 0.15, "AsrielOW")
                        asriel.z = 0
                    end
                -- run up
                elseif asriel.absx == 400 and asriel.absy < 260 then
                    asriel.absy = math.min(asriel.absy + 3, 260)

                    -- go left next
                    if asriel.absy == 260 then
                        asriel.SetAnimation({4, 5, 6, 5}, 0.15, "AsrielOW")
                    end
                -- run north off-screen
                elseif asriel.absx == 298 and asriel.absy < 480 then
                    asriel.absy = asriel.absy + 2

                    -- end of event
                    if asriel.absy >= 480 then
                        asriel.Remove()
                        asriel = nil
                        Event.MoveToPoint("Punder", punderSprite.absx, punderSprite.absy, true, false)
                        Event.SetPage("Punder", 2)
                        return true
                    end
                end
            end

            -- punder
            do
                -- run down next
                if     punderSprite.absy == 260 and punderSprite.absx == 215 then
                    Event.MoveToPoint("Punder", 215, 140, true, false)
                -- run right next
                elseif punderSprite.absx == 215 and punderSprite.absy == 140 then
                    Event.MoveToPoint("Punder", 400, 140, true, false)
                -- run up next
                elseif punderSprite.absy == 140 and punderSprite.absx == 400 then
                    Event.MoveToPoint("Punder", 400, 260, true, false)
                end
            end
        -- Asriel getting tagged
        elseif stare7Phase == 3 then
            -- run once
            if stare7Count < 1380 then
                stare7Count = 1380
                asriel.SetAnimation({12, 13, 14, 13}, 0.1875, "AsrielOW")
                asriel.Scale(1, 1)
                Event.SetAnimHeader("Punder", "Sun")
                Event.SetPage("Punder", 2)
                Event.SetSpeed("Punder", punderSpeed)
            elseif stare7Count > 1380 and asriel.absy < 480 then
                asriel.absy = asriel.absy + 2
            -- end of event
            elseif stare7Count > 1380 and asriel.absy >= 480 then
                asriel.Remove()
                asriel = nil
                return true
            end
        -- Tag game CW
        elseif stare7Phase == 4 then
            -- asriel
            do
                -- run right
                if     asriel.absy == 260 and asriel.absx < 400 then
                    asriel.absx = math.min(asriel.absx + 3, 400)

                    -- go down next
                    if asriel.absx == 400 then
                        asriel.SetAnimation({0, 1, 2, 1}, 0.15, "AsrielOW")
                    end
                -- run down
                elseif asriel.absx == 400 and asriel.absy > 140 then
                    asriel.absy = math.max(asriel.absy - 3, 140)

                    -- go left next
                    if asriel.absy == 140 then
                        asriel.SetAnimation({4, 5, 6, 5}, 0.15, "AsrielOW")
                        asriel.z = -1
                    end
                -- run left
                elseif asriel.absy == 140 and asriel.absx > 215 then
                    if asriel.absx > 320 then
                        asriel.absx = math.max(asriel.absx - 3, 320)

                        -- run south off-screen
                        if asriel.absx == 320 then
                            asriel.absy = asriel.absy - 1
                            asriel.SetAnimation({0, 1, 2, 1}, 0.1875, "AsrielOW")
                            asriel.z = -1
                            Event.MoveToPoint("Punder", punderSprite.absx, punderSprite.absy, true, false)
                        end
                    else
                        asriel.absx = math.max(asriel.absx - 3, 215)

                        -- go up next
                        if asriel.absx == 215 then
                            asriel.SetAnimation({12, 13, 14, 13}, 0.15, "AsrielOW")
                        end
                    end
                -- run up
                elseif asriel.absx == 215 and asriel.absy < 260 then
                    asriel.absy = math.min(asriel.absy + 3, 260)

                    -- go right next
                    if asriel.absy == 260 then
                        asriel.SetAnimation({8, 9, 10, 9}, 0.15, "AsrielOW")
                        asriel.z = 0
                    end
                -- run south off-screen
                elseif asriel.absx == 320 and asriel.absy < 140 and asriel.absy > -56 then
                    asriel.absy = asriel.absy - 2

                    -- end of event
                    if asriel.absy <= -56 then
                        asriel.Remove()
                        asriel = nil
                        Event.SetSpeed("Punder", punderSpeed)
                        Event.SetPage("Punder", 2)
                        return true
                    end
                end
            end

            -- punder
            do
                -- run down next
                if     punderSprite.absx == 400 and punderSprite.absy == 260 then
                    Event.MoveToPoint("Punder", 400, 140, true, false)
                -- run up next
                elseif punderSprite.absx == 215 and punderSprite.absy == 140 then
                    Event.MoveToPoint("Punder", 215, 260, true, false)
                -- run right next
                elseif punderSprite.absx == 215 and punderSprite.absy == 260 then
                    Event.MoveToPoint("Punder", 400, 260, true, false)
                -- run left next
                elseif punderSprite.absx == 400 and punderSprite.absy == 140 then
                    Event.MoveToPoint("Punder", 320 - 78, 140, true, false)
                end
            end
        -- Punder is talking to Asriel
        elseif stare7Phase > 4 then
            -- run once
            if stare7Count < 2701 then
                stare7Count = 2701
                asriel.SetAnimation({12, 13, 14, 15}, 0.1875, "AsrielOW")
                punderSprite.Scale(1, 1)
                Event.SetSpeed("Punder", punderSpeed)
                Event.SetPage("Punder", 2)
            elseif stare7Count > 2701 and asriel.absy < 480 then
                if asriel.absx ~= 320 then
                    asriel.absx = asriel.absx + (math.min(math.abs(asriel.absx - 320), 1.5) * (asriel.absx > 320 and -1 or 1))
                end
                asriel.absy = asriel.absy + 2
            -- end of event
            elseif stare7Count > 2701 and asriel.absy >= 480 then
                asriel.Remove()
                asriel = nil
                return true
            end
        end
    end

    stare7Count = stare7Count + 1
end

function Stare7Dead(frame)
    -- Create Asriel sprite
    if frame == 0 and not inputted then
        asriel = CreateSprite("AsrielOW/13")
        asriel.ypivot = 0
        asriel.MoveToAbs(320, -56)
        asriel.SetAnimation({12, 13, 14, 15}, 0.15, "AsrielOW")
    end

    if not inputted then
        -- Phase 0: Walk up
        if stare7Phase == 0 and asriel and asriel.absy < 260 then
            asriel.absy = math.min(asriel.absy + 3, 260)
        -- Phase 1: Look right and talk to Punder...but he's not there...
        elseif stare7Count == 120 then
            stare7Phase = 1
            asriel.StopAnimation()
            asriel.Set("Overworld/Asriel/16")
        elseif stare7Count >= 154 and stare7Count%14 == 0 and stare7Count <= 308 then
            asriel.Set("Overworld/Asriel/1" .. (stare7Count%28 == 14 and 6 or 7))
        elseif stare7Count == 350 then
            asriel.Set("Overworld/Asriel/15")
        elseif stare7Count == 395 then
            Audio.PlaySound("Squeak")
        elseif stare7Count > 395 and stare7Count < 420 then
            local i = stare7Count - 395
            local scale = 1 + math.sin(i * math.pi * 2 / 15) * 0.05
            asriel.xscale = 1 / scale
            asriel.yscale = scale
        elseif stare7Count == 420 then
            asriel.Scale(1, 1)
        elseif stare7Count == 500 then
            asriel.Set("Overworld/Asriel/11")
            asriel.xscale = -1
        -- Phase 2: Walk to where Punder used to be
        elseif stare7Count == 570 then
            stare7Phase = 2
            asriel.SetAnimation({12, 11, 13, 11}, 0.2, "Overworld/Asriel")
        elseif stare7Count > 570 and stare7Count < 625 then
            asriel.x = math.min(asriel.x + 1.5, 400)
        elseif stare7Count == 625 then
            asriel.StopAnimation()
            asriel.Set("Overworld/Asriel/11")
        -- Phase 3: Look around frantically for lost friend...
        elseif stare7Count == 710 or stare7Count == 920 then
            asriel.Set("Overworld/Asriel/11")   -- left
            asriel.xscale = 1
        elseif stare7Count == 750 or stare7Count == 880 then
            asriel.Set("AsrielOW/13")           -- up
            asriel.xscale = 1
        elseif stare7Count == 840 then
            asriel.Set("Overworld/Asriel/11")   -- right
            asriel.xscale = -1
        elseif stare7Count == 790 or stare7Count == 960 then
            asriel.Set("Overworld/Asriel/2")    -- down
            asriel.xscale = 1
        elseif stare7Count == 1060 then
            asriel.Set("Overworld/Asriel/11")
            asriel.xscale = -1
        elseif stare7Count == 1120 then
            asriel.Set("Overworld/Asriel/14")
            asriel.xscale = 1
        -- Phase 4: Begin to cry :'(
        elseif stare7Count == 1250 then
            stare7Phase = 4
            asriel.Set("Overworld/Asriel/2")
        elseif stare7Count == 1325 then
            asriel.Set("Overworld/Asriel/3")
        elseif stare7Count == 1400 then
            asriel.Set("Overworld/Asriel/4")
        elseif stare7Count == 1500 then
            asriel.Set("Overworld/Asriel/0")
        elseif stare7Count > 1550 and stare7Count%30 == 0 and stare7Count < 1800 then
            asriel.Set("Overworld/Asriel/" .. (stare7Count%60 == 0 and 0 or 1))
        -- Phase 5: Stop crying...
        elseif stare7Count >= 1800 and stare7Count%20 == 0 and stare7Count < 2060 then
            stare7Phase = 5
            asriel.Set("Overworld/Asriel/" .. (5 + ((stare7Count%80) / 20)))
        elseif stare7Count == 2120 then
            asriel.Set("Overworld/Asriel/9")
        elseif stare7Count == 2200 then
            asriel.Set("Overworld/Asriel/10")
        elseif stare7Count == 2275 then
            asriel.Set("Overworld/Asriel/2")
        -- Phase 6: Leave :c
        elseif stare7Count == 2400 then
            stare7Phase = 6
            asriel.Set("Overworld/Asriel/11")
        elseif stare7Count == 2460 then
            asriel.SetAnimation({12, 11, 13, 11}, 0.25, "Overworld/Asriel")
        elseif stare7Count > 2460 and stare7Count < 2525 then
            asriel.x = math.max(asriel.x - 1.25, 320)
        elseif stare7Count == 2525 then
            asriel.StopAnimation()
            asriel.Set("Overworld/Asriel/11")
        elseif stare7Count == 2575 then
            asriel.SetAnimation({12, 13, 14, 13}, 0.25, "AsrielOW")
        elseif stare7Count > 2575 and stare7Count < 2630 then
            asriel.y = asriel.y + 1.5
        elseif stare7Count == 2630 then
            asriel.y = math.floor(asriel.y)
            asriel.StopAnimation()
            asriel.Set("AsrielOW/13")
        elseif stare7Count == 2700 then
            asriel.Set("Overworld/Asriel/11")
            asriel.xscale = -1
        elseif stare7Count == 2790 then
            asriel.Set("AsrielOW/13")
            asriel.xscale = 1
        elseif stare7Count == 2830 then
            asriel.SetAnimation({12, 13, 14, 13}, 0.1875, "AsrielOW")
        elseif stare7Count >= 2830 and stare7Count < 2900 then
            asriel.absy = asriel.absy + 2
        -- The end!!
        elseif stare7Count >= 2900 then
            if asriel then
                asriel.Remove()
                asriel = nil
            end
            return true
        end
    -- Player pressed a key
    else
        -- Asriel hasn't walked all the way up yet
        if stare7Phase == 0 then
            -- run once
            if stare7Count < 120 then
                stare7Count = 120
                asriel.StopAnimation()
                asriel.Set("AsrielOW/13")
                asriel.xscale = 1
            elseif stare7Count == 150 then
                asriel.Set("AsrielOW/5")
            elseif stare7Count == 180 then
                asriel.Set("AsrielOW/9")
            elseif stare7Count == 210 then
                asriel.Set("AsrielOW/1")
            elseif stare7Count == 270 then
                asriel.SetAnimation({0, 1, 2, 1}, 0.1875, "AsrielOW")
                asriel.absy = math.floor(asriel.absy + 0.5)
            elseif stare7Count > 270 and asriel.absy > -56 then
                asriel.absy = asriel.absy - 2
            -- end of event
            elseif stare7Count > 270 and asriel.absy <= -56 then
                asriel.Remove()
                asriel = nil
                return true
            end
        -- Asriel is trying to talk to Punder
        elseif stare7Phase == 1 then
            -- run once
            if stare7Count < 570 then
                stare7Count = 570
                asriel.StopAnimation()
                asriel.SetAnimation({12, 13, 14, 13}, 0.15, "AsrielOW")
                asriel.Scale(1, 1)
            elseif asriel.absy < 480 then
                asriel.absy = asriel.absy + 2.5
            -- end of event
            elseif asriel.absy >= 480 then
                asriel.Remove()
                asriel = nil
                return true
            end
        -- any point after asriel starts walking right
        else
            -- run once
            if stare7Count < 2900 then
                stare7Count = 2900
                asriel.StopAnimation()
                -- walk left or up
                if asriel.absx > 320 then
                    asriel.SetAnimation({12, 11, 13, 11}, 0.1875, "Overworld/Asriel")
                else
                    asriel.SetAnimation({12, 13, 14, 13}, 0.1875, "AsrielOW")
                end
                asriel.xscale = 1
            elseif asriel.absx > 320 then
                asriel.absx = math.max(asriel.absx - 2, 320)

                -- start walking up
                if asriel.absx == 320 then
                    asriel.SetAnimation({12, 13, 14, 13}, 0.1875, "AsrielOW")
                end
            elseif asriel.absy < 480 then
                asriel.absy = asriel.absy + 2
            -- end of event
            elseif asriel.absy >= 480 then
                asriel.Remove()
                asriel = nil
                return true
            end
        end
    end

    stare7Count = asriel and stare7Count + 1 or stare7Count
end

function Stare8(frame)
   -- run once
    if frame == 0 and not inputted then
        chara = CreateSprite("Overworld/Chara/c0")
        chara.z = -1
        chara.ypivot = 0
        chara.MoveToAbs(-22, 230)

        vignette = CreateSprite("Overworld/Chara/vignette")
        vignette.alpha = 0
        vignette.SetParent(chara)
        vignette.MoveTo(0, 0)

        NewAudio.CreateChannel("zzz")
        NewAudio.PlayMusic("zzz", "mus_zzz_c", true, 0)
    end

    -- standard operations
    if stare8Phase > -1 then
        if not inputted then
            -- Phase 0: Slowly walk right with pauses
            if stare8Phase == 0 and stare8Count < 330 then
                if stare8Count % 110 == 0 then
                    chara.Set("Overworld/Chara/c0")
                elseif stare8Count % 110 == 45 then
                    chara.Set("Overworld/Chara/c1")
                end

                if stare8Count % 110 < 45 then
                    chara.x = chara.x + 0.5
                end
            elseif stare8Phase == 0 and stare8Count == 370 then
                stare8Phase = 1
                chara.SetAnimation({"c0", "c1"}, 0.3, "Overworld/Chara")
            -- Phase 1: Walk right normally but still slowly. then walk down
            elseif stare8Phase == 1 and stare8Count < 800 then
                chara.x = chara.x + 0.5
            elseif stare8Phase == 1 and stare8Count >= 800 then
                if stare8Count == 800 then
                    chara.animationspeed = chara.animationspeed * 2.5
                end

                chara.x = chara.x + 0.25
                chara.y = math.max(chara.y - 0.1875, 174)

                if chara.y == 174 then
                    stare8Phase = 2
                    chara.animationspeed = 1
                end
            -- Phase 2: Walk right just a bit slower
            elseif stare8Phase == 2 and stare8Count < 1300 then
                chara.x = chara.x + 0.2
            elseif stare8Count == 1300 then
                chara.StopAnimation()
                chara.Set("Overworld/Chara/c1")
                stare8Phase = 3
            -- Phase 3: Inch ever closer to Frisk
            elseif stare8Phase == 3 and stare8Count >= 1360 and stare8Count < 2710 then
                local timer = (stare8Count - 1360) % 140

                if timer == 0 then
                    chara.Set("Overworld/Chara/c0")
                elseif timer == 45 then
                    chara.Set("Overworld/Chara/c1")
                end

                if timer < 45 then
                    chara.x = math.min(chara.x + 0.1, 420)
                end
            elseif stare8Count == 2710 then
                stare8Phase = 4
            -- Phase 4: Hug...?
            elseif stare8Count == 2830 then
                chara.Set("Overworld/Chara/c2")
            elseif stare8Count == 3000 then
                chara.Set("Overworld/Chara/c3")
            elseif stare8Count == 3060 then
                stare8Phase = 5
            -- Phase 5: Fade out scary music
            elseif stare8Count == 3220 then
                chara.Set("Overworld/Chara/c4")
            elseif stare8Count == 3265 then
                chara.Set("Overworld/Chara/c5")
            elseif stare8Count == 3350 then
                chara.Set("CharaOW/9")
            elseif stare8Count == 3420 then
                stare8Phase = -1
                stare8Count = 0
                return false
            end

            -- vignette alpha
            if stare8Phase < 4 then
                vignette.alpha = chara.x/450
                NewAudio.SetVolume("zzz", math.min((chara.x/420) * 0.75, 0.75))
                Audio.Volume((1 - (chara.x/334)) * 0.75)
            elseif stare8Phase == 5 then
                NewAudio.SetVolume("zzz", NewAudio.GetVolume("zzz") - 0.003)
                vignette.alpha = vignette.alpha - 0.003
            end
        -- Player has pressed a key
        else
            -- Initial walk right
            if stare8Phase == 0 then
                -- run once
                if chara.xscale == 1 then
                    chara.xscale = -1
                    chara.Set("Overworld/Chara/c1")
                    chara.SetAnimation({"c1", "c0"}, 0.1875, "Overworld/Chara")
                    vignette.xscale = 1
                else
                    chara.x = math.max(chara.x - 2, -22)

                    NewAudio.SetVolume("zzz", NewAudio.GetVolume("zzz") - 0.003)
                    vignette.alpha = vignette.alpha - 0.003

                    if chara.x == -22 then
                        NewAudio.SetVolume("src", math.min(NewAudio.GetVolume("src") + 0.01, 0.75))
                        if NewAudio.GetVolume("src") == 0.75 and vignette.alpha == 0 then
                            chara.Remove()
                            chara = nil
                            vignette.Remove()
                            NewAudio.DestroyChannel("zzz")
                            return true
                        end
                    end
                end
            else
                stare8Phase = -1
                stare8Count = -1
            end
        end
    -- Make Chara run off
    else
        if stare8Count == 0 then
            charaY = chara.absy
            chara.Set("CharaOW/9")
            chara.StopAnimation()
            chara.ypivot = 0
            chara.absy = charaY

            -- make bubble
            Audio.PlaySound("BeginBattle1")
            bubble = CreateSprite("Overworld/EncounterBubble")
            bubble.ypivot = 0
            bubble.SetParent(chara)
            bubble.SetAnchor(0.5, 0)
            bubble.MoveTo(0, chara.height + 6)
        elseif stare8Count <= 11 then
            chara.y = chara.y + ( 6 - stare8Count)
        elseif stare8Count == 12 then
            chara.absy = charaY
        elseif stare8Count == 30 then
            bubble.Remove()
            Audio.PlaySound("runaway")
            chara.SetAnimation({4, 5, 6, 5}, 0.1, "CharaOW")
        elseif stare8Count > 30 then
            if chara then
                chara.x = math.max(chara.x - 4, -22)
                chara.y = math.min(chara.y + 2, 230)

                if chara.x == -22 then
                    NewAudio.SetVolume("src", math.min(NewAudio.GetVolume("src") + 0.01, 0.75))
                    if NewAudio.GetVolume("src") == 0.75 and vignette.alpha == 0 then
                        chara.Remove()
                        chara = nil
                        vignette.Remove()
                        NewAudio.DestroyChannel("zzz")
                        return true
                    end
                end
            else
                return true
            end
        end

        NewAudio.SetVolume("zzz", NewAudio.GetVolume("zzz") - 0.003)
        vignette.alpha = vignette.alpha - 0.003
    end

    stare8Count = chara and stare8Count + 1 or stare8Count
end

-- Auto
function EventPage2()
    stareShift = (Event.Exists("Punder") and Event.GetAnimHeader("Punder") == "") and 0 or 2
    stareFrame = 1800
    inputted = false
    currEventDone = true
    resetStareVars()
    Event.SetPage(Event.GetName(), 3)
    Player.CanMove(false)
    Event.MoveToPoint("Player", 430, 174, true)
    Event.SetDirection("Player", 6)
end

-- Parallel process
function EventPage3()
    local stareID = math.floor(stareFrame / eventFrequency)
    local realStareID = math.min(stareID + stareShift, maxStares)
    if stareID > 0 then
        currEventDone = _G["Stare" .. realStareID]((stareFrame + (stareShift * eventFrequency)) - realStareID * eventFrequency)
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