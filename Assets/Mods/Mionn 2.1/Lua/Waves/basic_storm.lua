timer = 0
timer2 = 0
Encounter.SetVar("wavetimer",math.huge)
Arena.Resize(400, 130)
Encounter.Call("Animate",Encounter['Anim_To_Side'])
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]

l_hand = CreateProjectile("lhand",-(Arena.width/2 + 25),-20)
r_hand = CreateProjectile("rhand",(Arena.width/2 + 25),-20)
l_hand.sprite.alpha = 0
r_hand.sprite.alpha = 0

spawntimer = 90 - difficulty * 10

fadintime = 60
fadinstate = 0
willshoot = false
source_x = Arena.width/2 + 10
source_y = 0
speed = 2
N = 4 + difficulty
state = 0
dir = 1

bullets = {}

spinner_on = false

function Update()
	timer = timer + Time.dt
	while timer >= 1/60 do
		timer = timer -1/60
		timer2 = timer2 + 1
		FadeHands()
		--CustomResize()
		MoveHands(0)
		ShootBullets()
		MoveBullets()
		Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
		if timer2 >= 60 * 12 then
			Encounter.Call("Animate",Encounter['Anim_To_Side_Inv'])
			EndWave()
		end
	end
end

function FadeHands()
	if fadinstate < fadintime then
		fadinstate = fadinstate + 1
		l_hand.sprite.alpha = fadinstate/fadintime
		r_hand.sprite.alpha = fadinstate/fadintime
	elseif willshoot ~= true then
		willshoot = true
		--ResizeCustomSpeed(0.5,{300, 130})
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
	--DEBUG(resize_state[1])
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
			for i = 1,N+1 do
				local bullet = CreateProjectile("fball",side * source_x,source_y)
				local angle = 60 + (i - 1) * 60/N + state * 5
				local rangle = math.rad(angle)
				local x = math.sin(rangle) * -side
				local y = math.cos(rangle)
				bullet.SetVar('velx',speed * x)
				bullet.SetVar('vely',speed * y)
				bullet.SetVar('posx',bullet.x)
				bullet.SetVar('posy',bullet.y)
				table.insert(bullets,bullet)
			end
		end
		if state ~= 0 then
			dir = - dir
		end
		state = state + dir
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
	Player.Hurt(5)
end