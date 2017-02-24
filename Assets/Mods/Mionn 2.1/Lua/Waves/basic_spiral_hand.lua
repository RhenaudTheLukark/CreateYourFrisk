timer = 0
timer2 = 0
Encounter.SetVar("wavetimer",math.huge)
Arena.Resize(150, 150)
Encounter.Call("Animate",Encounter['Anim_To_Down'])
Player.MoveTo(0,0,false)
difficulty = Encounter["difficulty"]

spawntimer = 80 - difficulty * 5

fadintime = 60
fadinstate = 0
willshoot = false
source_x = 0
source_y = Arena.height/2 + 30
speed = 0.8 + 0.2 * difficulty
N = 8 + difficulty
R_speed = 0.6 + 0.1 * difficulty
state = 0
dir = 1
a = 0

bullets = {}

function Update()
	timer = timer + Time.dt
	while timer >= 1/60 do
		timer = timer -1/60
		timer2 = timer2 + 1
		SpawnBullets()
		MoveSpirals()
		Encounter['Flame_mid_y'] = 300 + Arena.currentheight - 130
		if timer2 >= 60 * 12 then
			Encounter.Call("Animate",Encounter['Anim_To_Down_Inv'])
			EndWave()
		end
	end
end

function SpawnBullets()
	if timer2%spawntimer == 0 then
		Audio.PlaySound("bfball")
		for i = 1,N do
			local angle = i * 360/N
			local bullet = CreateProjectile("fball",source_x,source_y)
			bullet.SetVar('R',0)
			bullet.SetVar('angle',angle)
			table.insert(bullets,bullet)
		end
	end
end

function MoveSpirals()
	for i = 1,#bullets do
		local bullet = bullets[i]
		if bullet.isactive then
			local angle = bullet.GetVar('angle')
			local R = bullet.GetVar('R')
			angle = angle + speed
			R = R_speed + R
			if R < 300 then
				local rangle = math.rad(angle)
				local x = math.cos(rangle)
				local y = math.sin(rangle)
				local posx = source_x + x * (R + a * math.sin(timer2/30))
				local posy = source_y + y * (R + a * math.sin(timer2/30))
				bullet.SetVar('angle',angle)
				bullet.SetVar('R',R)
				bullet.MoveTo(posx,posy)
				if R > 200 then
					bullet.sprite.alpha = (300-R)/100
				end
			else
				bullet.Remove()
			end
		end
	end
end

function OnHit(bullet)
	Player.Hurt(5)
end