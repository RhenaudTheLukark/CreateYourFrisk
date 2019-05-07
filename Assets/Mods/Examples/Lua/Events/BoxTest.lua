function EventPage1()
    General.SetChoice({"Yes", "No"}, "Use the box?")
    if lastChoice == 0 then
        Inventory.SpawnBoxMenu()
    end
end