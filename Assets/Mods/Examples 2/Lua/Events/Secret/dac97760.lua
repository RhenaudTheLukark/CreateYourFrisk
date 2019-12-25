function EventPage0()
    if Event.GetAnimHeader("Player") ~= "" then
        Event.Remove(Event.GetName() .. " (1)")
        Event.Remove(Event.GetName())
    end
end

function EventPage1()
    local playerSprite = Event.GetSprite("Player")
    local maskSprite = Event.GetSprite("dac97760 (1)")
    local coeff = Event.GetDirection("Player") == 4 and 1 or -1
    local playerPos = Event.GetPosition("Player")
    Screen.DispImg("empty", 1, 320, 240)
    local imgSprite = Event.GetSprite("Image1")
    maskSprite.loopmode = "ONESHOT"
    maskSprite.xscale = coeff
    NewAudio.CreateChannel("temp")
    General.Wait(60)

    Event.SetAnimHeader("dac97760 (1)", "Fall")
    Event.Teleport("dac97760 (1)", playerPos[1] - 29 * coeff, playerPos[2])

    General.Wait(1)
    playerSprite.alpha = 0
    while not maskSprite.animcomplete do
        General.Wait(1)
    end
    for i = 1, 30 do
        Audio.Volume((30 - i) / 30)
        General.Wait(1)
    end
    Audio.Stop()
    Audio.Volume(1)
    maskSprite.Set("FriskUT/Fall/f2")
    local maskPos = Event.GetPosition("dac97760 (1)")
    for i = 1, 12 do
        Event.Teleport("dac97760 (1)", maskPos[1] + coeff, maskPos[2])
        coeff = -coeff
        General.Wait(10)
    end
    Event.Teleport("dac97760 (1)", maskPos[1], maskPos[2])
    General.Wait(120)

    Event.SetAnimHeader("dac97760 (1)", "Reveal")
    Event.Teleport("Image1", maskPos[1] - 17 * coeff, maskPos[2] + 11)
    General.Wait(10)

    imgSprite.loopmode = "ONESHOTEMPTY"
    imgSprite.SetAnimation({"FriskUT/Fall/ef0", "FriskUT/Fall/ef1", "FriskUT/Fall/ef2", "FriskUT/Fall/ef3", "FriskUT/Fall/ef4"}, 1/5)
    NewAudio.PlaySound("temp", "Secret/boing")
    General.Wait(1)
    while not NewAudio.isStopped("temp") do
        General.Wait(1)
    end

    General.Wait(60)
    Screen.SetTone(false, true, 0, 0, 0, 255)
    NewAudio.PlaySound("temp", "Secret/laugh")
    while not NewAudio.isStopped("temp") do
        General.Wait(1)
    end
    Event.Remove("Image1")
    General.Wait(30)
    SetRealGlobal("CYFInternalCross1", true)
    SetRealGlobal("CYFInternalCharacterSelected", false)
    Event.SetAnimHeader("dac97760 (1)", "StopDown")
    NewAudio.DestroyChannel("temp")
    Player.Teleport("test2", 320, 200, 2, false)
end