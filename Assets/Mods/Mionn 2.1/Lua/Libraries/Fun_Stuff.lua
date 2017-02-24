Fun = 0
FUN_Limit = 0

FUN_Flame_Fluctuations = {}
for i = 1,10 do
	FUN_Flame_Fluctuations[i] = 0
end
FUN_Flame_Fun_Factor = {}
for i = 1,10 do
	FUN_Flame_Fun_Factor[i] = "flame_effect_"
end

function Fluctuate_Flames()
	if Fun == 666 then
		for i = 1,10 do
			if FUN_Flame_Fluctuations[i] == 0 then
				local FUN = math.random(1000)
				if FUN == 666 and FUN_Limit < 2 then
					FUN_Flame_Fluctuations[i] = 1
					FUN_Flame_Fun_Factor[i] = "Final/Unused/Sign_o_Fun_"
					FUN_Limit = FUN_Limit + 1
				end
			elseif FUN_Flame_Fluctuations[i] == 1 then
				local FUN = math.random(100)
				if FUN == 66 then
					FUN_Flame_Fluctuations[i] = 0
					FUN_Flame_Fun_Factor[i] = "flame_effect_"
					FUN_Limit = FUN_Limit - 1
				end
			end
		end
	end
	Fun_Fafic()
	Fun_Enfic()
	Funny_noise()
end

FUN_Fafic_lasts = 20
FUN_Fafic_state = 0

function FUN_Fafic_Setup(alpha)
	if FUN_Fafic_body ~= nil then
		if FUN_Fafic_body.isactive then
			FUN_Fafic_body.Remove()
		end
	end
	FUN_Fafic_body = CreateProjectileAbs("Final/Unused/Fun1",320,240)
	FUN_Fafic_body.sprite.alpha = alpha
	FUN_Fafic_body.SetVar('safe',true)
end

function Fun_Fafic()
	local FUN
	if Fun == 666 and FUN_Enfic ~= true then
		FUN = math.random(66*6)
	end
	if FUN == 66 and FUN_Fafic ~= true then
		FUN_Fafic_Setup(0.05)
		FUN_Fafic = true
		FUN_Fafic_state = FUN_Fafic_lasts
		Audio.PlaySound("fafic")
	end
	if FUN_Fafic == true and FUN_Fafic_state > 0 then
		FUN_Fafic_state = FUN_Fafic_state - 1
		FUN_Fafic_body.sprite.Set("Final/Unused/Fun"..math.random(4))
	elseif FUN_Fafic == true then
		FUN_Fafic = false
		FUN_Fafic_body.Remove()
	end
end

function Fun_Spec_Fafic()
	FUN_Fafic_Setup(1)
	FUN_Fafic = true
	FUN_Fafic_state = FUN_Fafic_lasts
	Audio.PlaySound("fafic")
end

function Fun_Enfic_Start()
	FUN_Fafic_Setup(1)
	FUN_Enfic = true
	FUN_Fafic = false
	FUN_Fafic_state = FUN_Fafic_lasts * 2
	Audio.PlaySound("fafic")
end

function Fun_Enfic()
	if FUN_Enfic == true and FUN_Fafic_state > 0 then
		FUN_Fafic_state = FUN_Fafic_state - 1
		FUN_Fafic_body.sprite.Set("Final/Unused/Fun"..math.random(4))
	elseif FUN_Enfic == true then
		State("DONE")
	end
end

Funny_Pitch = 1

function Funny_noise()
	if Fun_Noises == true then
		Funny_Pitch = Funny_Pitch - 0.01
		Audio.Pitch(Funny_Pitch)
	end
end

function Initiate_Fun(bool)
	if enemies[3] ~= nil then
		enemies[3].Call("SetActive",bool)
	end
end