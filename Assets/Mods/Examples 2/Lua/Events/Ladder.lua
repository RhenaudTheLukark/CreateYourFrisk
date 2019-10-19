function EventPage0() end

function EventPage1()
    local speed = Event.GetSpeed("Player")

    Event.MoveToPoint("Player", 347, 769, false)
    Event.SetSpeed("Player", 1.5)
	Event.MoveToPoint("Player", 347, 630, true, false)
    Event.SetDirection("Player", 8)

    Screen.SetTone(true, false, 0, 0, 0, 255)

    while Event.GetPosition("Player")[2] > 630 do
        Misc.cameraY = 537
        General.Wait(1)
    end

    Event.SetSpeed("Player", speed)
    Screen.SetTone(false, false, 0, 0, 0, 0)
    General.EnterShop("Dummy", true)
end
