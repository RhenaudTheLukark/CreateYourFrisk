timer = 0
timer2 = 0
Encounter.SetVar("wavetimer",math.huge)
Arena.Resize(200,200)
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]
Encounter.Call("Animate",Encounter['Anim_To_Down'])

waitfor = 60
waiting = false
waited = 0

function Reset()
	side = (math.random(1,2)-1.5)*2
	wait = waitfor
	shot = false
	vspeed = 2 + waited/1.5
	acc = 0.05 + waited * 0.01 + difficulty * 0.01
	speed = 0
	maxspeed = 1.5 + difficulty * 0.2 + waited * 0.2
	height = 40 - difficulty - waited
	N = math.floor(Arena.height/height + 2)
	spawntimer = math.floor(height/vspeed)
end

bullets = {}

Reset()

function Update()
	timer = timer + Time.mult
	while timer >= 1 do
		timer = timer - 1
		timer2 = timer2 + 1
		if N > 0 then
			ShootBullets()
		elseif waiting == false and shot == false then
			StopVert()
			waiting = true
		end
		WaitToShoot()
		MoveBullets()
	end
end

function MoveBullets()
	if shot == true then
		if speed + acc > maxspeed then
			speed = maxspeed
		else
			speed = speed + acc
		end
	end
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local velx = bullet.GetVar('velx')
			local vely = bullet.GetVar('vely')
			local posx = bullet.GetVar('posx')
			local posy = bullet.GetVar('posy')
			local dir = bullet.GetVar('dir')
			if math.abs(posx + velx) > Arena.width/2 then
				bullet.Remove()
				Reset()
				if waited > 0 then
					Encounter.Call("Animate",Encounter['Anim_To_Down_Inv'])
					EndWave()
				end
			else
				local posx = posx + speed * dir
				local posy = posy + vely
				bullet.MoveTo(posx,posy)
				bullet.SetVar('posx',posx)
				bullet.SetVar('posy',posy)
			end
		end
	end
end

function ChangeDir()
	Audio.PlaySound("bfball")
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local dir = bullet.GetVar('dir')
			local velx = dir * speed
			bullet.SetVar('velx',velx)
		end
	end
end

function StopVert()
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			bullet.SetVar('vely',0)
		end
	end
end

function WaitToShoot()
	if waiting == true then
		if wait > 0 then
			wait = wait - 1
		else
			ChangeDir()
			waiting = false
			shot = true
			wait = waitfor
			waited = waited + 1
		end
	end
end

function ShootBullets()
	if timer2%spawntimer == 0 and timer2 > 30 then
		side = -side
		Audio.PlaySound("fball")
		local bullet = CreateProjectile("fball",0,Arena.height/2 + height/2)
		bullet.SetVar('velx',0)
		bullet.SetVar('vely',-vspeed)
		bullet.SetVar('posx',bullet.x)
		bullet.SetVar('posy',bullet.y)
		bullet.SetVar('dir',side)
		table.insert(bullets,bullet)
		N = N-1
	end
end

-- function Direction(postarget,posshooter)
	-- local dx = postarget[1] - posshooter[1]
	-- local dy = postarget[2] - posshooter[2]
	-- local d = math.sqrt(dx^2+dy^2)
	-- local alpha = math.acos(dx/d)
	-- if dy > 0 then
		-- alpha = alpha
	-- else
		-- alpha = - alpha
	-- end
	-- local x = math.cos(alpha)
	-- local y = math.sin(alpha)
	-- local dir = {x,y}
	-- return dir
-- end
	

function OnHit(bullet)
	Player.Hurt(5)
end