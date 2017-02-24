selector = CreateSprite("ut-heart", "Top")
selector.color = {1, 0, 0}
selector.alpha = 0
readDisclaimer = false
highlightedDifficulty = 1
Audio.Stop()
DEBUG("selector.alpha == " .. selector.alpha)
function Update()
	if Input.Confirm == 1 then
		DEBUG("asfjkagjdkasj")
		readDisclaimer = true
		Audio.LoadFile("menu")
		selector.alpha = 1
	end
end