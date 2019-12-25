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
    "Oh,[w:5] I lost.[w:10] I'll have to try again.",
    "Ah,[w:5] looks like I wasn't cut out for it this time.",
    "Aw...[w:10]I thought I was there...",
    "He's not gone yet.[w:10]\nI must try again.",
    "I'm starting to lose my patience.",
    "What is this dog's problem?!",
    "Come on,[w:5] let me through now!",
    "MOVE.[w:10] NOW.",
    "My eyes.[w:10]\nThey hurt.[w:15]\nSend help."
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
                        "It's bound to go if I stay around for some time!" }, true,
                      { "Frisk/normal", "Frisk/sad", "Frisk/happy" })
    Event.SetPage("Stare", 2)
    Event.SetPage(Event.GetName(), 1)
end

-- Auto page used with EventPage10
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

-- Auto page used with EventPage10
function EventPage5()
    local stareID = GetGlobal("CYFOWStare")
    General.SetDialog("[instant:stopall]" .. endTexts[stareID], true, "Frisk/" .. endFaceSprites[stareID])
    Player.CanMove(true)
    Event.SetPage(Event.GetName(), 1)
end

function EventPage14()
    -- Slice + 3D rotation
    General.SetDialog({ "There's a dog here and it's blocking the way.",
                        "It doesn't look like it'll move any time soon...",
                        "But I know EXACTLY how to force it to!" }, true,
                      { "Chara/normal", "Chara/sad", "Chara/creepy" })
    
    -- Replace Player with a sprite version of themselves
    Event.GetSprite("Player").alpha = 0
    local pla = CreateSprite("CharaOW/9")
    pla.ypivot = 0
    pla.MoveToAbs(430, 174)
    pla.loopmode = "ONESHOT"
    
    -- Replace dog with a sprite version
    Event.GetSprite(Event.GetName()).alpha = 0
    local dog = CreateSprite("Overworld/Dog")
    dog.ypivot = 0
    dog.MoveToAbs(490, 170)
    
    -- Attack!
    General.Wait(20)
    
    local slice = function(speed, angle, x, y)
        Audio.PlaySound("slice")
        pla.SetAnimation({8, 9}, speed / 20, "CharaOW")
        
        local slice = CreateSprite("Overworld/Chara/bigslice/0")
        slice.rotation = angle
        slice.MoveToAbs(pla.absx + x, pla.absy + y)
        dog.SetParent(slice)
        slice.Mask("invertedstencil")
        slice.loopmode = "ONESHOT"
        slice.SetAnimation({0, 1, 2, 3, 4, 5}, speed/60, "Overworld/Chara/bigslice")
        
        local slice2 = CreateSprite("UI/Battle/spr_slice_o_0")
        slice2.rotation = angle
        slice2.MoveToAbs(pla.absx + x, pla.absy + y)
        slice2.loopmode = "ONESHOT"
        slice2.SetAnimation({"spr_slice_o_0", "spr_slice_o_1", "spr_slice_o_2", "spr_slice_o_3", "spr_slice_o_4", "spr_slice_o_5"}, speed/60, "UI/Battle")
        General.Wait(10)
        
        -- animate slice
        while not slice.animcomplete do
            General.Wait(1)
        end
        
        dog.layer = "Default"
        slice.Remove()
        slice2.Remove()
    end
    
    slice(14, 0, 60, 30)
    General.Wait(20)
    General.SetDialog({ "[noskip]what[w:20][next]" }, true, "Chara/angry")
    for i = 0, 19 do
        slice(5 - math.floor(i / 4), math.random() * 360, 60, 30)
    end
    
    -- Restore player
    General.Wait(40)
    pla.Remove()
    Event.GetSprite("Player").alpha = 1
    
    -- Restore dog
    General.Wait(10)
    dog.Remove()
    Event.GetSprite(Event.GetName()).alpha = 1
    
    -- End of event
    General.Wait(60)
    General.SetDialog({ "..." }, true, "Chara/angry")
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
    
    
    -- Replace Player with a sprite version of themselves
    local player = Event.GetSprite("Player")
    player.alpha = 0
    local pla = CreateSprite("BoosterOW/9")
    pla.ypivot = 0
    pla.MoveToAbs(430, 174)
    pla.z = -1
    
    -- Replace Dog with a sprite version
    local dogSprite = Event.GetSprite(Event.GetName())
    dogSprite.alpha = 0
    
    -- Set up its 3 parts
    do
        dogButt = CreateSprite("Overworld/DogButt")
        dogButt.SetPivot(1, 0)
        dogButt.MoveTo(dogSprite.absx + dogSprite.width/2, dogSprite.absy)
        
        dogStretch = CreateSprite("Overworld/DogStretch")
        dogStretch.xpivot = 1
        dogStretch.SetParent(dogButt)
        dogStretch.SetAnchor(0, 0.5)
        dogStretch.MoveTo(0, 0)
        dogStretch.xscale = 0
        
        dogHead = CreateSprite("Overworld/DogHead")
        dogHead.xpivot = 1
        dogHead.SetParent(dogStretch)
        dogHead.SetAnchor(0, 0.5)
        dogHead.MoveTo(12, 0)
        
        dogButt.Scale(-1, -1)
        dogButt.rotation = 180
        pla.SendToTop()
    end
    
    General.Wait(40)
    
    -- walk left
    for i = 0, 188 do
        pla.x = pla.x - 0.75
        
        -- change sprite
        if i % 16 == 0 then
            pla.Set("BoosterOW/" .. (8 + ((i % 64) / 16)))
        end
        
        -- play step sound
        if i % 32 == 0 then
            Audio.PlaySound("step-floor")
        end
        
        General.Wait(1)
    end
    
    -- jump one
    General.Wait(80)
    pla.Set("Overworld/Booster/j")
    Audio.PlaySound("step-floor")
    for i = 500, 553 do
        pla.x = i < 525 and pla.x or pla.x + 1.5
        pla.y = 174 + (math.sin(math.rad((i - 500) / (26/90))) * 50)
        
        General.Wait(1)
    end
    pla.y = 174
    pla.Set("BoosterOW/10")
    Audio.PlaySound("step-floor")
    
    -- jump two
    General.Wait(10)
    pla.Set("Overworld/Booster/j")
    Audio.PlaySound("step-floor")
    for i = 590, 643 do
        pla.x = pla.x + 1.5
        pla.y = 174 + (math.sin(math.rad((i - 590) / (26/90))) * 110)
        
        if i > 643 - 25 then
            dogButt.x = lerp(dogButt.x, dogSprite.x - (dogSprite.width/3), 0.2)
            dogButt.rotation = lerp(dogButt.rotation, 90, 0.2)
        end
        
        General.Wait(1)
    end
    pla.y = 174
    pla.Set("BoosterOW/10")
    Audio.PlaySound("step-floor")
    
    dogButt.x = dogSprite.x - (dogSprite.width/3)
    dogButt.rotation = 90
    dogStretch.rotation = 270
    
    -- jump three
    General.Wait(6)
    pla.Set("Overworld/Booster/j")
    Audio.PlaySound("step-floor")
    Audio.PlaySound("Jump")
    for i = 676, 698 do
        pla.x = pla.x + 1.5
        pla.y = 174 + (math.sin(math.rad((i - 676) / (33/90))) * 150)
        pla.rotation = pla.rotation - 4.5
        
        dogStretch.xscale = lerp(dogStretch.xscale, 150, 0.075)
        General.Wait(1)
    end
    pla.Set("Overworld/Booster/p")
    pla.rotation = 0
    pla.Move(pla.height/2, -pla.height/3)
    Audio.PlaySound("sm64_impact")
    Audio.PlaySound("mario-pain")
    Misc.ShakeScreen(6, 30)
    
    if Player.GetHP() > 1 then
        Player.SetHP(math.max(Player.GetHP() - 2, 1))
    end
    
    -- fall
    for i = 1, 52 do
        pla.Move(-1, -(i * 2)/25)
        pla.rotation = pla.rotation + 0.75
        
        General.Wait(1)
    end
    General.Wait(44)
    for i = 1, 35 do
        pla.rotation = pla.rotation - 1
        General.Wait(1)
    end
    
    -- end of event
    player.MoveToAbs(pla.absx, pla.absy)
    player.alpha = 1
    pla.Remove()
    General.Wait(50)
    General.SetDialog({"Ooowwww..."}, true, {{"Booster/shockT", "Booster/shock", 0.2}})
    
    -- move dog back
    for i = 1, 23 do
        dogStretch.xscale = lerp(dogStretch.xscale, 0, 0.1)
        General.Wait(1)
    end
    dogStretch.xscale = 0
    for i = 1, 25 do
        dogButt.x = lerp(dogButt.x, dogSprite.absx + dogSprite.width/2, 0.2)
        dogButt.rotation = lerp(dogButt.rotation, 180, 0.2)
        General.Wait(1)
    end
    dogButt.Remove()
    dogSprite.alpha = 1
    
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

-- General math function used with EventPage2, EventPage69, and EventPage368395
function lerp(a, b, t)
    return a + ((b - a) * t)
end
