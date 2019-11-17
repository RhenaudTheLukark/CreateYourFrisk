function EventPage1()
    local speed = Event.GetSpeed("Player")

    Event.MoveToPoint("Player", 350, 769, false)
    Event.SetSpeed("Player", 1.5)
	Event.MoveToPoint("Player", 350, 630, true, false)
    Event.SetDirection("Player", 8)

    Screen.SetTone(true, false, 0, 0, 0, 255)

    local pos = Event.GetPosition("Player")
    while pos[2] > 630 do
        Misc.cameraY = 537
        General.Wait(1)
        NewAudio.SetVolume("src", 0.75 * ((pos[2] - 630) / 139))
        pos = Event.GetPosition("Player")
    end

    NewAudio.SetVolume("src", 0.75)
    Event.SetSpeed("Player", speed)
    Screen.SetTone(false, false, 0, 0, 0, 0)
    General.EnterShop("Dummy", true)
end
