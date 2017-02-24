spawntimer = 1
bigshots = {}
yOffset = 100
xOffset = 0
gMult = 1
xOffsetTypes = {20, 0, -20}
bulletfrick = 0.4
mult = 2

function OnHit(money)
    Player.Hurt(1, 0.04)
end

function Update()
    spawntimer = spawntimer + 1
    if(spawntimer % 30 == 0 and spawntimer < 500) then
		x = math.random(4)
		if x == 1 then
			xOffset = 230
			yOffset = 64
		elseif x == 2 then
			xOffset = -210
			yOffset = 64
		elseif x == 3 then
			xOffset = 230
			yOffset = -44
		else
			xOffset = -210
			yOffset = -44
		end
        local numbullets = 5
        for i=1,numbullets+1 do
            local bigshot = CreateProjectile('money', xOffset + math.random(20), yOffset + math.random(20))
			bigshot.SetVar('speed', 4)
			bigshot.SetVar('timer', 0)
			bigshot.SetVar('angle', (i*35)-220)
			bigshot.SetVar('xspeed',0)
			bigshot.SetVar('yspeed',0)
			if(i>3) then
			bigshot.SetVar('anglefrick',1*mult)
			else
			bigshot.SetVar('anglefrick',-1*mult)
			end
            table.insert(bigshots, bigshot)
        end
    end

    for i=1,#bigshots do
        -- set vurbles
		local bullet = bigshots[i]
		if bullet.isactive == true then
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
		if(timer<30)then
			xspeed = math.cos(angle*math.pi / 180)*speed
			yspeed = math.sin(angle*math.pi / 180)*speed
			bullet.SetVar('angle', angle+anglefrick)
			bullet.SetVar('speed', speed*bulletfrick)
		elseif(timer%30 == 0 and timer < 60) then
			bullet.SetVar('xspeed', (xdifference/length)*4)
			bullet.SetVar('yspeed', (ydifference/length)*4)
		end
		timer = timer+1
		bullet.SetVar('timer', timer)
		bullet.Move(xspeed*1.5, yspeed*1.5)
		if(bullet.x > 500)then
			bullet.Remove()
		elseif(bullet.x < -500) then
			bullet.Remove()
		elseif(bullet.y > 500)then
			bullet.Remove()
		elseif(bullet.y < -500)then
			bullet.Remove()
		end
		end
    end
	
end