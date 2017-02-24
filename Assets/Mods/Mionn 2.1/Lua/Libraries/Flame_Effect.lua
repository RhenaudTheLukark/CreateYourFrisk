Flame_flames_front = {}
Flame_flames_back = {}
Flame_flames_gamma = {}

Flame_state = 1
Flame_framerate = 5 -- Will be 5 + 7 * (DET/100) ; frame_wait = 60/FPS
Flame_frame_wait = 60/Flame_framerate
Flame_frame = 0

Flame_flames_alpha = 0.0

Flame_R_base = 150
Flame_R = Flame_R_base
Flame_R_a = 20
Flame_alpha = 0
Flame_alpha_a = 10
Flame_beta = 0
Flame_beta_a = 20
Flame_R_speed = 0.5 -- Will be 0.5 + 0.5 * DET/100
Flame_time_mod_alpha = 1
Flame_time_mod_beta = 5
Flame_time_mod_R = 3

Flame_mid_x = 320
Flame_mid_y = 300

function Flame_Refresh()
	for i = 1,10 do
		local gamma = i * 36
		local flame_front = CreateProjectile("flame_effect_1",0,0)
		flame_front.SetVar('safe',true)
		local flame_back
		Flame_flames_gamma[i] = gamma
		local app_R = math.cos(math.rad(gamma)) * Flame_R
		local depth = math.sin(math.rad(gamma)) * Flame_R
		local h1 = math.sin(math.rad(Flame_beta)) * app_R --D_from_alpha
		local x1 = math.cos(math.rad(Flame_alpha)) * app_R
		local y1 = math.sin(math.rad(Flame_alpha)) * app_R
		--local h1 = math.sin(math.rad(Flame_beta)) * D_from_alpha
		local x2 = x1 + h1 * math.cos(math.rad(Flame_alpha+90))
		local y2 = y1 + h1 * math.sin(math.rad(Flame_alpha+90))
		flame_front.MoveToAbs(x2+Flame_mid_x,y2+Flame_mid_y)
		Flame_flames_front[i] = flame_front
		if Flame_first ~= true then
			flame_back = CreateSprite("flame_effect_1")
			flame_back.MoveToAbs(x2+Flame_mid_x,y2+Flame_mid_y)
			Flame_flames_back[i] = flame_back
		end
		if depth < 0 then 
		Flame_flames_front[i].sprite.alpha = 0
		Flame_flames_back[i].alpha = Flame_flames_alpha
		else
		Flame_flames_front[i].sprite.alpha = Flame_flames_alpha
		Flame_flames_back[i].alpha = 0
		end
	end
	Flame_first = true
end
Flame_Refresh()

function Flame_Rotate()
	Flame_alpha = math.sin(Time.time/Flame_time_mod_alpha) * Flame_alpha_a
	Flame_R = math.sin(Time.time/Flame_time_mod_R) * Flame_R_a + Flame_R_base
	Flame_beta = math.sin(Time.time/Flame_time_mod_beta) * Flame_beta_a
	for i = 1,10 do
		Flame_flames_gamma[i] = Flame_flames_gamma[i] + Flame_R_speed
		local gamma = Flame_flames_gamma[i]
		local app_R = math.cos(math.rad(gamma)) * Flame_R
		local depth = math.sin(math.rad(gamma)) * Flame_R
		local h1 = math.sin(math.rad(Flame_beta)) * depth --D_from_alpha
		local x1 = math.cos(math.rad(Flame_alpha)) * app_R
		local y1 = math.sin(math.rad(Flame_alpha)) * app_R
		--local h1 = math.sin(math.rad(Flame_beta)) * D_from_alpha
		local x2 = x1 + h1 * math.cos(math.rad(Flame_alpha+90))
		local y2 = y1 + h1 * math.sin(math.rad(Flame_alpha+90))
		Flame_flames_front[i].MoveToAbs(x2+Flame_mid_x,y2+Flame_mid_y)
		Flame_flames_back[i].MoveToAbs(x2+Flame_mid_x,y2+Flame_mid_y)
		if depth < 0 then 
		Flame_flames_front[i].sprite.alpha = 0
		Flame_flames_back[i].alpha = Flame_flames_alpha
		else
		Flame_flames_front[i].sprite.alpha = Flame_flames_alpha
		Flame_flames_back[i].alpha = 0
		end
	end
	Flame_Animate()
end

function Refresh_DET_Base()
	Flame_framerate = 5 + 7 * DET/100
	Flame_frame_wait = 60/Flame_framerate
	Flame_R_speed = 0.5 + 0.5 * DET/100
	Flame_flames_alpha = DET/100
end

function Flame_Animate()
	Flame_frame = Flame_frame + 1
	if Flame_frame >= Flame_frame_wait then
		Flame_frame = Flame_frame - Flame_frame_wait
		if Flame_state >= 7 then
			Flame_state = 1
		else
			Flame_state = Flame_state + 1
		end
		for i = 1, 10 do
			local Flame_state = (Flame_state + i)%7 + 1
			Flame_flames_front[i].sprite.Set(FUN_Flame_Fun_Factor[i]..""..Flame_state)
			Flame_flames_back[i].Set(FUN_Flame_Fun_Factor[i]..""..Flame_state)
		end
	end
end

function Flame_Remove()
	Flame_Active = false
	for i = 1,10 do
		if Flame_flames_front[i].isactive then
			Flame_flames_front[i].Remove()
		end
		Flame_flames_back[i].Remove()
	end
end