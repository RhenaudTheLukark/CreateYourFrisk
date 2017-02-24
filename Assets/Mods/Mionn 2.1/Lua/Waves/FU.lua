timer = 0
timer2 = 0
timer3 = 0
Encounter.SetVar("wavetimer",math.huge)
Encounter.SetVar("DET",600)
Encounter.Call("Refresh_DET_Base")
Arena.Resize(400, 130)
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]
Audio.Pitch(1.1)

l_hand = CreateProjectile("lhand",-(Arena.width/2 + 25),-20)
r_hand = CreateProjectile("rhand",(Arena.width/2 + 25),-20)
l_hand.sprite.alpha = 0
r_hand.sprite.alpha = 0

spawntimer = 50 - difficulty * 10

fadintime = 60
fadinstate = 0
willshoot = false
source_x = Arena.width/2 + 10
source_y = 0
speed = 2
speed2 = 5
N = 5 + difficulty
N2 = math.floor(2 + 0.7 * difficulty)
L = 3
R = 0
R_max = 150
R_speed = 0
R_phase = 0
R_a = 40
state = 0
dir = 1

bullets = {}
bullets2 = {}

function Update()
	--if Player.isMoving or Input.Menu > 0 then
		--Audio.Unpause()
		timer = timer + Time.dt
		while timer >= 1/60 do
			timer = timer -1/60
			timer2 = timer2 + 1
			Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
			FadeHands()
			CustomResize()
			MoveHands(0)
			MoveBullets()
			ShootBullets()
			MoveSpirals()
			ChangeR()
			if timer2 >= 60 * 21 and done ~= true then -- 21
				Audio.PlaySound("flash")
				Encounter.SetVar('flash',200)
				Encounter.SetVar('collapse',1)
				done = true
			end
			if Encounter['collapse'] == 2 then
				Encounter.Call("StopAnimate")
				EndWave()
				Encounter.Call("CreateFlash")
			end
			Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
		end
	--else --Audio.Pause()
	--end
end

function ChangeR()
	if R_phase == 0 then
		if R + R_speed < R_max then
			R = R + R_speed
		else
			R = R_max
			R_phase = 1
		end
	elseif R_phase == 1 then
		timer3 = timer3 + 1
		R = R_max + math.sin(timer3/30) * R_a
	end
end

function SpawnBullets()
	Audio.PlaySound("bfball")
	if done ~= true then
		for k = 1,2 do
			local side = (k - 1.5) * 2
			for i = 1,L do
				local start = i * 180/N
				local R_mod = i / 3
				--local N = N + i
				for j = 1,N do
					local angle = j * 360/N + start
					local bullet = CreateProjectile("fball",side * source_x,source_y)
					bullet.SetVar('side',side)
					bullet.SetVar('R_mod',R_mod)
					bullet.SetVar('angle',angle)
					table.insert(bullets,bullet)
				end
			end
		end
	end
end

function MoveSpirals()
	for i = 1,#bullets do
		local bullet = bullets[i]
		local angle = bullet.GetVar('angle')
		local R_mod = bullet.GetVar('R_mod')
		local side = bullet.GetVar('side')
		angle = angle + speed
		local rangle = math.rad(angle)
		local x = math.cos(rangle)
		local y = math.sin(rangle)
		local posx = source_x * side + x * R * R_mod
		local posy = source_y + y * R * R_mod
		bullet.SetVar('angle',angle)
		bullet.MoveTo(posx,posy)
	end
end

function FadeHands()
	if fadinstate < fadintime then
		fadinstate = fadinstate + 1
		l_hand.sprite.alpha = fadinstate/fadintime
		r_hand.sprite.alpha = fadinstate/fadintime
	elseif willshoot ~= true then
		willshoot = true
		R_speed = 1.2
		SpawnBullets()
		ResizeCustomSpeed(0.4,{200, 130})
	end
end

function MoveHands(y)
	l_hand.MoveTo(-(Arena.currentwidth/2 + 25),y - 20)
	r_hand.MoveTo((Arena.currentwidth/2 + 25),y - 20)
	source_x = Arena.width/2 + 10
	source_y = 0
end

function CustomResize()
	if resize == true then
		local dw = resize_target[1] - resize_state[1]
		local w = dw/math.abs(dw)
		local dh = resize_target[2] - resize_state[2]
		local h = dh/math.abs(dh)
		local neow = 0
		local neoh = 0
		if math.abs(dw) - resize_speed > 0 then
			neow = resize_state[1] + w * resize_speed
		else
			neow = resize_target[1]
		end
		if math.abs(dh) - resize_speed > 0 then
			neoh = resize_state[2] + h * resize_speed
		else
			neoh = resize_target[2]
		end
		Arena.ResizeImmediate(neow,neoh)
		resize_state = {neow,neoh}
		if neow == resize_target[1] and neoh == resize_target[2] then
			resize = false
		end
	end
end

resize_speed = 0
resize_target = {}
resize = false
resize_state = {0,0}

function ResizeCustomSpeed(speed,target)
	resize_speed = speed
	resize_target = target
	resize = true
	resize_state = {Arena.width,Arena.height}
end

function ShootBullets()
	if timer2%spawntimer == 0 then
		Audio.PlaySound("fball")
		for j = 1,2 do
			local side = (j - 1.5) * 2
			for i = 1,N2+1 do
				local bullet = CreateProjectile("fball",side * source_x,source_y)
				local angle = 60 + (i - 1) * 60/N2 + state * 5
				local rangle = math.rad(angle)
				local x = math.sin(rangle) * -side
				local y = math.cos(rangle)
				bullet.SetVar('velx',speed2 * x)
				bullet.SetVar('vely',speed2 * y)
				bullet.SetVar('posx',bullet.x)
				bullet.SetVar('posy',bullet.y)
				table.insert(bullets2,bullet)
			end
		end
		if state ~= 0 then
			dir = - dir
		end
		state = state + dir
	end
end

function MoveBullets()
	for i = 1,#bullets2 do
		local bullet = bullets2[i]
		if bullet.isactive then
			local velx = bullet.GetVar('velx')
			local vely = bullet.GetVar('vely')
			local posx = bullet.GetVar('posx')
			local posy = bullet.GetVar('posy')
			local posx = posx + velx
			local posy = posy + vely
			bullet.MoveTo(posx,posy)
			bullet.SetVar('posx',posx)
			bullet.SetVar('posy',posy)
			if bullet.absx > 640 or bullet.absx < 0 then
				bullet.Remove()
			end
		end
	end
end

function OnHit(bullet)
	--if Player.isMoving or Input.Menu > 0 then
		Player.Hurt(3,0.1) -- 3!!!
	--end
end