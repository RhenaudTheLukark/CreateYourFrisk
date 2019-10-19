function EventPage0()
    Event.Remove(Event.GetName())
end

function EventPage1()
	Inventory.AddItem("Butterscotch Pie")
	General.SetDialog({"You pickup the Butterscotch Pie."}, true)
	Event.Remove(Event.GetName())
end

function EventPage2()
	Event.Remove(Event.GetName())
end
