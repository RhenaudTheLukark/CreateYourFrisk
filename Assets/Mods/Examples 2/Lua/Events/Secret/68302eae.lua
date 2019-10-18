function EventPage0()
    if Event.GetAnimHeader("Player") ~= "Asriel" then
        Event.Remove(Event.GetName() .. " (1)")
        Event.Remove(Event.GetName())
    end
end

function EventPage1()
    local playerSprite = Event.GetSprite("Player")
    local friskSprite = Event.GetSprite(Event.GetName())
    local blackSprite = Event.GetSprite(Event.GetName() .. " (1)")
    Event.MoveToPoint("Player", 440, 201, true)
    Event.SetDirection("Player", 4)
    Event.SetDirection(Event.GetName(), 6)
    General.SetDialog({"[voice:v_asriel](There's a human here, [w:15]all alone...)",
                       "[voice:v_asriel]Howdy! [w:25]Do you need any help?",
                       "..."}, true,
                      {"Asriel/normal",
                      {"Asriel/happyT", "Asriel/happy", 0.2},
                       "Frisk/sad"})
    General.SetChoice({"Help", "Don't help"})
    if lastChoice == 0 then
        NewAudio.CreateChannel("Appear")
        Event.IgnoreCollision(Event.GetName(), true)
        friskSprite.loopmode = "ONESHOT"
        Event.MoveToPoint("Player", 409, 200.4, true)
        General.Wait(30)
        playerSprite.alpha = 0
        Event.SetAnimHeader(Event.GetName(), "Huggu1")
        Event.Teleport(Event.GetName(), 403, 200)
        General.Wait(1)
        while not friskSprite.animcomplete do
            General.Wait(1)
        end
        General.Wait(30)
        friskSprite.loopmode = "LOOP"
        Event.SetAnimHeader(Event.GetName(), "Huggu2")
        General.Wait(30)
        General.SetDialog({"[voice:v_asriel]Don't worry, [w:15]everything is going to be okay..."}, true, {{"Asriel/sadT", "Asriel/sad", 0.2}})
        General.Wait(90)
        local playerPos = Event.GetPosition("Player")
        Event.Teleport(Event.GetName() .. " (1)", playerPos[1] < 320 and 320 or playerPos[1], playerPos[2] < 240 and 0 or playerPos[2] - 240)
        friskSprite.loopmode = "ONESHOT"
        Event.SetAnimHeader(Event.GetName(), "Huggu3")
        local appeared = false
        Audio.Stop()
		friskSprite.z = -1
        blackSprite.z = -0.5
        blackSprite.Set("black")
        for i = 1, 5 do
            if not appeared then
                blackSprite.alpha = 1
                NewAudio.PlaySound("Appear", "BeginBattle2")
            else
                blackSprite.alpha = 0
            end
            appeared = not appeared
            General.Wait(12)
        end
        General.Wait(18)
        Screen.SetTone(true, true, 0, 0, 0, 255)
        General.Wait(30)
        Event.IgnoreCollision(Event.GetName(), false)
        Event.SetAnimHeader(Event.GetName(), "")
        SetRealGlobal("CYFInternalCross5", true)
        SetRealGlobal("CYFInternalCharacterSelected", false)
        Player.Teleport("test2", 320, 200, 2, false)
    else
        General.SetDialog({"[noskip][voice:v_asriel](I hope he'll be fine...)"}, true, {"Asriel/verySad"})
        Event.SetDirection(Event.GetName(), 4)
    end
end