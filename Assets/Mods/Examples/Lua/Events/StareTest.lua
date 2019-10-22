function EventPage1() end

stareFrame = 0
stareShift = 0
eventFrequency = 300 -- 4500 = 1m 15s
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

function Stare1(frame) DEBUG("Stare1: " .. frame) return true end

stare2MovementDownDone = false
punderSprite = nil

function Stare2(frame)
    if frame == 0 and not inputted then
        Event.Teleport("Punder", 320, 480)
        if punderSprite == nil then
            punderSprite = Event.GetSprite("Punder")
        end
        Event.SetAnimHeader("Punder", "Sun")
        Event.MoveToPoint("Punder", 320, 320, true, false)
    elseif not stare2MovementDownDone and punderSprite.absx == 320 and punderSprite.absy == 320 then
        stare2MovementDownDone = true
        Event.MoveToPoint("Punder", 400, 240, true, false)
    elseif punderSprite.absx == 400 and punderSprite.absy == 240 then
        return true
    end
    return false
end

function Stare3(frame) DEBUG("Stare3: " .. frame) return true end
function Stare4(frame) DEBUG("Stare4: " .. frame) return true end
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