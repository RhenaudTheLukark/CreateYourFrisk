function EventPage0()
    if Event.GetAnimHeader("Player") ~= "Booster" then
        Event.Remove(Event.GetName() .. " (2)")
        Event.Remove(Event.GetName() .. " (1)")
        Event.Remove(Event.GetName())
    end
end

function EventPage1()
    local playerSprite = Event.GetSprite("Player")
    local charaSprite = Event.GetSprite("caf144b2 (1)")
    local playerMaskSprite = Event.GetSprite("caf144b2 (2)")
    local playerPos = Event.GetPosition("Player")
    local charaPos = Event.GetPosition("caf144b2 (1)")
    Event.SetDirection("Player", 4)
    Event.SetAnimHeader("caf144b2 (1)", "")
    Event.SetDirection("caf144b2 (1)", 6)
    Event.Teleport("caf144b2 (1)", charaPos[1], playerPos[2])
    General.SetDialog({"[noskip]Heeeeeeey!"}, true)
    Event.MoveToPoint("caf144b2 (1)", 900, playerPos[2], true)

    General.SetDialog({"[noskip]I was running after you, [w:10]you know!",
                       "[noskip]ahem[next]",
                       "[noskip]You're a human, [w:10]right?"}, true, {"Chara/angry", "Chara/ahem", "Chara/badsmile"})
    General.SetChoice({"Yes", "No"})
    if lastChoice == 0 then
        NewAudio.CreateChannel("temp")
        General.SetDialog({"[noskip]Oh, [w:10]cool!",
                           "[noskip]You know,[mugshot:Chara/normal] [w:10]I came by not a long time ago too.",
                           "[noskip]I know the place a bit now, [w:10][mugshot:Chara/smile]would you like me to show you around?",
                           "[noskip]No? [w:20]Excellent, [w:10]follow me!"}, true, {"Chara/smile", "Chara/thinking", "Chara/normal", "Chara/smile"})
        Event.SetAnimHeader("caf144b2 (2)", "")
        Event.Teleport("caf144b2 (2)", playerPos[1], playerPos[2])
        Event.Teleport("caf144b2 (2)", playerPos[1], playerPos[2])
        playerSprite.alpha = 0
        Event.MoveToPoint("caf144b2 (2)", 600, playerPos[2], true, false)
    else
        General.SetDialog({"[noskip]Oh alright, [w:10]carry on then."}, true, {"Chara/sad"})
    end
    Event.MoveToPoint("caf144b2 (1)", 600, playerPos[2], true, lastChoice ~= 0)
    if lastChoice == 0 then
        General.Wait(80)
        Screen.SetTone(true, true, 0, 0, 0, 255)
        General.StopBGM(60)
        General.Wait(120)
        Screen.Flash(30, 255, 0, 0, 255)
        NewAudio.PlaySound("temp", "Secret/laugh")
        while not NewAudio.isStopped("temp") do
            General.Wait(1)
        end
        General.Wait(30)
        Event.Remove(Event.GetName())
        SetRealGlobal("CYFInternalCross4", true)
        SetRealGlobal("CYFInternalCharacterSelected", false)
        Event.SetAnimHeader("caf144b2 (1)", "StopDown")
        Event.SetAnimHeader("caf144b2 (2)", "StopDown")
        NewAudio.DestroyChannel("temp")
        Player.Teleport("test2", 320, 200, 2, false)
    end
    Event.SetAnimHeader("caf144b2 (1)", "StopDown")
    Event.SetAnimHeader("caf144b2 (2)", "StopDown")
    Event.Remove(Event.GetName())
    Event.Remove(Event.GetName() .. " (1)")
    Event.Remove(Event.GetName() .. " (2)")
end