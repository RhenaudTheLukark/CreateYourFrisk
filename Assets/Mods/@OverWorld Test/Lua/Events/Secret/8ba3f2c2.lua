function EventPage0()
    if Event.GetAnimHeader("Player") ~= "MK" then
        Event.Remove(Event.GetName() .. " (1)")
        Event.Remove(Event.GetName())
    end
end

function EventPage1()
    local playerSprite = Event.GetSprite("Player")
    local charaSprite = Event.GetSprite("8ba3f2c2 (1)")
    local maskSprite = Event.GetSprite("8ba3f2c2 (2)")

    Event.MoveToPoint("Player", 380, 200, true)
    Event.SetDirection("Player", 2)
    General.SetDialog({"[noskip]Man, the Core is a nice place... [w:25][mugshot:MK/sad]but I'm very far away from home...",
                       "[noskip]I told my parents I went exploring the world, [w:25][mugshot:MK/sad2]but man, this place is scary...",
                       "[noskip]I better move on."}, true, {"MK/stars", "MK/normal", "MK/determined"})
    Event.MoveToPoint("Player", 400, 200, true)
	Event.Teleport("8ba3f2c2 (1)", 40, 200)
    local playerPos = Event.GetPosition("Player")
    Event.Teleport("8ba3f2c2 (2)", playerPos[1] + 14, playerPos[2])
    maskSprite.loopmode = "ONESHOT"
    maskSprite.Scale(-1, 1)
    Event.SetAnimHeader("8ba3f2c2 (2)", "Fall")
    playerSprite.alpha = 0
    General.Wait(1)
    while not maskSprite.animcomplete do
        General.Wait(1)
    end
	maskSprite.z = -1
    Event.Teleport("8ba3f2c2 (2)", playerPos[1], playerPos[2])
    maskSprite.loopmode = "LOOP"
    Event.SetAnimHeader("8ba3f2c2 (2)", "Fallen")
    Event.MoveToPoint("8ba3f2c2 (2)", 420, 146, true)
    General.Wait(30)
    General.SetDialog({"[noskip]W-W-What![w:25]\n[mugshot:MK/horrified]I tripped!"}, true, {"MK/surprised"})
    Event.SetAnimHeader("8ba3f2c2 (1)", "")
    Event.Teleport("8ba3f2c2 (1)", 0, 240)
    Event.MoveToPoint("8ba3f2c2 (1)", 400, 240, true)
    General.Wait(3)
    Event.SetDirection("8ba3f2c2 (1)", 2)
    General.Wait(30)
    Event.SetAnimHeader("8ba3f2c2 (2)", "Fallen2")
    General.SetDialog({"[noskip]Y-Yo![w:25]\nP-Please, [w:15]help m-me!", "[noskip]I...[w:25]\nI'm slipping!"}, true, {"MK/horrified", "MK/horrified2"})
    General.Wait(30)

    Event.MoveToPoint("8ba3f2c2 (2)", 420, 0, true, false)
    local maskPos
    repeat
        maskPos = Event.GetPosition("8ba3f2c2 (2)")
        local c = maskPos[2] / 150
        NewAudio.SetVolume("StaticKeptAudio", c)
        maskSprite.color = {c, c, c}
        General.Wait(1)
    until maskPos[2] == 0
    NewAudio.Stop("StaticKeptAudio")
    NewAudio.SetVolume("StaticKeptAudio", 1)

    General.Wait(60)
    NewAudio.CreateChannel("temp")
    NewAudio.PlaySound("temp", "Secret/noise")
    charaSprite.loopmode = "ONESHOT"
    Event.SetAnimHeader("8ba3f2c2 (1)", "Glitch")
    General.Wait(1)
    while not charaSprite.animcomplete do
        General.Wait(1)
    end
    Event.SetAnimHeader("8ba3f2c2 (1)", "Chara")
    General.Wait(60)
    Screen.SetTone(true, true, 0, 0, 0, 255)
    --General.Wait(30)
    NewAudio.DestroyChannel("temp")
    SetRealGlobal("CYFInternalCross3", true)
    SetRealGlobal("CYFInternalCharacterSelected", false)
    Event.SetAnimHeader("8ba3f2c2 (1)", "StopDown")
    Event.SetAnimHeader("8ba3f2c2 (2)", "StopDown")
    Player.Teleport("test2", 320, 200, 2, false)
end