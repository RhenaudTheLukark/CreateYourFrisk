function EventPage1()
	Inventory.AddItem("Butterscotch Pie")
	General.SetDialog({"You pickup the Butterscotch Pie."}, true)
	Event.SetPage("Bpie", -1)
    --Event.SetPage("Bpie", 2) --You shouldn't make "removing" pages directly. Instead, set -1 as the page of the event!
end

--You shouldn't.
function EventPage2()
	Event.Remove("Bpie")
end