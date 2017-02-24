timer = 0
timer2 = 0
timer3 = 0
Encounter.SetVar("wavetimer",math.huge)
Encounter.SetVar("DET",300)
Encounter.Call("Refresh_DET_Base")
Arena.Resize(200,200)
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]

fadintime = 60
fadinstate = 0
willshoot = false
source_x = Arena.width/2 + 10
source_y = 0

side = 1

spawntimer = 26 - difficulty * 4
spawntimer2 = 60 - difficulty * 6
speed = 1.5 + difficulty * 0.2
spread = 1

speed2 = 2 + difficulty * 0.15
speed3 = 2
N = 5 + difficulty
N2 = math.floor(2 + 0.7 * difficulty)
L = 3
R = 0
R_max = 200
R_speed = 0.8
R_phase = 0
R_a = 50
state = 0
dir = 1

bullets = {}
bullets2 = {}
bullets3 = {}

phase = 1
sub_phase = 0

spawnx = 0
spawny = 0
spawnx_target = 0
spawny_target = 0
spawn_speed = 0

function Update()
	timer = timer + Time.mult
	while timer >= 1 do
		timer = timer - 1
		timer2 = timer2 + 1
		Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
		if phase == 1 then
			FadeHands()
			if willshoot == true then
				ShootBullets()
			end
			if timer2 >= 6*60 and sub_phase < 1 then
				pattern = 2
				pattern_state = 1
				sub_phase = 1
			elseif timer2 >= 12*60 and sub_phase < 2 then
				pattern = 3
				pattern_state = 1
				sub_phase = 2
			elseif timer2 >= 18*60 and sub_phase < 3 then
				spread = 2
				sub_phase = 3
				spawntimer = 40 - difficulty * 4
			elseif timer2 >= 24*60 and phase < 2 then
				sub_phase = 0
				phase = 2
				pattern = 4
				pattern_state = 1
				ResizeCustomSpeed(8,{500,130})
				SpawnBullets()
			end
		elseif phase == 2 then
			ShootBullets2()
			MoveSpirals()
			ChangeR()
			if timer2 >= 42 * 60 and done ~= true then -- 21
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
		end
		MoveHands()
		MoveBullets()
		CustomResize()
		Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
	end
end

l_hand = CreateProjectile("lhand",-(Arena.width/2 + 25),-20)
r_hand = CreateProjectile("rhand",(Arena.width/2 + 25),-20)
l_hand.sprite.alpha = 0
r_hand.sprite.alpha = 0

function FadeHands()
	if fadinstate < fadintime then
		fadinstate = fadinstate + 1
		l_hand.sprite.alpha = fadinstate/fadintime
		r_hand.sprite.alpha = fadinstate/fadintime
	elseif willshoot ~= true then
		willshoot = true
	end
end

function MoveBullets()
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local velx = bullet.GetVar('velx')
			local vely = bullet.GetVar('vely')
			local posx = bullet.GetVar('posx')
			local posy = bullet.GetVar('posy')
			if (math.abs(posx + velx) > Arena.width/2 or
			math.abs(posy + vely) > Arena.height/2) and
			math.abs(bullet.x) < Arena.width/2 and
			math.abs(bullet.y) < Arena.height/2 then
				bullet.Remove()
			else
				local posx = posx + velx
				local posy = posy + vely
				bullet.MoveTo(posx,posy)
				bullet.SetVar('posx',posx)
				bullet.SetVar('posy',posy)
			end
		end
	end
	
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

function MoveSpirals()
	for i = 1,#bullets3 do
		local bullet = bullets3[i]
		local angle = bullet.GetVar('angle')
		local R_mod = bullet.GetVar('R_mod')
		local side = bullet.GetVar('side')
		angle = angle + speed3
		local rangle = math.rad(angle)
		local x = math.cos(rangle)
		local y = math.sin(rangle)
		local posx = (source_x  + spawnx) * side + x * R * R_mod 
		local posy = source_y + spawny + y * R * R_mod
		bullet.SetVar('angle',angle)
		bullet.MoveTo(posx,posy)
	end
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
		R = R_max + math.sin(timer3/50) * R_a
	end
end

function SpawnBullets()
	Audio.PlaySound("bfball")
	for k = 1,2 do
		local side = (k - 1.5) * 2
		for i = 1,L do
			local start = i * 180/N
			local R_mod = i / 3
			--local N = N + i
			for j = 1,N do
				local angle = j * 360/N + start
				local bullet = CreateProjectile("fball",side * (source_x + spawnx),source_y + spawny)
				bullet.SetVar('side',side)
				bullet.SetVar('R_mod',R_mod)
				bullet.SetVar('angle',angle)
				table.insert(bullets3,bullet)
			end
		end
	end
end

function ShootBullets()
	if timer2%spawntimer == 0 then
		Audio.PlaySound("fball")
		side = -side
		for i = -spread,spread do
			local bullet = CreateProjectile("fball",side * (source_x + spawnx),source_y + spawny)
			local dir = Direction({Player.x,Player.y + i*20},{bullet.x,bullet.y})
			bullet.SetVar('velx',speed * dir[1])
			bullet.SetVar('vely',speed * dir[2])
			bullet.SetVar('posx',bullet.x)
			bullet.SetVar('posy',bullet.y)
			table.insert(bullets,bullet)
		end
	end
end

function ShootBullets2()
	if timer2%spawntimer2 == 0 then
		Audio.PlaySound("fball")
		for j = 1,2 do
			local side = (j - 1.5) * 2
			for i = 1,N2+1 do
				local bullet = CreateProjectile("fball",side * (source_x + spawnx),source_y + spawny)
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
		source_x = Arena.width/2 + 10
		source_y = 0
		if neow == resize_target[1] and neoh == resize_target[2] then
			resize = false
			if phase == 2 and shrink ~= true then
				shrink = true
				ResizeCustomSpeed(0.3,{300, 130})
			end
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

function MoveHands()
	local dx = spawnx_target - spawnx
	local dy = spawny_target - spawny
	local d = (dx^2 + dy^2) ^ 0.5
	if d > spawn_speed then
		spawnx = spawnx + spawn_speed * dx / d
		spawny = spawny + spawn_speed * dy / d
	else
		spawnx = spawnx_target
		spawny = spawny_target	
		
		pattern_state = pattern_state + 1
		if pattern_state > #patterns[pattern] then
			pattern_state = 1
		end
		TargetHands(
		patterns[pattern][pattern_state][1],
		patterns[pattern][pattern_state][2],
		patterns[pattern][pattern_state][3]
		)
	end
	l_hand.MoveTo(-(Arena.currentwidth/2 + 25 + spawnx),-20 + spawny)
	r_hand.MoveTo((Arena.currentwidth/2 + 25 + spawnx),-20 + spawny)
end

function TargetHands(targetx,targety,speed)
	spawnx_target = targetx
	spawny_target = targety
	spawn_speed = speed * (1 + difficulty/8)
end

pattern = 1
pattern_state = 1

patterns = {
{ {0,60,2} , {50,0,2} , {0,-60,2} },
{ {60,60,1.5} , {0,0,1.5} , {60,-60,1.5} , {120,0,1.5} },
{ {30,60,1.8} , {90,30,1.8} , {30,0,1.8} , {90,-30,1.8} , {30,-60,1.8} },
{ {0,60,1} , {50,0,1} , {0,-60,1} }
}

function OnHit(bullet)
	Player.Hurt(5)
end