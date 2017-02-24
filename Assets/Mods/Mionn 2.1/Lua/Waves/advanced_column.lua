timer = 0
timer2 = 0
Encounter.SetVar("wavetimer",math.huge)
Arena.Resize(200,200)
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]
Encounter.Call("Animate",Encounter['Anim_To_Down'])

waitfor = 30
waiting = false
waited = 0


side = (math.random(1,2)-1.5)*2
wait = waitfor
shot = false
vspeed = 2 + waited/1.5
acc = 0.005
maxspeed = 2.5 + difficulty * 1.0 + waited * 0.2
speed = 0
height = 22
N = math.floor(Arena.height/height + 6)
spawntimer = math.floor(height/vspeed)

bullets = {}

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
		AngSound()
		Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
	end
end

function MoveBullets()
	if shot == true then
		if math.abs(acc + speed) > maxspeed then
			speed = maxspeed
		else
			speed = speed + acc
		end
	end
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local dir = bullet.GetVar('dir')
			local velv = bullet.GetVar('velv')
			local ang = bullet.GetVar('ang')
			local R = bullet.GetVar('R')
				if ang > (difficulty * 2 + 3) * 360 then
					Encounter.Call("Animate",Encounter['Anim_To_Down_Inv'])
					EndWave()
				end
			ang = ang + speed * dir
			local rang = math.rad(ang)
			R = R + velv
			posx = R * math.cos(rang)
			posy = R * math.sin(rang)
			bullet.MoveTo(posx,posy)
			bullet.SetVar('ang',ang)
			bullet.SetVar('R',R)
		end
	end
end

function ChangeDir()
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local dir = bullet.GetVar('dir')
			local acch = dir * acc
			bullet.SetVar('acch',acch)
		end
	end
end

function StopVert()
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			bullet.SetVar('velv',0)
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
		end
	end
end

function ShootBullets()
	if timer2%spawntimer == 0 and timer2 > 30 then
		side = -side
		if N%2 == 0 then
			Audio.PlaySound("fball")
		end
		local bullet = CreateProjectile("fball",0,Arena.height/2 + 2 * height)
		bullet.SetVar('velh',0)
		bullet.SetVar('velv',-vspeed)
		bullet.SetVar('acch',0)
		bullet.SetVar('ang',90)
		bullet.SetVar('R',bullet.y)
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

ang_sound = 0

function AngSound()
	if bullets[1] ~= nil then
		local ang = bullets[1].GetVar('ang') - 90
		if math.abs(ang) > ang_sound * 90 then
			ang_sound = ang_sound + 1
			Audio.PlaySound("fball")
		end
	end
end