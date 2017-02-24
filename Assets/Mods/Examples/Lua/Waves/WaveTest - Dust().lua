bullets = {} -- center is 0, 0
counter = 0 -- everyone is triggered~
Player.MoveTo(0, -100, false) -- get out of the way of the spiral
spiralarms = 1 * 2
spd = 5
startTime = Time.time
function Update()
	counter = counter + 1
	if counter % 5 == 0 and counter > 5 then
		for i = 0, spiralarms - 1 do
		local angle = (counter - (((360 / spiralarms) * i))%360 * 2)
		local b = CreateProjectile("wpbullet", math.cos(math.rad(angle)) * 400, math.sin(math.rad(angle)) * 400)
		b.SetVar("curAngle", angle)
		table.insert(bullets, b)
		end
	end
	for i = 1, #bullets do
		local b = bullets[i]
		if b.isactive then
			local angle = b.GetVar("curAngle")
			b.Move(-math.cos(math.rad(angle)) * spd, -math.sin(math.rad(angle)) * spd)
			if ((b.x < 20) and not (b.x < -20)) and ((b.y < 20) and not (b.y < -20)) then
				b.sprite.Dust()
				b.Remove()
			end
		else
		
		end
	end
	Player.sprite.Dust()
	if Time.time - startTime > 5 then
		EndWave()
	end
end

--math.cos(angle) math.sin(angle)
--angle in radians