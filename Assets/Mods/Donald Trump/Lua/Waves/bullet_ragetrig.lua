--For phase 2 of Dog
spawntimer = 0
bullets = {}
yOffset = 100
xOffset = 0
mult = 0.5
speed = 2
xOffsetTypes = {20, 0, -20}

function OnHit(money2)
    Player.Hurt(1, 0.03)
end

function Update()
    spawntimer = spawntimer + 1
	if(spawntimer > 60) then
	speed = math.random(9)
	spawntimer = 0
	xOffset = math.random(-77,77)
	end
    if(spawntimer % 4 == 0 and spawntimer <30) then
        local bullet = CreateProjectile('money2', xOffset, yOffset)
        local xdifference = Player.x - bullet.x
		local ydifference = Player.y - bullet.y
		local length = math.abs(math.sqrt(xdifference^2 + ydifference^2))
		
		bullet.SetVar('xspeed', (xdifference/length)*speed)
        bullet.SetVar('yspeed', (ydifference/length)*speed)   
        table.insert(bullets, bullet)
    end

    for i=1,#bullets do
        local bullet = bullets[i]
        local xspeed = bullet.GetVar('xspeed')
        local yspeed = bullet.GetVar('yspeed')
        bullet.Move(xspeed, yspeed)
    end
end