animationFrames = {"ExRumia/1", "ExRumia/2", "ExRumia/3", "ExRumia/4", "ExRumia/5", "ExRumia/6", "ExRumia/7", "ExRumia/8", "ExRumia/9"}
currentFrame = 1
animationTimer = 0

function AnimateExRumia()
	animationTimer = animationTimer + 1
	if(animationTimer%8 == 0) then
		currentFrame = (currentFrame % (#animationFrames)) + 1
		animationTimer = 0
		enemies[1].Call("SetSprite", animationFrames[currentFrame])
	end
end