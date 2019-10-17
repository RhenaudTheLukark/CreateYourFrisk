function EventPage1()
    if Event.Exists("Punder") then
        Event.MoveToPoint("Punder", 400, 240, true, false)
    end
    Event.MoveToPoint("Player", 430, 180, true)
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
    General.SetDialog(endTexts[stareID], true, "Frisk/" .. endFaceSprites[stareID])
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
    --R 7: Punderbolt & Asriel talk then play tag, then Asriel goes / Asriel enters room, look for something, sit & cry, then wipe tears & go
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
                        "It doesn't look like it'll move any time soon...",
                        "I should try pushing it away!" }, true,
                      { "MK/normal", "MK/sad", "MK/determined" })
    Event.SetPage(Event.GetName(), 1)
end

function EventPage368395()
    -- Jump SM64
    General.SetDialog({ "There's a dog here and it's blocking the way.",
                        "It doesn't look like it'll move any time soon...",
                        "I should try jumping over it!" }, true,
                      { { "Booster/normal", "Booster/normalT", 0.2 },
                        { "Booster/sad",    "Booster/sadT",    0.2 },
                        { "Booster/happy",  "Booster/happyT",  0.2 } })
    Event.SetPage(Event.GetName(), 1)
end

function EventPage69()
    -- Actually works + Super Paper Mario spin
    General.SetDialog({ "[voice:v_asriel]There's a dog here and it's blocking the way.",
                        "[voice:v_asriel]It doesn't look like it'll move any time soon...",
                        "[voice:v_asriel]Hmmm... Maybe I could try being nice to it...?",
                        "[voice:v_asriel]Mister doggy, may you please let me through?" }, true,
                      { { "Asriel/normal", "Asriel/normalT", 0.2 },
                        { "Asriel/sad",    "Asriel/sadT",    0.2 },
                        { "Asriel/normal", "Asriel/normalT", 0.2 },
                        { "Asriel/happy",  "Asriel/happyT" , 0.2 } })
    Event.SetPage(Event.GetName(), 1)
end

-- Auto page used with StareTest
function EventPage4()
    local lines = { }
    local faceSprites = { }
    while GetGlobal("CYFOWStareText" .. (#lines + 1)) do
        local lineID = #lines + 1
        lines[lineID] =       GetGlobal("CYFOWStareText" .. lineID)
        faceSprites[lineID] = GetGlobal("CYFOWStareFace" .. lineID)
        SetGlobal("CYFOWStareText" .. lineID, nil)
        SetGlobal("CYFOWStareFace" .. lineID, nil)
    end
    General.SetDialog(lines, true, faceSprites)
    Event.SetPage(Event.GetName(), 1)
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