function EventPage1()
    local spriteTest = GetSpriteOfEvent("Punder1")
	spriteTest.Set("Overworld/PunderLeft1")
	SetDialog({"[voice:punderbolt]Hey, [w:20]how's going?"}, true, {"pundermug"})
	spriteTest.Set("Overworld/PunderDown1")
end