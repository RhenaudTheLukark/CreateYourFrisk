Arena.resize(100, 145)
spawntimer = 0
heightMultMain = -1
bullets = {}
function OnHit(bullet)
    Player.Hurt(1,0.01)
end

function Update()
    spawntimer = spawntimer + 1
    if spawntimer%5 == 0 then
		heightMultMain = heightMultMain * -1
        local posx = -(Arena.width/2) + 8
        local posy = 0
        local bullet = CreateProjectile('money',posx,posy)
		bullet.SetVar('speedyInRa',0.001*math.random(299,199))
		bullet.SetVar('speedy',0)
		bullet.SetVar('heightMult',heightMultMain)
        table.insert(bullets,bullet)
		Audio.PlaySound("tornadosound")
    end

if spawntimer >= 200 then
	EndWave()
end

	for i=1,#bullets do
		local bullet = bullets[i]
		local speedy = bullet.GetVar('speedy')
		local heightMult = bullet.GetVar('heightMult')
		local newposx = bullet.x + 1
		local newposy = math.sin(speedy) * (48 * heightMult)

		if (bullet.x < -(Arena.width/2) + 8) then
			newposx = -(Arena.width/2) + 8 -- Lets it catch up when the arena shrinks down (when using the Check command)
		end

		if (bullet.x > (Arena.width/2) - 8) then
			newposx = 650 -- Basically deletes the bullet!
		end

		bullet.MoveTo(newposx, newposy)
		bullet.SetVar('speedy', speedy+bullet.GetVar('speedyInRa'))
	end
end