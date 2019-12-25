function EventPage1()
	Inventory.AddItem("Butterscotch Pie")
	General.SetDialog({"You pickup the Butterscotch Pie."}, true)
	Event.SetPage(Event.GetName(), -1) -- Removes the event for this save file forever, even between maps
end
