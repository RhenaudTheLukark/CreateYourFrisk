function EventPage1()
	local isTheItemAdded = AddItem("Butterscotch Pie")
	SetDialog({"You pickup the Butterscotch Pie."}, true)
	SetEventPage("Bpie", -1)
    --SetEventPage("Bpie", 2) --You shouldn't make "removing" pages directly. Instead, set -1 as the page of the event!
end

--You shouldn't.
function EventPage2()
	RemoveEvent("Bpie")
end