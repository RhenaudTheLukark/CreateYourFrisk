--For phase 2 of Dog
spawntimer = -1
bullets = {}
yOffset = 100
xOffset = 0
gMult = 1
xOffsetTypes = {20, 0, -20}
bulletfrick = 0.96
mult = 5
function Update()
    spawntimer = spawntimer + 1
    if(spawntimer % 60 == 0) then
        local numbullets = 5
        for i=1,numbullets+1 do
			
            local bullet = CreateProjectile('money2', 0, yOffset)
			bullet.SetVar('speed', 4)
			bullet.SetVar('timer', 0)
			bullet.SetVar('angle', (i*35)-220)
			bullet.SetVar('xspeed',0)
			bullet.SetVar('yspeed',0)
			if(i>3) then
			bullet.SetVar('anglefrick',1*mult)
			else
			bullet.SetVar('anglefrick',-1*mult)
			end
            table.insert(bullets, bullet)
        end
		mult = mult+2
    end

    for i=1,#bullets do
        -- set vurbles
		local bullet = bullets[i]
		local timer = bullet.GetVar('timer')
        local speed = bullet.GetVar('speed')
		local angle = bullet.GetVar('angle')
		local xspeed = bullet.GetVar('xspeed')
        local yspeed = bullet.GetVar('yspeed')
		local anglefrick = bullet.GetVar('anglefrick')
		local xdifference = Player.x - bullet.x
		local ydifference = Player.y - bullet.y
		local length = math.abs(math.sqrt(xdifference^2 + ydifference^2))
		-- change the trajectory for the next frame so the bullet curves
		if(timer<60)then
			xspeed = math.cos(angle*math.pi / 180)*speed
			yspeed = math.sin(angle*math.pi / 180)*speed
			bullet.SetVar('angle', angle+anglefrick)
			bullet.SetVar('speed', speed*bulletfrick)
		end
		if(timer==60) then
			
			bullet.SetVar('xspeed', (xdifference/length)*4)
			bullet.SetVar('yspeed', (ydifference/length)*4)
		end
		timer = timer+1
		bullet.SetVar('timer', timer)
		bullet.Move(xspeed, yspeed)
    end
	
end