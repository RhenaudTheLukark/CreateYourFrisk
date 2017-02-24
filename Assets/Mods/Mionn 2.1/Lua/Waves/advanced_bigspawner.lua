timer = 0
timer2 = 0
Encounter.SetVar("wavetimer",math.huge)
Arena.Resize(200,200)
Encounter.Call("Animate",Encounter['Anim_To_Down'])
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]

velbig = 2
vel = 1.4
rot = 0
velrot = 20
N = 4
spawntimer = 40

big_balls = {}
bullets = {}
phase = 0

sourceball = CreateProjectile("bigfball",0,Arena.height/2 + 30)
sourceball.sprite.alpha = 0

function FadeBall()
	if sourceball.sprite.alpha < 1 then
		sourceball.sprite.alpha = sourceball.sprite.alpha + 0.05
		sourceball.MoveTo(0,Arena.height/2 + 30)
	end
end

function FireBigBall(targetx,targety)
	Audio.PlaySound("bfball")
	local bullet = CreateProjectile("bigfball",0,Arena.height/2 + 30)
	bullet.SetVar('targetx',targetx)
	bullet.SetVar('targety',targety)
	local dir = Direction({targetx,targety},{bullet.x,bullet.y})
	bullet.SetVar('velx',dir[1]*velbig)
	bullet.SetVar('vely',dir[2]*velbig)
	bullet.SetVar('ready',false)
	table.insert(big_balls,bullet)
end

function MoveBullets()
	for i = 1,#big_balls do
		local bullet = big_balls[i]
		if bullet.GetVar('ready') ~= true then
			local velx = bullet.GetVar('velx')
			local vely = bullet.GetVar('vely')
			local targetx = bullet.GetVar('targetx')
			local targety = bullet.GetVar('targety')
			local dx = targetx - bullet.x
			local dy = targety - bullet.y
			local dist = math.sqrt(dx^2+dy^2)
			if dist > velbig then
				bullet.Move(velx,vely)
			else
				bullet.MoveTo(targetx,targety)
				bullet.SetVar('ready',true)
			end
		end
	end
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local newx = bullet.x + bullet.GetVar('velx')
			local newy = bullet.y + bullet.GetVar('vely')
			if math.abs(newx) > Arena.width/2 or math.abs(newy) > Arena.height/2 then
				bullet.Remove()
			else
				bullet.MoveTo(newx,newy)
			end
		end
	end
end

function SpawnSmalls()
	if timer2%spawntimer == 0 then
		Audio.PlaySound("fball")
		for i = 1,#big_balls do
			local spawner = big_balls[i]
			if spawner.GetVar('ready') == true then
				for i = 1,N do
					local bullet = CreateProjectile("fball",spawner.x,spawner.y)
					local angle = i * 360/N + rot
					local rangle = math.rad(angle)
					bullet.SetVar('velx',vel * math.cos(rangle))
					bullet.SetVar('vely',vel * math.sin(rangle))
					table.insert(bullets,bullet)
				end
			end
		end
		rot = rot + velrot
	end
end

function Direction(postarget,posshooter)
	local dx = postarget[1] - posshooter[1]
	local dy = postarget[2] - posshooter[2]
	local d = math.sqrt(dx^2+dy^2)
	local alpha = math.acos(dx/d)
	if dy > 0 then
		alpha = alpha
	else
		alpha = - alpha
	end
	local x = math.cos(alpha)
	local y = math.sin(alpha)
	local dir = {x,y}
	return dir
end

function FadeBigBalls()
	for i = 1,#big_balls do
		local spawner = big_balls[i]
		if spawner.isactive then
			spawner.sprite.alpha = spawner.sprite.alpha - 0.1
			if spawner.sprite.alpha <= 0 then
				spawner.Remove()
			end
		end
	end
end

function MoveBulletsRot()
	if shot == true then
		if math.abs(acc + speed) > maxspeed then
			speed = maxspeed
		else
			speed = speed + acc
		end
	end
	for i = 1,#bullets2 do
		local bullet = bullets2[i]
		if bullet.isactive then
			local dir = bullet.GetVar('dir')
			local velv = bullet.GetVar('velv')
			local ang = bullet.GetVar('ang')
			local R = bullet.GetVar('R')
				if ang > (2 * difficulty + 4) * 360 then
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
	for i = 1,#bullets2 do
		local bullet = bullets2[i]
		if bullet.isactive then
			local dir = bullet.GetVar('dir')
			local acch = dir * acc
			bullet.SetVar('acch',acch)
		end
	end
end

function StopVert()
	for i = 1,#bullets2 do
		local bullet = bullets2[i]
		if bullet.isactive then
			bullet.SetVar('velv',0)
		end
	end
end

function ShootBullets()
	if timer2%spawntimer2 == 0 and timer2 > 30 then
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
		table.insert(bullets2,bullet)
		N2 = N2-1
	end
end

side = (math.random(1,2)-1.5)*2
shot = false
vspeed = 2
acc = 0.004
maxspeed = 4 + difficulty * 2
speed = 0
height = 22
N2 = math.floor(Arena.height/height + 6)
spawntimer2 = math.floor(height/vspeed)

bullets2 = {}

function Update()
	timer = timer + Time.mult
	while timer >= 1 do
		timer = timer - 1
		timer2 = timer2 + 1
		Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
		FadeBall()
		MoveBullets()
		AngSound()
		if phase == 0 then
			SpawnSmalls()
			if timer2 == 20 then
				FireBigBall(Arena.width/2 - 20,Arena.height/2 - 20)
			end
			if timer2 == 4 * 60 then
				FireBigBall(-Arena.width/2 + 20,Arena.height/2 - 20)
				-- Encounter.Call("Animate",Encounter['Anim_To_Down_Mid'])
			end
			if timer2 == 8 * 60 then
				if difficulty >= 1 then
					FireBigBall(-Arena.width/2 + 20,-Arena.height/2 + 20)
					-- Encounter.Call("Animate",Encounter['Anim_To_Down_Mid'])
					if difficulty >= 2 then
						FireBigBall(Arena.width/2 - 20,-Arena.height/2 + 20)
					end
				end
			end
			if timer2 == 18 * 60 then
				phase = 1
				timer2 = 0
			end
		end
		if phase == 1 then
			FadeBigBalls()
			MoveBulletsRot()
			if N2 > 0 then
				ShootBullets()
			elseif shot ~= true then
				StopVert()
				shot = true
			end
		end
	end
end

function OnHit(bullet)
	Player.Hurt(5)
end

ang_sound = 0

function AngSound()
	if bullets2[1] ~= nil then
		local ang = bullets2[1].GetVar('ang') - 90
		if math.abs(ang) > ang_sound * 90 then
			ang_sound = ang_sound + 1
			Audio.PlaySound("fball")
		end
	end
end