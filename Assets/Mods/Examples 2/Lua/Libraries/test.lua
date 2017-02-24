SK_WPN = CreateSprite("posette")
SK_WPN.x = 320
SK_WPN.y = 240

Animation = {}

function Animation.Update()
	SK_WPN.x = SK_WPN.x + 1
end

return Animation