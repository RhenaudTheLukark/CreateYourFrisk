function EventPage1()
    Event.MoveToPoint("Player", 430, 174, true)
    Event.SetDirection("Player", 6)
    local animHeader = Event.GetAnimHeader("Player")
    if     animHeader == ""        then Event.SetPage(Event.GetName(), 10)     -- Frisk
    elseif animHeader == "Chara"   then Event.SetPage(Event.GetName(), 14)     -- Chara
    elseif animHeader == "MK"      then Event.SetPage(Event.GetName(), 2)      -- Monster Kid
    elseif animHeader == "Booster" then Event.SetPage(Event.GetName(), 368395) -- Booster
    elseif animHeader == "Asriel"  then Event.SetPage(Event.GetName(), 69)     -- Asriel
    end
end

endTexts = {
    "Oh, I lost. I'll have to try again.",
    "Ah, looks like I wasn't cut out for it this time.",
    "Aw...I thought I was there...",
    "He's not gone yet. I must try again.",
    "I'm starting to lose my patience.",
    "What is this dog's problem?!",
    "Come on, let me through now!",
    "MOVE. NOW.",
    "My eyes. They hurt. Send help."
}

endFaceSprites = {
    "glad",
    "normal",
    "sad",
    "frustrated",
    "serious",
    "angry",
    "mad",
    "fury",
    "woke"
}

function EventPage5()
    local stareID = GetGlobal("CYFOWStare")
    General.SetDialog("[instant:stopall]" .. endTexts[stareID], true, "Frisk/" .. endFaceSprites[stareID])
    Player.CanMove(true)
    Event.SetPage(Event.GetName(), 1)
end

function EventPage10()
    -- Stare at the dog
    --W 1: Pundy leaves the screen for a bit / If dead, jump to 3
    --R 2: Come back to his spot with sunglasses
    --R 3: Dog bounces a couple of times then raises with long legs, tricking the player into thinking he's letting him past, then goes back to normal
    --W 4: MK comes from bottom, looks at player for some time, then goes up
    --W 5: Papyrus come from left, walks to Player, talks to him (standard Pap lines), then gets upset (stomp foot), "Music kept", backs out, then run to left and jump from cliff using window jump
    --R 6: Chara chasing Booster, run in circles, go down, come from top, go down, Chara coming back alone from top, laugh.wav, go left
    --W 7: Punderbolt & Asriel talk then play tag, then Asriel goes / Asriel enters room, look for something, sit & cry, then wipe tears & go
    --W 8: Chara creepily approaching the player from behind, music slowly fades out, (play anticipation slow mo like in genocide?), then when Chara close to player, hug
    General.SetDialog({ "There's a dog here and it's blocking the way.",
                        "It doesn't look like it'll move any time soon...",
                        "He's bound to go if I stay around for some time!" }, true,
                      { "Frisk/normal", "Frisk/sad", "Frisk/happy" })
    Event.SetPage("Stare", 2)
    Event.SetPage(Event.GetName(), 1)
end

function EventPage14()
    -- Slice + 3D rotation
    General.SetDialog({ "There's a dog here and it's blocking the way.",
                        "It doesn't look like it'll move any time soon...",
                        "I know exactly how to force him to move!" }, true,
                      { "Chara/normal", "Chara/sad", "Chara/creepy" })
    Event.SetPage(Event.GetName(), 1)
end

function EventPage2()
    -- Push + boing
    General.SetDialog({ "There's a dog here and it's blocking the way.",
                        "It doesn't look like it'll m-[w:5]move any time soon...",
                        "I should try pushing it away!" }, true,
                      { "MK/normal", "MK/sad", "MK/determined" })
    Event.MoveToPoint("Player", 350, 174, false)
    General.Wait(45)
    Event.SetDirection("Player", 6)
    General.Wait(45)
    Event.MoveToPoint("Player", 450, 174, true)
    Event.GetSprite("Player").alpha = 0
    
    -- enter the dog
    local spr = CreateSprite("MonsterKidOW/9")
    spr.MoveTo(Event.GetPosition("Player")[1] - 10, Event.GetPosition("Player")[2])
    spr.xscale = -1
    spr.rotation = -90
    spr.SetPivot(1, 0)
    spr.Move(16, -8)
    
    local dogSprite = Event.GetSprite(Event.GetName())
    Event.MoveToPoint("Player", 398, 174, true, false)
    spr.Set("MonsterKidOW/f2")
    Audio.PlaySound("Surprised Bark", 1)
    dogSprite.Set("Overworld/DogBark")
    
    local startX = spr.x
    local finalX = spr.x + 16
    local doggyX = dogSprite.x
    for i = 1, 60 do
        spr.x = lerp(spr.x, finalX, 0.1)
        dogSprite.xscale = 1 - (((spr.x - startX) - 6) / dogSprite.width)
        dogSprite.x = doggyX - 3 + ((spr.x - startX) / 2)
        
        if i == 20 then
            dogSprite.Set("Overworld/Dog")
        end
        
        General.Wait(1)
    end
    General.Wait(60)
    
    -- struggle
    spr.Set("MonsterKidOW/f3")
    General.Wait(30)
    spr.Set("MonsterKidOW/f4")
    General.Wait(30)
    spr.Set("MonsterKidOW/f5")
    General.Wait(10)
    
    -- come out pt1
    for i = 1, 45 do
        spr.x = lerp(spr.x, finalX, -0.15)
        
        dogSprite.xscale = 1 - (((spr.x - startX) - 6) / dogSprite.width)
        dogSprite.x = doggyX - 3 + ((spr.x - startX) / 2)
        
        General.Wait(1)
    end
    spr.x = startX
    dogSprite.x = doggyX
    dogSprite.xscale = 1
    
    -- come out pt2
    spr.rotation = 0
    spr.xpivot = 0.5
    spr.y = spr.y + 8
    spr.Set("MonsterKidOW/f6")
    spr.x = spr.x - 16
    spr.x = spr.x - 7
    Audio.PlaySound("Bump", 1)
    
    local startX = spr.x
    local finalX = spr.x - 32
    
    -- doggy vibrates after the impact
    for i = 1, 60 do
        local scale = 1 + math.sin(i * math.pi * 2 / 15) * ((5 - math.ceil(i / 15)) / 40)
        dogSprite.Scale(scale, 1 / scale)
        General.Wait(1)
        
        spr.x = lerp(spr.x, finalX, 0.075)
        
        if i%20 == 0 then
            spr.Set("MonsterKidOW/f" .. (6 + (i/20)))
        end
    end
    dogSprite.Scale(1, 1)
    General.Wait(10)
    spr.Set("MonsterKidOW/f10")
    General.Wait(10)
    spr.Set("MonsterKidOW/f11")
    General.Wait(100)
    
    -- end event
    spr.Remove()
    dogSprite.xpivot = 0.5
    Event.GetSprite("Player").alpha = 1
    Event.SetDirection("Player", 4)
    General.Wait(20)
    General.SetDialog(({"Nope...", "Aww,[w:10] I thought I had it!", "M-[w:5]maybe I should try again?"})[math.random(3)], true, "MK/sad")
    Event.SetPage(Event.GetName(), 1)
end

function EventPage368395()
    -- Jump SM64
    General.SetDialog({ "There's a dog here and it's blocking the way.",
                        "It doesn't look like it'll move any time soon...",
                        "I should try jumping over it!" }, true,
                      { { "Booster/normalT", "Booster/normal", 0.2 },
                        { "Booster/sadT",    "Booster/sad",    0.2 },
                        { "Booster/happyT",  "Booster/happy",  0.2 } })
    Event.SetPage(Event.GetName(), 1)
end

function EventPage69()
    -- Actually works + Super Paper Mario spin
    General.SetDialog({ "[voice:v_asriel]There's a dog here and it's blocking the way.",
                        "[voice:v_asriel]It doesn't look like it'll move any time soon...",
                        "[voice:v_asriel]Hmmm...[w:20]Maybe I could try being nice to it...?",
                        "[voice:v_asriel]Mister doggy,[w:10] may you please let me through?" }, true,
                      { { "Asriel/normalT", "Asriel/normal", 0.2 },
                        { "Asriel/sadT",    "Asriel/sad",    0.2 },
                        { "Asriel/normalT", "Asriel/normal", 0.2 },
                        { "Asriel/happyT",  "Asriel/happy" , 0.2 } })
    General.Wait(160)
    
    -- create cursor sprite
    Audio.PlaySound("SE1_EVT_LINE_DRAW1")
    cursor = CreateSprite("Overworld/cursor")
    cursor.z = -1
    cursor.SetPivot(0, 1)
    local dogSprite = Event.GetSprite(Event.GetName())
    cursor.MoveToAbs(dogSprite.x - (dogSprite.width/2), dogSprite.y + dogSprite.height)
    cursor.alpha = 0
    
    -- create box edges
    box = {
        CreateSprite("UI/sq_white"), -- top line
        CreateSprite("UI/sq_white"), -- left line
        CreateSprite("UI/sq_white"), -- right line
        CreateSprite("UI/sq_white")  -- bottom line
    }
    
    for i = 1, #box do
        box[i].Scale(1/4, 1/4)
        box[i].SetPivot(0, 1)
        box[i].MoveTo(cursor.x, cursor.y)
        box[i].color = {0, 0, 0}
    end
    
    -- animaaate!
    for timer = 1, 120 do
        -- cursor
        if timer <= 10 then
            cursor.alpha = cursor.alpha + (1/10)
        elseif timer >= 40 and timer < 80 then
            cursor.alpha = cursor.alpha - (1/5)
        end
        
        -- move and draw box
        if timer > 10 and timer < 40 then
            -- move cursor
            cursor.x = lerp(cursor.x, dogSprite.absx + (dogSprite.width / 2), 1/6)
            cursor.y = lerp(cursor.y, dogSprite.absy                        , 1/6)
            
            -- draw box
            box[1].xscale = (cursor.absx - (dogSprite.absx - (dogSprite.width / 2))) /  4
            box[2].yscale = (cursor.absy - (dogSprite.absy +  dogSprite.height    )) / -4
            box[3].yscale = box[2].yscale
            box[3].absx = cursor.absx
            box[4].xscale = box[1].xscale
            box[4].absy = cursor.absy
        -- begin spinny animation
        elseif timer == 40 then
            Audio.PlaySound("SE1_EVT_LINE_TURN2")
            
            box[1].xpivot = 0.5
            box[1].x = box[1].x + (box[1].width * box[1].xscale) / 2
            box[4].SetParent(box[2])
            box[4].xpivot = 0.5
            box[4].x = box[4].x + (box[4].width * box[4].xscale) / 2
            box[2].SetParent(box[1])
            box[2].SetAnchor(0, 0.5)
            box[2].x = 0
            box[3].SetParent(box[1])
            box[3].SetAnchor(1, 0.5)
            box[3].x = 0
        elseif timer > 40 and timer <= 80 then
            dogSprite.xscale = timer < 80 and math.cos((timer - 40) / 5) or 0
            box[1].xscale = (cursor.absx - (dogSprite.absx - (dogSprite.width / 2))) /  4
            box[1].xscale = box[1].xscale * dogSprite.xscale
            box[4].xscale = box[1].xscale
        elseif timer > 80 and timer < 120 then
            for _, box in pairs(box) do
                box.alpha = box.alpha - (1/20)
            end
        -- remove sprites and event
        elseif timer == 120 then
            cursor.Remove()
            cursor = nil
            
            for _, box in pairs(box) do
                box.Remove()
            end
            box = nil
        end
        
        General.Wait(1)
    end
    
    -- end of event
    General.Wait(160)
    General.SetDialog({"[noskip][voice:v_asriel][waitall:5]...[waitall:1][w:40][noskip:off][mugshot:{Asriel/happyT,Asriel/happy,0.2}]Oh!",
                       "[voice:v_asriel]Thank you, mister doggy!"}, true,
                       {"Asriel/what",
                       {"Asriel/happyT", "Asriel/happy", 0.2}})
    Event.SetPage(Event.GetName(), -1)
end

-- Auto page used with StareTest
function EventPage4()
    SetGlobal("CYFOWStareSetDialogActive", true)
    General.SetDialog(load("return " .. GetGlobal("CYFOWStareSetDialog1"))(),
                                        GetGlobal("CYFOWStareSetDialog2"),
                      load("return " .. GetGlobal("CYFOWStareSetDialog3"))(),
                                        GetGlobal("CYFOWStareSetDialog4"))
    SetGlobal("CYFOWStareSetDialogActive", false)

    for i = 1, 4 do
        SetGlobal("CYFOWStareSetDialog" .. i, nil)
    end
    Event.SetPage(Event.GetName(), 1)
end

-- General math function used with EventPage2 and EventPage69
function lerp(a, b, t)
    return a + ((b - a) * t)
end

--[[function EventPage1()
    General.SetDialog({"Music kept = false\nErm[waitall:3]...[waitall:1][w:10] Why did you go here with your " .. hp .. " HP?"}, true, {"papyrus_mugshot_2"})
end

--This event page is a big mash-up test page.
function EventPage4()
	Misc.ShakeScreen(3, 3, true)
    General.Wait(180)
	Screen.Flash(60, 255, 0, 0, 255)
	--These following lines were used for Quaternion tests.
	--You can activate them if you want to
    --/!\ Not usable anymore /!\
	SetTone(true, true, 0, 0, 0, 128)
	DispImg("photo", 1, 320, 240, 436, 256, 224, 130, 40, 255)
	WaitForInput()
	RotateEvent("Image1", 360, 0, 0)
	WaitForInput()
	RotateEvent("Image1", 0, 360, 0)
	WaitForInput()
	RotateEvent("Image1", 0, 0, 360)
	WaitForInput()
	RotateEvent("Image1", 360, 360, 360)
	WaitForInput()
	RotateEvent("Image1", 180, 360, 0)
	WaitForInput()
	RotateEvent("Image1", 0, 180, 360)
	WaitForInput()
	RotateEvent("Image1", 360, 0, 180)
	WaitForInput()
	RotateEvent("Image1", 360, 360, 360)
	WaitForInput()
	SupprImg(1)
	SetTone(true, true, 0, 0, 0, 0)
	--General.GameOver({ "[voice:v_sans]Wazzup bro?", "[voice:v_sans]I love this music, don't you?" }, "mus_zz_megalovania")
end]]